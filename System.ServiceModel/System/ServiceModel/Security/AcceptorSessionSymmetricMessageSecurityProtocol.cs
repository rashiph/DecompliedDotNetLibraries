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
    using System.Xml;

    internal sealed class AcceptorSessionSymmetricMessageSecurityProtocol : MessageSecurityProtocol, IAcceptorSecuritySessionProtocol
    {
        private DerivedKeySecurityToken derivedEncryptionToken;
        private DerivedKeySecurityToken derivedSignatureToken;
        private SecurityToken outgoingSessionToken;
        private bool requireDerivedKeys;
        private bool returnCorrelationState;
        private UniqueId sessionId;
        private ReadOnlyCollection<SecurityTokenResolver> sessionResolverList;
        private SecurityStandardsManager sessionStandardsManager;
        private SecurityTokenAuthenticator sessionTokenAuthenticator;
        private SecurityTokenResolver sessionTokenResolver;
        private object thisLock;

        public AcceptorSessionSymmetricMessageSecurityProtocol(SessionSymmetricMessageSecurityProtocolFactory factory, EndpointAddress target) : base(factory, target, null)
        {
            this.thisLock = new object();
            if (factory.ActAsInitiator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProtocolMustBeRecipient", new object[] { base.GetType().ToString() })));
            }
            this.requireDerivedKeys = factory.SecurityTokenParameters.RequireDerivedKeys;
            if (this.requireDerivedKeys)
            {
                SecurityTokenSerializer securityTokenSerializer = this.Factory.StandardsManager.SecurityTokenSerializer;
                WSSecureConversation secureConversation = (securityTokenSerializer is WSSecurityTokenSerializer) ? ((WSSecurityTokenSerializer) securityTokenSerializer).SecureConversation : new WSSecurityTokenSerializer(this.Factory.MessageSecurityVersion.SecurityVersion).SecureConversation;
                this.sessionStandardsManager = new SecurityStandardsManager(factory.MessageSecurityVersion, new DerivedKeyCachingSecurityTokenSerializer(2, false, secureConversation, securityTokenSerializer));
            }
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityTokenParameters parameters;
            this.GetTokensForOutgoingMessages(out token, out token2, out parameters);
            this.SetUpDelayedSecurityExecution(ref message, token, token2, parameters, correlationState);
            return new CompletedAsyncResult<Message>(message, callback, state);
        }

        protected override void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            message = CompletedAsyncResult<Message>.End(result);
            newCorrelationState = null;
        }

        public SecurityToken GetOutgoingSessionToken()
        {
            lock (this.ThisLock)
            {
                return this.outgoingSessionToken;
            }
        }

        private void GetTokensForOutgoingMessages(out SecurityToken signingToken, out SecurityToken encryptionToken, out SecurityTokenParameters tokenParameters)
        {
            lock (this.ThisLock)
            {
                if (this.requireDerivedKeys)
                {
                    signingToken = this.derivedSignatureToken;
                    encryptionToken = this.derivedEncryptionToken;
                }
                else
                {
                    signingToken = encryptionToken = this.outgoingSessionToken;
                }
            }
            tokenParameters = this.Factory.GetTokenParameters();
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityTokenParameters parameters;
            this.GetTokensForOutgoingMessages(out token, out token2, out parameters);
            this.SetUpDelayedSecurityExecution(ref message, token, token2, parameters, correlationState);
            return null;
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
                    this.derivedSignatureToken = new DerivedKeySecurityToken(-1, 0, this.Factory.OutgoingAlgorithmSuite.GetSignatureKeyDerivationLength(token, this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.External), keyDerivationAlgorithm, System.ServiceModel.Security.SecurityUtils.GenerateId());
                    this.derivedEncryptionToken = new DerivedKeySecurityToken(-1, 0, this.Factory.OutgoingAlgorithmSuite.GetEncryptionKeyDerivationLength(token, this.sessionStandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.External), keyDerivationAlgorithm, System.ServiceModel.Security.SecurityUtils.GenerateId());
                }
            }
        }

        public void SetSessionTokenAuthenticator(UniqueId sessionId, SecurityTokenAuthenticator sessionTokenAuthenticator, SecurityTokenResolver sessionTokenResolver)
        {
            base.CommunicationObject.ThrowIfDisposedOrImmutable();
            lock (this.ThisLock)
            {
                this.sessionId = sessionId;
                this.sessionTokenAuthenticator = sessionTokenAuthenticator;
                this.sessionTokenResolver = sessionTokenResolver;
                List<SecurityTokenResolver> list = new List<SecurityTokenResolver>(1) {
                    this.sessionTokenResolver
                };
                this.sessionResolverList = new ReadOnlyCollection<SecurityTokenResolver>(list);
            }
        }

        private void SetUpDelayedSecurityExecution(ref Message message, SecurityToken signingToken, SecurityToken encryptionToken, SecurityTokenParameters tokenParameters, SecurityProtocolCorrelationState correlationState)
        {
            string actor = string.Empty;
            SendSecurityHeader header = base.ConfigureSendSecurityHeader(message, actor, null, correlationState);
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
            securityHeader.ConfigureSymmetricBindingServerReceiveHeader(this.sessionTokenAuthenticator, this.Factory.SecurityTokenParameters, list);
            securityHeader.ConfigureOutOfBandTokenResolver(base.MergeOutOfBandResolvers(list, this.sessionResolverList));
            securityHeader.EnforceDerivedKeyRequirement = message.Headers.Action != factory.StandardsManager.SecureConversationDriver.CloseAction.Value;
            base.ProcessSecurityHeader(securityHeader, ref message, null, timeout, correlationStates);
            SecurityToken signatureToken = securityHeader.SignatureToken;
            SecurityContextSecurityToken token2 = signatureToken as SecurityContextSecurityToken;
            if ((token2 == null) || (token2.ContextId != this.sessionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSessionTokenPresentInMessage")));
            }
            base.AttachRecipientSecurityProperty(message, signatureToken, false, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens, securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
            return base.GetCorrelationState(null, securityHeader);
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
    }
}

