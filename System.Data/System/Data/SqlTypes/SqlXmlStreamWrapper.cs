namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.IO;

    internal sealed class SqlXmlStreamWrapper : Stream
    {
        private bool m_isClosed;
        private long m_lPosition;
        private Stream m_stream;

        internal SqlXmlStreamWrapper(Stream stream)
        {
            this.m_stream = stream;
            this.m_lPosition = 0L;
            this.m_isClosed = false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.m_isClosed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            if (this.m_stream != null)
            {
                this.m_stream.Flush();
            }
        }

        private bool IsStreamClosed()
        {
            if ((!this.m_isClosed && (this.m_stream != null)) && ((this.m_stream.CanRead || this.m_stream.CanWrite) || this.m_stream.CanSeek))
            {
                return false;
            }
            return true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.ThrowIfStreamClosed("Read");
            this.ThrowIfStreamCannotRead("Read");
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.m_stream.CanSeek && (this.m_stream.Position != this.m_lPosition))
            {
                this.m_stream.Seek(this.m_lPosition, SeekOrigin.Begin);
            }
            int num = this.m_stream.Read(buffer, offset, count);
            this.m_lPosition += num;
            return num;
        }

        public override int ReadByte()
        {
            this.ThrowIfStreamClosed("ReadByte");
            this.ThrowIfStreamCannotRead("ReadByte");
            if (this.m_stream.CanSeek && (this.m_lPosition >= this.m_stream.Length))
            {
                return -1;
            }
            if (this.m_stream.CanSeek && (this.m_stream.Position != this.m_lPosition))
            {
                this.m_stream.Seek(this.m_lPosition, SeekOrigin.Begin);
            }
            int num = this.m_stream.ReadByte();
            this.m_lPosition += 1L;
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long num = 0L;
            this.ThrowIfStreamClosed("Seek");
            this.ThrowIfStreamCannotSeek("Seek");
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if ((offset < 0L) || (offset > this.m_stream.Length))
                    {
                        throw new ArgumentOutOfRangeException("offset");
                    }
                    this.m_lPosition = offset;
                    break;

                case SeekOrigin.Current:
                    num = this.m_lPosition + offset;
                    if ((num < 0L) || (num > this.m_stream.Length))
                    {
                        throw new ArgumentOutOfRangeException("offset");
                    }
                    this.m_lPosition = num;
                    break;

                case SeekOrigin.End:
                    num = this.m_stream.Length + offset;
                    if ((num < 0L) || (num > this.m_stream.Length))
                    {
                        throw new ArgumentOutOfRangeException("offset");
                    }
                    this.m_lPosition = num;
                    break;

                default:
                    throw ADP.InvalidSeekOrigin("offset");
            }
            return this.m_lPosition;
        }

        public override void SetLength(long value)
        {
            this.ThrowIfStreamClosed("SetLength");
            this.ThrowIfStreamCannotSeek("SetLength");
            this.m_stream.SetLength(value);
            if (this.m_lPosition > value)
            {
                this.m_lPosition = value;
            }
        }

        private void ThrowIfStreamCannotRead(string method)
        {
            if (!this.m_stream.CanRead)
            {
                throw new NotSupportedException(SQLResource.InvalidOpStreamNonReadable(method));
            }
        }

        private void ThrowIfStreamCannotSeek(string method)
        {
            if (!this.m_stream.CanSeek)
            {
                throw new NotSupportedException(SQLResource.InvalidOpStreamNonSeekable(method));
            }
        }

        private void ThrowIfStreamCannotWrite(string method)
        {
            if (!this.m_stream.CanWrite)
            {
                throw new NotSupportedException(SQLResource.InvalidOpStreamNonWritable(method));
            }
        }

        private void ThrowIfStreamClosed(string method)
        {
            if (this.IsStreamClosed())
            {
                throw new ObjectDisposedException(SQLResource.InvalidOpStreamClosed(method));
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.ThrowIfStreamClosed("Write");
            this.ThrowIfStreamCannotWrite("Write");
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (this.m_stream.CanSeek && (this.m_stream.Position != this.m_lPosition))
            {
                this.m_stream.Seek(this.m_lPosition, SeekOrigin.Begin);
            }
            this.m_stream.Write(buffer, offset, count);
            this.m_lPosition += count;
        }

        public override void WriteByte(byte value)
        {
            this.ThrowIfStreamClosed("WriteByte");
            this.ThrowIfStreamCannotWrite("WriteByte");
            if (this.m_stream.CanSeek && (this.m_stream.Position != this.m_lPosition))
            {
                this.m_stream.Seek(this.m_lPosition, SeekOrigin.Begin);
            }
            this.m_stream.WriteByte(value);
            this.m_lPosition += 1L;
        }

        public override bool CanRead
        {
            get
            {
                if (this.IsStreamClosed())
                {
                    return false;
                }
                return this.m_stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (this.IsStreamClosed())
                {
                    return false;
                }
                return this.m_stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (this.IsStreamClosed())
                {
                    return false;
                }
                return this.m_stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                this.ThrowIfStreamClosed("get_Length");
                this.ThrowIfStreamCannotSeek("get_Length");
                return this.m_stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.ThrowIfStreamClosed("get_Position");
                this.ThrowIfStreamCannotSeek("get_Position");
                return this.m_lPosition;
            }
            set
            {
                this.ThrowIfStreamClosed("set_Position");
                this.ThrowIfStreamCannotSeek("set_Position");
                if ((value < 0L) || (value > this.m_stream.Length))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_lPosition = value;
            }
        }
    }
}

