namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    public class RsaSecurityTokenParameters : SecurityTokenParameters
    {
        public RsaSecurityTokenParameters()
        {
            base.InclusionMode = SecurityTokenInclusionMode.Never;
        }

        protected RsaSecurityTokenParameters(RsaSecurityTokenParameters other) : base(other)
        {
            base.InclusionMode = SecurityTokenInclusionMode.Never;
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new RsaSecurityTokenParameters(this);
        }

        protected internal override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            return base.CreateKeyIdentifierClause<RsaKeyIdentifierClause, RsaKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = SecurityTokenTypes.Rsa;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = SecurityKeyType.AsymmetricKey;
        }

        protected internal override bool HasAsymmetricKey
        {
            get
            {
                return true;
            }
        }

        protected internal override bool SupportsClientAuthentication
        {
            get
            {
                return true;
            }
        }

        protected internal override bool SupportsClientWindowsIdentity
        {
            get
            {
                return false;
            }
        }

        protected internal override bool SupportsServerAuthentication
        {
            get
            {
                return true;
            }
        }
    }
}

