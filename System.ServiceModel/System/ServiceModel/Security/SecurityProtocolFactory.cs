namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;

    internal abstract class SecurityProtocolFactory : ISecurityCommunicationObject
    {
        private bool actAsInitiator;
        private bool addTimestamp;
        private System.ServiceModel.AuditLogLocation auditLogLocation;
        private ICollection<SupportingTokenAuthenticatorSpecification> channelSupportingTokenAuthenticatorSpecification;
        private WrapperSecurityCommunicationObject communicationObject;
        internal const bool defaultAddTimestamp = true;
        internal const bool defaultDeriveKeys = true;
        internal const bool defaultDetectReplays = true;
        internal const int defaultMaxCachedNonces = 0xdbba0;
        internal static readonly TimeSpan defaultMaxClockSkew = TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture);
        internal const string defaultMaxClockSkewString = "00:05:00";
        internal static readonly TimeSpan defaultReplayWindow = TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture);
        internal const string defaultReplayWindowString = "00:05:00";
        internal const System.ServiceModel.Channels.SecurityHeaderLayout defaultSecurityHeaderLayout = System.ServiceModel.Channels.SecurityHeaderLayout.Strict;
        internal static readonly TimeSpan defaultTimestampValidityDuration = TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture);
        internal const string defaultTimestampValidityDurationString = "00:05:00";
        private NonValidatingSecurityTokenAuthenticator<DerivedKeySecurityToken> derivedKeyTokenAuthenticator;
        private bool detectReplays;
        private static ReadOnlyCollection<SupportingTokenAuthenticatorSpecification> emptyTokenAuthenticators;
        private IMessageFilterTable<EndpointAddress> endpointFilterTable;
        private bool expectChannelBasicTokens;
        private bool expectChannelEndorsingTokens;
        private bool expectChannelSignedTokens;
        private bool expectIncomingMessages;
        private bool expectKeyDerivation;
        private bool expectOutgoingMessages;
        private bool expectSupportingTokens;
        private System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy extendedProtectionPolicy;
        private SecurityAlgorithmSuite incomingAlgorithmSuite;
        private bool isDuplexReply;
        private Uri listenUri;
        private int maxCachedNonces;
        private TimeSpan maxClockSkew;
        private Dictionary<string, MergedSupportingTokenAuthenticatorSpecification> mergedSupportingTokenAuthenticatorsMap;
        private AuditLevel messageAuthenticationAuditLevel;
        private System.ServiceModel.MessageSecurityVersion messageSecurityVersion;
        private System.ServiceModel.Security.NonceCache nonceCache;
        private SecurityAlgorithmSuite outgoingAlgorithmSuite;
        private Uri privacyNoticeUri;
        private int privacyNoticeVersion;
        private TimeSpan replayWindow;
        private string requestReplyErrorPropertyName;
        private Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>> scopedSupportingTokenAuthenticatorSpecification;
        private System.ServiceModel.Channels.SecurityBindingElement securityBindingElement;
        private System.ServiceModel.Channels.SecurityHeaderLayout securityHeaderLayout;
        private System.IdentityModel.Selectors.SecurityTokenManager securityTokenManager;
        private AuditLevel serviceAuthorizationAuditLevel;
        private SecurityStandardsManager standardsManager;
        private bool suppressAuditFailure;
        private TimeSpan timestampValidityDuration;

        protected SecurityProtocolFactory()
        {
            this.addTimestamp = true;
            this.detectReplays = true;
            this.incomingAlgorithmSuite = SecurityAlgorithmSuite.Default;
            this.maxCachedNonces = 0xdbba0;
            this.maxClockSkew = defaultMaxClockSkew;
            this.outgoingAlgorithmSuite = SecurityAlgorithmSuite.Default;
            this.replayWindow = defaultReplayWindow;
            this.standardsManager = SecurityStandardsManager.DefaultInstance;
            this.timestampValidityDuration = defaultTimestampValidityDuration;
            this.channelSupportingTokenAuthenticatorSpecification = new Collection<SupportingTokenAuthenticatorSpecification>();
            this.scopedSupportingTokenAuthenticatorSpecification = new Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>>();
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        internal SecurityProtocolFactory(SecurityProtocolFactory factory) : this()
        {
            if (factory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("factory");
            }
            this.actAsInitiator = factory.actAsInitiator;
            this.addTimestamp = factory.addTimestamp;
            this.detectReplays = factory.detectReplays;
            this.incomingAlgorithmSuite = factory.incomingAlgorithmSuite;
            this.maxCachedNonces = factory.maxCachedNonces;
            this.maxClockSkew = factory.maxClockSkew;
            this.outgoingAlgorithmSuite = factory.outgoingAlgorithmSuite;
            this.replayWindow = factory.replayWindow;
            this.channelSupportingTokenAuthenticatorSpecification = new Collection<SupportingTokenAuthenticatorSpecification>(new List<SupportingTokenAuthenticatorSpecification>(factory.channelSupportingTokenAuthenticatorSpecification));
            this.scopedSupportingTokenAuthenticatorSpecification = new Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>>(factory.scopedSupportingTokenAuthenticatorSpecification);
            this.standardsManager = factory.standardsManager;
            this.timestampValidityDuration = factory.timestampValidityDuration;
            this.auditLogLocation = factory.auditLogLocation;
            this.suppressAuditFailure = factory.suppressAuditFailure;
            this.serviceAuthorizationAuditLevel = factory.serviceAuthorizationAuditLevel;
            this.messageAuthenticationAuditLevel = factory.messageAuthenticationAuditLevel;
            if (factory.securityBindingElement != null)
            {
                this.securityBindingElement = (System.ServiceModel.Channels.SecurityBindingElement) factory.securityBindingElement.Clone();
            }
            this.securityTokenManager = factory.securityTokenManager;
            this.privacyNoticeUri = factory.privacyNoticeUri;
            this.privacyNoticeVersion = factory.privacyNoticeVersion;
            this.endpointFilterTable = factory.endpointFilterTable;
            this.extendedProtectionPolicy = factory.extendedProtectionPolicy;
        }

        private void AddSupportingTokenAuthenticators(SupportingTokenParameters supportingTokenParameters, bool isOptional, IList<SupportingTokenAuthenticatorSpecification> authenticatorSpecList)
        {
            for (int i = 0; i < supportingTokenParameters.Endorsing.Count; i++)
            {
                SecurityTokenRequirement tokenRequirement = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.Endorsing[i], SecurityTokenAttachmentMode.Endorsing);
                try
                {
                    SecurityTokenResolver resolver;
                    SupportingTokenAuthenticatorSpecification item = new SupportingTokenAuthenticatorSpecification(this.SecurityTokenManager.CreateSecurityTokenAuthenticator(tokenRequirement, out resolver), resolver, SecurityTokenAttachmentMode.Endorsing, supportingTokenParameters.Endorsing[i], isOptional);
                    authenticatorSpecList.Add(item);
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
                SecurityTokenRequirement requirement2 = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.SignedEndorsing[j], SecurityTokenAttachmentMode.SignedEndorsing);
                try
                {
                    SecurityTokenResolver resolver2;
                    SupportingTokenAuthenticatorSpecification specification2 = new SupportingTokenAuthenticatorSpecification(this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement2, out resolver2), resolver2, SecurityTokenAttachmentMode.SignedEndorsing, supportingTokenParameters.SignedEndorsing[j], isOptional);
                    authenticatorSpecList.Add(specification2);
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
                SecurityTokenRequirement requirement3 = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.SignedEncrypted[k], SecurityTokenAttachmentMode.SignedEncrypted);
                try
                {
                    SecurityTokenResolver resolver3;
                    SupportingTokenAuthenticatorSpecification specification3 = new SupportingTokenAuthenticatorSpecification(this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement3, out resolver3), resolver3, SecurityTokenAttachmentMode.SignedEncrypted, supportingTokenParameters.SignedEncrypted[k], isOptional);
                    authenticatorSpecList.Add(specification3);
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
                SecurityTokenRequirement requirement4 = this.CreateRecipientSecurityTokenRequirement(supportingTokenParameters.Signed[m], SecurityTokenAttachmentMode.Signed);
                try
                {
                    SecurityTokenResolver resolver4;
                    SupportingTokenAuthenticatorSpecification specification4 = new SupportingTokenAuthenticatorSpecification(this.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement4, out resolver4), resolver4, SecurityTokenAttachmentMode.Signed, supportingTokenParameters.Signed[m], isOptional);
                    authenticatorSpecList.Add(specification4);
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

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.CommunicationObject.BeginClose(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(bool actAsInitiator, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.actAsInitiator = actAsInitiator;
            return this.CommunicationObject.BeginOpen(timeout, callback, state);
        }

        public void Close(bool aborted, TimeSpan timeout)
        {
            if (aborted)
            {
                this.CommunicationObject.Abort();
            }
            else
            {
                this.CommunicationObject.Close(timeout);
            }
        }

        public virtual object CreateListenerSecurityState()
        {
            return null;
        }

        protected RecipientServiceModelSecurityTokenRequirement CreateRecipientSecurityTokenRequirement()
        {
            RecipientServiceModelSecurityTokenRequirement requirement = new RecipientServiceModelSecurityTokenRequirement {
                SecurityBindingElement = this.securityBindingElement,
                SecurityAlgorithmSuite = this.IncomingAlgorithmSuite,
                ListenUri = this.listenUri,
                MessageSecurityVersion = this.MessageSecurityVersion.SecurityTokenVersion,
                AuditLogLocation = this.auditLogLocation,
                SuppressAuditFailure = this.suppressAuditFailure,
                MessageAuthenticationAuditLevel = this.messageAuthenticationAuditLevel
            };
            requirement.Properties[ServiceModelSecurityTokenRequirement.ExtendedProtectionPolicy] = this.extendedProtectionPolicy;
            if (this.endpointFilterTable != null)
            {
                requirement.Properties.Add(ServiceModelSecurityTokenRequirement.EndpointFilterTableProperty, this.endpointFilterTable);
            }
            return requirement;
        }

        private RecipientServiceModelSecurityTokenRequirement CreateRecipientSecurityTokenRequirement(SecurityTokenParameters parameters, SecurityTokenAttachmentMode attachmentMode)
        {
            RecipientServiceModelSecurityTokenRequirement requirement = this.CreateRecipientSecurityTokenRequirement();
            parameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = SecurityKeyUsage.Signature;
            requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportingTokenAttachmentModeProperty] = attachmentMode;
            requirement.Properties[ServiceModelSecurityTokenRequirement.ExtendedProtectionPolicy] = this.extendedProtectionPolicy;
            return requirement;
        }

        public SecurityProtocol CreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, bool isReturnLegSecurityRequired, TimeSpan timeout)
        {
            this.ThrowIfNotOpen();
            SecurityProtocol protocol = this.OnCreateSecurityProtocol(target, via, listenerSecurityState, timeout);
            if (protocol == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("ProtocolFactoryCouldNotCreateProtocol")));
            }
            return protocol;
        }

        public void EndClose(IAsyncResult result)
        {
            this.CommunicationObject.EndClose(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.CommunicationObject.EndOpen(result);
        }

        public virtual EndpointIdentity GetIdentityOfSelf()
        {
            return null;
        }

        public virtual T GetProperty<T>()
        {
            if (!(typeof(T) == typeof(Collection<ISecurityContextSecurityTokenCache>)))
            {
                return default(T);
            }
            this.ThrowIfNotOpen();
            Collection<ISecurityContextSecurityTokenCache> collection = new Collection<ISecurityContextSecurityTokenCache>();
            if (this.channelSupportingTokenAuthenticatorSpecification != null)
            {
                foreach (SupportingTokenAuthenticatorSpecification specification in this.channelSupportingTokenAuthenticatorSpecification)
                {
                    if (specification.TokenAuthenticator is ISecurityContextSecurityTokenCacheProvider)
                    {
                        collection.Add(((ISecurityContextSecurityTokenCacheProvider) specification.TokenAuthenticator).TokenCache);
                    }
                }
            }
            return (T) collection;
        }

        internal IList<SupportingTokenAuthenticatorSpecification> GetSupportingTokenAuthenticators(string action, out bool expectSignedTokens, out bool expectBasicTokens, out bool expectEndorsingTokens)
        {
            if ((this.mergedSupportingTokenAuthenticatorsMap != null) && (this.mergedSupportingTokenAuthenticatorsMap.Count > 0))
            {
                if ((action != null) && this.mergedSupportingTokenAuthenticatorsMap.ContainsKey(action))
                {
                    MergedSupportingTokenAuthenticatorSpecification specification = this.mergedSupportingTokenAuthenticatorsMap[action];
                    expectSignedTokens = specification.ExpectSignedTokens;
                    expectBasicTokens = specification.ExpectBasicTokens;
                    expectEndorsingTokens = specification.ExpectEndorsingTokens;
                    return specification.SupportingTokenAuthenticators;
                }
                if (this.mergedSupportingTokenAuthenticatorsMap.ContainsKey("*"))
                {
                    MergedSupportingTokenAuthenticatorSpecification specification2 = this.mergedSupportingTokenAuthenticatorsMap["*"];
                    expectSignedTokens = specification2.ExpectSignedTokens;
                    expectBasicTokens = specification2.ExpectBasicTokens;
                    expectEndorsingTokens = specification2.ExpectEndorsingTokens;
                    return specification2.SupportingTokenAuthenticators;
                }
            }
            expectSignedTokens = this.expectChannelSignedTokens;
            expectBasicTokens = this.expectChannelBasicTokens;
            expectEndorsingTokens = this.expectChannelEndorsingTokens;
            if (!object.ReferenceEquals(this.channelSupportingTokenAuthenticatorSpecification, EmptyTokenAuthenticators))
            {
                return (IList<SupportingTokenAuthenticatorSpecification>) this.channelSupportingTokenAuthenticatorSpecification;
            }
            return null;
        }

        private void MergeSupportingTokenAuthenticators(TimeSpan timeout)
        {
            if (this.scopedSupportingTokenAuthenticatorSpecification.Count == 0)
            {
                this.mergedSupportingTokenAuthenticatorsMap = null;
            }
            else
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.expectSupportingTokens = true;
                this.mergedSupportingTokenAuthenticatorsMap = new Dictionary<string, MergedSupportingTokenAuthenticatorSpecification>();
                foreach (string str in this.scopedSupportingTokenAuthenticatorSpecification.Keys)
                {
                    ICollection<SupportingTokenAuthenticatorSpecification> is2 = this.scopedSupportingTokenAuthenticatorSpecification[str];
                    if ((is2 != null) && (is2.Count != 0))
                    {
                        Collection<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators = new Collection<SupportingTokenAuthenticatorSpecification>();
                        bool expectChannelSignedTokens = this.expectChannelSignedTokens;
                        bool expectChannelBasicTokens = this.expectChannelBasicTokens;
                        bool expectChannelEndorsingTokens = this.expectChannelEndorsingTokens;
                        foreach (SupportingTokenAuthenticatorSpecification specification in this.channelSupportingTokenAuthenticatorSpecification)
                        {
                            supportingTokenAuthenticators.Add(specification);
                        }
                        foreach (SupportingTokenAuthenticatorSpecification specification2 in is2)
                        {
                            System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(specification2.TokenAuthenticator, helper.RemainingTime());
                            supportingTokenAuthenticators.Add(specification2);
                            if (((specification2.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (specification2.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)) && (specification2.TokenParameters.RequireDerivedKeys && !specification2.TokenParameters.HasAsymmetricKey))
                            {
                                this.expectKeyDerivation = true;
                            }
                            SecurityTokenAttachmentMode securityTokenAttachmentMode = specification2.SecurityTokenAttachmentMode;
                            switch (securityTokenAttachmentMode)
                            {
                                case SecurityTokenAttachmentMode.SignedEncrypted:
                                case SecurityTokenAttachmentMode.Signed:
                                case SecurityTokenAttachmentMode.SignedEndorsing:
                                    expectChannelSignedTokens = true;
                                    if (securityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEncrypted)
                                    {
                                        expectChannelBasicTokens = true;
                                    }
                                    break;
                            }
                            if ((securityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (securityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing))
                            {
                                expectChannelEndorsingTokens = true;
                            }
                        }
                        this.VerifyTypeUniqueness(supportingTokenAuthenticators);
                        MergedSupportingTokenAuthenticatorSpecification specification3 = new MergedSupportingTokenAuthenticatorSpecification {
                            SupportingTokenAuthenticators = supportingTokenAuthenticators,
                            ExpectBasicTokens = expectChannelBasicTokens,
                            ExpectEndorsingTokens = expectChannelEndorsingTokens,
                            ExpectSignedTokens = expectChannelSignedTokens
                        };
                        this.mergedSupportingTokenAuthenticatorsMap.Add(str, specification3);
                    }
                }
            }
        }

        public virtual void OnAbort()
        {
            if (!this.actAsInitiator)
            {
                foreach (SupportingTokenAuthenticatorSpecification specification in this.channelSupportingTokenAuthenticatorSpecification)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(specification.TokenAuthenticator);
                }
                foreach (string str in this.scopedSupportingTokenAuthenticatorSpecification.Keys)
                {
                    ICollection<SupportingTokenAuthenticatorSpecification> is2 = this.scopedSupportingTokenAuthenticatorSpecification[str];
                    foreach (SupportingTokenAuthenticatorSpecification specification2 in is2)
                    {
                        System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(specification2.TokenAuthenticator);
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
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.actAsInitiator)
            {
                foreach (SupportingTokenAuthenticatorSpecification specification in this.channelSupportingTokenAuthenticatorSpecification)
                {
                    System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(specification.TokenAuthenticator, helper.RemainingTime());
                }
                foreach (string str in this.scopedSupportingTokenAuthenticatorSpecification.Keys)
                {
                    ICollection<SupportingTokenAuthenticatorSpecification> is2 = this.scopedSupportingTokenAuthenticatorSpecification[str];
                    foreach (SupportingTokenAuthenticatorSpecification specification2 in is2)
                    {
                        System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(specification2.TokenAuthenticator, helper.RemainingTime());
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

        protected abstract SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout);
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

        public virtual void OnOpen(TimeSpan timeout)
        {
            if (this.SecurityBindingElement == null)
            {
                this.OnPropertySettingsError("SecurityBindingElement", true);
            }
            if (this.SecurityTokenManager == null)
            {
                this.OnPropertySettingsError("SecurityTokenManager", true);
            }
            this.messageSecurityVersion = this.standardsManager.MessageSecurityVersion;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.expectOutgoingMessages = this.ActAsInitiator || this.SupportsRequestReply;
            this.expectIncomingMessages = !this.ActAsInitiator || this.SupportsRequestReply;
            if (!this.actAsInitiator)
            {
                this.AddSupportingTokenAuthenticators(this.securityBindingElement.EndpointSupportingTokenParameters, false, (IList<SupportingTokenAuthenticatorSpecification>) this.channelSupportingTokenAuthenticatorSpecification);
                this.AddSupportingTokenAuthenticators(this.securityBindingElement.OptionalEndpointSupportingTokenParameters, true, (IList<SupportingTokenAuthenticatorSpecification>) this.channelSupportingTokenAuthenticatorSpecification);
                foreach (string str in this.securityBindingElement.OperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenAuthenticatorSpecification> authenticatorSpecList = new Collection<SupportingTokenAuthenticatorSpecification>();
                    this.AddSupportingTokenAuthenticators(this.securityBindingElement.OperationSupportingTokenParameters[str], false, authenticatorSpecList);
                    this.scopedSupportingTokenAuthenticatorSpecification.Add(str, authenticatorSpecList);
                }
                foreach (string str2 in this.securityBindingElement.OptionalOperationSupportingTokenParameters.Keys)
                {
                    Collection<SupportingTokenAuthenticatorSpecification> collection2;
                    ICollection<SupportingTokenAuthenticatorSpecification> is2;
                    if (this.scopedSupportingTokenAuthenticatorSpecification.TryGetValue(str2, out is2))
                    {
                        collection2 = (Collection<SupportingTokenAuthenticatorSpecification>) is2;
                    }
                    else
                    {
                        collection2 = new Collection<SupportingTokenAuthenticatorSpecification>();
                        this.scopedSupportingTokenAuthenticatorSpecification.Add(str2, collection2);
                    }
                    this.AddSupportingTokenAuthenticators(this.securityBindingElement.OptionalOperationSupportingTokenParameters[str2], true, collection2);
                }
                if (!this.channelSupportingTokenAuthenticatorSpecification.IsReadOnly)
                {
                    if (this.channelSupportingTokenAuthenticatorSpecification.Count == 0)
                    {
                        this.channelSupportingTokenAuthenticatorSpecification = EmptyTokenAuthenticators;
                    }
                    else
                    {
                        this.expectSupportingTokens = true;
                        foreach (SupportingTokenAuthenticatorSpecification specification in this.channelSupportingTokenAuthenticatorSpecification)
                        {
                            System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(specification.TokenAuthenticator, helper.RemainingTime());
                            if (((specification.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (specification.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)) && (specification.TokenParameters.RequireDerivedKeys && !specification.TokenParameters.HasAsymmetricKey))
                            {
                                this.expectKeyDerivation = true;
                            }
                            SecurityTokenAttachmentMode securityTokenAttachmentMode = specification.SecurityTokenAttachmentMode;
                            switch (securityTokenAttachmentMode)
                            {
                                case SecurityTokenAttachmentMode.SignedEncrypted:
                                case SecurityTokenAttachmentMode.Signed:
                                case SecurityTokenAttachmentMode.SignedEndorsing:
                                    this.expectChannelSignedTokens = true;
                                    if (securityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEncrypted)
                                    {
                                        this.expectChannelBasicTokens = true;
                                    }
                                    break;
                            }
                            if ((securityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (securityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing))
                            {
                                this.expectChannelEndorsingTokens = true;
                            }
                        }
                        this.channelSupportingTokenAuthenticatorSpecification = new ReadOnlyCollection<SupportingTokenAuthenticatorSpecification>((Collection<SupportingTokenAuthenticatorSpecification>) this.channelSupportingTokenAuthenticatorSpecification);
                    }
                }
                this.VerifyTypeUniqueness(this.channelSupportingTokenAuthenticatorSpecification);
                this.MergeSupportingTokenAuthenticators(helper.RemainingTime());
            }
            if (this.DetectReplays)
            {
                if (!this.SupportsReplayDetection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("DetectReplays", System.ServiceModel.SR.GetString("SecurityProtocolCannotDoReplayDetection", new object[] { this }));
                }
                if ((this.MaxClockSkew == TimeSpan.MaxValue) || (this.ReplayWindow == TimeSpan.MaxValue))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoncesCachedInfinitely")));
                }
                this.nonceCache = new System.ServiceModel.Security.NonceCache((this.ReplayWindow + this.MaxClockSkew) + this.MaxClockSkew, this.MaxCachedNonces);
            }
            this.derivedKeyTokenAuthenticator = new NonValidatingSecurityTokenAuthenticator<DerivedKeySecurityToken>();
        }

        public void OnOpened()
        {
        }

        public void OnOpening()
        {
        }

        internal void OnPropertySettingsError(string propertyName, bool requiredForForwardDirection)
        {
            if (requiredForForwardDirection)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PropertySettingErrorOnProtocolFactory", new object[] { propertyName, this }), propertyName));
            }
            if (this.requestReplyErrorPropertyName == null)
            {
                this.requestReplyErrorPropertyName = propertyName;
            }
        }

        public void Open(bool actAsInitiator, TimeSpan timeout)
        {
            this.actAsInitiator = actAsInitiator;
            this.communicationObject.Open(timeout);
        }

        internal void Open(string propertyName, bool requiredForForwardDirection, SecurityTokenAuthenticator authenticator, TimeSpan timeout)
        {
            if (authenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(authenticator, timeout);
            }
            else
            {
                this.OnPropertySettingsError(propertyName, requiredForForwardDirection);
            }
        }

        internal void Open(string propertyName, bool requiredForForwardDirection, SecurityTokenProvider provider, TimeSpan timeout)
        {
            if (provider != null)
            {
                System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(provider, timeout);
            }
            else
            {
                this.OnPropertySettingsError(propertyName, requiredForForwardDirection);
            }
        }

        internal void ThrowIfImmutable()
        {
            this.communicationObject.ThrowIfDisposedOrImmutable();
        }

        private void ThrowIfNotOpen()
        {
            this.communicationObject.ThrowIfNotOpened();
        }

        private void ThrowIfReturnDirectionSecurityNotSupported()
        {
            if (this.requestReplyErrorPropertyName != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PropertySettingErrorOnProtocolFactory", new object[] { this.requestReplyErrorPropertyName, this }), this.requestReplyErrorPropertyName));
            }
        }

        private void VerifyTypeUniqueness(ICollection<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            foreach (SupportingTokenAuthenticatorSpecification specification in supportingTokenAuthenticators)
            {
                System.Type c = specification.TokenAuthenticator.GetType();
                int num = 0;
                foreach (SupportingTokenAuthenticatorSpecification specification2 in supportingTokenAuthenticators)
                {
                    System.Type type = specification2.TokenAuthenticator.GetType();
                    if (object.ReferenceEquals(specification, specification2))
                    {
                        if (num > 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MultipleSupportingAuthenticatorsOfSameType", new object[] { specification.TokenParameters.GetType() })));
                        }
                        num++;
                    }
                    else if (c.IsAssignableFrom(type) || type.IsAssignableFrom(c))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MultipleSupportingAuthenticatorsOfSameType", new object[] { specification.TokenParameters.GetType() })));
                    }
                }
            }
        }

        public bool ActAsInitiator
        {
            get
            {
                return this.actAsInitiator;
            }
        }

        public bool AddTimestamp
        {
            get
            {
                return this.addTimestamp;
            }
            set
            {
                this.ThrowIfImmutable();
                this.addTimestamp = value;
            }
        }

        public System.ServiceModel.AuditLogLocation AuditLogLocation
        {
            get
            {
                return this.auditLogLocation;
            }
            set
            {
                this.ThrowIfImmutable();
                AuditLogLocationHelper.Validate(value);
                this.auditLogLocation = value;
            }
        }

        public ICollection<SupportingTokenAuthenticatorSpecification> ChannelSupportingTokenAuthenticatorSpecification
        {
            get
            {
                return this.channelSupportingTokenAuthenticatorSpecification;
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

        internal NonValidatingSecurityTokenAuthenticator<DerivedKeySecurityToken> DerivedKeyTokenAuthenticator
        {
            get
            {
                return this.derivedKeyTokenAuthenticator;
            }
        }

        public bool DetectReplays
        {
            get
            {
                return this.detectReplays;
            }
            set
            {
                this.ThrowIfImmutable();
                this.detectReplays = value;
            }
        }

        private static ReadOnlyCollection<SupportingTokenAuthenticatorSpecification> EmptyTokenAuthenticators
        {
            get
            {
                if (emptyTokenAuthenticators == null)
                {
                    emptyTokenAuthenticators = Array.AsReadOnly<SupportingTokenAuthenticatorSpecification>(new SupportingTokenAuthenticatorSpecification[0]);
                }
                return emptyTokenAuthenticators;
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
                this.ThrowIfImmutable();
                this.endpointFilterTable = value;
            }
        }

        internal bool ExpectIncomingMessages
        {
            get
            {
                return this.expectIncomingMessages;
            }
        }

        internal bool ExpectKeyDerivation
        {
            get
            {
                return this.expectKeyDerivation;
            }
            set
            {
                this.expectKeyDerivation = value;
            }
        }

        internal bool ExpectOutgoingMessages
        {
            get
            {
                return this.expectOutgoingMessages;
            }
        }

        internal bool ExpectSupportingTokens
        {
            get
            {
                return this.expectSupportingTokens;
            }
            set
            {
                this.expectSupportingTokens = value;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
            set
            {
                this.extendedProtectionPolicy = value;
            }
        }

        public SecurityAlgorithmSuite IncomingAlgorithmSuite
        {
            get
            {
                return this.incomingAlgorithmSuite;
            }
            set
            {
                this.ThrowIfImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.incomingAlgorithmSuite = value;
            }
        }

        internal bool IsDuplexReply
        {
            get
            {
                return this.isDuplexReply;
            }
            set
            {
                this.isDuplexReply = value;
            }
        }

        protected bool IsReadOnly
        {
            get
            {
                return (this.CommunicationObject.State != CommunicationState.Created);
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
                this.ThrowIfImmutable();
                this.listenUri = value;
            }
        }

        public int MaxCachedNonces
        {
            get
            {
                return this.maxCachedNonces;
            }
            set
            {
                this.ThrowIfImmutable();
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.maxCachedNonces = value;
            }
        }

        public TimeSpan MaxClockSkew
        {
            get
            {
                return this.maxClockSkew;
            }
            set
            {
                this.ThrowIfImmutable();
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.maxClockSkew = value;
            }
        }

        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return this.messageAuthenticationAuditLevel;
            }
            set
            {
                this.ThrowIfImmutable();
                AuditLevelHelper.Validate(value);
                this.messageAuthenticationAuditLevel = value;
            }
        }

        internal System.ServiceModel.MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return this.messageSecurityVersion;
            }
        }

        public System.ServiceModel.Security.NonceCache NonceCache
        {
            get
            {
                return this.nonceCache;
            }
        }

        public SecurityAlgorithmSuite OutgoingAlgorithmSuite
        {
            get
            {
                return this.outgoingAlgorithmSuite;
            }
            set
            {
                this.ThrowIfImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.outgoingAlgorithmSuite = value;
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
                this.ThrowIfImmutable();
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
                this.ThrowIfImmutable();
                this.privacyNoticeVersion = value;
            }
        }

        public TimeSpan ReplayWindow
        {
            get
            {
                return this.replayWindow;
            }
            set
            {
                this.ThrowIfImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                this.replayWindow = value;
            }
        }

        public Dictionary<string, ICollection<SupportingTokenAuthenticatorSpecification>> ScopedSupportingTokenAuthenticatorSpecification
        {
            get
            {
                return this.scopedSupportingTokenAuthenticatorSpecification;
            }
        }

        public System.ServiceModel.Channels.SecurityBindingElement SecurityBindingElement
        {
            get
            {
                return this.securityBindingElement;
            }
            set
            {
                this.ThrowIfImmutable();
                if (value != null)
                {
                    value = (System.ServiceModel.Channels.SecurityBindingElement) value.Clone();
                }
                this.securityBindingElement = value;
            }
        }

        public System.ServiceModel.Channels.SecurityHeaderLayout SecurityHeaderLayout
        {
            get
            {
                return this.securityHeaderLayout;
            }
            set
            {
                this.ThrowIfImmutable();
                this.securityHeaderLayout = value;
            }
        }

        public System.IdentityModel.Selectors.SecurityTokenManager SecurityTokenManager
        {
            get
            {
                return this.securityTokenManager;
            }
            set
            {
                this.ThrowIfImmutable();
                this.securityTokenManager = value;
            }
        }

        public AuditLevel ServiceAuthorizationAuditLevel
        {
            get
            {
                return this.serviceAuthorizationAuditLevel;
            }
            set
            {
                this.ThrowIfImmutable();
                AuditLevelHelper.Validate(value);
                this.serviceAuthorizationAuditLevel = value;
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
                this.ThrowIfImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.standardsManager = value;
            }
        }

        public virtual bool SupportsDuplex
        {
            get
            {
                return false;
            }
        }

        public virtual bool SupportsReplayDetection
        {
            get
            {
                return true;
            }
        }

        public virtual bool SupportsRequestReply
        {
            get
            {
                return true;
            }
        }

        public bool SuppressAuditFailure
        {
            get
            {
                return this.suppressAuditFailure;
            }
            set
            {
                this.ThrowIfImmutable();
                this.suppressAuditFailure = value;
            }
        }

        public TimeSpan TimestampValidityDuration
        {
            get
            {
                return this.timestampValidityDuration;
            }
            set
            {
                this.ThrowIfImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                this.timestampValidityDuration = value;
            }
        }
    }
}

