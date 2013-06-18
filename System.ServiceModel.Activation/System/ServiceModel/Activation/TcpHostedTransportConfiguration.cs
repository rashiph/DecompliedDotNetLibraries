namespace System.ServiceModel.Activation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal sealed class TcpHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        private HostedTcpTransportManager uniqueManager;

        public TcpHostedTransportConfiguration() : base(Uri.UriSchemeNetTcp)
        {
            string[] bindings = HostedTransportConfigurationManager.MetabaseSettings.GetBindings(Uri.UriSchemeNetTcp);
            for (int i = 0; i < bindings.Length; i++)
            {
                BaseUriWithWildcard baseAddress = BaseUriWithWildcard.CreateHostedUri(Uri.UriSchemeNetTcp, bindings[i], HostingEnvironmentWrapper.ApplicationVirtualPath);
                if (i == 0)
                {
                    this.uniqueManager = new HostedTcpTransportManager(baseAddress);
                }
                base.ListenAddresses.Add(baseAddress);
                TcpChannelListener.StaticTransportManagerTable.RegisterUri(baseAddress.BaseAddress, baseAddress.HostNameComparisonMode, this.uniqueManager);
            }
        }

        internal TcpTransportManager TransportManager
        {
            get
            {
                return this.uniqueManager;
            }
        }
    }
}

