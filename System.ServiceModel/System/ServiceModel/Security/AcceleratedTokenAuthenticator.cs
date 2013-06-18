namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal sealed class AcceleratedTokenAuthenticator : NegotiationTokenAuthenticator<NegotiationTokenAuthenticatorState>
    {
        private SecurityBindingElement bootstrapSecurityBindingElement;
        private SecurityKeyEntropyMode keyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
        private bool preserveBootstrapTokens;
        private bool shouldMatchRstWithEndpointFilter;

        internal IChannelListener<TChannel> BuildNegotiationChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            SecurityCredentialsManager credentialsManager = base.IssuerBindingContext.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ServiceCredentials.CreateDefaultCredentials();
            }
            this.bootstrapSecurityBindingElement.ReaderQuotas = context.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.bootstrapSecurityBindingElement.ReaderQuotas == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EncodingBindingElementDoesNotHandleReaderQuotas")));
            }
            TransportBindingElement element = context.RemainingBindingElements.Find<TransportBindingElement>();
            if (element != null)
            {
                this.bootstrapSecurityBindingElement.MaxReceivedMessageSize = element.MaxReceivedMessageSize;
            }
            SecurityProtocolFactory factory = this.bootstrapSecurityBindingElement.CreateSecurityProtocolFactory<TChannel>(base.IssuerBindingContext.Clone(), credentialsManager, true, base.IssuerBindingContext.Clone());
            MessageSecurityProtocolFactory factory2 = factory as MessageSecurityProtocolFactory;
            if (factory2 != null)
            {
                factory2.ApplyConfidentiality = factory2.ApplyIntegrity = factory2.RequireConfidentiality = factory2.RequireIntegrity = true;
                MessagePartSpecification parts = new MessagePartSpecification(true);
                factory2.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.RequestSecurityTokenResponseAction);
                factory2.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, this.RequestSecurityTokenResponseAction);
                factory2.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.RequestSecurityTokenAction);
                factory2.ProtectionRequirements.IncomingEncryptionParts.AddParts(parts, this.RequestSecurityTokenAction);
            }
            SecurityChannelListener<TChannel> listener = new SecurityChannelListener<TChannel>(this.bootstrapSecurityBindingElement, context) {
                SecurityProtocolFactory = factory,
                SendUnsecuredFaults = !System.ServiceModel.Security.SecurityUtils.IsCompositeDuplexBinding(context)
            };
            ChannelBuilder channelBuilder = new ChannelBuilder(context, true);
            listener.InitializeListener(channelBuilder);
            this.shouldMatchRstWithEndpointFilter = System.ServiceModel.Security.SecurityUtils.ShouldMatchRstWithEndpointFilter(this.bootstrapSecurityBindingElement);
            return listener;
        }

        protected override MessageFilter GetListenerFilter()
        {
            return new RstDirectFilter(base.StandardsManager, this);
        }

        protected override Binding GetNegotiationBinding(Binding binding)
        {
            CustomBinding binding2 = new CustomBinding(binding);
            binding2.Elements.Insert(0, new AcceleratedTokenAuthenticatorBindingElement(this));
            return binding2;
        }

        protected override BodyWriter ProcessRequestSecurityToken(Message request, RequestSecurityToken requestSecurityToken, out NegotiationTokenAuthenticatorState negotiationState)
        {
            BodyWriter writer;
            if (request == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            if (requestSecurityToken == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("requestSecurityToken", request);
            }
            try
            {
                EndpointAddress address;
                DataContractSerializer serializer;
                string str;
                string str2;
                int num;
                byte[] buffer;
                byte[] buffer2;
                SecurityToken token;
                ReadOnlyCollection<IAuthorizationPolicy> instance;
                if ((requestSecurityToken.RequestType != null) && (requestSecurityToken.RequestType != base.StandardsManager.TrustDriver.RequestTypeIssue))
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidRstRequestType", new object[] { requestSecurityToken.RequestType })), request);
                }
                if ((requestSecurityToken.TokenType != null) && (requestSecurityToken.TokenType != base.SecurityContextTokenUri))
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("CannotIssueRstTokenType", new object[] { requestSecurityToken.TokenType })), request);
                }
                requestSecurityToken.GetAppliesToQName(out str, out str2);
                if ((str == "EndpointReference") && (str2 == request.Version.Addressing.Namespace))
                {
                    if (request.Version.Addressing != AddressingVersion.WSAddressing10)
                    {
                        if (request.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { request.Version.Addressing })));
                        }
                        serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddressAugust2004), 0x10000);
                        address = requestSecurityToken.GetAppliesTo<EndpointAddressAugust2004>(serializer).ToEndpointAddress();
                    }
                    else
                    {
                        serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddress10), 0x10000);
                        address = requestSecurityToken.GetAppliesTo<EndpointAddress10>(serializer).ToEndpointAddress();
                    }
                }
                else
                {
                    address = null;
                    serializer = null;
                }
                if (this.shouldMatchRstWithEndpointFilter)
                {
                    System.ServiceModel.Security.SecurityUtils.MatchRstWithEndpointFilter(request, base.EndpointFilterTable, base.ListenUri);
                }
                WSTrust.Driver.ProcessRstAndIssueKey(requestSecurityToken, null, this.KeyEntropyMode, base.SecurityAlgorithmSuite, out num, out buffer, out buffer2, out token);
                UniqueId contextId = System.ServiceModel.Security.SecurityUtils.GenerateUniqueId();
                string id = System.ServiceModel.Security.SecurityUtils.GenerateId();
                DateTime utcNow = DateTime.UtcNow;
                DateTime expirationTime = TimeoutHelper.Add(utcNow, base.ServiceTokenLifetime);
                SecurityMessageProperty security = request.Properties.Security;
                if (security != null)
                {
                    instance = SecuritySessionSecurityTokenAuthenticator.CreateSecureConversationPolicies(security, expirationTime);
                }
                else
                {
                    instance = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
                }
                SecurityContextSecurityToken token2 = base.IssueSecurityContextToken(contextId, id, buffer2, utcNow, expirationTime, instance, base.EncryptStateInServiceToken);
                if (this.preserveBootstrapTokens)
                {
                    token2.BootstrapMessageProperty = (security == null) ? null : ((SecurityMessageProperty) security.CreateCopy());
                    System.ServiceModel.Security.SecurityUtils.ErasePasswordInUsernameTokenIfPresent(token2.BootstrapMessageProperty);
                }
                RequestSecurityTokenResponse response = new RequestSecurityTokenResponse(base.StandardsManager) {
                    Context = requestSecurityToken.Context,
                    KeySize = num,
                    RequestedUnattachedReference = base.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(token2, SecurityTokenReferenceStyle.External),
                    RequestedAttachedReference = base.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(token2, SecurityTokenReferenceStyle.Internal),
                    TokenType = base.SecurityContextTokenUri,
                    RequestedSecurityToken = token2
                };
                if (buffer != null)
                {
                    response.SetIssuerEntropy(buffer);
                    response.ComputeKey = true;
                }
                if (token != null)
                {
                    response.RequestedProofToken = token;
                }
                if (address != null)
                {
                    if (request.Version.Addressing != AddressingVersion.WSAddressing10)
                    {
                        if (request.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { request.Version.Addressing })));
                        }
                        response.SetAppliesTo<EndpointAddressAugust2004>(EndpointAddressAugust2004.FromEndpointAddress(address), serializer);
                    }
                    else
                    {
                        response.SetAppliesTo<EndpointAddress10>(EndpointAddress10.FromEndpointAddress(address), serializer);
                    }
                }
                response.MakeReadOnly();
                negotiationState = new NegotiationTokenAuthenticatorState();
                negotiationState.SetServiceToken(token2);
                if (base.StandardsManager.MessageSecurityVersion.SecureConversationVersion == SecureConversationVersion.WSSecureConversationFeb2005)
                {
                    return response;
                }
                if (base.StandardsManager.MessageSecurityVersion.SecureConversationVersion != SecureConversationVersion.WSSecureConversation13)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                RequestSecurityTokenResponseCollection responses = new RequestSecurityTokenResponseCollection(new List<RequestSecurityTokenResponse>(1) { response }, base.StandardsManager);
                writer = responses;
            }
            finally
            {
                SecuritySessionSecurityTokenAuthenticator.RemoveCachedTokensIfRequired(request.Properties.Security);
            }
            return writer;
        }

        protected override BodyWriter ProcessRequestSecurityTokenResponse(NegotiationTokenAuthenticatorState negotiationState, Message request, RequestSecurityTokenResponse requestSecurityTokenResponse)
        {
            throw TraceUtility.ThrowHelperWarning(new NotSupportedException(System.ServiceModel.SR.GetString("RstDirectDoesNotExpectRstr")), request);
        }

        public SecurityBindingElement BootstrapSecurityBindingElement
        {
            get
            {
                return this.bootstrapSecurityBindingElement;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.bootstrapSecurityBindingElement = (SecurityBindingElement) value.Clone();
            }
        }

        protected override bool IsMultiLegNegotiation
        {
            get
            {
                return false;
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
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                SecurityKeyEntropyModeHelper.Validate(value);
                this.keyEntropyMode = value;
            }
        }

        public bool PreserveBootstrapTokens
        {
            get
            {
                return this.preserveBootstrapTokens;
            }
            set
            {
                this.preserveBootstrapTokens = value;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenAction
        {
            get
            {
                return base.StandardsManager.SecureConversationDriver.IssueAction;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenResponseAction
        {
            get
            {
                return base.StandardsManager.SecureConversationDriver.IssueResponseAction;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenResponseFinalAction
        {
            get
            {
                return base.StandardsManager.SecureConversationDriver.IssueResponseAction;
            }
        }

        private class RstDirectFilter : HeaderFilter
        {
            private AcceleratedTokenAuthenticator authenticator;
            private SecurityStandardsManager standardsManager;

            public RstDirectFilter(SecurityStandardsManager standardsManager, AcceleratedTokenAuthenticator authenticator)
            {
                this.standardsManager = standardsManager;
                this.authenticator = authenticator;
            }

            public override bool Match(Message message)
            {
                return ((message.Headers.Action == this.authenticator.RequestSecurityTokenAction.Value) && this.standardsManager.DoesMessageContainSecurityHeader(message));
            }
        }
    }
}

