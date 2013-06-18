namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;

    public abstract class SecurityTokenResolver
    {
        protected SecurityTokenResolver()
        {
        }

        public static SecurityTokenResolver CreateDefaultSecurityTokenResolver(ReadOnlyCollection<SecurityToken> tokens, bool canMatchLocalId)
        {
            return new SimpleTokenResolver(tokens, canMatchLocalId);
        }

        public SecurityKey ResolveSecurityKey(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SecurityKey key;
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            if (!this.TryResolveSecurityKeyCore(keyIdentifierClause, out key))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnableToResolveKeyReference", new object[] { keyIdentifierClause })));
            }
            return key;
        }

        public SecurityToken ResolveToken(SecurityKeyIdentifier keyIdentifier)
        {
            SecurityToken token;
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            if (!this.TryResolveTokenCore(keyIdentifier, out token))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnableToResolveTokenReference", new object[] { keyIdentifier })));
            }
            return token;
        }

        public SecurityToken ResolveToken(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SecurityToken token;
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            if (!this.TryResolveTokenCore(keyIdentifierClause, out token))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnableToResolveTokenReference", new object[] { keyIdentifierClause })));
            }
            return token;
        }

        public bool TryResolveSecurityKey(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            return this.TryResolveSecurityKeyCore(keyIdentifierClause, out key);
        }

        protected abstract bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key);
        public bool TryResolveToken(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            return this.TryResolveTokenCore(keyIdentifier, out token);
        }

        public bool TryResolveToken(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            return this.TryResolveTokenCore(keyIdentifierClause, out token);
        }

        protected abstract bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token);
        protected abstract bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token);

        private class SimpleTokenResolver : SecurityTokenResolver
        {
            private bool canMatchLocalId;
            private ReadOnlyCollection<SecurityToken> tokens;

            public SimpleTokenResolver(ReadOnlyCollection<SecurityToken> tokens, bool canMatchLocalId)
            {
                if (tokens == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokens");
                }
                this.tokens = tokens;
                this.canMatchLocalId = canMatchLocalId;
            }

            private SecurityToken ResolveSecurityToken(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                if (keyIdentifierClause == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
                }
                if (this.canMatchLocalId || !(keyIdentifierClause is LocalIdKeyIdentifierClause))
                {
                    for (int i = 0; i < this.tokens.Count; i++)
                    {
                        if (this.tokens[i].MatchesKeyIdentifierClause(keyIdentifierClause))
                        {
                            return this.tokens[i];
                        }
                    }
                }
                return null;
            }

            protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
            {
                if (keyIdentifierClause == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
                }
                key = null;
                for (int i = 0; i < this.tokens.Count; i++)
                {
                    SecurityKey key2 = this.tokens[i].ResolveKeyIdentifierClause(keyIdentifierClause);
                    if (key2 != null)
                    {
                        key = key2;
                        return true;
                    }
                }
                if (keyIdentifierClause is EncryptedKeyIdentifierClause)
                {
                    EncryptedKeyIdentifierClause clause = (EncryptedKeyIdentifierClause) keyIdentifierClause;
                    SecurityKeyIdentifier encryptingKeyIdentifier = clause.EncryptingKeyIdentifier;
                    if ((encryptingKeyIdentifier != null) && (encryptingKeyIdentifier.Count > 0))
                    {
                        for (int j = 0; j < encryptingKeyIdentifier.Count; j++)
                        {
                            SecurityKey key3 = null;
                            if (base.TryResolveSecurityKey(encryptingKeyIdentifier[j], out key3))
                            {
                                byte[] encryptedKey = clause.GetEncryptedKey();
                                string encryptionMethod = clause.EncryptionMethod;
                                byte[] symmetricKey = key3.DecryptKey(encryptionMethod, encryptedKey);
                                key = new InMemorySymmetricSecurityKey(symmetricKey, false);
                                return true;
                            }
                        }
                    }
                }
                return (key != null);
            }

            protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
            {
                if (keyIdentifier == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
                }
                token = null;
                for (int i = 0; i < keyIdentifier.Count; i++)
                {
                    SecurityToken token2 = this.ResolveSecurityToken(keyIdentifier[i]);
                    if (token2 != null)
                    {
                        token = token2;
                        break;
                    }
                }
                return (token != null);
            }

            protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
            {
                if (keyIdentifierClause == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
                }
                token = null;
                SecurityToken token2 = this.ResolveSecurityToken(keyIdentifierClause);
                if (token2 != null)
                {
                    token = token2;
                }
                return (token != null);
            }
        }
    }
}

