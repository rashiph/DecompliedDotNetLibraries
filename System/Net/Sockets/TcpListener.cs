namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Security.Permissions;

    public class TcpListener
    {
        private bool m_Active;
        private bool m_ExclusiveAddressUse;
        private Socket m_ServerSocket;
        private IPEndPoint m_ServerSocketEP;

        [Obsolete("This method has been deprecated. Please use TcpListener(IPAddress localaddr, int port) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public TcpListener(int port)
        {
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            this.m_ServerSocketEP = new IPEndPoint(IPAddress.Any, port);
            this.m_ServerSocket = new Socket(this.m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public TcpListener(IPEndPoint localEP)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "TcpListener", localEP);
            }
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }
            this.m_ServerSocketEP = localEP;
            this.m_ServerSocket = new Socket(this.m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "TcpListener", (string) null);
            }
        }

        public TcpListener(IPAddress localaddr, int port)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "TcpListener", localaddr);
            }
            if (localaddr == null)
            {
                throw new ArgumentNullException("localaddr");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            this.m_ServerSocketEP = new IPEndPoint(localaddr, port);
            this.m_ServerSocket = new Socket(this.m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "TcpListener", (string) null);
            }
        }

        public Socket AcceptSocket()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "AcceptSocket", (string) null);
            }
            if (!this.m_Active)
            {
                throw new InvalidOperationException(SR.GetString("net_stopped"));
            }
            Socket retObject = this.m_ServerSocket.Accept();
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "AcceptSocket", retObject);
            }
            return retObject;
        }

        public TcpClient AcceptTcpClient()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "AcceptTcpClient", (string) null);
            }
            if (!this.m_Active)
            {
                throw new InvalidOperationException(SR.GetString("net_stopped"));
            }
            TcpClient retObject = new TcpClient(this.m_ServerSocket.Accept());
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "AcceptTcpClient", retObject);
            }
            return retObject;
        }

        public void AllowNatTraversal(bool allowed)
        {
            if (this.m_Active)
            {
                throw new InvalidOperationException(SR.GetString("net_tcplistener_mustbestopped"));
            }
            if (allowed)
            {
                this.m_ServerSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            }
            else
            {
                this.m_ServerSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "BeginAcceptSocket", (string) null);
            }
            if (!this.m_Active)
            {
                throw new InvalidOperationException(SR.GetString("net_stopped"));
            }
            IAsyncResult result = this.m_ServerSocket.BeginAccept(callback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "BeginAcceptSocket", (string) null);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginAcceptTcpClient(AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "BeginAcceptTcpClient", (string) null);
            }
            if (!this.m_Active)
            {
                throw new InvalidOperationException(SR.GetString("net_stopped"));
            }
            IAsyncResult result = this.m_ServerSocket.BeginAccept(callback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "BeginAcceptTcpClient", (string) null);
            }
            return result;
        }

        public Socket EndAcceptSocket(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "EndAcceptSocket", (string) null);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            Socket socket = (result == null) ? null : (result.AsyncObject as Socket);
            if (socket == null)
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            Socket retObject = socket.EndAccept(asyncResult);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "EndAcceptSocket", retObject);
            }
            return retObject;
        }

        public TcpClient EndAcceptTcpClient(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "EndAcceptTcpClient", (string) null);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            Socket socket = (result == null) ? null : (result.AsyncObject as Socket);
            if (socket == null)
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            Socket retObject = socket.EndAccept(asyncResult);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "EndAcceptTcpClient", retObject);
            }
            return new TcpClient(retObject);
        }

        public bool Pending()
        {
            if (!this.m_Active)
            {
                throw new InvalidOperationException(SR.GetString("net_stopped"));
            }
            return this.m_ServerSocket.Poll(0, SelectMode.SelectRead);
        }

        public void Start()
        {
            this.Start(0x7fffffff);
        }

        public void Start(int backlog)
        {
            if ((backlog > 0x7fffffff) || (backlog < 0))
            {
                throw new ArgumentOutOfRangeException("backlog");
            }
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Start", (string) null);
            }
            if (this.m_ServerSocket == null)
            {
                throw new InvalidOperationException(SR.GetString("net_InvalidSocketHandle"));
            }
            if (this.m_Active)
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Sockets, this, "Start", (string) null);
                }
            }
            else
            {
                this.m_ServerSocket.Bind(this.m_ServerSocketEP);
                try
                {
                    this.m_ServerSocket.Listen(backlog);
                }
                catch (SocketException)
                {
                    this.Stop();
                    throw;
                }
                this.m_Active = true;
                if (Logging.On)
                {
                    Logging.Exit(Logging.Sockets, this, "Start", (string) null);
                }
            }
        }

        public void Stop()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Stop", (string) null);
            }
            if (this.m_ServerSocket != null)
            {
                this.m_ServerSocket.Close();
                this.m_ServerSocket = null;
            }
            this.m_Active = false;
            this.m_ServerSocket = new Socket(this.m_ServerSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (this.m_ExclusiveAddressUse)
            {
                this.m_ServerSocket.ExclusiveAddressUse = true;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "Stop", (string) null);
            }
        }

        protected bool Active
        {
            get
            {
                return this.m_Active;
            }
        }

        public bool ExclusiveAddressUse
        {
            get
            {
                return this.m_ServerSocket.ExclusiveAddressUse;
            }
            set
            {
                if (this.m_Active)
                {
                    throw new InvalidOperationException(SR.GetString("net_tcplistener_mustbestopped"));
                }
                this.m_ServerSocket.ExclusiveAddressUse = value;
                this.m_ExclusiveAddressUse = value;
            }
        }

        public EndPoint LocalEndpoint
        {
            get
            {
                if (!this.m_Active)
                {
                    return this.m_ServerSocketEP;
                }
                return this.m_ServerSocket.LocalEndPoint;
            }
        }

        public Socket Server
        {
            get
            {
                return this.m_ServerSocket;
            }
        }
    }
}

