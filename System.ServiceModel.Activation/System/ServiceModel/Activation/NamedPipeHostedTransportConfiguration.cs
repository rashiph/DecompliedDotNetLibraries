namespace System.ServiceModel.Activation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal sealed class NamedPipeHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        private HostedNamedPipeTransportManager uniqueManager;

        public NamedPipeHostedTransportConfiguration() : base(Uri.UriSchemeNetPipe)
        {
            string[] bindings = HostedTransportConfigurationManager.MetabaseSettings.GetBindings(Uri.UriSchemeNetPipe);
            for (int i = 0; i < bindings.Length; i++)
            {
                BaseUriWithWildcard baseAddress = BaseUriWithWildcard.CreateHostedPipeUri(bindings[i], HostingEnvironmentWrapper.ApplicationVirtualPath);
                if (i == 0)
                {
                    this.uniqueManager = new HostedNamedPipeTransportManager(baseAddress);
                }
                base.ListenAddresses.Add(baseAddress);
                NamedPipeChannelListener.StaticTransportManagerTable.RegisterUri(baseAddress.BaseAddress, baseAddress.HostNameComparisonMode, this.uniqueManager);
            }
        }

        internal NamedPipeTransportManager TransportManager
        {
            get
            {
                return this.uniqueManager;
            }
        }
    }
}

