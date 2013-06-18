namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Text;

    public abstract class SecurityTokenParameters
    {
        internal const SecurityTokenInclusionMode defaultInclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
        internal const SecurityTokenReferenceStyle defaultReferenceStyle = SecurityTokenReferenceStyle.Internal;
        internal const bool defaultRequireDerivedKeys = true;
        private SecurityTokenInclusionMode inclusionMode;
        private SecurityTokenReferenceStyle referenceStyle;
        private bool requireDerivedKeys;

        protected SecurityTokenParameters()
        {
            this.requireDerivedKeys = true;
        }

        protected SecurityTokenParameters(SecurityTokenParameters other)
        {
            this.requireDerivedKeys = true;
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            }
            this.requireDerivedKeys = other.requireDerivedKeys;
            this.inclusionMode = other.inclusionMode;
            this.referenceStyle = other.referenceStyle;
        }

        public SecurityTokenParameters Clone()
        {
            SecurityTokenParameters parameters = this.CloneCore();
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityTokenParametersCloneInvalidResult", new object[] { base.GetType().ToString() })));
            }
            return parameters;
        }

        protected abstract SecurityTokenParameters CloneCore();
        internal SecurityKeyIdentifierClause CreateGenericXmlTokenKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            GenericXmlSecurityToken token2 = token as GenericXmlSecurityToken;
            if (token2 != null)
            {
                if ((referenceStyle == SecurityTokenReferenceStyle.Internal) && (token2.InternalTokenReference != null))
                {
                    return token2.InternalTokenReference;
                }
                if ((referenceStyle == SecurityTokenReferenceStyle.External) && (token2.ExternalTokenReference != null))
                {
                    return token2.ExternalTokenReference;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToCreateTokenReference")));
        }

        protected internal abstract SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle);
        internal SecurityKeyIdentifierClause CreateKeyIdentifierClause<TExternalClause, TInternalClause>(SecurityToken token, SecurityTokenReferenceStyle referenceStyle) where TExternalClause: SecurityKeyIdentifierClause where TInternalClause: SecurityKeyIdentifierClause
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            switch (referenceStyle)
            {
                case SecurityTokenReferenceStyle.Internal:
                    return token.CreateKeyIdentifierClause<TInternalClause>();

                case SecurityTokenReferenceStyle.External:
                    return token.CreateKeyIdentifierClause<TExternalClause>();
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("TokenDoesNotSupportKeyIdentifierClauseCreation", new object[] { token.GetType().Name, referenceStyle })));
        }

        protected internal abstract void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement);
        internal bool MatchesGenericXmlTokenKeyIdentifierClause(SecurityToken token, SecurityKeyIdentifierClause keyIdentifierClause, SecurityTokenReferenceStyle referenceStyle)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            GenericXmlSecurityToken token2 = token as GenericXmlSecurityToken;
            if (token2 == null)
            {
                return false;
            }
            if ((referenceStyle == SecurityTokenReferenceStyle.External) && (token2.ExternalTokenReference != null))
            {
                return token2.ExternalTokenReference.Matches(keyIdentifierClause);
            }
            return ((referenceStyle == SecurityTokenReferenceStyle.Internal) && token2.MatchesKeyIdentifierClause(keyIdentifierClause));
        }

        protected internal virtual bool MatchesKeyIdentifierClause(SecurityToken token, SecurityKeyIdentifierClause keyIdentifierClause, SecurityTokenReferenceStyle referenceStyle)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (token is GenericXmlSecurityToken)
            {
                return this.MatchesGenericXmlTokenKeyIdentifierClause(token, keyIdentifierClause, referenceStyle);
            }
            switch (referenceStyle)
            {
                case SecurityTokenReferenceStyle.Internal:
                    return token.MatchesKeyIdentifierClause(keyIdentifierClause);

                case SecurityTokenReferenceStyle.External:
                    if (keyIdentifierClause is LocalIdKeyIdentifierClause)
                    {
                        return false;
                    }
                    return token.MatchesKeyIdentifierClause(keyIdentifierClause);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("TokenDoesNotSupportKeyIdentifierClauseCreation", new object[] { token.GetType().Name, referenceStyle })));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}:", new object[] { base.GetType().ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "InclusionMode: {0}", new object[] { this.inclusionMode.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "ReferenceStyle: {0}", new object[] { this.referenceStyle.ToString() }));
            builder.Append(string.Format(CultureInfo.InvariantCulture, "RequireDerivedKeys: {0}", new object[] { this.requireDerivedKeys.ToString() }));
            return builder.ToString();
        }

        protected internal abstract bool HasAsymmetricKey { get; }

        public SecurityTokenInclusionMode InclusionMode
        {
            get
            {
                return this.inclusionMode;
            }
            set
            {
                SecurityTokenInclusionModeHelper.Validate(value);
                this.inclusionMode = value;
            }
        }

        public SecurityTokenReferenceStyle ReferenceStyle
        {
            get
            {
                return this.referenceStyle;
            }
            set
            {
                TokenReferenceStyleHelper.Validate(value);
                this.referenceStyle = value;
            }
        }

        public bool RequireDerivedKeys
        {
            get
            {
                return this.requireDerivedKeys;
            }
            set
            {
                this.requireDerivedKeys = value;
            }
        }

        protected internal abstract bool SupportsClientAuthentication { get; }

        protected internal abstract bool SupportsClientWindowsIdentity { get; }

        protected internal abstract bool SupportsServerAuthentication { get; }
    }
}

