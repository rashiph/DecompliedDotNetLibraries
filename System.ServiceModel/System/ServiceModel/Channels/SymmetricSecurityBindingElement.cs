namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    public sealed class SymmetricSecurityBindingElement : SecurityBindingElement, IPolicyExportExtension
    {
        private System.ServiceModel.Security.MessageProtectionOrder messageProtectionOrder;
        private SecurityTokenParameters protectionTokenParameters;
        private bool requireSignatureConfirmation;

        public SymmetricSecurityBindingElement() : this((SecurityTokenParameters) null)
        {
        }

        private SymmetricSecurityBindingElement(SymmetricSecurityBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.messageProtectionOrder = elementToBeCloned.messageProtectionOrder;
            if (elementToBeCloned.protectionTokenParameters != null)
            {
                this.protectionTokenParameters = elementToBeCloned.protectionTokenParameters.Clone();
            }
            this.requireSignatureConfirmation = elementToBeCloned.requireSignatureConfirmation;
        }

        public SymmetricSecurityBindingElement(SecurityTokenParameters protectionTokenParameters)
        {
            this.messageProtectionOrder = System.ServiceModel.Security.MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
            this.requireSignatureConfirmation = false;
            this.protectionTokenParameters = protectionTokenParameters;
        }

        protected override IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context)
        {
            ISecurityCapabilities property = this.GetProperty<ISecurityCapabilities>(context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }
            bool addChannelDemuxerIfRequired = this.RequiresChannelDemuxer();
            ChannelBuilder builder = new ChannelBuilder(context, addChannelDemuxerIfRequired);
            if (addChannelDemuxerIfRequired)
            {
                base.ApplyPropertiesOnDemuxer(builder, context);
            }
            BindingContext issuerBindingContext = context.Clone();
            if (this.ProtectionTokenParameters is SecureConversationSecurityTokenParameters)
            {
                SecureConversationSecurityTokenParameters protectionTokenParameters = (SecureConversationSecurityTokenParameters) this.ProtectionTokenParameters;
                if (protectionTokenParameters.BootstrapSecurityBindingElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationSecurityTokenParametersRequireBootstrapBinding")));
                }
                BindingContext context3 = issuerBindingContext.Clone();
                context3.BindingParameters.Remove<ChannelProtectionRequirements>();
                context3.BindingParameters.Add(protectionTokenParameters.BootstrapProtectionRequirements);
                if (protectionTokenParameters.RequireCancellation)
                {
                    SessionSymmetricMessageSecurityProtocolFactory factory2 = new SessionSymmetricMessageSecurityProtocolFactory {
                        SecurityTokenParameters = protectionTokenParameters.Clone()
                    };
                    ((SecureConversationSecurityTokenParameters) factory2.SecurityTokenParameters).IssuerBindingContext = context3;
                    factory2.ApplyConfidentiality = true;
                    factory2.RequireConfidentiality = true;
                    factory2.ApplyIntegrity = true;
                    factory2.RequireIntegrity = true;
                    factory2.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
                    factory2.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                    factory2.MessageProtectionOrder = this.MessageProtectionOrder;
                    factory2.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
                    factory2.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, false));
                    base.ConfigureProtocolFactory(factory2, credentialsManager, false, issuerBindingContext, context.Binding);
                    SecuritySessionClientSettings<TChannel> sessionClientSettings = new SecuritySessionClientSettings<TChannel> {
                        ChannelBuilder = builder,
                        KeyRenewalInterval = base.LocalClientSettings.SessionKeyRenewalInterval,
                        CanRenewSession = protectionTokenParameters.CanRenewSession,
                        KeyRolloverInterval = base.LocalClientSettings.SessionKeyRolloverInterval,
                        TolerateTransportFailures = base.LocalClientSettings.ReconnectTransportOnFailure,
                        IssuedSecurityTokenParameters = protectionTokenParameters.Clone()
                    };
                    ((SecureConversationSecurityTokenParameters) sessionClientSettings.IssuedSecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    sessionClientSettings.SecurityStandardsManager = factory2.StandardsManager;
                    sessionClientSettings.SessionProtocolFactory = factory2;
                    return new SecurityChannelFactory<TChannel>(property, context, sessionClientSettings);
                }
                SymmetricSecurityProtocolFactory factory = new SymmetricSecurityProtocolFactory {
                    SecurityTokenParameters = protectionTokenParameters.Clone()
                };
                ((SecureConversationSecurityTokenParameters) factory.SecurityTokenParameters).IssuerBindingContext = context3;
                factory.ApplyConfidentiality = true;
                factory.RequireConfidentiality = true;
                factory.ApplyIntegrity = true;
                factory.RequireIntegrity = true;
                factory.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
                factory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                factory.MessageProtectionOrder = this.MessageProtectionOrder;
                factory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, false));
                base.ConfigureProtocolFactory(factory, credentialsManager, false, issuerBindingContext, context.Binding);
                return new SecurityChannelFactory<TChannel>(property, context, builder, factory);
            }
            return new SecurityChannelFactory<TChannel>(property, context, builder, this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, false, issuerBindingContext));
        }

        protected override IChannelListener<TChannel> BuildChannelListenerCore<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            SecurityChannelListener<TChannel> listener = new SecurityChannelListener<TChannel>(this, context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ServiceCredentials.CreateDefaultCredentials();
            }
            bool addChannelDemuxerIfRequired = this.RequiresChannelDemuxer();
            ChannelBuilder builder = new ChannelBuilder(context, addChannelDemuxerIfRequired);
            if (addChannelDemuxerIfRequired)
            {
                base.ApplyPropertiesOnDemuxer(builder, context);
            }
            BindingContext issuerBindingContext = context.Clone();
            if (this.ProtectionTokenParameters is SecureConversationSecurityTokenParameters)
            {
                SecureConversationSecurityTokenParameters protectionTokenParameters = (SecureConversationSecurityTokenParameters) this.ProtectionTokenParameters;
                if (protectionTokenParameters.BootstrapSecurityBindingElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationSecurityTokenParametersRequireBootstrapBinding")));
                }
                BindingContext secureConversationBindingContext = issuerBindingContext.Clone();
                secureConversationBindingContext.BindingParameters.Remove<ChannelProtectionRequirements>();
                secureConversationBindingContext.BindingParameters.Add(protectionTokenParameters.BootstrapProtectionRequirements);
                IMessageFilterTable<EndpointAddress> table = context.BindingParameters.Find<IMessageFilterTable<EndpointAddress>>();
                base.AddDemuxerForSecureConversation(builder, secureConversationBindingContext);
                if (protectionTokenParameters.RequireCancellation)
                {
                    SessionSymmetricMessageSecurityProtocolFactory factory = new SessionSymmetricMessageSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, factory);
                    factory.SecurityTokenParameters = protectionTokenParameters.Clone();
                    ((SecureConversationSecurityTokenParameters) factory.SecurityTokenParameters).IssuerBindingContext = secureConversationBindingContext;
                    factory.ApplyConfidentiality = true;
                    factory.RequireConfidentiality = true;
                    factory.ApplyIntegrity = true;
                    factory.RequireIntegrity = true;
                    factory.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
                    factory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                    factory.MessageProtectionOrder = this.MessageProtectionOrder;
                    factory.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
                    factory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, true));
                    base.ConfigureProtocolFactory(factory, credentialsManager, true, issuerBindingContext, context.Binding);
                    listener.SessionMode = true;
                    listener.SessionServerSettings.InactivityTimeout = base.LocalServiceSettings.InactivityTimeout;
                    listener.SessionServerSettings.KeyRolloverInterval = base.LocalServiceSettings.SessionKeyRolloverInterval;
                    listener.SessionServerSettings.MaximumPendingSessions = base.LocalServiceSettings.MaxPendingSessions;
                    listener.SessionServerSettings.MaximumKeyRenewalInterval = base.LocalServiceSettings.SessionKeyRenewalInterval;
                    listener.SessionServerSettings.TolerateTransportFailures = base.LocalServiceSettings.ReconnectTransportOnFailure;
                    listener.SessionServerSettings.CanRenewSession = protectionTokenParameters.CanRenewSession;
                    listener.SessionServerSettings.IssuedSecurityTokenParameters = protectionTokenParameters.Clone();
                    ((SecureConversationSecurityTokenParameters) listener.SessionServerSettings.IssuedSecurityTokenParameters).IssuerBindingContext = secureConversationBindingContext;
                    listener.SessionServerSettings.SecurityStandardsManager = factory.StandardsManager;
                    listener.SessionServerSettings.SessionProtocolFactory = factory;
                    listener.SessionServerSettings.SessionProtocolFactory.EndpointFilterTable = table;
                    if (((context.BindingParameters != null) && (context.BindingParameters.Find<IChannelDemuxFailureHandler>() == null)) && !base.IsUnderlyingListenerDuplex<TChannel>(context))
                    {
                        context.BindingParameters.Add(new SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler(factory.StandardsManager));
                    }
                }
                else
                {
                    SymmetricSecurityProtocolFactory factory2 = new SymmetricSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, factory2);
                    factory2.SecurityTokenParameters = protectionTokenParameters.Clone();
                    ((SecureConversationSecurityTokenParameters) factory2.SecurityTokenParameters).IssuerBindingContext = secureConversationBindingContext;
                    factory2.ApplyConfidentiality = true;
                    factory2.RequireConfidentiality = true;
                    factory2.ApplyIntegrity = true;
                    factory2.RequireIntegrity = true;
                    factory2.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
                    factory2.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
                    factory2.MessageProtectionOrder = this.MessageProtectionOrder;
                    factory2.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, true));
                    factory2.EndpointFilterTable = table;
                    base.ConfigureProtocolFactory(factory2, credentialsManager, true, issuerBindingContext, context.Binding);
                    listener.SecurityProtocolFactory = factory2;
                }
            }
            else
            {
                SecurityProtocolFactory factory3 = this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, true, issuerBindingContext);
                listener.SecurityProtocolFactory = factory3;
            }
            listener.InitializeListener(builder);
            return listener;
        }

        public override BindingElement Clone()
        {
            return new SymmetricSecurityBindingElement(this);
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
            if (this.ProtectionTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SymmetricSecurityBindingElementNeedsProtectionTokenParameters", new object[] { this.ToString() })));
            }
            SymmetricSecurityProtocolFactory factory = new SymmetricSecurityProtocolFactory();
            if (isForService)
            {
                base.ApplyAuditBehaviorSettings(context, factory);
            }
            factory.SecurityTokenParameters = this.ProtectionTokenParameters.Clone();
            SecurityBindingElement.SetIssuerBindingContextIfRequired(factory.SecurityTokenParameters, issuerBindingContext);
            factory.ApplyConfidentiality = true;
            factory.RequireConfidentiality = true;
            factory.ApplyIntegrity = true;
            factory.RequireIntegrity = true;
            factory.IdentityVerifier = base.LocalClientSettings.IdentityVerifier;
            factory.DoRequestSignatureConfirmation = this.RequireSignatureConfirmation;
            factory.MessageProtectionOrder = this.MessageProtectionOrder;
            factory.ProtectionRequirements.Add(SecurityBindingElement.ComputeProtectionRequirements(this, context.BindingParameters, context.Binding.Elements, isForService));
            base.ConfigureProtocolFactory(factory, credentialsManager, isForService, issuerBindingContext, context.Binding);
            return factory;
        }

        internal override ISecurityCapabilities GetIndividualISecurityCapabilities()
        {
            bool flag2;
            bool flag3;
            bool supportsServerAuth = false;
            base.GetSupportingTokensCapabilities(out flag2, out flag3);
            if (this.ProtectionTokenParameters != null)
            {
                flag2 = flag2 || this.ProtectionTokenParameters.SupportsClientAuthentication;
                flag3 = flag3 || this.ProtectionTokenParameters.SupportsClientWindowsIdentity;
                if (this.ProtectionTokenParameters.HasAsymmetricKey)
                {
                    supportsServerAuth = this.ProtectionTokenParameters.SupportsClientAuthentication;
                }
                else
                {
                    supportsServerAuth = this.ProtectionTokenParameters.SupportsServerAuthentication;
                }
            }
            return new SecurityCapabilities(flag2, supportsServerAuth, flag3, ProtectionLevel.EncryptAndSign, ProtectionLevel.EncryptAndSign);
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
            ChannelProtectionRequirements protectionRequirements = base.GetProtectionRequirements(addressing, ProtectionLevel.EncryptAndSign);
            protectionRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
            return (T) protectionRequirements;
        }

        internal override bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            if (!base.IsSetKeyDerivation(requireDerivedKeys))
            {
                return false;
            }
            if ((this.protectionTokenParameters != null) && (this.protectionTokenParameters.RequireDerivedKeys != requireDerivedKeys))
            {
                return false;
            }
            return true;
        }

        internal override bool RequiresChannelDemuxer()
        {
            if (!base.RequiresChannelDemuxer())
            {
                return base.RequiresChannelDemuxer(this.ProtectionTokenParameters);
            }
            return true;
        }

        public override void SetKeyDerivation(bool requireDerivedKeys)
        {
            base.SetKeyDerivation(requireDerivedKeys);
            if (this.protectionTokenParameters != null)
            {
                this.protectionTokenParameters.RequireDerivedKeys = requireDerivedKeys;
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
            builder.Append("ProtectionTokenParameters: ");
            if (this.protectionTokenParameters != null)
            {
                builder.AppendLine(this.protectionTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            }
            else
            {
                builder.AppendLine("null");
            }
            return builder.ToString().Trim();
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

        public SecurityTokenParameters ProtectionTokenParameters
        {
            get
            {
                return this.protectionTokenParameters;
            }
            set
            {
                this.protectionTokenParameters = value;
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

        internal override bool SessionMode
        {
            get
            {
                SecureConversationSecurityTokenParameters protectionTokenParameters = this.ProtectionTokenParameters as SecureConversationSecurityTokenParameters;
                return ((protectionTokenParameters != null) && protectionTokenParameters.RequireCancellation);
            }
        }

        internal override bool SupportsDuplex
        {
            get
            {
                return this.SessionMode;
            }
        }

        internal override bool SupportsRequestReply
        {
            get
            {
                return true;
            }
        }
    }
}

