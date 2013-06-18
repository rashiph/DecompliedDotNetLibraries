namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Web.Services;

    internal class SoapExtensionStream : Stream
    {
        private bool hasWritten;
        internal Stream innerStream;
        private bool streamReady;

        internal SoapExtensionStream()
        {
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.EnsureStreamReady();
            return this.innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.EnsureStreamReady();
            this.hasWritten = true;
            return this.innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.EnsureStreamReady();
                    this.hasWritten = true;
                    this.innerStream.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            this.EnsureStreamReady();
            return this.innerStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.EnsureStreamReady();
            this.hasWritten = true;
            this.innerStream.EndWrite(asyncResult);
        }

        private bool EnsureStreamReady()
        {
            if (!this.streamReady)
            {
                throw new InvalidOperationException(Res.GetString("WebBadStreamState"));
            }
            return true;
        }

        public override void Flush()
        {
            this.EnsureStreamReady();
            this.hasWritten = true;
            this.innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.EnsureStreamReady();
            return this.innerStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            this.EnsureStreamReady();
            return this.innerStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.EnsureStreamReady();
            return this.innerStream.Seek(offset, origin);
        }

        internal void SetInnerStream(Stream stream)
        {
            this.innerStream = stream;
            this.hasWritten = false;
        }

        public override void SetLength(long value)
        {
            this.EnsureStreamReady();
            this.innerStream.SetLength(value);
        }

        internal void SetStreamReady()
        {
            this.streamReady = true;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.EnsureStreamReady();
            this.hasWritten = true;
            this.innerStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.EnsureStreamReady();
            this.hasWritten = true;
            this.innerStream.WriteByte(value);
        }

        public override bool CanRead
        {
            get
            {
                this.EnsureStreamReady();
                return this.innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                this.EnsureStreamReady();
                return this.innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                this.EnsureStreamReady();
                return this.innerStream.CanWrite;
            }
        }

        internal bool HasWritten
        {
            get
            {
                return this.hasWritten;
            }
        }

        public override long Length
        {
            get
            {
                this.EnsureStreamReady();
                return this.innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.EnsureStreamReady();
                return this.innerStream.Position;
            }
            set
            {
                this.EnsureStreamReady();
                this.hasWritten = true;
                this.innerStream.Position = value;
            }
        }
    }
}

