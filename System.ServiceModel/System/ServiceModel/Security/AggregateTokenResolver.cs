namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    internal class AggregateTokenResolver : SecurityTokenResolver
    {
        private ReadOnlyCollection<SecurityTokenResolver> outOfBandTokenResolvers;
        private SecurityHeaderTokenResolver tokenResolver;

        public AggregateTokenResolver(SecurityHeaderTokenResolver tokenResolver, ReadOnlyCollection<SecurityTokenResolver> outOfBandTokenResolvers)
        {
            if (tokenResolver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenResolver");
            }
            if (outOfBandTokenResolvers == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outOfBandTokenResolvers");
            }
            this.tokenResolver = tokenResolver;
            this.outOfBandTokenResolvers = outOfBandTokenResolvers;
        }

        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            bool flag = false;
            key = null;
            flag = this.tokenResolver.TryResolveSecurityKey(keyIdentifierClause, false, out key);
            if (!flag && (this.outOfBandTokenResolvers != null))
            {
                for (int i = 0; i < this.outOfBandTokenResolvers.Count; i++)
                {
                    flag = this.outOfBandTokenResolvers[i].TryResolveSecurityKey(keyIdentifierClause, out key);
                    if (flag)
                    {
                        break;
                    }
                }
            }
            if (!flag)
            {
                flag = System.ServiceModel.Security.SecurityUtils.TryCreateKeyFromIntrinsicKeyClause(keyIdentifierClause, this, out key);
            }
            return flag;
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            bool flag = false;
            token = null;
            flag = this.tokenResolver.TryResolveToken(keyIdentifier, false, false, out token);
            if (!flag && (this.outOfBandTokenResolvers != null))
            {
                for (int i = 0; i < this.outOfBandTokenResolvers.Count; i++)
                {
                    flag = this.outOfBandTokenResolvers[i].TryResolveToken(keyIdentifier, out token);
                    if (flag)
                    {
                        break;
                    }
                }
            }
            if (!flag)
            {
                for (int j = 0; j < keyIdentifier.Count; j++)
                {
                    if (this.TryResolveTokenFromIntrinsicKeyClause(keyIdentifier[j], out token))
                    {
                        return true;
                    }
                }
            }
            return flag;
        }

        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            bool flag = false;
            token = null;
            flag = this.tokenResolver.TryResolveToken(keyIdentifierClause, false, false, out token);
            if (!flag && (this.outOfBandTokenResolvers != null))
            {
                for (int i = 0; i < this.outOfBandTokenResolvers.Count; i++)
                {
                    flag = this.outOfBandTokenResolvers[i].TryResolveToken(keyIdentifierClause, out token);
                    if (flag)
                    {
                        break;
                    }
                }
            }
            if (!flag)
            {
                flag = this.TryResolveTokenFromIntrinsicKeyClause(keyIdentifierClause, out token);
            }
            return flag;
        }

        private bool TryResolveTokenFromIntrinsicKeyClause(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            token = null;
            if (keyIdentifierClause is RsaKeyIdentifierClause)
            {
                token = new RsaSecurityToken(((RsaKeyIdentifierClause) keyIdentifierClause).Rsa);
                return true;
            }
            if (keyIdentifierClause is X509RawDataKeyIdentifierClause)
            {
                token = new X509SecurityToken(new X509Certificate2(((X509RawDataKeyIdentifierClause) keyIdentifierClause).GetX509RawData()), false);
                return true;
            }
            if (keyIdentifierClause is EncryptedKeyIdentifierClause)
            {
                SecurityToken token2;
                EncryptedKeyIdentifierClause keyClause = (EncryptedKeyIdentifierClause) keyIdentifierClause;
                SecurityKeyIdentifier encryptingKeyIdentifier = keyClause.EncryptingKeyIdentifier;
                if (base.TryResolveToken(encryptingKeyIdentifier, out token2))
                {
                    token = System.ServiceModel.Security.SecurityUtils.CreateTokenFromEncryptedKeyClause(keyClause, token2);
                    return true;
                }
            }
            return false;
        }

        public ReadOnlyCollection<SecurityTokenResolver> OutOfBandTokenResolver
        {
            get
            {
                return this.outOfBandTokenResolvers;
            }
        }
    }
}

