namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal abstract class IssuanceTokenProviderBase<T> : CommunicationObjectSecurityTokenProvider where T: IssuanceTokenProviderState
    {
        private System.ServiceModel.Security.SecurityAlgorithmSuite algorithmSuite;
        private ChannelProtectionRequirements applicationProtectionRequirements;
        private SecurityToken cachedToken;
        private bool cacheServiceTokens;
        internal const bool defaultClientCacheTokens = true;
        internal const string defaultClientMaxTokenCachingTimeString = "10675199.02:48:05.4775807";
        internal const int defaultServiceTokenValidityThresholdPercentage = 60;
        private EndpointAddress issuerAddress;
        private TimeSpan maxServiceTokenCachingTime;
        private string sctUri;
        private int serviceTokenValidityThresholdPercentage;
        private SecurityStandardsManager standardsManager;
        private EndpointAddress targetAddress;
        private object thisLock;
        private Uri via;

        protected IssuanceTokenProviderBase()
        {
            this.cacheServiceTokens = true;
            this.serviceTokenValidityThresholdPercentage = 60;
            this.thisLock = new object();
            this.cacheServiceTokens = true;
            this.serviceTokenValidityThresholdPercentage = 60;
            this.maxServiceTokenCachingTime = IssuanceTokenProviderBase<T>.DefaultClientMaxTokenCachingTime;
            this.standardsManager = null;
        }

        protected abstract IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state);
        protected override IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            lock (this.ThisLock)
            {
                SecurityToken currentServiceToken = this.GetCurrentServiceToken();
                if (currentServiceToken != null)
                {
                    SecurityTraceRecordHelper.TraceUsingCachedServiceToken<T>((IssuanceTokenProviderBase<T>) this, currentServiceToken, this.targetAddress);
                    return new CompletedAsyncResult<SecurityToken>(currentServiceToken, callback, state);
                }
                return this.BeginNegotiation(timeout, callback, state);
            }
        }

        protected abstract IAsyncResult BeginInitializeChannelFactories(EndpointAddress target, TimeSpan timeout, AsyncCallback callback, object state);
        protected IAsyncResult BeginNegotiation(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfClosedOrCreated();
            SecurityTraceRecordHelper.TraceBeginSecurityNegotiation<T>((IssuanceTokenProviderBase<T>) this, this.targetAddress);
            return new SecurityNegotiationAsyncResult<T>((IssuanceTokenProviderBase<T>) this, timeout, callback, state);
        }

        protected override void CancelTokenCore(TimeSpan timeout, SecurityToken token)
        {
            if (this.CacheServiceTokens)
            {
                lock (this.ThisLock)
                {
                    if (object.ReferenceEquals(token, this.cachedToken))
                    {
                        this.cachedToken = null;
                    }
                }
            }
        }

        private void Cleanup(IChannel rstChannel, T negotiationState)
        {
            if (negotiationState != null)
            {
                negotiationState.Dispose();
            }
            if (rstChannel != null)
            {
                rstChannel.Abort();
            }
        }

        protected abstract IRequestChannel CreateClientChannel(EndpointAddress target, Uri via);
        protected abstract T CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout);
        protected abstract bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via);
        protected SecurityToken DoNegotiation(TimeSpan timeout)
        {
            SecurityToken token2;
            this.ThrowIfClosedOrCreated();
            SecurityTraceRecordHelper.TraceBeginSecurityNegotiation<T>((IssuanceTokenProviderBase<T>) this, this.targetAddress);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            IRequestChannel rstChannel = null;
            T negotiationState = default(T);
            TimeSpan span = timeout;
            int num = 1;
            try
            {
                negotiationState = this.CreateNegotiationState(this.targetAddress, this.via, helper.RemainingTime());
                this.InitializeNegotiationState(negotiationState);
                this.InitializeChannelFactories(negotiationState.RemoteAddress, helper.RemainingTime());
                rstChannel = this.CreateClientChannel(negotiationState.RemoteAddress, this.via);
                rstChannel.Open(helper.RemainingTime());
                Message nextOutgoingMessage = null;
                Message incomingMessage = null;
                SecurityToken serviceToken = null;
                while (true)
                {
                    nextOutgoingMessage = this.GetNextOutgoingMessage(incomingMessage, negotiationState);
                    if (incomingMessage != null)
                    {
                        incomingMessage.Close();
                    }
                    if (nextOutgoingMessage == null)
                    {
                        break;
                    }
                    using (nextOutgoingMessage)
                    {
                        TraceUtility.ProcessOutgoingMessage(nextOutgoingMessage);
                        span = helper.RemainingTime();
                        incomingMessage = rstChannel.Request(nextOutgoingMessage, span);
                        if (incomingMessage == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("FailToRecieveReplyFromNegotiation")));
                        }
                        TraceUtility.ProcessIncomingMessage(incomingMessage);
                    }
                    num += 2;
                }
                if (!negotiationState.IsNegotiationCompleted)
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoNegotiationMessageToSend")), incomingMessage);
                }
                try
                {
                    rstChannel.Close(helper.RemainingTime());
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    rstChannel.Abort();
                }
                catch (TimeoutException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                    rstChannel.Abort();
                }
                rstChannel = null;
                this.ValidateAndCacheServiceToken(negotiationState);
                serviceToken = negotiationState.ServiceToken;
                SecurityTraceRecordHelper.TraceEndSecurityNegotiation<T>((IssuanceTokenProviderBase<T>) this, serviceToken, this.targetAddress);
                token2 = serviceToken;
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (exception3 is TimeoutException)
                {
                    exception3 = new TimeoutException(System.ServiceModel.SR.GetString("ClientSecurityNegotiationTimeout", new object[] { timeout, num, span }), exception3);
                }
                EndpointAddress targetAddress = (negotiationState == null) ? null : negotiationState.RemoteAddress;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(IssuanceTokenProviderBase<T>.WrapExceptionIfRequired(exception3, targetAddress, this.issuerAddress));
            }
            finally
            {
                this.Cleanup(rstChannel, negotiationState);
            }
            return token2;
        }

        protected abstract T EndCreateNegotiationState(IAsyncResult result);
        protected override SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<SecurityToken>)
            {
                return CompletedAsyncResult<SecurityToken>.End(result);
            }
            return this.EndNegotiation(result);
        }

        protected abstract void EndInitializeChannelFactories(IAsyncResult result);
        protected SecurityToken EndNegotiation(IAsyncResult result)
        {
            SecurityToken serviceToken = SecurityNegotiationAsyncResult<T>.End(result);
            SecurityTraceRecordHelper.TraceEndSecurityNegotiation<T>((IssuanceTokenProviderBase<T>) this, serviceToken, this.targetAddress);
            return serviceToken;
        }

        protected void EnsureEndpointAddressDoesNotRequireEncryption(EndpointAddress target)
        {
            if ((this.ApplicationProtectionRequirements != null) && (this.ApplicationProtectionRequirements.OutgoingEncryptionParts != null))
            {
                MessagePartSpecification channelParts = this.ApplicationProtectionRequirements.OutgoingEncryptionParts.ChannelParts;
                if (channelParts != null)
                {
                    for (int i = 0; i < this.targetAddress.Headers.Count; i++)
                    {
                        AddressHeader header = target.Headers[i];
                        if (channelParts.IsHeaderIncluded(header.Name, header.Namespace))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("SecurityNegotiationCannotProtectConfidentialEndpointHeader", new object[] { target, header.Name, header.Namespace })));
                        }
                    }
                }
            }
        }

        private SecurityToken GetCurrentServiceToken()
        {
            if ((this.CacheServiceTokens && (this.cachedToken != null)) && this.IsServiceTokenTimeValid(this.cachedToken))
            {
                return this.cachedToken;
            }
            return null;
        }

        protected abstract BodyWriter GetFirstOutgoingMessageBody(T negotiationState, out MessageProperties properties);
        private Message GetNextOutgoingMessage(Message incomingMessage, T negotiationState)
        {
            BodyWriter firstOutgoingMessageBody;
            MessageProperties properties = null;
            Message message;
            if (incomingMessage == null)
            {
                firstOutgoingMessageBody = this.GetFirstOutgoingMessageBody(negotiationState, out properties);
            }
            else
            {
                firstOutgoingMessageBody = this.GetNextOutgoingMessageBody(incomingMessage, negotiationState);
            }
            if (firstOutgoingMessageBody == null)
            {
                return null;
            }
            if (incomingMessage == null)
            {
                message = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.RequestSecurityTokenAction, this.MessageVersion.Addressing), firstOutgoingMessageBody);
            }
            else
            {
                message = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.RequestSecurityTokenResponseAction, this.MessageVersion.Addressing), firstOutgoingMessageBody);
            }
            if (properties != null)
            {
                message.Properties.CopyProperties(properties);
            }
            this.PrepareRequest(message, firstOutgoingMessageBody as RequestSecurityToken);
            return message;
        }

        protected abstract BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, T negotiationState);
        private DateTime GetServiceTokenEffectiveExpirationTime(SecurityToken serviceToken)
        {
            if (serviceToken.ValidTo.ToUniversalTime() >= System.ServiceModel.Security.SecurityUtils.MaxUtcDateTime)
            {
                return serviceToken.ValidTo;
            }
            TimeSpan span = (TimeSpan) (serviceToken.ValidTo.ToUniversalTime() - serviceToken.ValidFrom.ToUniversalTime());
            long ticks = span.Ticks;
            long num2 = Convert.ToInt64((((double) this.ServiceTokenValidityThresholdPercentage) / 100.0) * ticks, NumberFormatInfo.InvariantInfo);
            DateTime time = TimeoutHelper.Add(serviceToken.ValidFrom.ToUniversalTime(), new TimeSpan(num2));
            DateTime time2 = TimeoutHelper.Add(serviceToken.ValidFrom.ToUniversalTime(), this.MaxServiceTokenCachingTime);
            if (time <= time2)
            {
                return time;
            }
            return time2;
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            SecurityToken currentServiceToken;
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            lock (this.ThisLock)
            {
                currentServiceToken = this.GetCurrentServiceToken();
                if (currentServiceToken != null)
                {
                    SecurityTraceRecordHelper.TraceUsingCachedServiceToken<T>((IssuanceTokenProviderBase<T>) this, currentServiceToken, this.targetAddress);
                }
            }
            if (currentServiceToken == null)
            {
                currentServiceToken = this.DoNegotiation(timeout);
            }
            return currentServiceToken;
        }

        protected abstract void InitializeChannelFactories(EndpointAddress target, TimeSpan timeout);
        private void InitializeNegotiationState(T negotiationState)
        {
            negotiationState.TargetAddress = this.targetAddress;
            if ((negotiationState.Context == null) && this.IsMultiLegNegotiation)
            {
                negotiationState.Context = System.ServiceModel.Security.SecurityUtils.GenerateId();
            }
            if (this.IssuerAddress != null)
            {
                negotiationState.RemoteAddress = this.IssuerAddress;
            }
            else
            {
                negotiationState.RemoteAddress = negotiationState.TargetAddress;
            }
        }

        private bool IsServiceTokenTimeValid(SecurityToken serviceToken)
        {
            DateTime serviceTokenEffectiveExpirationTime = this.GetServiceTokenEffectiveExpirationTime(serviceToken);
            return (DateTime.UtcNow <= serviceTokenEffectiveExpirationTime);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.targetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TargetAddressIsNotSet", new object[] { base.GetType() })));
            }
            if (this.SecurityAlgorithmSuite == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityAlgorithmSuiteNotSet", new object[] { base.GetType() })));
            }
            this.sctUri = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
        }

        private void PrepareRequest(Message nextMessage)
        {
            this.PrepareRequest(nextMessage, null);
        }

        private void PrepareRequest(Message nextMessage, RequestSecurityToken rst)
        {
            if ((rst != null) && !rst.IsReadOnly)
            {
                rst.Message = nextMessage;
            }
            RequestReplyCorrelator.PrepareRequest(nextMessage);
            if (this.RequiresManualReplyAddressing)
            {
                nextMessage.Headers.ReplyTo = EndpointAddress.AnonymousAddress;
            }
        }

        private static bool ShouldWrapException(Exception e)
        {
            return (((((e is Win32Exception) || (e is XmlException)) || ((e is InvalidOperationException) || (e is ArgumentException))) || (((e is QuotaExceededException) || (e is SecurityException)) || (e is CryptographicException))) || (e is SecurityTokenException));
        }

        protected void ThrowIfClosedOrCreated()
        {
            base.CommunicationObject.ThrowIfClosed();
            this.ThrowIfCreated();
        }

        protected void ThrowIfCreated()
        {
            CommunicationState state = base.CommunicationObject.State;
            if (state == CommunicationState.Created)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("CommunicationObjectCannotBeUsed", new object[] { base.GetType().ToString(), state.ToString() }));
                throw TraceUtility.ThrowHelperError(exception, Guid.Empty, this);
            }
        }

        protected static void ThrowIfFault(Message message, EndpointAddress target)
        {
            System.ServiceModel.Security.SecurityUtils.ThrowIfNegotiationFault(message, target);
        }

        private void ValidateAndCacheServiceToken(T negotiationState)
        {
            this.ValidateKeySize(negotiationState.ServiceToken);
            lock (this.ThisLock)
            {
                if (this.CacheServiceTokens)
                {
                    this.cachedToken = negotiationState.ServiceToken;
                }
            }
        }

        protected virtual void ValidateKeySize(GenericXmlSecurityToken issuedToken)
        {
            if (this.SecurityAlgorithmSuite != null)
            {
                ReadOnlyCollection<SecurityKey> securityKeys = issuedToken.SecurityKeys;
                if ((securityKeys == null) || (securityKeys.Count != 1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("CannotObtainIssuedTokenKeySize")));
                }
                SymmetricSecurityKey key = securityKeys[0] as SymmetricSecurityKey;
                if ((key != null) && !this.SecurityAlgorithmSuite.IsSymmetricKeyLengthSupported(key.KeySize))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidIssuedTokenKeySize", new object[] { key.KeySize })));
                }
            }
        }

        protected abstract bool WillInitializeChannelFactoriesCompleteSynchronously(EndpointAddress target);
        private static Exception WrapExceptionIfRequired(Exception e, EndpointAddress targetAddress, EndpointAddress issuerAddress)
        {
            if (IssuanceTokenProviderBase<T>.ShouldWrapException(e))
            {
                Uri uri;
                Uri uri2;
                if (targetAddress != null)
                {
                    uri = targetAddress.Uri;
                }
                else
                {
                    uri = null;
                }
                if (issuerAddress != null)
                {
                    uri2 = issuerAddress.Uri;
                }
                else
                {
                    uri2 = uri;
                }
                if (uri != null)
                {
                    e = new SecurityNegotiationException(System.ServiceModel.SR.GetString("SoapSecurityNegotiationFailedForIssuerAndTarget", new object[] { uri2, uri }), e);
                    return e;
                }
                e = new SecurityNegotiationException(System.ServiceModel.SR.GetString("SoapSecurityNegotiationFailed"), e);
            }
            return e;
        }

        public ChannelProtectionRequirements ApplicationProtectionRequirements
        {
            get
            {
                return this.applicationProtectionRequirements;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.applicationProtectionRequirements = value;
            }
        }

        public bool CacheServiceTokens
        {
            get
            {
                return this.cacheServiceTokens;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.cacheServiceTokens = value;
            }
        }

        internal static TimeSpan DefaultClientMaxTokenCachingTime
        {
            get
            {
                return TimeSpan.MaxValue;
            }
        }

        protected virtual bool IsMultiLegNegotiation
        {
            get
            {
                return true;
            }
        }

        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.issuerAddress;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuerAddress = value;
            }
        }

        public TimeSpan MaxServiceTokenCachingTime
        {
            get
            {
                return this.maxServiceTokenCachingTime;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.maxServiceTokenCachingTime = value;
            }
        }

        protected abstract System.ServiceModel.Channels.MessageVersion MessageVersion { get; }

        public abstract XmlDictionaryString RequestSecurityTokenAction { get; }

        public abstract XmlDictionaryString RequestSecurityTokenResponseAction { get; }

        protected abstract bool RequiresManualReplyAddressing { get; }

        public System.ServiceModel.Security.SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.algorithmSuite;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.algorithmSuite = value;
            }
        }

        protected string SecurityContextTokenUri
        {
            get
            {
                this.ThrowIfCreated();
                return this.sctUri;
            }
        }

        public int ServiceTokenValidityThresholdPercentage
        {
            get
            {
                return this.serviceTokenValidityThresholdPercentage;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if ((value <= 0) || (value > 100))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 1, 100 })));
                }
                this.serviceTokenValidityThresholdPercentage = value;
            }
        }

        public SecurityStandardsManager StandardsManager
        {
            get
            {
                if (this.standardsManager == null)
                {
                    return SecurityStandardsManager.DefaultInstance;
                }
                return this.standardsManager;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.standardsManager = value;
            }
        }

        public override bool SupportsTokenCancellation
        {
            get
            {
                return true;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return this.targetAddress;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.targetAddress = value;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.via = value;
            }
        }

        private class SecurityNegotiationAsyncResult : AsyncResult
        {
            private static AsyncCallback closeChannelCallback;
            private static AsyncCallback createNegotiationStateCallback;
            private static AsyncCallback initializeChannelFactoriesCallback;
            private EndpointAddress issuer;
            private T negotiationState;
            private Message nextOutgoingMessage;
            private static AsyncCallback openChannelCallback;
            private IRequestChannel rstChannel;
            private static AsyncCallback sendRequestCallback;
            private SecurityToken serviceToken;
            private EndpointAddress target;
            private TimeSpan timeout;
            private TimeoutHelper timeoutHelper;
            private IssuanceTokenProviderBase<T> tokenProvider;
            private Uri via;

            static SecurityNegotiationAsyncResult()
            {
                IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.createNegotiationStateCallback = Fx.ThunkCallback(new AsyncCallback(IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.CreateNegotiationStateCallback));
                IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.initializeChannelFactoriesCallback = Fx.ThunkCallback(new AsyncCallback(IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.InitializeChannelFactoriesCallback));
                IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.closeChannelCallback = Fx.ThunkCallback(new AsyncCallback(IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.CloseChannelCallback));
                IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.sendRequestCallback = Fx.ThunkCallback(new AsyncCallback(IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.SendRequestCallback));
                IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.openChannelCallback = Fx.ThunkCallback(new AsyncCallback(IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.OpenChannelCallback));
            }

            public SecurityNegotiationAsyncResult(IssuanceTokenProviderBase<T> tokenProvider, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.timeout = timeout;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.tokenProvider = tokenProvider;
                this.target = tokenProvider.targetAddress;
                this.issuer = tokenProvider.issuerAddress;
                this.via = tokenProvider.via;
                bool flag = false;
                try
                {
                    flag = this.StartNegotiation();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.OnSyncNegotiationFailure(exception));
                }
                if (flag)
                {
                    this.OnNegotiationComplete();
                    base.Complete(true);
                }
            }

            private void Cleanup()
            {
                this.tokenProvider.Cleanup(this.rstChannel, this.negotiationState);
                this.rstChannel = null;
                this.negotiationState = default(T);
            }

            private static void CloseChannelCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult asyncState = (IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        asyncState.rstChannel.EndClose(result);
                        asyncState.OnNegotiationComplete();
                        flag = true;
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = asyncState.OnAsyncNegotiationFailure(exception2);
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool CloseRequestChannel()
            {
                IAsyncResult result = this.rstChannel.BeginClose(this.timeoutHelper.RemainingTime(), IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.closeChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.rstChannel.EndClose(result);
                return true;
            }

            private static void CreateNegotiationStateCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult asyncState = (IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        asyncState.negotiationState = asyncState.tokenProvider.EndCreateNegotiationState(result);
                        flag = asyncState.OnCreateStateComplete();
                        if (flag)
                        {
                            asyncState.OnNegotiationComplete();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = asyncState.OnAsyncNegotiationFailure(exception2);
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool DoNegotiation(Message incomingMessage)
            {
                this.nextOutgoingMessage = this.tokenProvider.GetNextOutgoingMessage(incomingMessage, this.negotiationState);
                if (this.nextOutgoingMessage != null)
                {
                    return this.SendRequest();
                }
                if (!this.negotiationState.IsNegotiationCompleted)
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoNegotiationMessageToSend")), incomingMessage);
                }
                return this.CloseRequestChannel();
            }

            public static SecurityToken End(IAsyncResult result)
            {
                return AsyncResult.End<IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult>(result).serviceToken;
            }

            private bool InitializeChannelFactories()
            {
                if (this.tokenProvider.WillInitializeChannelFactoriesCompleteSynchronously(this.negotiationState.RemoteAddress))
                {
                    this.tokenProvider.InitializeChannelFactories(this.negotiationState.RemoteAddress, this.timeoutHelper.RemainingTime());
                }
                else
                {
                    IAsyncResult result = this.tokenProvider.BeginInitializeChannelFactories(this.negotiationState.RemoteAddress, this.timeoutHelper.RemainingTime(), IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.initializeChannelFactoriesCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.tokenProvider.EndInitializeChannelFactories(result);
                }
                return this.OnChannelFactoriesInitialized();
            }

            private static void InitializeChannelFactoriesCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult asyncState = (IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        asyncState.tokenProvider.EndInitializeChannelFactories(result);
                        flag = asyncState.OnChannelFactoriesInitialized();
                        if (flag)
                        {
                            asyncState.OnNegotiationComplete();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = asyncState.OnAsyncNegotiationFailure(exception2);
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private Exception OnAsyncNegotiationFailure(Exception e)
            {
                EndpointAddress targetAddress = null;
                try
                {
                    targetAddress = (this.negotiationState == null) ? null : this.negotiationState.RemoteAddress;
                    this.Cleanup();
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                return IssuanceTokenProviderBase<T>.WrapExceptionIfRequired(e, targetAddress, this.issuer);
            }

            private bool OnChannelFactoriesInitialized()
            {
                this.rstChannel = this.tokenProvider.CreateClientChannel(this.negotiationState.RemoteAddress, this.via);
                this.nextOutgoingMessage = null;
                return this.OnRequestChannelCreated();
            }

            private bool OnCreateStateComplete()
            {
                this.tokenProvider.InitializeNegotiationState(this.negotiationState);
                return this.InitializeChannelFactories();
            }

            private void OnNegotiationComplete()
            {
                using (this.negotiationState)
                {
                    SecurityToken serviceToken = this.negotiationState.ServiceToken;
                    this.tokenProvider.ValidateAndCacheServiceToken(this.negotiationState);
                    this.serviceToken = serviceToken;
                }
            }

            private bool OnRequestChannelCreated()
            {
                IAsyncResult result = this.rstChannel.BeginOpen(this.timeoutHelper.RemainingTime(), IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.openChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.rstChannel.EndOpen(result);
                return this.OnRequestChannelOpened();
            }

            private bool OnRequestChannelOpened()
            {
                return this.SendRequest();
            }

            private Exception OnSyncNegotiationFailure(Exception e)
            {
                EndpointAddress targetAddress = (this.negotiationState == null) ? null : this.negotiationState.RemoteAddress;
                return IssuanceTokenProviderBase<T>.WrapExceptionIfRequired(e, targetAddress, this.issuer);
            }

            private static void OpenChannelCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult asyncState = (IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        asyncState.rstChannel.EndOpen(result);
                        flag = asyncState.OnRequestChannelOpened();
                        if (flag)
                        {
                            asyncState.OnNegotiationComplete();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = asyncState.OnAsyncNegotiationFailure(exception2);
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool SendRequest()
            {
                if (this.nextOutgoingMessage == null)
                {
                    return this.DoNegotiation(null);
                }
                this.tokenProvider.PrepareRequest(this.nextOutgoingMessage);
                bool flag = true;
                Message incomingMessage = null;
                IAsyncResult result = null;
                try
                {
                    result = this.rstChannel.BeginRequest(this.nextOutgoingMessage, this.timeoutHelper.RemainingTime(), IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.sendRequestCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        flag = false;
                        return false;
                    }
                    incomingMessage = this.rstChannel.EndRequest(result);
                }
                finally
                {
                    if (flag && (this.nextOutgoingMessage != null))
                    {
                        this.nextOutgoingMessage.Close();
                    }
                }
                using (incomingMessage)
                {
                    if (incomingMessage == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("FailToRecieveReplyFromNegotiation")));
                    }
                    return this.DoNegotiation(incomingMessage);
                }
            }

            private static void SendRequestCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult asyncState = (IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        Message incomingMessage = null;
                        try
                        {
                            incomingMessage = asyncState.rstChannel.EndRequest(result);
                        }
                        finally
                        {
                            if (asyncState.nextOutgoingMessage != null)
                            {
                                asyncState.nextOutgoingMessage.Close();
                            }
                        }
                        using (incomingMessage)
                        {
                            if (incomingMessage == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("FailToRecieveReplyFromNegotiation")));
                            }
                            flag = asyncState.DoNegotiation(incomingMessage);
                        }
                        if (flag)
                        {
                            asyncState.OnNegotiationComplete();
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = asyncState.OnAsyncNegotiationFailure(exception2);
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool StartNegotiation()
            {
                if (this.tokenProvider.CreateNegotiationStateCompletesSynchronously(this.target, this.via))
                {
                    this.negotiationState = this.tokenProvider.CreateNegotiationState(this.target, this.via, this.timeoutHelper.RemainingTime());
                }
                else
                {
                    IAsyncResult result = this.tokenProvider.BeginCreateNegotiationState(this.target, this.via, this.timeoutHelper.RemainingTime(), IssuanceTokenProviderBase<T>.SecurityNegotiationAsyncResult.createNegotiationStateCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.negotiationState = this.tokenProvider.EndCreateNegotiationState(result);
                }
                return this.OnCreateStateComplete();
            }
        }
    }
}

