namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    public class WrappedKeySecurityToken : SecurityToken
    {
        private DateTime effectiveTime;
        private System.ServiceModel.Security.EncryptedKey encryptedKey;
        private string id;
        private ReadOnlyCollection<SecurityKey> securityKey;
        private bool serializeCarriedKeyName;
        private byte[] wrappedKey;
        private byte[] wrappedKeyHash;
        private string wrappingAlgorithm;
        private XmlDictionaryString wrappingAlgorithmDictionaryString;
        private SecurityKey wrappingSecurityKey;
        private ISspiNegotiation wrappingSspiContext;
        private SecurityToken wrappingToken;
        private SecurityKeyIdentifier wrappingTokenReference;

        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, ISspiNegotiation wrappingSspiContext) : this(id, keyToWrap, (wrappingSspiContext != null) ? wrappingSspiContext.KeyEncryptionAlgorithm : null, wrappingSspiContext, null)
        {
        }

        private WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, XmlDictionaryString wrappingAlgorithmDictionaryString)
        {
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            if (wrappingAlgorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappingAlgorithm");
            }
            if (keyToWrap == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityKeyToWrap");
            }
            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            this.securityKey = System.ServiceModel.Security.SecurityUtils.CreateSymmetricSecurityKeys(keyToWrap);
            this.wrappingAlgorithm = wrappingAlgorithm;
            this.wrappingAlgorithmDictionaryString = wrappingAlgorithmDictionaryString;
        }

        public WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference) : this(id, keyToWrap, wrappingAlgorithm, null, wrappingToken, wrappingTokenReference)
        {
        }

        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, ISspiNegotiation wrappingSspiContext, byte[] wrappedKey) : this(id, keyToWrap, wrappingAlgorithm, null)
        {
            if (wrappingSspiContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappingSspiContext");
            }
            this.wrappingSspiContext = wrappingSspiContext;
            if (wrappedKey == null)
            {
                this.wrappedKey = wrappingSspiContext.Encrypt(keyToWrap);
            }
            else
            {
                this.wrappedKey = wrappedKey;
            }
            this.serializeCarriedKeyName = false;
        }

        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, XmlDictionaryString wrappingAlgorithmDictionaryString, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference) : this(id, keyToWrap, wrappingAlgorithm, wrappingAlgorithmDictionaryString, wrappingToken, wrappingTokenReference, null, null)
        {
        }

        internal WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference, byte[] wrappedKey, SecurityKey wrappingSecurityKey) : this(id, keyToWrap, wrappingAlgorithm, null, wrappingToken, wrappingTokenReference, wrappedKey, wrappingSecurityKey)
        {
        }

        private WrappedKeySecurityToken(string id, byte[] keyToWrap, string wrappingAlgorithm, XmlDictionaryString wrappingAlgorithmDictionaryString, SecurityToken wrappingToken, SecurityKeyIdentifier wrappingTokenReference, byte[] wrappedKey, SecurityKey wrappingSecurityKey) : this(id, keyToWrap, wrappingAlgorithm, wrappingAlgorithmDictionaryString)
        {
            if (wrappingToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("wrappingToken");
            }
            this.wrappingToken = wrappingToken;
            this.wrappingTokenReference = wrappingTokenReference;
            if (wrappedKey == null)
            {
                this.wrappedKey = System.ServiceModel.Security.SecurityUtils.EncryptKey(wrappingToken, wrappingAlgorithm, keyToWrap);
            }
            else
            {
                this.wrappedKey = wrappedKey;
            }
            this.wrappingSecurityKey = wrappingSecurityKey;
            this.serializeCarriedKeyName = true;
        }

        public override bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            return ((typeof(T) == typeof(EncryptedKeyHashIdentifierClause)) || base.CanCreateKeyIdentifierClause<T>());
        }

        public override T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            if (typeof(T) == typeof(EncryptedKeyHashIdentifierClause))
            {
                return (new EncryptedKeyHashIdentifierClause(this.GetHash()) as T);
            }
            return base.CreateKeyIdentifierClause<T>();
        }

        internal void EnsureEncryptedKeySetUp()
        {
            if (this.encryptedKey == null)
            {
                System.ServiceModel.Security.EncryptedKey key = new System.ServiceModel.Security.EncryptedKey {
                    Id = this.Id
                };
                if (this.serializeCarriedKeyName)
                {
                    key.CarriedKeyName = this.CarriedKeyName;
                }
                else
                {
                    key.CarriedKeyName = null;
                }
                key.EncryptionMethod = this.WrappingAlgorithm;
                key.EncryptionMethodDictionaryString = this.wrappingAlgorithmDictionaryString;
                key.SetUpKeyWrap(this.wrappedKey);
                if (this.WrappingTokenReference != null)
                {
                    key.KeyIdentifier = this.WrappingTokenReference;
                }
                this.encryptedKey = key;
            }
        }

        internal byte[] GetHash()
        {
            if (this.wrappedKeyHash == null)
            {
                this.EnsureEncryptedKeySetUp();
                using (HashAlgorithm algorithm = CryptoHelper.NewSha1HashAlgorithm())
                {
                    this.wrappedKeyHash = algorithm.ComputeHash(this.encryptedKey.GetWrappedKey());
                }
            }
            return this.wrappedKeyHash;
        }

        public byte[] GetWrappedKey()
        {
            return System.ServiceModel.Security.SecurityUtils.CloneBuffer(this.wrappedKey);
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            EncryptedKeyHashIdentifierClause clause = keyIdentifierClause as EncryptedKeyHashIdentifierClause;
            if (clause != null)
            {
                return clause.Matches(this.GetHash());
            }
            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        internal string CarriedKeyName
        {
            get
            {
                return null;
            }
        }

        internal System.ServiceModel.Security.EncryptedKey EncryptedKey
        {
            get
            {
                return this.encryptedKey;
            }
            set
            {
                this.encryptedKey = value;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        internal System.ServiceModel.Security.ReferenceList ReferenceList
        {
            get
            {
                if (this.encryptedKey != null)
                {
                    return this.encryptedKey.ReferenceList;
                }
                return null;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return this.securityKey;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return DateTime.MaxValue;
            }
        }

        public string WrappingAlgorithm
        {
            get
            {
                return this.wrappingAlgorithm;
            }
        }

        internal SecurityKey WrappingSecurityKey
        {
            get
            {
                return this.wrappingSecurityKey;
            }
        }

        public SecurityToken WrappingToken
        {
            get
            {
                return this.wrappingToken;
            }
        }

        public SecurityKeyIdentifier WrappingTokenReference
        {
            get
            {
                return this.wrappingTokenReference;
            }
        }
    }
}

