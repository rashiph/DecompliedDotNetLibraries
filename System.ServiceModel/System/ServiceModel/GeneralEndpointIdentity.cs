namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Claims;

    internal class GeneralEndpointIdentity : EndpointIdentity
    {
        public GeneralEndpointIdentity(Claim identityClaim)
        {
            base.Initialize(identityClaim);
        }
    }
}

