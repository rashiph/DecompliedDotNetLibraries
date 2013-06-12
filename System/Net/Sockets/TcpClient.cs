namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Security.Permissions;
    using System.Threading;

    public class TcpClient : IDisposable
    {
        private bool m_Active;
        private bool m_CleanedUp;
        private Socket m_ClientSocket;
        private NetworkStream m_DataStream;
        private AddressFamily m_Family;

        public TcpClient() : this(AddressFamily.InterNetwork)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "TcpClient", (string) null);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "TcpClient", (string) null);
            }
        }

        public TcpClient(IPEndPoint localEP)
        {
            this.m_Family = AddressFamily.InterNetwork;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "TcpClient", localEP);
            }
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }
            this.m_Family = localEP.AddressFamily;
            this.initialize();
            this.Client.Bind(localEP);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "TcpClient", "");
            }
        }

        public TcpClient(AddressFamily family)
        {
            this.m_Family = AddressFamily.InterNetwork;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "TcpClient", family);
            }
            if ((family != AddressFamily.InterNetwork) && (family != AddressFamily.InterNetworkV6))
            {
                throw new ArgumentException(SR.GetString("net_protocol_invalid_family", new object[] { "TCP" }), "family");
            }
            this.m_Family = family;
            this.initialize();
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "TcpClient", (string) null);
            }
        }

        internal TcpClient(Socket acceptedSocket)
        {
            this.m_Family = AddressFamily.InterNetwork;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "TcpClient", acceptedSocket);
            }
            this.Client = acceptedSocket;
            this.m_Active = true;
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "TcpClient", (string) null);
            }
        }

        public TcpClient(string hostname, int port)
        {
            this.m_Family = AddressFamily.InterNetwork;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "TcpClient", hostname);
            }
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            try
            {
                this.Connect(hostname, port);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (this.m_ClientSocket != null)
                {
                    this.m_ClientSocket.Close();
                }
                throw exception;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "TcpClient", (string) null);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnect", address);
            }
            IAsyncResult result = this.Client.BeginConnect(address, port, requestCallback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnect", (string) null);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnect", host);
            }
            IAsyncResult result = this.Client.BeginConnect(host, port, requestCallback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnect", (string) null);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "BeginConnect", addresses);
            }
            IAsyncResult result = this.Client.BeginConnect(addresses, port, requestCallback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "BeginConnect", (string) null);
            }
            return result;
        }

        public void Close()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Close", "");
            }
            ((IDisposable) this).Dispose();
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "Close", "");
            }
        }

        public void Connect(IPEndPoint remoteEP)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", remoteEP);
            }
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            this.Client.Connect(remoteEP);
            this.m_Active = true;
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "Connect", (string) null);
            }
        }

        public void Connect(IPAddress address, int port)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", address);
            }
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            IPEndPoint remoteEP = new IPEndPoint(address, port);
            this.Connect(remoteEP);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "Connect", (string) null);
            }
        }

        public void Connect(string hostname, int port)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", hostname);
            }
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if (this.m_Active)
            {
                throw new SocketException(SocketError.IsConnected);
            }
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
            Exception exception = null;
            Socket socket = null;
            Socket socket2 = null;
            try
            {
                if (this.m_ClientSocket == null)
                {
                    if (Socket.OSSupportsIPv4)
                    {
                        socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                    if (Socket.OSSupportsIPv6)
                    {
                        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    }
                }
                foreach (IPAddress address in hostAddresses)
                {
                    try
                    {
                        if (this.m_ClientSocket == null)
                        {
                            if ((address.AddressFamily == AddressFamily.InterNetwork) && (socket2 != null))
                            {
                                socket2.Connect(address, port);
                                this.m_ClientSocket = socket2;
                                if (socket != null)
                                {
                                    socket.Close();
                                }
                            }
                            else if (socket != null)
                            {
                                socket.Connect(address, port);
                                this.m_ClientSocket = socket;
                                if (socket2 != null)
                                {
                                    socket2.Close();
                                }
                            }
                            this.m_Family = address.AddressFamily;
                            this.m_Active = true;
                            goto Label_01BF;
                        }
                        if (address.AddressFamily == this.m_Family)
                        {
                            this.Connect(new IPEndPoint(address, port));
                            this.m_Active = true;
                            goto Label_01BF;
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                }
            }
            catch (Exception exception3)
            {
                if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                {
                    throw;
                }
                exception = exception3;
            }
            finally
            {
                if (!this.m_Active)
                {
                    if (socket != null)
                    {
                        socket.Close();
                    }
                    if (socket2 != null)
                    {
                        socket2.Close();
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                    throw new SocketException(SocketError.NotConnected);
                }
            }
        Label_01BF:
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "Connect", (string) null);
            }
        }

        public void Connect(IPAddress[] ipAddresses, int port)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Connect", ipAddresses);
            }
            this.Client.Connect(ipAddresses, port);
            this.m_Active = true;
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "Connect", (string) null);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "Dispose", "");
            }
            if (this.m_CleanedUp)
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Sockets, this, "Dispose", "");
                }
            }
            else
            {
                if (disposing)
                {
                    IDisposable dataStream = this.m_DataStream;
                    if (dataStream != null)
                    {
                        dataStream.Dispose();
                    }
                    else
                    {
                        Socket client = this.Client;
                        if (client != null)
                        {
                            try
                            {
                                client.InternalShutdown(SocketShutdown.Both);
                            }
                            finally
                            {
                                client.Close();
                                this.Client = null;
                            }
                        }
                    }
                    GC.SuppressFinalize(this);
                }
                this.m_CleanedUp = true;
                if (Logging.On)
                {
                    Logging.Exit(Logging.Sockets, this, "Dispose", "");
                }
            }
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "EndConnect", asyncResult);
            }
            this.Client.EndConnect(asyncResult);
            this.m_Active = true;
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "EndConnect", (string) null);
            }
        }

        ~TcpClient()
        {
            this.Dispose(false);
        }

        public NetworkStream GetStream()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, this, "GetStream", "");
            }
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!this.Client.Connected)
            {
                throw new InvalidOperationException(SR.GetString("net_notconnected"));
            }
            if (this.m_DataStream == null)
            {
                this.m_DataStream = new NetworkStream(this.Client, true);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, this, "GetStream", this.m_DataStream);
            }
            return this.m_DataStream;
        }

        private void initialize()
        {
            this.Client = new Socket(this.m_Family, SocketType.Stream, ProtocolType.Tcp);
            this.m_Active = false;
        }

        private int numericOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            return (int) this.Client.GetSocketOption(optionLevel, optionName);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        protected bool Active
        {
            get
            {
                return this.m_Active;
            }
            set
            {
                this.m_Active = value;
            }
        }

        public int Available
        {
            get
            {
                return this.m_ClientSocket.Available;
            }
        }

        public Socket Client
        {
            get
            {
                return this.m_ClientSocket;
            }
            set
            {
                this.m_ClientSocket = value;
            }
        }

        public bool Connected
        {
            get
            {
                return this.m_ClientSocket.Connected;
            }
        }

        public bool ExclusiveAddressUse
        {
            get
            {
                return this.m_ClientSocket.ExclusiveAddressUse;
            }
            set
            {
                this.m_ClientSocket.ExclusiveAddressUse = value;
            }
        }

        public LingerOption LingerState
        {
            get
            {
                return (LingerOption) this.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
            }
            set
            {
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
            }
        }

        public bool NoDelay
        {
            get
            {
                if (this.numericOption(SocketOptionLevel.Tcp, SocketOptionName.Debug) == 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                this.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, value ? 1 : 0);
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                return this.numericOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
            }
            set
            {
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                return this.numericOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
            }
            set
            {
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
            }
        }

        public int SendBufferSize
        {
            get
            {
                return this.numericOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
            }
            set
            {
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
            }
        }

        public int SendTimeout
        {
            get
            {
                return this.numericOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
            }
            set
            {
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
            }
        }
    }
}

