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
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    internal sealed class SymmetricSecurityProtocol : MessageSecurityProtocol
    {
        private SecurityTokenProvider initiatorAsymmetricTokenProvider;
        private SecurityTokenProvider initiatorSymmetricTokenProvider;
        private SecurityTokenAuthenticator initiatorTokenAuthenticator;

        public SymmetricSecurityProtocol(SymmetricSecurityProtocolFactory factory, EndpointAddress target, Uri via) : base(factory, target, via)
        {
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken token;
            SecurityTokenParameters parameters;
            IList<SupportingTokenSpecification> list;
            SecurityToken token2;
            SecurityProtocolCorrelationState state2;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, false, helper.RemainingTime(), out token, out parameters, out token2, out list, out state2))
            {
                this.SetUpDelayedSecurityExecution(ref message, token2, token, parameters, list, base.GetSignatureConfirmationCorrelationState(correlationState, state2));
                return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, state2, callback, state);
            }
            if (!this.Factory.ActAsInitiator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProtocolMustBeInitiator", new object[] { base.GetType().ToString() })));
            }
            return new SecureOutgoingMessageAsyncResult(message, this, this.GetTokenProvider(), this.Factory.ApplyConfidentiality, this.initiatorTokenAuthenticator, correlationState, helper.RemainingTime(), callback, state);
        }

        private InitiatorServiceModelSecurityTokenRequirement CreateInitiatorTokenRequirement()
        {
            InitiatorServiceModelSecurityTokenRequirement requirement = base.CreateInitiatorSecurityTokenRequirement();
            this.Factory.SecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = this.Factory.SecurityTokenParameters.HasAsymmetricKey ? SecurityKeyUsage.Exchange : SecurityKeyUsage.Signature;
            requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
            if (this.Factory.SecurityTokenParameters.HasAsymmetricKey)
            {
                requirement.IsOutOfBandToken = true;
            }
            return requirement;
        }

        private WrappedKeySecurityToken CreateWrappedKeyToken(SecurityToken wrappingToken, SecurityTokenParameters wrappingTokenParameters, SecurityTokenReferenceStyle wrappingTokenReferenceStyle)
        {
            int keyLength = Math.Max(0x80, this.Factory.OutgoingAlgorithmSuite.DefaultSymmetricKeyLength);
            CryptoHelper.ValidateSymmetricKeyLength(keyLength, this.Factory.OutgoingAlgorithmSuite);
            byte[] buffer = new byte[keyLength / 8];
            CryptoHelper.FillRandomBytes(buffer);
            string id = System.ServiceModel.Security.SecurityUtils.GenerateId();
            string defaultAsymmetricKeyWrapAlgorithm = this.Factory.OutgoingAlgorithmSuite.DefaultAsymmetricKeyWrapAlgorithm;
            SecurityKeyIdentifierClause clause = wrappingTokenParameters.CreateKeyIdentifierClause(wrappingToken, wrappingTokenReferenceStyle);
            SecurityKeyIdentifier wrappingTokenReference = new SecurityKeyIdentifier();
            wrappingTokenReference.Add(clause);
            return new WrappedKeySecurityToken(id, buffer, defaultAsymmetricKeyWrapAlgorithm, wrappingToken, wrappingTokenReference);
        }

        protected override void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result is CompletedAsyncResult<Message, SecurityProtocolCorrelationState>)
            {
                message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
            }
            else
            {
                message = MessageSecurityProtocol.GetOneTokenAndSetUpSecurityAsyncResult.End(result, out newCorrelationState);
            }
        }

        private void EnsureWrappedToken(SecurityToken token, Message message)
        {
            if (!(token is WrappedKeySecurityToken))
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("IncomingSigningTokenMustBeAnEncryptedKey")), message);
            }
        }

        private SecurityToken GetCorrelationToken(SecurityProtocolCorrelationState[] correlationStates, out SecurityTokenParameters correlationTokenParameters)
        {
            SecurityToken correlationToken = base.GetCorrelationToken(correlationStates);
            correlationTokenParameters = this.Factory.GetProtectionTokenParameters();
            return correlationToken;
        }

        private SecurityToken GetInitiatorToken(SecurityToken providerToken, Message message, TimeSpan timeout, out SecurityTokenParameters tokenParameters, out SecurityToken prerequisiteWrappingToken)
        {
            SecurityToken token;
            tokenParameters = null;
            prerequisiteWrappingToken = null;
            if (this.Factory.SecurityTokenParameters.HasAsymmetricKey)
            {
                SecurityToken wrappingToken = providerToken;
                bool flag = SendSecurityHeader.ShouldSerializeToken(this.Factory.SecurityTokenParameters, MessageDirection.Input);
                if (flag)
                {
                    prerequisiteWrappingToken = wrappingToken;
                }
                token = this.CreateWrappedKeyToken(wrappingToken, this.Factory.SecurityTokenParameters, flag ? SecurityTokenReferenceStyle.Internal : SecurityTokenReferenceStyle.External);
            }
            else
            {
                token = providerToken;
            }
            tokenParameters = this.Factory.GetProtectionTokenParameters();
            return token;
        }

        private SecurityTokenProvider GetTokenProvider()
        {
            if (!this.Factory.ActAsInitiator)
            {
                return this.Factory.RecipientAsymmetricTokenProvider;
            }
            return (this.initiatorSymmetricTokenProvider ?? this.initiatorAsymmetricTokenProvider);
        }

        public override void OnAbort()
        {
            if (this.Factory.ActAsInitiator)
            {
                SecurityTokenProvider tokenProvider = this.initiatorSymmetricTokenProvider ?? this.initiatorAsymmetricTokenProvider;
                if (tokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(tokenProvider);
                }
                if (this.initiatorTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.initiatorTokenAuthenticator);
                }
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.Factory.ActAsInitiator)
            {
                SecurityTokenProvider tokenProvider = this.initiatorSymmetricTokenProvider ?? this.initiatorAsymmetricTokenProvider;
                if (tokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(tokenProvider, helper.RemainingTime());
                }
                if (this.initiatorTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.initiatorTokenAuthenticator, helper.RemainingTime());
                }
            }
            base.OnClose(helper.RemainingTime());
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            if (this.Factory.ActAsInitiator)
            {
                SecurityTokenResolver resolver;
                InitiatorServiceModelSecurityTokenRequirement tokenRequirement = this.CreateInitiatorTokenRequirement();
                SecurityTokenProvider tokenProvider = this.Factory.SecurityTokenManager.CreateSecurityTokenProvider(tokenRequirement);
                System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(tokenProvider, helper.RemainingTime());
                if (this.Factory.SecurityTokenParameters.HasAsymmetricKey)
                {
                    this.initiatorAsymmetricTokenProvider = tokenProvider;
                }
                else
                {
                    this.initiatorSymmetricTokenProvider = tokenProvider;
                }
                InitiatorServiceModelSecurityTokenRequirement requirement2 = this.CreateInitiatorTokenRequirement();
                this.initiatorTokenAuthenticator = this.Factory.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement2, out resolver);
                System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(this.initiatorTokenAuthenticator, helper.RemainingTime());
            }
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken token;
            SecurityTokenParameters parameters;
            IList<SupportingTokenSpecification> list;
            SecurityToken token2;
            SecurityProtocolCorrelationState state;
            this.TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, true, timeout, out token, out parameters, out token2, out list, out state);
            this.SetUpDelayedSecurityExecution(ref message, token2, token, parameters, list, base.GetSignatureConfirmationCorrelationState(correlationState, state));
            return state;
        }

        private void SetUpDelayedSecurityExecution(ref Message message, SecurityToken prerequisiteToken, SecurityToken primaryToken, SecurityTokenParameters primaryTokenParameters, IList<SupportingTokenSpecification> supportingTokens, SecurityProtocolCorrelationState correlationState)
        {
            string actor = string.Empty;
            SendSecurityHeader header = base.ConfigureSendSecurityHeader(message, actor, supportingTokens, correlationState);
            if (prerequisiteToken != null)
            {
                header.AddPrerequisiteToken(prerequisiteToken);
            }
            if (this.Factory.ApplyIntegrity || header.HasSignedTokens)
            {
                if (!this.Factory.ApplyIntegrity)
                {
                    header.SignatureParts = MessagePartSpecification.NoParts;
                }
                header.SetSigningToken(primaryToken, primaryTokenParameters);
            }
            if (this.Factory.ApplyConfidentiality || header.HasEncryptedTokens)
            {
                if (!this.Factory.ApplyConfidentiality)
                {
                    header.EncryptionParts = MessagePartSpecification.NoParts;
                }
                header.SetEncryptionToken(primaryToken, primaryTokenParameters);
            }
            message = header.SetupExecution();
        }

        private bool TryGetTokenSynchronouslyForOutgoingSecurity(Message message, SecurityProtocolCorrelationState correlationState, bool isBlockingCall, TimeSpan timeout, out SecurityToken token, out SecurityTokenParameters tokenParameters, out SecurityToken prerequisiteWrappingToken, out IList<SupportingTokenSpecification> supportingTokens, out SecurityProtocolCorrelationState newCorrelationState)
        {
            SymmetricSecurityProtocolFactory factory = this.Factory;
            supportingTokens = null;
            prerequisiteWrappingToken = null;
            token = null;
            tokenParameters = null;
            newCorrelationState = null;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (factory.ApplyIntegrity || factory.ApplyConfidentiality)
            {
                if (factory.ActAsInitiator)
                {
                    if (!isBlockingCall || !base.TryGetSupportingTokens(factory, base.Target, base.Via, message, helper.RemainingTime(), isBlockingCall, out supportingTokens))
                    {
                        return false;
                    }
                    SecurityTokenProvider tokenProvider = this.GetTokenProvider();
                    SecurityToken providerToken = base.GetTokenAndEnsureOutgoingIdentity(tokenProvider, factory.ApplyConfidentiality, helper.RemainingTime(), this.initiatorTokenAuthenticator);
                    token = this.GetInitiatorToken(providerToken, message, helper.RemainingTime(), out tokenParameters, out prerequisiteWrappingToken);
                    newCorrelationState = base.GetCorrelationState(token);
                }
                else
                {
                    token = base.GetCorrelationToken(correlationState);
                    tokenParameters = this.Factory.GetProtectionTokenParameters();
                }
            }
            return true;
        }

        protected override SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            IList<SupportingTokenAuthenticatorSpecification> list;
            SymmetricSecurityProtocolFactory factory = this.Factory;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            ReceiveSecurityHeader securityHeader = base.ConfigureReceiveSecurityHeader(message, string.Empty, correlationStates, out list);
            SecurityToken requiredSigningToken = null;
            if (this.Factory.ActAsInitiator)
            {
                SecurityTokenParameters parameters;
                SecurityToken correlationToken = this.GetCorrelationToken(correlationStates, out parameters);
                securityHeader.ConfigureSymmetricBindingClientReceiveHeader(correlationToken, parameters);
                requiredSigningToken = correlationToken;
            }
            else
            {
                if (factory.RecipientSymmetricTokenAuthenticator != null)
                {
                    securityHeader.ConfigureSymmetricBindingServerReceiveHeader(this.Factory.RecipientSymmetricTokenAuthenticator, this.Factory.SecurityTokenParameters, list);
                }
                else
                {
                    securityHeader.ConfigureSymmetricBindingServerReceiveHeader(this.Factory.RecipientAsymmetricTokenProvider.GetToken(helper.RemainingTime()), this.Factory.SecurityTokenParameters, list);
                    securityHeader.WrappedKeySecurityTokenAuthenticator = this.Factory.WrappedKeySecurityTokenAuthenticator;
                }
                securityHeader.ConfigureOutOfBandTokenResolver(base.MergeOutOfBandResolvers(list, this.Factory.RecipientOutOfBandTokenResolverList));
            }
            base.ProcessSecurityHeader(securityHeader, ref message, requiredSigningToken, helper.RemainingTime(), correlationStates);
            SecurityToken signatureToken = securityHeader.SignatureToken;
            if (factory.RequireIntegrity)
            {
                if (factory.SecurityTokenParameters.HasAsymmetricKey)
                {
                    this.EnsureWrappedToken(signatureToken, message);
                }
                else
                {
                    MessageSecurityProtocol.EnsureNonWrappedToken(signatureToken, message);
                }
                if (factory.ActAsInitiator)
                {
                    if (!factory.SecurityTokenParameters.HasAsymmetricKey)
                    {
                        ReadOnlyCollection<IAuthorizationPolicy> protectionTokenPolicies = this.initiatorTokenAuthenticator.ValidateToken(signatureToken);
                        base.DoIdentityCheckAndAttachInitiatorSecurityProperty(message, signatureToken, protectionTokenPolicies);
                    }
                    else
                    {
                        SecurityToken wrappingToken = (signatureToken as WrappedKeySecurityToken).WrappingToken;
                        ReadOnlyCollection<IAuthorizationPolicy> onlys2 = this.initiatorTokenAuthenticator.ValidateToken(wrappingToken);
                        base.DoIdentityCheckAndAttachInitiatorSecurityProperty(message, signatureToken, onlys2);
                    }
                }
                else
                {
                    base.AttachRecipientSecurityProperty(message, signatureToken, this.Factory.SecurityTokenParameters.HasAsymmetricKey, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens, securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
                }
            }
            return base.GetCorrelationState(signatureToken, securityHeader);
        }

        private SymmetricSecurityProtocolFactory Factory
        {
            get
            {
                return (SymmetricSecurityProtocolFactory) base.MessageSecurityProtocolFactory;
            }
        }

        public SecurityTokenProvider InitiatorAsymmetricTokenProvider
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorAsymmetricTokenProvider;
            }
        }

        public SecurityTokenProvider InitiatorSymmetricTokenProvider
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorSymmetricTokenProvider;
            }
        }

        public SecurityTokenAuthenticator InitiatorTokenAuthenticator
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorTokenAuthenticator;
            }
        }

        private sealed class SecureOutgoingMessageAsyncResult : MessageSecurityProtocol.GetOneTokenAndSetUpSecurityAsyncResult
        {
            private SymmetricSecurityProtocol symmetricBinding;

            public SecureOutgoingMessageAsyncResult(Message m, SymmetricSecurityProtocol binding, SecurityTokenProvider provider, bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator, SecurityProtocolCorrelationState correlationState, TimeSpan timeout, AsyncCallback callback, object state) : base(m, binding, provider, doIdentityChecks, identityCheckAuthenticator, correlationState, timeout, callback, state)
            {
                this.symmetricBinding = binding;
                base.Start();
            }

            protected override void OnGetTokenDone(ref Message message, SecurityToken providerToken, TimeSpan timeout)
            {
                SecurityTokenParameters parameters;
                SecurityToken token;
                SecurityToken token2 = this.symmetricBinding.GetInitiatorToken(providerToken, message, timeout, out parameters, out token);
                base.SetCorrelationToken(token2);
                this.symmetricBinding.SetUpDelayedSecurityExecution(ref message, token, token2, parameters, base.SupportingTokens, base.Binding.GetSignatureConfirmationCorrelationState(base.OldCorrelationState, base.NewCorrelationState));
            }
        }
    }
}

