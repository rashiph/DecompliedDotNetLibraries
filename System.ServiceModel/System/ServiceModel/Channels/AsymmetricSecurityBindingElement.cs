namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    public sealed class AsymmetricSecurityBindingElement : SecurityBindingElement, IPolicyExportExtension
    {
        private bool allowSerializedSigningTokenOnReply;
        internal const bool defaultAllowSerializedSigningTokenOnReply = false;
        private SecurityTokenParameters initiatorTokenParameters;
        private bool isCertificateSignatureBinding;
        private System.ServiceModel.Security.MessageProtectionOrder messageProtectionOrder;
        private SecurityTokenParameters recipientTokenParameters;
        private bool requireSignatureConfirmation;

        public AsymmetricSecurityBindingElement() : this(null, null)
        {
        }

        private AsymmetricSecurityBindingElement(AsymmetricSecurityBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            if (elementToBeCloned.initiatorTokenParameters != null)
            {
                this.initiatorTokenParameters = elementToBeCloned.initiatorTokenParameters.Clone();
            }
            this.messageProtectionOrder = elementToBeCloned.messageProtectionOrder;
            if (elementToBeCloned.recipientTokenParameters != null)
            {
                this.recipientTokenParameters = elementToBeCloned.recipientTokenParameters.Clone();
            }
            this.requireSignatureConfirmation = elementToBeCloned.requireSignatureConfirmation;
            this.allowSerializedSigningTokenOnReply = elementToBeCloned.allowSerializedSigningTokenOnReply;
            this.isCertificateSignatureBinding = elementToBeCloned.isCertificateSignatureBinding;
        }

        public AsymmetricSecurityBindingElement(SecurityTokenParameters recipientTokenParameters) : this(recipientTokenParameters, null)
        {
        }

        public AsymmetricSecurityBindingElement(SecurityTokenParameters recipientTokenParameters, SecurityTokenParameters initiatorTokenParameters) : this(recipientTokenParameters, initiatorTokenParameters, false)
        {
        }

        internal AsymmetricSecurityBindingElement(SecurityTokenParameters recipientTokenParameters, SecurityTokenParameters initiatorTokenParameters, bool allowSerializedSigningTokenOnReply)
        {
            this.messageProtectionOrder = System.ServiceModel.Security.MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
            this.requireSignatureConfirmation = false;
            this.initiatorTokenParameters = initiatorTokenParameters;
            this.recipientTokenParameters = recipientTokenParameters;
            this.allowSerializedSigningTokenOnReply = allowSerializedSigningTokenOnReply;
            this.isCertificateSignatureBinding = false;
        }

        protected override IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context)
        {
            ISecurityCapabilities property = this.GetProperty<ISecurityCapabilities>(context);
            bool addChannelDemuxerIfRequired = this.RequiresChannelDemuxer();
            ChannelBuilder builder = new ChannelBuilder(context, addChannelDemuxerIfRequired);
            if (addChannelDemuxerIfRequired)
            {
                base.ApplyPropertiesOnDemuxer(builder, context);
            }
            BindingContext issuanceBindingContext = context.Clone();
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }
            return new SecurityChannelFactory<TChannel>(property, context, builder, this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, false, issuanceBindingContext));
        }

        protected override IChannelListener<TChannel> BuildChannelListenerCore<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            bool addChannelDemuxerIfRequired = this.RequiresChannelDemuxer();
            ChannelBuilder builder = new ChannelBuilder(context, addChannelDemuxerIfRequired);
            if (addChannelDemuxerIfRequired)
            {
                base.ApplyPropertiesOnDemuxer(builder, context);
            }
            BindingContext issuanceBindingContext = context.Clone();
            SecurityChannelListener<TChannel> listener = new SecurityChannelListener<TChannel>(this, context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ServiceCredentials.CreateDefaultCredentials();
            }
            SecurityProtocolFactory factory = this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, true, issuanceBindingContext);
            listener.SecurityProtocolFactory = factory;
            listener.InitializeListener(builder);
            return listener;
        }

        public override BindingElement Clone()
        {
            return new AsymmetricSecurityBindingElement(this);
        }

        internal override SecurityProtocolFactory CreateSecurityProtocolFactory<TChannel>(BindingContext context, SecurityCredentialsManager credentialsManager, bool isForService, BindingContext issuerBindingContext)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (credentialsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("credentialsManager");
            }
            if (this.InitiatorTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AsymmetricSecurityBindingElementNeedsInitiatorTokenParameters", new object[] { this.ToString() })));
            }
            if (this.RecipientTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AsymmetricSecurityBindingElementNeedsRecipientTokenParameters", new object[] { this.ToString() })));
            }
            bool flag = !this.isCertificateSignatureBinding && ((typeof(IDuplexChannel) == typeof(TChannel)) || (typeof(IDuplexSessionChannel) == typeof(TChannel)));
            AsymmetricSecurityProtocolFactory factory = new AsymmetricSecurityProtocolFactory();
            factory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, isForService));
            factory.RequireConfidentiality = this.HasProtectionRequirements(factory.ProtectionRequirements.IncomingEncryptionParts);
            factory.RequireIntegrity = this.HasProtectionRequirements(factory.ProtectionRequirements.IncomingSignatureParts);
            if (this.isCertificateSignatureBinding)
            {
                if (isForService)
                {
                    factory.ApplyIntegrity = factory.ApplyConfidentiality = false;
                }
                else
                {
                    factory.ApplyConfidentiality = factory.RequireIntegrity = false;
                }
            }
            else
            {
                factory.ApplyIntegrity = this.HasProtectionRequirements(factory.ProtectionRequirements.OutgoingSignatureParts);
                factory.ApplyConfidentiality = this.HasProtectionRequirements(factory.ProtectionRequirements.OutgoingEncryptionParts);
            }
            if (isForService)
            {
                base.ApplyAuditBehaviorSettings(context, factory);
                if (factory.RequireConfidentiality || (!this.isCertificateSignatureBinding && factory.ApplyIntegrity))
                {
                    factory.AsymmetricTokenParameters = this.RecipientTokenParameters.Clone();
                }
                else
                {
                    factory.AsymmetricTokenParameters = null;
                }
                factory.CryptoTokenParameters = this.InitiatorTokenParameters.Clone();
                SecurityBindingElement.SetIssuerBindingContextIfRequired(factory.CryptoTokenParameters, issuerBindingContext);
            }
            else
            {
                if (factory.ApplyConfidentiality || (!this.isCertificateSignatureBinding && factory.RequireIntegrity))
                {
                    factory.AsymmetricTokenParameters = this.RecipientTokenParameters.Clone();
                }
                else
                {
                    factory.AsymmetricTokenParameters = null;
                }
                factory.CryptoTokenParameters = this.InitiatorTokenParameters.Clone();
                SecurityBindingElement.SetIssuerBindingContextIfRequired(factory.CryptoTokenParameters, issuerBindingContext);
            }
            if (flag)
            {
                if (isForService)
                {
                    factory.ApplyConfidentiality = factory.ApplyIntegrity = false;
                }
                else
                {
                    factory.RequireIntegrity = factory.RequireConfidentiality = false;
                }
            }
            else if (!isForService)
            {
                factory.AllowSerializedSigningTokenOnReply = this.AllowSerializedSigningTokenOnReply;
            }
            factory.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
            factory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
            factory.MessageProtectionOrder = this.MessageProtectionOrder;
            base.ConfigureProtocolFactory(factory, credentialsManager, isForService, issuerBindingContext, context.Binding);
            if (!factory.RequireIntegrity)
            {
                factory.DetectReplays = false;
            }
            if (!flag)
            {
                return factory;
            }
            AsymmetricSecurityProtocolFactory factory3 = new AsymmetricSecurityProtocolFactory();
            if (isForService)
            {
                factory3.AsymmetricTokenParameters = this.InitiatorTokenParameters.Clone();
                factory3.AsymmetricTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.External;
                factory3.AsymmetricTokenParameters.InclusionMode = SecurityTokenInclusionMode.Never;
                factory3.CryptoTokenParameters = this.RecipientTokenParameters.Clone();
                factory3.CryptoTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.Internal;
                factory3.CryptoTokenParameters.InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
                factory3.IdentityVerifier = null;
            }
            else
            {
                factory3.AsymmetricTokenParameters = this.InitiatorTokenParameters.Clone();
                factory3.AsymmetricTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.External;
                factory3.AsymmetricTokenParameters.InclusionMode = SecurityTokenInclusionMode.Never;
                factory3.CryptoTokenParameters = this.RecipientTokenParameters.Clone();
                factory3.CryptoTokenParameters.ReferenceStyle = SecurityTokenReferenceStyle.Internal;
                factory3.CryptoTokenParameters.InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
                factory3.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
            }
            factory3.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
            factory3.MessageProtectionOrder = this.MessageProtectionOrder;
            factory3.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, isForService));
            if (isForService)
            {
                factory3.ApplyConfidentiality = this.HasProtectionRequirements(factory3.ProtectionRequirements.OutgoingEncryptionParts);
                factory3.ApplyIntegrity = true;
                factory3.RequireIntegrity = factory3.RequireConfidentiality = false;
            }
            else
            {
                factory3.RequireConfidentiality = this.HasProtectionRequirements(factory3.ProtectionRequirements.IncomingEncryptionParts);
                factory3.RequireIntegrity = true;
                factory3.ApplyIntegrity = factory3.ApplyConfidentiality = false;
            }
            base.ConfigureProtocolFactory(factory3, credentialsManager, !isForService, issuerBindingContext, context.Binding);
            if (!factory3.RequireIntegrity)
            {
                factory3.DetectReplays = false;
            }
            factory3.IsDuplexReply = true;
            return new DuplexSecurityProtocolFactory { ForwardProtocolFactory = factory, ReverseProtocolFactory = factory3 };
        }

        internal override ISecurityCapabilities GetIndividualISecurityCapabilities()
        {
            bool flag2;
            bool flag3;
            ProtectionLevel encryptAndSign = ProtectionLevel.EncryptAndSign;
            ProtectionLevel responseProtectionLevel = ProtectionLevel.EncryptAndSign;
            bool supportsServerAuth = false;
            if (this.IsCertificateSignatureBinding)
            {
                encryptAndSign = ProtectionLevel.Sign;
                responseProtectionLevel = ProtectionLevel.None;
            }
            else if (this.RecipientTokenParameters != null)
            {
                supportsServerAuth = this.RecipientTokenParameters.SupportsServerAuthentication;
            }
            base.GetSupportingTokensCapabilities(out flag2, out flag3);
            if (this.InitiatorTokenParameters != null)
            {
                flag2 = flag2 || this.InitiatorTokenParameters.SupportsClientAuthentication;
                flag3 = flag3 || this.InitiatorTokenParameters.SupportsClientWindowsIdentity;
            }
            return new SecurityCapabilities(flag2, supportsServerAuth, flag3, encryptAndSign, responseProtectionLevel);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!(typeof(T) == typeof(ChannelProtectionRequirements)))
            {
                return base.GetProperty<T>(context);
            }
            AddressingVersion addressing = MessageVersion.Default.Addressing;
            MessageEncodingBindingElement element = context.Binding.Elements.Find<MessageEncodingBindingElement>();
            if (element != null)
            {
                addressing = element.MessageVersion.Addressing;
            }
            ChannelProtectionRequirements protectionRequirements = base.GetProtectionRequirements(addressing, base.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel);
            protectionRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
            return (T) protectionRequirements;
        }

        private bool HasProtectionRequirements(ScopedMessagePartSpecification scopedParts)
        {
            foreach (string str in scopedParts.Actions)
            {
                MessagePartSpecification specification;
                if (scopedParts.TryGetParts(str, out specification) && !specification.IsEmpty())
                {
                    return true;
                }
            }
            return false;
        }

        internal override bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            if (!base.IsSetKeyDerivation(requireDerivedKeys))
            {
                return false;
            }
            if ((this.initiatorTokenParameters != null) && (this.initiatorTokenParameters.RequireDerivedKeys != requireDerivedKeys))
            {
                return false;
            }
            if ((this.recipientTokenParameters != null) && (this.recipientTokenParameters.RequireDerivedKeys != requireDerivedKeys))
            {
                return false;
            }
            return true;
        }

        internal override bool RequiresChannelDemuxer()
        {
            if (!base.RequiresChannelDemuxer())
            {
                return base.RequiresChannelDemuxer(this.InitiatorTokenParameters);
            }
            return true;
        }

        public override void SetKeyDerivation(bool requireDerivedKeys)
        {
            base.SetKeyDerivation(requireDerivedKeys);
            if (this.initiatorTokenParameters != null)
            {
                this.initiatorTokenParameters.RequireDerivedKeys = requireDerivedKeys;
            }
            if (this.recipientTokenParameters != null)
            {
                this.recipientTokenParameters.RequireDerivedKeys = requireDerivedKeys;
            }
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            SecurityBindingElement.ExportPolicy(exporter, context);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "MessageProtectionOrder: {0}", new object[] { this.messageProtectionOrder.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "RequireSignatureConfirmation: {0}", new object[] { this.requireSignatureConfirmation.ToString() }));
            builder.Append("InitiatorTokenParameters: ");
            if (this.initiatorTokenParameters != null)
            {
                builder.AppendLine(this.initiatorTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            }
            else
            {
                builder.AppendLine("null");
            }
            builder.Append("RecipientTokenParameters: ");
            if (this.recipientTokenParameters != null)
            {
                builder.AppendLine(this.recipientTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            }
            else
            {
                builder.AppendLine("null");
            }
            return builder.ToString().Trim();
        }

        public bool AllowSerializedSigningTokenOnReply
        {
            get
            {
                return this.allowSerializedSigningTokenOnReply;
            }
            set
            {
                this.allowSerializedSigningTokenOnReply = value;
            }
        }

        public SecurityTokenParameters InitiatorTokenParameters
        {
            get
            {
                return this.initiatorTokenParameters;
            }
            set
            {
                this.initiatorTokenParameters = value;
            }
        }

        internal bool IsCertificateSignatureBinding
        {
            get
            {
                return this.isCertificateSignatureBinding;
            }
            set
            {
                this.isCertificateSignatureBinding = value;
            }
        }

        public System.ServiceModel.Security.MessageProtectionOrder MessageProtectionOrder
        {
            get
            {
                return this.messageProtectionOrder;
            }
            set
            {
                if (!MessageProtectionOrderHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.messageProtectionOrder = value;
            }
        }

        public SecurityTokenParameters RecipientTokenParameters
        {
            get
            {
                return this.recipientTokenParameters;
            }
            set
            {
                this.recipientTokenParameters = value;
            }
        }

        public bool RequireSignatureConfirmation
        {
            get
            {
                return this.requireSignatureConfirmation;
            }
            set
            {
                this.requireSignatureConfirmation = value;
            }
        }

        internal override bool SupportsDuplex
        {
            get
            {
                return !this.isCertificateSignatureBinding;
            }
        }

        internal override bool SupportsRequestReply
        {
            get
            {
                return !this.isCertificateSignatureBinding;
            }
        }
    }
}

