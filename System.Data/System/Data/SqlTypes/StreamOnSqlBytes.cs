namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.IO;

    internal sealed class StreamOnSqlBytes : Stream
    {
        private long m_lPosition;
        private SqlBytes m_sb;

        internal StreamOnSqlBytes(SqlBytes sb)
        {
            this.m_sb = sb;
            this.m_lPosition = 0L;
        }

        private void CheckIfStreamClosed(string methodname)
        {
            if (this.FClosed())
            {
                throw ADP.StreamClosed(methodname);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.m_sb = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private bool FClosed()
        {
            return (this.m_sb == null);
        }

        public override void Flush()
        {
            if (this.m_sb.FStream())
            {
                this.m_sb.m_stream.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.CheckIfStreamClosed("Read");
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
            int num = (int) this.m_sb.Read(this.m_lPosition, buffer, offset, count);
            this.m_lPosition += num;
            return num;
        }

        public override int ReadByte()
        {
            this.CheckIfStreamClosed("ReadByte");
            if (this.m_lPosition >= this.m_sb.Length)
            {
                return -1;
            }
            int num = this.m_sb[this.m_lPosition];
            this.m_lPosition += 1L;
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.CheckIfStreamClosed("Seek");
            long num = 0L;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if ((offset < 0L) || (offset > this.m_sb.Length))
                    {
                        throw new ArgumentOutOfRangeException("offset");
                    }
                    this.m_lPosition = offset;
                    break;

                case SeekOrigin.Current:
                    num = this.m_lPosition + offset;
                    if ((num < 0L) || (num > this.m_sb.Length))
                    {
                        throw new ArgumentOutOfRangeException("offset");
                    }
                    this.m_lPosition = num;
                    break;

                case SeekOrigin.End:
                    num = this.m_sb.Length + offset;
                    if ((num < 0L) || (num > this.m_sb.Length))
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
            this.CheckIfStreamClosed("SetLength");
            this.m_sb.SetLength(value);
            if (this.m_lPosition > value)
            {
                this.m_lPosition = value;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckIfStreamClosed("Write");
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
            this.m_sb.Write(this.m_lPosition, buffer, offset, count);
            this.m_lPosition += count;
        }

        public override void WriteByte(byte value)
        {
            this.CheckIfStreamClosed("WriteByte");
            this.m_sb[this.m_lPosition] = value;
            this.m_lPosition += 1L;
        }

        public override bool CanRead
        {
            get
            {
                return ((this.m_sb != null) && !this.m_sb.IsNull);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return (this.m_sb != null);
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (this.m_sb == null)
                {
                    return false;
                }
                if (this.m_sb.IsNull)
                {
                    return (this.m_sb.m_rgbBuf != null);
                }
                return true;
            }
        }

        public override long Length
        {
            get
            {
                this.CheckIfStreamClosed("get_Length");
                return this.m_sb.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.CheckIfStreamClosed("get_Position");
                return this.m_lPosition;
            }
            set
            {
                this.CheckIfStreamClosed("set_Position");
                if ((value < 0L) || (value > this.m_sb.Length))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_lPosition = value;
            }
        }
    }
}

