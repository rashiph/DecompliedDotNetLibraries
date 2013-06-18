namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    public class SecurityContextSecurityToken : SecurityToken, TimeBoundedCache.IExpirableItem, IDisposable
    {
        private ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
        private SecurityMessageProperty bootstrapMessageProperty;
        private UniqueId contextId;
        private byte[] cookieBlob;
        private bool disposed;
        private string id;
        private bool isCookieMode;
        private byte[] key;
        private DateTime keyEffectiveTime;
        private DateTime keyExpirationTime;
        private UniqueId keyGeneration;
        private string keyString;
        private ReadOnlyCollection<SecurityKey> securityKeys;
        private DateTime tokenEffectiveTime;
        private DateTime tokenExpirationTime;

        private SecurityContextSecurityToken(SecurityContextSecurityToken from)
        {
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = System.IdentityModel.SecurityUtils.CloneAuthorizationPoliciesIfNecessary(from.authorizationPolicies);
            this.id = from.id;
            this.Initialize(from.contextId, from.key, from.tokenEffectiveTime, from.tokenExpirationTime, authorizationPolicies, from.isCookieMode, from.keyGeneration, from.keyEffectiveTime, from.keyExpirationTime);
            this.cookieBlob = from.cookieBlob;
            this.bootstrapMessageProperty = (from.bootstrapMessageProperty == null) ? null : ((SecurityMessageProperty) from.BootstrapMessageProperty.CreateCopy());
        }

        internal SecurityContextSecurityToken(SecurityContextSecurityToken sourceToken, string id) : this(sourceToken, id, sourceToken.key, sourceToken.keyGeneration, sourceToken.keyEffectiveTime, sourceToken.keyExpirationTime, sourceToken.AuthorizationPolicies)
        {
        }

        public SecurityContextSecurityToken(UniqueId contextId, byte[] key, DateTime validFrom, DateTime validTo) : this(contextId, System.ServiceModel.Security.SecurityUtils.GenerateId(), key, validFrom, validTo)
        {
        }

        public SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo) : this(contextId, id, key, validFrom, validTo, null)
        {
        }

        public SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            this.id = id;
            this.Initialize(contextId, key, validFrom, validTo, authorizationPolicies, false, null, validFrom, validTo);
        }

        internal SecurityContextSecurityToken(SecurityContextSecurityToken sourceToken, string id, byte[] key, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            this.id = id;
            this.Initialize(sourceToken.contextId, key, sourceToken.ValidFrom, sourceToken.ValidTo, authorizationPolicies, sourceToken.isCookieMode, keyGeneration, keyEffectiveTime, keyExpirationTime);
            this.cookieBlob = sourceToken.cookieBlob;
            this.bootstrapMessageProperty = (sourceToken.bootstrapMessageProperty == null) ? null : ((SecurityMessageProperty) sourceToken.BootstrapMessageProperty.CreateCopy());
        }

        internal SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode, byte[] cookieBlob) : this(contextId, id, key, validFrom, validTo, authorizationPolicies, isCookieMode, cookieBlob, null, validFrom, validTo)
        {
        }

        public SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            this.id = id;
            this.Initialize(contextId, key, validFrom, validTo, authorizationPolicies, false, keyGeneration, keyEffectiveTime, keyExpirationTime);
        }

        internal SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode, byte[] cookieBlob, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime)
        {
            this.id = id;
            this.Initialize(contextId, key, validFrom, validTo, authorizationPolicies, isCookieMode, keyGeneration, keyEffectiveTime, keyExpirationTime);
            this.cookieBlob = cookieBlob;
        }

        public override bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            return ((typeof(T) == typeof(SecurityContextKeyIdentifierClause)) || base.CanCreateKeyIdentifierClause<T>());
        }

        internal SecurityContextSecurityToken Clone()
        {
            this.ThrowIfDisposed();
            return new SecurityContextSecurityToken(this);
        }

        public static SecurityContextSecurityToken CreateCookieSecurityContextToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, SecurityStateEncoder securityStateEncoder)
        {
            return CreateCookieSecurityContextToken(contextId, id, key, validFrom, validTo, null, validFrom, validTo, authorizationPolicies, securityStateEncoder);
        }

        public static SecurityContextSecurityToken CreateCookieSecurityContextToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, SecurityStateEncoder securityStateEncoder)
        {
            byte[] cookieBlob = new SecurityContextCookieSerializer(securityStateEncoder, null).CreateCookieFromSecurityContext(contextId, id, key, validFrom, validTo, keyGeneration, keyEffectiveTime, keyExpirationTime, authorizationPolicies);
            return new SecurityContextSecurityToken(contextId, id, key, validFrom, validTo, authorizationPolicies, true, cookieBlob, keyGeneration, keyEffectiveTime, keyExpirationTime);
        }

        public override T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            if (typeof(T) == typeof(SecurityContextKeyIdentifierClause))
            {
                return (new SecurityContextKeyIdentifierClause(this.contextId, this.keyGeneration) as T);
            }
            return base.CreateKeyIdentifierClause<T>();
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                System.IdentityModel.SecurityUtils.DisposeAuthorizationPoliciesIfNecessary(this.authorizationPolicies);
                if (this.bootstrapMessageProperty != null)
                {
                    this.bootstrapMessageProperty.Dispose();
                }
            }
        }

        internal string GetBase64KeyString()
        {
            if (this.keyString == null)
            {
                this.keyString = Convert.ToBase64String(this.key);
            }
            return this.keyString;
        }

        internal byte[] GetKeyBytes()
        {
            byte[] dst = new byte[this.key.Length];
            Buffer.BlockCopy(this.key, 0, dst, 0, this.key.Length);
            return dst;
        }

        private void Initialize(UniqueId contextId, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime)
        {
            if (contextId == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }
            if ((key == null) || (key.Length == 0))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            DateTime time = validFrom.ToUniversalTime();
            DateTime time2 = validTo.ToUniversalTime();
            if (time > time2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("validFrom", System.ServiceModel.SR.GetString("EffectiveGreaterThanExpiration"));
            }
            this.tokenEffectiveTime = time;
            this.tokenExpirationTime = time2;
            this.keyEffectiveTime = keyEffectiveTime.ToUniversalTime();
            this.keyExpirationTime = keyExpirationTime.ToUniversalTime();
            if (this.keyEffectiveTime > this.keyExpirationTime)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("keyEffectiveTime", System.ServiceModel.SR.GetString("EffectiveGreaterThanExpiration"));
            }
            if ((this.keyEffectiveTime < time) || (this.keyExpirationTime > time2))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("KeyLifetimeNotWithinTokenLifetime"));
            }
            this.key = new byte[key.Length];
            Buffer.BlockCopy(key, 0, this.key, 0, key.Length);
            this.contextId = contextId;
            this.keyGeneration = keyGeneration;
            this.authorizationPolicies = authorizationPolicies ?? System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            this.securityKeys = new List<SecurityKey>(1) { new InMemorySymmetricSecurityKey(this.key, 0) }.AsReadOnly();
            this.isCookieMode = isCookieMode;
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SecurityContextKeyIdentifierClause clause = keyIdentifierClause as SecurityContextKeyIdentifierClause;
            if (clause != null)
            {
                return clause.Matches(this.contextId, this.keyGeneration);
            }
            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "SecurityContextSecurityToken(Identifier='{0}', KeyGeneration='{1}')", new object[] { this.contextId, this.keyGeneration });
        }

        public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies
        {
            get
            {
                this.ThrowIfDisposed();
                return this.authorizationPolicies;
            }
        }

        public SecurityMessageProperty BootstrapMessageProperty
        {
            get
            {
                return this.bootstrapMessageProperty;
            }
            set
            {
                this.bootstrapMessageProperty = value;
            }
        }

        public UniqueId ContextId
        {
            get
            {
                return this.contextId;
            }
        }

        internal byte[] CookieBlob
        {
            get
            {
                return this.cookieBlob;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public bool IsCookieMode
        {
            get
            {
                return this.isCookieMode;
            }
        }

        public DateTime KeyEffectiveTime
        {
            get
            {
                return this.keyEffectiveTime;
            }
        }

        public DateTime KeyExpirationTime
        {
            get
            {
                return this.keyExpirationTime;
            }
        }

        public UniqueId KeyGeneration
        {
            get
            {
                return this.keyGeneration;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return this.securityKeys;
            }
        }

        DateTime TimeBoundedCache.IExpirableItem.ExpirationTime
        {
            get
            {
                return this.ValidTo;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return this.tokenEffectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return this.tokenExpirationTime;
            }
        }
    }
}

