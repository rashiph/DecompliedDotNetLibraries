namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public abstract class StreamSecurityUpgradeProvider : StreamUpgradeProvider
    {
        protected StreamSecurityUpgradeProvider()
        {
        }

        protected StreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts) : base(timeouts)
        {
        }

        public abstract EndpointIdentity Identity { get; }
    }
}

