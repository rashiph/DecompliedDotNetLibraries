namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class PeerEndpointIdentity : EndpointIdentity
    {
        public PeerEndpointIdentity()
        {
            base.Initialize(PeerIdentityClaim.Claim());
        }
    }
}

