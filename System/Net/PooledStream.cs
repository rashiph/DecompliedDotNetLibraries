namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Security.Permissions;

    internal class PooledStream : Stream
    {
        private System.Net.Sockets.Socket m_AbortSocket;
        private System.Net.Sockets.Socket m_AbortSocket6;
        private GeneralAsyncDelegate m_AsyncCallback;
        private bool m_CheckLifetime;
        private bool m_ConnectionIsDoomed;
        private ConnectionPool m_ConnectionPool;
        private DateTime m_CreateTime;
        private bool m_Initalizing;
        private bool m_JustConnected;
        private TimeSpan m_Lifetime;
        private System.Net.Sockets.NetworkStream m_NetworkStream;
        private WeakReference m_Owner;
        private int m_PooledCount;
        private IPAddress m_ServerAddress;

        internal PooledStream(object owner)
        {
            this.m_Owner = new WeakReference(owner);
            this.m_PooledCount = -1;
            this.m_Initalizing = true;
            this.m_NetworkStream = new System.Net.Sockets.NetworkStream();
            this.m_CreateTime = DateTime.UtcNow;
        }

        internal PooledStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime)
        {
            this.m_ConnectionPool = connectionPool;
            this.m_Lifetime = lifetime;
            this.m_CheckLifetime = checkLifetime;
            this.m_Initalizing = true;
            this.m_NetworkStream = new System.Net.Sockets.NetworkStream();
            this.m_CreateTime = DateTime.UtcNow;
        }

        internal bool Activate(object owningObject, GeneralAsyncDelegate asyncCallback)
        {
            return this.Activate(owningObject, asyncCallback != null, -1, asyncCallback);
        }

        protected bool Activate(object owningObject, bool async, int timeout, GeneralAsyncDelegate asyncCallback)
        {
            bool flag;
            try
            {
                if (this.m_Initalizing)
                {
                    IPAddress address = null;
                    this.m_AsyncCallback = asyncCallback;
                    System.Net.Sockets.Socket socket = this.ServicePoint.GetConnection(this, owningObject, async, out address, ref this.m_AbortSocket, ref this.m_AbortSocket6, timeout);
                    if (socket != null)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_socket_connected", new object[] { socket.LocalEndPoint, socket.RemoteEndPoint }));
                        }
                        this.m_NetworkStream.InitNetworkStream(socket, FileAccess.ReadWrite);
                        this.m_ServerAddress = address;
                        this.m_Initalizing = false;
                        this.m_JustConnected = true;
                        this.m_AbortSocket = null;
                        this.m_AbortSocket6 = null;
                        return true;
                    }
                    return false;
                }
                if (async && (asyncCallback != null))
                {
                    asyncCallback(owningObject, this);
                }
                flag = true;
            }
            catch
            {
                this.m_Initalizing = false;
                throw;
            }
            return flag;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        internal IAsyncResult BeginMultipleWrite(BufferOffsetSize[] buffers, AsyncCallback callback, object state)
        {
            return this.m_NetworkStream.BeginMultipleWrite(buffers, callback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return this.m_NetworkStream.BeginRead(buffer, offset, size, callback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return this.m_NetworkStream.BeginWrite(buffer, offset, size, callback, state);
        }

        protected void CheckLifetime()
        {
            if (!this.m_ConnectionIsDoomed)
            {
                TimeSpan span = DateTime.UtcNow.Subtract(this.m_CreateTime);
                this.m_ConnectionIsDoomed = 0 < TimeSpan.Compare(this.m_Lifetime, span);
            }
        }

        public void Close(int timeout)
        {
            System.Net.Sockets.Socket abortSocket = this.m_AbortSocket;
            System.Net.Sockets.Socket socket2 = this.m_AbortSocket6;
            this.m_NetworkStream.Close(timeout);
            if (abortSocket != null)
            {
                abortSocket.Close(timeout);
            }
            if (socket2 != null)
            {
                socket2.Close(timeout);
            }
        }

        internal void CloseSocket()
        {
            System.Net.Sockets.Socket abortSocket = this.m_AbortSocket;
            System.Net.Sockets.Socket socket2 = this.m_AbortSocket6;
            this.m_NetworkStream.Close();
            if (abortSocket != null)
            {
                abortSocket.Close();
            }
            if (socket2 != null)
            {
                socket2.Close();
            }
        }

        internal virtual void ConnectionCallback(object owningObject, Exception e, System.Net.Sockets.Socket socket, IPAddress address)
        {
            object state = null;
            if (e != null)
            {
                this.m_Initalizing = false;
                state = e;
            }
            else
            {
                try
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_socket_connected", new object[] { socket.LocalEndPoint, socket.RemoteEndPoint }));
                    }
                    this.m_NetworkStream.InitNetworkStream(socket, FileAccess.ReadWrite);
                    state = this;
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    state = exception;
                }
                this.m_ServerAddress = address;
                this.m_Initalizing = false;
                this.m_JustConnected = true;
            }
            if (this.m_AsyncCallback != null)
            {
                this.m_AsyncCallback(owningObject, state);
            }
            this.m_AbortSocket = null;
            this.m_AbortSocket6 = null;
        }

        internal void Deactivate()
        {
            this.m_AsyncCallback = null;
            if (!this.m_ConnectionIsDoomed && this.m_CheckLifetime)
            {
                this.CheckLifetime();
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.m_Owner = null;
                    this.m_ConnectionIsDoomed = true;
                    this.CloseSocket();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal void EndMultipleWrite(IAsyncResult asyncResult)
        {
            this.m_NetworkStream.EndMultipleWrite(asyncResult);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.m_NetworkStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.m_NetworkStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this.m_NetworkStream.Flush();
        }

        internal void MultipleWrite(BufferOffsetSize[] buffers)
        {
            this.m_NetworkStream.MultipleWrite(buffers);
        }

        internal bool Poll(int microSeconds, SelectMode mode)
        {
            return this.m_NetworkStream.Poll(microSeconds, mode);
        }

        internal bool PollRead()
        {
            return this.m_NetworkStream.PollRead();
        }

        internal void PostPop(object newOwner)
        {
            lock (this)
            {
                if (this.m_Owner == null)
                {
                    this.m_Owner = new WeakReference(newOwner);
                }
                else
                {
                    if (this.m_Owner.Target != null)
                    {
                        throw new InternalException();
                    }
                    this.m_Owner.Target = newOwner;
                }
                this.m_PooledCount--;
                if (this.Pool != null)
                {
                    if (this.m_PooledCount != 0)
                    {
                        throw new InternalException();
                    }
                }
                else if (-1 != this.m_PooledCount)
                {
                    throw new InternalException();
                }
            }
        }

        internal void PrePush(object expectedOwner)
        {
            lock (this)
            {
                if (expectedOwner == null)
                {
                    if ((this.m_Owner != null) && (this.m_Owner.Target != null))
                    {
                        throw new InternalException();
                    }
                }
                else if ((this.m_Owner == null) || (this.m_Owner.Target != expectedOwner))
                {
                    throw new InternalException();
                }
                this.m_PooledCount++;
                if (1 != this.m_PooledCount)
                {
                    throw new InternalException();
                }
                if (this.m_Owner != null)
                {
                    this.m_Owner.Target = null;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            return this.m_NetworkStream.Read(buffer, offset, size);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.m_NetworkStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.m_NetworkStream.SetLength(value);
        }

        internal void SetSocketTimeoutOption(SocketShutdown mode, int timeout, bool silent)
        {
            this.m_NetworkStream.SetSocketTimeoutOption(mode, timeout, silent);
        }

        internal virtual IAsyncResult UnsafeBeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return this.m_NetworkStream.UnsafeBeginRead(buffer, offset, size, callback, state);
        }

        internal virtual IAsyncResult UnsafeBeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            return this.m_NetworkStream.UnsafeBeginWrite(buffer, offset, size, callback, state);
        }

        internal void UpdateLifetime()
        {
            TimeSpan maxValue;
            int connectionLeaseTimeout = this.ServicePoint.ConnectionLeaseTimeout;
            if (connectionLeaseTimeout == -1)
            {
                maxValue = TimeSpan.MaxValue;
                this.m_CheckLifetime = false;
            }
            else
            {
                maxValue = new TimeSpan(0, 0, 0, 0, connectionLeaseTimeout);
                this.m_CheckLifetime = true;
            }
            if (maxValue != this.m_Lifetime)
            {
                this.m_Lifetime = maxValue;
            }
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            this.m_NetworkStream.Write(buffer, offset, size);
        }

        internal bool CanBePooled
        {
            get
            {
                if (this.m_Initalizing)
                {
                    return true;
                }
                if (!this.m_NetworkStream.Connected)
                {
                    return false;
                }
                WeakReference owner = this.m_Owner;
                return (!this.m_ConnectionIsDoomed && ((owner == null) || !owner.IsAlive));
            }
            set
            {
                this.m_ConnectionIsDoomed |= !value;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.m_NetworkStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.m_NetworkStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this.m_NetworkStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.m_NetworkStream.CanWrite;
            }
        }

        internal bool IsEmancipated
        {
            get
            {
                WeakReference owner = this.m_Owner;
                return ((0 >= this.m_PooledCount) && ((owner == null) || !owner.IsAlive));
            }
        }

        internal bool IsInitalizing
        {
            get
            {
                return this.m_Initalizing;
            }
        }

        internal bool JustConnected
        {
            get
            {
                if (this.m_JustConnected)
                {
                    this.m_JustConnected = false;
                    return true;
                }
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return this.m_NetworkStream.Length;
            }
        }

        internal System.Net.Sockets.NetworkStream NetworkStream
        {
            get
            {
                return this.m_NetworkStream;
            }
            set
            {
                this.m_Initalizing = false;
                this.m_NetworkStream = value;
            }
        }

        internal object Owner
        {
            get
            {
                WeakReference owner = this.m_Owner;
                if ((owner != null) && owner.IsAlive)
                {
                    return owner.Target;
                }
                return null;
            }
            set
            {
                lock (this)
                {
                    if (this.m_Owner != null)
                    {
                        this.m_Owner.Target = value;
                    }
                }
            }
        }

        internal ConnectionPool Pool
        {
            get
            {
                return this.m_ConnectionPool;
            }
        }

        public override long Position
        {
            get
            {
                return this.m_NetworkStream.Position;
            }
            set
            {
                this.m_NetworkStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.m_NetworkStream.ReadTimeout;
            }
            set
            {
                this.m_NetworkStream.ReadTimeout = value;
            }
        }

        internal IPAddress ServerAddress
        {
            get
            {
                return this.m_ServerAddress;
            }
        }

        internal virtual System.Net.ServicePoint ServicePoint
        {
            get
            {
                return this.Pool.ServicePoint;
            }
        }

        protected System.Net.Sockets.Socket Socket
        {
            get
            {
                return this.m_NetworkStream.InternalSocket;
            }
        }

        protected bool UsingSecureStream
        {
            get
            {
                return (this.m_NetworkStream is TlsStream);
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.m_NetworkStream.WriteTimeout;
            }
            set
            {
                this.m_NetworkStream.WriteTimeout = value;
            }
        }
    }
}

