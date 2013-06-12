namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public class StreamWriter : TextWriter
    {
        private static System.Text.Encoding _UTF8NoBOM;
        internal bool autoFlush;
        internal byte[] byteBuffer;
        internal char[] charBuffer;
        internal int charLen;
        internal int charPos;
        private bool closable;
        private const int DefaultBufferSize = 0x400;
        private const int DefaultFileStreamBufferSize = 0x1000;
        private System.Text.Encoder encoder;
        private System.Text.Encoding encoding;
        private bool haveWrittenPreamble;
        [NonSerialized]
        private MdaHelper mdaHelper;
        private const int MinBufferSize = 0x80;
        public static readonly StreamWriter Null = new StreamWriter(Stream.Null, new UTF8Encoding(false, true), 0x80, false);
        internal Stream stream;

        internal StreamWriter() : base(null)
        {
        }

        [SecuritySafeCritical]
        public StreamWriter(Stream stream) : this(stream, UTF8NoBOM, 0x400)
        {
        }

        [SecuritySafeCritical]
        public StreamWriter(string path) : this(path, false, UTF8NoBOM, 0x400)
        {
        }

        [SecuritySafeCritical]
        public StreamWriter(Stream stream, System.Text.Encoding encoding) : this(stream, encoding, 0x400)
        {
        }

        [SecuritySafeCritical]
        public StreamWriter(string path, bool append) : this(path, append, UTF8NoBOM, 0x400)
        {
        }

        [SecuritySafeCritical]
        public StreamWriter(Stream stream, System.Text.Encoding encoding, int bufferSize) : base(null)
        {
            if ((stream == null) || (encoding == null))
            {
                throw new ArgumentNullException((stream == null) ? "stream" : "encoding");
            }
            if (!stream.CanWrite)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotWritable"));
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            this.Init(stream, encoding, bufferSize);
        }

        [SecuritySafeCritical]
        public StreamWriter(string path, bool append, System.Text.Encoding encoding) : this(path, append, encoding, 0x400)
        {
        }

        internal StreamWriter(Stream stream, System.Text.Encoding encoding, int bufferSize, bool closeable) : this(stream, encoding, bufferSize)
        {
            this.closable = closeable;
        }

        [SecuritySafeCritical]
        public StreamWriter(string path, bool append, System.Text.Encoding encoding, int bufferSize) : base(null)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            Stream stream = CreateFile(path, append);
            this.Init(stream, encoding, bufferSize);
        }

        public override void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static Stream CreateFile(string path, bool append)
        {
            return new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 0x1000, FileOptions.SequentialScan);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((this.stream != null) && (disposing || (!this.Closable && (this.stream is __ConsoleStream))))
                {
                    this.Flush(true, true);
                    if (this.mdaHelper != null)
                    {
                        GC.SuppressFinalize(this.mdaHelper);
                    }
                }
            }
            finally
            {
                if (this.Closable && (this.stream != null))
                {
                    try
                    {
                        if (disposing)
                        {
                            this.stream.Close();
                        }
                    }
                    finally
                    {
                        this.stream = null;
                        this.byteBuffer = null;
                        this.charBuffer = null;
                        this.encoding = null;
                        this.encoder = null;
                        this.charLen = 0;
                        base.Dispose(disposing);
                    }
                }
            }
        }

        public override void Flush()
        {
            this.Flush(true, true);
        }

        private void Flush(bool flushStream, bool flushEncoder)
        {
            if (this.stream == null)
            {
                __Error.WriterClosed();
            }
            if (((this.charPos != 0) || flushStream) || flushEncoder)
            {
                if (!this.haveWrittenPreamble)
                {
                    this.haveWrittenPreamble = true;
                    byte[] preamble = this.encoding.GetPreamble();
                    if (preamble.Length > 0)
                    {
                        this.stream.Write(preamble, 0, preamble.Length);
                    }
                }
                int count = this.encoder.GetBytes(this.charBuffer, 0, this.charPos, this.byteBuffer, 0, flushEncoder);
                this.charPos = 0;
                if (count > 0)
                {
                    this.stream.Write(this.byteBuffer, 0, count);
                }
                if (flushStream)
                {
                    this.stream.Flush();
                }
            }
        }

        private void Init(Stream stream, System.Text.Encoding encoding, int bufferSize)
        {
            this.stream = stream;
            this.encoding = encoding;
            this.encoder = encoding.GetEncoder();
            if (bufferSize < 0x80)
            {
                bufferSize = 0x80;
            }
            this.charBuffer = new char[bufferSize];
            this.byteBuffer = new byte[encoding.GetMaxByteCount(bufferSize)];
            this.charLen = bufferSize;
            if (stream.CanSeek && (stream.Position > 0L))
            {
                this.haveWrittenPreamble = true;
            }
            this.closable = true;
            if (Mda.StreamWriterBufferedDataLost.Enabled)
            {
                string cs = null;
                if (Mda.StreamWriterBufferedDataLost.CaptureAllocatedCallStack)
                {
                    cs = Environment.GetStackTrace(null, false);
                }
                this.mdaHelper = new MdaHelper(this, cs);
            }
        }

        public override void Write(char value)
        {
            if (this.charPos == this.charLen)
            {
                this.Flush(false, false);
            }
            this.charBuffer[this.charPos] = value;
            this.charPos++;
            if (this.autoFlush)
            {
                this.Flush(true, false);
            }
        }

        [SecuritySafeCritical]
        public override void Write(char[] buffer)
        {
            if (buffer != null)
            {
                int num3;
                int num = 0;
                for (int i = buffer.Length; i > 0; i -= num3)
                {
                    if (this.charPos == this.charLen)
                    {
                        this.Flush(false, false);
                    }
                    num3 = this.charLen - this.charPos;
                    if (num3 > i)
                    {
                        num3 = i;
                    }
                    Buffer.InternalBlockCopy(buffer, num * 2, this.charBuffer, this.charPos * 2, num3 * 2);
                    this.charPos += num3;
                    num += num3;
                }
                if (this.autoFlush)
                {
                    this.Flush(true, false);
                }
            }
        }

        [SecuritySafeCritical]
        public override void Write(string value)
        {
            if (value != null)
            {
                int length = value.Length;
                int sourceIndex = 0;
                while (length > 0)
                {
                    if (this.charPos == this.charLen)
                    {
                        this.Flush(false, false);
                    }
                    int count = this.charLen - this.charPos;
                    if (count > length)
                    {
                        count = length;
                    }
                    value.CopyTo(sourceIndex, this.charBuffer, this.charPos, count);
                    this.charPos += count;
                    sourceIndex += count;
                    length -= count;
                }
                if (this.autoFlush)
                {
                    this.Flush(true, false);
                }
            }
        }

        [SecuritySafeCritical]
        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            while (count > 0)
            {
                if (this.charPos == this.charLen)
                {
                    this.Flush(false, false);
                }
                int num = this.charLen - this.charPos;
                if (num > count)
                {
                    num = count;
                }
                Buffer.InternalBlockCopy(buffer, index * 2, this.charBuffer, this.charPos * 2, num * 2);
                this.charPos += num;
                index += num;
                count -= num;
            }
            if (this.autoFlush)
            {
                this.Flush(true, false);
            }
        }

        public virtual bool AutoFlush
        {
            get
            {
                return this.autoFlush;
            }
            set
            {
                this.autoFlush = value;
                if (value)
                {
                    this.Flush(true, false);
                }
            }
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
                return this.closable;
            }
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
        }

        internal bool HaveWrittenPreamble
        {
            set
            {
                this.haveWrittenPreamble = value;
            }
        }

        internal static System.Text.Encoding UTF8NoBOM
        {
            get
            {
                if (_UTF8NoBOM == null)
                {
                    UTF8Encoding encoding = new UTF8Encoding(false, true);
                    Thread.MemoryBarrier();
                    _UTF8NoBOM = encoding;
                }
                return _UTF8NoBOM;
            }
        }
    }
}

