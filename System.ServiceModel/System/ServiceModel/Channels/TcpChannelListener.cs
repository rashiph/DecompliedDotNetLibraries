namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal abstract class TcpChannelListener : ConnectionOrientedTransportChannelListener
    {
        private ExtendedProtectionPolicy extendedProtectionPolicy;
        private Socket ipv4ListenSocket;
        private Socket ipv6ListenSocket;
        private int listenBacklog;
        private bool portSharingEnabled;
        private static Random randomPortGenerator = new Random(AppDomain.CurrentDomain.GetHashCode() | Environment.TickCount);
        private bool teredoEnabled;
        private static UriPrefixTable<ITransportManagerRegistration> transportManagerTable = new UriPrefixTable<ITransportManagerRegistration>(true);

        protected TcpChannelListener(TcpTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
            this.listenBacklog = bindingElement.ListenBacklog;
            this.portSharingEnabled = bindingElement.PortSharingEnabled;
            this.teredoEnabled = bindingElement.TeredoEnabled;
            this.extendedProtectionPolicy = bindingElement.ExtendedProtectionPolicy;
            base.SetIdleTimeout(bindingElement.ConnectionPoolSettings.IdleTimeout);
            base.SetMaxPooledConnections(bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);
            if (!bindingElement.PortSharingEnabled && (context.ListenUriMode == ListenUriMode.Unique))
            {
                this.SetupUniquePort(context);
            }
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration()
        {
            Uri baseUri = base.BaseUri;
            if (!this.PortSharingEnabled)
            {
                UriBuilder uriBuilder = new UriBuilder(baseUri.Scheme, baseUri.Host, baseUri.Port);
                FixIpv6Hostname(uriBuilder, baseUri);
                baseUri = uriBuilder.Uri;
            }
            return this.CreateTransportManagerRegistration(baseUri);
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            if (this.PortSharingEnabled)
            {
                return new SharedTcpTransportManager(listenUri, this);
            }
            return new ExclusiveTcpTransportManagerRegistration(listenUri, this);
        }

        internal static void FixIpv6Hostname(UriBuilder uriBuilder, Uri originalUri)
        {
            if (originalUri.HostNameType == UriHostNameType.IPv6)
            {
                string dnsSafeHost = originalUri.DnsSafeHost;
                uriBuilder.Host = "[" + dnsSafeHost + "]";
            }
        }

        internal Socket GetListenSocket(UriHostNameType ipHostNameType)
        {
            if (ipHostNameType == UriHostNameType.IPv4)
            {
                Socket socket = this.ipv4ListenSocket;
                this.ipv4ListenSocket = null;
                return socket;
            }
            Socket socket2 = this.ipv6ListenSocket;
            this.ipv6ListenSocket = null;
            return socket2;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(ExtendedProtectionPolicy))
            {
                return (T) this.extendedProtectionPolicy;
            }
            return base.GetProperty<T>();
        }

        private Socket ListenAndBind(IPEndPoint localEndpoint)
        {
            Socket socket = new Socket(localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(localEndpoint);
            }
            catch (SocketException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(SocketConnectionListener.ConvertListenException(exception, localEndpoint));
            }
            return socket;
        }

        private void SetupUniquePort(BindingContext context)
        {
            IPAddress any = IPAddress.Any;
            IPAddress address = IPAddress.IPv6Any;
            bool flag = Socket.OSSupportsIPv4;
            bool flag2 = Socket.OSSupportsIPv6;
            if (this.Uri.HostNameType == UriHostNameType.IPv6)
            {
                flag = false;
                address = IPAddress.Parse(this.Uri.DnsSafeHost);
            }
            else if (this.Uri.HostNameType == UriHostNameType.IPv4)
            {
                flag2 = false;
                any = IPAddress.Parse(this.Uri.DnsSafeHost);
            }
            if (!flag && !flag2)
            {
                if (this.Uri.HostNameType == UriHostNameType.IPv6)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("context", System.ServiceModel.SR.GetString("TcpV6AddressInvalid", new object[] { this.Uri }));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("context", System.ServiceModel.SR.GetString("TcpV4AddressInvalid", new object[] { this.Uri }));
            }
            UriBuilder builder = new UriBuilder(context.ListenUriBaseAddress);
            int port = -1;
            if (!flag2)
            {
                this.ipv4ListenSocket = this.ListenAndBind(new IPEndPoint(any, 0));
                port = ((IPEndPoint) this.ipv4ListenSocket.LocalEndPoint).Port;
            }
            else if (!flag)
            {
                this.ipv6ListenSocket = this.ListenAndBind(new IPEndPoint(address, 0));
                port = ((IPEndPoint) this.ipv6ListenSocket.LocalEndPoint).Port;
            }
            else
            {
                int[] numArray = new int[10];
                lock (randomPortGenerator)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        numArray[j] = randomPortGenerator.Next(0xc000, 0xffff);
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    port = numArray[i];
                    try
                    {
                        this.ipv4ListenSocket = this.ListenAndBind(new IPEndPoint(any, port));
                        this.ipv6ListenSocket = this.ListenAndBind(new IPEndPoint(address, port));
                        break;
                    }
                    catch (AddressAlreadyInUseException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                        if (this.ipv4ListenSocket != null)
                        {
                            this.ipv4ListenSocket.Close();
                            this.ipv4ListenSocket = null;
                        }
                        this.ipv6ListenSocket = null;
                    }
                }
                if (this.ipv4ListenSocket == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAlreadyInUseException(System.ServiceModel.SR.GetString("UniquePortNotAvailable")));
                }
            }
            builder.Port = port;
            base.SetUri(builder.Uri, context.ListenUriRelativeAddress);
        }

        public int ListenBacklog
        {
            get
            {
                return this.listenBacklog;
            }
        }

        public bool PortSharingEnabled
        {
            get
            {
                return this.portSharingEnabled;
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeNetTcp;
            }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        public bool TeredoEnabled
        {
            get
            {
                return this.teredoEnabled;
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }
    }
}

