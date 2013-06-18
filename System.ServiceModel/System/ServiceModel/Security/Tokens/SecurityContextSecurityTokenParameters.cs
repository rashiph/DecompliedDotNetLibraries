namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    internal class SecurityContextSecurityTokenParameters : SecurityTokenParameters
    {
        public SecurityContextSecurityTokenParameters()
        {
            base.InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
        }

        protected SecurityContextSecurityTokenParameters(SecurityContextSecurityTokenParameters other) : base(other)
        {
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SecurityContextSecurityTokenParameters(this);
        }

        protected internal override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            return base.CreateKeyIdentifierClause<SecurityContextKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = ServiceModelSecurityTokenTypes.SecurityContext;
            requirement.KeyType = SecurityKeyType.SymmetricKey;
            requirement.RequireCryptographicToken = true;
        }

        protected internal override bool HasAsymmetricKey
        {
            get
            {
                return false;
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
                return true;
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

