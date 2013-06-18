namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.IdentityModel.Claims;

    internal class PeerAuthenticationFailureTraceRecord : PeerSecurityTraceRecord
    {
        public PeerAuthenticationFailureTraceRecord(string meshId, string remoteAddress) : base(meshId, remoteAddress, null, null)
        {
        }

        public PeerAuthenticationFailureTraceRecord(string meshId, string remoteAddress, ClaimSet claimSet, Exception e) : base(meshId, remoteAddress, claimSet, e)
        {
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PeerAuthenticationTraceRecord";
            }
        }
    }
}

