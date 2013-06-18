namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class HttpHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        private static bool canDebugPrint = true;
        private Collection<HostedHttpTransportManager> transportManagerDirectory;

        internal HttpHostedTransportConfiguration() : this(Uri.UriSchemeHttp)
        {
        }

        protected internal HttpHostedTransportConfiguration(string scheme) : base(scheme)
        {
            this.CreateTransportManagers();
        }

        private HostedHttpTransportManager CreateTransportManager(BaseUriWithWildcard listenAddress)
        {
            UriPrefixTable<ITransportManagerRegistration> staticTransportManagerTable = null;
            if (object.ReferenceEquals(base.Scheme, Uri.UriSchemeHttp))
            {
                staticTransportManagerTable = HttpChannelListener.StaticTransportManagerTable;
            }
            else
            {
                staticTransportManagerTable = SharedHttpsTransportManager.StaticTransportManagerTable;
            }
            HostedHttpTransportManager item = null;
            lock (staticTransportManagerTable)
            {
                ITransportManagerRegistration registration;
                if (!staticTransportManagerTable.TryLookupUri(listenAddress.BaseAddress, listenAddress.HostNameComparisonMode, out registration))
                {
                    item = new HostedHttpTransportManager(listenAddress);
                    staticTransportManagerTable.RegisterUri(listenAddress.BaseAddress, listenAddress.HostNameComparisonMode, item);
                }
            }
            return item;
        }

        private void CreateTransportManagers()
        {
            Collection<HostedHttpTransportManager> collection = new Collection<HostedHttpTransportManager>();
            foreach (string str in HostedTransportConfigurationManager.MetabaseSettings.GetBindings(base.Scheme))
            {
                BaseUriWithWildcard listenAddress = BaseUriWithWildcard.CreateHostedUri(base.Scheme, str, HostingEnvironmentWrapper.ApplicationVirtualPath);
                bool flag = false;
                if (ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
                {
                    listenAddress = new BaseUriWithWildcard(listenAddress.BaseAddress, HostNameComparisonMode.WeakWildcard);
                    flag = true;
                }
                HostedHttpTransportManager item = this.CreateTransportManager(listenAddress);
                if (item != null)
                {
                    collection.Add(item);
                    base.ListenAddresses.Add(listenAddress);
                }
                if (flag)
                {
                    break;
                }
            }
            this.transportManagerDirectory = collection;
        }

        public override Uri[] GetBaseAddresses(string virtualPath)
        {
            if (((string.CompareOrdinal(base.Scheme, Uri.UriSchemeHttp) == 0) && !ServiceHostingEnvironment.IsSimpleApplicationHost) && HostedTransportConfigurationManager.MetabaseSettings.GetAllowSslOnly(virtualPath))
            {
                return new Uri[0];
            }
            return base.GetBaseAddresses(virtualPath);
        }

        internal HostedHttpTransportManager GetHttpTransportManager(Uri uri)
        {
            if (ServiceHostingEnvironment.MultipleSiteBindingsEnabled)
            {
                return this.transportManagerDirectory[0];
            }
            switch (this.transportManagerDirectory.Count)
            {
                case 0:
                    return null;

                case 1:
                {
                    HostedHttpTransportManager manager = this.transportManagerDirectory[0];
                    if (((manager.Port != uri.Port) || (string.Compare(manager.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase) != 0)) || ((manager.HostNameComparisonMode == HostNameComparisonMode.Exact) && (string.Compare(manager.ListenUri.Host, uri.Host, StringComparison.OrdinalIgnoreCase) != 0)))
                    {
                        return null;
                    }
                    return manager;
                }
            }
            HostedHttpTransportManager manager2 = null;
            HostedHttpTransportManager manager3 = null;
            string scheme = uri.Scheme;
            int port = uri.Port;
            string str2 = null;
            foreach (HostedHttpTransportManager manager4 in this.transportManagerDirectory)
            {
                if ((manager4.Port == port) && (string.Compare(manager4.Scheme, scheme, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    if (manager4.HostNameComparisonMode == HostNameComparisonMode.StrongWildcard)
                    {
                        return manager4;
                    }
                    if (manager4.HostNameComparisonMode == HostNameComparisonMode.WeakWildcard)
                    {
                        manager3 = manager4;
                    }
                    if ((manager4.HostNameComparisonMode == HostNameComparisonMode.Exact) && (string.Compare(manager4.Host, str2 ?? (str2 = uri.Host), StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        manager2 = manager4;
                    }
                }
            }
            return (manager2 ?? manager3);
        }

        [Conditional("DEBUG")]
        private static void TryDebugPrint(string message)
        {
            if (canDebugPrint)
            {
            }
        }
    }
}

