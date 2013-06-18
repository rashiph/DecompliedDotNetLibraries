namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.ComIntegration;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Xml;

    public class IssuedSecurityTokenProvider : SecurityTokenProvider, ICommunicationObject
    {
        private CoreFederatedTokenProvider federatedTokenProvider;
        private System.ServiceModel.MessageSecurityVersion messageSecurityVersion;
        private System.IdentityModel.Selectors.SecurityTokenSerializer securityTokenSerializer;

        public event EventHandler Closed
        {
            add
            {
                this.federatedTokenProvider.Closed += value;
            }
            remove
            {
                this.federatedTokenProvider.Closed -= value;
            }
        }

        public event EventHandler Closing
        {
            add
            {
                this.federatedTokenProvider.Closing += value;
            }
            remove
            {
                this.federatedTokenProvider.Closing -= value;
            }
        }

        public event EventHandler Faulted
        {
            add
            {
                this.federatedTokenProvider.Faulted += value;
            }
            remove
            {
                this.federatedTokenProvider.Faulted -= value;
            }
        }

        public event EventHandler Opened
        {
            add
            {
                this.federatedTokenProvider.Opened += value;
            }
            remove
            {
                this.federatedTokenProvider.Opened -= value;
            }
        }

        public event EventHandler Opening
        {
            add
            {
                this.federatedTokenProvider.Opening += value;
            }
            remove
            {
                this.federatedTokenProvider.Opening -= value;
            }
        }

        public IssuedSecurityTokenProvider() : this(null)
        {
        }

        internal IssuedSecurityTokenProvider(SafeFreeCredentials credentialsHandle)
        {
            this.federatedTokenProvider = new CoreFederatedTokenProvider(credentialsHandle);
            this.messageSecurityVersion = System.ServiceModel.MessageSecurityVersion.Default;
        }

        public void Abort()
        {
            this.federatedTokenProvider.Abort();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.federatedTokenProvider.BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.federatedTokenProvider.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.federatedTokenProvider.BeginGetToken(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            this.OnOpenCore();
            return this.federatedTokenProvider.BeginOpen(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpenCore();
            return this.federatedTokenProvider.BeginOpen(timeout, callback, state);
        }

        public void Close()
        {
            this.federatedTokenProvider.Close();
        }

        public void Close(TimeSpan timeout)
        {
            this.federatedTokenProvider.Close(timeout);
        }

        public void Dispose()
        {
            this.Close();
        }

        public void EndClose(IAsyncResult result)
        {
            this.federatedTokenProvider.EndClose(result);
        }

        protected override SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            return this.federatedTokenProvider.EndGetToken(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.federatedTokenProvider.EndOpen(result);
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return this.federatedTokenProvider.GetToken(timeout);
        }

        private void OnOpenCore()
        {
            if (this.securityTokenSerializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TokenSerializerNotSetonFederationProvider")));
            }
            this.federatedTokenProvider.StandardsManager = new SecurityStandardsManager(this.messageSecurityVersion, this.securityTokenSerializer);
        }

        public void Open()
        {
            this.OnOpenCore();
            this.federatedTokenProvider.Open();
        }

        public void Open(TimeSpan timeout)
        {
            this.OnOpenCore();
            this.federatedTokenProvider.Open(timeout);
        }

        public bool CacheIssuedTokens
        {
            get
            {
                return this.federatedTokenProvider.CacheServiceTokens;
            }
            set
            {
                this.federatedTokenProvider.CacheServiceTokens = value;
            }
        }

        internal ChannelParameterCollection ChannelParameters
        {
            get
            {
                return this.federatedTokenProvider.ChannelParameters;
            }
            set
            {
                this.federatedTokenProvider.ChannelParameters = value;
            }
        }

        public virtual TimeSpan DefaultCloseTimeout
        {
            get
            {
                return ServiceDefaults.CloseTimeout;
            }
        }

        public virtual TimeSpan DefaultOpenTimeout
        {
            get
            {
                return ServiceDefaults.OpenTimeout;
            }
        }

        public System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.federatedTokenProvider.IdentityVerifier;
            }
            set
            {
                this.federatedTokenProvider.IdentityVerifier = value;
            }
        }

        public int IssuedTokenRenewalThresholdPercentage
        {
            get
            {
                return this.federatedTokenProvider.ServiceTokenValidityThresholdPercentage;
            }
            set
            {
                this.federatedTokenProvider.ServiceTokenValidityThresholdPercentage = value;
            }
        }

        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.federatedTokenProvider.IssuerAddress;
            }
            set
            {
                this.federatedTokenProvider.IssuerAddress = value;
            }
        }

        public Binding IssuerBinding
        {
            get
            {
                return this.federatedTokenProvider.IssuerBinding;
            }
            set
            {
                this.federatedTokenProvider.IssuerBinding = value;
            }
        }

        public KeyedByTypeCollection<IEndpointBehavior> IssuerChannelBehaviors
        {
            get
            {
                return this.federatedTokenProvider.IssuerChannelBehaviors;
            }
        }

        public SecurityKeyEntropyMode KeyEntropyMode
        {
            get
            {
                return this.federatedTokenProvider.KeyEntropyMode;
            }
            set
            {
                this.federatedTokenProvider.KeyEntropyMode = value;
            }
        }

        public TimeSpan MaxIssuedTokenCachingTime
        {
            get
            {
                return this.federatedTokenProvider.MaxServiceTokenCachingTime;
            }
            set
            {
                this.federatedTokenProvider.MaxServiceTokenCachingTime = value;
            }
        }

        public System.ServiceModel.MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return this.messageSecurityVersion;
            }
            set
            {
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.messageSecurityVersion = value;
            }
        }

        public System.ServiceModel.Security.SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.federatedTokenProvider.SecurityAlgorithmSuite;
            }
            set
            {
                this.federatedTokenProvider.SecurityAlgorithmSuite = value;
            }
        }

        public System.IdentityModel.Selectors.SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return this.securityTokenSerializer;
            }
            set
            {
                this.securityTokenSerializer = value;
            }
        }

        public CommunicationState State
        {
            get
            {
                return this.federatedTokenProvider.State;
            }
        }

        public override bool SupportsTokenCancellation
        {
            get
            {
                return this.federatedTokenProvider.SupportsTokenCancellation;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return this.federatedTokenProvider.TargetAddress;
            }
            set
            {
                this.federatedTokenProvider.TargetAddress = value;
            }
        }

        public Collection<XmlElement> TokenRequestParameters
        {
            get
            {
                return this.federatedTokenProvider.RequestProperties;
            }
        }

        private class CoreFederatedTokenProvider : IssuanceTokenProviderBase<IssuedSecurityTokenProvider.FederatedTokenProviderState>
        {
            private bool addTargetServiceAppliesTo;
            private KeyedByTypeCollection<IEndpointBehavior> channelBehaviors;
            private IChannelFactory<IRequestChannel> channelFactory;
            private ChannelParameterCollection channelParameters;
            private SafeFreeCredentials credentialsHandle;
            internal const SecurityKeyEntropyMode defaultKeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
            private int defaultPublicKeySize = 0x400;
            private System.ServiceModel.Security.IdentityVerifier identityVerifier = System.ServiceModel.Security.IdentityVerifier.CreateDefault();
            private bool isKeySizePresentInRstProperties;
            private bool isKeyTypePresentInRstProperties;
            private Binding issuerBinding;
            private SecurityKeyEntropyMode keyEntropyMode;
            private int keySize;
            private SecurityKeyType keyType;
            private static int MaxRsaSecurityTokenCacheSize = 0x400;
            private System.ServiceModel.Channels.MessageVersion messageVersion;
            private bool ownCredentialsHandle;
            private Collection<XmlElement> requestProperties = new Collection<XmlElement>();
            private readonly List<RsaSecurityToken> rsaSecurityTokens = new List<RsaSecurityToken>();

            public CoreFederatedTokenProvider(SafeFreeCredentials credentialsHandle)
            {
                this.credentialsHandle = credentialsHandle;
                this.channelBehaviors = new KeyedByTypeCollection<IEndpointBehavior>();
                this.addTargetServiceAppliesTo = true;
                this.keyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
            }

            protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult<IssuedSecurityTokenProvider.FederatedTokenProviderState>(this.CreateNegotiationState(target, via, timeout), callback, state);
            }

            protected override IAsyncResult BeginInitializeChannelFactories(EndpointAddress target, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (this.channelFactory.State == CommunicationState.Created)
                {
                    return this.channelFactory.BeginOpen(timeout, callback, state);
                }
                return new CompletedAsyncResult(callback, state);
            }

            private void CleanUpRsaSecurityTokenCache()
            {
                lock (this.rsaSecurityTokens)
                {
                    for (int i = 0; i < this.rsaSecurityTokens.Count; i++)
                    {
                        this.rsaSecurityTokens[i].Dispose();
                    }
                    this.rsaSecurityTokens.Clear();
                }
            }

            private RsaSecurityToken CreateAndCacheRsaSecurityToken()
            {
                if ((MaxRsaSecurityTokenCacheSize >= 0) && this.IsImpersonatedContext())
                {
                    RsaSecurityToken item = RsaSecurityToken.CreateSafeRsaSecurityToken(this.keySize);
                    if (MaxRsaSecurityTokenCacheSize <= 0)
                    {
                        return item;
                    }
                    lock (this.rsaSecurityTokens)
                    {
                        if (this.rsaSecurityTokens.Count >= MaxRsaSecurityTokenCacheSize)
                        {
                            this.rsaSecurityTokens.RemoveAt(0);
                        }
                        this.rsaSecurityTokens.Add(item);
                        return item;
                    }
                }
                return new RsaSecurityToken(new RSACryptoServiceProvider(this.keySize));
            }

            protected override IRequestChannel CreateClientChannel(EndpointAddress target, Uri via)
            {
                IRequestChannel innerChannel = this.channelFactory.CreateChannel(base.IssuerAddress);
                if (this.channelParameters != null)
                {
                    this.channelParameters.PropagateChannelParameters(innerChannel);
                }
                if (this.ownCredentialsHandle)
                {
                    ChannelParameterCollection property = innerChannel.GetProperty<ChannelParameterCollection>();
                    if (property != null)
                    {
                        property.Add(new SspiIssuanceChannelParameter(true, this.credentialsHandle));
                    }
                }
                this.ReplaceSspiIssuanceChannelParameter(innerChannel.GetProperty<ChannelParameterCollection>(), new SspiIssuanceChannelParameter(true, this.credentialsHandle));
                return innerChannel;
            }

            protected override IssuedSecurityTokenProvider.FederatedTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
            {
                if ((this.keyType == SecurityKeyType.SymmetricKey) || (this.keyType == SecurityKeyType.BearerKey))
                {
                    byte[] buffer;
                    if ((this.KeyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy) || (this.KeyEntropyMode == SecurityKeyEntropyMode.ClientEntropy))
                    {
                        buffer = new byte[this.keySize / 8];
                        System.ServiceModel.Security.CryptoHelper.FillRandomBytes(buffer);
                    }
                    else
                    {
                        buffer = null;
                    }
                    return new IssuedSecurityTokenProvider.FederatedTokenProviderState(buffer);
                }
                if (this.keyType != SecurityKeyType.AsymmetricKey)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                return new IssuedSecurityTokenProvider.FederatedTokenProviderState(this.CreateAndCacheRsaSecurityToken());
            }

            protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
            {
                return true;
            }

            protected override IssuedSecurityTokenProvider.FederatedTokenProviderState EndCreateNegotiationState(IAsyncResult result)
            {
                return CompletedAsyncResult<IssuedSecurityTokenProvider.FederatedTokenProviderState>.End(result);
            }

            protected override void EndInitializeChannelFactories(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    this.channelFactory.EndOpen(result);
                }
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

            protected override BodyWriter GetFirstOutgoingMessageBody(IssuedSecurityTokenProvider.FederatedTokenProviderState negotiationState, out MessageProperties messageProperties)
            {
                messageProperties = null;
                RequestSecurityToken token = new RequestSecurityToken(base.StandardsManager);
                if (this.addTargetServiceAppliesTo)
                {
                    if (this.MessageVersion.Addressing != AddressingVersion.WSAddressing10)
                    {
                        if (this.MessageVersion.Addressing != AddressingVersion.WSAddressingAugust2004)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { this.MessageVersion.Addressing })));
                        }
                        token.SetAppliesTo<EndpointAddressAugust2004>(EndpointAddressAugust2004.FromEndpointAddress(negotiationState.TargetAddress), DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddressAugust2004), 0x10000));
                    }
                    else
                    {
                        token.SetAppliesTo<EndpointAddress10>(EndpointAddress10.FromEndpointAddress(negotiationState.TargetAddress), DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddress10), 0x10000));
                    }
                }
                token.Context = negotiationState.Context;
                if (!this.isKeySizePresentInRstProperties)
                {
                    token.KeySize = this.keySize;
                }
                Collection<XmlElement> collection = new Collection<XmlElement>();
                if (this.requestProperties != null)
                {
                    for (int i = 0; i < this.requestProperties.Count; i++)
                    {
                        collection.Add(this.requestProperties[i]);
                    }
                }
                if (!this.isKeyTypePresentInRstProperties)
                {
                    XmlElement item = base.StandardsManager.TrustDriver.CreateKeyTypeElement(this.keyType);
                    collection.Insert(0, item);
                }
                if (this.keyType == SecurityKeyType.SymmetricKey)
                {
                    byte[] requestorEntropy = negotiationState.GetRequestorEntropy();
                    token.SetRequestorEntropy(requestorEntropy);
                }
                else if (this.keyType == SecurityKeyType.AsymmetricKey)
                {
                    RsaKeyIdentifierClause clause = new RsaKeyIdentifierClause(negotiationState.Rsa);
                    SecurityKeyIdentifier keyIdentifier = new SecurityKeyIdentifier(new SecurityKeyIdentifierClause[] { clause });
                    collection.Add(base.StandardsManager.TrustDriver.CreateUseKeyElement(keyIdentifier, base.StandardsManager));
                    RsaSecurityTokenParameters tokenParameters = new RsaSecurityTokenParameters {
                        InclusionMode = SecurityTokenInclusionMode.Never,
                        RequireDerivedKeys = false
                    };
                    SupportingTokenSpecification specification = new SupportingTokenSpecification(negotiationState.RsaSecurityToken, System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, SecurityTokenAttachmentMode.Endorsing, tokenParameters);
                    messageProperties = new MessageProperties();
                    SecurityMessageProperty property = new SecurityMessageProperty {
                        OutgoingSupportingTokens = { specification }
                    };
                    messageProperties.Security = property;
                }
                if ((this.keyType == SecurityKeyType.SymmetricKey) && (this.KeyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy))
                {
                    collection.Add(base.StandardsManager.TrustDriver.CreateComputedKeyAlgorithmElement(base.StandardsManager.TrustDriver.ComputedKeyAlgorithm));
                }
                token.RequestProperties = collection;
                token.MakeReadOnly();
                return token;
            }

            protected override BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, IssuedSecurityTokenProvider.FederatedTokenProviderState negotiationState)
            {
                GenericXmlSecurityToken token;
                IssuanceTokenProviderBase<IssuedSecurityTokenProvider.FederatedTokenProviderState>.ThrowIfFault(incomingMessage, base.IssuerAddress);
                if ((((base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005) && (incomingMessage.Headers.Action != base.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction.Value)) || ((base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13) && (incomingMessage.Headers.Action != base.StandardsManager.TrustDriver.RequestSecurityTokenResponseFinalAction.Value))) || (incomingMessage.Headers.Action == null))
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidActionForNegotiationMessage", new object[] { incomingMessage.Headers.Action })), incomingMessage);
                }
                RequestSecurityTokenResponse response = null;
                XmlDictionaryReader readerAtBodyContents = incomingMessage.GetReaderAtBodyContents();
                using (readerAtBodyContents)
                {
                    if (base.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                    {
                        response = base.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponse(readerAtBodyContents);
                    }
                    else
                    {
                        if (base.StandardsManager.MessageSecurityVersion.TrustVersion != TrustVersion.WSTrust13)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                        }
                        foreach (RequestSecurityTokenResponse response2 in base.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(readerAtBodyContents).RstrCollection)
                        {
                            if (response != null)
                            {
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MoreThanOneRSTRInRSTRC")));
                            }
                            response = response2;
                        }
                    }
                    incomingMessage.ReadFromBodyContentsToEnd(readerAtBodyContents);
                }
                if (response.Context != negotiationState.Context)
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("BadSecurityNegotiationContext")), incomingMessage);
                }
                if ((this.keyType == SecurityKeyType.SymmetricKey) || (this.keyType == SecurityKeyType.BearerKey))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> serviceAuthorizationPolicies = this.GetServiceAuthorizationPolicies(negotiationState);
                    byte[] requestorEntropy = negotiationState.GetRequestorEntropy();
                    token = response.GetIssuedToken(null, null, this.KeyEntropyMode, requestorEntropy, null, serviceAuthorizationPolicies, this.keySize, this.keyType == SecurityKeyType.BearerKey);
                }
                else
                {
                    if (this.keyType != SecurityKeyType.AsymmetricKey)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }
                    token = response.GetIssuedToken(null, System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, negotiationState.Rsa);
                }
                negotiationState.SetServiceToken(token);
                return null;
            }

            protected ReadOnlyCollection<IAuthorizationPolicy> GetServiceAuthorizationPolicies(AcceleratedTokenProviderState negotiationState)
            {
                EndpointIdentity identity;
                if (this.identityVerifier.TryGetIdentity(negotiationState.TargetAddress, out identity))
                {
                    List<Claim> claims = new List<Claim>(1) {
                        identity.IdentityClaim
                    };
                    return new List<IAuthorizationPolicy>(1) { new UnconditionalPolicy(System.ServiceModel.Security.SecurityUtils.CreateIdentity(identity.IdentityClaim.Resource.ToString()), new DefaultClaimSet(ClaimSet.System, claims)) }.AsReadOnly();
                }
                return System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }

            protected override void InitializeChannelFactories(EndpointAddress target, TimeSpan timeout)
            {
                if (this.channelFactory.State == CommunicationState.Created)
                {
                    this.channelFactory.Open(timeout);
                }
            }

            private bool IsImpersonatedContext()
            {
                SafeCloseHandle tokenHandle = null;
                if (!System.ServiceModel.ComIntegration.SafeNativeMethods.OpenCurrentThreadToken(System.ServiceModel.ComIntegration.SafeNativeMethods.GetCurrentThread(), TokenAccessLevels.Query, true, out tokenHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    Utility.CloseInvalidOutSafeHandle(tokenHandle);
                    if (error == 0x3f0)
                    {
                        return false;
                    }
                    ErrorBehavior.ThrowAndCatch(new Win32Exception(error));
                    return true;
                }
                tokenHandle.Close();
                return true;
            }

            public override void OnAbort()
            {
                if ((this.channelFactory != null) && (this.channelFactory.State == CommunicationState.Opened))
                {
                    this.channelFactory.Abort();
                    this.channelFactory = null;
                }
                this.CleanUpRsaSecurityTokenCache();
                this.FreeCredentialsHandle();
                base.OnAbort();
            }

            public override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if ((this.channelFactory != null) && (this.channelFactory.State == CommunicationState.Opened))
                {
                    this.channelFactory.Close(helper.RemainingTime());
                    this.channelFactory = null;
                    this.CleanUpRsaSecurityTokenCache();
                    this.FreeCredentialsHandle();
                    base.OnClose(helper.RemainingTime());
                }
            }

            public override void OnOpen(TimeSpan timeout)
            {
                if (base.IssuerAddress == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StsAddressNotSet", new object[] { base.TargetAddress })));
                }
                if (this.IssuerBinding == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StsBindingNotSet", new object[] { base.IssuerAddress })));
                }
                if (base.SecurityAlgorithmSuite == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityAlgorithmSuiteNotSet", new object[] { typeof(IssuedSecurityTokenProvider) })));
                }
                this.channelFactory = base.StandardsManager.TrustDriver.CreateFederationProxy(base.IssuerAddress, this.IssuerBinding, this.IssuerChannelBehaviors);
                this.messageVersion = this.IssuerBinding.MessageVersion;
                for (int i = 0; i < this.requestProperties.Count; i++)
                {
                    if (base.StandardsManager.TrustDriver.IsAppliesTo(this.requestProperties[i].LocalName, this.requestProperties[i].NamespaceURI))
                    {
                        this.addTargetServiceAppliesTo = false;
                        break;
                    }
                }
                this.isKeyTypePresentInRstProperties = this.TryGetKeyType(out this.keyType);
                if (!this.isKeyTypePresentInRstProperties)
                {
                    this.keyType = SecurityKeyType.SymmetricKey;
                }
                this.isKeySizePresentInRstProperties = this.TryGetKeySize(out this.keySize);
                if (!this.isKeySizePresentInRstProperties && (this.keyType != SecurityKeyType.BearerKey))
                {
                    this.keySize = (this.keyType == SecurityKeyType.SymmetricKey) ? base.SecurityAlgorithmSuite.DefaultSymmetricKeyLength : this.defaultPublicKeySize;
                }
                base.OnOpen(timeout);
            }

            public override void OnOpening()
            {
                base.OnOpening();
                if (this.credentialsHandle == null)
                {
                    if (this.IssuerBinding == null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StsBindingNotSet", new object[] { base.IssuerAddress })));
                    }
                    this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle(this.IssuerBinding, this.IssuerChannelBehaviors);
                    this.ownCredentialsHandle = true;
                }
            }

            private void ReplaceSspiIssuanceChannelParameter(ChannelParameterCollection channelParameters, SspiIssuanceChannelParameter sicp)
            {
                if (channelParameters != null)
                {
                    for (int i = 0; i < channelParameters.Count; i++)
                    {
                        if (channelParameters[i] is SspiIssuanceChannelParameter)
                        {
                            channelParameters.RemoveAt(i);
                        }
                    }
                    channelParameters.Add(sicp);
                }
            }

            private bool TryGetKeySize(out int keySize)
            {
                if (this.requestProperties != null)
                {
                    for (int i = 0; i < this.requestProperties.Count; i++)
                    {
                        if (base.StandardsManager.TrustDriver.TryParseKeySizeElement(this.requestProperties[i], out keySize))
                        {
                            return true;
                        }
                    }
                }
                keySize = 0;
                return false;
            }

            private bool TryGetKeyType(out SecurityKeyType keyType)
            {
                if (this.requestProperties != null)
                {
                    for (int i = 0; i < this.requestProperties.Count; i++)
                    {
                        if (base.StandardsManager.TrustDriver.TryParseKeyTypeElement(this.requestProperties[i], out keyType))
                        {
                            return true;
                        }
                    }
                }
                keyType = SecurityKeyType.SymmetricKey;
                return false;
            }

            protected override void ValidateKeySize(GenericXmlSecurityToken issuedToken)
            {
                if (this.keyType != SecurityKeyType.BearerKey)
                {
                    base.ValidateKeySize(issuedToken);
                }
            }

            protected override bool WillInitializeChannelFactoriesCompleteSynchronously(EndpointAddress target)
            {
                return (this.channelFactory.State != CommunicationState.Opened);
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

            public System.ServiceModel.Security.IdentityVerifier IdentityVerifier
            {
                get
                {
                    return this.identityVerifier;
                }
                set
                {
                    base.CommunicationObject.ThrowIfDisposedOrImmutable();
                    this.identityVerifier = value;
                }
            }

            protected override bool IsMultiLegNegotiation
            {
                get
                {
                    return false;
                }
            }

            public Binding IssuerBinding
            {
                get
                {
                    return this.issuerBinding;
                }
                set
                {
                    base.CommunicationObject.ThrowIfDisposedOrImmutable();
                    this.issuerBinding = value;
                }
            }

            public KeyedByTypeCollection<IEndpointBehavior> IssuerChannelBehaviors
            {
                get
                {
                    return this.channelBehaviors;
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

            protected override System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }

            public Collection<XmlElement> RequestProperties
            {
                get
                {
                    return this.requestProperties;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenAction
            {
                get
                {
                    return base.StandardsManager.TrustDriver.RequestSecurityTokenAction;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenResponseAction
            {
                get
                {
                    return base.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction;
                }
            }

            protected override bool RequiresManualReplyAddressing
            {
                get
                {
                    return false;
                }
            }
        }

        private class FederatedTokenProviderState : AcceleratedTokenProviderState
        {
            private System.IdentityModel.Tokens.RsaSecurityToken rsaToken;

            public FederatedTokenProviderState(byte[] entropy) : base(entropy)
            {
            }

            public FederatedTokenProviderState(System.IdentityModel.Tokens.RsaSecurityToken rsaToken) : base(null)
            {
                this.rsaToken = rsaToken;
            }

            public RSA Rsa
            {
                get
                {
                    return this.rsaToken.Rsa;
                }
            }

            public System.IdentityModel.Tokens.RsaSecurityToken RsaSecurityToken
            {
                get
                {
                    return this.rsaToken;
                }
            }
        }
    }
}

