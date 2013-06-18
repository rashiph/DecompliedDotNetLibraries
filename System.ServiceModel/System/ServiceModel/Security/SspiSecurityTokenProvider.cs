namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Security.Principal;
    using System.ServiceModel.Security.Tokens;

    public class SspiSecurityTokenProvider : SecurityTokenProvider
    {
        internal const bool DefaultAllowNtlm = true;
        internal const bool DefaultAllowUnauthenticatedCallers = false;
        internal const bool DefaultExtractWindowsGroupClaims = true;
        private SspiSecurityToken token;

        public SspiSecurityTokenProvider(NetworkCredential credential, bool extractGroupsForWindowsAccounts, bool allowUnauthenticatedCallers)
        {
            this.token = new SspiSecurityToken(credential, extractGroupsForWindowsAccounts, allowUnauthenticatedCallers);
        }

        public SspiSecurityTokenProvider(NetworkCredential credential, bool allowNtlm, TokenImpersonationLevel impersonationLevel)
        {
            this.token = new SspiSecurityToken(impersonationLevel, allowNtlm, credential);
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return this.token;
        }
    }
}

