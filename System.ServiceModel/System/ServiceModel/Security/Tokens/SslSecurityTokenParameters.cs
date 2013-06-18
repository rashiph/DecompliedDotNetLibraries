namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Channels;
    using System.Text;

    public class SslSecurityTokenParameters : SecurityTokenParameters
    {
        internal const bool defaultRequireCancellation = false;
        internal const bool defaultRequireClientCertificate = false;
        private BindingContext issuerBindingContext;
        private bool requireCancellation;
        private bool requireClientCertificate;

        public SslSecurityTokenParameters() : this(false)
        {
        }

        public SslSecurityTokenParameters(bool requireClientCertificate) : this(requireClientCertificate, false)
        {
        }

        protected SslSecurityTokenParameters(SslSecurityTokenParameters other) : base(other)
        {
            this.requireClientCertificate = other.requireClientCertificate;
            this.requireCancellation = other.requireCancellation;
            if (other.issuerBindingContext != null)
            {
                this.issuerBindingContext = other.issuerBindingContext.Clone();
            }
        }

        public SslSecurityTokenParameters(bool requireClientCertificate, bool requireCancellation)
        {
            this.requireClientCertificate = requireClientCertificate;
            this.requireCancellation = requireCancellation;
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SslSecurityTokenParameters(this);
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
            requirement.TokenType = this.RequireClientCertificate ? ServiceModelSecurityTokenTypes.MutualSslnego : ServiceModelSecurityTokenTypes.AnonymousSslnego;
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
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "RequireCancellation: {0}", new object[] { this.RequireCancellation.ToString() }));
            builder.Append(string.Format(CultureInfo.InvariantCulture, "RequireClientCertificate: {0}", new object[] { this.RequireClientCertificate.ToString() }));
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

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
            set
            {
                this.requireClientCertificate = value;
            }
        }

        protected internal override bool SupportsClientAuthentication
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        protected internal override bool SupportsClientWindowsIdentity
        {
            get
            {
                return this.requireClientCertificate;
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

