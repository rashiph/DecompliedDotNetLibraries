namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    public abstract class SecurityBindingElement : BindingElement
    {
        private bool allowInsecureTransport;
        private SecurityAlgorithmSuite defaultAlgorithmSuite;
        internal const string defaultAlgorithmSuiteString = "Default";
        internal const bool defaultAllowInsecureTransport = false;
        internal static readonly SecurityAlgorithmSuite defaultDefaultAlgorithmSuite = SecurityAlgorithmSuite.Default;
        internal const bool defaultEnableUnsecuredResponse = false;
        internal const bool defaultIncludeTimestamp = true;
        internal const MessageProtectionOrder defaultMessageProtectionOrder = MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
        internal const bool defaultRequireSignatureConfirmation = false;
        private bool doNotEmitTrust;
        private bool enableUnsecuredResponse;
        private SupportingTokenParameters endpointSupportingTokenParameters;
        private bool includeTimestamp;
        private InternalDuplexBindingElement internalDuplexBindingElement;
        private SecurityKeyEntropyMode keyEntropyMode;
        private LocalClientSecuritySettings localClientSettings;
        private LocalServiceSecuritySettings localServiceSettings;
        private long maxReceivedMessageSize;
        private System.ServiceModel.MessageSecurityVersion messageSecurityVersion;
        private Dictionary<string, SupportingTokenParameters> operationSupportingTokenParameters;
        private SupportingTokenParameters optionalEndpointSupportingTokenParameters;
        private Dictionary<string, SupportingTokenParameters> optionalOperationSupportingTokenParameters;
        private XmlDictionaryReaderQuotas readerQuotas;
        private System.ServiceModel.Channels.SecurityHeaderLayout securityHeaderLayout;
        private bool supportsExtendedProtectionPolicy;

        internal SecurityBindingElement()
        {
            this.maxReceivedMessageSize = 0x10000L;
            this.messageSecurityVersion = System.ServiceModel.MessageSecurityVersion.Default;
            this.keyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
            this.includeTimestamp = true;
            this.defaultAlgorithmSuite = defaultDefaultAlgorithmSuite;
            this.localClientSettings = new LocalClientSecuritySettings();
            this.localServiceSettings = new LocalServiceSecuritySettings();
            this.endpointSupportingTokenParameters = new SupportingTokenParameters();
            this.optionalEndpointSupportingTokenParameters = new SupportingTokenParameters();
            this.operationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            this.optionalOperationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            this.securityHeaderLayout = System.ServiceModel.Channels.SecurityHeaderLayout.Strict;
            this.allowInsecureTransport = false;
            this.enableUnsecuredResponse = false;
        }

        internal SecurityBindingElement(SecurityBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.maxReceivedMessageSize = 0x10000L;
            if (elementToBeCloned == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elementToBeCloned");
            }
            this.defaultAlgorithmSuite = elementToBeCloned.defaultAlgorithmSuite;
            this.includeTimestamp = elementToBeCloned.includeTimestamp;
            this.keyEntropyMode = elementToBeCloned.keyEntropyMode;
            this.messageSecurityVersion = elementToBeCloned.messageSecurityVersion;
            this.securityHeaderLayout = elementToBeCloned.securityHeaderLayout;
            this.endpointSupportingTokenParameters = elementToBeCloned.endpointSupportingTokenParameters.Clone();
            this.optionalEndpointSupportingTokenParameters = elementToBeCloned.optionalEndpointSupportingTokenParameters.Clone();
            this.operationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            foreach (string str in elementToBeCloned.operationSupportingTokenParameters.Keys)
            {
                this.operationSupportingTokenParameters[str] = elementToBeCloned.operationSupportingTokenParameters[str].Clone();
            }
            this.optionalOperationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            foreach (string str2 in elementToBeCloned.optionalOperationSupportingTokenParameters.Keys)
            {
                this.optionalOperationSupportingTokenParameters[str2] = elementToBeCloned.optionalOperationSupportingTokenParameters[str2].Clone();
            }
            this.localClientSettings = elementToBeCloned.localClientSettings.Clone();
            this.localServiceSettings = elementToBeCloned.localServiceSettings.Clone();
            this.internalDuplexBindingElement = elementToBeCloned.internalDuplexBindingElement;
            this.maxReceivedMessageSize = elementToBeCloned.maxReceivedMessageSize;
            this.readerQuotas = elementToBeCloned.readerQuotas;
            this.doNotEmitTrust = elementToBeCloned.doNotEmitTrust;
            this.allowInsecureTransport = elementToBeCloned.allowInsecureTransport;
            this.enableUnsecuredResponse = elementToBeCloned.enableUnsecuredResponse;
            this.supportsExtendedProtectionPolicy = elementToBeCloned.supportsExtendedProtectionPolicy;
        }

        private static void AddAssertionIfNotNull(PolicyConversionContext policyContext, Collection<XmlElement> assertions)
        {
            if ((policyContext != null) && (assertions != null))
            {
                PolicyAssertionCollection bindingAssertions = policyContext.GetBindingAssertions();
                for (int i = 0; i < assertions.Count; i++)
                {
                    bindingAssertions.Add(assertions[i]);
                }
            }
        }

        private static void AddAssertionIfNotNull(PolicyConversionContext policyContext, XmlElement assertion)
        {
            if ((policyContext != null) && (assertion != null))
            {
                policyContext.GetBindingAssertions().Add(assertion);
            }
        }

        private static void AddAssertionIfNotNull(PolicyConversionContext policyContext, FaultDescription message, XmlElement assertion)
        {
            if ((policyContext != null) && (assertion != null))
            {
                policyContext.GetFaultBindingAssertions(message).Add(assertion);
            }
        }

        private static void AddAssertionIfNotNull(PolicyConversionContext policyContext, MessageDescription message, XmlElement assertion)
        {
            if ((policyContext != null) && (assertion != null))
            {
                policyContext.GetMessageBindingAssertions(message).Add(assertion);
            }
        }

        private static void AddAssertionIfNotNull(PolicyConversionContext policyContext, OperationDescription operation, Collection<XmlElement> assertions)
        {
            if ((policyContext != null) && (assertions != null))
            {
                PolicyAssertionCollection operationBindingAssertions = policyContext.GetOperationBindingAssertions(operation);
                for (int i = 0; i < assertions.Count; i++)
                {
                    operationBindingAssertions.Add(assertions[i]);
                }
            }
        }

        private static void AddAssertionIfNotNull(PolicyConversionContext policyContext, OperationDescription operation, XmlElement assertion)
        {
            if ((policyContext != null) && (assertion != null))
            {
                policyContext.GetOperationBindingAssertions(operation).Add(assertion);
            }
        }

        private static void AddBindingProtectionRequirements(ChannelProtectionRequirements requirements, BindingElementCollection bindingElements, bool isForChannel)
        {
            CustomBinding binding = new CustomBinding(bindingElements);
            BindingContext context = new BindingContext(binding, new BindingParameterCollection());
            foreach (BindingElement element in bindingElements)
            {
                if (element != null)
                {
                    context.RemainingBindingElements.Clear();
                    context.RemainingBindingElements.Add(element);
                    ChannelProtectionRequirements innerProperty = context.GetInnerProperty<ChannelProtectionRequirements>();
                    if (innerProperty != null)
                    {
                        requirements.Add(innerProperty);
                    }
                }
            }
        }

        internal void AddDemuxerForSecureConversation(ChannelBuilder builder, BindingContext secureConversationBindingContext)
        {
            int num = 0;
            bool flag = false;
            for (int i = 0; i < builder.Binding.Elements.Count; i++)
            {
                if (!(builder.Binding.Elements[i] is MessageEncodingBindingElement) && !(builder.Binding.Elements[i] is StreamUpgradeBindingElement))
                {
                    if (builder.Binding.Elements[i] is ChannelDemuxerBindingElement)
                    {
                        num++;
                    }
                    else
                    {
                        if (builder.Binding.Elements[i] is TransportBindingElement)
                        {
                            break;
                        }
                        flag = true;
                    }
                }
            }
            if ((num != 1) || flag)
            {
                ChannelDemuxerBindingElement item = new ChannelDemuxerBindingElement(false) {
                    MaxPendingSessions = this.LocalServiceSettings.MaxPendingSessions,
                    PeekTimeout = this.LocalServiceSettings.NegotiationTimeout
                };
                builder.Binding.Elements.Insert(0, item);
                secureConversationBindingContext.RemainingBindingElements.Insert(0, item);
            }
        }

        internal void ApplyAuditBehaviorSettings(BindingContext context, SecurityProtocolFactory factory)
        {
            ServiceSecurityAuditBehavior behavior = context.BindingParameters.Find<ServiceSecurityAuditBehavior>();
            if (behavior != null)
            {
                factory.AuditLogLocation = behavior.AuditLogLocation;
                factory.SuppressAuditFailure = behavior.SuppressAuditFailure;
                factory.ServiceAuthorizationAuditLevel = behavior.ServiceAuthorizationAuditLevel;
                factory.MessageAuthenticationAuditLevel = behavior.MessageAuthenticationAuditLevel;
            }
            else
            {
                factory.AuditLogLocation = AuditLogLocation.Default;
                factory.SuppressAuditFailure = true;
                factory.ServiceAuthorizationAuditLevel = AuditLevel.None;
                factory.MessageAuthenticationAuditLevel = AuditLevel.None;
            }
        }

        internal void ApplyPropertiesOnDemuxer(ChannelBuilder builder, BindingContext context)
        {
            foreach (ChannelDemuxerBindingElement element in builder.Binding.Elements.FindAll<ChannelDemuxerBindingElement>())
            {
                if (element != null)
                {
                    element.MaxPendingSessions = this.LocalServiceSettings.MaxPendingSessions;
                    element.PeekTimeout = this.LocalServiceSettings.NegotiationTimeout;
                }
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }), "TChannel"));
            }
            this.readerQuotas = context.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EncodingBindingElementDoesNotHandleReaderQuotas")));
            }
            TransportBindingElement element = null;
            if (context.RemainingBindingElements != null)
            {
                element = context.RemainingBindingElements.Find<TransportBindingElement>();
            }
            if (element != null)
            {
                this.maxReceivedMessageSize = element.MaxReceivedMessageSize;
            }
            IChannelFactory<TChannel> factory = this.BuildChannelFactoryCore<TChannel>(context);
            if (element != null)
            {
                SecurityChannelFactory<TChannel> factory2 = factory as SecurityChannelFactory<TChannel>;
                if ((factory2 != null) && (factory2.SecurityProtocolFactory != null))
                {
                    factory2.SecurityProtocolFactory.ExtendedProtectionPolicy = element.GetProperty<ExtendedProtectionPolicy>(context);
                }
            }
            return factory;
        }

        protected abstract IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context);
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }), "TChannel"));
            }
            this.readerQuotas = context.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EncodingBindingElementDoesNotHandleReaderQuotas")));
            }
            TransportBindingElement element = null;
            if (context.RemainingBindingElements != null)
            {
                element = context.RemainingBindingElements.Find<TransportBindingElement>();
            }
            if (element != null)
            {
                this.maxReceivedMessageSize = element.MaxReceivedMessageSize;
            }
            return this.BuildChannelListenerCore<TChannel>(context);
        }

        protected abstract IChannelListener<TChannel> BuildChannelListenerCore<TChannel>(BindingContext context) where TChannel: class, IChannel;
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref this.internalDuplexBindingElement);
            if (this.SessionMode)
            {
                return this.CanBuildSessionChannelFactory<TChannel>(context);
            }
            if (!context.CanBuildInnerChannelFactory<TChannel>())
            {
                return false;
            }
            if (((typeof(TChannel) != typeof(IOutputChannel)) && (typeof(TChannel) != typeof(IOutputSessionChannel))) && (!this.SupportsDuplex || ((typeof(TChannel) != typeof(IDuplexChannel)) && (typeof(TChannel) != typeof(IDuplexSessionChannel)))))
            {
                if (!this.SupportsRequestReply)
                {
                    return false;
                }
                if (!(typeof(TChannel) == typeof(IRequestChannel)))
                {
                    return (typeof(TChannel) == typeof(IRequestSessionChannel));
                }
            }
            return true;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            InternalDuplexBindingElement.AddDuplexListenerSupport(context, ref this.internalDuplexBindingElement);
            if (this.SessionMode)
            {
                return this.CanBuildSessionChannelListener<TChannel>(context);
            }
            if (!context.CanBuildInnerChannelListener<TChannel>())
            {
                return false;
            }
            if (((typeof(TChannel) != typeof(IInputChannel)) && (typeof(TChannel) != typeof(IInputSessionChannel))) && (!this.SupportsDuplex || ((typeof(TChannel) != typeof(IDuplexChannel)) && (typeof(TChannel) != typeof(IDuplexSessionChannel)))))
            {
                if (!this.SupportsRequestReply)
                {
                    return false;
                }
                if (!(typeof(TChannel) == typeof(IReplyChannel)))
                {
                    return (typeof(TChannel) == typeof(IReplySessionChannel));
                }
            }
            return true;
        }

        private bool CanBuildSessionChannelFactory<TChannel>(BindingContext context)
        {
            if ((!context.CanBuildInnerChannelFactory<IRequestChannel>() && !context.CanBuildInnerChannelFactory<IRequestSessionChannel>()) && (!context.CanBuildInnerChannelFactory<IDuplexChannel>() && !context.CanBuildInnerChannelFactory<IDuplexSessionChannel>()))
            {
                return false;
            }
            if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                if (!context.CanBuildInnerChannelFactory<IRequestChannel>())
                {
                    return context.CanBuildInnerChannelFactory<IRequestSessionChannel>();
                }
                return true;
            }
            if (!(typeof(TChannel) == typeof(IDuplexSessionChannel)))
            {
                return false;
            }
            if (!context.CanBuildInnerChannelFactory<IDuplexChannel>())
            {
                return context.CanBuildInnerChannelFactory<IDuplexSessionChannel>();
            }
            return true;
        }

        private bool CanBuildSessionChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if ((!context.CanBuildInnerChannelListener<IReplyChannel>() && !context.CanBuildInnerChannelListener<IReplySessionChannel>()) && (!context.CanBuildInnerChannelListener<IDuplexChannel>() && !context.CanBuildInnerChannelListener<IDuplexSessionChannel>()))
            {
                return false;
            }
            if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                if (!context.CanBuildInnerChannelListener<IReplyChannel>())
                {
                    return context.CanBuildInnerChannelListener<IReplySessionChannel>();
                }
                return true;
            }
            if (!(typeof(TChannel) == typeof(IDuplexSessionChannel)))
            {
                return false;
            }
            if (!context.CanBuildInnerChannelListener<IDuplexChannel>())
            {
                return context.CanBuildInnerChannelListener<IDuplexSessionChannel>();
            }
            return true;
        }

        internal static ChannelProtectionRequirements ComputeProtectionRequirements(SecurityBindingElement security, BindingParameterCollection parameterCollection, BindingElementCollection bindingElements, bool isForService)
        {
            if (parameterCollection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterCollection");
            }
            if (bindingElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }
            if (security == null)
            {
                return null;
            }
            ChannelProtectionRequirements requirements = null;
            if ((security is SymmetricSecurityBindingElement) || (security is AsymmetricSecurityBindingElement))
            {
                requirements = new ChannelProtectionRequirements();
                ChannelProtectionRequirements protectionRequirements = parameterCollection.Find<ChannelProtectionRequirements>();
                if (protectionRequirements != null)
                {
                    requirements.Add(protectionRequirements);
                }
                AddBindingProtectionRequirements(requirements, bindingElements, !isForService);
            }
            return requirements;
        }

        internal void ConfigureProtocolFactory(SecurityProtocolFactory factory, SecurityCredentialsManager credentialsManager, bool isForService, BindingContext issuerBindingContext, Binding binding)
        {
            if (factory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("factory"));
            }
            if (credentialsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("credentialsManager"));
            }
            factory.AddTimestamp = this.IncludeTimestamp;
            factory.IncomingAlgorithmSuite = this.DefaultAlgorithmSuite;
            factory.OutgoingAlgorithmSuite = this.DefaultAlgorithmSuite;
            factory.SecurityHeaderLayout = this.SecurityHeaderLayout;
            if (!isForService)
            {
                factory.TimestampValidityDuration = this.LocalClientSettings.TimestampValidityDuration;
                factory.DetectReplays = this.LocalClientSettings.DetectReplays;
                factory.MaxCachedNonces = this.LocalClientSettings.ReplayCacheSize;
                factory.MaxClockSkew = this.LocalClientSettings.MaxClockSkew;
                factory.ReplayWindow = this.LocalClientSettings.ReplayWindow;
            }
            else
            {
                factory.TimestampValidityDuration = this.LocalServiceSettings.TimestampValidityDuration;
                factory.DetectReplays = this.LocalServiceSettings.DetectReplays;
                factory.MaxCachedNonces = this.LocalServiceSettings.ReplayCacheSize;
                factory.MaxClockSkew = this.LocalServiceSettings.MaxClockSkew;
                factory.ReplayWindow = this.LocalServiceSettings.ReplayWindow;
            }
            factory.SecurityBindingElement = (SecurityBindingElement) this.Clone();
            factory.SecurityBindingElement.SetIssuerBindingContextIfRequired(issuerBindingContext);
            factory.SecurityTokenManager = credentialsManager.CreateSecurityTokenManager();
            SecurityTokenSerializer tokenSerializer = factory.SecurityTokenManager.CreateSecurityTokenSerializer(this.messageSecurityVersion.SecurityTokenVersion);
            factory.StandardsManager = new SecurityStandardsManager(this.messageSecurityVersion, tokenSerializer);
            if (!isForService)
            {
                this.SetPrivacyNoticeUriIfRequired(factory, binding);
            }
        }

        public static SymmetricSecurityBindingElement CreateAnonymousForCertificateBindingElement()
        {
            return new SymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.Never)) { MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11, RequireSignatureConfirmation = true };
        }

        public static TransportSecurityBindingElement CreateCertificateOverTransportBindingElement()
        {
            return CreateCertificateOverTransportBindingElement(System.ServiceModel.MessageSecurityVersion.Default);
        }

        public static TransportSecurityBindingElement CreateCertificateOverTransportBindingElement(System.ServiceModel.MessageSecurityVersion version)
        {
            X509KeyIdentifierClauseType any;
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            if (version.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                any = X509KeyIdentifierClauseType.Any;
            }
            else
            {
                any = X509KeyIdentifierClauseType.Thumbprint;
            }
            TransportSecurityBindingElement element = new TransportSecurityBindingElement();
            X509SecurityTokenParameters item = new X509SecurityTokenParameters(any, SecurityTokenInclusionMode.AlwaysToRecipient, false);
            element.EndpointSupportingTokenParameters.Endorsing.Add(item);
            element.IncludeTimestamp = true;
            element.LocalClientSettings.DetectReplays = false;
            element.LocalServiceSettings.DetectReplays = false;
            element.MessageSecurityVersion = version;
            return element;
        }

        public static AsymmetricSecurityBindingElement CreateCertificateSignatureBindingElement()
        {
            AsymmetricSecurityBindingElement element = new AsymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.Never, false), new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.AlwaysToRecipient, false)) {
                IsCertificateSignatureBinding = true
            };
            element.LocalClientSettings.DetectReplays = false;
            element.MessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
            return element;
        }

        public static SymmetricSecurityBindingElement CreateIssuedTokenBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            if (issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");
            }
            if (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("IssuedTokenAuthenticationModeRequiresSymmetricIssuedKey"));
            }
            return new SymmetricSecurityBindingElement(issuedTokenParameters);
        }

        public static SymmetricSecurityBindingElement CreateIssuedTokenForCertificateBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            if (issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");
            }
            SymmetricSecurityBindingElement element = new SymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.Never));
            if (issuedTokenParameters.KeyType == SecurityKeyType.BearerKey)
            {
                element.EndpointSupportingTokenParameters.SignedEncrypted.Add(issuedTokenParameters);
                element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.WSSXDefault;
            }
            else
            {
                element.EndpointSupportingTokenParameters.Endorsing.Add(issuedTokenParameters);
                element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.Default;
            }
            element.RequireSignatureConfirmation = true;
            return element;
        }

        public static SymmetricSecurityBindingElement CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            return CreateIssuedTokenForSslBindingElement(issuedTokenParameters, false);
        }

        public static SymmetricSecurityBindingElement CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters issuedTokenParameters, bool requireCancellation)
        {
            if (issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");
            }
            SymmetricSecurityBindingElement element = new SymmetricSecurityBindingElement(new SslSecurityTokenParameters(false, requireCancellation));
            if (issuedTokenParameters.KeyType == SecurityKeyType.BearerKey)
            {
                element.EndpointSupportingTokenParameters.SignedEncrypted.Add(issuedTokenParameters);
                element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.WSSXDefault;
            }
            else
            {
                element.EndpointSupportingTokenParameters.Endorsing.Add(issuedTokenParameters);
                element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.Default;
            }
            element.RequireSignatureConfirmation = true;
            return element;
        }

        public static TransportSecurityBindingElement CreateIssuedTokenOverTransportBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            if (issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");
            }
            issuedTokenParameters.RequireDerivedKeys = false;
            TransportSecurityBindingElement element = new TransportSecurityBindingElement();
            if (issuedTokenParameters.KeyType == SecurityKeyType.BearerKey)
            {
                element.EndpointSupportingTokenParameters.Signed.Add(issuedTokenParameters);
                element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.WSSXDefault;
            }
            else
            {
                element.EndpointSupportingTokenParameters.Endorsing.Add(issuedTokenParameters);
                element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.Default;
            }
            element.LocalClientSettings.DetectReplays = false;
            element.LocalServiceSettings.DetectReplays = false;
            element.IncludeTimestamp = true;
            return element;
        }

        private static BindingContext CreateIssuerBindingContextForNegotiation(BindingContext issuerBindingContext)
        {
            TransportBindingElement element = issuerBindingContext.RemainingBindingElements.Find<TransportBindingElement>();
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransportBindingElementNotFound")));
            }
            ChannelDemuxerBindingElement element2 = null;
            for (int i = 0; i < issuerBindingContext.RemainingBindingElements.Count; i++)
            {
                if (issuerBindingContext.RemainingBindingElements[i] is ChannelDemuxerBindingElement)
                {
                    element2 = (ChannelDemuxerBindingElement) issuerBindingContext.RemainingBindingElements[i];
                }
            }
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ChannelDemuxerBindingElementNotFound")));
            }
            BindingElementCollection bindingElements = new BindingElementCollection();
            bindingElements.Add(element2.Clone());
            bindingElements.Add(element.Clone());
            CustomBinding binding = new CustomBinding(bindingElements) {
                OpenTimeout = issuerBindingContext.Binding.OpenTimeout,
                CloseTimeout = issuerBindingContext.Binding.CloseTimeout,
                SendTimeout = issuerBindingContext.Binding.SendTimeout,
                ReceiveTimeout = issuerBindingContext.Binding.ReceiveTimeout
            };
            if (issuerBindingContext.ListenUriBaseAddress != null)
            {
                return new BindingContext(binding, new BindingParameterCollection(issuerBindingContext.BindingParameters), issuerBindingContext.ListenUriBaseAddress, issuerBindingContext.ListenUriRelativeAddress, issuerBindingContext.ListenUriMode);
            }
            return new BindingContext(binding, new BindingParameterCollection(issuerBindingContext.BindingParameters));
        }

        public static SymmetricSecurityBindingElement CreateKerberosBindingElement()
        {
            return new SymmetricSecurityBindingElement(new KerberosSecurityTokenParameters()) { DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault };
        }

        public static TransportSecurityBindingElement CreateKerberosOverTransportBindingElement()
        {
            TransportSecurityBindingElement element = new TransportSecurityBindingElement();
            KerberosSecurityTokenParameters item = new KerberosSecurityTokenParameters {
                RequireDerivedKeys = false
            };
            element.EndpointSupportingTokenParameters.Endorsing.Add(item);
            element.IncludeTimestamp = true;
            element.LocalClientSettings.DetectReplays = false;
            element.LocalServiceSettings.DetectReplays = false;
            element.DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault;
            element.SupportsExtendedProtectionPolicy = true;
            return element;
        }

        public static SecurityBindingElement CreateMutualCertificateBindingElement()
        {
            return CreateMutualCertificateBindingElement(System.ServiceModel.MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11);
        }

        public static SecurityBindingElement CreateMutualCertificateBindingElement(System.ServiceModel.MessageSecurityVersion version)
        {
            return CreateMutualCertificateBindingElement(version, false);
        }

        public static SecurityBindingElement CreateMutualCertificateBindingElement(System.ServiceModel.MessageSecurityVersion version, bool allowSerializedSigningTokenOnReply)
        {
            SecurityBindingElement element;
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            if (version.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                element = new AsymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.Never, false), new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.AlwaysToRecipient, false), allowSerializedSigningTokenOnReply);
            }
            else
            {
                element = new SymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.Never));
                element.EndpointSupportingTokenParameters.Endorsing.Add(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.AlwaysToRecipient, false));
                ((SymmetricSecurityBindingElement) element).RequireSignatureConfirmation = true;
            }
            element.MessageSecurityVersion = version;
            return element;
        }

        public static AsymmetricSecurityBindingElement CreateMutualCertificateDuplexBindingElement()
        {
            return CreateMutualCertificateDuplexBindingElement(System.ServiceModel.MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11);
        }

        public static AsymmetricSecurityBindingElement CreateMutualCertificateDuplexBindingElement(System.ServiceModel.MessageSecurityVersion version)
        {
            AsymmetricSecurityBindingElement element;
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            if (version.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                element = new AsymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.AlwaysToInitiator, false), new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.AlwaysToRecipient, false));
            }
            else
            {
                element = new AsymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.AlwaysToInitiator, false), new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.AlwaysToRecipient, false));
            }
            element.MessageSecurityVersion = version;
            return element;
        }

        public static SecurityBindingElement CreateSecureConversationBindingElement(SecurityBindingElement bootstrapSecurity)
        {
            return CreateSecureConversationBindingElement(bootstrapSecurity, true, null);
        }

        public static SecurityBindingElement CreateSecureConversationBindingElement(SecurityBindingElement bootstrapSecurity, bool requireCancellation)
        {
            return CreateSecureConversationBindingElement(bootstrapSecurity, requireCancellation, null);
        }

        public static SecurityBindingElement CreateSecureConversationBindingElement(SecurityBindingElement bootstrapSecurity, bool requireCancellation, ChannelProtectionRequirements bootstrapProtectionRequirements)
        {
            if (bootstrapSecurity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bootstrapBinding");
            }
            if (bootstrapSecurity is TransportSecurityBindingElement)
            {
                TransportSecurityBindingElement element2 = new TransportSecurityBindingElement();
                SecureConversationSecurityTokenParameters item = new SecureConversationSecurityTokenParameters(bootstrapSecurity, requireCancellation, bootstrapProtectionRequirements) {
                    RequireDerivedKeys = false
                };
                element2.EndpointSupportingTokenParameters.Endorsing.Add(item);
                element2.LocalClientSettings.DetectReplays = false;
                element2.LocalServiceSettings.DetectReplays = false;
                element2.IncludeTimestamp = true;
                return element2;
            }
            return new SymmetricSecurityBindingElement(new SecureConversationSecurityTokenParameters(bootstrapSecurity, requireCancellation, bootstrapProtectionRequirements)) { RequireSignatureConfirmation = false };
        }

        internal abstract SecurityProtocolFactory CreateSecurityProtocolFactory<TChannel>(BindingContext context, SecurityCredentialsManager credentialsManager, bool isForService, BindingContext issuanceBindingContext);
        public static SymmetricSecurityBindingElement CreateSslNegotiationBindingElement(bool requireClientCertificate)
        {
            return CreateSslNegotiationBindingElement(requireClientCertificate, false);
        }

        public static SymmetricSecurityBindingElement CreateSslNegotiationBindingElement(bool requireClientCertificate, bool requireCancellation)
        {
            return new SymmetricSecurityBindingElement(new SslSecurityTokenParameters(requireClientCertificate, requireCancellation));
        }

        public static SymmetricSecurityBindingElement CreateSspiNegotiationBindingElement()
        {
            return CreateSspiNegotiationBindingElement(false);
        }

        public static SymmetricSecurityBindingElement CreateSspiNegotiationBindingElement(bool requireCancellation)
        {
            return new SymmetricSecurityBindingElement(new SspiSecurityTokenParameters(requireCancellation));
        }

        public static TransportSecurityBindingElement CreateSspiNegotiationOverTransportBindingElement()
        {
            return CreateSspiNegotiationOverTransportBindingElement(true);
        }

        public static TransportSecurityBindingElement CreateSspiNegotiationOverTransportBindingElement(bool requireCancellation)
        {
            TransportSecurityBindingElement element = new TransportSecurityBindingElement();
            SspiSecurityTokenParameters item = new SspiSecurityTokenParameters(requireCancellation) {
                RequireDerivedKeys = false
            };
            element.EndpointSupportingTokenParameters.Endorsing.Add(item);
            element.IncludeTimestamp = true;
            element.LocalClientSettings.DetectReplays = false;
            element.LocalServiceSettings.DetectReplays = false;
            element.SupportsExtendedProtectionPolicy = true;
            return element;
        }

        public static SymmetricSecurityBindingElement CreateUserNameForCertificateBindingElement()
        {
            SymmetricSecurityBindingElement element = new SymmetricSecurityBindingElement(new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.Never));
            element.EndpointSupportingTokenParameters.SignedEncrypted.Add(new UserNameSecurityTokenParameters());
            element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            return element;
        }

        public static SymmetricSecurityBindingElement CreateUserNameForSslBindingElement()
        {
            return CreateUserNameForSslBindingElement(false);
        }

        public static SymmetricSecurityBindingElement CreateUserNameForSslBindingElement(bool requireCancellation)
        {
            SymmetricSecurityBindingElement element = new SymmetricSecurityBindingElement(new SslSecurityTokenParameters(false, requireCancellation));
            element.EndpointSupportingTokenParameters.SignedEncrypted.Add(new UserNameSecurityTokenParameters());
            element.MessageSecurityVersion = System.ServiceModel.MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            return element;
        }

        public static TransportSecurityBindingElement CreateUserNameOverTransportBindingElement()
        {
            TransportSecurityBindingElement element = new TransportSecurityBindingElement();
            element.EndpointSupportingTokenParameters.SignedEncrypted.Add(new UserNameSecurityTokenParameters());
            element.IncludeTimestamp = true;
            element.LocalClientSettings.DetectReplays = false;
            element.LocalServiceSettings.DetectReplays = false;
            return element;
        }

        private static void ExportAsymmetricSecurityBindingElement(AsymmetricSecurityBindingElement binding, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy securityPolicyDriver = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);
            AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspAsymmetricBindingAssertion(exporter, policyContext, binding));
            AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspSupportingTokensAssertion(exporter, binding.EndpointSupportingTokenParameters.Signed, binding.EndpointSupportingTokenParameters.SignedEncrypted, binding.EndpointSupportingTokenParameters.Endorsing, binding.EndpointSupportingTokenParameters.SignedEndorsing, binding.OptionalEndpointSupportingTokenParameters.Signed, binding.OptionalEndpointSupportingTokenParameters.SignedEncrypted, binding.OptionalEndpointSupportingTokenParameters.Endorsing, binding.OptionalEndpointSupportingTokenParameters.SignedEndorsing));
            AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspWssAssertion(exporter, binding));
            if (RequiresWsspTrust(binding))
            {
                AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspTrustAssertion(exporter, binding.KeyEntropyMode));
            }
        }

        private static void ExportMessageScopeProtectionPolicy(SecurityBindingElement security, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            BindingParameterCollection parameterCollection = new BindingParameterCollection {
                ChannelProtectionRequirements.CreateFromContract(policyContext.Contract, policyContext.BindingElements.Find<SecurityBindingElement>().GetIndividualProperty<ISecurityCapabilities>(), 0)
            };
            ChannelProtectionRequirements requirements = ComputeProtectionRequirements(security, parameterCollection, policyContext.BindingElements, true);
            requirements.MakeReadOnly();
            WSSecurityPolicy securityPolicyDriver = WSSecurityPolicy.GetSecurityPolicyDriver(security.MessageSecurityVersion);
            foreach (OperationDescription description in policyContext.Contract.Operations)
            {
                foreach (MessageDescription description2 in description.Messages)
                {
                    MessagePartSpecification specification;
                    ScopedMessagePartSpecification incomingSignatureParts;
                    if (description2.Direction == MessageDirection.Input)
                    {
                        incomingSignatureParts = requirements.IncomingSignatureParts;
                    }
                    else
                    {
                        incomingSignatureParts = requirements.OutgoingSignatureParts;
                    }
                    if (incomingSignatureParts.TryGetParts(description2.Action, out specification))
                    {
                        AddAssertionIfNotNull(policyContext, description2, securityPolicyDriver.CreateWsspSignedPartsAssertion(specification));
                    }
                    if (description2.Direction == MessageDirection.Input)
                    {
                        incomingSignatureParts = requirements.IncomingEncryptionParts;
                    }
                    else
                    {
                        incomingSignatureParts = requirements.OutgoingEncryptionParts;
                    }
                    if (incomingSignatureParts.TryGetParts(description2.Action, out specification))
                    {
                        AddAssertionIfNotNull(policyContext, description2, securityPolicyDriver.CreateWsspEncryptedPartsAssertion(specification));
                    }
                }
                foreach (FaultDescription description3 in description.Faults)
                {
                    MessagePartSpecification specification3;
                    if (requirements.OutgoingSignatureParts.TryGetParts(description3.Action, out specification3))
                    {
                        AddAssertionIfNotNull(policyContext, description3, securityPolicyDriver.CreateWsspSignedPartsAssertion(specification3));
                    }
                    if (requirements.OutgoingEncryptionParts.TryGetParts(description3.Action, out specification3))
                    {
                        AddAssertionIfNotNull(policyContext, description3, securityPolicyDriver.CreateWsspEncryptedPartsAssertion(specification3));
                    }
                }
            }
        }

        private static void ExportOperationScopeSupportingTokensPolicy(SecurityBindingElement binding, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy securityPolicyDriver = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);
            if ((binding.OperationSupportingTokenParameters.Count != 0) || (binding.OptionalOperationSupportingTokenParameters.Count != 0))
            {
                foreach (OperationDescription description in policyContext.Contract.Operations)
                {
                    foreach (MessageDescription description2 in description.Messages)
                    {
                        if (description2.Direction == MessageDirection.Input)
                        {
                            SupportingTokenParameters parameters = null;
                            SupportingTokenParameters parameters2 = null;
                            if (binding.OperationSupportingTokenParameters.ContainsKey(description2.Action))
                            {
                                parameters = binding.OperationSupportingTokenParameters[description2.Action];
                            }
                            if (binding.OptionalOperationSupportingTokenParameters.ContainsKey(description2.Action))
                            {
                                parameters2 = binding.OptionalOperationSupportingTokenParameters[description2.Action];
                            }
                            if ((parameters != null) || (parameters2 != null))
                            {
                                AddAssertionIfNotNull(policyContext, description, securityPolicyDriver.CreateWsspSupportingTokensAssertion(exporter, (parameters == null) ? null : parameters.Signed, (parameters == null) ? null : parameters.SignedEncrypted, (parameters == null) ? null : parameters.Endorsing, (parameters == null) ? null : parameters.SignedEndorsing, (parameters2 == null) ? null : parameters2.Signed, (parameters2 == null) ? null : parameters2.SignedEncrypted, (parameters2 == null) ? null : parameters2.Endorsing, (parameters2 == null) ? null : parameters2.SignedEndorsing));
                            }
                        }
                    }
                }
            }
        }

        internal static void ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            SecurityTraceRecordHelper.TraceExportChannelBindingEntry();
            SecurityBindingElement binding = null;
            BindingElementCollection elements = new BindingElementCollection();
            if ((context != null) && (context.BindingElements != null))
            {
                foreach (BindingElement element2 in context.BindingElements)
                {
                    if (element2 is SecurityBindingElement)
                    {
                        binding = (SecurityBindingElement) element2;
                    }
                    else
                    {
                        if (((binding != null) || (element2 is MessageEncodingBindingElement)) || (element2 is ITransportTokenAssertionProvider))
                        {
                            elements.Add(element2);
                        }
                        if (element2 is ITransportTokenAssertionProvider)
                        {
                            ITransportTokenAssertionProvider provider1 = (ITransportTokenAssertionProvider) element2;
                        }
                    }
                }
            }
            exporter.State["SecureConversationBootstrapBindingElementsBelowSecurityKey"] = elements;
            bool flag = false;
            try
            {
                if (binding is SymmetricSecurityBindingElement)
                {
                    ExportSymmetricSecurityBindingElement((SymmetricSecurityBindingElement) binding, exporter, context);
                    ExportOperationScopeSupportingTokensPolicy(binding, exporter, context);
                    ExportMessageScopeProtectionPolicy(binding, exporter, context);
                }
                else if (binding is AsymmetricSecurityBindingElement)
                {
                    ExportAsymmetricSecurityBindingElement((AsymmetricSecurityBindingElement) binding, exporter, context);
                    ExportOperationScopeSupportingTokensPolicy(binding, exporter, context);
                    ExportMessageScopeProtectionPolicy(binding, exporter, context);
                }
                flag = true;
            }
            finally
            {
                try
                {
                    exporter.State.Remove("SecureConversationBootstrapBindingElementsBelowSecurityKey");
                }
                catch (Exception exception)
                {
                    if (flag || Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
            }
        }

        internal static void ExportPolicyForTransportTokenAssertionProviders(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            SecurityTraceRecordHelper.TraceExportChannelBindingEntry();
            SecurityBindingElement binding = null;
            ITransportTokenAssertionProvider transportTokenAssertionProvider = null;
            BindingElementCollection elements = new BindingElementCollection();
            if ((context != null) && (context.BindingElements != null))
            {
                foreach (BindingElement element2 in context.BindingElements)
                {
                    if (element2 is SecurityBindingElement)
                    {
                        binding = (SecurityBindingElement) element2;
                    }
                    else
                    {
                        if (((binding != null) || (element2 is MessageEncodingBindingElement)) || (element2 is ITransportTokenAssertionProvider))
                        {
                            elements.Add(element2);
                        }
                        if (element2 is ITransportTokenAssertionProvider)
                        {
                            transportTokenAssertionProvider = (ITransportTokenAssertionProvider) element2;
                        }
                    }
                }
            }
            exporter.State["SecureConversationBootstrapBindingElementsBelowSecurityKey"] = elements;
            bool flag = false;
            try
            {
                if (binding is TransportSecurityBindingElement)
                {
                    if (transportTokenAssertionProvider == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ExportOfBindingWithTransportSecurityBindingElementAndNoTransportSecurityNotSupported")));
                    }
                    ExportTransportSecurityBindingElement((TransportSecurityBindingElement) binding, transportTokenAssertionProvider, exporter, context);
                    ExportOperationScopeSupportingTokensPolicy(binding, exporter, context);
                }
                else if (transportTokenAssertionProvider != null)
                {
                    TransportSecurityBindingElement element3 = new TransportSecurityBindingElement();
                    if (binding == null)
                    {
                        element3.IncludeTimestamp = false;
                    }
                    HttpsTransportBindingElement element4 = transportTokenAssertionProvider as HttpsTransportBindingElement;
                    if ((element4 != null) && (element4.MessageSecurityVersion != null))
                    {
                        element3.MessageSecurityVersion = element4.MessageSecurityVersion;
                    }
                    ExportTransportSecurityBindingElement(element3, transportTokenAssertionProvider, exporter, context);
                }
                flag = true;
            }
            finally
            {
                try
                {
                    exporter.State.Remove("SecureConversationBootstrapBindingElementsBelowSecurityKey");
                }
                catch (Exception exception)
                {
                    if (flag || Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
            }
        }

        private static void ExportSymmetricSecurityBindingElement(SymmetricSecurityBindingElement binding, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy securityPolicyDriver = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);
            AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspSymmetricBindingAssertion(exporter, policyContext, binding));
            AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspSupportingTokensAssertion(exporter, binding.EndpointSupportingTokenParameters.Signed, binding.EndpointSupportingTokenParameters.SignedEncrypted, binding.EndpointSupportingTokenParameters.Endorsing, binding.EndpointSupportingTokenParameters.SignedEndorsing, binding.OptionalEndpointSupportingTokenParameters.Signed, binding.OptionalEndpointSupportingTokenParameters.SignedEncrypted, binding.OptionalEndpointSupportingTokenParameters.Endorsing, binding.OptionalEndpointSupportingTokenParameters.SignedEndorsing));
            AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspWssAssertion(exporter, binding));
            if (RequiresWsspTrust(binding))
            {
                AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspTrustAssertion(exporter, binding.KeyEntropyMode));
            }
        }

        private static void ExportTransportSecurityBindingElement(TransportSecurityBindingElement binding, ITransportTokenAssertionProvider transportTokenAssertionProvider, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy securityPolicyDriver = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);
            XmlElement transportTokenAssertion = transportTokenAssertionProvider.GetTransportTokenAssertion();
            if (transportTokenAssertion == null)
            {
                if (transportTokenAssertionProvider is HttpsTransportBindingElement)
                {
                    transportTokenAssertion = securityPolicyDriver.CreateWsspHttpsTokenAssertion(exporter, (HttpsTransportBindingElement) transportTokenAssertionProvider);
                }
                if (transportTokenAssertion == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoTransportTokenAssertionProvided", new object[] { transportTokenAssertionProvider.GetType().ToString() })));
                }
            }
            AddressingVersion addressingVersion = AddressingVersion.WSAddressing10;
            MessageEncodingBindingElement element2 = policyContext.BindingElements.Find<MessageEncodingBindingElement>();
            if (element2 != null)
            {
                addressingVersion = element2.MessageVersion.Addressing;
            }
            AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspTransportBindingAssertion(exporter, binding, transportTokenAssertion));
            Collection<XmlElement> assertions = securityPolicyDriver.CreateWsspSupportingTokensAssertion(exporter, binding.EndpointSupportingTokenParameters.Signed, binding.EndpointSupportingTokenParameters.SignedEncrypted, binding.EndpointSupportingTokenParameters.Endorsing, binding.EndpointSupportingTokenParameters.SignedEndorsing, binding.OptionalEndpointSupportingTokenParameters.Signed, binding.OptionalEndpointSupportingTokenParameters.SignedEncrypted, binding.OptionalEndpointSupportingTokenParameters.Endorsing, binding.OptionalEndpointSupportingTokenParameters.SignedEndorsing, addressingVersion);
            AddAssertionIfNotNull(policyContext, assertions);
            if ((assertions.Count > 0) || HasEndorsingSupportingTokensAtOperationScope(binding))
            {
                AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspWssAssertion(exporter, binding));
                if (RequiresWsspTrust(binding))
                {
                    AddAssertionIfNotNull(policyContext, securityPolicyDriver.CreateWsspTrustAssertion(exporter, binding.KeyEntropyMode));
                }
            }
        }

        internal abstract ISecurityCapabilities GetIndividualISecurityCapabilities();
        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) this.GetSecurityCapabilities(context);
            }
            if (typeof(T) == typeof(IdentityVerifier))
            {
                return (T) this.localClientSettings.IdentityVerifier;
            }
            return context.GetInnerProperty<T>();
        }

        internal ChannelProtectionRequirements GetProtectionRequirements(AddressingVersion addressing, ProtectionLevel defaultProtectionLevel)
        {
            if (addressing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressing");
            }
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            ProtectionLevel supportedRequestProtectionLevel = base.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel;
            ProtectionLevel supportedResponseProtectionLevel = base.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel;
            if (ProtectionLevelHelper.IsStrongerOrEqual(supportedRequestProtectionLevel, defaultProtectionLevel) && ProtectionLevelHelper.IsStrongerOrEqual(supportedResponseProtectionLevel, defaultProtectionLevel))
            {
                MessagePartSpecification parts = new MessagePartSpecification();
                MessagePartSpecification specification2 = new MessagePartSpecification();
                if (defaultProtectionLevel != ProtectionLevel.None)
                {
                    parts.IsBodyIncluded = true;
                    if (defaultProtectionLevel == ProtectionLevel.EncryptAndSign)
                    {
                        specification2.IsBodyIncluded = true;
                    }
                }
                parts.MakeReadOnly();
                specification2.MakeReadOnly();
                if (addressing.FaultAction != null)
                {
                    requirements.IncomingSignatureParts.AddParts(parts, addressing.FaultAction);
                    requirements.OutgoingSignatureParts.AddParts(parts, addressing.FaultAction);
                    requirements.IncomingEncryptionParts.AddParts(specification2, addressing.FaultAction);
                    requirements.OutgoingEncryptionParts.AddParts(specification2, addressing.FaultAction);
                }
                if (addressing.DefaultFaultAction != null)
                {
                    requirements.IncomingSignatureParts.AddParts(parts, addressing.DefaultFaultAction);
                    requirements.OutgoingSignatureParts.AddParts(parts, addressing.DefaultFaultAction);
                    requirements.IncomingEncryptionParts.AddParts(specification2, addressing.DefaultFaultAction);
                    requirements.OutgoingEncryptionParts.AddParts(specification2, addressing.DefaultFaultAction);
                }
                requirements.IncomingSignatureParts.AddParts(parts, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault");
                requirements.OutgoingSignatureParts.AddParts(parts, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault");
                requirements.IncomingEncryptionParts.AddParts(specification2, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault");
                requirements.OutgoingEncryptionParts.AddParts(specification2, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault");
            }
            return requirements;
        }

        private ISecurityCapabilities GetSecurityCapabilities(BindingContext context)
        {
            ISecurityCapabilities individualISecurityCapabilities = this.GetIndividualISecurityCapabilities();
            ISecurityCapabilities innerProperty = context.GetInnerProperty<ISecurityCapabilities>();
            if (innerProperty == null)
            {
                return individualISecurityCapabilities;
            }
            bool supportsClientAuthentication = individualISecurityCapabilities.SupportsClientAuthentication;
            bool supportsClientWindowsIdentity = individualISecurityCapabilities.SupportsClientWindowsIdentity;
            bool supportsServerAuth = individualISecurityCapabilities.SupportsServerAuthentication || innerProperty.SupportsServerAuthentication;
            ProtectionLevel requestProtectionLevel = ProtectionLevelHelper.Max(individualISecurityCapabilities.SupportedRequestProtectionLevel, innerProperty.SupportedRequestProtectionLevel);
            return new SecurityCapabilities(supportsClientAuthentication, supportsServerAuth, supportsClientWindowsIdentity, requestProtectionLevel, ProtectionLevelHelper.Max(individualISecurityCapabilities.SupportedResponseProtectionLevel, innerProperty.SupportedResponseProtectionLevel));
        }

        internal void GetSupportingTokensCapabilities(out bool supportsClientAuth, out bool supportsWindowsIdentity)
        {
            this.GetSupportingTokensCapabilities(this.EndpointSupportingTokenParameters, out supportsClientAuth, out supportsWindowsIdentity);
        }

        private void GetSupportingTokensCapabilities(SupportingTokenParameters requirements, out bool supportsClientAuth, out bool supportsWindowsIdentity)
        {
            bool flag;
            bool flag2;
            supportsClientAuth = false;
            supportsWindowsIdentity = false;
            this.GetSupportingTokensCapabilities(requirements.Endorsing, out flag, out flag2);
            supportsClientAuth = supportsClientAuth || flag;
            supportsWindowsIdentity = supportsWindowsIdentity || flag2;
            this.GetSupportingTokensCapabilities(requirements.SignedEndorsing, out flag, out flag2);
            supportsClientAuth = supportsClientAuth || flag;
            supportsWindowsIdentity = supportsWindowsIdentity || flag2;
            this.GetSupportingTokensCapabilities(requirements.SignedEncrypted, out flag, out flag2);
            supportsClientAuth = supportsClientAuth || flag;
            supportsWindowsIdentity = supportsWindowsIdentity || flag2;
        }

        private void GetSupportingTokensCapabilities(ICollection<SecurityTokenParameters> parameters, out bool supportsClientAuth, out bool supportsWindowsIdentity)
        {
            supportsClientAuth = false;
            supportsWindowsIdentity = false;
            foreach (SecurityTokenParameters parameters2 in parameters)
            {
                if (parameters2.SupportsClientAuthentication)
                {
                    supportsClientAuth = true;
                }
                if (parameters2.SupportsClientWindowsIdentity)
                {
                    supportsWindowsIdentity = true;
                }
            }
        }

        private static bool HasEndorsingSupportingTokensAtOperationScope(SecurityBindingElement binding)
        {
            foreach (SupportingTokenParameters parameters in binding.OperationSupportingTokenParameters.Values)
            {
                if ((parameters.Endorsing.Count > 0) || (parameters.SignedEndorsing.Count > 0))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsAnonymousForCertificateBinding(SecurityBindingElement sbe)
        {
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!element.RequireSignatureConfirmation)
            {
                return false;
            }
            X509SecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as X509SecurityTokenParameters;
            if (((protectionTokenParameters == null) || (protectionTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint)) || (protectionTokenParameters.InclusionMode != SecurityTokenInclusionMode.Never))
            {
                return false;
            }
            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
            {
                return false;
            }
            return true;
        }

        internal static bool IsCertificateOverTransportBinding(SecurityBindingElement sbe)
        {
            if (!sbe.IncludeTimestamp)
            {
                return false;
            }
            if (!(sbe is TransportSecurityBindingElement))
            {
                return false;
            }
            SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
            if (((endpointSupportingTokenParameters.Signed.Count != 0) || (endpointSupportingTokenParameters.SignedEncrypted.Count != 0)) || ((endpointSupportingTokenParameters.Endorsing.Count != 1) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0)))
            {
                return false;
            }
            X509SecurityTokenParameters parameters2 = endpointSupportingTokenParameters.Endorsing[0] as X509SecurityTokenParameters;
            if (parameters2 == null)
            {
                return false;
            }
            if (parameters2.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient)
            {
                return false;
            }
            if (parameters2.X509ReferenceStyle != X509KeyIdentifierClauseType.Any)
            {
                return (parameters2.X509ReferenceStyle == X509KeyIdentifierClauseType.Thumbprint);
            }
            return true;
        }

        internal static bool IsIssuedTokenForCertificateBinding(SecurityBindingElement sbe, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            issuedTokenParameters = null;
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!element.RequireSignatureConfirmation)
            {
                return false;
            }
            X509SecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as X509SecurityTokenParameters;
            if (((protectionTokenParameters == null) || (protectionTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint)) || (protectionTokenParameters.InclusionMode != SecurityTokenInclusionMode.Never))
            {
                return false;
            }
            SupportingTokenParameters endpointSupportingTokenParameters = element.EndpointSupportingTokenParameters;
            if (((endpointSupportingTokenParameters.Signed.Count != 0) || ((endpointSupportingTokenParameters.SignedEncrypted.Count == 0) && (endpointSupportingTokenParameters.Endorsing.Count == 0))) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0))
            {
                return false;
            }
            if ((endpointSupportingTokenParameters.SignedEncrypted.Count == 1) && (endpointSupportingTokenParameters.Endorsing.Count == 0))
            {
                issuedTokenParameters = endpointSupportingTokenParameters.SignedEncrypted[0] as IssuedSecurityTokenParameters;
                if ((issuedTokenParameters != null) && (issuedTokenParameters.KeyType != SecurityKeyType.BearerKey))
                {
                    return false;
                }
            }
            else if ((endpointSupportingTokenParameters.Endorsing.Count == 1) && (endpointSupportingTokenParameters.SignedEncrypted.Count == 0))
            {
                issuedTokenParameters = endpointSupportingTokenParameters.Endorsing[0] as IssuedSecurityTokenParameters;
                if (((issuedTokenParameters != null) && (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey)) && (issuedTokenParameters.KeyType != SecurityKeyType.AsymmetricKey))
                {
                    return false;
                }
            }
            return (issuedTokenParameters != null);
        }

        internal static bool IsIssuedTokenForSslBinding(SecurityBindingElement sbe, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            return IsIssuedTokenForSslBinding(sbe, false, out issuedTokenParameters);
        }

        internal static bool IsIssuedTokenForSslBinding(SecurityBindingElement sbe, bool requireCancellation, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            issuedTokenParameters = null;
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!element.RequireSignatureConfirmation)
            {
                return false;
            }
            SslSecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as SslSecurityTokenParameters;
            if (protectionTokenParameters == null)
            {
                return false;
            }
            if (protectionTokenParameters.RequireClientCertificate || (protectionTokenParameters.RequireCancellation != requireCancellation))
            {
                return false;
            }
            SupportingTokenParameters endpointSupportingTokenParameters = element.EndpointSupportingTokenParameters;
            if (((endpointSupportingTokenParameters.Signed.Count != 0) || ((endpointSupportingTokenParameters.SignedEncrypted.Count == 0) && (endpointSupportingTokenParameters.Endorsing.Count == 0))) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0))
            {
                return false;
            }
            if ((endpointSupportingTokenParameters.SignedEncrypted.Count == 1) && (endpointSupportingTokenParameters.Endorsing.Count == 0))
            {
                issuedTokenParameters = endpointSupportingTokenParameters.SignedEncrypted[0] as IssuedSecurityTokenParameters;
                if ((issuedTokenParameters != null) && (issuedTokenParameters.KeyType != SecurityKeyType.BearerKey))
                {
                    return false;
                }
            }
            else if ((endpointSupportingTokenParameters.Endorsing.Count == 1) && (endpointSupportingTokenParameters.SignedEncrypted.Count == 0))
            {
                issuedTokenParameters = endpointSupportingTokenParameters.Endorsing[0] as IssuedSecurityTokenParameters;
                if (((issuedTokenParameters != null) && (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey)) && (issuedTokenParameters.KeyType != SecurityKeyType.AsymmetricKey))
                {
                    return false;
                }
            }
            return (issuedTokenParameters != null);
        }

        internal static bool IsIssuedTokenOverTransportBinding(SecurityBindingElement sbe, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            issuedTokenParameters = null;
            if (!(sbe is TransportSecurityBindingElement))
            {
                return false;
            }
            if (!sbe.IncludeTimestamp)
            {
                return false;
            }
            SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
            if (((endpointSupportingTokenParameters.SignedEncrypted.Count != 0) || ((endpointSupportingTokenParameters.Signed.Count == 0) && (endpointSupportingTokenParameters.Endorsing.Count == 0))) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0))
            {
                return false;
            }
            if ((endpointSupportingTokenParameters.Signed.Count == 1) && (endpointSupportingTokenParameters.Endorsing.Count == 0))
            {
                issuedTokenParameters = endpointSupportingTokenParameters.Signed[0] as IssuedSecurityTokenParameters;
                if ((issuedTokenParameters != null) && (issuedTokenParameters.KeyType != SecurityKeyType.BearerKey))
                {
                    return false;
                }
            }
            else if ((endpointSupportingTokenParameters.Endorsing.Count == 1) && (endpointSupportingTokenParameters.Signed.Count == 0))
            {
                issuedTokenParameters = endpointSupportingTokenParameters.Endorsing[0] as IssuedSecurityTokenParameters;
                if (((issuedTokenParameters != null) && (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey)) && (issuedTokenParameters.KeyType != SecurityKeyType.AsymmetricKey))
                {
                    return false;
                }
            }
            if (issuedTokenParameters == null)
            {
                return false;
            }
            if (issuedTokenParameters.RequireDerivedKeys)
            {
                return false;
            }
            return true;
        }

        internal static bool IsKerberosBinding(SecurityBindingElement sbe)
        {
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!(element.ProtectionTokenParameters is KerberosSecurityTokenParameters))
            {
                return false;
            }
            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
            {
                return false;
            }
            return true;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            SecurityBindingElement element = b as SecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            return SecurityElementBase.AreBindingsMatching(this, element);
        }

        internal static bool IsMutualCertificateBinding(SecurityBindingElement sbe)
        {
            return IsMutualCertificateBinding(sbe, false);
        }

        internal static bool IsMutualCertificateBinding(SecurityBindingElement sbe, bool allowSerializedSigningTokenOnReply)
        {
            AsymmetricSecurityBindingElement element = sbe as AsymmetricSecurityBindingElement;
            if (element != null)
            {
                X509SecurityTokenParameters recipientTokenParameters = element.RecipientTokenParameters as X509SecurityTokenParameters;
                if (((recipientTokenParameters == null) || (recipientTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Any)) || (recipientTokenParameters.InclusionMode != SecurityTokenInclusionMode.Never))
                {
                    return false;
                }
                X509SecurityTokenParameters initiatorTokenParameters = element.InitiatorTokenParameters as X509SecurityTokenParameters;
                if (((initiatorTokenParameters == null) || (initiatorTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Any)) || (initiatorTokenParameters.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient))
                {
                    return false;
                }
                if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
                {
                    return false;
                }
            }
            else
            {
                SymmetricSecurityBindingElement element2 = sbe as SymmetricSecurityBindingElement;
                if (element2 == null)
                {
                    return false;
                }
                X509SecurityTokenParameters protectionTokenParameters = element2.ProtectionTokenParameters as X509SecurityTokenParameters;
                if (((protectionTokenParameters == null) || (protectionTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint)) || (protectionTokenParameters.InclusionMode != SecurityTokenInclusionMode.Never))
                {
                    return false;
                }
                SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
                if (((endpointSupportingTokenParameters.Signed.Count != 0) || (endpointSupportingTokenParameters.SignedEncrypted.Count != 0)) || ((endpointSupportingTokenParameters.Endorsing.Count != 1) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0)))
                {
                    return false;
                }
                protectionTokenParameters = endpointSupportingTokenParameters.Endorsing[0] as X509SecurityTokenParameters;
                if (((protectionTokenParameters == null) || (protectionTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint)) || (protectionTokenParameters.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient))
                {
                    return false;
                }
                if (!element2.RequireSignatureConfirmation)
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsMutualCertificateDuplexBinding(SecurityBindingElement sbe)
        {
            AsymmetricSecurityBindingElement element = sbe as AsymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            X509SecurityTokenParameters recipientTokenParameters = element.RecipientTokenParameters as X509SecurityTokenParameters;
            if (((recipientTokenParameters == null) || ((recipientTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Any) && (recipientTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint))) || (recipientTokenParameters.InclusionMode != SecurityTokenInclusionMode.AlwaysToInitiator))
            {
                return false;
            }
            X509SecurityTokenParameters initiatorTokenParameters = element.InitiatorTokenParameters as X509SecurityTokenParameters;
            if (((initiatorTokenParameters == null) || ((initiatorTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Any) && (initiatorTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint))) || (initiatorTokenParameters.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient))
            {
                return false;
            }
            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
            {
                return false;
            }
            return true;
        }

        internal static bool IsSecureConversationBinding(SecurityBindingElement sbe, out SecurityBindingElement bootstrapSecurity)
        {
            return IsSecureConversationBinding(sbe, true, out bootstrapSecurity);
        }

        internal static bool IsSecureConversationBinding(SecurityBindingElement sbe, bool requireCancellation, out SecurityBindingElement bootstrapSecurity)
        {
            bootstrapSecurity = null;
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element != null)
            {
                if (element.RequireSignatureConfirmation)
                {
                    return false;
                }
                SecureConversationSecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as SecureConversationSecurityTokenParameters;
                if (protectionTokenParameters == null)
                {
                    return false;
                }
                if (protectionTokenParameters.RequireCancellation != requireCancellation)
                {
                    return false;
                }
                bootstrapSecurity = protectionTokenParameters.BootstrapSecurityBindingElement;
            }
            else
            {
                if (!sbe.IncludeTimestamp)
                {
                    return false;
                }
                if (!(sbe is TransportSecurityBindingElement))
                {
                    return false;
                }
                SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
                if (((endpointSupportingTokenParameters.Signed.Count != 0) || (endpointSupportingTokenParameters.SignedEncrypted.Count != 0)) || ((endpointSupportingTokenParameters.Endorsing.Count != 1) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0)))
                {
                    return false;
                }
                SecureConversationSecurityTokenParameters parameters3 = endpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
                if (parameters3 == null)
                {
                    return false;
                }
                if (parameters3.RequireCancellation != requireCancellation)
                {
                    return false;
                }
                bootstrapSecurity = parameters3.BootstrapSecurityBindingElement;
            }
            if ((bootstrapSecurity != null) && (bootstrapSecurity.SecurityHeaderLayout != System.ServiceModel.Channels.SecurityHeaderLayout.Strict))
            {
                return false;
            }
            return (bootstrapSecurity != null);
        }

        internal virtual bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            if (!this.EndpointSupportingTokenParameters.IsSetKeyDerivation(requireDerivedKeys))
            {
                return false;
            }
            if (!this.OptionalEndpointSupportingTokenParameters.IsSetKeyDerivation(requireDerivedKeys))
            {
                return false;
            }
            foreach (SupportingTokenParameters parameters in this.OperationSupportingTokenParameters.Values)
            {
                if (!parameters.IsSetKeyDerivation(requireDerivedKeys))
                {
                    return false;
                }
            }
            foreach (SupportingTokenParameters parameters2 in this.OptionalOperationSupportingTokenParameters.Values)
            {
                if (!parameters2.IsSetKeyDerivation(requireDerivedKeys))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsSslNegotiationBinding(SecurityBindingElement sbe, bool requireClientCertificate, bool requireCancellation)
        {
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
            {
                return false;
            }
            SslSecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as SslSecurityTokenParameters;
            if (protectionTokenParameters == null)
            {
                return false;
            }
            return ((protectionTokenParameters.RequireClientCertificate == requireClientCertificate) && (protectionTokenParameters.RequireCancellation == requireCancellation));
        }

        internal static bool IsSspiNegotiationBinding(SecurityBindingElement sbe, bool requireCancellation)
        {
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
            {
                return false;
            }
            SspiSecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as SspiSecurityTokenParameters;
            if (protectionTokenParameters == null)
            {
                return false;
            }
            return (protectionTokenParameters.RequireCancellation == requireCancellation);
        }

        internal static bool IsSspiNegotiationOverTransportBinding(SecurityBindingElement sbe, bool requireCancellation)
        {
            if (sbe.IncludeTimestamp)
            {
                SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
                if (((endpointSupportingTokenParameters.Signed.Count != 0) || (endpointSupportingTokenParameters.SignedEncrypted.Count != 0)) || ((endpointSupportingTokenParameters.Endorsing.Count != 1) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0)))
                {
                    return false;
                }
                SspiSecurityTokenParameters parameters2 = endpointSupportingTokenParameters.Endorsing[0] as SspiSecurityTokenParameters;
                if (parameters2 != null)
                {
                    if (parameters2.RequireDerivedKeys)
                    {
                        return false;
                    }
                    if (parameters2.RequireCancellation != requireCancellation)
                    {
                        return false;
                    }
                    if (sbe is TransportSecurityBindingElement)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool IsUnderlyingListenerDuplex<TChannel>(BindingContext context)
        {
            return (((typeof(TChannel) == typeof(IDuplexSessionChannel)) && context.CanBuildInnerChannelListener<IDuplexChannel>()) && !context.CanBuildInnerChannelListener<IDuplexSessionChannel>());
        }

        internal static bool IsUserNameForCertificateBinding(SecurityBindingElement sbe)
        {
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element != null)
            {
                X509SecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as X509SecurityTokenParameters;
                if (((protectionTokenParameters == null) || (protectionTokenParameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint)) || (protectionTokenParameters.InclusionMode != SecurityTokenInclusionMode.Never))
                {
                    return false;
                }
                SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
                if (((endpointSupportingTokenParameters.Signed.Count != 0) || (endpointSupportingTokenParameters.SignedEncrypted.Count != 1)) || ((endpointSupportingTokenParameters.Endorsing.Count != 0) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0)))
                {
                    return false;
                }
                if (endpointSupportingTokenParameters.SignedEncrypted[0] is UserNameSecurityTokenParameters)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsUserNameForSslBinding(SecurityBindingElement sbe, bool requireCancellation)
        {
            SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
            if (element == null)
            {
                return false;
            }
            SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
            if (((endpointSupportingTokenParameters.Signed.Count != 0) || (endpointSupportingTokenParameters.SignedEncrypted.Count != 1)) || ((endpointSupportingTokenParameters.Endorsing.Count != 0) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0)))
            {
                return false;
            }
            if (!(endpointSupportingTokenParameters.SignedEncrypted[0] is UserNameSecurityTokenParameters))
            {
                return false;
            }
            SslSecurityTokenParameters protectionTokenParameters = element.ProtectionTokenParameters as SslSecurityTokenParameters;
            if (protectionTokenParameters == null)
            {
                return false;
            }
            return ((protectionTokenParameters.RequireCancellation == requireCancellation) && !protectionTokenParameters.RequireClientCertificate);
        }

        internal static bool IsUserNameOverTransportBinding(SecurityBindingElement sbe)
        {
            if (sbe.IncludeTimestamp)
            {
                if (!(sbe is TransportSecurityBindingElement))
                {
                    return false;
                }
                SupportingTokenParameters endpointSupportingTokenParameters = sbe.EndpointSupportingTokenParameters;
                if (((endpointSupportingTokenParameters.Signed.Count != 0) || (endpointSupportingTokenParameters.SignedEncrypted.Count != 1)) || ((endpointSupportingTokenParameters.Endorsing.Count != 0) || (endpointSupportingTokenParameters.SignedEndorsing.Count != 0)))
                {
                    return false;
                }
                if (endpointSupportingTokenParameters.SignedEncrypted[0] is UserNameSecurityTokenParameters)
                {
                    return true;
                }
            }
            return false;
        }

        internal virtual bool RequiresChannelDemuxer()
        {
            foreach (SecurityTokenParameters parameters in this.EndpointSupportingTokenParameters.Endorsing)
            {
                if (this.RequiresChannelDemuxer(parameters))
                {
                    return true;
                }
            }
            foreach (SecurityTokenParameters parameters2 in this.EndpointSupportingTokenParameters.SignedEndorsing)
            {
                if (this.RequiresChannelDemuxer(parameters2))
                {
                    return true;
                }
            }
            foreach (SecurityTokenParameters parameters3 in this.OptionalEndpointSupportingTokenParameters.Endorsing)
            {
                if (this.RequiresChannelDemuxer(parameters3))
                {
                    return true;
                }
            }
            foreach (SecurityTokenParameters parameters4 in this.OptionalEndpointSupportingTokenParameters.SignedEndorsing)
            {
                if (this.RequiresChannelDemuxer(parameters4))
                {
                    return true;
                }
            }
            foreach (SupportingTokenParameters parameters5 in this.OperationSupportingTokenParameters.Values)
            {
                foreach (SecurityTokenParameters parameters6 in parameters5.Endorsing)
                {
                    if (this.RequiresChannelDemuxer(parameters6))
                    {
                        return true;
                    }
                }
                foreach (SecurityTokenParameters parameters7 in parameters5.SignedEndorsing)
                {
                    if (this.RequiresChannelDemuxer(parameters7))
                    {
                        return true;
                    }
                }
            }
            foreach (SupportingTokenParameters parameters8 in this.OptionalOperationSupportingTokenParameters.Values)
            {
                foreach (SecurityTokenParameters parameters9 in parameters8.Endorsing)
                {
                    if (this.RequiresChannelDemuxer(parameters9))
                    {
                        return true;
                    }
                }
                foreach (SecurityTokenParameters parameters10 in parameters8.SignedEndorsing)
                {
                    if (this.RequiresChannelDemuxer(parameters10))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool RequiresChannelDemuxer(SecurityTokenParameters parameters)
        {
            return (((parameters is SecureConversationSecurityTokenParameters) || (parameters is SslSecurityTokenParameters)) || (parameters is SspiSecurityTokenParameters));
        }

        private static bool RequiresWsspTrust(SecurityBindingElement sbe)
        {
            if (sbe == null)
            {
                return false;
            }
            return !sbe.doNotEmitTrust;
        }

        private void SetIssuerBindingContextIfRequired(BindingContext issuerBindingContext)
        {
            SetIssuerBindingContextIfRequired(this.EndpointSupportingTokenParameters, issuerBindingContext);
            SetIssuerBindingContextIfRequired(this.OptionalEndpointSupportingTokenParameters, issuerBindingContext);
            foreach (SupportingTokenParameters parameters in this.OperationSupportingTokenParameters.Values)
            {
                SetIssuerBindingContextIfRequired(parameters, issuerBindingContext);
            }
            foreach (SupportingTokenParameters parameters2 in this.OptionalOperationSupportingTokenParameters.Values)
            {
                SetIssuerBindingContextIfRequired(parameters2, issuerBindingContext);
            }
        }

        protected static void SetIssuerBindingContextIfRequired(SecurityTokenParameters parameters, BindingContext issuerBindingContext)
        {
            if (parameters is SslSecurityTokenParameters)
            {
                ((SslSecurityTokenParameters) parameters).IssuerBindingContext = CreateIssuerBindingContextForNegotiation(issuerBindingContext);
            }
            else if (parameters is SspiSecurityTokenParameters)
            {
                ((SspiSecurityTokenParameters) parameters).IssuerBindingContext = CreateIssuerBindingContextForNegotiation(issuerBindingContext);
            }
        }

        private static void SetIssuerBindingContextIfRequired(SupportingTokenParameters supportingParameters, BindingContext issuerBindingContext)
        {
            for (int i = 0; i < supportingParameters.Endorsing.Count; i++)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.Endorsing[i], issuerBindingContext);
            }
            for (int j = 0; j < supportingParameters.SignedEndorsing.Count; j++)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.SignedEndorsing[j], issuerBindingContext);
            }
            for (int k = 0; k < supportingParameters.Signed.Count; k++)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.Signed[k], issuerBindingContext);
            }
            for (int m = 0; m < supportingParameters.SignedEncrypted.Count; m++)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.SignedEncrypted[m], issuerBindingContext);
            }
        }

        public virtual void SetKeyDerivation(bool requireDerivedKeys)
        {
            this.EndpointSupportingTokenParameters.SetKeyDerivation(requireDerivedKeys);
            this.OptionalEndpointSupportingTokenParameters.SetKeyDerivation(requireDerivedKeys);
            foreach (SupportingTokenParameters parameters in this.OperationSupportingTokenParameters.Values)
            {
                parameters.SetKeyDerivation(requireDerivedKeys);
            }
            foreach (SupportingTokenParameters parameters2 in this.OptionalOperationSupportingTokenParameters.Values)
            {
                parameters2.SetKeyDerivation(requireDerivedKeys);
            }
        }

        private void SetPrivacyNoticeUriIfRequired(SecurityProtocolFactory factory, Binding binding)
        {
            PrivacyNoticeBindingElement element = binding.CreateBindingElements().Find<PrivacyNoticeBindingElement>();
            if (element != null)
            {
                factory.PrivacyNoticeUri = element.Url;
                factory.PrivacyNoticeVersion = element.Version;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}:", new object[] { base.GetType().ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DefaultAlgorithmSuite: {0}", new object[] { this.defaultAlgorithmSuite.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "IncludeTimestamp: {0}", new object[] { this.includeTimestamp.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "KeyEntropyMode: {0}", new object[] { this.keyEntropyMode.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "MessageSecurityVersion: {0}", new object[] { this.MessageSecurityVersion.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "SecurityHeaderLayout: {0}", new object[] { this.securityHeaderLayout.ToString() }));
            builder.AppendLine("EndpointSupportingTokenParameters:");
            builder.AppendLine("  " + this.EndpointSupportingTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            builder.AppendLine("OptionalEndpointSupportingTokenParameters:");
            builder.AppendLine("  " + this.OptionalEndpointSupportingTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            if (this.operationSupportingTokenParameters.Count == 0)
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "OperationSupportingTokenParameters: none", new object[0]));
            }
            else
            {
                foreach (string str in this.OperationSupportingTokenParameters.Keys)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "OperationSupportingTokenParameters[\"{0}\"]:", new object[] { str }));
                    builder.AppendLine("  " + this.OperationSupportingTokenParameters[str].ToString().Trim().Replace("\n", "\n  "));
                }
            }
            if (this.optionalOperationSupportingTokenParameters.Count == 0)
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "OptionalOperationSupportingTokenParameters: none", new object[0]));
            }
            else
            {
                foreach (string str2 in this.OptionalOperationSupportingTokenParameters.Keys)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "OptionalOperationSupportingTokenParameters[\"{0}\"]:", new object[] { str2 }));
                    builder.AppendLine("  " + this.OptionalOperationSupportingTokenParameters[str2].ToString().Trim().Replace("\n", "\n  "));
                }
            }
            return builder.ToString().Trim();
        }

        public bool AllowInsecureTransport
        {
            get
            {
                return this.allowInsecureTransport;
            }
            set
            {
                this.allowInsecureTransport = value;
            }
        }

        public SecurityAlgorithmSuite DefaultAlgorithmSuite
        {
            get
            {
                return this.defaultAlgorithmSuite;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.defaultAlgorithmSuite = value;
            }
        }

        internal bool DoNotEmitTrust
        {
            get
            {
                return this.doNotEmitTrust;
            }
            set
            {
                this.doNotEmitTrust = value;
            }
        }

        public bool EnableUnsecuredResponse
        {
            get
            {
                return this.enableUnsecuredResponse;
            }
            set
            {
                this.enableUnsecuredResponse = value;
            }
        }

        public SupportingTokenParameters EndpointSupportingTokenParameters
        {
            get
            {
                return this.endpointSupportingTokenParameters;
            }
        }

        public bool IncludeTimestamp
        {
            get
            {
                return this.includeTimestamp;
            }
            set
            {
                this.includeTimestamp = value;
            }
        }

        public SecurityKeyEntropyMode KeyEntropyMode
        {
            get
            {
                return this.keyEntropyMode;
            }
            set
            {
                if (!SecurityKeyEntropyModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.keyEntropyMode = value;
            }
        }

        public LocalClientSecuritySettings LocalClientSettings
        {
            get
            {
                return this.localClientSettings;
            }
        }

        public LocalServiceSecuritySettings LocalServiceSettings
        {
            get
            {
                return this.localServiceSettings;
            }
        }

        internal long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
            set
            {
                this.maxReceivedMessageSize = value;
            }
        }

        public System.ServiceModel.MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return this.messageSecurityVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.messageSecurityVersion = value;
            }
        }

        public IDictionary<string, SupportingTokenParameters> OperationSupportingTokenParameters
        {
            get
            {
                return this.operationSupportingTokenParameters;
            }
        }

        public SupportingTokenParameters OptionalEndpointSupportingTokenParameters
        {
            get
            {
                return this.optionalEndpointSupportingTokenParameters;
            }
        }

        public IDictionary<string, SupportingTokenParameters> OptionalOperationSupportingTokenParameters
        {
            get
            {
                return this.optionalOperationSupportingTokenParameters;
            }
        }

        internal XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
            set
            {
                this.readerQuotas = value;
            }
        }

        public System.ServiceModel.Channels.SecurityHeaderLayout SecurityHeaderLayout
        {
            get
            {
                return this.securityHeaderLayout;
            }
            set
            {
                if (!SecurityHeaderLayoutHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.securityHeaderLayout = value;
            }
        }

        internal virtual bool SessionMode
        {
            get
            {
                return false;
            }
        }

        internal virtual bool SupportsDuplex
        {
            get
            {
                return false;
            }
        }

        internal bool SupportsExtendedProtectionPolicy
        {
            get
            {
                return this.supportsExtendedProtectionPolicy;
            }
            set
            {
                this.supportsExtendedProtectionPolicy = value;
            }
        }

        internal virtual bool SupportsRequestReply
        {
            get
            {
                return false;
            }
        }
    }
}

