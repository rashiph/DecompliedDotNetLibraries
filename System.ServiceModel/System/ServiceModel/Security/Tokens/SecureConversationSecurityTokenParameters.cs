namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Text;

    public class SecureConversationSecurityTokenParameters : SecurityTokenParameters
    {
        private ChannelProtectionRequirements bootstrapProtectionRequirements;
        private SecurityBindingElement bootstrapSecurityBindingElement;
        private bool canRenewSession;
        internal const bool defaultCanRenewSession = true;
        internal const bool defaultRequireCancellation = true;
        private BindingContext issuerBindingContext;
        private bool requireCancellation;

        public SecureConversationSecurityTokenParameters() : this(null, true, (ChannelProtectionRequirements) null)
        {
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement) : this(bootstrapSecurityBindingElement, true, (ChannelProtectionRequirements) null)
        {
        }

        protected SecureConversationSecurityTokenParameters(SecureConversationSecurityTokenParameters other) : base(other)
        {
            this.canRenewSession = true;
            this.requireCancellation = other.requireCancellation;
            this.canRenewSession = other.canRenewSession;
            if (other.bootstrapSecurityBindingElement != null)
            {
                this.bootstrapSecurityBindingElement = (SecurityBindingElement) other.bootstrapSecurityBindingElement.Clone();
            }
            if (other.bootstrapProtectionRequirements != null)
            {
                this.bootstrapProtectionRequirements = new ChannelProtectionRequirements(other.bootstrapProtectionRequirements);
            }
            if (other.issuerBindingContext != null)
            {
                this.issuerBindingContext = other.issuerBindingContext.Clone();
            }
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation) : this(bootstrapSecurityBindingElement, requireCancellation, true)
        {
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation, bool canRenewSession) : this(bootstrapSecurityBindingElement, requireCancellation, canRenewSession, null)
        {
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation, ChannelProtectionRequirements bootstrapProtectionRequirements) : this(bootstrapSecurityBindingElement, requireCancellation, true, null)
        {
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation, bool canRenewSession, ChannelProtectionRequirements bootstrapProtectionRequirements)
        {
            this.canRenewSession = true;
            this.bootstrapSecurityBindingElement = bootstrapSecurityBindingElement;
            this.canRenewSession = canRenewSession;
            if (bootstrapProtectionRequirements != null)
            {
                this.bootstrapProtectionRequirements = new ChannelProtectionRequirements(bootstrapProtectionRequirements);
            }
            else
            {
                this.bootstrapProtectionRequirements = new ChannelProtectionRequirements();
                this.bootstrapProtectionRequirements.IncomingEncryptionParts.AddParts(new MessagePartSpecification(true));
                this.bootstrapProtectionRequirements.IncomingSignatureParts.AddParts(new MessagePartSpecification(true));
                this.bootstrapProtectionRequirements.OutgoingEncryptionParts.AddParts(new MessagePartSpecification(true));
                this.bootstrapProtectionRequirements.OutgoingSignatureParts.AddParts(new MessagePartSpecification(true));
            }
            this.requireCancellation = requireCancellation;
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SecureConversationSecurityTokenParameters(this);
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
            requirement.TokenType = ServiceModelSecurityTokenTypes.SecureConversation;
            requirement.KeyType = SecurityKeyType.SymmetricKey;
            requirement.RequireCryptographicToken = true;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportSecurityContextCancellationProperty] = this.RequireCancellation;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SecureConversationSecurityBindingElementProperty] = this.BootstrapSecurityBindingElement;
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty] = this.IssuerBindingContext.Clone();
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty] = base.Clone();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "RequireCancellation: {0}", new object[] { this.requireCancellation.ToString() }));
            if (this.bootstrapSecurityBindingElement == null)
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "BootstrapSecurityBindingElement: null", new object[0]));
            }
            else
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "BootstrapSecurityBindingElement:", new object[0]));
                builder.AppendLine("  " + this.BootstrapSecurityBindingElement.ToString().Trim().Replace("\n", "\n  "));
            }
            return builder.ToString().Trim();
        }

        public ChannelProtectionRequirements BootstrapProtectionRequirements
        {
            get
            {
                return this.bootstrapProtectionRequirements;
            }
        }

        public SecurityBindingElement BootstrapSecurityBindingElement
        {
            get
            {
                return this.bootstrapSecurityBindingElement;
            }
            set
            {
                this.bootstrapSecurityBindingElement = value;
            }
        }

        private ISecurityCapabilities BootstrapSecurityCapabilities
        {
            get
            {
                return this.bootstrapSecurityBindingElement.GetIndividualProperty<ISecurityCapabilities>();
            }
        }

        public bool CanRenewSession
        {
            get
            {
                return this.canRenewSession;
            }
            set
            {
                this.canRenewSession = value;
            }
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
                return ((this.BootstrapSecurityCapabilities != null) && this.BootstrapSecurityCapabilities.SupportsClientAuthentication);
            }
        }

        protected internal override bool SupportsClientWindowsIdentity
        {
            get
            {
                return ((this.BootstrapSecurityCapabilities != null) && this.BootstrapSecurityCapabilities.SupportsClientWindowsIdentity);
            }
        }

        protected internal override bool SupportsServerAuthentication
        {
            get
            {
                return ((this.BootstrapSecurityCapabilities != null) && this.BootstrapSecurityCapabilities.SupportsServerAuthentication);
            }
        }
    }
}

