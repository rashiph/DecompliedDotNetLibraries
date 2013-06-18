namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.IO;
    using System.Runtime.Remoting;

    internal sealed class PipeStream : Stream
    {
        private IpcPort _port;
        private int _timeout;

        public PipeStream(IpcPort port)
        {
            if (port == null)
            {
                throw new ArgumentNullException("port");
            }
            this._port = port;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return this._port.BeginRead(buffer, offset, size, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this._port.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this._port.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (this._timeout <= 0)
            {
                return this._port.Read(buffer, offset, size);
            }
            IAsyncResult iar = this._port.BeginRead(buffer, offset, size, null, null);
            if ((this._timeout > 0) && !iar.IsCompleted)
            {
                iar.AsyncWaitHandle.WaitOne(this._timeout, false);
                if (!iar.IsCompleted)
                {
                    throw new RemotingTimeoutException();
                }
            }
            return this._port.EndRead(iar);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._port.Write(buffer, offset, count);
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
                return true;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

