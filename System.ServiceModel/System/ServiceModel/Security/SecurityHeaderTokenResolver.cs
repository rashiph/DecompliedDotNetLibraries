namespace System.ServiceModel.Security
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal sealed class SecurityHeaderTokenResolver : SecurityTokenResolver
    {
        private SecurityToken expectedWrapper;
        private SecurityTokenParameters expectedWrapperTokenParameters;
        private const int InitialTokenArraySize = 10;
        private ReceiveSecurityHeader securityHeader;
        private int tokenCount;
        private SecurityTokenEntry[] tokens;

        public SecurityHeaderTokenResolver() : this(null)
        {
        }

        public SecurityHeaderTokenResolver(ReceiveSecurityHeader securityHeader)
        {
            this.tokens = new SecurityTokenEntry[10];
            this.securityHeader = securityHeader;
        }

        public void Add(SecurityToken token)
        {
            this.Add(token, SecurityTokenReferenceStyle.Internal, null);
        }

        public void Add(SecurityToken token, SecurityTokenReferenceStyle allowedReferenceStyle, SecurityTokenParameters tokenParameters)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if ((allowedReferenceStyle == SecurityTokenReferenceStyle.External) && (tokenParameters == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ResolvingExternalTokensRequireSecurityTokenParameters"));
            }
            this.EnsureCapacityToAddToken();
            this.tokens[this.tokenCount++] = new SecurityTokenEntry(token, tokenParameters, allowedReferenceStyle);
        }

        internal bool CheckExternalWrapperMatch(SecurityKeyIdentifier keyIdentifier)
        {
            if ((this.expectedWrapper != null) && (this.expectedWrapperTokenParameters != null))
            {
                for (int i = 0; i < keyIdentifier.Count; i++)
                {
                    if (this.expectedWrapperTokenParameters.MatchesKeyIdentifierClause(this.expectedWrapper, keyIdentifier[i], SecurityTokenReferenceStyle.External))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void EnsureCapacityToAddToken()
        {
            if (this.tokenCount == this.tokens.Length)
            {
                SecurityTokenEntry[] destinationArray = new SecurityTokenEntry[this.tokens.Length * 2];
                Array.Copy(this.tokens, 0, destinationArray, 0, this.tokenCount);
                this.tokens = destinationArray;
            }
        }

        private bool MatchDirectReference(SecurityToken token, SecurityKeyIdentifierClause keyClause)
        {
            LocalIdKeyIdentifierClause keyIdentifierClause = keyClause as LocalIdKeyIdentifierClause;
            if (keyIdentifierClause == null)
            {
                return false;
            }
            return token.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        private SecurityKey ResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, bool createIntrinsicKeys)
        {
            SecurityKey key;
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("keyIdentifierClause"));
            }
            for (int i = 0; i < this.tokenCount; i++)
            {
                key = this.tokens[i].Token.ResolveKeyIdentifierClause(keyIdentifierClause);
                if (key != null)
                {
                    return key;
                }
            }
            if (createIntrinsicKeys && System.ServiceModel.Security.SecurityUtils.TryCreateKeyFromIntrinsicKeyClause(keyIdentifierClause, this, out key))
            {
                return key;
            }
            return null;
        }

        internal SecurityToken ResolveToken(SecurityKeyIdentifier keyIdentifier, bool matchOnlyExternalTokens, bool resolveIntrinsicKeyClause)
        {
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            for (int i = 0; i < keyIdentifier.Count; i++)
            {
                SecurityToken token = this.ResolveToken(keyIdentifier[i], matchOnlyExternalTokens, resolveIntrinsicKeyClause);
                if (token != null)
                {
                    return token;
                }
            }
            return null;
        }

        internal SecurityToken ResolveToken(SecurityKeyIdentifierClause keyIdentifierClause, bool matchOnlyExternal, bool resolveIntrinsicKeyClause)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            SecurityToken token = null;
            for (int i = 0; i < this.tokenCount; i++)
            {
                if (!matchOnlyExternal || (this.tokens[i].AllowedReferenceStyle == SecurityTokenReferenceStyle.External))
                {
                    SecurityToken token2 = this.tokens[i].Token;
                    if ((this.tokens[i].TokenParameters != null) && this.tokens[i].TokenParameters.MatchesKeyIdentifierClause(token2, keyIdentifierClause, this.tokens[i].AllowedReferenceStyle))
                    {
                        token = token2;
                        break;
                    }
                    if (((this.tokens[i].TokenParameters == null) && (this.tokens[i].AllowedReferenceStyle == SecurityTokenReferenceStyle.Internal)) && this.MatchDirectReference(token2, keyIdentifierClause))
                    {
                        token = token2;
                        break;
                    }
                }
            }
            if ((token == null) && (keyIdentifierClause is EncryptedKeyIdentifierClause))
            {
                SecurityToken expectedWrapper;
                EncryptedKeyIdentifierClause keyClause = (EncryptedKeyIdentifierClause) keyIdentifierClause;
                SecurityKeyIdentifier encryptingKeyIdentifier = keyClause.EncryptingKeyIdentifier;
                if ((this.expectedWrapper != null) && this.CheckExternalWrapperMatch(encryptingKeyIdentifier))
                {
                    expectedWrapper = this.expectedWrapper;
                }
                else
                {
                    expectedWrapper = this.ResolveToken(encryptingKeyIdentifier, true, resolveIntrinsicKeyClause);
                }
                if (expectedWrapper != null)
                {
                    token = System.ServiceModel.Security.SecurityUtils.CreateTokenFromEncryptedKeyClause(keyClause, expectedWrapper);
                }
            }
            if (((token == null) && (keyIdentifierClause is X509RawDataKeyIdentifierClause)) && (!matchOnlyExternal && resolveIntrinsicKeyClause))
            {
                token = new X509SecurityToken(new X509Certificate2(((X509RawDataKeyIdentifierClause) keyIdentifierClause).GetX509RawData()));
            }
            byte[] derivationNonce = keyIdentifierClause.GetDerivationNonce();
            if ((token != null) && (derivationNonce != null))
            {
                if (System.ServiceModel.Security.SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(token) == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToDeriveKeyFromKeyInfoClause", new object[] { keyIdentifierClause, token })));
                }
                int length = (keyIdentifierClause.DerivationLength == 0) ? 0x20 : keyIdentifierClause.DerivationLength;
                if (length > this.securityHeader.MaxDerivedKeyLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("DerivedKeyLengthSpecifiedInImplicitDerivedKeyClauseTooLong", new object[] { keyIdentifierClause.ToString(), length, this.securityHeader.MaxDerivedKeyLength })));
                }
                bool flag = false;
                for (int j = 0; j < this.tokenCount; j++)
                {
                    DerivedKeySecurityToken token4 = this.tokens[j].Token as DerivedKeySecurityToken;
                    if (((token4 != null) && (token4.Length == length)) && (CryptoHelper.IsEqual(token4.Nonce, derivationNonce) && token4.TokenToDerive.MatchesKeyIdentifierClause(keyIdentifierClause)))
                    {
                        token = this.tokens[j].Token;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(this.securityHeader.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                    token = new DerivedKeySecurityToken(-1, 0, length, null, derivationNonce, token, keyIdentifierClause, keyDerivationAlgorithm, System.ServiceModel.Security.SecurityUtils.GenerateId());
                    ((DerivedKeySecurityToken) token).InitializeDerivedKey(length);
                    this.Add(token, SecurityTokenReferenceStyle.Internal, null);
                    this.securityHeader.EnsureDerivedKeyLimitNotReached();
                }
            }
            return token;
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.WriteLine("SecurityTokenResolver");
                writer.WriteLine("    (");
                writer.WriteLine("    TokenCount = {0},", this.tokenCount);
                for (int i = 0; i < this.tokenCount; i++)
                {
                    writer.WriteLine("    TokenEntry[{0}] = (AllowedReferenceStyle={1}, Token={2}, Parameters={3})", new object[] { i, this.tokens[i].AllowedReferenceStyle, this.tokens[i].Token.GetType(), this.tokens[i].TokenParameters });
                }
                writer.WriteLine("    )");
                return writer.ToString();
            }
        }

        internal bool TryResolveSecurityKey(SecurityKeyIdentifierClause keyIdentifierClause, bool createIntrinsicKeys, out SecurityKey key)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            key = this.ResolveSecurityKeyCore(keyIdentifierClause, createIntrinsicKeys);
            return (key != null);
        }

        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            key = this.ResolveSecurityKeyCore(keyIdentifierClause, true);
            return (key != null);
        }

        internal bool TryResolveToken(SecurityKeyIdentifier keyIdentifier, bool matchOnlyExternalTokens, bool resolveIntrinsicKeyClause, out SecurityToken token)
        {
            token = this.ResolveToken(keyIdentifier, matchOnlyExternalTokens, resolveIntrinsicKeyClause);
            return (token != null);
        }

        internal bool TryResolveToken(SecurityKeyIdentifierClause keyIdentifierClause, bool matchOnlyExternalTokens, bool resolveIntrinsicKeyClause, out SecurityToken token)
        {
            token = this.ResolveToken(keyIdentifierClause, matchOnlyExternalTokens, resolveIntrinsicKeyClause);
            return (token != null);
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            token = this.ResolveToken(keyIdentifier, false, true);
            return (token != null);
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            token = this.ResolveToken(keyIdentifierClause, false, true);
            return (token != null);
        }

        public SecurityToken ExpectedWrapper
        {
            get
            {
                return this.expectedWrapper;
            }
            set
            {
                this.expectedWrapper = value;
            }
        }

        public SecurityTokenParameters ExpectedWrapperTokenParameters
        {
            get
            {
                return this.expectedWrapperTokenParameters;
            }
            set
            {
                this.expectedWrapperTokenParameters = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityTokenEntry
        {
            private SecurityTokenParameters tokenParameters;
            private SecurityToken token;
            private SecurityTokenReferenceStyle allowedReferenceStyle;
            public SecurityTokenEntry(SecurityToken token, SecurityTokenParameters tokenParameters, SecurityTokenReferenceStyle allowedReferenceStyle)
            {
                this.token = token;
                this.tokenParameters = tokenParameters;
                this.allowedReferenceStyle = allowedReferenceStyle;
            }

            public SecurityToken Token
            {
                get
                {
                    return this.token;
                }
            }
            public SecurityTokenParameters TokenParameters
            {
                get
                {
                    return this.tokenParameters;
                }
            }
            public SecurityTokenReferenceStyle AllowedReferenceStyle
            {
                get
                {
                    return this.allowedReferenceStyle;
                }
            }
        }
    }
}

