namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    public abstract class SecurityTokenManager
    {
        protected SecurityTokenManager()
        {
        }

        public abstract SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver);
        public abstract SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement);
        public abstract SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version);
    }
}

