namespace System.Net.Cache
{
    using System;
    using System.IO;
    using System.Net;

    internal class RangeStream : BaseWrapperStream, ICloseEx
    {
        private long m_Offset;
        private long m_Position;
        private long m_Size;

        internal RangeStream(Stream parentStream, long offset, long size) : base(parentStream)
        {
            this.m_Offset = offset;
            this.m_Size = size;
            if (!base.WrappedStream.CanSeek)
            {
                throw new NotSupportedException(SR.GetString("net_cache_non_seekable_stream_not_supported"));
            }
            base.WrappedStream.Position = offset;
            this.m_Position = offset;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (this.m_Position >= (this.m_Offset + this.m_Size))
            {
                count = 0;
            }
            else if ((this.m_Position + count) > (this.m_Offset + this.m_Size))
            {
                count = (int) ((this.m_Offset + this.m_Size) - this.m_Position);
            }
            return base.WrappedStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if ((this.m_Position + offset) > (this.m_Offset + this.m_Size))
            {
                throw new NotSupportedException(SR.GetString("net_cache_unsupported_partial_stream"));
            }
            return base.WrappedStream.BeginWrite(buffer, offset, count, callback, state);
        }

        protected sealed override void Dispose(bool disposing)
        {
            this.Dispose(disposing, CloseExState.Normal);
        }

        protected virtual void Dispose(bool disposing, CloseExState closeState)
        {
            try
            {
                if (disposing)
                {
                    ICloseEx wrappedStream = base.WrappedStream as ICloseEx;
                    if (wrappedStream != null)
                    {
                        wrappedStream.CloseEx(closeState);
                    }
                    else
                    {
                        base.WrappedStream.Close();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int num = base.WrappedStream.EndRead(asyncResult);
            this.m_Position += num;
            return num;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            base.WrappedStream.EndWrite(asyncResult);
            this.m_Position = base.WrappedStream.Position;
        }

        public override void Flush()
        {
            base.WrappedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.m_Position >= (this.m_Offset + this.m_Size))
            {
                return 0;
            }
            if ((this.m_Position + count) > (this.m_Offset + this.m_Size))
            {
                count = (int) ((this.m_Offset + this.m_Size) - this.m_Position);
            }
            int num = base.WrappedStream.Read(buffer, offset, count);
            this.m_Position += num;
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    offset += this.m_Offset;
                    if (offset > (this.m_Offset + this.m_Size))
                    {
                        offset = this.m_Offset + this.m_Size;
                    }
                    if (offset < this.m_Offset)
                    {
                        offset = this.m_Offset;
                    }
                    break;

                case SeekOrigin.End:
                    offset -= this.m_Offset + this.m_Size;
                    if (offset > 0L)
                    {
                        offset = 0L;
                    }
                    if (offset < -this.m_Size)
                    {
                        offset = -this.m_Size;
                    }
                    break;

                default:
                    if ((this.m_Position + offset) > (this.m_Offset + this.m_Size))
                    {
                        offset = (this.m_Offset + this.m_Size) - this.m_Position;
                    }
                    if ((this.m_Position + offset) < this.m_Offset)
                    {
                        offset = this.m_Offset - this.m_Position;
                    }
                    break;
            }
            this.m_Position = base.WrappedStream.Seek(offset, origin);
            return (this.m_Position - this.m_Offset);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("net_cache_unsupported_partial_stream"));
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            this.Dispose(true, closeState);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if ((this.m_Position + count) > (this.m_Offset + this.m_Size))
            {
                throw new NotSupportedException(SR.GetString("net_cache_unsupported_partial_stream"));
            }
            base.WrappedStream.Write(buffer, offset, count);
            this.m_Position += count;
        }

        public override bool CanRead
        {
            get
            {
                return base.WrappedStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return base.WrappedStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return base.WrappedStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return base.WrappedStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                long length = base.WrappedStream.Length;
                return this.m_Size;
            }
        }

        public override long Position
        {
            get
            {
                return (base.WrappedStream.Position - this.m_Offset);
            }
            set
            {
                value += this.m_Offset;
                if (value > (this.m_Offset + this.m_Size))
                {
                    value = this.m_Offset + this.m_Size;
                }
                base.WrappedStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return base.WrappedStream.ReadTimeout;
            }
            set
            {
                base.WrappedStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return base.WrappedStream.WriteTimeout;
            }
            set
            {
                base.WrappedStream.WriteTimeout = value;
            }
        }
    }
}

