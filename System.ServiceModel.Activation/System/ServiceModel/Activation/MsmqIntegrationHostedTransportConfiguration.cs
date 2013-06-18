namespace System.ServiceModel.Activation
{
    using System;

    internal sealed class MsmqIntegrationHostedTransportConfiguration : MsmqHostedTransportConfiguration
    {
        public MsmqIntegrationHostedTransportConfiguration() : base(MsmqUri.FormatNameAddressTranslator)
        {
        }
    }
}

