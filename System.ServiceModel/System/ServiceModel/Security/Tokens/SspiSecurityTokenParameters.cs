namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Channels;
    using System.Text;

    public class SspiSecurityTokenParameters : SecurityTokenParameters
    {
        internal const bool defaultRequireCancellation = false;
        private BindingContext issuerBindingContext;
        private bool requireCancellation;

        public SspiSecurityTokenParameters() : this(false)
        {
        }

        public SspiSecurityTokenParameters(bool requireCancellation)
        {
            this.requireCancellation = requireCancellation;
        }

        protected SspiSecurityTokenParameters(SspiSecurityTokenParameters other) : base(other)
        {
            this.requireCancellation = other.requireCancellation;
            if (other.issuerBindingContext != null)
            {
                this.issuerBindingContext = other.issuerBindingContext.Clone();
            }
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SspiSecurityTokenParameters(this);
        }

        protected internal override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (token is GenericXmlSecurityToken)
            {
                return base.CreateGenericXmlTokenKeyIdentifierClause(token, referenceStyle);
            }
            return base.CreateKeyIdentifierClause<SecurityContextKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = ServiceModelSecurityTokenTypes.Spnego;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = SecurityKeyType.SymmetricKey;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportSecurityContextCancellationProperty] = this.RequireCancellation;
            if (this.IssuerBindingContext != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty] = this.IssuerBindingContext.Clone();
            }
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty] = base.Clone();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.Append(string.Format(CultureInfo.InvariantCulture, "RequireCancellation: {0}", new object[] { this.RequireCancellation.ToString() }));
            return builder.ToString();
        }

        protected internal override bool HasAsymmetricKey
        {
            get
            {
                return false;
            }
        }

        internal BindingContext IssuerBindingContext
        {
            get
            {
                return this.issuerBindingContext;
            }
            set
            {
                if (value != null)
                {
                    value = value.Clone();
                }
                this.issuerBindingContext = value;
            }
        }

        public bool RequireCancellation
        {
            get
            {
                return this.requireCancellation;
            }
            set
            {
                this.requireCancellation = value;
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

