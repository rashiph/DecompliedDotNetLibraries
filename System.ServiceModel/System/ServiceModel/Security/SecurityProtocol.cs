namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    internal abstract class SecurityProtocol : ISecurityCommunicationObject
    {
        private ChannelParameterCollection channelParameters;
        private ICollection<SupportingTokenProviderSpecification> channelSupportingTokenProviderSpecification;
        private WrapperSecurityCommunicationObject communicationObject;
        private static ReadOnlyCollection<SupportingTokenProviderSpecification> emptyTokenProviders;
        private System.ServiceModel.Security.SecurityProtocolFactory factory;
        private Dictionary<string, Collection<SupportingTokenProviderSpecification>> mergedSupportingTokenProvidersMap;
        private Dictionary<string, ICollection<SupportingTokenProviderSpecification>> scopedSupportingTokenProviderSpecification;
        private EndpointAddress target;
        private Uri via;

        protected SecurityProtocol(System.ServiceModel.Security.SecurityProtocolFactory factory, EndpointAddress target, Uri via)
        {
            this.factory = factory;
            this.target = target;
            this.via = via;
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        internal void AddMessageSupportingTokens(Message message, ref IList<SupportingTokenSpecification> supportingTokens)
        {
            SecurityMessageProperty security = message.Properties.Security;
            if ((security != null) && security.HasOutgoingSupportingTokens)
            {
                if (supportingTokens == null)
                {
                    supportingTokens = new Collection<SupportingTokenSpecification>();
                }
                for (int i = 0; i < security.OutgoingSupportingTokens.Count; i++)
                {
                    SupportingTokenSpecification item = security.OutgoingSupportingTokens[i];
                    if (item.SecurityTokenParameters == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SenderSideSupportingTokensMustSpecifySecurityTokenParameters")));
                    }
                    supportingTokens.Add(item);
                }
            }
        }

        private void AddSupportingTokenProviders(SupportingTokenParameters supportingTokenParameters, bool isOptional, IList<SupportingTokenProviderSpecification> providerSpecList)
        {
            for (int i = 0; i < supportingTokenParameters.Endorsing.Count; i++)
            {
                SecurityTokenRequirement tokenRequirement = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.Endorsing[i], SecurityTokenAttachmentMode.Endorsing);
                try
                {
                    SupportingTokenProviderSpecification item = new SupportingTokenProviderSpecification(this.factory.SecurityTokenManager.CreateSecurityTokenProvider(tokenRequirement), SecurityTokenAttachmentMode.Endorsing, supportingTokenParameters.Endorsing[i]);
                    providerSpecList.Add(item);
                }
                catch (Exception exception)
                {
                    if (!isOptional || Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
            }
            for (int j = 0; j < supportingTokenParameters.SignedEndorsing.Count; j++)
            {
                SecurityTokenRequirement requirement2 = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.SignedEndorsing[j], SecurityTokenAttachmentMode.SignedEndorsing);
                try
                {
                    SupportingTokenProviderSpecification specification2 = new SupportingTokenProviderSpecification(this.factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement2), SecurityTokenAttachmentMode.SignedEndorsing, supportingTokenParameters.SignedEndorsing[j]);
                    providerSpecList.Add(specification2);
                }
                catch (Exception exception2)
                {
                    if (!isOptional || Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                }
            }
            for (int k = 0; k < supportingTokenParameters.SignedEncrypted.Count; k++)
            {
                SecurityTokenRequirement requirement3 = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.SignedEncrypted[k], SecurityTokenAttachmentMode.SignedEncrypted);
                try
                {
                    SupportingTokenProviderSpecification specification3 = new SupportingTokenProviderSpecification(this.factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement3), SecurityTokenAttachmentMode.SignedEncrypted, supportingTokenParameters.SignedEncrypted[k]);
                    providerSpecList.Add(specification3);
                }
                catch (Exception exception3)
                {
                    if (!isOptional || Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                }
            }
            for (int m = 0; m < supportingTokenParameters.Signed.Count; m++)
            {
                SecurityTokenRequirement requirement4 = this.CreateInitiatorSecurityTokenRequirement(supportingTokenParameters.Signed[m], SecurityTokenAttachmentMode.Signed);
                try
                {
                    SupportingTokenProviderSpecification specification4 = new SupportingTokenProviderSpecification(this.factory.SecurityTokenManager.CreateSecurityTokenProvider(requirement4), SecurityTokenAttachmentMode.Signed, supportingTokenParameters.Signed[m]);
                    providerSpecList.Add(specification4);
                }
                catch (Exception exception4)
                {
                    if (!isOptional || Fx.IsFatal(exception4))
                    {
                        throw;
                    }
                }
            }
        }

        protected void AddSupportingTokens(SendSecurityHeader securityHeader, IList<SupportingTokenSpecification> supportingTokens)
        {
            if (supportingTokens != null)
            {
                for (int i = 0; i < supportingTokens.Count; i++)
                {
                    SecurityToken securityToken = supportingTokens[i].SecurityToken;
                    SecurityTokenParameters securityTokenParameters = supportingTokens[i].SecurityTokenParameters;
                    switch (supportingTokens[i].SecurityTokenAttachmentMode)
                    {
                        case SecurityTokenAttachmentMode.Signed:
                            securityHeader.AddSignedSupportingToken(securityToken, securityTokenParameters);
                            break;

                        case SecurityTokenAttachmentMode.Endorsing:
                            securityHeader.AddEndorsingSupportingToken(securityToken, securityTokenParameters);
                            break;

                        case SecurityTokenAttachmentMode.SignedEndorsing:
                            securityHeader.AddSignedEndorsingSupportingToken(securityToken, securityTokenParameters);
                            break;

                        case SecurityTokenAttachmentMode.SignedEncrypted:
                            securityHeader.AddBasicSupportingToken(securityToken, securityTokenParameters);
                            break;

                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnknownTokenAttachmentMode", new object[] { supportingTokens[i].SecurityTokenAttachmentMode.ToString() })));
                    }
                }
            }
        }

        private void AddSupportingTokenSpecification(SecurityMessageProperty security, IList<SecurityToken> tokens, SecurityTokenAttachmentMode attachmentMode, IDictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            if ((tokens != null) && (tokens.Count != 0))
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    security.IncomingSupportingTokens.Add(new SupportingTokenSpecification(tokens[i], tokenPoliciesMapping[tokens[i]], attachmentMode));
                }
            }
        }

        protected void AddSupportingTokenSpecification(SecurityMessageProperty security, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens, IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, IDictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            this.AddSupportingTokenSpecification(security, basicTokens, SecurityTokenAttachmentMode.SignedEncrypted, tokenPoliciesMapping);
            this.AddSupportingTokenSpecification(security, endorsingTokens, SecurityTokenAttachmentMode.Endorsing, tokenPoliciesMapping);
            this.AddSupportingTokenSpecification(security, signedEndorsingTokens, SecurityTokenAttachmentMode.SignedEndorsing, tokenPoliciesMapping);
            this.AddSupportingTokenSpecification(security, signedTokens, SecurityTokenAttachmentMode.Signed, tokenPoliciesMapping);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(timeout, callback, state);
        }

        public virtual IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.SecureOutgoingMessage(ref message, timeout);
            return new CompletedAsyncResult<Message>(message, callback, state);
        }

        public virtual IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, this.SecureOutgoingMessage(ref message, timeout, correlationState), callback, state);
        }

        public virtual IAsyncResult BeginVerifyIncomingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.VerifyIncomingMessage(ref message, timeout);
            return new CompletedAsyncResult<Message>(message, callback, state);
        }

        public virtual IAsyncResult BeginVerifyIncomingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState[] correlationStates, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, this.VerifyIncomingMessage(ref message, timeout, correlationStates), callback, state);
        }

        public void Close(bool aborted, TimeSpan timeout)
        {
            if (aborted)
            {
                this.communicationObject.Abort();
            }
            else
            {
                this.communicationObject.Close(timeout);
            }
        }

        protected InitiatorServiceModelSecurityTokenRequirement CreateInitiatorSecurityTokenRequirement()
        {
            InitiatorServiceModelSecurityTokenRequirement requirement = new InitiatorServiceModelSecurityTokenRequirement {
                TargetAddress = this.Target,
                Via = this.via,
                SecurityBindingElement = this.factory.SecurityBindingElement,
                SecurityAlgorithmSuite = this.factory.OutgoingAlgorithmSuite,
                MessageSecurityVersion = this.factory.MessageSecurityVersion.SecurityTokenVersion
            };
            if (this.factory.PrivacyNoticeUri != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty] = this.factory.PrivacyNoticeUri;
            }
            if (this.channelParameters != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = this.channelParameters;
            }
            requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty] = this.factory.PrivacyNoticeVersion;
            return requirement;
        }

        private InitiatorServiceModelSecurityTokenRequirement CreateInitiatorSecurityTokenRequirement(SecurityTokenParameters parameters, SecurityTokenAttachmentMode attachmentMode)
        {
            InitiatorServiceModelSecurityTokenRequirement requirement = this.CreateInitiatorSecurityTokenRequirement();
            parameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = SecurityKeyUsage.Signature;
            requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Output;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportingTokenAttachmentModeProperty] = attachmentMode;
            return requirement;
        }

        protected SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, System.ServiceModel.Security.SecurityProtocolFactory factory)
        {
            return this.CreateSendSecurityHeader(message, actor, factory, true);
        }

        private SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, System.ServiceModel.Security.SecurityProtocolFactory factory, bool requireMessageProtection)
        {
            MessageDirection direction = factory.ActAsInitiator ? MessageDirection.Input : MessageDirection.Output;
            SendSecurityHeader securityHeader = factory.StandardsManager.CreateSendSecurityHeader(message, actor, true, false, factory.OutgoingAlgorithmSuite, direction);
            securityHeader.Layout = factory.SecurityHeaderLayout;
            securityHeader.RequireMessageProtection = requireMessageProtection;
            SetSecurityHeaderId(securityHeader, message);
            if (factory.AddTimestamp)
            {
                securityHeader.AddTimestamp(factory.TimestampValidityDuration);
            }
            return securityHeader;
        }

        protected SendSecurityHeader CreateSendSecurityHeaderForTransportProtocol(Message message, string actor, System.ServiceModel.Security.SecurityProtocolFactory factory)
        {
            return this.CreateSendSecurityHeader(message, actor, factory, false);
        }

        public void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.communicationObject.EndOpen(result);
        }

        public virtual void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
        {
            message = CompletedAsyncResult<Message>.End(result);
        }

        public virtual void EndSecureOutgoingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
        }

        public virtual void EndVerifyIncomingMessage(IAsyncResult result, out Message message)
        {
            message = CompletedAsyncResult<Message>.End(result);
        }

        public virtual void EndVerifyIncomingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
        }

        protected IList<SupportingTokenAuthenticatorSpecification> GetSupportingTokenAuthenticatorsAndSetExpectationFlags(System.ServiceModel.Security.SecurityProtocolFactory factory, Message message, ReceiveSecurityHeader securityHeader)
        {
            bool flag;
            bool flag2;
            bool flag3;
            if (factory.ActAsInitiator)
            {
                return null;
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            IList<SupportingTokenAuthenticatorSpecification> list = factory.GetSupportingTokenAuthenticators(message.Headers.Action, out flag2, out flag, out flag3);
            securityHeader.ExpectBasicTokens = flag;
            securityHeader.ExpectEndorsingTokens = flag3;
            securityHeader.ExpectSignedTokens = flag2;
            return list;
        }

        internal IList<SupportingTokenProviderSpecification> GetSupportingTokenProviders(string action)
        {
            if ((this.mergedSupportingTokenProvidersMap != null) && (this.mergedSupportingTokenProvidersMap.Count > 0))
            {
                if ((action != null) && this.mergedSupportingTokenProvidersMap.ContainsKey(action))
                {
                    return this.mergedSupportingTokenProvidersMap[action];
                }
                if (this.mergedSupportingTokenProvidersMap.ContainsKey("*"))
                {
                    return this.mergedSupportingTokenProvidersMap["*"];
                }
            }
            if (this.channelSupportingTokenProviderSpecification != EmptyTokenProviders)
            {
                return (IList<SupportingTokenProviderSpecification>) this.channelSupportingTokenProviderSpecification;
            }
            return null;
        }

        internal static SecurityToken GetToken(SecurityTokenProvider provider, EndpointAddress target, TimeSpan timeout)
        {
            if (provider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenProviderCannotGetTokensForTarget", new object[] { target })));
            }
            SecurityToken token = null;
            try
            {
                token = provider.GetToken(timeout);
            }
            catch (SecurityTokenException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenProviderCannotGetTokensForTarget", new object[] { target }), exception));
            }
            catch (SecurityNegotiationException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("TokenProviderCannotGetTokensForTarget", new object[] { target }), exception2));
            }
            return token;
        }

        protected ReadOnlyCollection<SecurityTokenResolver> MergeOutOfBandResolvers(IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators, ReadOnlyCollection<SecurityTokenResolver> primaryResolvers)
        {
            Collection<SecurityTokenResolver> list = null;
            if ((supportingAuthenticators != null) && (supportingAuthenticators.Count > 0))
            {
                for (int i = 0; i < supportingAuthenticators.Count; i++)
                {
                    if (supportingAuthenticators[i].TokenResolver != null)
                    {
                        list = list ?? new Collection<SecurityTokenResolver>();
                        list.Add(supportingAuthenticators[i].TokenResolver);
                    }
                }
            }
            if (list != null)
            {
                if (primaryResolvers != null)
                {
                    for (int j = 0; j < primaryResolvers.Count; j++)
                    {
                        list.Insert(0, primaryResolvers[j]);
                    }
                }
                return new ReadOnlyCollection<SecurityTokenResolver>(list);
            }
            return (primaryResolvers ?? EmptyReadOnlyCollection<SecurityTokenResolver>.Instance);
        }

        private void MergeSupportingTokenProviders(TimeSpan timeout)
        {
            if (this.ScopedSupportingTokenProviderSpecification.Count == 0)
            {
                this.mergedSupportingTokenProvidersMap = null;
            }
            else
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.factory.ExpectSupportingTokens = true;
                this.mergedSupportingTokenProvidersMap = new Dictionary<string, Collection<SupportingTokenProviderSpecification>>();
                foreach (string str in this.ScopedSupportingTokenProviderSpecification.Keys)
                {
                    ICollection<SupportingTokenProviderSpecification> is2 = this.ScopedSupportingTokenProviderSpecification[str];
                    if ((is2 != null) && (is2.Count != 0))
                    {
                        Collection<SupportingTokenProviderSpecification> collection = new Collection<SupportingTokenProviderSpecification>();
                        foreach (SupportingTokenProviderSpecification specification in this.channelSupportingTokenProviderSpecification)
                        {
                            collection.Add(specification);
                        }
                        foreach (SupportingTokenProviderSpecification specification2 in is2)
                        {
                            System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(specification2.TokenProvider, helper.RemainingTime());
                            if (((specification2.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (specification2.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)) && (specification2.TokenParameters.RequireDerivedKeys && !specification2.TokenParameters.HasAsymmetricKey))
                            {
                                this.factory.ExpectKeyDerivation = true;
                            }
                            collection.Add(specification2);
                        }
                        this.mergedSupportingTokenProvidersMap.Add(str, collection);
                    }
                }
            }
        }

        public virtual void OnAbort()
        {
            if (this.factory.ActAsInitiator)
            {
                foreach (SupportingTokenProviderSpecification specification in this.channelSupportingTokenProviderSpecification)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(specification.TokenProvider);
                }
                foreach (string str in this.scopedSupportingTokenProviderSpecification.Keys)
                {
                    ICollection<SupportingTokenProviderSpecification> is2 = this.scopedSupportingTokenProviderSpecification[str];
                    foreach (SupportingTokenProviderSpecification specification2 in is2)
                    {
                        System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(specification2.TokenProvider);
                    }
                }
            }
        }

        public IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        public IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        public virtual void OnClose(TimeSpan timeout)
        {
            if (this.factory.ActAsInitiator)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                foreach (SupportingTokenProviderSpecification specification in this.channelSupportingTokenProviderSpecification)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(specification.TokenProvider, helper.RemainingTime());
                }
                foreach (string str in this.scopedSupportingTokenProviderSpecification.Keys)
                {
                    ICollection<SupportingTokenProviderSpecification> is2 = this.scopedSupportingTokenProviderSpecification[str];
                    foreach (SupportingTokenProviderSpecification specification2 in is2)
                    {
                        System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(specification2.TokenProvider, helper.RemainingTime());
                    }
                }
            }
        }

        public void OnClosed()
        {
        }

        public void OnClosing()
        {
        }

        public void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        public void OnFaulted()
        {
        }

        protected virtual void OnIncomingMessageVerified(Message verifiedMessage)
        {
            SecurityTraceRecordHelper.TraceIncomingMessageVerified(this, verifiedMessage);
            if (AuditLevel.Success == (this.factory.MessageAuthenticationAuditLevel & AuditLevel.Success))
            {
                SecurityAuditHelper.WriteMessageAuthenticationSuccessEvent(this.factory.AuditLogLocation, this.factory.SuppressAuditFailure, verifiedMessage, verifiedMessage.Headers.To, verifiedMessage.Headers.Action, System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(verifiedMessage.Properties.Security.ServiceSecurityContext.AuthorizationContext));
            }
        }

        public virtual void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.factory.ActAsInitiator)
            {
                this.channelSupportingTokenProviderSpecification = new Collection<SupportingTokenProviderSpecification>();
                this.scopedSupportingTokenProviderSpecification = new Dictionary<string, ICollection<SupportingTokenProviderSpecification>>();
                this.AddSupportingTokenProviders(this.factory.SecurityBindingElement.EndpointSupportingTokenParameters, false, (IList<SupportingTokenProviderSpecification>) this.channelSupportingTokenProviderSpecification);
                this.AddSupportingTokenProviders(this.factory.SecurityBindingElement.OptionalEndpointSupportingTokenParameters, true, (IList<SupportingTokenProviderSpecification>) this.channelSupportingTokenProviderSpecification);
                foreach (string str in this.factory.SecurityBindingElement.OperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenProviderSpecification> providerSpecList = new Collection<SupportingTokenProviderSpecification>();
                    this.AddSupportingTokenProviders(this.factory.SecurityBindingElement.OperationSupportingTokenParameters[str], false, providerSpecList);
                    this.scopedSupportingTokenProviderSpecification.Add(str, providerSpecList);
                }
                foreach (string str2 in this.factory.SecurityBindingElement.OptionalOperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenProviderSpecification> collection2;
                    ICollection<SupportingTokenProviderSpecification> is2;
                    if (this.scopedSupportingTokenProviderSpecification.TryGetValue(str2, out is2))
                    {
                        collection2 = (Collection<SupportingTokenProviderSpecification>) is2;
                    }
                    else
                    {
                        collection2 = new Collection<SupportingTokenProviderSpecification>();
                        this.scopedSupportingTokenProviderSpecification.Add(str2, collection2);
                    }
                    this.AddSupportingTokenProviders(this.factory.SecurityBindingElement.OptionalOperationSupportingTokenParameters[str2], true, collection2);
                }
                if (!this.channelSupportingTokenProviderSpecification.IsReadOnly)
                {
                    if (this.channelSupportingTokenProviderSpecification.Count == 0)
                    {
                        this.channelSupportingTokenProviderSpecification = EmptyTokenProviders;
                    }
                    else
                    {
                        this.factory.ExpectSupportingTokens = true;
                        foreach (SupportingTokenProviderSpecification specification in this.channelSupportingTokenProviderSpecification)
                        {
                            System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(specification.TokenProvider, helper.RemainingTime());
                            if (((specification.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (specification.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)) && (specification.TokenParameters.RequireDerivedKeys && !specification.TokenParameters.HasAsymmetricKey))
                            {
                                this.factory.ExpectKeyDerivation = true;
                            }
                        }
                        this.channelSupportingTokenProviderSpecification = new ReadOnlyCollection<SupportingTokenProviderSpecification>((Collection<SupportingTokenProviderSpecification>) this.channelSupportingTokenProviderSpecification);
                    }
                }
                this.MergeSupportingTokenProviders(helper.RemainingTime());
            }
        }

        public void OnOpened()
        {
        }

        public void OnOpening()
        {
        }

        protected virtual void OnOutgoingMessageSecured(Message securedMessage)
        {
            SecurityTraceRecordHelper.TraceOutgoingMessageSecured(this, securedMessage);
        }

        protected virtual void OnSecureOutgoingMessageFailure(Message message)
        {
            SecurityTraceRecordHelper.TraceSecureOutgoingMessageFailure(this, message);
        }

        protected virtual void OnVerifyIncomingMessageFailure(Message message, Exception exception)
        {
            SecurityTraceRecordHelper.TraceVerifyIncomingMessageFailure(this, message);
            if ((PerformanceCounters.PerformanceCountersEnabled && (null != this.factory.ListenUri)) && (((exception.GetType() == typeof(MessageSecurityException)) || exception.GetType().IsSubclassOf(typeof(MessageSecurityException))) || ((exception.GetType() == typeof(SecurityTokenException)) || exception.GetType().IsSubclassOf(typeof(SecurityTokenException)))))
            {
                PerformanceCounters.AuthenticationFailed(message, this.factory.ListenUri);
            }
            if (AuditLevel.Failure == (this.factory.MessageAuthenticationAuditLevel & AuditLevel.Failure))
            {
                try
                {
                    string identityNamesFromContext;
                    SecurityMessageProperty security = message.Properties.Security;
                    if ((security != null) && (security.ServiceSecurityContext != null))
                    {
                        identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(security.ServiceSecurityContext.AuthorizationContext);
                    }
                    else
                    {
                        identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.AnonymousIdentity.Name;
                    }
                    SecurityAuditHelper.WriteMessageAuthenticationFailureEvent(this.factory.AuditLogLocation, this.factory.SuppressAuditFailure, message, message.Headers.To, message.Headers.Action, identityNamesFromContext, exception);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                }
            }
        }

        public void Open(TimeSpan timeout)
        {
            this.communicationObject.Open(timeout);
        }

        public abstract void SecureOutgoingMessage(ref Message message, TimeSpan timeout);
        public virtual SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
        {
            this.SecureOutgoingMessage(ref message, timeout);
            return null;
        }

        private static void SetSecurityHeaderId(SendSecurityHeader securityHeader, Message message)
        {
            SecurityMessageProperty security = message.Properties.Security;
            if (security != null)
            {
                securityHeader.IdPrefix = security.SenderIdPrefix;
            }
        }

        internal bool TryGetSupportingTokens(System.ServiceModel.Security.SecurityProtocolFactory factory, EndpointAddress target, Uri via, Message message, TimeSpan timeout, bool isBlockingCall, out IList<SupportingTokenSpecification> supportingTokens)
        {
            if (!factory.ActAsInitiator)
            {
                supportingTokens = null;
                return true;
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            supportingTokens = null;
            IList<SupportingTokenProviderSpecification> supportingTokenProviders = this.GetSupportingTokenProviders(message.Headers.Action);
            if ((supportingTokenProviders != null) && (supportingTokenProviders.Count > 0))
            {
                if (!isBlockingCall)
                {
                    return false;
                }
                supportingTokens = new Collection<SupportingTokenSpecification>();
                for (int i = 0; i < supportingTokenProviders.Count; i++)
                {
                    SecurityToken token;
                    SupportingTokenProviderSpecification specification = supportingTokenProviders[i];
                    if ((this is TransportSecurityProtocol) && (specification.TokenParameters is KerberosSecurityTokenParameters))
                    {
                        token = new ProviderBackedSecurityToken(specification.TokenProvider, helper.RemainingTime());
                    }
                    else
                    {
                        token = specification.TokenProvider.GetToken(helper.RemainingTime());
                    }
                    supportingTokens.Add(new SupportingTokenSpecification(token, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, specification.SecurityTokenAttachmentMode, specification.TokenParameters));
                }
            }
            this.AddMessageSupportingTokens(message, ref supportingTokens);
            return true;
        }

        public abstract void VerifyIncomingMessage(ref Message message, TimeSpan timeout);
        public virtual SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout, params SecurityProtocolCorrelationState[] correlationStates)
        {
            this.VerifyIncomingMessage(ref message, timeout);
            return null;
        }

        public ChannelParameterCollection ChannelParameters
        {
            get
            {
                return this.channelParameters;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.channelParameters = value;
            }
        }

        public ICollection<SupportingTokenProviderSpecification> ChannelSupportingTokenProviderSpecification
        {
            get
            {
                return this.channelSupportingTokenProviderSpecification;
            }
        }

        protected WrapperSecurityCommunicationObject CommunicationObject
        {
            get
            {
                return this.communicationObject;
            }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get
            {
                return ServiceDefaults.CloseTimeout;
            }
        }

        public TimeSpan DefaultOpenTimeout
        {
            get
            {
                return ServiceDefaults.OpenTimeout;
            }
        }

        private static ReadOnlyCollection<SupportingTokenProviderSpecification> EmptyTokenProviders
        {
            get
            {
                if (emptyTokenProviders == null)
                {
                    emptyTokenProviders = new ReadOnlyCollection<SupportingTokenProviderSpecification>(new List<SupportingTokenProviderSpecification>());
                }
                return emptyTokenProviders;
            }
        }

        public Dictionary<string, ICollection<SupportingTokenProviderSpecification>> ScopedSupportingTokenProviderSpecification
        {
            get
            {
                return this.scopedSupportingTokenProviderSpecification;
            }
        }

        public System.ServiceModel.Security.SecurityProtocolFactory SecurityProtocolFactory
        {
            get
            {
                return this.factory;
            }
        }

        public EndpointAddress Target
        {
            get
            {
                return this.target;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
        }

        protected abstract class GetSupportingTokensAsyncResult : AsyncResult
        {
            private SecurityProtocol binding;
            private int currentTokenProviderIndex;
            private static AsyncCallback getSupportingTokensCallback = Fx.ThunkCallback(new AsyncCallback(SecurityProtocol.GetSupportingTokensAsyncResult.GetSupportingTokenCallback));
            private Message message;
            private IList<SupportingTokenProviderSpecification> supportingTokenProviders;
            private IList<SupportingTokenSpecification> supportingTokens;
            private TimeoutHelper timeoutHelper;

            public GetSupportingTokensAsyncResult(Message m, SecurityProtocol binding, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.message = m;
                this.binding = binding;
                this.timeoutHelper = new TimeoutHelper(timeout);
            }

            private void AddSupportingToken(IAsyncResult result)
            {
                SupportingTokenProviderSpecification specification = this.supportingTokenProviders[this.currentTokenProviderIndex];
                if (result is SecurityTokenProvider.SecurityTokenAsyncResult)
                {
                    this.supportingTokens.Add(new SupportingTokenSpecification(SecurityTokenProvider.SecurityTokenAsyncResult.End(result), EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, specification.SecurityTokenAttachmentMode, specification.TokenParameters));
                }
                else
                {
                    this.supportingTokens.Add(new SupportingTokenSpecification(specification.TokenProvider.EndGetToken(result), EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, specification.SecurityTokenAttachmentMode, specification.TokenParameters));
                }
                this.currentTokenProviderIndex++;
            }

            private bool AddSupportingTokens()
            {
                while (this.currentTokenProviderIndex < this.supportingTokenProviders.Count)
                {
                    SupportingTokenProviderSpecification specification = this.supportingTokenProviders[this.currentTokenProviderIndex];
                    IAsyncResult result = null;
                    if ((this.binding is TransportSecurityProtocol) && (specification.TokenParameters is KerberosSecurityTokenParameters))
                    {
                        result = new SecurityTokenProvider.SecurityTokenAsyncResult(new ProviderBackedSecurityToken(specification.TokenProvider, this.timeoutHelper.RemainingTime()), null, this);
                    }
                    else
                    {
                        result = specification.TokenProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), getSupportingTokensCallback, this);
                    }
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.AddSupportingToken(result);
                }
                this.binding.AddMessageSupportingTokens(this.message, ref this.supportingTokens);
                return this.OnGetSupportingTokensDone(this.timeoutHelper.RemainingTime());
            }

            private static void GetSupportingTokenCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    SecurityProtocol.GetSupportingTokensAsyncResult asyncState = (SecurityProtocol.GetSupportingTokensAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.AddSupportingToken(result);
                        flag = asyncState.AddSupportingTokens();
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

            protected abstract bool OnGetSupportingTokensDone(TimeSpan timeout);
            protected void Start()
            {
                bool flag;
                if (this.binding.TryGetSupportingTokens(this.binding.SecurityProtocolFactory, this.binding.Target, this.binding.Via, this.message, this.timeoutHelper.RemainingTime(), false, out this.supportingTokens))
                {
                    flag = this.OnGetSupportingTokensDone(this.timeoutHelper.RemainingTime());
                }
                else
                {
                    this.supportingTokens = new Collection<SupportingTokenSpecification>();
                    this.supportingTokenProviders = this.binding.GetSupportingTokenProviders(this.message.Headers.Action);
                    if ((this.supportingTokenProviders == null) || (this.supportingTokenProviders.Count <= 0))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException("There must be at least 1 supporting token provider"));
                    }
                    flag = this.AddSupportingTokens();
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            protected IList<SupportingTokenSpecification> SupportingTokens
            {
                get
                {
                    return this.supportingTokens;
                }
            }
        }
    }
}

