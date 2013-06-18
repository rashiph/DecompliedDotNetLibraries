namespace System.ServiceModel.Channels
{
    using System;
    using System.IdentityModel.Claims;
    using System.ServiceModel;

    internal class PeerIdentityClaim
    {
        public const string PeerClaimType = "http://schemas.microsoft.com/net/2006/05/peer/peer";
        private const string resourceRight = "peer";
        private const string resourceValue = "peer";

        internal static System.IdentityModel.Claims.Claim Claim()
        {
            return new System.IdentityModel.Claims.Claim("http://schemas.microsoft.com/net/2006/05/peer/peer", "peer", "peer");
        }

        internal static bool IsMatch(EndpointIdentity identity)
        {
            return (identity.IdentityClaim.ClaimType == "http://schemas.microsoft.com/net/2006/05/peer/peer");
        }
    }
}

