namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
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

    internal class SecuritySessionSecurityTokenAuthenticator : CommunicationObjectSecurityTokenAuthenticator, IIssuanceSecurityTokenAuthenticator, ILogonTokenCacheManager
    {
        private SecurityBindingElement bootstrapSecurityBindingElement;
        internal const int defaultMaxCachedSessionTokens = 0x7fffffff;
        internal static readonly TimeSpan defaultSessionTokenLifetime = TimeSpan.MaxValue;
        internal static readonly SecurityStandardsManager defaultStandardsManager = SecurityStandardsManager.DefaultInstance;
        private IMessageFilterTable<EndpointAddress> endpointFilterTable;
        private bool isClientAnonymous = false;
        private System.ServiceModel.Security.Tokens.IssuedSecurityTokenHandler issuedSecurityTokenHandler;
        private ISecurityContextSecurityTokenCache issuedTokenCache;
        private SecurityTokenParameters issuedTokenParameters;
        private BindingContext issuerBindingContext;
        private SecurityKeyEntropyMode keyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
        private TimeSpan keyRenewalInterval;
        private Uri listenUri;
        private int maximumConcurrentNegotiations = 0x80;
        private TimeSpan negotiationTimeout = NegotiationTokenAuthenticator<NegotiationTokenAuthenticatorState>.defaultServerMaxNegotiationLifetime;
        private bool preserveBootstrapTokens;
        private System.ServiceModel.Security.Tokens.RenewedSecurityTokenHandler renewedSecurityTokenHandler;
        private ServiceHostBase rstListener;
        private string sctUri;
        private System.ServiceModel.Security.SecurityAlgorithmSuite securityAlgorithmSuite;
        private SecurityContextSecurityTokenAuthenticator sessionTokenAuthenticator = new SecurityContextSecurityTokenAuthenticator();
        private TimeSpan sessionTokenLifetime = defaultSessionTokenLifetime;
        private bool shouldMatchRstWithEndpointFilter;
        private SecurityStandardsManager standardsManager = defaultStandardsManager;
        private object thisLock = new object();

        private static void AddTokenToRemoveIfRequired(SecurityToken token, Collection<SecurityContextSecurityToken> sctsToRemove)
        {
            SecurityContextSecurityToken item = token as SecurityContextSecurityToken;
            if (item != null)
            {
                sctsToRemove.Add(item);
            }
        }

        internal IChannelListener<TChannel> BuildResponderChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            SecurityCredentialsManager credentialsManager = this.IssuerBindingContext.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialsManager == null)
            {
                credentialsManager = ServiceCredentials.CreateDefaultCredentials();
            }
            this.bootstrapSecurityBindingElement.ReaderQuotas = this.IssuerBindingContext.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.bootstrapSecurityBindingElement.ReaderQuotas == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EncodingBindingElementDoesNotHandleReaderQuotas")));
            }
            TransportBindingElement element = context.RemainingBindingElements.Find<TransportBindingElement>();
            if (element != null)
            {
                this.bootstrapSecurityBindingElement.MaxReceivedMessageSize = element.MaxReceivedMessageSize;
            }
            SecurityProtocolFactory factory = this.bootstrapSecurityBindingElement.CreateSecurityProtocolFactory<TChannel>(this.IssuerBindingContext.Clone(), credentialsManager, true, this.IssuerBindingContext.Clone());
            if (factory is MessageSecurityProtocolFactory)
            {
                MessageSecurityProtocolFactory factory2 = (MessageSecurityProtocolFactory) factory;
                factory2.ApplyConfidentiality = factory2.ApplyIntegrity = factory2.RequireConfidentiality = factory2.RequireIntegrity = true;
                factory2.ProtectionRequirements.IncomingSignatureParts.ChannelParts.IsBodyIncluded = true;
                factory2.ProtectionRequirements.OutgoingSignatureParts.ChannelParts.IsBodyIncluded = true;
                MessagePartSpecification parts = new MessagePartSpecification(true);
                factory2.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.IssueResponseAction);
                factory2.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, this.IssueResponseAction);
                factory2.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.RenewResponseAction);
                factory2.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, this.RenewResponseAction);
                factory2.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.IssueAction);
                factory2.ProtectionRequirements.IncomingEncryptionParts.AddParts(parts, this.IssueAction);
                factory2.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.RenewAction);
                factory2.ProtectionRequirements.IncomingEncryptionParts.AddParts(parts, this.RenewAction);
            }
            SupportingTokenParameters parameters = new SupportingTokenParameters();
            SecurityContextSecurityTokenParameters item = new SecurityContextSecurityTokenParameters {
                RequireDerivedKeys = this.IssuedSecurityTokenParameters.RequireDerivedKeys
            };
            parameters.Endorsing.Add(item);
            factory.SecurityBindingElement.OperationSupportingTokenParameters.Add(this.RenewAction.Value, parameters);
            factory.SecurityTokenManager = new SessionRenewSecurityTokenManager(factory.SecurityTokenManager, this.sessionTokenAuthenticator, (SecurityTokenResolver) this.IssuedTokenCache);
            SecurityChannelListener<TChannel> listener = new SecurityChannelListener<TChannel>(this.bootstrapSecurityBindingElement, this.IssuerBindingContext) {
                SecurityProtocolFactory = factory,
                SendUnsecuredFaults = !System.ServiceModel.Security.SecurityUtils.IsCompositeDuplexBinding(context)
            };
            ChannelBuilder channelBuilder = new ChannelBuilder(context, true);
            listener.InitializeListener(channelBuilder);
            this.shouldMatchRstWithEndpointFilter = System.ServiceModel.Security.SecurityUtils.ShouldMatchRstWithEndpointFilter(this.bootstrapSecurityBindingElement);
            return listener;
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is SecurityContextSecurityToken);
        }

        private Message CreateFault(Message request, Exception e)
        {
            FaultCode code;
            FaultReason reason;
            bool flag;
            FaultCode code2;
            if (e is QuotaExceededException)
            {
                code = new FaultCode("ServerTooBusy", "http://schemas.microsoft.com/ws/2006/05/security");
                reason = new FaultReason(System.ServiceModel.SR.GetString("PendingSessionsExceededFaultReason"), CultureInfo.CurrentCulture);
                flag = false;
            }
            else if (e is EndpointNotFoundException)
            {
                code = new FaultCode("EndpointUnavailable", request.Version.Addressing.Namespace);
                reason = new FaultReason(System.ServiceModel.SR.GetString("SecurityListenerClosingFaultReason"), CultureInfo.CurrentCulture);
                flag = false;
            }
            else
            {
                code = new FaultCode("InvalidRequest", "http://schemas.xmlsoap.org/ws/2005/02/trust");
                reason = new FaultReason(System.ServiceModel.SR.GetString("InvalidRequestTrustFaultCode"), CultureInfo.CurrentCulture);
                flag = true;
            }
            if (flag)
            {
                code2 = FaultCode.CreateSenderFaultCode(code);
            }
            else
            {
                code2 = FaultCode.CreateReceiverFaultCode(code);
            }
            MessageFault fault = MessageFault.CreateFault(code2, reason);
            Message message = Message.CreateMessage(request.Version, fault, request.Version.Addressing.DefaultFaultAction);
            message.Headers.RelatesTo = request.Headers.MessageId;
            return message;
        }

        private static Message CreateReply(Message request, XmlDictionaryString action, BodyWriter body)
        {
            if (request.Headers.MessageId != null)
            {
                Message message = Message.CreateMessage(request.Version, ActionHeader.Create(action, request.Version.Addressing), body);
                message.InitializeReply(request);
                return message;
            }
            return Message.CreateMessage(request.Version, ActionHeader.Create(action, request.Version.Addressing), body);
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CreateSecureConversationPolicies(SecurityMessageProperty security, DateTime expirationTime)
        {
            return CreateSecureConversationPolicies(security, null, expirationTime);
        }

        private static ReadOnlyCollection<IAuthorizationPolicy> CreateSecureConversationPolicies(SecurityMessageProperty security, ReadOnlyCollection<IAuthorizationPolicy> currentTokenPolicies, DateTime expirationTime)
        {
            if (security == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>();
            if ((security.ServiceSecurityContext != null) && (security.ServiceSecurityContext.AuthorizationPolicies != null))
            {
                list.AddRange(security.ServiceSecurityContext.AuthorizationPolicies);
                if (((security.TransportToken != null) && (security.TransportToken.SecurityTokenPolicies != null)) && (security.TransportToken.SecurityTokenPolicies.Count > 0))
                {
                    foreach (IAuthorizationPolicy policy in security.TransportToken.SecurityTokenPolicies)
                    {
                        if (list.Contains(policy))
                        {
                            list.Remove(policy);
                        }
                    }
                }
                if (currentTokenPolicies != null)
                {
                    for (int j = 0; j < currentTokenPolicies.Count; j++)
                    {
                        if (list.Contains(currentTokenPolicies[j]))
                        {
                            list.Remove(currentTokenPolicies[j]);
                        }
                    }
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].GetType() == typeof(UnconditionalPolicy))
                    {
                        UnconditionalPolicy policy3 = (UnconditionalPolicy) list[i];
                        UnconditionalPolicy policy2 = new UnconditionalPolicy(policy3.PrimaryIdentity, policy3.Issuances, expirationTime);
                        list[i] = policy2;
                    }
                }
            }
            return list.AsReadOnly();
        }

        internal static bool DoesSkiClauseMatchSigningToken(SecurityContextKeyIdentifierClause skiClause, Message request)
        {
            SecurityMessageProperty security = request.Properties.Security;
            if (security == null)
            {
                throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("SFxSecurityContextPropertyMissingFromRequestMessage")), request);
            }
            SecurityContextSecurityToken securityToken = (security.ProtectionToken != null) ? (security.ProtectionToken.SecurityToken as SecurityContextSecurityToken) : null;
            if ((securityToken != null) && skiClause.Matches(securityToken.ContextId, securityToken.KeyGeneration))
            {
                return true;
            }
            if (security.HasIncomingSupportingTokens)
            {
                for (int i = 0; i < security.IncomingSupportingTokens.Count; i++)
                {
                    if (security.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing)
                    {
                        securityToken = security.IncomingSupportingTokens[i].SecurityToken as SecurityContextSecurityToken;
                        if ((securityToken != null) && skiClause.Matches(securityToken.ContextId, securityToken.KeyGeneration))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void FlushLogonTokenCache()
        {
            if ((this.RequestSecurityTokenListener != null) && (this.RequestSecurityTokenListener.ChannelDispatchers.Count > 0))
            {
                IChannelListener listener = null;
                ILogonTokenCacheManager property = null;
                for (int i = 0; i < this.RequestSecurityTokenListener.ChannelDispatchers.Count; i++)
                {
                    listener = this.RequestSecurityTokenListener.ChannelDispatchers[i].Listener;
                    if (listener != null)
                    {
                        property = listener.GetProperty<ILogonTokenCacheManager>();
                        if (property != null)
                        {
                            property.FlushLogonTokenCache();
                        }
                    }
                }
            }
        }

        private DateTime GetKeyExpirationTime(SecurityToken currentToken, DateTime keyEffectiveTime)
        {
            DateTime time = TimeoutHelper.Add(keyEffectiveTime, this.keyRenewalInterval);
            DateTime time2 = (currentToken != null) ? currentToken.ValidTo : TimeoutHelper.Add(keyEffectiveTime, this.sessionTokenLifetime);
            if (time > time2)
            {
                time = time2;
            }
            return time;
        }

        private static SecurityTokenSpecification GetMatchingEndorsingSct(SecurityContextKeyIdentifierClause sctSkiClause, SecurityMessageProperty supportingTokenProperty)
        {
            if (sctSkiClause != null)
            {
                for (int i = 0; i < supportingTokenProperty.IncomingSupportingTokens.Count; i++)
                {
                    if ((supportingTokenProperty.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (supportingTokenProperty.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing))
                    {
                        SecurityContextSecurityToken securityToken = supportingTokenProperty.IncomingSupportingTokens[i].SecurityToken as SecurityContextSecurityToken;
                        if ((securityToken != null) && sctSkiClause.Matches(securityToken.ContextId, securityToken.KeyGeneration))
                        {
                            return supportingTokenProperty.IncomingSupportingTokens[i];
                        }
                    }
                }
            }
            return null;
        }

        private Message HandleOperationException(SecuritySessionOperation operation, Message request, Exception e)
        {
            SecurityTraceRecordHelper.TraceServerSessionOperationException(operation, e, this.ListenUri);
            return this.CreateFault(request, e);
        }

        private static bool IsSameIdentity(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, ServiceSecurityContext incomingContext)
        {
            Claim primaryIdentityClaim = System.ServiceModel.Security.SecurityUtils.GetPrimaryIdentityClaim(authorizationPolicies);
            if (primaryIdentityClaim == null)
            {
                return incomingContext.IsAnonymous;
            }
            return Claim.DefaultComparer.Equals(incomingContext.IdentityClaim, primaryIdentityClaim);
        }

        private SecurityContextSecurityToken IssueToken(RequestSecurityToken rst, Message request, SecurityContextSecurityToken currentToken, ReadOnlyCollection<IAuthorizationPolicy> currentTokenPolicies, out RequestSecurityTokenResponse rstr)
        {
            ServiceSecurityContext serviceSecurityContext;
            byte[] buffer;
            byte[] buffer2;
            int num;
            SecurityToken token;
            SecurityContextSecurityToken token2;
            if ((rst.TokenType != null) && (rst.TokenType != this.sctUri))
            {
                throw TraceUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotIssueRstTokenType", new object[] { rst.TokenType })), request);
            }
            SecurityMessageProperty security = request.Properties.Security;
            if (security != null)
            {
                serviceSecurityContext = security.ServiceSecurityContext;
            }
            else
            {
                serviceSecurityContext = ServiceSecurityContext.Anonymous;
            }
            if (serviceSecurityContext == null)
            {
                throw TraceUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityContextMissing", new object[] { request.Headers.Action })), request);
            }
            if ((currentToken != null) && !IsSameIdentity(currentToken.AuthorizationPolicies, serviceSecurityContext))
            {
                throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("WrongIdentityRenewingToken")), request);
            }
            WSTrust.Driver.ProcessRstAndIssueKey(rst, null, this.KeyEntropyMode, this.SecurityAlgorithmSuite, out num, out buffer2, out buffer, out token);
            DateTime utcNow = DateTime.UtcNow;
            DateTime keyExpirationTime = this.GetKeyExpirationTime(currentToken, utcNow);
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = (security != null) ? CreateSecureConversationPolicies(security, currentTokenPolicies, keyExpirationTime) : EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            if (currentToken != null)
            {
                token2 = new SecurityContextSecurityToken(currentToken, System.ServiceModel.Security.SecurityUtils.GenerateId(), buffer, System.ServiceModel.Security.SecurityUtils.GenerateUniqueId(), utcNow, keyExpirationTime, authorizationPolicies);
            }
            else
            {
                UniqueId contextId = System.ServiceModel.Security.SecurityUtils.GenerateUniqueId();
                string id = System.ServiceModel.Security.SecurityUtils.GenerateId();
                DateTime time = utcNow;
                DateTime validTo = TimeoutHelper.Add(time, this.sessionTokenLifetime);
                token2 = new SecurityContextSecurityToken(contextId, id, buffer, time, validTo, null, utcNow, keyExpirationTime, authorizationPolicies);
                if (this.preserveBootstrapTokens)
                {
                    token2.BootstrapMessageProperty = (security == null) ? null : ((SecurityMessageProperty) security.CreateCopy());
                    System.ServiceModel.Security.SecurityUtils.ErasePasswordInUsernameTokenIfPresent(token2.BootstrapMessageProperty);
                }
            }
            rstr = new RequestSecurityTokenResponse(this.standardsManager);
            rstr.Context = rst.Context;
            rstr.KeySize = num;
            rstr.RequestedUnattachedReference = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(token2, SecurityTokenReferenceStyle.External);
            rstr.RequestedAttachedReference = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(token2, SecurityTokenReferenceStyle.Internal);
            rstr.TokenType = this.sctUri;
            rstr.RequestedSecurityToken = token2;
            if (buffer2 != null)
            {
                rstr.SetIssuerEntropy(buffer2);
                rstr.ComputeKey = true;
            }
            if (token != null)
            {
                rstr.RequestedProofToken = token;
            }
            rstr.SetLifetime(utcNow, keyExpirationTime);
            return token2;
        }

        private void NotifyOperationCompletion(SecuritySessionOperation operation, SecurityContextSecurityToken newSessionToken, SecurityContextSecurityToken previousSessionToken, EndpointAddress remoteAddress)
        {
            if (operation == SecuritySessionOperation.Issue)
            {
                if (this.issuedSecurityTokenHandler == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException("IssueSessionTokenHandlerNotSet"));
                }
                this.issuedSecurityTokenHandler(newSessionToken, remoteAddress);
            }
            else
            {
                if (operation != SecuritySessionOperation.Renew)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                if (this.renewedSecurityTokenHandler == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException("RenewSessionTokenHandlerNotSet"));
                }
                this.renewedSecurityTokenHandler(newSessionToken, previousSessionToken);
            }
        }

        public override void OnAbort()
        {
            if (this.rstListener != null)
            {
                this.rstListener.Abort();
                this.rstListener = null;
            }
            if (this.issuedTokenCache != null)
            {
                this.issuedTokenCache.ClearContexts();
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.rstListener != null)
            {
                this.rstListener.Close(helper.RemainingTime());
                this.rstListener = null;
            }
            if (this.issuedTokenCache != null)
            {
                this.issuedTokenCache.ClearContexts();
            }
            base.OnClose(helper.RemainingTime());
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.BootstrapSecurityBindingElement == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BootstrapSecurityBindingElementNotSet", new object[] { base.GetType() })));
            }
            if (this.IssuerBindingContext == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuerBuildContextNotSet", new object[] { base.GetType() })));
            }
            if (this.IssuedSecurityTokenParameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedSecurityTokenParametersNotSet", new object[] { base.GetType() })));
            }
            if (this.SecurityAlgorithmSuite == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityAlgorithmSuiteNotSet", new object[] { base.GetType() })));
            }
            if (this.IssuedTokenCache == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedTokenCacheNotSet", new object[] { base.GetType() })));
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.SetupSessionListener();
            this.rstListener.Open(helper.RemainingTime());
            this.sctUri = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
            base.OnOpen(helper.RemainingTime());
        }

        protected virtual Message ProcessIssueRequest(Message request)
        {
            Message message2;
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            try
            {
                RequestSecurityToken token;
                EndpointAddress address;
                DataContractSerializer serializer;
                string str;
                string str2;
                RequestSecurityTokenResponse response;
                using (XmlDictionaryReader reader = request.GetReaderAtBodyContents())
                {
                    token = this.StandardsManager.TrustDriver.CreateRequestSecurityToken(reader);
                    request.ReadFromBodyContentsToEnd(reader);
                }
                if ((token.RequestType != null) && (token.RequestType != this.StandardsManager.TrustDriver.RequestTypeIssue))
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidRstRequestType", new object[] { token.RequestType })), request);
                }
                token.GetAppliesToQName(out str, out str2);
                if ((str == "EndpointReference") && (str2 == request.Version.Addressing.Namespace))
                {
                    if (request.Version.Addressing != AddressingVersion.WSAddressing10)
                    {
                        if (request.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { request.Version.Addressing })));
                        }
                        serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddressAugust2004), 0x10000);
                        address = token.GetAppliesTo<EndpointAddressAugust2004>(serializer).ToEndpointAddress();
                    }
                    else
                    {
                        serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddress10), 0x10000);
                        address = token.GetAppliesTo<EndpointAddress10>(serializer).ToEndpointAddress();
                    }
                }
                else
                {
                    address = null;
                    serializer = null;
                }
                if (this.shouldMatchRstWithEndpointFilter)
                {
                    System.ServiceModel.Security.SecurityUtils.MatchRstWithEndpointFilter(request, this.endpointFilterTable, this.listenUri);
                }
                SecurityContextSecurityToken newSessionToken = this.IssueToken(token, request, null, null, out response);
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
                BodyWriter body = response;
                if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    RequestSecurityTokenResponseCollection responses = new RequestSecurityTokenResponseCollection(new List<RequestSecurityTokenResponse>(1) { response }, this.StandardsManager);
                    body = responses;
                }
                this.NotifyOperationCompletion(SecuritySessionOperation.Issue, newSessionToken, null, request.Headers.ReplyTo);
                Message message = CreateReply(request, this.IssueResponseAction, body);
                if (!newSessionToken.IsCookieMode)
                {
                    this.issuedTokenCache.AddContext(newSessionToken);
                }
                message2 = message;
            }
            finally
            {
                RemoveCachedTokensIfRequired(request.Properties.Security);
            }
            return message2;
        }

        protected virtual Message ProcessRenewRequest(Message request)
        {
            Message message2;
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            try
            {
                RequestSecurityToken token;
                RequestSecurityTokenResponse response;
                SecurityMessageProperty security = request.Properties.Security;
                if ((security == null) || !security.HasIncomingSupportingTokens)
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("RenewSessionMissingSupportingToken")), request);
                }
                XmlDictionaryReader readerAtBodyContents = request.GetReaderAtBodyContents();
                using (readerAtBodyContents)
                {
                    token = this.StandardsManager.TrustDriver.CreateRequestSecurityToken(readerAtBodyContents);
                    request.ReadFromBodyContentsToEnd(readerAtBodyContents);
                }
                if (token.RequestType != this.StandardsManager.TrustDriver.RequestTypeRenew)
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidRstRequestType", new object[] { token.RequestType })), request);
                }
                if (token.RenewTarget == null)
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoRenewTargetSpecified")), request);
                }
                SecurityContextKeyIdentifierClause renewTarget = token.RenewTarget as SecurityContextKeyIdentifierClause;
                SecurityTokenSpecification matchingEndorsingSct = GetMatchingEndorsingSct(renewTarget, security);
                if ((renewTarget == null) || (matchingEndorsingSct == null))
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("BadRenewTarget", new object[] { token.RenewTarget })), request);
                }
                SecurityContextSecurityToken newSessionToken = this.IssueToken(token, request, (SecurityContextSecurityToken) matchingEndorsingSct.SecurityToken, matchingEndorsingSct.SecurityTokenPolicies, out response);
                response.MakeReadOnly();
                BodyWriter body = response;
                if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    RequestSecurityTokenResponseCollection responses = new RequestSecurityTokenResponseCollection(new List<RequestSecurityTokenResponse>(1) { response }, this.StandardsManager);
                    body = responses;
                }
                this.NotifyOperationCompletion(SecuritySessionOperation.Renew, newSessionToken, (SecurityContextSecurityToken) matchingEndorsingSct.SecurityToken, request.Headers.ReplyTo);
                Message message = CreateReply(request, this.RenewResponseAction, body);
                if (!newSessionToken.IsCookieMode)
                {
                    this.issuedTokenCache.AddContext(newSessionToken);
                }
                message2 = message;
            }
            finally
            {
                RemoveCachedTokensIfRequired(request.Properties.Security);
            }
            return message2;
        }

        private Message ProcessRequest(Message request)
        {
            SecuritySessionOperation none = SecuritySessionOperation.None;
            try
            {
                if (request == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
                }
                if (request.Headers.Action == this.IssueAction.Value)
                {
                    none = SecuritySessionOperation.Issue;
                    return this.ProcessIssueRequest(request);
                }
                if (request.Headers.Action != this.RenewAction.Value)
                {
                    throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidActionForNegotiationMessage", new object[] { request.Headers.Action })), request);
                }
                none = SecuritySessionOperation.Renew;
                return this.ProcessRenewRequest(request);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                return this.HandleOperationException(none, request, exception);
            }
        }

        public bool RemoveCachedLogonToken(string username)
        {
            if (this.RequestSecurityTokenListener != null)
            {
                IChannelListener listener = null;
                ILogonTokenCacheManager property = null;
                for (int i = 0; i < this.RequestSecurityTokenListener.ChannelDispatchers.Count; i++)
                {
                    listener = this.RequestSecurityTokenListener.ChannelDispatchers[i].Listener;
                    if (listener != null)
                    {
                        property = listener.GetProperty<ILogonTokenCacheManager>();
                        if (property != null)
                        {
                            return property.RemoveCachedLogonToken(username);
                        }
                    }
                }
            }
            return false;
        }

        internal static void RemoveCachedTokensIfRequired(SecurityMessageProperty security)
        {
            if (security != null)
            {
                ILogonTokenCacheManager property = OperationContext.Current.EndpointDispatcher.ChannelDispatcher.Listener.GetProperty<ILogonTokenCacheManager>();
                Collection<ISecurityContextSecurityTokenCache> collection = OperationContext.Current.EndpointDispatcher.ChannelDispatcher.Listener.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
                if ((property != null) || ((collection != null) && (collection.Count != 0)))
                {
                    Collection<SecurityContextSecurityToken> sctsToRemove = new Collection<SecurityContextSecurityToken>();
                    if (security.ProtectionToken != null)
                    {
                        AddTokenToRemoveIfRequired(security.ProtectionToken.SecurityToken, sctsToRemove);
                    }
                    if (security.InitiatorToken != null)
                    {
                        AddTokenToRemoveIfRequired(security.InitiatorToken.SecurityToken, sctsToRemove);
                    }
                    if (security.HasIncomingSupportingTokens)
                    {
                        for (int i = 0; i < security.IncomingSupportingTokens.Count; i++)
                        {
                            if (((security.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (security.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEncrypted)) || (security.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing))
                            {
                                AddTokenToRemoveIfRequired(security.IncomingSupportingTokens[i].SecurityToken, sctsToRemove);
                            }
                        }
                    }
                    if (collection != null)
                    {
                        for (int j = 0; j < sctsToRemove.Count; j++)
                        {
                            for (int k = 0; k < collection.Count; k++)
                            {
                                collection[k].RemoveContext(sctsToRemove[j].ContextId, sctsToRemove[j].KeyGeneration);
                            }
                        }
                    }
                }
            }
        }

        private void SetupSessionListener()
        {
            ChannelBuilder channelBuilder = new ChannelBuilder(this.IssuerBindingContext, true);
            channelBuilder.Binding.Elements.Insert(0, new ReplyAdapterBindingElement());
            channelBuilder.Binding.Elements.Insert(0, new SecuritySessionAuthenticatorBindingElement(this));
            List<string> list = new List<string> {
                this.IssueAction.Value,
                this.RenewAction.Value
            };
            foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(this.IssuerBindingContext.Binding.Elements.Find<SecurityBindingElement>()))
            {
                if (parameters is SecureConversationSecurityTokenParameters)
                {
                    SecureConversationSecurityTokenParameters parameters2 = (SecureConversationSecurityTokenParameters) parameters;
                    if (!parameters2.CanRenewSession)
                    {
                        list.Remove(this.RenewAction.Value);
                        break;
                    }
                }
            }
            MessageFilter filter = new SessionActionFilter(this.standardsManager, list.ToArray());
            SecuritySessionHost host = new SecuritySessionHost(this, filter, this.ListenUri, channelBuilder);
            this.rstListener = host;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            SecurityContextSecurityToken token2 = (SecurityContextSecurityToken) token;
            return token2.AuthorizationPolicies;
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

        public IMessageFilterTable<EndpointAddress> EndpointFilterTable
        {
            get
            {
                return this.endpointFilterTable;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.endpointFilterTable = value;
            }
        }

        public bool IsClientAnonymous
        {
            get
            {
                return this.isClientAnonymous;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.isClientAnonymous = value;
            }
        }

        public virtual XmlDictionaryString IssueAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.IssueAction;
            }
        }

        public System.ServiceModel.Security.Tokens.IssuedSecurityTokenHandler IssuedSecurityTokenHandler
        {
            get
            {
                return this.issuedSecurityTokenHandler;
            }
            set
            {
                this.issuedSecurityTokenHandler = value;
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

        public ISecurityContextSecurityTokenCache IssuedTokenCache
        {
            get
            {
                return this.issuedTokenCache;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuedTokenCache = value;
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

        public TimeSpan KeyRenewalInterval
        {
            get
            {
                return this.keyRenewalInterval;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.keyRenewalInterval = value;
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.listenUri = value;
            }
        }

        public int MaximumConcurrentNegotiations
        {
            get
            {
                return this.maximumConcurrentNegotiations;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value < 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.maximumConcurrentNegotiations = value;
            }
        }

        public TimeSpan NegotiationTimeout
        {
            get
            {
                return this.negotiationTimeout;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.negotiationTimeout = value;
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

        public virtual XmlDictionaryString RenewAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.RenewAction;
            }
        }

        public System.ServiceModel.Security.Tokens.RenewedSecurityTokenHandler RenewedSecurityTokenHandler
        {
            get
            {
                return this.renewedSecurityTokenHandler;
            }
            set
            {
                this.renewedSecurityTokenHandler = value;
            }
        }

        public virtual XmlDictionaryString RenewResponseAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.RenewResponseAction;
            }
        }

        internal ServiceHostBase RequestSecurityTokenListener
        {
            get
            {
                return this.rstListener;
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

        public SecurityContextSecurityTokenAuthenticator SessionTokenAuthenticator
        {
            get
            {
                return this.sessionTokenAuthenticator;
            }
        }

        public TimeSpan SessionTokenLifetime
        {
            get
            {
                return this.sessionTokenLifetime;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.sessionTokenLifetime = value;
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

        private class SecuritySessionAuthenticatorBindingElement : BindingElement
        {
            private SecuritySessionSecurityTokenAuthenticator authenticator;

            public SecuritySessionAuthenticatorBindingElement(SecuritySessionSecurityTokenAuthenticator authenticator)
            {
                this.authenticator = authenticator;
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
            {
                if (context == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
                }
                return this.authenticator.BuildResponderChannelListener<TChannel>(context);
            }

            public override BindingElement Clone()
            {
                return new SecuritySessionSecurityTokenAuthenticator.SecuritySessionAuthenticatorBindingElement(this.authenticator);
            }

            public override T GetProperty<T>(BindingContext context) where T: class
            {
                if (typeof(T) == typeof(ISecurityCapabilities))
                {
                    return this.authenticator.BootstrapSecurityBindingElement.GetProperty<ISecurityCapabilities>(context);
                }
                return context.GetInnerProperty<T>();
            }
        }

        private class SecuritySessionHost : ServiceHostBase
        {
            private SecuritySessionSecurityTokenAuthenticator authenticator;
            private ChannelBuilder channelBuilder;
            private MessageFilter filter;
            private Uri listenUri;

            public SecuritySessionHost(SecuritySessionSecurityTokenAuthenticator authenticator, MessageFilter filter, Uri listenUri, ChannelBuilder channelBuilder)
            {
                this.authenticator = authenticator;
                this.filter = filter;
                this.listenUri = listenUri;
                this.channelBuilder = channelBuilder;
            }

            protected override System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
            {
                implementedContracts = null;
                return null;
            }

            protected override void InitializeRuntime()
            {
                MessageFilter filter = this.filter;
                int priority = 0x7ffffff5;
                System.Type[] supportedChannels = new System.Type[] { typeof(IReplyChannel), typeof(IDuplexChannel), typeof(IReplySessionChannel), typeof(IDuplexSessionChannel) };
                IChannelListener result = null;
                BindingParameterCollection parameters = new BindingParameterCollection(this.channelBuilder.BindingParameters);
                Binding binding = this.channelBuilder.Binding;
                binding.ReceiveTimeout = this.authenticator.NegotiationTimeout;
                parameters.Add(new ChannelDemuxerFilter(filter, priority));
                DispatcherBuilder.MaybeCreateListener(true, supportedChannels, binding, parameters, this.listenUri, "", ListenUriMode.Explicit, base.ServiceThrottle, out result);
                if (result == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotCreateTwoWayListenerForNegotiation")));
                }
                ChannelDispatcher item = new ChannelDispatcher(result, null, binding) {
                    MessageVersion = binding.MessageVersion,
                    ManualAddressing = true,
                    ServiceThrottle = new ServiceThrottle(this)
                };
                item.ServiceThrottle.MaxConcurrentCalls = this.authenticator.MaximumConcurrentNegotiations;
                item.ServiceThrottle.MaxConcurrentSessions = this.authenticator.MaximumConcurrentNegotiations;
                EndpointDispatcher dispatcher2 = new EndpointDispatcher(new EndpointAddress(this.listenUri, new AddressHeader[0]), "IssueAndRenewSession", "http://tempuri.org/", true) {
                    DispatchRuntime = { SingletonInstanceContext = new InstanceContext(null, this.authenticator, false), ConcurrencyMode = ConcurrencyMode.Multiple },
                    AddressFilter = new MatchAllMessageFilter(),
                    ContractFilter = filter,
                    FilterPriority = priority
                };
                dispatcher2.DispatchRuntime.PrincipalPermissionMode = PrincipalPermissionMode.None;
                dispatcher2.DispatchRuntime.InstanceContextProvider = new SingletonInstanceContextProvider(dispatcher2.DispatchRuntime);
                dispatcher2.DispatchRuntime.SynchronizationContext = null;
                if ((this.authenticator.IssuerBindingContext != null) && (this.authenticator.IssuerBindingContext.BindingParameters != null))
                {
                    ServiceAuthenticationManager wrappedServiceAuthManager = this.authenticator.IssuerBindingContext.BindingParameters.Find<ServiceAuthenticationManager>();
                    if (wrappedServiceAuthManager != null)
                    {
                        dispatcher2.DispatchRuntime.ServiceAuthenticationManager = new SCTServiceAuthenticationManagerWrapper(wrappedServiceAuthManager);
                    }
                }
                DispatchOperation operation = new DispatchOperation(dispatcher2.DispatchRuntime, "*", "*", "*") {
                    Formatter = new MessageOperationFormatter(),
                    Invoker = new SecuritySessionAuthenticatorInvoker(this.authenticator)
                };
                dispatcher2.DispatchRuntime.UnhandledDispatchOperation = operation;
                item.Endpoints.Add(dispatcher2);
                base.ChannelDispatchers.Add(item);
            }

            private class SecuritySessionAuthenticatorInvoker : IOperationInvoker
            {
                private SecuritySessionSecurityTokenAuthenticator parent;

                internal SecuritySessionAuthenticatorInvoker(SecuritySessionSecurityTokenAuthenticator parent)
                {
                    this.parent = parent;
                }

                public object[] AllocateInputs()
                {
                    return EmptyArray<object>.Allocate(1);
                }

                public object Invoke(object instance, object[] inputs, out object[] outputs)
                {
                    outputs = EmptyArray<object>.Allocate(0);
                    return this.parent.ProcessRequest((Message) inputs[0]);
                }

                public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public bool IsSynchronous
                {
                    get
                    {
                        return true;
                    }
                }
            }
        }

        public class SessionRenewSecurityTokenManager : SecurityTokenManager
        {
            private SecurityTokenManager innerTokenManager;
            private SecurityTokenAuthenticator renewTokenAuthenticator;
            private SecurityTokenResolver renewTokenResolver;

            public SessionRenewSecurityTokenManager(SecurityTokenManager innerTokenManager, SecurityTokenAuthenticator renewTokenAuthenticator, SecurityTokenResolver renewTokenResolver)
            {
                this.innerTokenManager = innerTokenManager;
                this.renewTokenAuthenticator = renewTokenAuthenticator;
                this.renewTokenResolver = renewTokenResolver;
            }

            public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
            {
                if (tokenRequirement == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
                }
                if (tokenRequirement.TokenType == ServiceModelSecurityTokenTypes.SecurityContext)
                {
                    outOfBandTokenResolver = this.renewTokenResolver;
                    return this.renewTokenAuthenticator;
                }
                return this.innerTokenManager.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
            }

            public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement requirement)
            {
                return this.innerTokenManager.CreateSecurityTokenProvider(requirement);
            }

            public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
            {
                return this.innerTokenManager.CreateSecurityTokenSerializer(version);
            }
        }
    }
}

