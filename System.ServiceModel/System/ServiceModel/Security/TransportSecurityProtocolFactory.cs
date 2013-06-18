namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;

    internal class TransportSecurityProtocolFactory : SecurityProtocolFactory
    {
        public TransportSecurityProtocolFactory()
        {
        }

        internal TransportSecurityProtocolFactory(TransportSecurityProtocolFactory factory) : base(factory)
        {
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new TransportSecurityProtocol(this, target, via);
        }

        public override bool SupportsDuplex
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsReplayDetection
        {
            get
            {
                return false;
            }
        }
    }
}

