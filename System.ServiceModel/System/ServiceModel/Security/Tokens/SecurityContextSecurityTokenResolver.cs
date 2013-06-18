namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    public class SecurityContextSecurityTokenResolver : SecurityTokenResolver, ISecurityContextSecurityTokenCache
    {
        private int capacity;
        private TimeSpan clockSkew;
        private bool removeOldestTokensOnCacheFull;
        private SecurityContextTokenCache tokenCache;

        public SecurityContextSecurityTokenResolver(int securityContextCacheCapacity, bool removeOldestTokensOnCacheFull) : this(securityContextCacheCapacity, removeOldestTokensOnCacheFull, SecurityProtocolFactory.defaultMaxClockSkew)
        {
        }

        public SecurityContextSecurityTokenResolver(int securityContextCacheCapacity, bool removeOldestTokensOnCacheFull, TimeSpan clockSkew)
        {
            this.clockSkew = SecurityProtocolFactory.defaultMaxClockSkew;
            if (securityContextCacheCapacity <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("securityContextCacheCapacity", System.ServiceModel.SR.GetString("ValueMustBeGreaterThanZero")));
            }
            if (clockSkew < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("clockSkew", System.ServiceModel.SR.GetString("TimeSpanCannotBeLessThanTimeSpanZero")));
            }
            this.capacity = securityContextCacheCapacity;
            this.removeOldestTokensOnCacheFull = removeOldestTokensOnCacheFull;
            this.clockSkew = clockSkew;
            this.tokenCache = new SecurityContextTokenCache(this.capacity, this.removeOldestTokensOnCacheFull, clockSkew);
        }

        public void AddContext(SecurityContextSecurityToken token)
        {
            this.tokenCache.AddContext(token);
        }

        public void ClearContexts()
        {
            this.tokenCache.ClearContexts();
        }

        public Collection<SecurityContextSecurityToken> GetAllContexts(UniqueId contextId)
        {
            return this.tokenCache.GetAllContexts(contextId);
        }

        public SecurityContextSecurityToken GetContext(UniqueId contextId, UniqueId generation)
        {
            return this.tokenCache.GetContext(contextId, generation);
        }

        public void RemoveAllContexts(UniqueId contextId)
        {
            this.tokenCache.RemoveAllContexts(contextId);
        }

        public void RemoveContext(UniqueId contextId, UniqueId generation)
        {
            this.tokenCache.RemoveContext(contextId, generation, false);
        }

        public bool TryAddContext(SecurityContextSecurityToken token)
        {
            return this.tokenCache.TryAddContext(token);
        }

        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            SecurityToken token;
            if (this.TryResolveTokenCore(keyIdentifierClause, out token))
            {
                key = ((SecurityContextSecurityToken) token).SecurityKeys[0];
                return true;
            }
            key = null;
            return false;
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            SecurityContextKeyIdentifierClause clause;
            if (keyIdentifier.TryFind<SecurityContextKeyIdentifierClause>(out clause))
            {
                return base.TryResolveToken(clause, out token);
            }
            token = null;
            return false;
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            SecurityContextKeyIdentifierClause clause = keyIdentifierClause as SecurityContextKeyIdentifierClause;
            if (clause != null)
            {
                token = this.tokenCache.GetContext(clause.ContextId, clause.Generation);
            }
            else
            {
                token = null;
            }
            return (token != null);
        }

        public void UpdateContextCachingTime(SecurityContextSecurityToken context, DateTime expirationTime)
        {
            this.tokenCache.UpdateContextCachingTime(context, expirationTime);
        }

        public TimeSpan ClockSkew
        {
            get
            {
                return this.clockSkew;
            }
        }

        public bool RemoveOldestTokensOnCacheFull
        {
            get
            {
                return this.removeOldestTokensOnCacheFull;
            }
        }

        public int SecurityContextTokenCacheCapacity
        {
            get
            {
                return this.capacity;
            }
        }
    }
}

