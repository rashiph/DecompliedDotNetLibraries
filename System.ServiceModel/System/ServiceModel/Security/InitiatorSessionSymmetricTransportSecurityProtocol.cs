namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;

    internal sealed class InitiatorSessionSymmetricTransportSecurityProtocol : TransportSecurityProtocol, IInitiatorSecuritySessionProtocol
    {
        private DerivedKeySecurityToken derivedSignatureToken;
        private List<SecurityToken> incomingSessionTokens;
        private SecurityToken outgoingSessionToken;
        private bool requireDerivedKeys;
        private object thisLock;

        public InitiatorSessionSymmetricTransportSecurityProtocol(SessionSymmetricTransportSecurityProtocolFactory factory, EndpointAddress target, Uri via) : base(factory, target, via)
        {
            this.thisLock = new object();
            if (!factory.ActAsInitiator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ProtocolMustBeInitiator", new object[] { "InitiatorSessionSymmetricTransportSecurityProtocol" })));
            }
            this.requireDerivedKeys = factory.SecurityTokenParameters.RequireDerivedKeys;
        }

        protected override IAsyncResult BeginSecureOutgoingMessageAtInitiatorCore(Message message, string actor, TimeSpan timeout, AsyncCallback callback, object state)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityTokenParameters parameters;
            IList<SupportingTokenSpecification> list;
            this.GetTokensForOutgoingMessages(out token, out token2, out parameters);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!base.TryGetSupportingTokens(base.SecurityProtocolFactory, base.Target, base.Via, message, helper.RemainingTime(), false, out list))
            {
                return new SecureOutgoingMessageAsyncResult(actor, message, this, token, token2, parameters, helper.RemainingTime(), callback, state);
            }
            this.SetupDelayedSecurityExecution(actor, ref message, token, token2, parameters, list);
            return new CompletedAsyncResult<Message>(message, callback, state);
        }

        protected override Message EndSecureOutgoingMessageAtInitiatorCore(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<Message>)
            {
                return CompletedAsyncResult<Message>.End(result);
            }
            return SecureOutgoingMessageAsyncResult.End(result);
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

        private void GetTokensForOutgoingMessages(out SecurityToken signingToken, out SecurityToken sourceToken, out SecurityTokenParameters tokenParameters)
        {
            lock (this.ThisLock)
            {
                if (this.requireDerivedKeys)
                {
                    signingToken = this.derivedSignatureToken;
                    sourceToken = this.outgoingSessionToken;
                }
                else
                {
                    signingToken = this.outgoingSessionToken;
                    sourceToken = null;
                }
            }
            tokenParameters = this.Factory.GetTokenParameters();
        }

        protected override void SecureOutgoingMessageAtInitiator(ref Message message, string actor, TimeSpan timeout)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityTokenParameters parameters;
            IList<SupportingTokenSpecification> list;
            this.GetTokensForOutgoingMessages(out token, out token2, out parameters);
            base.TryGetSupportingTokens(base.SecurityProtocolFactory, base.Target, base.Via, message, timeout, true, out list);
            this.SetupDelayedSecurityExecution(actor, ref message, token, token2, parameters, list);
        }

        public void SetIdentityCheckAuthenticator(SecurityTokenAuthenticator authenticator)
        {
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
                    string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(this.Factory.MessageSecurityVersion.SecureConversationVersion);
                    this.derivedSignatureToken = new DerivedKeySecurityToken(-1, 0, this.Factory.OutgoingAlgorithmSuite.GetSignatureKeyDerivationLength(token, this.Factory.MessageSecurityVersion.SecureConversationVersion), null, 0x10, token, this.Factory.SecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal), keyDerivationAlgorithm, System.ServiceModel.Security.SecurityUtils.GenerateId());
                }
            }
        }

        internal void SetupDelayedSecurityExecution(string actor, ref Message message, SecurityToken signingToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters, IList<SupportingTokenSpecification> supportingTokens)
        {
            SendSecurityHeader securityHeader = base.CreateSendSecurityHeaderForTransportProtocol(message, actor, this.Factory);
            securityHeader.RequireMessageProtection = false;
            if (sourceToken != null)
            {
                securityHeader.AddPrerequisiteToken(sourceToken);
            }
            base.AddSupportingTokens(securityHeader, supportingTokens);
            securityHeader.AddEndorsingSupportingToken(signingToken, tokenParameters);
            message = securityHeader.SetupExecution();
        }

        private SessionSymmetricTransportSecurityProtocolFactory Factory
        {
            get
            {
                return (SessionSymmetricTransportSecurityProtocolFactory) base.SecurityProtocolFactory;
            }
        }

        public bool ReturnCorrelationState
        {
            get
            {
                return false;
            }
            set
            {
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
            private string actor;
            private InitiatorSessionSymmetricTransportSecurityProtocol binding;
            private Message message;
            private SecurityToken signingToken;
            private SecurityToken sourceToken;
            private SecurityTokenParameters tokenParameters;

            public SecureOutgoingMessageAsyncResult(string actor, Message message, InitiatorSessionSymmetricTransportSecurityProtocol binding, SecurityToken signingToken, SecurityToken sourceToken, SecurityTokenParameters tokenParameters, TimeSpan timeout, AsyncCallback callback, object state) : base(message, binding, timeout, callback, state)
            {
                this.actor = actor;
                this.message = message;
                this.binding = binding;
                this.signingToken = signingToken;
                this.sourceToken = sourceToken;
                this.tokenParameters = tokenParameters;
                base.Start();
            }

            internal static Message End(IAsyncResult result)
            {
                return AsyncResult.End<InitiatorSessionSymmetricTransportSecurityProtocol.SecureOutgoingMessageAsyncResult>(result).message;
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.binding.SetupDelayedSecurityExecution(this.actor, ref this.message, this.signingToken, this.sourceToken, this.tokenParameters, base.SupportingTokens);
                return true;
            }
        }
    }
}

