namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Net.Sockets;
    using System.Threading;

    internal class SocketCache
    {
        private static Hashtable _connections = new Hashtable();
        private SocketHandlerFactory _handlerFactory;
        private int _receiveTimeout;
        private static RegisteredWaitHandle _registeredWaitHandle;
        private SocketCachePolicy _socketCachePolicy;
        private TimeSpan _socketTimeout;
        private static WaitOrTimerCallback _socketTimeoutDelegate;
        private static TimeSpan _socketTimeoutPollTime = TimeSpan.FromSeconds(10.0);
        private static AutoResetEvent _socketTimeoutWaitHandle;

        static SocketCache()
        {
            InitializeSocketTimeoutHandler();
        }

        internal SocketCache(SocketHandlerFactory handlerFactory, SocketCachePolicy socketCachePolicy, TimeSpan socketTimeout)
        {
            this._handlerFactory = handlerFactory;
            this._socketCachePolicy = socketCachePolicy;
            this._socketTimeout = socketTimeout;
        }

        internal SocketHandler CreateSocketHandler(Socket socket, string machineAndPort)
        {
            socket.ReceiveTimeout = this._receiveTimeout;
            return this._handlerFactory(socket, this, machineAndPort);
        }

        public SocketHandler GetSocket(string machinePortAndSid, bool openNew)
        {
            RemoteConnection connection = (RemoteConnection) _connections[machinePortAndSid];
            if (openNew || (connection == null))
            {
                connection = new RemoteConnection(this, machinePortAndSid);
                lock (_connections)
                {
                    _connections[machinePortAndSid] = connection;
                }
            }
            return connection.GetSocket();
        }

        private static void InitializeSocketTimeoutHandler()
        {
            _socketTimeoutDelegate = new WaitOrTimerCallback(SocketCache.TimeoutSockets);
            _socketTimeoutWaitHandle = new AutoResetEvent(false);
            _registeredWaitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(_socketTimeoutWaitHandle, _socketTimeoutDelegate, "TcpChannelSocketTimeout", _socketTimeoutPollTime, true);
        }

        public void ReleaseSocket(string machinePortAndSid, SocketHandler socket)
        {
            RemoteConnection connection = (RemoteConnection) _connections[machinePortAndSid];
            if (connection != null)
            {
                connection.ReleaseSocket(socket);
            }
            else
            {
                socket.Close();
            }
        }

        private static void TimeoutSockets(object state, bool wasSignalled)
        {
            DateTime utcNow = DateTime.UtcNow;
            lock (_connections)
            {
                foreach (DictionaryEntry entry in _connections)
                {
                    ((RemoteConnection) entry.Value).TimeoutSockets(utcNow);
                }
            }
            _registeredWaitHandle.Unregister(null);
            _registeredWaitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(_socketTimeoutWaitHandle, _socketTimeoutDelegate, "TcpChannelSocketTimeout", _socketTimeoutPollTime, true);
        }

        internal SocketCachePolicy CachePolicy
        {
            get
            {
                return this._socketCachePolicy;
            }
            set
            {
                this._socketCachePolicy = value;
            }
        }

        internal int ReceiveTimeout
        {
            get
            {
                return this._receiveTimeout;
            }
            set
            {
                this._receiveTimeout = value;
            }
        }

        internal TimeSpan SocketTimeout
        {
            get
            {
                return this._socketTimeout;
            }
            set
            {
                this._socketTimeout = value;
            }
        }
    }
}

