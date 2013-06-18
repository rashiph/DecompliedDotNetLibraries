namespace Microsoft.Build.Shared.LanguageParser
{
    using Microsoft.Build.Shared;
    using System;
    using System.IO;
    using System.Text;

    internal sealed class StreamMappedString
    {
        private Stream binaryStream;
        private int charactersRead;
        private char[] currentPage;
        private int currentPageNumber;
        private int finalPageNumber;
        private bool forceANSI;
        private int pagesAllocated;
        private int pageSize;
        private char[] priorPage;
        private StreamReader reader;

        public StreamMappedString(Stream binaryStream, bool forceANSI) : this(binaryStream, forceANSI, DefaultPageSize)
        {
        }

        internal StreamMappedString(Stream binaryStream, bool forceANSI, int pageSize)
        {
            this.currentPageNumber = -1;
            this.finalPageNumber = 0x7fffffff;
            this.binaryStream = binaryStream;
            this.forceANSI = forceANSI;
            this.pageSize = pageSize;
            this.RestartReader();
        }

        private int AbsoluteOffsetToPageOffset(int offset)
        {
            return (offset - (this.PageFromAbsoluteOffset(offset) * this.pageSize));
        }

        private void AppendCharacterToStream(char c)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.charactersRead != this.pageSize, "Attempt to append to non-last page.");
            this.currentPage[this.charactersRead] = c;
            this.charactersRead++;
        }

        public char GetAt(int offset)
        {
            char[] page = this.GetPage(offset);
            if (page == null)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            int index = this.AbsoluteOffsetToPageOffset(offset);
            return page[index];
        }

        private int GetCharactersOnPage(int offset)
        {
            int num = this.PageFromAbsoluteOffset(offset);
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((num >= (this.currentPageNumber - 1)) && (num <= this.currentPageNumber), "Could not get character count for this page.");
            if (num == this.currentPageNumber)
            {
                return this.charactersRead;
            }
            return this.pageSize;
        }

        private char[] GetPage(int offset)
        {
            int num = this.PageFromAbsoluteOffset(offset);
            if (num < (this.currentPageNumber - 1))
            {
                this.RestartReader();
            }
            while (num > this.currentPageNumber)
            {
                int currentPageNumber = this.currentPageNumber;
                if (!this.ReadNextPage())
                {
                    break;
                }
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(currentPageNumber != this.currentPageNumber, "Expected a new page.");
            }
            if (num == this.currentPageNumber)
            {
                if (this.charactersRead > this.AbsoluteOffsetToPageOffset(offset))
                {
                    return this.currentPage;
                }
                return null;
            }
            if (num == (this.currentPageNumber - 1))
            {
                return this.priorPage;
            }
            return null;
        }

        public bool IsPastEnd(int offset)
        {
            return (this.GetPage(offset) == null);
        }

        private bool IsZeroLengthStream()
        {
            return ((this.charactersRead == 0) && (this.currentPageNumber == 0));
        }

        private char LastCharacterInStream()
        {
            if (this.charactersRead == 0)
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.priorPage != null, "There is no last character in the stream.");
                return this.priorPage[this.pageSize - 1];
            }
            return this.currentPage[this.charactersRead - 1];
        }

        private int PageFromAbsoluteOffset(int offset)
        {
            return (offset / this.pageSize);
        }

        private void ReadBlockStripEOF()
        {
            if (this.priorPage == null)
            {
                this.pagesAllocated++;
                this.priorPage = new char[this.pageSize];
            }
            this.charactersRead = this.reader.ReadBlock(this.priorPage, 0, this.pageSize);
            for (int i = 0; i < this.charactersRead; i++)
            {
                if (this.priorPage[i] == '\x001a')
                {
                    Array.Copy(this.priorPage, i + 1, this.priorPage, i, (this.charactersRead - i) - 1);
                    this.charactersRead += this.reader.ReadBlock(this.priorPage, this.charactersRead - 1, 1);
                    i--;
                    this.charactersRead--;
                }
            }
        }

        private bool ReadNextPage()
        {
            if (this.currentPageNumber == this.finalPageNumber)
            {
                return false;
            }
            this.ReadBlockStripEOF();
            this.SwapPages();
            this.currentPageNumber++;
            if (this.charactersRead < this.pageSize)
            {
                this.finalPageNumber = this.currentPageNumber;
                if (!this.IsZeroLengthStream() && !TokenChar.IsNewLine(this.LastCharacterInStream()))
                {
                    this.AppendCharacterToStream('\r');
                }
            }
            return (this.charactersRead > 0);
        }

        private void RestartReader()
        {
            this.currentPageNumber = -1;
            this.charactersRead = 0;
            this.priorPage = null;
            this.currentPage = null;
            if (this.binaryStream.Position != 0L)
            {
                this.binaryStream.Seek(0L, SeekOrigin.Begin);
            }
            if (this.forceANSI)
            {
                this.reader = new StreamReader(this.binaryStream, Encoding.Default, false);
            }
            else
            {
                Encoding encoding = new UTF8Encoding(false, true);
                this.reader = new StreamReader(this.binaryStream, encoding, true);
            }
        }

        public string Substring(int startPosition, int length)
        {
            StringBuilder builder = new StringBuilder(length);
            int charCount = 0;
            for (int i = 0; i < length; i += charCount)
            {
                char[] page = this.GetPage(startPosition + i);
                if (page == null)
                {
                    throw new ArgumentOutOfRangeException("length");
                }
                int startIndex = this.AbsoluteOffsetToPageOffset(startPosition + i);
                int charactersOnPage = this.GetCharactersOnPage(startPosition + i);
                charCount = Math.Min((int) (length - i), (int) (charactersOnPage - startIndex));
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(charCount > 0, "Expected non-zero extraction count.");
                builder.Append(page, startIndex, charCount);
            }
            return builder.ToString();
        }

        private void SwapPages()
        {
            char[] currentPage = this.currentPage;
            this.currentPage = this.priorPage;
            this.priorPage = currentPage;
        }

        public static int DefaultPageSize
        {
            get
            {
                return 0x100;
            }
        }

        public int PagesAllocated
        {
            get
            {
                return this.pagesAllocated;
            }
        }
    }
}

