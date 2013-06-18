namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class AcceleratedTokenProvider : NegotiationTokenProvider<AcceleratedTokenProviderState>
    {
        private SecurityBindingElement bootstrapSecurityBindingElement;
        private ChannelParameterCollection channelParameters;
        private SafeFreeCredentials credentialsHandle;
        internal const SecurityKeyEntropyMode defaultKeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
        private SecurityKeyEntropyMode keyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
        private bool ownCredentialsHandle;
        private Uri privacyNoticeUri;
        private int privacyNoticeVersion;

        public AcceleratedTokenProvider(SafeFreeCredentials credentialsHandle)
        {
            this.credentialsHandle = credentialsHandle;
        }

        protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<AcceleratedTokenProviderState>(this.CreateNegotiationState(target, via, timeout), callback, state);
        }

        protected override IRequestChannel CreateClientChannel(EndpointAddress target, Uri via)
        {
            IRequestChannel innerChannel = base.CreateClientChannel(target, via);
            if (this.channelParameters != null)
            {
                this.channelParameters.PropagateChannelParameters(innerChannel);
            }
            if (this.ownCredentialsHandle)
            {
                ChannelParameterCollection property = innerChannel.GetProperty<ChannelParameterCollection>();
                if (property != null)
                {
                    property.Add(new SspiIssuanceChannelParameter(true, this.credentialsHandle));
                }
            }
            return innerChannel;
        }

        protected override AcceleratedTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
        {
            byte[] buffer;
            if ((this.keyEntropyMode == SecurityKeyEntropyMode.ClientEntropy) || (this.keyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy))
            {
                buffer = new byte[base.SecurityAlgorithmSuite.DefaultSymmetricKeyLength / 8];
                System.ServiceModel.Security.CryptoHelper.FillRandomBytes(buffer);
            }
            else
            {
                buffer = null;
            }
            return new AcceleratedTokenProviderState(buffer);
        }

        protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
        {
            return true;
        }

        protected override AcceleratedTokenProviderState EndCreateNegotiationState(IAsyncResult result)
        {
            return CompletedAsyncResult<AcceleratedTokenProviderState>.End(result);
        }

        private void FreeCredentialsHandle()
        {
            if (this.credentialsHandle != null)
            {
                if (this.ownCredentialsHandle)
                {
                    this.credentialsHandle.Close();
                }
                this.credentialsHandle = null;
            }
        }

        protected override BodyWriter GetFirstOutgoingMessageBody(AcceleratedTokenProviderState negotiationState, out MessageProperties messageProperties)
        {
            messageProperties = null;
            RequestSecurityToken token = new RequestSecurityToken(base.StandardsManager) {
                Context = negotiationState.Context,
                KeySize = base.SecurityAlgorithmSuite.DefaultSymmetricKeyLength,
                TokenType = base.SecurityContextTokenUri
            };
            byte[] requestorEntropy = negotiationState.GetRequestorEntropy();
            if (requestorEntropy != null)
            {
                token.SetRequestorEntropy(requestorEntropy);
            }
            token.MakeReadOnly();
            return token;
        }

        protected override IChannelFactory<IRequestChannel> GetNegotiationChannelFactory(IChannelFactory<IRequestChannel> transportChannelFactory, ChannelBuilder channelBuilder)
        {
            ISecurityCapabilities property = this.bootstrapSecurityBindingElement.GetProperty<ISecurityCapabilities>(base.IssuerBindingContext);
            SecurityCredentialsManager credentialsManager = base.IssuerBindingContext.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }
            this.bootstrapSecurityBindingElement.ReaderQuotas = base.IssuerBindingContext.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.bootstrapSecurityBindingElement.ReaderQuotas == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EncodingBindingElementDoesNotHandleReaderQuotas")));
            }
            TransportBindingElement element = base.IssuerBindingContext.RemainingBindingElements.Find<TransportBindingElement>();
            if (element != null)
            {
                this.bootstrapSecurityBindingElement.MaxReceivedMessageSize = element.MaxReceivedMessageSize;
            }
            SecurityProtocolFactory protocolFactory = this.bootstrapSecurityBindingElement.CreateSecurityProtocolFactory<IRequestChannel>(base.IssuerBindingContext.Clone(), credentialsManager, false, base.IssuerBindingContext.Clone());
            MessageSecurityProtocolFactory factory2 = protocolFactory as MessageSecurityProtocolFactory;
            if (factory2 != null)
            {
                factory2.ApplyConfidentiality = factory2.ApplyIntegrity = factory2.RequireConfidentiality = factory2.RequireIntegrity = true;
                MessagePartSpecification parts = new MessagePartSpecification(true);
                factory2.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.RequestSecurityTokenAction);
                factory2.ProtectionRequirements.IncomingEncryptionParts.AddParts(parts, this.RequestSecurityTokenAction);
                factory2.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.RequestSecurityTokenResponseAction);
                factory2.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, this.RequestSecurityTokenResponseAction);
            }
            protocolFactory.PrivacyNoticeUri = this.PrivacyNoticeUri;
            protocolFactory.PrivacyNoticeVersion = this.PrivacyNoticeVersion;
            return new SecurityChannelFactory<IRequestChannel>(property, base.IssuerBindingContext, channelBuilder, protocolFactory, transportChannelFactory);
        }

        protected override BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, AcceleratedTokenProviderState negotiationState)
        {
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
            IssuanceTokenProviderBase<AcceleratedTokenProviderState>.ThrowIfFault(incomingMessage, base.TargetAddress);
            if (incomingMessage.Headers.Action != this.RequestSecurityTokenResponseAction.Value)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidActionForNegotiationMessage", new object[] { incomingMessage.Headers.Action })), incomingMessage);
            }
            SecurityMessageProperty security = incomingMessage.Properties.Security;
            if ((security != null) && (security.ServiceSecurityContext != null))
            {
                authorizationPolicies = security.ServiceSecurityContext.AuthorizationPolicies;
            }
            else
            {
                authorizationPolicies = System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            RequestSecurityTokenResponse response = null;
            XmlDictionaryReader readerAtBodyContents = incomingMessage.GetReaderAtBodyContents();
            using (readerAtBodyContents)
            {
                if (base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                {
                    response = RequestSecurityTokenResponse.CreateFrom(base.StandardsManager, readerAtBodyContents);
                }
                else
                {
                    if (base.StandardsManager.MessageSecurityVersion.TrustVersion != TrustVersion.WSTrust13)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }
                    foreach (RequestSecurityTokenResponse response2 in base.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(readerAtBodyContents).RstrCollection)
                    {
                        if (response != null)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MoreThanOneRSTRInRSTRC")));
                        }
                        response = response2;
                    }
                }
                incomingMessage.ReadFromBodyContentsToEnd(readerAtBodyContents);
            }
            if (response.Context != negotiationState.Context)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("BadSecurityNegotiationContext")), incomingMessage);
            }
            byte[] requestorEntropy = negotiationState.GetRequestorEntropy();
            GenericXmlSecurityToken serviceToken = response.GetIssuedToken(null, null, this.keyEntropyMode, requestorEntropy, base.SecurityContextTokenUri, authorizationPolicies, base.SecurityAlgorithmSuite.DefaultSymmetricKeyLength, false);
            negotiationState.SetServiceToken(serviceToken);
            return null;
        }

        public override void OnAbort()
        {
            base.OnAbort();
            this.FreeCredentialsHandle();
        }

        public override void OnClose(TimeSpan timeout)
        {
            base.OnClose(timeout);
            this.FreeCredentialsHandle();
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.BootstrapSecurityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BootstrapSecurityBindingElementNotSet", new object[] { base.GetType() })));
            }
            base.OnOpen(timeout);
        }

        public override void OnOpening()
        {
            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                if (this.BootstrapSecurityBindingElement == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BootstrapSecurityBindingElementNotSet", new object[] { base.GetType() })));
                }
                this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle(this.BootstrapSecurityBindingElement, base.IssuerBindingContext);
                this.ownCredentialsHandle = true;
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
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.bootstrapSecurityBindingElement = (SecurityBindingElement) value.Clone();
            }
        }

        public ChannelParameterCollection ChannelParameters
        {
            get
            {
                return this.channelParameters;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.channelParameters = value;
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

        public Uri PrivacyNoticeUri
        {
            get
            {
                return this.privacyNoticeUri;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.privacyNoticeUri = value;
            }
        }

        public int PrivacyNoticeVersion
        {
            get
            {
                return this.privacyNoticeVersion;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.privacyNoticeVersion = value;
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
    }
}

