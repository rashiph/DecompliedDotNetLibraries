namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;

    public class HttpClientChannel : BaseChannelWithProperties, IChannelSender, IChannel, ISecurableChannel
    {
        private bool _bAuthenticatedConnectionSharing;
        private bool _bUseDefaultCredentials;
        private string _channelName;
        private int _channelPriority;
        private int _clientConnectionLimit;
        private string _proxyName;
        private IWebProxy _proxyObject;
        private int _proxyPort;
        private bool _secure;
        private IClientChannelSinkProvider _sinkProvider;
        private int _timeout;
        private const string ProxyNameKey = "proxyname";
        private const string ProxyPortKey = "proxyport";
        private static ICollection s_keySet;

        public HttpClientChannel()
        {
            this._channelPriority = 1;
            this._channelName = "http client";
            this._proxyPort = -1;
            this._timeout = -1;
            this._bAuthenticatedConnectionSharing = true;
            this.SetupChannel();
        }

        public HttpClientChannel(IDictionary properties, IClientChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "http client";
            this._proxyPort = -1;
            this._timeout = -1;
            this._bAuthenticatedConnectionSharing = true;
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    switch (((string) entry.Key))
                    {
                        case "name":
                            this._channelName = (string) entry.Value;
                            break;

                        case "priority":
                            this._channelPriority = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "proxyName":
                            this["proxyName"] = entry.Value;
                            break;

                        case "proxyPort":
                            this["proxyPort"] = entry.Value;
                            break;

                        case "timeout":
                            this._timeout = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "clientConnectionLimit":
                            this._clientConnectionLimit = Convert.ToInt32(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "useDefaultCredentials":
                            this._bUseDefaultCredentials = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;

                        case "useAuthenticatedConnectionSharing":
                            this._bAuthenticatedConnectionSharing = Convert.ToBoolean(entry.Value, CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }
            this._sinkProvider = sinkProvider;
            this.SetupChannel();
        }

        public HttpClientChannel(string name, IClientChannelSinkProvider sinkProvider)
        {
            this._channelPriority = 1;
            this._channelName = "http client";
            this._proxyPort = -1;
            this._timeout = -1;
            this._bAuthenticatedConnectionSharing = true;
            this._channelName = name;
            this._sinkProvider = sinkProvider;
            this.SetupChannel();
        }

        private IClientChannelSinkProvider CreateDefaultClientProviderChain()
        {
            IClientChannelSinkProvider provider = new SoapClientFormatterSinkProvider();
            IClientChannelSinkProvider provider2 = provider;
            provider2.Next = new HttpClientTransportSinkProvider(this._timeout);
            return provider;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public virtual IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI)
        {
            objectURI = null;
            string uriString = null;
            if (url != null)
            {
                uriString = this.Parse(url, out objectURI);
            }
            else if ((remoteChannelData != null) && (remoteChannelData is IChannelDataStore))
            {
                IChannelDataStore store = (IChannelDataStore) remoteChannelData;
                if (this.Parse(store.ChannelUris[0], out objectURI) != null)
                {
                    uriString = store.ChannelUris[0];
                }
            }
            if (uriString == null)
            {
                return null;
            }
            if (url == null)
            {
                url = uriString;
            }
            if (this._clientConnectionLimit > 0)
            {
                ServicePoint point = ServicePointManager.FindServicePoint(new Uri(uriString));
                if (point.ConnectionLimit < this._clientConnectionLimit)
                {
                    point.ConnectionLimit = this._clientConnectionLimit;
                }
            }
            IClientChannelSink sink = this._sinkProvider.CreateSink(this, url, remoteChannelData);
            IMessageSink sink2 = sink as IMessageSink;
            if ((sink != null) && (sink2 == null))
            {
                throw new RemotingException(CoreChannel.GetResourceString("Remoting_Channels_ChannelSinkNotMsgSink"));
            }
            return sink2;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure, Infrastructure=true)]
        public string Parse(string url, out string objectURI)
        {
            return HttpChannelHelper.ParseURL(url, out objectURI);
        }

        private void SetupChannel()
        {
            if (this._sinkProvider != null)
            {
                CoreChannel.AppendProviderToClientProviderChain(this._sinkProvider, new HttpClientTransportSinkProvider(this._timeout));
            }
            else
            {
                this._sinkProvider = this.CreateDefaultClientProviderChain();
            }
        }

        private void UpdateProxy()
        {
            if (((this._proxyName != null) && (this._proxyName.Length > 0)) && (this._proxyPort > 0))
            {
                WebProxy proxy = new WebProxy(this._proxyName, this._proxyPort) {
                    BypassProxyOnLocal = true
                };
                proxy.BypassList = new string[] { CoreChannel.GetMachineIp() };
                this._proxyObject = proxy;
            }
            else
            {
                this._proxyObject = new WebProxy();
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

        public override object this[object key]
        {
            get
            {
                string str = key as string;
                if (str != null)
                {
                    switch (str.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "proxyname":
                            return this._proxyName;

                        case "proxyport":
                            return this._proxyPort;
                    }
                }
                return null;
            }
            set
            {
                string str2;
                string str = key as string;
                if ((str != null) && ((str2 = str.ToLower(CultureInfo.InvariantCulture)) != null))
                {
                    if (!(str2 == "proxyname"))
                    {
                        if (str2 == "proxyport")
                        {
                            this._proxyPort = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                            this.UpdateProxy();
                        }
                    }
                    else
                    {
                        this._proxyName = (string) value;
                        this.UpdateProxy();
                    }
                }
            }
        }

        public override ICollection Keys
        {
            get
            {
                if (s_keySet == null)
                {
                    ArrayList list = new ArrayList(2);
                    list.Add("proxyname");
                    list.Add("proxyport");
                    s_keySet = list;
                }
                return s_keySet;
            }
        }

        internal IWebProxy ProxyObject
        {
            get
            {
                return this._proxyObject;
            }
        }

        internal bool UseAuthenticatedConnectionSharing
        {
            get
            {
                return this._bAuthenticatedConnectionSharing;
            }
        }

        internal bool UseDefaultCredentials
        {
            get
            {
                if (!this._secure)
                {
                    return this._bUseDefaultCredentials;
                }
                return true;
            }
        }
    }
}

