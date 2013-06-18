namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    internal class WrappedKeySecurityTokenParameters : SecurityTokenParameters
    {
        public WrappedKeySecurityTokenParameters()
        {
            base.InclusionMode = SecurityTokenInclusionMode.Once;
        }

        protected WrappedKeySecurityTokenParameters(WrappedKeySecurityTokenParameters other) : base(other)
        {
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new WrappedKeySecurityTokenParameters(this);
        }

        protected internal override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            return base.CreateKeyIdentifierClause<EncryptedKeyHashIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
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
                return false;
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

