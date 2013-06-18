namespace System.ServiceModel.Channels
{
    using System;
    using System.IdentityModel.Policy;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class PeerIdentityVerifier : IdentityVerifier
    {
        public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
        {
            return true;
        }

        public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
        {
            if (reference == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reference");
            }
            identity = reference.Identity;
            if (identity == null)
            {
                identity = new PeerEndpointIdentity();
            }
            return true;
        }
    }
}

