namespace System.Web
{
    using System;
    using System.IO;
    using System.Web.Configuration;
    using System.Web.Hosting;

    internal class HttpBufferlessInputStream : Stream
    {
        private HttpContext _Context;
        private long _CurrentPosition;
        private long _Length = -1L;
        private bool _MaxLengthRead;
        private long _MaxRequestLength;
        private bool _PreloadedConsumed;

        internal HttpBufferlessInputStream(HttpContext context)
        {
            this._Context = context;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if ((this._Context.WorkerRequest == null) || (count == 0))
            {
                return 0;
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || ((offset + count) > buffer.Length))
            {
                throw new ArgumentException(null, "offset");
            }
            if (count < 0)
            {
                throw new ArgumentException(null, "count");
            }
            int numBytesRead = 0;
            if (!this._MaxLengthRead)
            {
                this._MaxRequestLength = RuntimeConfig.GetConfig(this._Context).HttpRuntime.MaxRequestLengthBytes;
                if (this.Length > this._MaxRequestLength)
                {
                    if (!(this._Context.WorkerRequest is IIS7WorkerRequest))
                    {
                        this._Context.Response.CloseConnectionAfterError();
                    }
                    throw new HttpException(System.Web.SR.GetString("Max_request_length_exceeded"), null, 0xbbc);
                }
                this._MaxLengthRead = true;
            }
            if (!this._PreloadedConsumed)
            {
                byte[] preloadedEntityBody = this._Context.WorkerRequest.GetPreloadedEntityBody();
                if (preloadedEntityBody != null)
                {
                    int num2 = preloadedEntityBody.Length - ((int) this._CurrentPosition);
                    int num3 = Math.Min(count, num2);
                    Buffer.BlockCopy(preloadedEntityBody, (int) this._CurrentPosition, buffer, offset, num3);
                    this.UpdateCounters(num3, ref offset, ref count, ref numBytesRead);
                    this._PreloadedConsumed = numBytesRead == num2;
                }
                else
                {
                    this._PreloadedConsumed = true;
                }
            }
            if ((count != 0) && !this._Context.WorkerRequest.IsEntireEntityBodyIsPreloaded())
            {
                while (count > 0)
                {
                    long num4 = this._MaxRequestLength - this._CurrentPosition;
                    int size = (int) Math.Min(0x7fffffffL, Math.Min((long) count, num4 + 1L));
                    int bytesRead = this._Context.WorkerRequest.ReadEntityBody(buffer, offset, size);
                    if (bytesRead <= 0)
                    {
                        if (!this._Context.WorkerRequest.IsClientConnected())
                        {
                            throw new HttpException(System.Web.SR.GetString("ViewState_ClientDisconnected"));
                        }
                        return numBytesRead;
                    }
                    this.UpdateCounters(bytesRead, ref offset, ref count, ref numBytesRead);
                }
                return numBytesRead;
            }
            return numBytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long length)
        {
            throw new NotSupportedException();
        }

        private void UpdateCounters(int bytesRead, ref int offset, ref int count, ref int numBytesRead)
        {
            this._Context.WorkerRequest.UpdateRequestCounters(bytesRead);
            count -= bytesRead;
            offset += bytesRead;
            this._CurrentPosition += bytesRead;
            numBytesRead += bytesRead;
            if (this._Length < this._CurrentPosition)
            {
                this._Length = this._CurrentPosition;
            }
            if (this.Length > this._MaxRequestLength)
            {
                if (!(this._Context.WorkerRequest is IIS7WorkerRequest))
                {
                    this._Context.Response.CloseConnectionAfterError();
                }
                throw new HttpException(System.Web.SR.GetString("Max_request_length_exceeded"), null, 0xbbc);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
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
                return false;
            }
        }

        public override long Length
        {
            get
            {
                if (this._Length < 0L)
                {
                    this._Length = this._Context.Request.ContentLength;
                }
                return this._Length;
            }
        }

        public override long Position
        {
            get
            {
                return this._CurrentPosition;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

