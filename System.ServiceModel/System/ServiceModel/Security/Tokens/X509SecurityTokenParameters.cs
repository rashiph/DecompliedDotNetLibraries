namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Text;

    public class X509SecurityTokenParameters : SecurityTokenParameters
    {
        internal const X509KeyIdentifierClauseType defaultX509ReferenceStyle = X509KeyIdentifierClauseType.Any;
        private X509KeyIdentifierClauseType x509ReferenceStyle;

        public X509SecurityTokenParameters() : this(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.AlwaysToRecipient)
        {
        }

        public X509SecurityTokenParameters(X509KeyIdentifierClauseType x509ReferenceStyle) : this(x509ReferenceStyle, SecurityTokenInclusionMode.AlwaysToRecipient)
        {
        }

        protected X509SecurityTokenParameters(X509SecurityTokenParameters other) : base(other)
        {
            this.x509ReferenceStyle = other.x509ReferenceStyle;
        }

        public X509SecurityTokenParameters(X509KeyIdentifierClauseType x509ReferenceStyle, SecurityTokenInclusionMode inclusionMode) : this(x509ReferenceStyle, inclusionMode, true)
        {
        }

        internal X509SecurityTokenParameters(X509KeyIdentifierClauseType x509ReferenceStyle, SecurityTokenInclusionMode inclusionMode, bool requireDerivedKeys)
        {
            this.X509ReferenceStyle = x509ReferenceStyle;
            base.InclusionMode = inclusionMode;
            base.RequireDerivedKeys = requireDerivedKeys;
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new X509SecurityTokenParameters(this);
        }

        protected internal override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            SecurityKeyIdentifierClause clause = null;
            switch (this.x509ReferenceStyle)
            {
                case X509KeyIdentifierClauseType.Thumbprint:
                    return base.CreateKeyIdentifierClause<X509ThumbprintKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);

                case X509KeyIdentifierClauseType.IssuerSerial:
                    return base.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);

                case X509KeyIdentifierClauseType.SubjectKeyIdentifier:
                    return base.CreateKeyIdentifierClause<X509SubjectKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);

                case X509KeyIdentifierClauseType.RawDataKeyIdentifier:
                    return base.CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
            }
            if (referenceStyle == SecurityTokenReferenceStyle.External)
            {
                X509SecurityToken token2 = token as X509SecurityToken;
                if (token2 != null)
                {
                    X509SubjectKeyIdentifierClause clause2;
                    if (X509SubjectKeyIdentifierClause.TryCreateFrom(token2.Certificate, out clause2))
                    {
                        clause = clause2;
                    }
                }
                else
                {
                    X509SubjectKeyIdentifierClause clause3;
                    X509WindowsSecurityToken token3 = token as X509WindowsSecurityToken;
                    if ((token3 != null) && X509SubjectKeyIdentifierClause.TryCreateFrom(token3.Certificate, out clause3))
                    {
                        clause = clause3;
                    }
                }
                if (clause == null)
                {
                    clause = token.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause>();
                }
                if (clause == null)
                {
                    clause = token.CreateKeyIdentifierClause<X509ThumbprintKeyIdentifierClause>();
                }
                return clause;
            }
            return token.CreateKeyIdentifierClause<LocalIdKeyIdentifierClause>();
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = SecurityTokenTypes.X509Certificate;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = SecurityKeyType.AsymmetricKey;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.Append(string.Format(CultureInfo.InvariantCulture, "X509ReferenceStyle: {0}", new object[] { this.x509ReferenceStyle.ToString() }));
            return builder.ToString();
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

        public X509KeyIdentifierClauseType X509ReferenceStyle
        {
            get
            {
                return this.x509ReferenceStyle;
            }
            set
            {
                X509SecurityTokenReferenceStyleHelper.Validate(value);
                this.x509ReferenceStyle = value;
            }
        }
    }
}

