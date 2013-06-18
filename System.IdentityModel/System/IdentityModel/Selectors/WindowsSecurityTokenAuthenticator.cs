namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Tokens;

    public class WindowsSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        private bool includeWindowsGroups;

        public WindowsSecurityTokenAuthenticator() : this(true)
        {
        }

        public WindowsSecurityTokenAuthenticator(bool includeWindowsGroups)
        {
            this.includeWindowsGroups = includeWindowsGroups;
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is WindowsSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            WindowsSecurityToken token2 = (WindowsSecurityToken) token;
            WindowsClaimSet claimSet = new WindowsClaimSet(token2.WindowsIdentity, token2.AuthenticationType, this.includeWindowsGroups, token2.ValidTo);
            return System.IdentityModel.SecurityUtils.CreateAuthorizationPolicies(claimSet, token2.ValidTo);
        }
    }
}

