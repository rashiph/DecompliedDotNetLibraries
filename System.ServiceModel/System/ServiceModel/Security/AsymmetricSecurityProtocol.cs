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
    using System.ServiceModel.Security.Tokens;

    internal sealed class AsymmetricSecurityProtocol : MessageSecurityProtocol
    {
        private SecurityTokenAuthenticator initiatorAsymmetricTokenAuthenticator;
        private SecurityTokenProvider initiatorAsymmetricTokenProvider;
        private SecurityTokenProvider initiatorCryptoTokenProvider;

        public AsymmetricSecurityProtocol(AsymmetricSecurityProtocolFactory factory, EndpointAddress target, Uri via) : base(factory, target, via)
        {
        }

        private void AttachRecipientSecurityProperty(Message message, SecurityToken initiatorToken, SecurityToken recipientToken, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens, IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            SecurityMessageProperty orCreate = SecurityMessageProperty.GetOrCreate(message);
            orCreate.InitiatorToken = (initiatorToken != null) ? new SecurityTokenSpecification(initiatorToken, tokenPoliciesMapping[initiatorToken]) : null;
            orCreate.RecipientToken = (recipientToken != null) ? new SecurityTokenSpecification(recipientToken, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance) : null;
            base.AddSupportingTokenSpecification(orCreate, basicTokens, endorsingTokens, signedEndorsingTokens, signedTokens, tokenPoliciesMapping);
            orCreate.ServiceSecurityContext = new ServiceSecurityContext(orCreate.GetInitiatorTokenAuthorizationPolicies());
        }

        protected override IAsyncResult BeginSecureOutgoingMessageCore(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityProtocolCorrelationState state2;
            IList<SupportingTokenSpecification> list;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, false, helper.RemainingTime(), out token, out token2, out list, out state2))
            {
                this.SetUpDelayedSecurityExecution(ref message, token, token2, list, base.GetSignatureConfirmationCorrelationState(correlationState, state2));
                return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, state2, callback, state);
            }
            if (!this.Factory.ActAsInitiator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SendingOutgoingmessageOnRecipient")));
            }
            AsymmetricSecurityProtocolFactory factory = this.Factory;
            SecurityTokenProvider primaryProvider = factory.ApplyConfidentiality ? this.initiatorAsymmetricTokenProvider : null;
            return new SecureOutgoingMessageAsyncResult(message, this, primaryProvider, factory.ApplyIntegrity ? this.initiatorCryptoTokenProvider : null, factory.ApplyConfidentiality, this.initiatorAsymmetricTokenAuthenticator, correlationState, helper.RemainingTime(), callback, state);
        }

        private void DoIdentityCheckAndAttachInitiatorSecurityProperty(Message message, SecurityToken initiatorToken, SecurityToken recipientToken, ReadOnlyCollection<IAuthorizationPolicy> recipientTokenPolicies)
        {
            AuthorizationContext authorizationContext = base.EnsureIncomingIdentity(message, recipientToken, recipientTokenPolicies);
            SecurityMessageProperty orCreate = SecurityMessageProperty.GetOrCreate(message);
            orCreate.InitiatorToken = (initiatorToken != null) ? new SecurityTokenSpecification(initiatorToken, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance) : null;
            orCreate.RecipientToken = new SecurityTokenSpecification(recipientToken, recipientTokenPolicies);
            orCreate.ServiceSecurityContext = new ServiceSecurityContext(authorizationContext, recipientTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
        }

        protected override void EndSecureOutgoingMessageCore(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result is CompletedAsyncResult<Message, SecurityProtocolCorrelationState>)
            {
                message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
            }
            else
            {
                message = MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult.End(result, out newCorrelationState);
            }
        }

        public override void OnAbort()
        {
            if (this.Factory.ActAsInitiator)
            {
                if (this.initiatorCryptoTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.initiatorCryptoTokenProvider);
                }
                if (this.initiatorAsymmetricTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.initiatorAsymmetricTokenProvider);
                }
                if (this.initiatorAsymmetricTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.initiatorAsymmetricTokenAuthenticator);
                }
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.Factory.ActAsInitiator)
            {
                if (this.initiatorCryptoTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.initiatorCryptoTokenProvider, helper.RemainingTime());
                }
                if (this.initiatorAsymmetricTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.initiatorAsymmetricTokenProvider, helper.RemainingTime());
                }
                if (this.initiatorAsymmetricTokenAuthenticator != null)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.initiatorAsymmetricTokenAuthenticator, helper.RemainingTime());
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
                if (this.Factory.ApplyIntegrity)
                {
                    InitiatorServiceModelSecurityTokenRequirement requirement = base.CreateInitiatorSecurityTokenRequirement();
                    this.Factory.CryptoTokenParameters.InitializeSecurityTokenRequirement(requirement);
                    requirement.KeyUsage = SecurityKeyUsage.Signature;
                    requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
                    this.initiatorCryptoTokenProvider = this.Factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
                    System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.initiatorCryptoTokenProvider, helper.RemainingTime());
                }
                if (this.Factory.RequireIntegrity || this.Factory.ApplyConfidentiality)
                {
                    SecurityTokenResolver resolver;
                    InitiatorServiceModelSecurityTokenRequirement requirement2 = base.CreateInitiatorSecurityTokenRequirement();
                    this.Factory.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(requirement2);
                    requirement2.KeyUsage = SecurityKeyUsage.Exchange;
                    requirement2.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = this.Factory.ApplyConfidentiality ? MessageDirection.Output : MessageDirection.Input;
                    this.initiatorAsymmetricTokenProvider = this.Factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement2);
                    System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.initiatorAsymmetricTokenProvider, helper.RemainingTime());
                    InitiatorServiceModelSecurityTokenRequirement requirement3 = base.CreateInitiatorSecurityTokenRequirement();
                    this.Factory.AsymmetricTokenParameters.InitializeSecurityTokenRequirement(requirement3);
                    requirement3.IsOutOfBandToken = !this.Factory.AllowSerializedSigningTokenOnReply;
                    requirement3.KeyUsage = SecurityKeyUsage.Exchange;
                    requirement3.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = this.Factory.ApplyConfidentiality ? MessageDirection.Output : MessageDirection.Input;
                    this.initiatorAsymmetricTokenAuthenticator = this.Factory.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement3, out resolver);
                    System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(this.initiatorAsymmetricTokenAuthenticator, helper.RemainingTime());
                }
            }
        }

        protected override SecurityProtocolCorrelationState SecureOutgoingMessageCore(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            SecurityToken token;
            SecurityToken token2;
            SecurityProtocolCorrelationState state;
            IList<SupportingTokenSpecification> list;
            this.TryGetTokenSynchronouslyForOutgoingSecurity(message, correlationState, true, timeout, out token, out token2, out list, out state);
            this.SetUpDelayedSecurityExecution(ref message, token, token2, list, base.GetSignatureConfirmationCorrelationState(correlationState, state));
            return state;
        }

        private void SetUpDelayedSecurityExecution(ref Message message, SecurityToken encryptingToken, SecurityToken signingToken, IList<SupportingTokenSpecification> supportingTokens, SecurityProtocolCorrelationState correlationState)
        {
            AsymmetricSecurityProtocolFactory factory = this.Factory;
            string actor = string.Empty;
            SendSecurityHeader header = base.ConfigureSendSecurityHeader(message, actor, supportingTokens, correlationState);
            SecurityTokenParameters tokenParameters = this.Factory.ActAsInitiator ? this.Factory.CryptoTokenParameters : this.Factory.AsymmetricTokenParameters;
            SecurityTokenParameters parameters2 = this.Factory.ActAsInitiator ? this.Factory.AsymmetricTokenParameters : this.Factory.CryptoTokenParameters;
            if (this.Factory.ApplyIntegrity || header.HasSignedTokens)
            {
                if (!this.Factory.ApplyIntegrity)
                {
                    header.SignatureParts = MessagePartSpecification.NoParts;
                }
                header.SetSigningToken(signingToken, tokenParameters);
            }
            if (this.Factory.ApplyConfidentiality || header.HasEncryptedTokens)
            {
                if (!this.Factory.ApplyConfidentiality)
                {
                    header.EncryptionParts = MessagePartSpecification.NoParts;
                }
                header.SetEncryptionToken(encryptingToken, parameters2);
            }
            message = header.SetupExecution();
        }

        private bool TryGetTokenSynchronouslyForOutgoingSecurity(Message message, SecurityProtocolCorrelationState correlationState, bool isBlockingCall, TimeSpan timeout, out SecurityToken encryptingToken, out SecurityToken signingToken, out IList<SupportingTokenSpecification> supportingTokens, out SecurityProtocolCorrelationState newCorrelationState)
        {
            AsymmetricSecurityProtocolFactory factory = this.Factory;
            encryptingToken = null;
            signingToken = null;
            newCorrelationState = null;
            supportingTokens = null;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (factory.ActAsInitiator)
            {
                if (!isBlockingCall || !base.TryGetSupportingTokens(this.Factory, base.Target, base.Via, message, helper.RemainingTime(), isBlockingCall, out supportingTokens))
                {
                    return false;
                }
                if (factory.ApplyConfidentiality)
                {
                    encryptingToken = base.GetTokenAndEnsureOutgoingIdentity(this.initiatorAsymmetricTokenProvider, true, helper.RemainingTime(), this.initiatorAsymmetricTokenAuthenticator);
                }
                if (factory.ApplyIntegrity)
                {
                    signingToken = SecurityProtocol.GetToken(this.initiatorCryptoTokenProvider, base.Target, helper.RemainingTime());
                    newCorrelationState = base.GetCorrelationState(signingToken);
                }
            }
            else
            {
                if (factory.ApplyConfidentiality)
                {
                    encryptingToken = base.GetCorrelationToken(correlationState);
                }
                if (factory.ApplyIntegrity)
                {
                    signingToken = SecurityProtocol.GetToken(factory.RecipientAsymmetricTokenProvider, null, helper.RemainingTime());
                }
            }
            return true;
        }

        protected override SecurityProtocolCorrelationState VerifyIncomingMessageCore(ref Message message, string actor, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates)
        {
            IList<SupportingTokenAuthenticatorSpecification> list;
            AsymmetricSecurityProtocolFactory factory = this.Factory;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            ReceiveSecurityHeader securityHeader = base.ConfigureReceiveSecurityHeader(message, string.Empty, correlationStates, out list);
            SecurityToken requiredSigningToken = null;
            if (factory.ActAsInitiator)
            {
                SecurityTokenAuthenticator initiatorAsymmetricTokenAuthenticator;
                SecurityToken token = null;
                SecurityToken primaryToken = null;
                if (factory.RequireIntegrity)
                {
                    primaryToken = SecurityProtocol.GetToken(this.initiatorAsymmetricTokenProvider, null, helper.RemainingTime());
                    requiredSigningToken = primaryToken;
                }
                if (factory.RequireConfidentiality)
                {
                    token = base.GetCorrelationToken(correlationStates);
                    if (!System.ServiceModel.Security.SecurityUtils.HasSymmetricSecurityKey(token))
                    {
                        securityHeader.WrappedKeySecurityTokenAuthenticator = this.Factory.WrappedKeySecurityTokenAuthenticator;
                    }
                }
                if (factory.AllowSerializedSigningTokenOnReply)
                {
                    initiatorAsymmetricTokenAuthenticator = this.initiatorAsymmetricTokenAuthenticator;
                    requiredSigningToken = null;
                }
                else
                {
                    initiatorAsymmetricTokenAuthenticator = null;
                }
                securityHeader.ConfigureAsymmetricBindingClientReceiveHeader(primaryToken, factory.AsymmetricTokenParameters, token, factory.CryptoTokenParameters, initiatorAsymmetricTokenAuthenticator);
            }
            else
            {
                SecurityToken token4;
                if ((this.Factory.RecipientAsymmetricTokenProvider != null) && this.Factory.RequireConfidentiality)
                {
                    token4 = SecurityProtocol.GetToken(factory.RecipientAsymmetricTokenProvider, null, helper.RemainingTime());
                }
                else
                {
                    token4 = null;
                }
                securityHeader.ConfigureAsymmetricBindingServerReceiveHeader(this.Factory.RecipientCryptoTokenAuthenticator, this.Factory.CryptoTokenParameters, token4, this.Factory.AsymmetricTokenParameters, list);
                securityHeader.WrappedKeySecurityTokenAuthenticator = this.Factory.WrappedKeySecurityTokenAuthenticator;
                securityHeader.ConfigureOutOfBandTokenResolver(base.MergeOutOfBandResolvers(list, this.Factory.RecipientOutOfBandTokenResolverList));
            }
            base.ProcessSecurityHeader(securityHeader, ref message, requiredSigningToken, helper.RemainingTime(), correlationStates);
            SecurityToken signatureToken = securityHeader.SignatureToken;
            SecurityToken encryptionToken = securityHeader.EncryptionToken;
            if (factory.RequireIntegrity)
            {
                if (factory.ActAsInitiator)
                {
                    ReadOnlyCollection<IAuthorizationPolicy> recipientTokenPolicies = this.initiatorAsymmetricTokenAuthenticator.ValidateToken(signatureToken);
                    MessageSecurityProtocol.EnsureNonWrappedToken(signatureToken, message);
                    this.DoIdentityCheckAndAttachInitiatorSecurityProperty(message, encryptionToken, signatureToken, recipientTokenPolicies);
                }
                else
                {
                    MessageSecurityProtocol.EnsureNonWrappedToken(signatureToken, message);
                    this.AttachRecipientSecurityProperty(message, signatureToken, encryptionToken, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens, securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
                }
            }
            return base.GetCorrelationState(signatureToken, securityHeader);
        }

        protected override bool DoAutomaticEncryptionMatch
        {
            get
            {
                return false;
            }
        }

        private AsymmetricSecurityProtocolFactory Factory
        {
            get
            {
                return (AsymmetricSecurityProtocolFactory) base.MessageSecurityProtocolFactory;
            }
        }

        public SecurityTokenAuthenticator InitiatorAsymmetricTokenAuthenticator
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorAsymmetricTokenAuthenticator;
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

        public SecurityTokenProvider InitiatorCryptoTokenProvider
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.initiatorCryptoTokenProvider;
            }
        }

        private sealed class SecureOutgoingMessageAsyncResult : MessageSecurityProtocol.GetTwoTokensAndSetUpSecurityAsyncResult
        {
            public SecureOutgoingMessageAsyncResult(Message m, AsymmetricSecurityProtocol binding, SecurityTokenProvider primaryProvider, SecurityTokenProvider secondaryProvider, bool doIdentityChecks, SecurityTokenAuthenticator identityCheckAuthenticator, SecurityProtocolCorrelationState correlationState, TimeSpan timeout, AsyncCallback callback, object state) : base(m, binding, primaryProvider, secondaryProvider, doIdentityChecks, identityCheckAuthenticator, correlationState, timeout, callback, state)
            {
                base.Start();
            }

            protected override void OnBothGetTokenCallsDone(ref Message message, SecurityToken primaryToken, SecurityToken secondaryToken, TimeSpan timeout)
            {
                AsymmetricSecurityProtocol binding = (AsymmetricSecurityProtocol) base.Binding;
                if (secondaryToken != null)
                {
                    base.SetCorrelationToken(secondaryToken);
                }
                binding.SetUpDelayedSecurityExecution(ref message, primaryToken, secondaryToken, base.SupportingTokens, binding.GetSignatureConfirmationCorrelationState(base.OldCorrelationState, base.NewCorrelationState));
            }
        }
    }
}

