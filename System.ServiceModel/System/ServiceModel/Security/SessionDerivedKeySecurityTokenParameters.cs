namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class SessionDerivedKeySecurityTokenParameters : SecurityTokenParameters
    {
        private bool actAsInitiator;

        public SessionDerivedKeySecurityTokenParameters(bool actAsInitiator)
        {
            this.actAsInitiator = actAsInitiator;
            base.InclusionMode = actAsInitiator ? SecurityTokenInclusionMode.AlwaysToRecipient : SecurityTokenInclusionMode.AlwaysToInitiator;
            base.RequireDerivedKeys = false;
        }

        protected SessionDerivedKeySecurityTokenParameters(SessionDerivedKeySecurityTokenParameters other) : base(other)
        {
            this.actAsInitiator = other.actAsInitiator;
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SessionDerivedKeySecurityTokenParameters(this);
        }

        protected internal override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (referenceStyle == SecurityTokenReferenceStyle.Internal)
            {
                return token.CreateKeyIdentifierClause<LocalIdKeyIdentifierClause>();
            }
            return null;
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        protected internal override bool MatchesKeyIdentifierClause(SecurityToken token, SecurityKeyIdentifierClause keyIdentifierClause, SecurityTokenReferenceStyle referenceStyle)
        {
            if (referenceStyle != SecurityTokenReferenceStyle.Internal)
            {
                return false;
            }
            LocalIdKeyIdentifierClause clause = keyIdentifierClause as LocalIdKeyIdentifierClause;
            if (clause == null)
            {
                return false;
            }
            return (clause.LocalId == token.Id);
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
                return false;
            }
        }
    }
}

