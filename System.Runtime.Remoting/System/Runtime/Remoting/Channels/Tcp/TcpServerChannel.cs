namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    public class TcpServerChannel : IChannelReceiver, IChannel, ISecurableChannel
    {
        private AsyncCallback _acceptSocketCallback;
        private IAuthorizeRemotingConnection _authorizeRemotingConnection;
        private bool _bExclusiveAddressUse;
        private IPAddress _bindToAddr;
        private bool _bListening;
        private bool _bSuppressChannelData;
        private bool _bUseIpAddress;
        private ChannelDataStore _channelData;
        private string _channelName;
        private int _channelPriority;
        private string _forcedMachineName;
        private bool _impersonate;
        private string _machineName;
        private int _port;
        private ProtectionLevel _protectionLevel;
        private bool _secure;
        private IServerChannelSinkProvider _sinkProvider;
        private ExclusiveTcpListener _tcpListener;
        private TcpServerTransportSink _transportSink;
        private bool authSet;

        public TcpServerChannel(int port)
        {
            this._channelPriority = 1;
            this._channelName = "tcp";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._protectionLevel = ProtectionLevel.EncryptAndSign;
            this._bExclusiveAddressUse = true;
            this._port = port;
            this.SetupMachineName();
            this.SetupChannel();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TcpServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider) : this(properties, sinkProvider, null)
        {
        }

        public TcpServerChannel(string name, int port)
        {
            this._channelPriority = 1;
            this._channelName = "tcp";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._protectionLevel = ProtectionLevel.EncryptAndSign;
            this._bExclusiveAddressUse = true;
            this._channelName = name;
            this._port = port;
            this.SetupMachineName();
            this.SetupChannel();
        }

        public TcpServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider, IAuthorizeRemotingConnection authorizeCallback)
        {
            this._channelPriority = 1;
            this._channelName = "tcp";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._protectionLevel = ProtectionLevel.EncryptAndSign;
            this._bExclusiveAddressUse = true;
            this._authorizeRemotingConnection = authorizeCallback;
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    switch (((string) entry.Key))
                    {
                        case "name":
                        {
                            this._channelName = (string) entry.Value;
                            continue;
                        }
                        case "bindTo":
                        {
                            this._bindToAddr = IPAddress.Parse((string) entry.Value);
                            continue;
                        }
                        case "port":
                        {
                            this._port = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            continue;
                        }
                        case "priority":
                        {
                            this._channelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            continue;
                        }
                        case "secure":
                        {
                            this._secure = Convert.ToBoolean(entry.Value);
                            continue;
                        }
                        case "impersonate":
                        {
                            this._impersonate = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            this.authSet = true;
                            continue;
                        }
                        case "protectionLevel":
                        {
                            this._protectionLevel = (entry.Value is ProtectionLevel) ? ((ProtectionLevel) entry.Value) : ((ProtectionLevel) System.Enum.Parse(typeof(ProtectionLevel), (string) entry.Value, true));
                            this.authSet = true;
                            continue;
                        }
                        case "machineName":
                        {
                            this._forcedMachineName = (string) entry.Value;
                            continue;
                        }
                        case "rejectRemoteRequests":
                        {
                            if (Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture))
                            {
                                if (!Socket.OSSupportsIPv4)
                                {
                                    break;
                                }
                                this._bindToAddr = IPAddress.Loopback;
                            }
                            continue;
                        }
                        case "suppressChannelData":
                        {
                            this._bSuppressChannelData = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            continue;
                        }
                        case "useIpAddress":
                        {
                            this._bUseIpAddress = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            continue;
                        }
                        case "exclusiveAddressUse":
                        {
                            this._bExclusiveAddressUse = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            continue;
                        }
                        case "authorizationModule":
                        {
                            this._authorizeRemotingConnection = (IAuthorizeRemotingConnection) Activator.CreateInstance(Type.GetType((string) entry.Value, true));
                            continue;
                        }
                        default:
                        {
                            continue;
                        }
                    }
                    this._bindToAddr = IPAddress.IPv6Loopback;
                }
            }
            this._sinkProvider = sinkProvider;
            this.SetupMachineName();
            this.SetupChannel();
        }

        public TcpServerChannel(string name, int port, IServerChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "tcp";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._protectionLevel = ProtectionLevel.EncryptAndSign;
            this._bExclusiveAddressUse = true;
            this._channelName = name;
            this._port = port;
            this._sinkProvider = sinkProvider;
            this.SetupMachineName();
            this.SetupChannel();
        }

        private void AcceptSocketCallback(IAsyncResult ar)
        {
            Socket socket = null;
            TcpServerSocketHandler streamManager = null;
            bool flag = true;
            try
            {
                if (this._tcpListener.IsListening)
                {
                    this._tcpListener.BeginAcceptSocket(this._acceptSocketCallback, null);
                }
                socket = this._tcpListener.EndAcceptSocket(ar);
                if (socket == null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Socket_Accept"), new object[] { Marshal.GetLastWin32Error().ToString(CultureInfo.CurrentCulture) }));
                }
                if ((this._authorizeRemotingConnection != null) && !this._authorizeRemotingConnection.IsConnectingEndPointAuthorized(socket.RemoteEndPoint))
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_ServerAuthorizationEndpointFailed"));
                }
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, 1);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                LingerOption optionValue = new LingerOption(true, 3);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
                Stream stream = new SocketStream(socket);
                streamManager = new TcpServerSocketHandler(socket, CoreChannel.RequestQueue, stream);
                WindowsIdentity identity = null;
                flag = false;
                if (this._secure)
                {
                    identity = this.Authenticate(ref stream, streamManager);
                    streamManager = new TcpServerSocketHandler(socket, CoreChannel.RequestQueue, stream);
                    if ((this._authorizeRemotingConnection != null) && !this._authorizeRemotingConnection.IsConnectingIdentityAuthorized(identity))
                    {
                        throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_ServerAuthorizationIdentityFailed"));
                    }
                }
                streamManager.ImpersonationIdentity = identity;
                streamManager.DataArrivedCallback = new WaitCallback(this._transportSink.ServiceRequest);
                streamManager.BeginReadMessage();
            }
            catch (Exception exception)
            {
                try
                {
                    if (streamManager != null)
                    {
                        streamManager.SendErrorResponse(exception, false);
                    }
                    if (socket != null)
                    {
                        if (flag)
                        {
                            socket.Close(0);
                        }
                        else
                        {
                            socket.Close();
                        }
                    }
                }
                catch (Exception)
                {
                }
                if (this._bListening)
                {
                    SocketException exception3 = exception as SocketException;
                }
            }
        }

        private void AcceptSocketCallbackAsync(object state)
        {
            this.AcceptSocketCallback((IAsyncResult) state);
        }

        private void AcceptSocketCallbackHelper(IAsyncResult ar)
        {
            if (ar.CompletedSynchronously)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.AcceptSocketCallbackAsync), ar);
            }
            else
            {
                this.AcceptSocketCallback(ar);
            }
        }

        private WindowsIdentity Authenticate(ref Stream netStream, TcpServerSocketHandler streamManager)
        {
            NegotiateStream stream = null;
            WindowsIdentity remoteIdentity;
            try
            {
                stream = new NegotiateStream(netStream);
                TokenImpersonationLevel identification = TokenImpersonationLevel.Identification;
                if (this._impersonate)
                {
                    identification = TokenImpersonationLevel.Impersonation;
                }
                stream.AuthenticateAsServer((NetworkCredential) CredentialCache.DefaultCredentials, this._protectionLevel, identification);
                netStream = stream;
                remoteIdentity = (WindowsIdentity) stream.RemoteIdentity;
            }
            catch
            {
                streamManager.SendErrorResponse(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Tcp_ServerAuthenticationFailed"), new object[0]), false);
                if (stream != null)
                {
                    stream.Close();
                }
                throw;
            }
            return remoteIdentity;
        }

        private IServerChannelSinkProvider CreateDefaultServerProviderChain()
        {
            IServerChannelSinkProvider provider = new BinaryServerFormatterSinkProvider();
            IServerChannelSinkProvider provider2 = provider;
            provider2.Next = new SoapServerFormatterSinkProvider();
            return provider;
        }

        public string GetChannelUri()
        {
            return string.Concat(new object[] { "tcp://", this._machineName, ":", this._port });
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public virtual string[] GetUrlsForUri(string objectUri)
        {
            string[] strArray = new string[1];
            if (!objectUri.StartsWith("/", StringComparison.Ordinal))
            {
                objectUri = "/" + objectUri;
            }
            strArray[0] = this.GetChannelUri() + objectUri;
            return strArray;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string Parse(string url, out string objectURI)
        {
            return TcpChannelHelper.ParseURL(url, out objectURI);
        }

        private void SetupChannel()
        {
            if (this.authSet && !this._secure)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Tcp_AuthenticationConfigServer"));
            }
            this._channelData = new ChannelDataStore(null);
            if (this._port > 0)
            {
                this._channelData.ChannelUris = new string[] { this.GetChannelUri() };
            }
            if (this._sinkProvider == null)
            {
                this._sinkProvider = this.CreateDefaultServerProviderChain();
            }
            CoreChannel.CollectChannelDataFromServerSinkProviders(this._channelData, this._sinkProvider);
            IServerChannelSink nextSink = ChannelServices.CreateServerChannelSinkChain(this._sinkProvider, this);
            this._transportSink = new TcpServerTransportSink(nextSink, this._impersonate);
            this._acceptSocketCallback = new AsyncCallback(this.AcceptSocketCallbackHelper);
            if (this._port >= 0)
            {
                this._tcpListener = new ExclusiveTcpListener(this._bindToAddr, this._port);
                this.StartListening(null);
            }
        }

        private void SetupMachineName()
        {
            if (this._forcedMachineName != null)
            {
                this._machineName = CoreChannel.DecodeMachineName(this._forcedMachineName);
            }
            else if (!this._bUseIpAddress)
            {
                this._machineName = CoreChannel.GetMachineName();
            }
            else
            {
                if ((this._bindToAddr == IPAddress.Any) || (this._bindToAddr == IPAddress.IPv6Any))
                {
                    this._machineName = CoreChannel.GetMachineIp();
                }
                else
                {
                    this._machineName = this._bindToAddr.ToString();
                }
                if (this._bindToAddr.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    this._machineName = "[" + this._machineName + "]";
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StartListening(object data)
        {
            if (this._port >= 0)
            {
                this._tcpListener.Start(this._bExclusiveAddressUse);
                this._bListening = true;
                if (this._port == 0)
                {
                    this._port = ((IPEndPoint) this._tcpListener.LocalEndpoint).Port;
                    if (this._channelData != null)
                    {
                        this._channelData.ChannelUris = new string[] { this.GetChannelUri() };
                    }
                }
                this._tcpListener.BeginAcceptSocket(this._acceptSocketCallback, null);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StopListening(object data)
        {
            if (this._port > 0)
            {
                this._bListening = false;
                if (this._tcpListener != null)
                {
                    this._tcpListener.Stop();
                }
            }
        }

        public object ChannelData
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                if (!this._bSuppressChannelData && this._bListening)
                {
                    return this._channelData;
                }
                return null;
            }
        }

        public string ChannelName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._channelName;
            }
        }

        public int ChannelPriority
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._channelPriority;
            }
        }

        public bool IsSecured
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._secure;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            set
            {
                this._secure = value;
            }
        }
    }
}

