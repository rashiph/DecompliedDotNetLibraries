namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class PeerDoNothingSecurityProtocolFactory : SecurityProtocolFactory
    {
        public override void OnAbort()
        {
        }

        public override void OnClose(TimeSpan timeout)
        {
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            return new PeerDoNothingSecurityProtocol(this);
        }

        public override void OnOpen(TimeSpan timeout)
        {
        }
    }
}

