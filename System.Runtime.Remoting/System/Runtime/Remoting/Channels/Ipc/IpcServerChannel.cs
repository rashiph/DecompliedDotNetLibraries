namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;

    public class IpcServerChannel : IChannelReceiver, IChannel, ISecurableChannel
    {
        private string _authorizedGroup;
        private bool _bExclusiveAddressUse;
        private bool _bListening;
        private bool _bSuppressChannelData;
        private ChannelDataStore _channelData;
        private string _channelName;
        private int _channelPriority;
        private bool _impersonate;
        private Thread _listenerThread;
        private IpcPort _port;
        private string _portName;
        private bool _secure;
        private CommonSecurityDescriptor _securityDescriptor;
        private IServerChannelSinkProvider _sinkProvider;
        private Exception _startListeningException;
        private IpcServerTransportSink _transportSink;
        private AutoResetEvent _waitForStartListening;
        private bool authSet;

        public IpcServerChannel(string portName)
        {
            this._channelPriority = 20;
            this._channelName = "ipc server";
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            if (portName == null)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Ipc_NoPortNameSpecified"));
            }
            this._portName = portName;
            this.SetupChannel();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IpcServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider) : this(properties, sinkProvider, null)
        {
        }

        public IpcServerChannel(string name, string portName)
        {
            this._channelPriority = 20;
            this._channelName = "ipc server";
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            if (portName == null)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Ipc_NoPortNameSpecified"));
            }
            this._channelName = name;
            this._portName = portName;
            this.SetupChannel();
        }

        public IpcServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider, CommonSecurityDescriptor securityDescriptor)
        {
            this._channelPriority = 20;
            this._channelName = "ipc server";
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

                        case "portName":
                            this._portName = (string) entry.Value;
                            break;

                        case "priority":
                            this._channelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "secure":
                            this._secure = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "impersonate":
                            this._impersonate = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            this.authSet = true;
                            break;

                        case "suppressChannelData":
                            this._bSuppressChannelData = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "authorizedGroup":
                            this._authorizedGroup = (string) entry.Value;
                            break;

                        case "exclusiveAddressUse":
                            this._bExclusiveAddressUse = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }
            if (this._portName == null)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Ipc_NoPortNameSpecified"));
            }
            this._sinkProvider = sinkProvider;
            this._securityDescriptor = securityDescriptor;
            this.SetupChannel();
        }

        public IpcServerChannel(string name, string portName, IServerChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 20;
            this._channelName = "ipc server";
            this._bExclusiveAddressUse = true;
            this._waitForStartListening = new AutoResetEvent(false);
            if (portName == null)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Ipc_NoPortNameSpecified"));
            }
            this._channelName = name;
            this._portName = portName;
            this._sinkProvider = sinkProvider;
            this.SetupChannel();
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
            return ("ipc://" + this._portName);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public virtual string[] GetUrlsForUri(string objectUri)
        {
            if (objectUri == null)
            {
                throw new ArgumentNullException("objectUri");
            }
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
            bool flag = true;
            bool flag2 = false;
            CommonSecurityDescriptor securityDescriptor = this._securityDescriptor;
            if (flag)
            {
                try
                {
                    if ((securityDescriptor == null) && (this._authorizedGroup != null))
                    {
                        NTAccount account = new NTAccount(this._authorizedGroup);
                        securityDescriptor = IpcPort.CreateSecurityDescriptor((SecurityIdentifier) account.Translate(typeof(SecurityIdentifier)));
                    }
                    this._port = IpcPort.Create(this._portName, securityDescriptor, this._bExclusiveAddressUse);
                }
                catch (Exception exception)
                {
                    this._startListeningException = exception;
                }
                finally
                {
                    this._waitForStartListening.Set();
                }
                if (this._port != null)
                {
                    flag2 = this._port.WaitForConnect();
                    flag = this._bListening;
                }
            }
            while (flag && (this._startListeningException == null))
            {
                IpcPort port = IpcPort.Create(this._portName, securityDescriptor, false);
                if (flag2)
                {
                    new IpcServerHandler(this._port, CoreChannel.RequestQueue, new PipeStream(this._port)) { DataArrivedCallback = new WaitCallback(this._transportSink.ServiceRequest) }.BeginReadMessage();
                }
                this._port = port;
                flag2 = this._port.WaitForConnect();
                flag = this._bListening;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string Parse(string url, out string objectURI)
        {
            return IpcChannelHelper.ParseURL(url, out objectURI);
        }

        private void SetupChannel()
        {
            if (this.authSet && !this._secure)
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Ipc_AuthenticationConfig"));
            }
            this._channelData = new ChannelDataStore(null);
            this._channelData.ChannelUris = new string[] { this.GetChannelUri() };
            if (this._sinkProvider == null)
            {
                this._sinkProvider = this.CreateDefaultServerProviderChain();
            }
            CoreChannel.CollectChannelDataFromServerSinkProviders(this._channelData, this._sinkProvider);
            IServerChannelSink nextSink = ChannelServices.CreateServerChannelSinkChain(this._sinkProvider, this);
            this._transportSink = new IpcServerTransportSink(nextSink, this._secure, this._impersonate);
            ThreadStart start = new ThreadStart(this.Listen);
            this._listenerThread = new Thread(start);
            this._listenerThread.IsBackground = true;
            this.StartListening(null);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StartListening(object data)
        {
            if (!this._listenerThread.IsAlive)
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
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public void StopListening(object data)
        {
            this._bListening = false;
            this._port.Dispose();
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
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
            set
            {
                this._secure = value;
                if (this._transportSink != null)
                {
                    this._transportSink.IsSecured = value;
                }
            }
        }
    }
}

