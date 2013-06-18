namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    internal abstract class HttpTransportManager : TransportManager, ITransportManagerRegistration
    {
        private volatile Dictionary<string, UriPrefixTable<HttpChannelListener>> addressTables;
        private readonly System.ServiceModel.HostNameComparisonMode hostNameComparisonMode;
        private readonly Uri listenUri;
        private readonly string realm;

        internal HttpTransportManager()
        {
            this.addressTables = new Dictionary<string, UriPrefixTable<HttpChannelListener>>();
        }

        internal HttpTransportManager(Uri listenUri, System.ServiceModel.HostNameComparisonMode hostNameComparisonMode) : this()
        {
            this.hostNameComparisonMode = hostNameComparisonMode;
            this.listenUri = listenUri;
        }

        internal HttpTransportManager(Uri listenUri, System.ServiceModel.HostNameComparisonMode hostNameComparisonMode, string realm) : this(listenUri, hostNameComparisonMode)
        {
            this.realm = realm;
        }

        private void Cleanup()
        {
            this.TransportManagerTable.UnregisterUri(this.ListenUri, this.HostNameComparisonMode);
        }

        protected void Fault(Exception exception)
        {
            lock (base.ThisLock)
            {
                foreach (KeyValuePair<string, UriPrefixTable<HttpChannelListener>> pair in this.addressTables)
                {
                    base.Fault<HttpChannelListener>(pair.Value, exception);
                }
            }
        }

        internal virtual bool IsCompatible(HttpChannelListener listener)
        {
            return ((this.hostNameComparisonMode == listener.HostNameComparisonMode) && (this.realm == listener.Realm));
        }

        internal override void OnAbort()
        {
            this.Cleanup();
            base.OnAbort();
        }

        internal override void OnClose(TimeSpan timeout)
        {
            this.Cleanup();
        }

        internal override void Register(TransportChannelListener channelListener)
        {
            UriPrefixTable<HttpChannelListener> table;
            string method = ((HttpChannelListener) channelListener).Method;
            if (!this.addressTables.TryGetValue(method, out table))
            {
                lock (base.ThisLock)
                {
                    if (!this.addressTables.TryGetValue(method, out table))
                    {
                        Dictionary<string, UriPrefixTable<HttpChannelListener>> dictionary = new Dictionary<string, UriPrefixTable<HttpChannelListener>>(this.addressTables);
                        table = new UriPrefixTable<HttpChannelListener>();
                        dictionary[method] = table;
                        this.addressTables = dictionary;
                    }
                }
            }
            table.RegisterUri(channelListener.Uri, channelListener.InheritBaseAddressSettings ? this.hostNameComparisonMode : channelListener.HostNameComparisonModeInternal, (HttpChannelListener) channelListener);
        }

        protected void StartReceiveBytesActivity(ServiceModelActivity activity, Uri requestUri)
        {
            ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityReceiveBytes", new object[] { requestUri.ToString() }), ActivityType.ReceiveBytes);
        }

        IList<TransportManager> ITransportManagerRegistration.Select(TransportChannelListener channelListener)
        {
            IList<TransportManager> list = null;
            if (this.IsCompatible((HttpChannelListener) channelListener))
            {
                list = new List<TransportManager> {
                    this
                };
            }
            return list;
        }

        protected void TraceMessageReceived(Uri listenUri)
        {
            if (TD.MessageReceivedByTransportIsEnabled())
            {
                TD.MessageReceivedByTransport(listenUri.OriginalString);
            }
        }

        protected bool TryLookupUri(Uri requestUri, string requestMethod, System.ServiceModel.HostNameComparisonMode hostNameComparisonMode, out HttpChannelListener listener)
        {
            UriPrefixTable<HttpChannelListener> table;
            listener = null;
            if (requestMethod == null)
            {
                requestMethod = string.Empty;
            }
            Dictionary<string, UriPrefixTable<HttpChannelListener>> addressTables = this.addressTables;
            HttpChannelListener item = null;
            if (((requestMethod.Length > 0) && addressTables.TryGetValue(requestMethod, out table)) && (table.TryLookupUri(requestUri, hostNameComparisonMode, out item) && (string.Compare(requestUri.AbsolutePath, item.Uri.AbsolutePath, StringComparison.OrdinalIgnoreCase) != 0)))
            {
                item = null;
            }
            if (addressTables.TryGetValue(string.Empty, out table) && table.TryLookupUri(requestUri, hostNameComparisonMode, out listener))
            {
                if ((item != null) && (item.Uri.AbsoluteUri.Length >= listener.Uri.AbsoluteUri.Length))
                {
                    listener = item;
                }
            }
            else
            {
                listener = item;
            }
            return (listener != null);
        }

        internal override void Unregister(TransportChannelListener channelListener)
        {
            UriPrefixTable<HttpChannelListener> table;
            if (!this.addressTables.TryGetValue(((HttpChannelListener) channelListener).Method, out table))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ListenerFactoryNotRegistered", new object[] { channelListener.Uri })));
            }
            System.ServiceModel.HostNameComparisonMode registeredComparisonMode = channelListener.InheritBaseAddressSettings ? this.hostNameComparisonMode : channelListener.HostNameComparisonModeInternal;
            TransportManager.EnsureRegistered<HttpChannelListener>(table, (HttpChannelListener) channelListener, registeredComparisonMode);
            table.UnregisterUri(channelListener.Uri, registeredComparisonMode);
        }

        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
        }

        internal bool IsHosted { get; set; }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        internal string Realm
        {
            get
            {
                return this.realm;
            }
        }

        internal override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttp;
            }
        }

        internal virtual UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return HttpChannelListener.StaticTransportManagerTable;
            }
        }
    }
}

