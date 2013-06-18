namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Web.Services;

    internal class BufferedResponseStream : Stream
    {
        private byte[] buffer;
        private bool flushEnabled = true;
        private Stream outputStream;
        private int position;

        internal BufferedResponseStream(Stream outputStream, int buffersize)
        {
            this.buffer = new byte[buffersize];
            this.outputStream = outputStream;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException(Res.GetString("StreamDoesNotRead"));
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.outputStream.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException(Res.GetString("StreamDoesNotRead"));
        }

        public override void Flush()
        {
            if (this.flushEnabled)
            {
                this.FlushWrite();
            }
        }

        private void FlushWrite()
        {
            if (this.position > 0)
            {
                this.outputStream.Write(this.buffer, 0, this.position);
                this.position = 0;
            }
            this.outputStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(Res.GetString("StreamDoesNotRead"));
        }

        public override int ReadByte()
        {
            throw new NotSupportedException(Res.GetString("StreamDoesNotRead"));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(Res.GetString("StreamDoesNotSeek"));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(Res.GetString("StreamDoesNotSeek"));
        }

        public override void Write(byte[] array, int offset, int count)
        {
            if (this.position > 0)
            {
                int length = this.buffer.Length - this.position;
                if (length > 0)
                {
                    if (length > count)
                    {
                        length = count;
                    }
                    Array.Copy(array, offset, this.buffer, this.position, length);
                    this.position += length;
                    if (count == length)
                    {
                        return;
                    }
                    offset += length;
                    count -= length;
                }
                this.FlushWrite();
            }
            if (count >= this.buffer.Length)
            {
                this.outputStream.Write(array, offset, count);
            }
            else
            {
                Array.Copy(array, offset, this.buffer, this.position, count);
                this.position = count;
            }
        }

        public override void WriteByte(byte value)
        {
            if (this.position == this.buffer.Length)
            {
                this.FlushWrite();
            }
            this.buffer[this.position++] = value;
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        internal bool FlushEnabled
        {
            set
            {
                this.flushEnabled = value;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException(Res.GetString("StreamDoesNotSeek"));
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(Res.GetString("StreamDoesNotSeek"));
            }
            set
            {
                throw new NotSupportedException(Res.GetString("StreamDoesNotSeek"));
            }
        }
    }
}

