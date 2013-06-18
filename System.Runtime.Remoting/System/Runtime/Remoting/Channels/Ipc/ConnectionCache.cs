namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Collections;
    using System.Security.Principal;
    using System.Threading;

    internal class ConnectionCache
    {
        private static Hashtable _connections = new Hashtable();
        private static TimeSpan _portLifetime = TimeSpan.FromSeconds(10.0);
        private static RegisteredWaitHandle _registeredWaitHandle;
        private static WaitOrTimerCallback _socketTimeoutDelegate;
        private static TimeSpan _socketTimeoutPollTime = TimeSpan.FromSeconds(10.0);
        private static AutoResetEvent _socketTimeoutWaitHandle;

        static ConnectionCache()
        {
            InitializeConnectionTimeoutHandler();
        }

        public IpcPort GetConnection(string portName, bool secure, TokenImpersonationLevel level, int timeout)
        {
            PortConnection connection = null;
            lock (_connections)
            {
                bool flag = true;
                if (secure)
                {
                    try
                    {
                        WindowsIdentity current = WindowsIdentity.GetCurrent(true);
                        if (current != null)
                        {
                            flag = false;
                            current.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    connection = (PortConnection) _connections[portName];
                }
                if ((connection == null) || connection.Port.IsDisposed)
                {
                    connection = new PortConnection(IpcPort.Connect(portName, secure, level, timeout)) {
                        Port = { Cacheable = flag }
                    };
                }
                else
                {
                    _connections.Remove(portName);
                }
            }
            return connection.Port;
        }

        private static void InitializeConnectionTimeoutHandler()
        {
            _socketTimeoutDelegate = new WaitOrTimerCallback(ConnectionCache.TimeoutConnections);
            _socketTimeoutWaitHandle = new AutoResetEvent(false);
            _registeredWaitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(_socketTimeoutWaitHandle, _socketTimeoutDelegate, "IpcConnectionTimeout", _socketTimeoutPollTime, true);
        }

        public void ReleaseConnection(IpcPort port)
        {
            string name = port.Name;
            PortConnection connection = (PortConnection) _connections[name];
            if (port.Cacheable && ((connection == null) || connection.Port.IsDisposed))
            {
                lock (_connections)
                {
                    _connections[name] = new PortConnection(port);
                    return;
                }
            }
            port.Dispose();
        }

        private static void TimeoutConnections(object state, bool wasSignalled)
        {
            DateTime utcNow = DateTime.UtcNow;
            lock (_connections)
            {
                foreach (DictionaryEntry entry in _connections)
                {
                    PortConnection connection = (PortConnection) entry.Value;
                    if ((DateTime.Now - connection.LastUsed) > _portLifetime)
                    {
                        connection.Port.Dispose();
                    }
                }
            }
            _registeredWaitHandle.Unregister(null);
            _registeredWaitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(_socketTimeoutWaitHandle, _socketTimeoutDelegate, "IpcConnectionTimeout", _socketTimeoutPollTime, true);
        }
    }
}

