namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;

    internal class WindowsUserNameCachingSecurityTokenAuthenticator : WindowsUserNameSecurityTokenAuthenticator, ILogonTokenCacheManager, IDisposable
    {
        private LogonTokenCache logonTokenCache;

        public WindowsUserNameCachingSecurityTokenAuthenticator(bool includeWindowsGroups, int maxCachedLogonTokens, TimeSpan cachedLogonTokenLifetime) : base(includeWindowsGroups)
        {
            this.logonTokenCache = new LogonTokenCache(maxCachedLogonTokens, cachedLogonTokenLifetime);
        }

        public void Dispose()
        {
            this.FlushLogonTokenCache();
        }

        public void FlushLogonTokenCache()
        {
            if (this.logonTokenCache != null)
            {
                this.logonTokenCache.Flush();
            }
        }

        public bool RemoveCachedLogonToken(string username)
        {
            if (this.logonTokenCache == null)
            {
                return false;
            }
            return this.logonTokenCache.TryRemoveTokenCache(username);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            LogonToken token;
            if (this.logonTokenCache.TryGetTokenCache(userName, out token))
            {
                if (token.PasswordEquals(password))
                {
                    return token.GetAuthorizationPolicies();
                }
                this.logonTokenCache.TryRemoveTokenCache(userName);
            }
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = base.ValidateUserNamePasswordCore(userName, password);
            this.logonTokenCache.TryAddTokenCache(userName, password, authorizationPolicies);
            return authorizationPolicies;
        }
    }
}

