namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    internal class RemoteConnection
    {
        private IPAddress[] _addressList;
        private CachedSocketList _cachedSocketList;
        private EndPoint _lkgIPEndPoint;
        private string _machineAndPort;
        private int _port;
        private SocketCache _socketCache;
        private Uri _uri;
        private static char[] colonSep = new char[] { ':' };
        private bool connectIPv6;

        internal RemoteConnection(SocketCache socketCache, string machineAndPort)
        {
            this._socketCache = socketCache;
            this._cachedSocketList = new CachedSocketList(socketCache.SocketTimeout, socketCache.CachePolicy);
            this._uri = new Uri("dummy://" + machineAndPort);
            this._port = this._uri.Port;
            this._machineAndPort = machineAndPort;
        }

        private SocketHandler CreateNewSocket()
        {
            this._addressList = Dns.GetHostAddresses(this._uri.Host);
            this.connectIPv6 = Socket.OSSupportsIPv6 && this.HasIPv6Address(this._addressList);
            if (this._addressList.Length == 1)
            {
                return this.CreateNewSocket(new IPEndPoint(this._addressList[0], this._port));
            }
            if (this._lkgIPEndPoint != null)
            {
                try
                {
                    return this.CreateNewSocket(this._lkgIPEndPoint);
                }
                catch (Exception)
                {
                    this._lkgIPEndPoint = null;
                }
            }
            if (this.connectIPv6)
            {
                try
                {
                    return this.CreateNewSocket(AddressFamily.InterNetworkV6);
                }
                catch (Exception)
                {
                }
            }
            return this.CreateNewSocket(AddressFamily.InterNetwork);
        }

        private SocketHandler CreateNewSocket(EndPoint ipEndPoint)
        {
            Socket socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.DisableNagleDelays(socket);
            socket.Connect(ipEndPoint);
            this._lkgIPEndPoint = socket.RemoteEndPoint;
            return this._socketCache.CreateSocketHandler(socket, this._machineAndPort);
        }

        private SocketHandler CreateNewSocket(AddressFamily family)
        {
            Socket socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
            this.DisableNagleDelays(socket);
            socket.Connect(this._addressList, this._port);
            this._lkgIPEndPoint = socket.RemoteEndPoint;
            return this._socketCache.CreateSocketHandler(socket, this._machineAndPort);
        }

        private void DisableNagleDelays(Socket socket)
        {
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, 1);
        }

        internal SocketHandler GetSocket()
        {
            SocketHandler socket = this._cachedSocketList.GetSocket();
            if (socket != null)
            {
                return socket;
            }
            return this.CreateNewSocket();
        }

        private bool HasIPv6Address(IPAddress[] addressList)
        {
            foreach (IPAddress address in addressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return true;
                }
            }
            return false;
        }

        internal void ReleaseSocket(SocketHandler socket)
        {
            socket.ReleaseControl();
            this._cachedSocketList.ReturnSocket(socket);
        }

        internal void TimeoutSockets(DateTime currentTime)
        {
            this._cachedSocketList.TimeoutSockets(currentTime, this._socketCache.SocketTimeout);
        }
    }
}

