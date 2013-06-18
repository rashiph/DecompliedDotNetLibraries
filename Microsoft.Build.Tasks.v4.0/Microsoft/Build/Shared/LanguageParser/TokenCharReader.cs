namespace Microsoft.Build.Shared.LanguageParser
{
    using Microsoft.Build.Shared;
    using System;
    using System.IO;

    internal class TokenCharReader
    {
        private int currentLine;
        private int position;
        private StreamMappedString sources;

        internal TokenCharReader(Stream binaryStream, bool forceANSI)
        {
            this.Reset();
            this.sources = new StreamMappedString(binaryStream, forceANSI);
        }

        internal string GetCurrentMatchedString(int startPosition)
        {
            return this.sources.Substring(startPosition, this.position - startPosition);
        }

        internal bool MatchNextIdentifierStart()
        {
            if ((this.CurrentCharacter != '_') && !TokenChar.IsLetter(this.CurrentCharacter))
            {
                return false;
            }
            return true;
        }

        internal void Reset()
        {
            this.position = 0;
            this.currentLine = 1;
        }

        internal bool Sink(string match)
        {
            return this.Sink(match, false);
        }

        private bool Sink(string match, bool ignoreCase)
        {
            if (!this.sources.IsPastEnd((this.position + match.Length) - 1))
            {
                string strB = this.sources.Substring(this.position, match.Length);
                if (string.Compare(match, strB, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                {
                    this.Skip(match.Length);
                    return true;
                }
            }
            return false;
        }

        internal char SinkCharacter()
        {
            char currentCharacter = this.CurrentCharacter;
            this.Skip();
            return currentCharacter;
        }

        internal bool SinkIdentifierPart()
        {
            if ((!TokenChar.IsLetter(this.CurrentCharacter) && !TokenChar.IsDecimalDigit(this.CurrentCharacter)) && ((!TokenChar.IsConnecting(this.CurrentCharacter) && !TokenChar.IsCombining(this.CurrentCharacter)) && !TokenChar.IsFormatting(this.CurrentCharacter)))
            {
                return false;
            }
            this.Skip();
            return true;
        }

        internal bool SinkIdentifierStart()
        {
            if (this.MatchNextIdentifierStart())
            {
                this.Skip();
                return true;
            }
            return false;
        }

        internal bool SinkIgnoreCase(string match)
        {
            return this.Sink(match, true);
        }

        internal bool SinkMultipleDecimalDigits()
        {
            int num = 0;
            while (TokenChar.IsDecimalDigit(this.CurrentCharacter))
            {
                num++;
                this.Skip();
            }
            return (num > 0);
        }

        internal bool SinkMultipleHexDigits()
        {
            int num = 0;
            while (TokenChar.IsHexDigit(this.CurrentCharacter))
            {
                num++;
                this.Skip();
            }
            return (num > 0);
        }

        internal bool SinkNewLine()
        {
            if (!this.EndOfLines)
            {
                int position = this.position;
                if (this.Sink("\r\n"))
                {
                    this.currentLine++;
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(position != this.position, "Expected position to be incremented.");
                    return true;
                }
                if (TokenChar.IsNewLine(this.CurrentCharacter))
                {
                    this.Skip();
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(position != this.position, "Expected position to be incremented.");
                    return true;
                }
            }
            return false;
        }

        internal bool SinkToEndOfLine()
        {
            while (!TokenChar.IsNewLine(this.CurrentCharacter))
            {
                this.Skip();
            }
            return true;
        }

        internal bool SinkUntil(string find)
        {
            bool flag = false;
            while (!this.EndOfLines && !flag)
            {
                if (this.Sink(find))
                {
                    flag = true;
                }
                else
                {
                    this.Skip();
                }
            }
            return flag;
        }

        protected void Skip()
        {
            if (TokenChar.IsNewLine(this.CurrentCharacter))
            {
                this.currentLine++;
            }
            this.position++;
        }

        protected void Skip(int n)
        {
            for (int i = 0; i < n; i++)
            {
                this.Skip();
            }
        }

        internal char CurrentCharacter
        {
            get
            {
                return this.sources.GetAt(this.position);
            }
        }

        internal int CurrentLine
        {
            get
            {
                return this.currentLine;
            }
        }

        internal bool EndOfLines
        {
            get
            {
                return this.sources.IsPastEnd(this.position);
            }
        }

        internal int Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }
    }
}

