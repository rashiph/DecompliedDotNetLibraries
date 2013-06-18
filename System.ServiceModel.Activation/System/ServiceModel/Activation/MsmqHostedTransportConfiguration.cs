namespace System.ServiceModel.Activation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Web.Hosting;

    internal class MsmqHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        private MsmqHostedTransportManager uniqueManager;

        public MsmqHostedTransportConfiguration() : this(MsmqUri.NetMsmqAddressTranslator)
        {
        }

        protected MsmqHostedTransportConfiguration(MsmqUri.IAddressTranslator addressing) : base(addressing.Scheme)
        {
            string[] bindings = HostedTransportConfigurationManager.MetabaseSettings.GetBindings(addressing.Scheme);
            this.uniqueManager = new MsmqHostedTransportManager(bindings, addressing);
            for (int i = 0; i < bindings.Length; i++)
            {
                Uri baseAddress = addressing.CreateUri(bindings[i], HostingEnvironment.ApplicationVirtualPath, false);
                base.ListenAddresses.Add(new BaseUriWithWildcard(baseAddress, HostNameComparisonMode.Exact));
                UniqueTransportManagerRegistration item = new UniqueTransportManagerRegistration(this.uniqueManager, baseAddress, HostNameComparisonMode.Exact);
                Msmq.StaticTransportManagerTable.RegisterUri(baseAddress, HostNameComparisonMode.Exact, item);
            }
            this.uniqueManager.Start(null);
        }

        public override Uri[] GetBaseAddresses(string virtualPath)
        {
            return this.uniqueManager.GetBaseAddresses(virtualPath);
        }

        internal MsmqHostedTransportManager TransportManager
        {
            get
            {
                return this.uniqueManager;
            }
        }
    }
}

