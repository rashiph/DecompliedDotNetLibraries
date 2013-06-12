namespace System.Net.Sockets
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    public class NetworkStream : Stream
    {
        private bool m_CleanedUp;
        private int m_CloseTimeout;
        private int m_CurrentReadTimeout;
        private int m_CurrentWriteTimeout;
        private bool m_OwnsSocket;
        private bool m_Readable;
        private System.Net.Sockets.Socket m_StreamSocket;
        private bool m_Writeable;

        internal NetworkStream()
        {
            this.m_CloseTimeout = -1;
            this.m_CurrentReadTimeout = -1;
            this.m_CurrentWriteTimeout = -1;
            this.m_OwnsSocket = true;
        }

        public NetworkStream(System.Net.Sockets.Socket socket)
        {
            this.m_CloseTimeout = -1;
            this.m_CurrentReadTimeout = -1;
            this.m_CurrentWriteTimeout = -1;
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            this.InitNetworkStream(socket, FileAccess.ReadWrite);
        }

        internal NetworkStream(NetworkStream networkStream, bool ownsSocket)
        {
            this.m_CloseTimeout = -1;
            this.m_CurrentReadTimeout = -1;
            this.m_CurrentWriteTimeout = -1;
            System.Net.Sockets.Socket socket = networkStream.Socket;
            if (socket == null)
            {
                throw new ArgumentNullException("networkStream");
            }
            this.InitNetworkStream(socket, FileAccess.ReadWrite);
            this.m_OwnsSocket = ownsSocket;
        }

        public NetworkStream(System.Net.Sockets.Socket socket, bool ownsSocket)
        {
            this.m_CloseTimeout = -1;
            this.m_CurrentReadTimeout = -1;
            this.m_CurrentWriteTimeout = -1;
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            this.InitNetworkStream(socket, FileAccess.ReadWrite);
            this.m_OwnsSocket = ownsSocket;
        }

        public NetworkStream(System.Net.Sockets.Socket socket, FileAccess access)
        {
            this.m_CloseTimeout = -1;
            this.m_CurrentReadTimeout = -1;
            this.m_CurrentWriteTimeout = -1;
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            this.InitNetworkStream(socket, access);
        }

        public NetworkStream(System.Net.Sockets.Socket socket, FileAccess access, bool ownsSocket)
        {
            this.m_CloseTimeout = -1;
            this.m_CurrentReadTimeout = -1;
            this.m_CurrentWriteTimeout = -1;
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            this.InitNetworkStream(socket, access);
            this.m_OwnsSocket = ownsSocket;
        }

        internal virtual IAsyncResult BeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                buffers = this.ConcatenateBuffersOnWin9x(buffers);
                result2 = streamSocket.BeginMultipleSend(buffers, SocketFlags.None, callback, state);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
            return result2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (!this.CanRead)
            {
                throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                result2 = streamSocket.BeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { exception.Message }), exception);
            }
            return result2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (!this.CanWrite)
            {
                throw new InvalidOperationException(SR.GetString("net_readonlystream"));
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                result2 = streamSocket.BeginSend(buffer, offset, size, SocketFlags.None, callback, state);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
            return result2;
        }

        public void Close(int timeout)
        {
            if (timeout < -1)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            this.m_CloseTimeout = timeout;
            this.Close();
        }

        private BufferOffsetSize[] ConcatenateBuffersOnWin9x(BufferOffsetSize[] buffers)
        {
            if (ComNetOS.IsWin9x && (buffers.Length > 0x10))
            {
                int num;
                BufferOffsetSize[] sizeArray = new BufferOffsetSize[0x10];
                for (num = 0; num < 0x10; num++)
                {
                    sizeArray[num] = buffers[num];
                }
                int size = 0;
                for (num = 15; num < buffers.Length; num++)
                {
                    size += buffers[num].Size;
                }
                if (size > 0)
                {
                    sizeArray[15] = new BufferOffsetSize(new byte[size], 0, size, false);
                    size = 0;
                    for (num = 15; num < buffers.Length; num++)
                    {
                        Buffer.BlockCopy(buffers[num].Buffer, buffers[num].Offset, sizeArray[15].Buffer, size, buffers[num].Size);
                        size += buffers[num].Size;
                    }
                }
                buffers = sizeArray;
            }
            return buffers;
        }

        internal void ConvertToNotSocketOwner()
        {
            this.m_OwnsSocket = false;
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            if ((!this.m_CleanedUp && disposing) && (this.m_StreamSocket != null))
            {
                this.m_Readable = false;
                this.m_Writeable = false;
                if (this.m_OwnsSocket)
                {
                    System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
                    if (streamSocket != null)
                    {
                        streamSocket.InternalShutdown(SocketShutdown.Both);
                        streamSocket.Close(this.m_CloseTimeout);
                    }
                }
            }
            this.m_CleanedUp = true;
            base.Dispose(disposing);
        }

        internal virtual void EndMultipleWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                streamSocket.EndMultipleSend(asyncResult);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int num2;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                num2 = streamSocket.EndReceive(asyncResult);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { exception.Message }), exception);
            }
            return num2;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                streamSocket.EndSend(asyncResult);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
        }

        ~NetworkStream()
        {
            this.Dispose(false);
        }

        public override void Flush()
        {
        }

        internal void InitNetworkStream(System.Net.Sockets.Socket socket, FileAccess Access)
        {
            if (!socket.Blocking)
            {
                throw new IOException(SR.GetString("net_sockets_blocking"));
            }
            if (!socket.Connected)
            {
                throw new IOException(SR.GetString("net_notconnected"));
            }
            if (socket.SocketType != SocketType.Stream)
            {
                throw new IOException(SR.GetString("net_notstream"));
            }
            this.m_StreamSocket = socket;
            switch (Access)
            {
                case FileAccess.Read:
                    this.m_Readable = true;
                    return;

                case FileAccess.Write:
                    this.m_Writeable = true;
                    return;
            }
            this.m_Readable = true;
            this.m_Writeable = true;
        }

        internal virtual void MultipleWrite(BufferOffsetSize[] buffers)
        {
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                buffers = this.ConcatenateBuffersOnWin9x(buffers);
                streamSocket.MultipleSend(buffers, SocketFlags.None);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
        }

        internal bool Poll(int microSeconds, SelectMode mode)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            return streamSocket.Poll(microSeconds, mode);
        }

        internal bool PollRead()
        {
            if (this.m_CleanedUp)
            {
                return false;
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                return false;
            }
            return streamSocket.Poll(0, SelectMode.SelectRead);
        }

        public override int Read([In, Out] byte[] buffer, int offset, int size)
        {
            int num2;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (!this.CanRead)
            {
                throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                num2 = streamSocket.Receive(buffer, offset, size, SocketFlags.None);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { exception.Message }), exception);
            }
            return num2;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("net_noseek"));
        }

        internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout, bool silent)
        {
            if (timeout < 0)
            {
                timeout = 0;
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket != null)
            {
                if (((mode == SocketShutdown.Send) || (mode == SocketShutdown.Both)) && (timeout != this.m_CurrentWriteTimeout))
                {
                    streamSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout, silent);
                    this.m_CurrentWriteTimeout = timeout;
                }
                if (((mode == SocketShutdown.Receive) || (mode == SocketShutdown.Both)) && (timeout != this.m_CurrentReadTimeout))
                {
                    streamSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout, silent);
                    this.m_CurrentReadTimeout = timeout;
                }
            }
        }

        internal virtual IAsyncResult UnsafeBeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                buffers = this.ConcatenateBuffersOnWin9x(buffers);
                result2 = streamSocket.UnsafeBeginMultipleSend(buffers, SocketFlags.None, callback, state);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
            return result2;
        }

        internal virtual IAsyncResult UnsafeBeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!this.CanRead)
            {
                throw new InvalidOperationException(SR.GetString("net_writeonlystream"));
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                result2 = streamSocket.UnsafeBeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_readfailure", new object[] { exception.Message }), exception);
            }
            return result2;
        }

        internal virtual IAsyncResult UnsafeBeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!this.CanWrite)
            {
                throw new InvalidOperationException(SR.GetString("net_readonlystream"));
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                result2 = streamSocket.UnsafeBeginSend(buffer, offset, size, SocketFlags.None, callback, state);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
            return result2;
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((size < 0) || (size > (buffer.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (!this.CanWrite)
            {
                throw new InvalidOperationException(SR.GetString("net_readonlystream"));
            }
            System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
            if (streamSocket == null)
            {
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { SR.GetString("net_io_connectionclosed") }));
            }
            try
            {
                streamSocket.Send(buffer, offset, size, SocketFlags.None);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new IOException(SR.GetString("net_io_writefailure", new object[] { exception.Message }), exception);
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.m_Readable;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.m_Writeable;
            }
        }

        internal bool Connected
        {
            get
            {
                System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
                return ((!this.m_CleanedUp && (streamSocket != null)) && streamSocket.Connected);
            }
        }

        public virtual bool DataAvailable
        {
            get
            {
                if (this.m_CleanedUp)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
                if (streamSocket == null)
                {
                    throw new IOException(SR.GetString("net_io_readfailure", new object[] { SR.GetString("net_io_connectionclosed") }));
                }
                return (streamSocket.Available != 0);
            }
        }

        internal System.Net.Sockets.Socket InternalSocket
        {
            get
            {
                System.Net.Sockets.Socket streamSocket = this.m_StreamSocket;
                if (this.m_CleanedUp || (streamSocket == null))
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                return streamSocket;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException(SR.GetString("net_noseek"));
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(SR.GetString("net_noseek"));
            }
            set
            {
                throw new NotSupportedException(SR.GetString("net_noseek"));
            }
        }

        protected bool Readable
        {
            get
            {
                return this.m_Readable;
            }
            set
            {
                this.m_Readable = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                int socketOption = (int) this.m_StreamSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
                if (socketOption == 0)
                {
                    return -1;
                }
                return socketOption;
            }
            set
            {
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                }
                this.SetSocketTimeoutOption(SocketShutdown.Receive, value, false);
            }
        }

        protected System.Net.Sockets.Socket Socket
        {
            get
            {
                return this.m_StreamSocket;
            }
        }

        protected bool Writeable
        {
            get
            {
                return this.m_Writeable;
            }
            set
            {
                this.m_Writeable = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                int socketOption = (int) this.m_StreamSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
                if (socketOption == 0)
                {
                    return -1;
                }
                return socketOption;
            }
            set
            {
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                }
                this.SetSocketTimeoutOption(SocketShutdown.Send, value, false);
            }
        }
    }
}

