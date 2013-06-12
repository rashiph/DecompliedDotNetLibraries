namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.IO;

    internal sealed class StreamOnSqlChars : SqlStreamChars
    {
        private long m_lPosition;
        private SqlChars m_sqlchars;

        internal StreamOnSqlChars(SqlChars s)
        {
            this.m_sqlchars = s;
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
            this.m_sqlchars = null;
        }

        private bool FClosed()
        {
            return (this.m_sqlchars == null);
        }

        public override void Flush()
        {
            if (this.m_sqlchars.FStream())
            {
                this.m_sqlchars.m_stream.Flush();
            }
        }

        public override int Read(char[] buffer, int offset, int count)
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
            int num = (int) this.m_sqlchars.Read(this.m_lPosition, buffer, offset, count);
            this.m_lPosition += num;
            return num;
        }

        public override int ReadChar()
        {
            this.CheckIfStreamClosed("ReadChar");
            if (this.m_lPosition >= this.m_sqlchars.Length)
            {
                return -1;
            }
            int num = this.m_sqlchars[this.m_lPosition];
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
                    if ((offset < 0L) || (offset > this.m_sqlchars.Length))
                    {
                        throw ADP.ArgumentOutOfRange("offset");
                    }
                    this.m_lPosition = offset;
                    break;

                case SeekOrigin.Current:
                    num = this.m_lPosition + offset;
                    if ((num < 0L) || (num > this.m_sqlchars.Length))
                    {
                        throw ADP.ArgumentOutOfRange("offset");
                    }
                    this.m_lPosition = num;
                    break;

                case SeekOrigin.End:
                    num = this.m_sqlchars.Length + offset;
                    if ((num < 0L) || (num > this.m_sqlchars.Length))
                    {
                        throw ADP.ArgumentOutOfRange("offset");
                    }
                    this.m_lPosition = num;
                    break;

                default:
                    throw ADP.ArgumentOutOfRange("offset");
            }
            return this.m_lPosition;
        }

        public override void SetLength(long value)
        {
            this.CheckIfStreamClosed("SetLength");
            this.m_sqlchars.SetLength(value);
            if (this.m_lPosition > value)
            {
                this.m_lPosition = value;
            }
        }

        public override void Write(char[] buffer, int offset, int count)
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
            this.m_sqlchars.Write(this.m_lPosition, buffer, offset, count);
            this.m_lPosition += count;
        }

        public override void WriteChar(char value)
        {
            this.CheckIfStreamClosed("WriteChar");
            this.m_sqlchars[this.m_lPosition] = value;
            this.m_lPosition += 1L;
        }

        public override bool CanRead
        {
            get
            {
                return ((this.m_sqlchars != null) && !this.m_sqlchars.IsNull);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return (this.m_sqlchars != null);
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (this.m_sqlchars == null)
                {
                    return false;
                }
                if (this.m_sqlchars.IsNull)
                {
                    return (this.m_sqlchars.m_rgchBuf != null);
                }
                return true;
            }
        }

        public override bool IsNull
        {
            get
            {
                if (this.m_sqlchars != null)
                {
                    return this.m_sqlchars.IsNull;
                }
                return true;
            }
        }

        public override long Length
        {
            get
            {
                this.CheckIfStreamClosed("get_Length");
                return this.m_sqlchars.Length;
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
                if ((value < 0L) || (value > this.m_sqlchars.Length))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_lPosition = value;
            }
        }
    }
}

