namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class DerivedKeyCachingSecurityTokenSerializer : SecurityTokenSerializer
    {
        private DerivedKeySecurityTokenCache[] cachedTokens;
        private int indexToCache;
        private SecurityTokenSerializer innerTokenSerializer;
        private bool isInitiator;
        private WSSecureConversation secureConversation;
        private object thisLock;

        internal DerivedKeyCachingSecurityTokenSerializer(int cacheSize, bool isInitiator, WSSecureConversation secureConversation, SecurityTokenSerializer innerTokenSerializer)
        {
            if (innerTokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerTokenSerializer");
            }
            if (secureConversation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("secureConversation");
            }
            if (cacheSize <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("cacheSize", System.ServiceModel.SR.GetString("ValueMustBeGreaterThanZero")));
            }
            this.cachedTokens = new DerivedKeySecurityTokenCache[cacheSize];
            this.isInitiator = isInitiator;
            this.secureConversation = secureConversation;
            this.innerTokenSerializer = innerTokenSerializer;
            this.thisLock = new object();
        }

        protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
        {
            return this.innerTokenSerializer.CanReadKeyIdentifierClause(reader);
        }

        protected override bool CanReadKeyIdentifierCore(XmlReader reader)
        {
            return this.innerTokenSerializer.CanReadKeyIdentifier(reader);
        }

        protected override bool CanReadTokenCore(XmlReader reader)
        {
            return this.innerTokenSerializer.CanReadToken(reader);
        }

        protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return this.innerTokenSerializer.CanWriteKeyIdentifierClause(keyIdentifierClause);
        }

        protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
        {
            return this.innerTokenSerializer.CanWriteKeyIdentifier(keyIdentifier);
        }

        protected override bool CanWriteTokenCore(SecurityToken token)
        {
            return this.innerTokenSerializer.CanWriteToken(token);
        }

        private DerivedKeySecurityToken GetCachedToken(string id, int generation, int offset, int length, string label, byte[] nonce, SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm)
        {
            for (int i = 0; i < this.cachedTokens.Length; i++)
            {
                DerivedKeySecurityTokenCache cachedToken = this.cachedTokens[i];
                if ((cachedToken != null) && this.IsMatch(cachedToken, id, generation, offset, length, label, nonce, tokenToDerive, derivationAlgorithm))
                {
                    DerivedKeySecurityToken token = new DerivedKeySecurityToken(generation, offset, length, label, nonce, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm, id);
                    token.InitializeDerivedKey(cachedToken.SecurityKeys);
                    return token;
                }
            }
            return null;
        }

        private bool IsMatch(DerivedKeySecurityTokenCache cachedToken, string id, int generation, int offset, int length, string label, byte[] nonce, SecurityToken tokenToDerive, string derivationAlgorithm)
        {
            if ((((cachedToken.Generation != generation) || (cachedToken.Offset != offset)) || ((cachedToken.Length != length) || !(cachedToken.Label == label))) || !(cachedToken.KeyDerivationAlgorithm == derivationAlgorithm))
            {
                return false;
            }
            if (!cachedToken.IsSourceKeyEqual(tokenToDerive))
            {
                return false;
            }
            return (CryptoHelper.IsEqual(cachedToken.Nonce, nonce) && (cachedToken.SecurityKeys != null));
        }

        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
        {
            return this.innerTokenSerializer.ReadKeyIdentifierClause(reader);
        }

        protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
        {
            return this.innerTokenSerializer.ReadKeyIdentifier(reader);
        }

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            if (this.secureConversation.IsAtDerivedKeyToken(reader2))
            {
                string str;
                string str2;
                string str3;
                int num;
                byte[] buffer;
                int num2;
                int num3;
                SecurityKeyIdentifierClause clause;
                SecurityToken token;
                this.secureConversation.ReadDerivedKeyTokenParameters(reader2, tokenResolver, out str, out str2, out str3, out num, out buffer, out num2, out num3, out clause, out token);
                DerivedKeySecurityToken token2 = this.GetCachedToken(str, num3, num2, num, str3, buffer, token, clause, str2);
                if (token2 != null)
                {
                    return token2;
                }
                lock (this.thisLock)
                {
                    token2 = this.GetCachedToken(str, num3, num2, num, str3, buffer, token, clause, str2);
                    if (token2 != null)
                    {
                        return token2;
                    }
                    SecurityToken token3 = this.secureConversation.CreateDerivedKeyToken(str, str2, str3, num, buffer, num2, num3, clause, token);
                    DerivedKeySecurityToken cachedToken = token3 as DerivedKeySecurityToken;
                    if (cachedToken != null)
                    {
                        int indexToCache = this.indexToCache;
                        if (this.indexToCache == 0x7fffffff)
                        {
                            this.indexToCache = 0;
                        }
                        else
                        {
                            this.indexToCache = ++this.indexToCache % this.cachedTokens.Length;
                        }
                        this.cachedTokens[indexToCache] = new DerivedKeySecurityTokenCache(cachedToken);
                    }
                    return token3;
                }
            }
            return this.innerTokenSerializer.ReadToken(reader, tokenResolver);
        }

        protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            this.innerTokenSerializer.WriteKeyIdentifierClause(writer, keyIdentifierClause);
        }

        protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            this.innerTokenSerializer.WriteKeyIdentifier(writer, keyIdentifier);
        }

        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            this.innerTokenSerializer.WriteToken(writer, token);
        }

        private class DerivedKeySecurityTokenCache
        {
            private DerivedKeySecurityToken cachedToken;
            private int generation;
            private string keyDerivationAlgorithm;
            private ReadOnlyCollection<SecurityKey> keys;
            private byte[] keyToDerive;
            private string label;
            private int length;
            private byte[] nonce;
            private int offset;

            public DerivedKeySecurityTokenCache(DerivedKeySecurityToken cachedToken)
            {
                this.keyToDerive = ((SymmetricSecurityKey) cachedToken.TokenToDerive.SecurityKeys[0]).GetSymmetricKey();
                this.generation = cachedToken.Generation;
                this.offset = cachedToken.Offset;
                this.length = cachedToken.Length;
                this.label = cachedToken.Label;
                this.keyDerivationAlgorithm = cachedToken.KeyDerivationAlgorithm;
                this.nonce = cachedToken.Nonce;
                this.cachedToken = cachedToken;
            }

            public bool IsSourceKeyEqual(SecurityToken token)
            {
                if (token.SecurityKeys.Count != 1)
                {
                    return false;
                }
                SymmetricSecurityKey key = token.SecurityKeys[0] as SymmetricSecurityKey;
                if (key == null)
                {
                    return false;
                }
                return CryptoHelper.IsEqual(this.keyToDerive, key.GetSymmetricKey());
            }

            public int Generation
            {
                get
                {
                    return this.generation;
                }
            }

            public string KeyDerivationAlgorithm
            {
                get
                {
                    return this.keyDerivationAlgorithm;
                }
            }

            public string Label
            {
                get
                {
                    return this.label;
                }
            }

            public int Length
            {
                get
                {
                    return this.length;
                }
            }

            public byte[] Nonce
            {
                get
                {
                    return this.nonce;
                }
            }

            public int Offset
            {
                get
                {
                    return this.offset;
                }
            }

            public ReadOnlyCollection<SecurityKey> SecurityKeys
            {
                get
                {
                    lock (this)
                    {
                        ReadOnlyCollection<SecurityKey> onlys;
                        if ((this.keys == null) && this.cachedToken.TryGetSecurityKeys(out onlys))
                        {
                            this.keys = onlys;
                            this.cachedToken = null;
                        }
                    }
                    return this.keys;
                }
            }
        }
    }
}

