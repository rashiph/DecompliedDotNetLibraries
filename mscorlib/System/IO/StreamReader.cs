namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [Serializable, ComVisible(true)]
    public class StreamReader : TextReader
    {
        private bool _checkPreamble;
        private bool _closable;
        private bool _detectEncoding;
        private bool _isBlocked;
        private int _maxCharsPerBuffer;
        private byte[] _preamble;
        private byte[] byteBuffer;
        private int byteLen;
        private int bytePos;
        private char[] charBuffer;
        private int charLen;
        private int charPos;
        private System.Text.Decoder decoder;
        internal const int DefaultBufferSize = 0x400;
        private const int DefaultFileStreamBufferSize = 0x1000;
        private Encoding encoding;
        private const int MinBufferSize = 0x80;
        public static readonly StreamReader Null = new NullStreamReader();
        private Stream stream;

        internal StreamReader()
        {
        }

        public StreamReader(Stream stream) : this(stream, true)
        {
        }

        [SecuritySafeCritical]
        public StreamReader(string path) : this(path, true)
        {
        }

        public StreamReader(Stream stream, bool detectEncodingFromByteOrderMarks) : this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, 0x400)
        {
        }

        public StreamReader(Stream stream, Encoding encoding) : this(stream, encoding, true, 0x400)
        {
        }

        [SecuritySafeCritical]
        public StreamReader(string path, bool detectEncodingFromByteOrderMarks) : this(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, 0x400)
        {
        }

        [SecuritySafeCritical]
        public StreamReader(string path, Encoding encoding) : this(path, encoding, true, 0x400)
        {
        }

        public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks) : this(stream, encoding, detectEncodingFromByteOrderMarks, 0x400)
        {
        }

        [SecuritySafeCritical]
        public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks) : this(path, encoding, detectEncodingFromByteOrderMarks, 0x400)
        {
        }

        public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            if ((stream == null) || (encoding == null))
            {
                throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotReadable"));
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            this.Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
        }

        [SecuritySafeCritical]
        public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            if ((path == null) || (encoding == null))
            {
                throw new ArgumentNullException((path == null) ? "path" : "encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, FileOptions.SequentialScan);
            this.Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
        }

        internal StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool closable) : this(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            this._closable = closable;
        }

        public override void Close()
        {
            this.Dispose(true);
        }

        private void CompressBuffer(int n)
        {
            Buffer.InternalBlockCopy(this.byteBuffer, n, this.byteBuffer, 0, this.byteLen - n);
            this.byteLen -= n;
        }

        private void DetectEncoding()
        {
            if (this.byteLen >= 2)
            {
                this._detectEncoding = false;
                bool flag = false;
                if ((this.byteBuffer[0] == 0xfe) && (this.byteBuffer[1] == 0xff))
                {
                    this.encoding = new UnicodeEncoding(true, true);
                    this.CompressBuffer(2);
                    flag = true;
                }
                else if ((this.byteBuffer[0] == 0xff) && (this.byteBuffer[1] == 0xfe))
                {
                    if (((this.byteLen < 4) || (this.byteBuffer[2] != 0)) || (this.byteBuffer[3] != 0))
                    {
                        this.encoding = new UnicodeEncoding(false, true);
                        this.CompressBuffer(2);
                        flag = true;
                    }
                    else
                    {
                        this.encoding = new UTF32Encoding(false, true);
                        this.CompressBuffer(4);
                        flag = true;
                    }
                }
                else if (((this.byteLen >= 3) && (this.byteBuffer[0] == 0xef)) && ((this.byteBuffer[1] == 0xbb) && (this.byteBuffer[2] == 0xbf)))
                {
                    this.encoding = Encoding.UTF8;
                    this.CompressBuffer(3);
                    flag = true;
                }
                else if ((((this.byteLen >= 4) && (this.byteBuffer[0] == 0)) && ((this.byteBuffer[1] == 0) && (this.byteBuffer[2] == 0xfe))) && (this.byteBuffer[3] == 0xff))
                {
                    this.encoding = new UTF32Encoding(true, true);
                    this.CompressBuffer(4);
                    flag = true;
                }
                else if (this.byteLen == 2)
                {
                    this._detectEncoding = true;
                }
                if (flag)
                {
                    this.decoder = this.encoding.GetDecoder();
                    this._maxCharsPerBuffer = this.encoding.GetMaxCharCount(this.byteBuffer.Length);
                    this.charBuffer = new char[this._maxCharsPerBuffer];
                }
            }
        }

        public void DiscardBufferedData()
        {
            this.byteLen = 0;
            this.charLen = 0;
            this.charPos = 0;
            if (this.encoding != null)
            {
                this.decoder = this.encoding.GetDecoder();
            }
            this._isBlocked = false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((this.Closable && disposing) && (this.stream != null))
                {
                    this.stream.Close();
                }
            }
            finally
            {
                if (this.Closable && (this.stream != null))
                {
                    this.stream = null;
                    this.encoding = null;
                    this.decoder = null;
                    this.byteBuffer = null;
                    this.charBuffer = null;
                    this.charPos = 0;
                    this.charLen = 0;
                    base.Dispose(disposing);
                }
            }
        }

        internal void Init(Stream stream)
        {
            this.stream = stream;
            this._closable = true;
        }

        private void Init(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            this.stream = stream;
            this.encoding = encoding;
            this.decoder = encoding.GetDecoder();
            if (bufferSize < 0x80)
            {
                bufferSize = 0x80;
            }
            this.byteBuffer = new byte[bufferSize];
            this._maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            this.charBuffer = new char[this._maxCharsPerBuffer];
            this.byteLen = 0;
            this.bytePos = 0;
            this._detectEncoding = detectEncodingFromByteOrderMarks;
            this._preamble = encoding.GetPreamble();
            this._checkPreamble = this._preamble.Length > 0;
            this._isBlocked = false;
            this._closable = true;
        }

        private bool IsPreamble()
        {
            if (this._checkPreamble)
            {
                int num = (this.byteLen >= this._preamble.Length) ? (this._preamble.Length - this.bytePos) : (this.byteLen - this.bytePos);
                int num2 = 0;
                while (num2 < num)
                {
                    if (this.byteBuffer[this.bytePos] != this._preamble[this.bytePos])
                    {
                        this.bytePos = 0;
                        this._checkPreamble = false;
                        break;
                    }
                    num2++;
                    this.bytePos++;
                }
                if (this._checkPreamble && (this.bytePos == this._preamble.Length))
                {
                    this.CompressBuffer(this._preamble.Length);
                    this.bytePos = 0;
                    this._checkPreamble = false;
                    this._detectEncoding = false;
                }
            }
            return this._checkPreamble;
        }

        [SecuritySafeCritical]
        public override int Peek()
        {
            if (this.stream == null)
            {
                __Error.ReaderClosed();
            }
            if ((this.charPos != this.charLen) || (!this._isBlocked && (this.ReadBuffer() != 0)))
            {
                return this.charBuffer[this.charPos];
            }
            return -1;
        }

        [SecuritySafeCritical]
        public override int Read()
        {
            if (this.stream == null)
            {
                __Error.ReaderClosed();
            }
            if ((this.charPos == this.charLen) && (this.ReadBuffer() == 0))
            {
                return -1;
            }
            int num = this.charBuffer[this.charPos];
            this.charPos++;
            return num;
        }

        [SecuritySafeCritical]
        public override int Read([In, Out] char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this.stream == null)
            {
                __Error.ReaderClosed();
            }
            int num = 0;
            bool readToUserBuffer = false;
            while (count > 0)
            {
                int num2 = this.charLen - this.charPos;
                if (num2 == 0)
                {
                    num2 = this.ReadBuffer(buffer, index + num, count, out readToUserBuffer);
                }
                if (num2 == 0)
                {
                    return num;
                }
                if (num2 > count)
                {
                    num2 = count;
                }
                if (!readToUserBuffer)
                {
                    Buffer.InternalBlockCopy(this.charBuffer, this.charPos * 2, buffer, (index + num) * 2, num2 * 2);
                    this.charPos += num2;
                }
                num += num2;
                count -= num2;
                if (this._isBlocked)
                {
                    return num;
                }
            }
            return num;
        }

        internal virtual int ReadBuffer()
        {
            this.charLen = 0;
            this.charPos = 0;
            if (!this._checkPreamble)
            {
                this.byteLen = 0;
            }
            do
            {
                if (this._checkPreamble)
                {
                    int num = this.stream.Read(this.byteBuffer, this.bytePos, this.byteBuffer.Length - this.bytePos);
                    if (num == 0)
                    {
                        if (this.byteLen > 0)
                        {
                            this.charLen += this.decoder.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, this.charLen);
                        }
                        return this.charLen;
                    }
                    this.byteLen += num;
                }
                else
                {
                    this.byteLen = this.stream.Read(this.byteBuffer, 0, this.byteBuffer.Length);
                    if (this.byteLen == 0)
                    {
                        return this.charLen;
                    }
                }
                this._isBlocked = this.byteLen < this.byteBuffer.Length;
                if (!this.IsPreamble())
                {
                    if (this._detectEncoding && (this.byteLen >= 2))
                    {
                        this.DetectEncoding();
                    }
                    this.charLen += this.decoder.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, this.charLen);
                }
            }
            while (this.charLen == 0);
            return this.charLen;
        }

        private int ReadBuffer(char[] userBuffer, int userOffset, int desiredChars, out bool readToUserBuffer)
        {
            this.charLen = 0;
            this.charPos = 0;
            if (!this._checkPreamble)
            {
                this.byteLen = 0;
            }
            int charIndex = 0;
            readToUserBuffer = desiredChars >= this._maxCharsPerBuffer;
            do
            {
                if (this._checkPreamble)
                {
                    int num2 = this.stream.Read(this.byteBuffer, this.bytePos, this.byteBuffer.Length - this.bytePos);
                    if (num2 == 0)
                    {
                        if (this.byteLen > 0)
                        {
                            if (readToUserBuffer)
                            {
                                charIndex += this.decoder.GetChars(this.byteBuffer, 0, this.byteLen, userBuffer, userOffset + charIndex);
                                this.charLen = 0;
                                return charIndex;
                            }
                            charIndex = this.decoder.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, charIndex);
                            this.charLen += charIndex;
                        }
                        return charIndex;
                    }
                    this.byteLen += num2;
                }
                else
                {
                    this.byteLen = this.stream.Read(this.byteBuffer, 0, this.byteBuffer.Length);
                    if (this.byteLen == 0)
                    {
                        return charIndex;
                    }
                }
                this._isBlocked = this.byteLen < this.byteBuffer.Length;
                if (!this.IsPreamble())
                {
                    if (this._detectEncoding && (this.byteLen >= 2))
                    {
                        this.DetectEncoding();
                        readToUserBuffer = desiredChars >= this._maxCharsPerBuffer;
                    }
                    this.charPos = 0;
                    if (readToUserBuffer)
                    {
                        charIndex += this.decoder.GetChars(this.byteBuffer, 0, this.byteLen, userBuffer, userOffset + charIndex);
                        this.charLen = 0;
                    }
                    else
                    {
                        charIndex = this.decoder.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, charIndex);
                        this.charLen += charIndex;
                    }
                }
            }
            while (charIndex == 0);
            this._isBlocked &= charIndex < desiredChars;
            return charIndex;
        }

        [SecuritySafeCritical]
        public override string ReadLine()
        {
            if (this.stream == null)
            {
                __Error.ReaderClosed();
            }
            if ((this.charPos == this.charLen) && (this.ReadBuffer() == 0))
            {
                return null;
            }
            StringBuilder builder = null;
            do
            {
                int charPos = this.charPos;
                do
                {
                    char ch = this.charBuffer[charPos];
                    switch (ch)
                    {
                        case '\r':
                        case '\n':
                            string str;
                            if (builder != null)
                            {
                                builder.Append(this.charBuffer, this.charPos, charPos - this.charPos);
                                str = builder.ToString();
                            }
                            else
                            {
                                str = new string(this.charBuffer, this.charPos, charPos - this.charPos);
                            }
                            this.charPos = charPos + 1;
                            if (((ch == '\r') && ((this.charPos < this.charLen) || (this.ReadBuffer() > 0))) && (this.charBuffer[this.charPos] == '\n'))
                            {
                                this.charPos++;
                            }
                            return str;
                    }
                    charPos++;
                }
                while (charPos < this.charLen);
                charPos = this.charLen - this.charPos;
                if (builder == null)
                {
                    builder = new StringBuilder(charPos + 80);
                }
                builder.Append(this.charBuffer, this.charPos, charPos);
            }
            while (this.ReadBuffer() > 0);
            return builder.ToString();
        }

        [SecuritySafeCritical]
        public override string ReadToEnd()
        {
            if (this.stream == null)
            {
                __Error.ReaderClosed();
            }
            StringBuilder builder = new StringBuilder(this.charLen - this.charPos);
            do
            {
                builder.Append(this.charBuffer, this.charPos, this.charLen - this.charPos);
                this.charPos = this.charLen;
                this.ReadBuffer();
            }
            while (this.charLen > 0);
            return builder.ToString();
        }

        public virtual Stream BaseStream
        {
            get
            {
                return this.stream;
            }
        }

        internal bool Closable
        {
            get
            {
                return this._closable;
            }
        }

        public virtual Encoding CurrentEncoding
        {
            get
            {
                return this.encoding;
            }
        }

        public bool EndOfStream
        {
            [SecuritySafeCritical]
            get
            {
                if (this.stream == null)
                {
                    __Error.ReaderClosed();
                }
                if (this.charPos < this.charLen)
                {
                    return false;
                }
                return (this.ReadBuffer() == 0);
            }
        }

        private class NullStreamReader : StreamReader
        {
            internal NullStreamReader()
            {
                base.Init(Stream.Null);
            }

            protected override void Dispose(bool disposing)
            {
            }

            public override int Peek()
            {
                return -1;
            }

            public override int Read()
            {
                return -1;
            }

            public override int Read(char[] buffer, int index, int count)
            {
                return 0;
            }

            internal override int ReadBuffer()
            {
                return 0;
            }

            public override string ReadLine()
            {
                return null;
            }

            public override string ReadToEnd()
            {
                return string.Empty;
            }

            public override Stream BaseStream
            {
                get
                {
                    return Stream.Null;
                }
            }

            public override Encoding CurrentEncoding
            {
                get
                {
                    return Encoding.Unicode;
                }
            }
        }
    }
}

