namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;

    internal sealed class InitiatorSessionSymmetricMessageSecurityProtocol : MessageSecurityProtocol, IInitiatorSecuritySessionProtocol
    {
        private DerivedKeySecurityToken derivedEncryptionToken;
        private DerivedKeySecurityToken derivedSignatureToken;
        private List<SecurityToken> incomingSessionTokens;
        private SecurityToken outgoingSessionToken;
        private bool requireDerivedKeys;
        private bool returnCorrelationState;
        private SecurityStandardsManager sessionStandardsManager;
        private SecurityTokenAuthenticator sessionTokenAuthenticator;
        private object thisLock;

        public InitiatorSessionSymmetricMessageSecurityProtocol(SessionSymmetricMessageSecurityProtocolFactory factory, EndpointAddress target, Uri via) : base(factory, target, via)
        {
            this.thisLock = new object();
            if (!factory.ActAsInitiator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ProtocolMustBeInitiator", new object[] { "InitiatorSessionSymmetricMessageSecurityProtocol" })));
            }
            this.requireDerivedKeys = factory.SecurityTokenParameters.RequireDerivedKeys;
            if (this.requireDerivedKeys)
            {
                SecurityTokenSerializer securityTokenSerializer = this.Factory.StandardsManager.SecurityTokenSerializer;
                WSSecureConversation secureConversation = (securityTokenSerializer is WSSecurityTokenSerializer) ? ((WSSecurityTokenSerializer) securityTokenSerializer).SecureConversation : new WSSecurityTokenSerializer(this.Factory.MessageSecurityVersion.SecurityVersion).SecureConversation;
                this.sessionStandardsManager = new SecurityStandardsManager(factory.MessageSecurityVersion, new DerivedKeyCachingSecurityTokenSerializer(2, true, secureConversation, securityTokenSerializer));
            }
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityToken token3;
            SecurityTokenParameters parameters;
            IList<SupportingTokenSpecification> list;
            this.GetTokensForOutgoingMessages(out token, out token2, out token3, out parameters);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (base.TryGetSupportingTokens(this.Factory, base.Target, base.Via, message, helper.RemainingTime(), false, out list))
            {
                SecurityProtocolCorrelationState state2 = this.CreateCorrelationStateIfRequired();
                this.SetUpDelayedSecurityExecution(ref message, token, token2, token3, parameters, list, state2);
                return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, state2, callback, state);
            }
            return new SecureOutgoingMessageAsyncResult(message, this, token, token2, token3, parameters, helper.RemainingTime(), callback, state);
        }

        internal SecurityProtocolCorrelationState CreateCorrelationStateIfRequired()
        {
            if (this.ReturnCorrelationState)
            {
                return new SecurityProtocolCorrelationState(null);
            }
            return null;
        }

        protected override void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result is CompletedAsyncResult<Message, SecurityProtocolCorrelationState>)
            {
                message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
            }
            else
            {
                message = SecureOutgoingMessageAsyncResult.End(result, out newCorrelationState);
            }
        }

        public List<SecurityToken> GetIncomingSessionTokens()
        {
            lock (this.ThisLock)
            {
                return this.incomingSessionTokens;
            }
        }

        public SecurityToken GetOutgoingSessionToken()
        {
            lock (this.ThisLock)
            {
                return this.outgoingSessionToken;
            }
        }

        private void GetTokensForOutgoingMessages(out SecurityToken signingToken, out SecurityToken encryptionToken, out SecurityToken sourceToken, out SecurityTokenParameters tokenParameters)
        {
            lock (this.ThisLock)
            {
                if (this.requireDerivedKeys)
                {
                    signingToken = this.derivedSignatureToken;
                    encryptionToken = this.derivedEncryptionToken;
                    sourceToken = this.outgoingSessionToken;
                }
                else
                {
                    signingToken = encryptionToken = this.outgoingSessionToken;
                    sourceToken = null;
                }
            }
            if (this.Factory.ApplyConfidentiality)
            {
                base.EnsureOutgoingIdentity(sourceToken ?? encryptionToken, this.sessionTokenAuthenticator);
            }
            tokenParameters = this.Factory.GetTokenParameters();
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityToken token3;
            SecurityTokenParameters parameters;
            IList<SupportingTokenSpecification> list;
            this.GetTokensForOutgoingMessages(out token, out token2, out token3, out parameters);
            SecurityProtocolCorrelationState state = this.CreateCorrelationStateIfRequired();
            base.TryGetSupportingTokens(base.SecurityProtocolFactory, base.Target, base.Via, message, timeout, true, out list);
            this.SetUpDelayedSecurityExecution(ref message, token, token2, token3, parameters, list, state);
            return state;
        }

        public void SetIdentityCheckAuthenticator(SecurityTokenAuthenticator authenticator)
        {
            this.sessionTokenAuthenticator = authenticator;
        }

        public void SetIncomingSessionTokens(List<SecurityToken> tokens)
        {
            if (tokens == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokens");
            }
            lock (this.ThisLock)
            {
                this.incomingSessionTokens = new List<SecurityToken>(tokens);
            }
        }

        public void SetOutgoingSessionToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            lock (this.ThisLock)
            {
                this.outgoingSessionToken = token;
                if (this.requireDerivedKeys)
                {
                    string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion);
                    this.derivedSignatureToken = new DerivedKeySecurityToken(-1, 0, this.Factory.OutgoingAlgorithmSuite.GetSignatureKeyDerivationLength(token, this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal), keyDerivationAlgorithm, System.ServiceModel.Security.SecurityUtils.GenerateId());
                    this.derivedEncryptionToken = new DerivedKeySecurityToken(-1, 0, this.Factory.OutgoingAlgorithmSuite.GetEncryptionKeyDerivationLength(token, this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal), keyDerivationAlgorithm, System.ServiceModel.Security.SecurityUtils.GenerateId());
                }
            }
        }

        internal void SetUpDelayedSecurityExecution(ref Message message, SecurityToken signingToken, SecurityToken encryptionToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters, IList<SupportingTokenSpecification> supportingTokens, SecurityProtocolCorrelationState correlationState)
        {
            SessionSymmetricMessageSecurityProtocolFactory factory = this.Factory;
            string actor = string.Empty;
            SendSecurityHeader header = base.ConfigureSendSecurityHeader(message, actor, supportingTokens, correlationState);
            if (sourceToken != null)
            {
                header.AddPrerequisiteToken(sourceToken);
            }
            if (this.Factory.ApplyIntegrity)
            {
                header.SetSigningToken(signingToken, tokenParameters);
            }
            if (this.Factory.ApplyConfidentiality)
            {
                header.SetEncryptionToken(encryptionToken, tokenParameters);
            }
            message = header.SetupExecution();
        }

        protected override SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            IList<SupportingTokenAuthenticatorSpecification> list;
            SessionSymmetricMessageSecurityProtocolFactory factory = this.Factory;
            ReceiveSecurityHeader securityHeader = base.ConfigureReceiveSecurityHeader(message, string.Empty, correlationStates, this.requireDerivedKeys ? this.sessionStandardsManager : null, out list);
            List<SecurityToken> incomingSessionTokens = this.GetIncomingSessionTokens();
            securityHeader.ConfigureSymmetricBindingClientReceiveHeader(incomingSessionTokens, this.Factory.SecurityTokenParameters);
            securityHeader.EnforceDerivedKeyRequirement = message.Headers.Action != factory.StandardsManager.SecureConversationDriver.CloseResponseAction.Value;
            base.ProcessSecurityHeader(securityHeader, ref message, null, timeout, correlationStates);
            SecurityToken signatureToken = securityHeader.SignatureToken;
            bool flag = false;
            for (int i = 0; i < incomingSessionTokens.Count; i++)
            {
                if (object.ReferenceEquals(signatureToken, incomingSessionTokens[i]))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSessionTokenPresentInMessage")));
            }
            if (factory.RequireIntegrity)
            {
                ReadOnlyCollection<IAuthorizationPolicy> protectionTokenPolicies = this.sessionTokenAuthenticator.ValidateToken(signatureToken);
                base.DoIdentityCheckAndAttachInitiatorSecurityProperty(message, signatureToken, protectionTokenPolicies);
            }
            return null;
        }

        private SessionSymmetricMessageSecurityProtocolFactory Factory
        {
            get
            {
                return (SessionSymmetricMessageSecurityProtocolFactory) base.MessageSecurityProtocolFactory;
            }
        }

        protected override bool PerformIncomingAndOutgoingMessageExpectationChecks
        {
            get
            {
                return false;
            }
        }

        public bool ReturnCorrelationState
        {
            get
            {
                return this.returnCorrelationState;
            }
            set
            {
                this.returnCorrelationState = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private sealed class SecureOutgoingMessageAsyncResult : SecurityProtocol.GetSupportingTokensAsyncResult
        {
            private InitiatorSessionSymmetricMessageSecurityProtocol binding;
            private SecurityToken encryptionToken;
            private Message message;
            private SecurityProtocolCorrelationState newCorrelationState;
            private SecurityToken signingToken;
            private SecurityToken sourceToken;
            private SecurityTokenParameters tokenParameters;

            public SecureOutgoingMessageAsyncResult(Message message, InitiatorSessionSymmetricMessageSecurityProtocol binding, SecurityToken signingToken, SecurityToken encryptionToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters, TimeSpan timeout, AsyncCallback callback, object state) : base(message, binding, timeout, callback, state)
            {
                this.message = message;
                this.binding = binding;
                this.signingToken = signingToken;
                this.encryptionToken = encryptionToken;
                this.sourceToken = sourceToken;
                this.tokenParameters = tokenParameters;
                base.Start();
            }

            internal static Message End(IAsyncResult result, out SecurityProtocolCorrelationState newCorrelationState)
            {
                InitiatorSessionSymmetricMessageSecurityProtocol.SecureOutgoingMessageAsyncResult result2 = AsyncResult.End<InitiatorSessionSymmetricMessageSecurityProtocol.SecureOutgoingMessageAsyncResult>(result);
                newCorrelationState = result2.newCorrelationState;
                return result2.message;
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.newCorrelationState = this.binding.CreateCorrelationStateIfRequired();
                this.binding.SetUpDelayedSecurityExecution(ref this.message, this.signingToken, this.encryptionToken, this.sourceToken, this.tokenParameters, base.SupportingTokens, this.newCorrelationState);
                return true;
            }
        }
    }
}

