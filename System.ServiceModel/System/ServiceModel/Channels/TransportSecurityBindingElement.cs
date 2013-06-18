namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public sealed class TransportSecurityBindingElement : SecurityBindingElement, IPolicyExportExtension
    {
        public TransportSecurityBindingElement()
        {
            base.LocalClientSettings.DetectReplays = base.LocalServiceSettings.DetectReplays = false;
        }

        private TransportSecurityBindingElement(TransportSecurityBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
        }

        protected override IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context)
        {
            ISecurityCapabilities property = this.GetProperty<ISecurityCapabilities>(context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }
            SecureConversationSecurityTokenParameters item = null;
            if (base.EndpointSupportingTokenParameters.Endorsing.Count > 0)
            {
                item = base.EndpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
            }
            bool addChannelDemuxerIfRequired = this.RequiresChannelDemuxer();
            ChannelBuilder builder = new ChannelBuilder(context, addChannelDemuxerIfRequired);
            if (addChannelDemuxerIfRequired)
            {
                base.ApplyPropertiesOnDemuxer(builder, context);
            }
            BindingContext issuerBindingContext = context.Clone();
            if (item != null)
            {
                if (item.BootstrapSecurityBindingElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationSecurityTokenParametersRequireBootstrapBinding")));
                }
                item.IssuerBindingContext = issuerBindingContext;
                if (item.RequireCancellation)
                {
                    SessionSymmetricTransportSecurityProtocolFactory factory2 = new SessionSymmetricTransportSecurityProtocolFactory {
                        SecurityTokenParameters = item.Clone()
                    };
                    ((SecureConversationSecurityTokenParameters) factory2.SecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    base.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                    try
                    {
                        base.ConfigureProtocolFactory(factory2, credentialsManager, false, issuerBindingContext, context.Binding);
                    }
                    finally
                    {
                        base.EndpointSupportingTokenParameters.Endorsing.Insert(0, item);
                    }
                    SecuritySessionClientSettings<TChannel> sessionClientSettings = new SecuritySessionClientSettings<TChannel> {
                        ChannelBuilder = builder,
                        KeyRenewalInterval = base.LocalClientSettings.SessionKeyRenewalInterval,
                        KeyRolloverInterval = base.LocalClientSettings.SessionKeyRolloverInterval,
                        TolerateTransportFailures = base.LocalClientSettings.ReconnectTransportOnFailure,
                        CanRenewSession = item.CanRenewSession,
                        IssuedSecurityTokenParameters = item.Clone()
                    };
                    ((SecureConversationSecurityTokenParameters) sessionClientSettings.IssuedSecurityTokenParameters).IssuerBindingContext = issuerBindingContext;
                    sessionClientSettings.SecurityStandardsManager = factory2.StandardsManager;
                    sessionClientSettings.SessionProtocolFactory = factory2;
                    return new SecurityChannelFactory<TChannel>(property, context, sessionClientSettings);
                }
                TransportSecurityProtocolFactory factory = new TransportSecurityProtocolFactory();
                base.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                try
                {
                    base.ConfigureProtocolFactory(factory, credentialsManager, false, issuerBindingContext, context.Binding);
                    SecureConversationSecurityTokenParameters parameters2 = (SecureConversationSecurityTokenParameters) item.Clone();
                    parameters2.IssuerBindingContext = issuerBindingContext;
                    factory.SecurityBindingElement.EndpointSupportingTokenParameters.Endorsing.Insert(0, parameters2);
                }
                finally
                {
                    base.EndpointSupportingTokenParameters.Endorsing.Insert(0, item);
                }
                return new SecurityChannelFactory<TChannel>(property, context, builder, factory);
            }
            return new SecurityChannelFactory<TChannel>(property, context, builder, this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, false, issuerBindingContext));
        }

        protected override IChannelListener<TChannel> BuildChannelListenerCore<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            SecureConversationSecurityTokenParameters parameters;
            SecurityChannelListener<TChannel> listener = new SecurityChannelListener<TChannel>(this, context);
            SecurityCredentialsManager credentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ServiceCredentials.CreateDefaultCredentials();
            }
            if (base.EndpointSupportingTokenParameters.Endorsing.Count > 0)
            {
                parameters = base.EndpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
            }
            else
            {
                parameters = null;
            }
            bool addChannelDemuxerIfRequired = this.RequiresChannelDemuxer();
            ChannelBuilder builder = new ChannelBuilder(context, addChannelDemuxerIfRequired);
            if (addChannelDemuxerIfRequired)
            {
                base.ApplyPropertiesOnDemuxer(builder, context);
            }
            BindingContext secureConversationBindingContext = context.Clone();
            if (parameters != null)
            {
                if (parameters.BootstrapSecurityBindingElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationSecurityTokenParametersRequireBootstrapBinding")));
                }
                base.AddDemuxerForSecureConversation(builder, secureConversationBindingContext);
                if (parameters.RequireCancellation)
                {
                    SessionSymmetricTransportSecurityProtocolFactory factory = new SessionSymmetricTransportSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, factory);
                    factory.SecurityTokenParameters = parameters.Clone();
                    ((SecureConversationSecurityTokenParameters) factory.SecurityTokenParameters).IssuerBindingContext = secureConversationBindingContext;
                    base.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                    try
                    {
                        base.ConfigureProtocolFactory(factory, credentialsManager, true, secureConversationBindingContext, context.Binding);
                    }
                    finally
                    {
                        base.EndpointSupportingTokenParameters.Endorsing.Insert(0, parameters);
                    }
                    listener.SessionMode = true;
                    listener.SessionServerSettings.InactivityTimeout = base.LocalServiceSettings.InactivityTimeout;
                    listener.SessionServerSettings.KeyRolloverInterval = base.LocalServiceSettings.SessionKeyRolloverInterval;
                    listener.SessionServerSettings.MaximumPendingSessions = base.LocalServiceSettings.MaxPendingSessions;
                    listener.SessionServerSettings.MaximumKeyRenewalInterval = base.LocalServiceSettings.SessionKeyRenewalInterval;
                    listener.SessionServerSettings.TolerateTransportFailures = base.LocalServiceSettings.ReconnectTransportOnFailure;
                    listener.SessionServerSettings.CanRenewSession = parameters.CanRenewSession;
                    listener.SessionServerSettings.IssuedSecurityTokenParameters = parameters.Clone();
                    ((SecureConversationSecurityTokenParameters) listener.SessionServerSettings.IssuedSecurityTokenParameters).IssuerBindingContext = secureConversationBindingContext;
                    listener.SessionServerSettings.SecurityStandardsManager = factory.StandardsManager;
                    listener.SessionServerSettings.SessionProtocolFactory = factory;
                    if (((context.BindingParameters != null) && (context.BindingParameters.Find<IChannelDemuxFailureHandler>() == null)) && !base.IsUnderlyingListenerDuplex<TChannel>(context))
                    {
                        context.BindingParameters.Add(new SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler(factory.StandardsManager));
                    }
                }
                else
                {
                    TransportSecurityProtocolFactory factory2 = new TransportSecurityProtocolFactory();
                    base.ApplyAuditBehaviorSettings(context, factory2);
                    base.EndpointSupportingTokenParameters.Endorsing.RemoveAt(0);
                    try
                    {
                        base.ConfigureProtocolFactory(factory2, credentialsManager, true, secureConversationBindingContext, context.Binding);
                        SecureConversationSecurityTokenParameters item = (SecureConversationSecurityTokenParameters) parameters.Clone();
                        item.IssuerBindingContext = secureConversationBindingContext;
                        factory2.SecurityBindingElement.EndpointSupportingTokenParameters.Endorsing.Insert(0, item);
                    }
                    finally
                    {
                        base.EndpointSupportingTokenParameters.Endorsing.Insert(0, parameters);
                    }
                    listener.SecurityProtocolFactory = factory2;
                }
            }
            else
            {
                SecurityProtocolFactory factory3 = this.CreateSecurityProtocolFactory<TChannel>(context, credentialsManager, true, secureConversationBindingContext);
                listener.SecurityProtocolFactory = factory3;
            }
            listener.InitializeListener(builder);
            return listener;
        }

        public override BindingElement Clone()
        {
            return new TransportSecurityBindingElement(this);
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
            TransportSecurityProtocolFactory factory = new TransportSecurityProtocolFactory();
            if (isForService)
            {
                base.ApplyAuditBehaviorSettings(context, factory);
            }
            base.ConfigureProtocolFactory(factory, credentialsManager, isForService, issuerBindingContext, context.Binding);
            factory.DetectReplays = false;
            return factory;
        }

        internal override ISecurityCapabilities GetIndividualISecurityCapabilities()
        {
            bool flag;
            bool flag2;
            base.GetSupportingTokensCapabilities(out flag, out flag2);
            return new SecurityCapabilities(flag, false, flag2, ProtectionLevel.None, ProtectionLevel.None);
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

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (policyContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
            }
            if (policyContext.BindingElements.Find<ITransportTokenAssertionProvider>() == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ExportOfBindingWithTransportSecurityBindingElementAndNoTransportSecurityNotSupported")));
            }
        }

        internal override bool SessionMode
        {
            get
            {
                SecureConversationSecurityTokenParameters parameters = null;
                if (base.EndpointSupportingTokenParameters.Endorsing.Count > 0)
                {
                    parameters = base.EndpointSupportingTokenParameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
                }
                return ((parameters != null) && parameters.RequireCancellation);
            }
        }

        internal override bool SupportsDuplex
        {
            get
            {
                return true;
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

