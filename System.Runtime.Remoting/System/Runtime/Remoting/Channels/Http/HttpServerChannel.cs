namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.MetadataServices;
    using System.Security.Permissions;
    using System.Threading;

    public class HttpServerChannel : BaseChannelWithProperties, IChannelReceiver, IChannel, IChannelReceiverHook
    {
        private bool _bExclusiveAddressUse;
        private bool _bHooked;
        private IPAddress _bindToAddr;
        private bool _bListening;
        private bool _bSuppressChannelData;
        private bool _bUseIpAddress;
        private ChannelDataStore _channelData;
        private string _channelName;
        private int _channelPriority;
        private string _forcedMachineName;
        private Thread _listenerThread;
        private string _machineName;
        private int _port;
        private IServerChannelSink _sinkChain;
        private IServerChannelSinkProvider _sinkProvider;
        private Exception _startListeningException;
        private ExclusiveTcpListener _tcpListener;
        private HttpServerTransportSink _transportSink;
        private AutoResetEvent _waitForStartListening;
        private bool _wantsToListen;

        public HttpServerChannel()
        {
            this._channelPriority = 1;
            this._channelName = "http server";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._wantsToListen = true;
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            this.SetupMachineName();
            this.SetupChannel();
        }

        public HttpServerChannel(int port)
        {
            this._channelPriority = 1;
            this._channelName = "http server";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._wantsToListen = true;
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            this._port = port;
            this.SetupMachineName();
            this.SetupChannel();
        }

        public HttpServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "http server";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._wantsToListen = true;
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    switch (((string) entry.Key))
                    {
                        case "name":
                            this._channelName = (string) entry.Value;
                            break;

                        case "bindTo":
                            this._bindToAddr = IPAddress.Parse((string) entry.Value);
                            break;

                        case "listen":
                            this._wantsToListen = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "machineName":
                            this._forcedMachineName = (string) entry.Value;
                            break;

                        case "port":
                            this._port = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "priority":
                            this._channelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "suppressChannelData":
                            this._bSuppressChannelData = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "useIpAddress":
                            this._bUseIpAddress = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "exclusiveAddressUse":
                            this._bExclusiveAddressUse = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }
            this._sinkProvider = sinkProvider;
            this.SetupMachineName();
            this.SetupChannel();
        }

        public HttpServerChannel(string name, int port)
        {
            this._channelPriority = 1;
            this._channelName = "http server";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._wantsToListen = true;
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            this._channelName = name;
            this._port = port;
            this.SetupMachineName();
            this.SetupChannel();
        }

        public HttpServerChannel(string name, int port, IServerChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "http server";
            this._port = -1;
            this._bUseIpAddress = true;
            this._bindToAddr = Socket.OSSupportsIPv4 ? IPAddress.Any : IPAddress.IPv6Any;
            this._wantsToListen = true;
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            this._channelName = name;
            this._port = port;
            this._sinkProvider = sinkProvider;
            this.SetupMachineName();
            this.SetupChannel();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void AddHookChannelUri(string channelUri)
        {
            if (this._channelData.ChannelUris != null)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_LimitListenerOfOne"));
            }
            if (this._forcedMachineName != null)
            {
                channelUri = HttpChannelHelper.ReplaceMachineNameWithThisString(channelUri, this._forcedMachineName);
            }
            else if (this._bUseIpAddress)
            {
                channelUri = HttpChannelHelper.ReplaceMachineNameWithThisString(channelUri, CoreChannel.GetMachineIp());
            }
            this._channelData.ChannelUris = new string[] { channelUri };
            this._wantsToListen = false;
            this._bHooked = true;
        }

        private IServerChannelSinkProvider CreateDefaultServerProviderChain()
        {
            IServerChannelSinkProvider provider = new SdlChannelSinkProvider();
            IServerChannelSinkProvider provider2 = provider;
            provider2.Next = new SoapServerFormatterSinkProvider();
            provider2.Next.Next = new BinaryServerFormatterSinkProvider();
            return provider;
        }

        public string GetChannelUri()
        {
            if ((this._channelData != null) && (this._channelData.ChannelUris != null))
            {
                return this._channelData.ChannelUris[0];
            }
            return string.Concat(new object[] { "http://", this._machineName, ":", this._port });
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

        private void Listen()
        {
            bool flag = false;
            try
            {
                this._tcpListener.Start(this._bExclusiveAddressUse);
                flag = true;
            }
            catch (Exception exception)
            {
                this._startListeningException = exception;
            }
            this._waitForStartListening.Set();
            while (flag)
            {
                try
                {
                    Socket socket = this._tcpListener.AcceptSocket();
                    if (socket == null)
                    {
                        throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Socket_Accept"), new object[] { Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture) }));
                    }
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, 1);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                    LingerOption optionValue = new LingerOption(true, 3);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
                    Stream stream = new SocketStream(socket);
                    new HttpServerSocketHandler(socket, CoreChannel.RequestQueue, stream) { DataArrivedCallback = new WaitCallback(this._transportSink.ServiceRequest) }.BeginReadMessage();
                    continue;
                }
                catch (Exception exception2)
                {
                    if (!this._bListening)
                    {
                        flag = false;
                    }
                    else
                    {
                        SocketException exception4 = exception2 as SocketException;
                    }
                    continue;
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string Parse(string url, out string objectURI)
        {
            return HttpChannelHelper.ParseURL(url, out objectURI);
        }

        private void SetupChannel()
        {
            this._channelData = new ChannelDataStore(null);
            if (this._port > 0)
            {
                string channelUri = this.GetChannelUri();
                this._channelData.ChannelUris = new string[] { channelUri };
                this._wantsToListen = false;
            }
            if (this._sinkProvider == null)
            {
                this._sinkProvider = this.CreateDefaultServerProviderChain();
            }
            CoreChannel.CollectChannelDataFromServerSinkProviders(this._channelData, this._sinkProvider);
            this._sinkChain = ChannelServices.CreateServerChannelSinkChain(this._sinkProvider, this);
            this._transportSink = new HttpServerTransportSink(this._sinkChain);
            base.SinksWithProperties = this._sinkChain;
            if (this._port >= 0)
            {
                this._tcpListener = new ExclusiveTcpListener(this._bindToAddr, this._port);
                ThreadStart start = new ThreadStart(this.Listen);
                this._listenerThread = new Thread(start);
                this._listenerThread.IsBackground = true;
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
            if ((this._port >= 0) && !this._listenerThread.IsAlive)
            {
                this._listenerThread.Start();
                this._waitForStartListening.WaitOne();
                if (this._startListeningException != null)
                {
                    Exception exception = this._startListeningException;
                    this._startListeningException = null;
                    throw exception;
                }
                this._bListening = true;
                if (this._port == 0)
                {
                    this._port = ((IPEndPoint) this._tcpListener.LocalEndpoint).Port;
                    if (this._channelData != null)
                    {
                        string channelUri = this.GetChannelUri();
                        this._channelData.ChannelUris = new string[] { channelUri };
                    }
                }
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
                if (this._bSuppressChannelData || (!this._bListening && !this._bHooked))
                {
                    return null;
                }
                return this._channelData;
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

        public string ChannelScheme
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
            get
            {
                return "http";
            }
        }

        public IServerChannelSink ChannelSinkChain
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
            get
            {
                return this._sinkChain;
            }
        }

        internal bool IsSecured
        {
            get
            {
                return false;
            }
            set
            {
                if ((this._port >= 0) && value)
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_Http_UseIISToSecureHttpServer"));
                }
            }
        }

        public override object this[object key]
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public override ICollection Keys
        {
            get
            {
                return new ArrayList();
            }
        }

        public bool WantsToListen
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            get
            {
                return this._wantsToListen;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._wantsToListen = value;
            }
        }
    }
}

