namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class SecuritySessionSecurityTokenProvider : CommunicationObjectSecurityTokenProvider
    {
        private SecurityBindingElement bootstrapSecurityBindingElement;
        private ChannelParameterCollection channelParameters;
        private SafeFreeCredentials credentialsHandle;
        private SecurityTokenParameters issuedTokenParameters;
        private BindingContext issuerBindingContext;
        private SecurityKeyEntropyMode keyEntropyMode;
        private EndpointAddress localAddress;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private static readonly MessageOperationFormatter operationFormatter = new MessageOperationFormatter();
        private bool ownCredentialsHandle;
        private Uri privacyNoticeUri;
        private int privacyNoticeVersion;
        private bool requiresManualReplyAddressing;
        private IChannelFactory<IRequestChannel> rstChannelFactory;
        private string sctUri;
        private System.ServiceModel.Security.SecurityAlgorithmSuite securityAlgorithmSuite;
        private SecurityStandardsManager standardsManager;
        private EndpointAddress targetAddress;
        private object thisLock = new object();
        private Uri via;

        public SecuritySessionSecurityTokenProvider(SafeFreeCredentials credentialsHandle)
        {
            this.credentialsHandle = credentialsHandle;
            this.standardsManager = SecurityStandardsManager.DefaultInstance;
            this.keyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
        }

        protected override IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            return new SessionOperationAsyncResult(this, SecuritySessionOperation.Issue, this.TargetAddress, this.Via, null, timeout, callback, state);
        }

        protected override IAsyncResult BeginRenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed, AsyncCallback callback, object state)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            return new SessionOperationAsyncResult(this, SecuritySessionOperation.Renew, this.TargetAddress, this.Via, tokenToBeRenewed, timeout, callback, state);
        }

        private IRequestChannel CreateChannel(SecuritySessionOperation operation, EndpointAddress target, Uri via)
        {
            IRequestChannel channel;
            if ((operation != SecuritySessionOperation.Issue) && (operation != SecuritySessionOperation.Renew))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            IChannelFactory<IRequestChannel> rstChannelFactory = this.rstChannelFactory;
            if (via != null)
            {
                channel = rstChannelFactory.CreateChannel(target, via);
            }
            else
            {
                channel = rstChannelFactory.CreateChannel(target);
            }
            if (this.channelParameters != null)
            {
                this.channelParameters.PropagateChannelParameters(channel);
            }
            if (this.ownCredentialsHandle)
            {
                ChannelParameterCollection property = channel.GetProperty<ChannelParameterCollection>();
                if (property != null)
                {
                    property.Add(new SspiIssuanceChannelParameter(true, this.credentialsHandle));
                }
            }
            return channel;
        }

        protected virtual Message CreateIssueRequest(EndpointAddress target, out object requestState)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            RequestSecurityToken body = this.CreateRst(target, out requestState);
            body.RequestType = this.StandardsManager.TrustDriver.RequestTypeIssue;
            body.MakeReadOnly();
            Message message = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.IssueAction, this.MessageVersion.Addressing), body);
            this.PrepareRequest(message);
            return message;
        }

        protected virtual Message CreateRenewRequest(EndpointAddress target, SecurityToken currentSessionToken, out object requestState)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            RequestSecurityToken body = this.CreateRst(target, out requestState);
            body.RequestType = this.StandardsManager.TrustDriver.RequestTypeRenew;
            body.RenewTarget = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(currentSessionToken, SecurityTokenReferenceStyle.External);
            body.MakeReadOnly();
            Message message = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.RenewAction, this.MessageVersion.Addressing), body);
            SecurityMessageProperty property = new SecurityMessageProperty {
                OutgoingSupportingTokens = { new SupportingTokenSpecification(currentSessionToken, System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, SecurityTokenAttachmentMode.Endorsing, this.IssuedSecurityTokenParameters) }
            };
            message.Properties.Security = property;
            this.PrepareRequest(message);
            return message;
        }

        private Message CreateRequest(SecuritySessionOperation operation, EndpointAddress target, SecurityToken currentToken, out object requestState)
        {
            if (operation == SecuritySessionOperation.Issue)
            {
                return this.CreateIssueRequest(target, out requestState);
            }
            if (operation != SecuritySessionOperation.Renew)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return this.CreateRenewRequest(target, currentToken, out requestState);
        }

        private RequestSecurityToken CreateRst(EndpointAddress target, out object requestState)
        {
            RequestSecurityToken token = new RequestSecurityToken(this.standardsManager) {
                KeySize = this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength,
                TokenType = this.sctUri
            };
            if ((this.KeyEntropyMode == SecurityKeyEntropyMode.ClientEntropy) || (this.KeyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy))
            {
                byte[] entropy = this.GenerateEntropy(token.KeySize);
                token.SetRequestorEntropy(entropy);
                requestState = entropy;
                return token;
            }
            requestState = null;
            return token;
        }

        private GenericXmlSecurityToken DoOperation(SecuritySessionOperation operation, EndpointAddress target, Uri via, SecurityToken currentToken, TimeSpan timeout)
        {
            GenericXmlSecurityToken token2;
            if (target == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("target");
            }
            if ((operation == SecuritySessionOperation.Renew) && (currentToken == null))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("currentToken");
            }
            IRequestChannel channel = null;
            try
            {
                object obj2;
                GenericXmlSecurityToken token;
                SecurityTraceRecordHelper.TraceBeginSecuritySessionOperation(operation, target, currentToken);
                channel = this.CreateChannel(operation, target, via);
                TimeoutHelper helper = new TimeoutHelper(timeout);
                channel.Open(helper.RemainingTime());
                using (Message message = this.CreateRequest(operation, target, currentToken, out obj2))
                {
                    TraceUtility.ProcessOutgoingMessage(message);
                    using (Message message2 = channel.Request(message, helper.RemainingTime()))
                    {
                        if (message2 == null)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("FailToRecieveReplyFromNegotiation")));
                        }
                        TraceUtility.ProcessIncomingMessage(message2);
                        ThrowIfFault(message2, this.targetAddress);
                        token = this.ProcessReply(message2, operation, obj2);
                        this.ValidateKeySize(token);
                    }
                }
                channel.Close(helper.RemainingTime());
                this.OnOperationSuccess(operation, target, token, currentToken);
                token2 = token;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (exception is TimeoutException)
                {
                    exception = new TimeoutException(System.ServiceModel.SR.GetString("ClientSecuritySessionRequestTimeout", new object[] { timeout }), exception);
                }
                this.OnOperationFailure(operation, target, currentToken, exception, channel);
                throw;
            }
            return token2;
        }

        protected override SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            return SessionOperationAsyncResult.End(result);
        }

        protected override SecurityToken EndRenewTokenCore(IAsyncResult result)
        {
            return SessionOperationAsyncResult.End(result);
        }

        private GenericXmlSecurityToken ExtractToken(Message response, object requestState)
        {
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
            byte[] buffer;
            SecurityMessageProperty security = response.Properties.Security;
            if ((security != null) && (security.ServiceSecurityContext != null))
            {
                authorizationPolicies = security.ServiceSecurityContext.AuthorizationPolicies;
            }
            else
            {
                authorizationPolicies = System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            RequestSecurityTokenResponse response2 = null;
            XmlDictionaryReader readerAtBodyContents = response.GetReaderAtBodyContents();
            using (readerAtBodyContents)
            {
                if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                {
                    response2 = this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponse(readerAtBodyContents);
                }
                else
                {
                    if (this.StandardsManager.MessageSecurityVersion.TrustVersion != TrustVersion.WSTrust13)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }
                    foreach (RequestSecurityTokenResponse response3 in this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(readerAtBodyContents).RstrCollection)
                    {
                        if (response2 != null)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MoreThanOneRSTRInRSTRC")));
                        }
                        response2 = response3;
                    }
                }
                response.ReadFromBodyContentsToEnd(readerAtBodyContents);
            }
            if (requestState != null)
            {
                buffer = (byte[]) requestState;
            }
            else
            {
                buffer = null;
            }
            return response2.GetIssuedToken(null, null, this.KeyEntropyMode, buffer, this.sctUri, authorizationPolicies, this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength, false);
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

        private byte[] GenerateEntropy(int entropySize)
        {
            byte[] buffer = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(entropySize / 8);
            System.ServiceModel.Security.CryptoHelper.FillRandomBytes(buffer);
            return buffer;
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            return this.DoOperation(SecuritySessionOperation.Issue, this.targetAddress, this.via, null, timeout);
        }

        private void InitializeFactories()
        {
            IChannelFactory<IRequestChannel> factory3;
            ISecurityCapabilities property = this.BootstrapSecurityBindingElement.GetProperty<ISecurityCapabilities>(this.IssuerBindingContext);
            SecurityCredentialsManager credentialsManager = this.IssuerBindingContext.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ClientCredentials.CreateDefaultCredentials();
            }
            BindingContext issuerBindingContext = this.IssuerBindingContext;
            this.bootstrapSecurityBindingElement.ReaderQuotas = issuerBindingContext.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.bootstrapSecurityBindingElement.ReaderQuotas == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EncodingBindingElementDoesNotHandleReaderQuotas")));
            }
            TransportBindingElement element = issuerBindingContext.RemainingBindingElements.Find<TransportBindingElement>();
            if (element != null)
            {
                this.bootstrapSecurityBindingElement.MaxReceivedMessageSize = element.MaxReceivedMessageSize;
            }
            SecurityProtocolFactory protocolFactory = this.BootstrapSecurityBindingElement.CreateSecurityProtocolFactory<IRequestChannel>(this.IssuerBindingContext.Clone(), credentialsManager, false, this.IssuerBindingContext.Clone());
            if (protocolFactory is MessageSecurityProtocolFactory)
            {
                MessageSecurityProtocolFactory factory2 = protocolFactory as MessageSecurityProtocolFactory;
                factory2.ApplyConfidentiality = factory2.ApplyIntegrity = factory2.RequireConfidentiality = factory2.RequireIntegrity = true;
                factory2.ProtectionRequirements.IncomingSignatureParts.ChannelParts.IsBodyIncluded = true;
                factory2.ProtectionRequirements.OutgoingSignatureParts.ChannelParts.IsBodyIncluded = true;
                MessagePartSpecification parts = new MessagePartSpecification(true);
                factory2.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.IssueAction);
                factory2.ProtectionRequirements.IncomingEncryptionParts.AddParts(parts, this.IssueAction);
                factory2.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.RenewAction);
                factory2.ProtectionRequirements.IncomingEncryptionParts.AddParts(parts, this.RenewAction);
                factory2.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.IssueResponseAction);
                factory2.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, this.IssueResponseAction);
                factory2.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.RenewResponseAction);
                factory2.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, this.RenewResponseAction);
            }
            protocolFactory.PrivacyNoticeUri = this.PrivacyNoticeUri;
            protocolFactory.PrivacyNoticeVersion = this.privacyNoticeVersion;
            if (this.localAddress != null)
            {
                MessageFilter filter = new SessionActionFilter(this.standardsManager, new string[] { this.IssueResponseAction.Value, this.RenewResponseAction.Value });
                issuerBindingContext.BindingParameters.Add(new LocalAddressProvider(this.localAddress, filter));
            }
            ChannelBuilder channelBuilder = new ChannelBuilder(issuerBindingContext, true);
            if (channelBuilder.CanBuildChannelFactory<IRequestChannel>())
            {
                factory3 = channelBuilder.BuildChannelFactory<IRequestChannel>();
                this.requiresManualReplyAddressing = true;
            }
            else
            {
                ClientRuntime clientRuntime = new ClientRuntime("RequestSecuritySession", "http://tempuri.org/") {
                    UseSynchronizationContext = false,
                    AddTransactionFlowProperties = false,
                    ValidateMustUnderstand = false
                };
                ServiceChannelFactory serviceChannelFactory = ServiceChannelFactory.BuildChannelFactory(channelBuilder, clientRuntime);
                ClientOperation item = new ClientOperation(serviceChannelFactory.ClientRuntime, "Issue", this.IssueAction.Value) {
                    Formatter = operationFormatter
                };
                serviceChannelFactory.ClientRuntime.Operations.Add(item);
                ClientOperation operation2 = new ClientOperation(serviceChannelFactory.ClientRuntime, "Renew", this.RenewAction.Value) {
                    Formatter = operationFormatter
                };
                serviceChannelFactory.ClientRuntime.Operations.Add(operation2);
                factory3 = new RequestChannelFactory(serviceChannelFactory);
                this.requiresManualReplyAddressing = false;
            }
            SecurityChannelFactory<IRequestChannel> factory5 = new SecurityChannelFactory<IRequestChannel>(property, this.IssuerBindingContext, channelBuilder, protocolFactory, factory3);
            if ((element != null) && (factory5.SecurityProtocolFactory != null))
            {
                factory5.SecurityProtocolFactory.ExtendedProtectionPolicy = element.GetProperty<ExtendedProtectionPolicy>(issuerBindingContext);
            }
            this.rstChannelFactory = factory5;
            this.messageVersion = factory5.MessageVersion;
        }

        public override void OnAbort()
        {
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Abort();
                this.rstChannelFactory = null;
            }
            this.FreeCredentialsHandle();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Close(helper.RemainingTime());
                this.rstChannelFactory = null;
            }
            this.FreeCredentialsHandle();
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.targetAddress == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TargetAddressIsNotSet", new object[] { base.GetType() })));
            }
            if (this.IssuerBindingContext == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuerBuildContextNotSet", new object[] { base.GetType() })));
            }
            if (this.IssuedSecurityTokenParameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedSecurityTokenParametersNotSet", new object[] { base.GetType() })));
            }
            if (this.BootstrapSecurityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BootstrapSecurityBindingElementNotSet", new object[] { base.GetType() })));
            }
            if (this.SecurityAlgorithmSuite == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityAlgorithmSuiteNotSet", new object[] { base.GetType() })));
            }
            this.InitializeFactories();
            this.rstChannelFactory.Open(helper.RemainingTime());
            this.sctUri = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
        }

        public override void OnOpening()
        {
            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                if (this.IssuerBindingContext == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuerBuildContextNotSet", new object[] { base.GetType() })));
                }
                if (this.BootstrapSecurityBindingElement == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BootstrapSecurityBindingElementNotSet", new object[] { base.GetType() })));
                }
                this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle(this.bootstrapSecurityBindingElement, this.issuerBindingContext);
                this.ownCredentialsHandle = true;
            }
        }

        private void OnOperationFailure(SecuritySessionOperation operation, EndpointAddress target, SecurityToken currentToken, Exception e, IChannel channel)
        {
            SecurityTraceRecordHelper.TraceSecuritySessionOperationFailure(operation, target, currentToken, e);
            if (channel != null)
            {
                channel.Abort();
            }
        }

        private void OnOperationSuccess(SecuritySessionOperation operation, EndpointAddress target, SecurityToken issuedToken, SecurityToken currentToken)
        {
            SecurityTraceRecordHelper.TraceSecuritySessionOperationSuccess(operation, target, currentToken, issuedToken);
        }

        private void PrepareRequest(Message message)
        {
            RequestReplyCorrelator.PrepareRequest(message);
            if (this.requiresManualReplyAddressing)
            {
                if (this.localAddress != null)
                {
                    message.Headers.ReplyTo = this.LocalAddress;
                }
                else
                {
                    message.Headers.ReplyTo = EndpointAddress.AnonymousAddress;
                }
            }
        }

        protected virtual GenericXmlSecurityToken ProcessIssueResponse(Message response, object requestState)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            return this.ExtractToken(response, requestState);
        }

        protected virtual GenericXmlSecurityToken ProcessRenewResponse(Message response, object requestState)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            if (response.Headers.Action != this.RenewResponseAction.Value)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidRenewResponseAction", new object[] { response.Headers.Action })), response);
            }
            return this.ExtractToken(response, requestState);
        }

        private GenericXmlSecurityToken ProcessReply(Message reply, SecuritySessionOperation operation, object requestState)
        {
            ThrowIfFault(reply, this.targetAddress);
            GenericXmlSecurityToken token = null;
            if (operation == SecuritySessionOperation.Issue)
            {
                return this.ProcessIssueResponse(reply, requestState);
            }
            if (operation == SecuritySessionOperation.Renew)
            {
                token = this.ProcessRenewResponse(reply, requestState);
            }
            return token;
        }

        protected override SecurityToken RenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            return this.DoOperation(SecuritySessionOperation.Renew, this.targetAddress, this.via, tokenToBeRenewed, timeout);
        }

        protected static void ThrowIfFault(Message message, EndpointAddress target)
        {
            System.ServiceModel.Security.SecurityUtils.ThrowIfNegotiationFault(message, target);
        }

        protected void ValidateKeySize(GenericXmlSecurityToken issuedToken)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            ReadOnlyCollection<SecurityKey> securityKeys = issuedToken.SecurityKeys;
            if ((securityKeys == null) || (securityKeys.Count != 1))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("CannotObtainIssuedTokenKeySize")));
            }
            SymmetricSecurityKey key = securityKeys[0] as SymmetricSecurityKey;
            if ((key != null) && !this.SecurityAlgorithmSuite.IsSymmetricKeyLengthSupported(key.KeySize))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidIssuedTokenKeySize", new object[] { key.KeySize })));
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

        public virtual XmlDictionaryString CloseAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.CloseAction;
            }
        }

        public virtual XmlDictionaryString CloseResponseAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.CloseResponseAction;
            }
        }

        public virtual XmlDictionaryString IssueAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.IssueAction;
            }
        }

        public SecurityTokenParameters IssuedSecurityTokenParameters
        {
            get
            {
                return this.issuedTokenParameters;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuedTokenParameters = value;
            }
        }

        public BindingContext IssuerBindingContext
        {
            get
            {
                return this.issuerBindingContext;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.issuerBindingContext = value.Clone();
            }
        }

        public virtual XmlDictionaryString IssueResponseAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.IssueResponseAction;
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

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.localAddress = value;
            }
        }

        private System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
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

        public virtual XmlDictionaryString RenewAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.RenewAction;
            }
        }

        public virtual XmlDictionaryString RenewResponseAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.RenewResponseAction;
            }
        }

        public System.ServiceModel.Security.SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.securityAlgorithmSuite;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.securityAlgorithmSuite = value;
            }
        }

        public SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                if (!value.TrustDriver.IsSessionSupported)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("TrustDriverVersionDoesNotSupportSession"), "value"));
                }
                if (!value.SecureConversationDriver.IsSessionSupported)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SecureConversationDriverVersionDoesNotSupportSession"), "value"));
                }
                this.standardsManager = value;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return this.targetAddress;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.targetAddress = value;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.via = value;
            }
        }

        private class ChannelOpenAsyncResultWrapper
        {
            public System.ServiceModel.Channels.Message Message;
            public object RequestState;
        }

        internal class RequestChannelFactory : ChannelFactoryBase<IRequestChannel>
        {
            private ServiceChannelFactory serviceChannelFactory;

            public RequestChannelFactory(ServiceChannelFactory serviceChannelFactory)
            {
                this.serviceChannelFactory = serviceChannelFactory;
            }

            public override T GetProperty<T>() where T: class
            {
                return this.serviceChannelFactory.GetProperty<T>();
            }

            protected override void OnAbort()
            {
                this.serviceChannelFactory.Abort();
                base.OnAbort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ICommunicationObject[] { this.serviceChannelFactory });
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.serviceChannelFactory.BeginOpen(timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                base.OnClose(timeout);
                this.serviceChannelFactory.Close(timeout);
            }

            protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via)
            {
                return this.serviceChannelFactory.CreateChannel<IRequestChannel>(address, via);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.serviceChannelFactory.EndOpen(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.serviceChannelFactory.Open(timeout);
            }
        }

        private class SessionOperationAsyncResult : AsyncResult
        {
            private IRequestChannel channel;
            private static AsyncCallback closeChannelCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionSecurityTokenProvider.SessionOperationAsyncResult.CloseChannelCallback));
            private SecurityToken currentToken;
            private GenericXmlSecurityToken issuedToken;
            private static AsyncCallback openChannelCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionSecurityTokenProvider.SessionOperationAsyncResult.OpenChannelCallback));
            private SecuritySessionOperation operation;
            private SecuritySessionSecurityTokenProvider requestor;
            private EndpointAddress target;
            private TimeoutHelper timeoutHelper;
            private Uri via;

            public SessionOperationAsyncResult(SecuritySessionSecurityTokenProvider requestor, SecuritySessionOperation operation, EndpointAddress target, Uri via, SecurityToken currentToken, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.requestor = requestor;
                this.operation = operation;
                this.target = target;
                this.via = via;
                this.currentToken = currentToken;
                this.timeoutHelper = new TimeoutHelper(timeout);
                SecurityTraceRecordHelper.TraceBeginSecuritySessionOperation(operation, target, currentToken);
                bool flag = false;
                try
                {
                    flag = this.StartOperation();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.OnOperationFailure(exception);
                    throw;
                }
                if (flag)
                {
                    this.OnOperationComplete();
                    base.Complete(true);
                }
            }

            private static void CloseChannelCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SecuritySessionSecurityTokenProvider.SessionOperationAsyncResult asyncState = (SecuritySessionSecurityTokenProvider.SessionOperationAsyncResult) result.AsyncState;
                    Exception e = null;
                    try
                    {
                        asyncState.channel.EndClose(result);
                        asyncState.OnOperationComplete();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        e = exception2;
                        asyncState.OnOperationFailure(e);
                    }
                    asyncState.Complete(false, e);
                }
            }

            public static SecurityToken End(IAsyncResult result)
            {
                return AsyncResult.End<SecuritySessionSecurityTokenProvider.SessionOperationAsyncResult>(result).issuedToken;
            }

            private bool OnChannelOpened()
            {
                object obj2;
                bool flag2;
                Message message = this.requestor.CreateRequest(this.operation, this.target, this.currentToken, out obj2);
                if (message == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NullSessionRequestMessage", new object[] { this.operation.ToString() })));
                }
                SecuritySessionSecurityTokenProvider.ChannelOpenAsyncResultWrapper state = new SecuritySessionSecurityTokenProvider.ChannelOpenAsyncResultWrapper {
                    Message = message,
                    RequestState = obj2
                };
                bool flag = true;
                try
                {
                    IAsyncResult result = this.channel.BeginRequest(message, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.RequestCallback)), state);
                    if (!result.CompletedSynchronously)
                    {
                        flag = false;
                        return false;
                    }
                    Message reply = this.channel.EndRequest(result);
                    flag2 = this.OnReplyReceived(reply, obj2);
                }
                finally
                {
                    if (flag)
                    {
                        state.Message.Close();
                    }
                }
                return flag2;
            }

            private void OnOperationComplete()
            {
                this.requestor.OnOperationSuccess(this.operation, this.target, this.issuedToken, this.currentToken);
            }

            private void OnOperationFailure(Exception e)
            {
                try
                {
                    this.requestor.OnOperationFailure(this.operation, this.target, this.currentToken, e, this.channel);
                }
                catch (CommunicationException exception)
                {
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                    {
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
            }

            private bool OnReplyProcessed()
            {
                IAsyncResult result = this.channel.BeginClose(this.timeoutHelper.RemainingTime(), closeChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.channel.EndClose(result);
                return true;
            }

            private bool OnReplyReceived(Message reply, object requestState)
            {
                if (reply == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("FailToRecieveReplyFromNegotiation")));
                }
                using (reply)
                {
                    this.issuedToken = this.requestor.ProcessReply(reply, this.operation, requestState);
                    this.requestor.ValidateKeySize(this.issuedToken);
                }
                return this.OnReplyProcessed();
            }

            private static void OpenChannelCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SecuritySessionSecurityTokenProvider.SessionOperationAsyncResult asyncState = (SecuritySessionSecurityTokenProvider.SessionOperationAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception e = null;
                    try
                    {
                        asyncState.channel.EndOpen(result);
                        flag = asyncState.OnChannelOpened();
                        if (flag)
                        {
                            asyncState.OnOperationComplete();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        e = exception2;
                        asyncState.OnOperationFailure(e);
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, e);
                    }
                }
            }

            private void RequestCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SecuritySessionSecurityTokenProvider.ChannelOpenAsyncResultWrapper asyncState = (SecuritySessionSecurityTokenProvider.ChannelOpenAsyncResultWrapper) result.AsyncState;
                    object requestState = asyncState.RequestState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        Message reply = this.channel.EndRequest(result);
                        flag = this.OnReplyReceived(reply, requestState);
                        if (flag)
                        {
                            this.OnOperationComplete();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                        this.OnOperationFailure(exception2);
                    }
                    finally
                    {
                        if (asyncState.Message != null)
                        {
                            asyncState.Message.Close();
                        }
                    }
                    if (flag)
                    {
                        base.Complete(false, exception);
                    }
                }
            }

            private bool StartOperation()
            {
                this.channel = this.requestor.CreateChannel(this.operation, this.target, this.via);
                IAsyncResult result = this.channel.BeginOpen(this.timeoutHelper.RemainingTime(), openChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.channel.EndOpen(result);
                return this.OnChannelOpened();
            }
        }
    }
}

