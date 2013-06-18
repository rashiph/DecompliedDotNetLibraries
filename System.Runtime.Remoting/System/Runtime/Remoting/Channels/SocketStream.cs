namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Remoting;

    internal sealed class SocketStream : Stream
    {
        private Socket _socket;
        private int _timeout;
        private const int maxSocketRead = 0x400000;
        private const int maxSocketWrite = 0x10000;

        public SocketStream(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            this._socket = socket;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return this._socket.BeginReceive(buffer, offset, Math.Min(size, 0x400000), SocketFlags.None, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return this._socket.BeginSend(buffer, offset, size, SocketFlags.None, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this._socket.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this._socket.EndReceive(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this._socket.EndSend(asyncResult);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (this._timeout <= 0)
            {
                return this._socket.Receive(buffer, offset, Math.Min(size, 0x400000), SocketFlags.None);
            }
            IAsyncResult asyncResult = this._socket.BeginReceive(buffer, offset, Math.Min(size, 0x400000), SocketFlags.None, null, null);
            if ((this._timeout > 0) && !asyncResult.IsCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne(this._timeout, false);
                if (!asyncResult.IsCompleted)
                {
                    throw new RemotingTimeoutException();
                }
            }
            return this._socket.EndReceive(asyncResult);
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
            int num = count;
            while (num > 0)
            {
                count = Math.Min(num, 0x10000);
                this._socket.Send(buffer, offset, count, SocketFlags.None);
                num -= count;
                offset += count;
            }
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

