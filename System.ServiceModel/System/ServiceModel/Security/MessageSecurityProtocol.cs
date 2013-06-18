namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    internal abstract class MessageSecurityProtocol : SecurityProtocol
    {
        private readonly System.ServiceModel.Security.MessageSecurityProtocolFactory factory;
        private SecurityToken identityVerifiedToken;

        protected MessageSecurityProtocol(System.ServiceModel.Security.MessageSecurityProtocolFactory factory, EndpointAddress target, Uri via) : base(factory, target, via)
        {
            this.factory = factory;
        }

        protected void AttachRecipientSecurityProperty(Message message, SecurityToken protectionToken, bool isWrappedToken, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens, IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            ReadOnlyCollection<IAuthorizationPolicy> instance;
            if (isWrappedToken)
            {
                instance = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            else
            {
                instance = tokenPoliciesMapping[protectionToken];
            }
            SecurityMessageProperty orCreate = SecurityMessageProperty.GetOrCreate(message);
            orCreate.ProtectionToken = new SecurityTokenSpecification(protectionToken, instance);
            base.AddSupportingTokenSpecification(orCreate, basicTokens, endorsingTokens, signedEndorsingTokens, signedTokens, tokenPoliciesMapping);
            orCreate.ServiceSecurityContext = new ServiceSecurityContext(orCreate.GetInitiatorTokenAuthorizationPolicies());
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            try
            {
                base.CommunicationObject.ThrowIfClosedOrNotOpen();
                this.ValidateOutgoingState(message);
                if (!this.RequiresOutgoingSecurityProcessing && (message.Properties.Security == null))
                {
                    return new CompletedAsyncResult<Message>(message, callback, state);
                }
                result = this.BeginSecureOutgoingMessageCore(message, timeout, null, callback, state);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
            return result;
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            try
            {
                base.CommunicationObject.ThrowIfClosedOrNotOpen();
                this.ValidateOutgoingState(message);
                if (!this.RequiresOutgoingSecurityProcessing && (message.Properties.Security == null))
                {
                    return new CompletedAsyncResult<Message>(message, callback, state);
                }
                result = this.BeginSecureOutgoingMessageCore(message, timeout, correlationState, callback, state);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
            return result;
        }

        protected abstract IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state);
        protected void CheckSignatureConfirmation(ReceiveSecurityHeader securityHeader, SecurityProtocolCorrelationState[] correlationStates)
        {
            SignatureConfirmations sentSignatureConfirmations = securityHeader.GetSentSignatureConfirmations();
            SignatureConfirmations signatureConfirmations = null;
            if (correlationStates != null)
            {
                for (int i = 0; i < correlationStates.Length; i++)
                {
                    if (correlationStates[i].SignatureConfirmations != null)
                    {
                        signatureConfirmations = correlationStates[i].SignatureConfirmations;
                        break;
                    }
                }
            }
            if (signatureConfirmations == null)
            {
                if ((sentSignatureConfirmations != null) && (sentSignatureConfirmations.Count > 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("FoundUnexpectedSignatureConfirmations")));
                }
            }
            else
            {
                bool flag = false;
                if ((sentSignatureConfirmations != null) && (signatureConfirmations.Count == sentSignatureConfirmations.Count))
                {
                    bool[] flagArray = new bool[signatureConfirmations.Count];
                    for (int j = 0; j < signatureConfirmations.Count; j++)
                    {
                        byte[] buffer;
                        bool flag2;
                        signatureConfirmations.GetConfirmation(j, out buffer, out flag2);
                        for (int k = 0; k < sentSignatureConfirmations.Count; k++)
                        {
                            if (!flagArray[k])
                            {
                                byte[] buffer2;
                                bool flag3;
                                sentSignatureConfirmations.GetConfirmation(k, out buffer2, out flag3);
                                if ((flag3 == flag2) && CryptoHelper.IsEqual(buffer2, buffer))
                                {
                                    flagArray[k] = true;
                                    break;
                                }
                            }
                        }
                    }
                    int index = 0;
                    while (index < flagArray.Length)
                    {
                        if (!flagArray[index])
                        {
                            break;
                        }
                        index++;
                    }
                    if (index == flagArray.Length)
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NotAllSignaturesConfirmed")));
                }
            }
        }

        protected ReceiveSecurityHeader ConfigureReceiveSecurityHeader(Message message, string actor, SecurityProtocolCorrelationState[] correlationStates, out IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators)
        {
            return this.ConfigureReceiveSecurityHeader(message, actor, correlationStates, null, out supportingAuthenticators);
        }

        protected ReceiveSecurityHeader ConfigureReceiveSecurityHeader(Message message, string actor, SecurityProtocolCorrelationState[] correlationStates, SecurityStandardsManager standardsManager, out IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators)
        {
            System.ServiceModel.Security.MessageSecurityProtocolFactory messageSecurityProtocolFactory = this.MessageSecurityProtocolFactory;
            MessageDirection transferDirection = messageSecurityProtocolFactory.ActAsInitiator ? MessageDirection.Output : MessageDirection.Input;
            ReceiveSecurityHeader securityHeader = this.CreateSecurityHeader(message, actor, transferDirection, standardsManager);
            string action = message.Headers.Action;
            supportingAuthenticators = base.GetSupportingTokenAuthenticatorsAndSetExpectationFlags(this.factory, message, securityHeader);
            if (messageSecurityProtocolFactory.RequireIntegrity || securityHeader.ExpectSignedTokens)
            {
                securityHeader.RequiredSignatureParts = messageSecurityProtocolFactory.GetIncomingSignatureParts(action);
            }
            if (messageSecurityProtocolFactory.RequireConfidentiality || securityHeader.ExpectBasicTokens)
            {
                securityHeader.RequiredEncryptionParts = messageSecurityProtocolFactory.GetIncomingEncryptionParts(action);
            }
            securityHeader.ExpectEncryption = messageSecurityProtocolFactory.RequireConfidentiality || securityHeader.ExpectBasicTokens;
            securityHeader.ExpectSignature = messageSecurityProtocolFactory.RequireIntegrity || securityHeader.ExpectSignedTokens;
            securityHeader.SetRequiredProtectionOrder(messageSecurityProtocolFactory.MessageProtectionOrder);
            if ((messageSecurityProtocolFactory.ActAsInitiator && messageSecurityProtocolFactory.DoRequestSignatureConfirmation) && this.HasCorrelationState(correlationStates))
            {
                securityHeader.MaintainSignatureConfirmationState = true;
                securityHeader.ExpectSignatureConfirmation = true;
                return securityHeader;
            }
            if (!messageSecurityProtocolFactory.ActAsInitiator && messageSecurityProtocolFactory.DoRequestSignatureConfirmation)
            {
                securityHeader.MaintainSignatureConfirmationState = true;
                return securityHeader;
            }
            securityHeader.MaintainSignatureConfirmationState = false;
            return securityHeader;
        }

        protected SendSecurityHeader ConfigureSendSecurityHeader(Message message, string actor, IList<SupportingTokenSpecification> supportingTokens, SecurityProtocolCorrelationState correlationState)
        {
            System.ServiceModel.Security.MessageSecurityProtocolFactory messageSecurityProtocolFactory = this.MessageSecurityProtocolFactory;
            SendSecurityHeader securityHeader = base.CreateSendSecurityHeader(message, actor, messageSecurityProtocolFactory);
            securityHeader.SignThenEncrypt = messageSecurityProtocolFactory.MessageProtectionOrder != MessageProtectionOrder.EncryptBeforeSign;
            securityHeader.EncryptPrimarySignature = messageSecurityProtocolFactory.MessageProtectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
            if (messageSecurityProtocolFactory.DoRequestSignatureConfirmation && (correlationState != null))
            {
                if (messageSecurityProtocolFactory.ActAsInitiator)
                {
                    securityHeader.MaintainSignatureConfirmationState = true;
                    securityHeader.CorrelationState = correlationState;
                }
                else if (correlationState.SignatureConfirmations != null)
                {
                    securityHeader.AddSignatureConfirmations(correlationState.SignatureConfirmations);
                }
            }
            string action = message.Headers.Action;
            if (this.factory.ApplyIntegrity)
            {
                securityHeader.SignatureParts = this.factory.GetOutgoingSignatureParts(action);
            }
            if (messageSecurityProtocolFactory.ApplyConfidentiality)
            {
                securityHeader.EncryptionParts = this.factory.GetOutgoingEncryptionParts(action);
            }
            base.AddSupportingTokens(securityHeader, supportingTokens);
            return securityHeader;
        }

        protected ReceiveSecurityHeader CreateSecurityHeader(Message message, string actor, MessageDirection transferDirection, SecurityStandardsManager standardsManager)
        {
            standardsManager = standardsManager ?? this.factory.StandardsManager;
            ReceiveSecurityHeader header = standardsManager.CreateReceiveSecurityHeader(message, actor, this.factory.IncomingAlgorithmSuite, transferDirection);
            header.Layout = this.factory.SecurityHeaderLayout;
            header.MaxReceivedMessageSize = this.factory.SecurityBindingElement.MaxReceivedMessageSize;
            header.ReaderQuotas = this.factory.SecurityBindingElement.ReaderQuotas;
            if (this.factory.ExpectKeyDerivation)
            {
                header.DerivedTokenAuthenticator = this.factory.DerivedKeyTokenAuthenticator;
            }
            return header;
        }

        protected void DoIdentityCheckAndAttachInitiatorSecurityProperty(Message message, SecurityToken protectionToken, ReadOnlyCollection<IAuthorizationPolicy> protectionTokenPolicies)
        {
            AuthorizationContext authorizationContext = this.EnsureIncomingIdentity(message, protectionToken, protectionTokenPolicies);
            SecurityMessageProperty orCreate = SecurityMessageProperty.GetOrCreate(message);
            orCreate.ProtectionToken = new SecurityTokenSpecification(protectionToken, protectionTokenPolicies);
            orCreate.ServiceSecurityContext = new ServiceSecurityContext(authorizationContext, protectionTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
        }

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            try
            {
                SecurityProtocolCorrelationState state;
                this.EndSecureOutgoingMessageCore(result, out message, out state);
                base.OnOutgoingMessageSecured(message);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(null);
                throw;
            }
        }

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            try
            {
                this.EndSecureOutgoingMessageCore(result, out message, out newCorrelationState);
                base.OnOutgoingMessageSecured(message);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(null);
                throw;
            }
        }

        protected abstract void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState);
        protected AuthorizationContext EnsureIncomingIdentity(Message message, SecurityToken token, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (token == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSigningTokenAvailableToDoIncomingIdentityCheck")), message);
            }
            AuthorizationContext authorizationContext = (authorizationPolicies != null) ? AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies) : null;
            if (this.factory.IdentityVerifier != null)
            {
                if (base.Target == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoOutgoingEndpointAddressAvailableForDoingIdentityCheckOnReply")), message);
                }
                this.factory.IdentityVerifier.EnsureIncomingIdentity(base.Target, authorizationContext);
            }
            return authorizationContext;
        }

        protected static void EnsureNonWrappedToken(SecurityToken token, Message message)
        {
            if (token is WrappedKeySecurityToken)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenNotExpectedInSecurityHeader", new object[] { token })), message);
            }
        }

        protected void EnsureOutgoingIdentity(SecurityToken token, SecurityTokenAuthenticator authenticator)
        {
            if (!object.ReferenceEquals(token, this.identityVerifiedToken) && (this.factory.IdentityVerifier != null))
            {
                if (base.Target == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoOutgoingEndpointAddressAvailableForDoingIdentityCheck")));
                }
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = authenticator.ValidateToken(token);
                this.factory.IdentityVerifier.EnsureOutgoingIdentity(base.Target, authorizationPolicies);
                if (this.CacheIdentityCheckResultForToken)
                {
                    this.identityVerifiedToken = token;
                }
            }
        }

        protected SecurityProtocolCorrelationState GetCorrelationState(SecurityToken correlationToken)
        {
            return new SecurityProtocolCorrelationState(correlationToken);
        }

        protected SecurityProtocolCorrelationState GetCorrelationState(SecurityToken correlationToken, ReceiveSecurityHeader securityHeader)
        {
            SecurityProtocolCorrelationState state = new SecurityProtocolCorrelationState(correlationToken);
            if (securityHeader.MaintainSignatureConfirmationState && !this.factory.ActAsInitiator)
            {
                state.SignatureConfirmations = securityHeader.GetSentSignatureValues();
            }
            return state;
        }

        protected SecurityToken GetCorrelationToken(SecurityProtocolCorrelationState[] correlationStates)
        {
            SecurityToken objA = null;
            if (correlationStates != null)
            {
                for (int i = 0; i < correlationStates.Length; i++)
                {
                    if (correlationStates[i].Token != null)
                    {
                        if (objA == null)
                        {
                            objA = correlationStates[i].Token;
                        }
                        else if (!object.ReferenceEquals(objA, correlationStates[i].Token))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MultipleCorrelationTokensFound")));
                        }
                    }
                }
            }
            if (objA == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoCorrelationTokenFound")));
            }
            return objA;
        }

        protected SecurityToken GetCorrelationToken(SecurityProtocolCorrelationState correlationState)
        {
            if ((correlationState == null) || (correlationState.Token == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("CannotFindCorrelationStateForApplyingSecurity")));
            }
            return correlationState.Token;
        }

        internal SecurityProtocolCorrelationState GetSignatureConfirmationCorrelationState(SecurityProtocolCorrelationState oldCorrelationState, SecurityProtocolCorrelationState newCorrelationState)
        {
            if (this.factory.ActAsInitiator)
            {
                return newCorrelationState;
            }
            return oldCorrelationState;
        }

        protected SecurityToken GetTokenAndEnsureOutgoingIdentity(SecurityTokenProvider provider, bool isEncryptionOn, TimeSpan timeout, SecurityTokenAuthenticator authenticator)
        {
            SecurityToken token = SecurityProtocol.GetToken(provider, base.Target, timeout);
            if (isEncryptionOn)
            {
                this.EnsureOutgoingIdentity(token, authenticator);
            }
            return token;
        }

        private bool HasCorrelationState(SecurityProtocolCorrelationState[] correlationState)
        {
            if ((correlationState == null) || (correlationState.Length == 0))
            {
                return false;
            }
            if ((correlationState.Length == 1) && (correlationState[0] == null))
            {
                return false;
            }
            return true;
        }

        protected void ProcessSecurityHeader(ReceiveSecurityHeader securityHeader, ref Message message, SecurityToken requiredSigningToken, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            securityHeader.SetTimeParameters(this.factory.NonceCache, this.factory.ReplayWindow, this.factory.MaxClockSkew);
            securityHeader.Process(helper.RemainingTime(), System.ServiceModel.Security.SecurityUtils.GetChannelBindingFromMessage(message), this.factory.ExtendedProtectionPolicy);
            if (this.factory.AddTimestamp && (securityHeader.Timestamp == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredTimestampMissingInSecurityHeader")));
            }
            if ((requiredSigningToken != null) && (requiredSigningToken != securityHeader.SignatureToken))
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("ReplyWasNotSignedWithRequiredSigningToken")), message);
            }
            if (this.DoAutomaticEncryptionMatch)
            {
                System.ServiceModel.Security.SecurityUtils.EnsureExpectedSymmetricMatch(securityHeader.SignatureToken, securityHeader.EncryptionToken, message);
            }
            if (securityHeader.MaintainSignatureConfirmationState && this.factory.ActAsInitiator)
            {
                this.CheckSignatureConfirmation(securityHeader, correlationStates);
            }
            message = securityHeader.ProcessedMessage;
        }

        protected bool RequiresIncomingSecurityProcessing(Message message)
        {
            if ((this.factory.ActAsInitiator && this.factory.SecurityBindingElement.EnableUnsecuredResponse) && !this.factory.StandardsManager.SecurityVersion.DoesMessageContainSecurityHeader(message))
            {
                return false;
            }
            if (!((this.factory.RequireIntegrity || this.factory.RequireConfidentiality) || this.factory.DetectReplays))
            {
                return this.factory.ExpectSupportingTokens;
            }
            return true;
        }

        public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
        {
            try
            {
                base.CommunicationObject.ThrowIfClosedOrNotOpen();
                this.ValidateOutgoingState(message);
                if (this.RequiresOutgoingSecurityProcessing || (message.Properties.Security != null))
                {
                    this.SecureOutgoingMessageCore(ref message, timeout, null);
                    base.OnOutgoingMessageSecured(message);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        public override SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityProtocolCorrelationState state2;
            try
            {
                base.CommunicationObject.ThrowIfClosedOrNotOpen();
                this.ValidateOutgoingState(message);
                if (!this.RequiresOutgoingSecurityProcessing && (message.Properties.Security == null))
                {
                    return null;
                }
                SecurityProtocolCorrelationState state = this.SecureOutgoingMessageCore(ref message, timeout, correlationState);
                base.OnOutgoingMessageSecured(message);
                state2 = state;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
            return state2;
        }

        protected abstract SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState);
        private void ValidateOutgoingState(Message message)
        {
            if (this.PerformIncomingAndOutgoingMessageExpectationChecks && !this.factory.ExpectOutgoingMessages)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityBindingNotSetUpToProcessOutgoingMessages")));
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
        }

        public override void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
        {
            try
            {
                base.CommunicationObject.ThrowIfClosedOrNotOpen();
                if (this.PerformIncomingAndOutgoingMessageExpectationChecks && !this.factory.ExpectIncomingMessages)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityBindingNotSetUpToProcessIncomingMessages")));
                }
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                if (this.RequiresIncomingSecurityProcessing(message))
                {
                    string actor = string.Empty;
                    this.VerifyIncomingMessageCore(ref message, actor, timeout, null);
                    base.OnIncomingMessageVerified(message);
                }
            }
            catch (MessageSecurityException exception)
            {
                base.OnVerifyIncomingMessageFailure(message, exception);
                throw;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                base.OnVerifyIncomingMessageFailure(message, exception2);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageSecurityVerificationFailed"), exception2));
            }
        }

        public override SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout, params SecurityProtocolCorrelationState[] correlationStates)
        {
            SecurityProtocolCorrelationState state2;
            try
            {
                base.CommunicationObject.ThrowIfClosedOrNotOpen();
                if (this.PerformIncomingAndOutgoingMessageExpectationChecks && !this.factory.ExpectIncomingMessages)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityBindingNotSetUpToProcessIncomingMessages")));
                }
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                if (!this.RequiresIncomingSecurityProcessing(message))
                {
                    return null;
                }
                string actor = string.Empty;
                SecurityProtocolCorrelationState state = this.VerifyIncomingMessageCore(ref message, actor, timeout, correlationStates);
                base.OnIncomingMessageVerified(message);
                state2 = state;
            }
            catch (MessageSecurityException exception)
            {
                base.OnVerifyIncomingMessageFailure(message, exception);
                throw;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                base.OnVerifyIncomingMessageFailure(message, exception2);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageSecurityVerificationFailed"), exception2));
            }
            return state2;
        }

        protected abstract SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates);

        protected virtual bool CacheIdentityCheckResultForToken
        {
            get
            {
                return true;
            }
        }

        protected virtual bool DoAutomaticEncryptionMatch
        {
            get
            {
                return true;
            }
        }

        protected System.ServiceModel.Security.MessageSecurityProtocolFactory MessageSecurityProtocolFactory
        {
            get
            {
                return this.factory;
            }
        }

        protected virtual bool PerformIncomingAndOutgoingMessageExpectationChecks
        {
            get
            {
                return true;
            }
        }

        protected bool RequiresOutgoingSecurityProcessing
        {
            get
            {
                if (!this.factory.ActAsInitiator && this.factory.SecurityBindingElement.EnableUnsecuredResponse)
                {
                    return false;
                }
                if (!((this.factory.ApplyIntegrity || this.factory.ApplyConfidentiality) || this.factory.AddTimestamp))
                {
                    return this.factory.ExpectSupportingTokens;
                }
                return true;
            }
        }

        protected abstract class GetOneTokenAndSetUpSecurityAsyncResult : SecurityProtocol.GetSupportingTokensAsyncResult
        {
            private readonly MessageSecurityProtocol binding;
            private readonly bool doIdentityChecks;
            private static AsyncCallback getTokenCompleteCallback = Fx.ThunkCallback(new AsyncCallback(MessageSecurityProtocol.GetOneTokenAndSetUpSecurityAsyncResult.GetTokenCompleteCallback));
            private SecurityTokenAuthenticator identityCheckAuthenticator;
            private Message message;
            private SecurityProtocolCorrelationState newCorrelationState;
            private SecurityProtocolCorrelationState oldCorrelationState;
            private readonly SecurityTokenProvider provider;
            private TimeoutHelper timeoutHelper;

            public GetOneTokenAndSetUpSecurityAsyncResult(Message m, MessageSecurityProtocol binding, SecurityTokenProvider provider, bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator, SecurityProtocolCorrelationState oldCorrelationState, TimeSpan timeout, AsyncCallback callback, object state) : base(m, binding, timeout, callback, state)
            {
                this.message = m;
                this.binding = binding;
                this.provider = provider;
                this.doIdentityChecks = doIdentityChecks;
                this.oldCorrelationState = oldCorrelationState;
                this.identityCheckAuthenticator = identityCheckAuthenticator;
            }

            internal static Message End(IAsyncResult result, out SecurityProtocolCorrelationState newCorrelationState)
            {
                MessageSecurityProtocol.GetOneTokenAndSetUpSecurityAsyncResult result2 = AsyncResult.End<MessageSecurityProtocol.GetOneTokenAndSetUpSecurityAsyncResult>(result);
                newCorrelationState = result2.newCorrelationState;
                return result2.message;
            }

            private static void GetTokenCompleteCallback(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }
                if (!result.CompletedSynchronously)
                {
                    MessageSecurityProtocol.GetOneTokenAndSetUpSecurityAsyncResult asyncState = result.AsyncState as MessageSecurityProtocol.GetOneTokenAndSetUpSecurityAsyncResult;
                    if (asyncState == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("InvalidAsyncResult"));
                    }
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        SecurityToken token = asyncState.provider.EndGetToken(result);
                        flag = asyncState.OnGetTokenComplete(token);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                IAsyncResult result = this.provider.BeginGetToken(this.timeoutHelper.RemainingTime(), getTokenCompleteCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                SecurityToken token = this.provider.EndGetToken(result);
                return this.OnGetTokenComplete(token);
            }

            private bool OnGetTokenComplete(SecurityToken token)
            {
                if (token == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenProviderCannotGetTokensForTarget", new object[] { this.binding.Target })));
                }
                if (this.doIdentityChecks)
                {
                    this.binding.EnsureOutgoingIdentity(token, this.identityCheckAuthenticator);
                }
                this.OnGetTokenDone(ref this.message, token, this.timeoutHelper.RemainingTime());
                return true;
            }

            protected abstract void OnGetTokenDone(ref Message message, SecurityToken token, TimeSpan timeout);
            protected void SetCorrelationToken(SecurityToken token)
            {
                this.newCorrelationState = new SecurityProtocolCorrelationState(token);
            }

            protected MessageSecurityProtocol Binding
            {
                get
                {
                    return this.binding;
                }
            }

            protected SecurityProtocolCorrelationState NewCorrelationState
            {
                get
                {
                    return this.newCorrelationState;
                }
            }

            protected SecurityProtocolCorrelationState OldCorrelationState
            {
                get
                {
                    return this.oldCorrelationState;
                }
            }
        }

        protected abstract class GetTwoTokensAndSetUpSecurityAsyncResult : SecurityProtocol.GetSupportingTokensAsyncResult
        {
            private readonly MessageSecurityProtocol binding;
            private readonly bool doIdentityChecks;
            private static readonly AsyncCallback getPrimaryTokenCompleteCallback = Fx.ThunkCallback(new AsyncCallback(MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult.GetPrimaryTokenCompleteCallback));
            private static readonly AsyncCallback getSecondaryTokenCompleteCallback = Fx.ThunkCallback(new AsyncCallback(MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult.GetSecondaryTokenCompleteCallback));
            private SecurityTokenAuthenticator identityCheckAuthenticator;
            private Message message;
            private SecurityProtocolCorrelationState newCorrelationState;
            private SecurityProtocolCorrelationState oldCorrelationState;
            private readonly SecurityTokenProvider primaryProvider;
            private SecurityToken primaryToken;
            private readonly SecurityTokenProvider secondaryProvider;
            private TimeoutHelper timeoutHelper;

            public GetTwoTokensAndSetUpSecurityAsyncResult(Message m, MessageSecurityProtocol binding, SecurityTokenProvider primaryProvider, SecurityTokenProvider secondaryProvider, bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator, SecurityProtocolCorrelationState oldCorrelationState, TimeSpan timeout, AsyncCallback callback, object state) : base(m, binding, timeout, callback, state)
            {
                this.message = m;
                this.binding = binding;
                this.primaryProvider = primaryProvider;
                this.secondaryProvider = secondaryProvider;
                this.doIdentityChecks = doIdentityChecks;
                this.identityCheckAuthenticator = identityCheckAuthenticator;
                this.oldCorrelationState = oldCorrelationState;
            }

            internal static Message End(IAsyncResult result, out SecurityProtocolCorrelationState newCorrelationState)
            {
                MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult result2 = AsyncResult.End<MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult>(result);
                newCorrelationState = result2.newCorrelationState;
                return result2.message;
            }

            private static void GetPrimaryTokenCompleteCallback(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }
                if (!result.CompletedSynchronously)
                {
                    MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult asyncState = result.AsyncState as MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult;
                    if (asyncState == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("InvalidAsyncResult"));
                    }
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        SecurityToken token = asyncState.primaryProvider.EndGetToken(result);
                        flag = asyncState.OnGetPrimaryTokenComplete(token);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void GetSecondaryTokenCompleteCallback(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }
                if (!result.CompletedSynchronously)
                {
                    MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult asyncState = result.AsyncState as MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult;
                    if (asyncState == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("InvalidAsyncResult"));
                    }
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        SecurityToken token = asyncState.secondaryProvider.EndGetToken(result);
                        flag = asyncState.OnGetSecondaryTokenComplete(token);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            protected abstract void OnBothGetTokenCallsDone(ref Message message, SecurityToken primaryToken, SecurityToken secondaryToken, TimeSpan timeout);
            private bool OnGetPrimaryTokenComplete(SecurityToken token)
            {
                return this.OnGetPrimaryTokenComplete(token, false);
            }

            private bool OnGetPrimaryTokenComplete(SecurityToken token, bool primaryCallSkipped)
            {
                if (!primaryCallSkipped)
                {
                    if (token == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenProviderCannotGetTokensForTarget", new object[] { this.binding.Target })), this.message);
                    }
                    if (this.doIdentityChecks)
                    {
                        this.binding.EnsureOutgoingIdentity(token, this.identityCheckAuthenticator);
                    }
                }
                this.primaryToken = token;
                if (this.secondaryProvider == null)
                {
                    return this.OnGetSecondaryTokenComplete(null, true);
                }
                IAsyncResult result = this.secondaryProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), getSecondaryTokenCompleteCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                SecurityToken token2 = this.secondaryProvider.EndGetToken(result);
                return this.OnGetSecondaryTokenComplete(token2);
            }

            private bool OnGetSecondaryTokenComplete(SecurityToken token)
            {
                return this.OnGetSecondaryTokenComplete(token, false);
            }

            private bool OnGetSecondaryTokenComplete(SecurityToken token, bool secondaryCallSkipped)
            {
                if (!secondaryCallSkipped && (token == null))
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenProviderCannotGetTokensForTarget", new object[] { this.binding.Target })), this.message);
                }
                this.OnBothGetTokenCallsDone(ref this.message, this.primaryToken, token, this.timeoutHelper.RemainingTime());
                return true;
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = false;
                if (this.primaryProvider == null)
                {
                    return this.OnGetPrimaryTokenComplete(null);
                }
                IAsyncResult result = this.primaryProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), getPrimaryTokenCompleteCallback, this);
                if (result.CompletedSynchronously)
                {
                    SecurityToken token = this.primaryProvider.EndGetToken(result);
                    flag = this.OnGetPrimaryTokenComplete(token);
                }
                return flag;
            }

            protected void SetCorrelationToken(SecurityToken token)
            {
                this.newCorrelationState = new SecurityProtocolCorrelationState(token);
            }

            protected MessageSecurityProtocol Binding
            {
                get
                {
                    return this.binding;
                }
            }

            protected SecurityProtocolCorrelationState NewCorrelationState
            {
                get
                {
                    return this.newCorrelationState;
                }
            }

            protected SecurityProtocolCorrelationState OldCorrelationState
            {
                get
                {
                    return this.oldCorrelationState;
                }
            }
        }
    }
}

