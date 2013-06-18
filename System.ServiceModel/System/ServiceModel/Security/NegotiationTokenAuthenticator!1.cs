namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal abstract class NegotiationTokenAuthenticator<T> : CommunicationObjectSecurityTokenAuthenticator, IIssuanceSecurityTokenAuthenticator, ISecurityContextSecurityTokenCacheProvider where T: NegotiationTokenAuthenticatorState
    {
        private List<IChannel> activeNegotiationChannels1;
        private List<IChannel> activeNegotiationChannels2;
        private System.ServiceModel.AuditLogLocation auditLogLocation;
        private SecurityContextCookieSerializer cookieSerializer;
        internal static readonly System.ServiceModel.Security.SecurityStateEncoder defaultSecurityStateEncoder;
        internal static readonly TimeSpan defaultServerIssuedTokenLifetime;
        internal const string defaultServerIssuedTokenLifetimeString = "10:00:00";
        internal static readonly TimeSpan defaultServerIssuedTransitionTokenLifetime;
        internal const string defaultServerIssuedTransitionTokenLifetimeString = "00:15:00";
        internal const bool defaultServerMaintainState = true;
        internal const int defaultServerMaxActiveNegotiations = 0x80;
        internal const int defaultServerMaxCachedTokens = 0x3e8;
        internal static readonly TimeSpan defaultServerMaxNegotiationLifetime;
        internal const string defaultServerMaxNegotiationLifetimeString = "00:01:00";
        internal static readonly SecurityStandardsManager defaultStandardsManager;
        private bool encryptStateInServiceToken;
        private IMessageFilterTable<EndpointAddress> endpointFilterTable;
        private IOThreadTimer idlingNegotiationSessionTimer;
        private bool isClientAnonymous;
        private System.ServiceModel.Security.Tokens.IssuedSecurityTokenHandler issuedSecurityTokenHandler;
        private SecurityTokenParameters issuedSecurityTokenParameters;
        private ISecurityContextSecurityTokenCache issuedTokenCache;
        private BindingContext issuerBindingContext;
        private bool isTimerCancelled;
        private IList<System.Type> knownTypes;
        private Uri listenUri;
        private int maximumCachedNegotiationState;
        private int maximumConcurrentNegotiations;
        private int maxMessageSize;
        private AuditLevel messageAuthenticationAuditLevel;
        private NegotiationHost<T> negotiationHost;
        private TimeSpan negotiationTimeout;
        private System.ServiceModel.Security.Tokens.RenewedSecurityTokenHandler renewedSecurityTokenHandler;
        private string sctUri;
        private System.ServiceModel.Security.SecurityAlgorithmSuite securityAlgorithmSuite;
        private System.ServiceModel.Security.SecurityStateEncoder securityStateEncoder;
        private TimeSpan serviceTokenLifetime;
        private SecurityStandardsManager standardsManager;
        private NegotiationTokenAuthenticatorStateCache<T> stateCache;
        private bool suppressAuditFailure;

        static NegotiationTokenAuthenticator()
        {
            NegotiationTokenAuthenticator<T>.defaultServerMaxNegotiationLifetime = TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture);
            NegotiationTokenAuthenticator<T>.defaultServerIssuedTokenLifetime = TimeSpan.Parse("10:00:00", CultureInfo.InvariantCulture);
            NegotiationTokenAuthenticator<T>.defaultServerIssuedTransitionTokenLifetime = TimeSpan.Parse("00:15:00", CultureInfo.InvariantCulture);
            NegotiationTokenAuthenticator<T>.defaultStandardsManager = SecurityStandardsManager.DefaultInstance;
            NegotiationTokenAuthenticator<T>.defaultSecurityStateEncoder = new DataProtectionSecurityStateEncoder();
        }

        protected NegotiationTokenAuthenticator()
        {
            this.InitializeDefaults();
        }

        private void AddNegotiationChannelForIdleTracking()
        {
            if (OperationContext.Current.SessionId != null)
            {
                lock (this.ThisLock)
                {
                    if (this.idlingNegotiationSessionTimer != null)
                    {
                        IChannel item = OperationContext.Current.Channel;
                        if (!this.activeNegotiationChannels1.Contains(item) && !this.activeNegotiationChannels2.Contains(item))
                        {
                            this.activeNegotiationChannels1.Add(item);
                        }
                        if (this.isTimerCancelled)
                        {
                            this.isTimerCancelled = false;
                            this.idlingNegotiationSessionTimer.Set(this.negotiationTimeout);
                        }
                    }
                }
            }
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is SecurityContextSecurityToken);
        }

        private Message CreateFault(Message request, Exception e)
        {
            FaultCode code;
            FaultReason reason;
            bool flag;
            FaultCode code2;
            MessageVersion version = request.Version;
            if ((e is SecurityTokenValidationException) || (e is Win32Exception))
            {
                code = new FaultCode("FailedAuthentication", "http://schemas.xmlsoap.org/ws/2005/02/trust");
                reason = new FaultReason(System.ServiceModel.SR.GetString("FailedAuthenticationTrustFaultCode"), CultureInfo.CurrentCulture);
                flag = true;
            }
            else if (e is QuotaExceededException)
            {
                code = new FaultCode("ServerTooBusy", "http://schemas.microsoft.com/ws/2006/05/security");
                reason = new FaultReason(System.ServiceModel.SR.GetString("NegotiationQuotasExceededFaultReason"), CultureInfo.CurrentCulture);
                flag = false;
            }
            else
            {
                code = new FaultCode("InvalidRequest", "http://schemas.xmlsoap.org/ws/2005/02/trust");
                reason = new FaultReason(System.ServiceModel.SR.GetString("InvalidRequestTrustFaultCode"), CultureInfo.CurrentCulture);
                flag = true;
            }
            if (flag)
            {
                code2 = FaultCode.CreateSenderFaultCode(code);
            }
            else
            {
                code2 = FaultCode.CreateReceiverFaultCode(code);
            }
            MessageFault fault = MessageFault.CreateFault(code2, reason);
            Message message = Message.CreateMessage(version, fault, version.Addressing.DefaultFaultAction);
            message.Headers.RelatesTo = request.Headers.MessageId;
            return message;
        }

        private static Message CreateReply(Message request, XmlDictionaryString action, BodyWriter body)
        {
            if (request.Headers.MessageId != null)
            {
                Message message = Message.CreateMessage(request.Version, ActionHeader.Create(action, request.Version.Addressing), body);
                message.InitializeReply(request);
                return message;
            }
            return Message.CreateMessage(request.Version, ActionHeader.Create(action, request.Version.Addressing), body);
        }

        protected abstract MessageFilter GetListenerFilter();
        protected abstract Binding GetNegotiationBinding(Binding binding);
        private Message HandleNegotiationException(Message request, Exception e)
        {
            SecurityTraceRecordHelper.TraceServiceSecurityNegotiationFailure<T>((NegotiationTokenAuthenticator<T>) this, e);
            return this.CreateFault(request, e);
        }

        private void InitializeDefaults()
        {
            this.encryptStateInServiceToken = false;
            this.serviceTokenLifetime = NegotiationTokenAuthenticator<T>.defaultServerIssuedTokenLifetime;
            this.maximumCachedNegotiationState = 0x80;
            this.negotiationTimeout = NegotiationTokenAuthenticator<T>.defaultServerMaxNegotiationLifetime;
            this.isClientAnonymous = false;
            this.standardsManager = NegotiationTokenAuthenticator<T>.defaultStandardsManager;
            this.securityStateEncoder = NegotiationTokenAuthenticator<T>.defaultSecurityStateEncoder;
            this.maximumConcurrentNegotiations = 0x80;
            this.maxMessageSize = 0x7fffffff;
        }

        protected SecurityContextSecurityToken IssueSecurityContextToken(UniqueId contextId, string id, byte[] key, DateTime tokenEffectiveTime, DateTime tokenExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode)
        {
            return this.IssueSecurityContextToken(contextId, id, key, tokenEffectiveTime, tokenExpirationTime, null, tokenEffectiveTime, tokenExpirationTime, authorizationPolicies, isCookieMode);
        }

        protected SecurityContextSecurityToken IssueSecurityContextToken(UniqueId contextId, string id, byte[] key, DateTime tokenEffectiveTime, DateTime tokenExpirationTime, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode)
        {
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            if ((this.securityStateEncoder == null) && isCookieMode)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SctCookieNotSupported")));
            }
            return new SecurityContextSecurityToken(contextId, id, key, tokenEffectiveTime, tokenExpirationTime, authorizationPolicies, isCookieMode, isCookieMode ? this.cookieSerializer.CreateCookieFromSecurityContext(contextId, id, key, tokenEffectiveTime, tokenExpirationTime, keyGeneration, keyEffectiveTime, keyExpirationTime, authorizationPolicies) : null, keyGeneration, keyEffectiveTime, keyExpirationTime);
        }

        public override void OnAbort()
        {
            if (this.negotiationHost != null)
            {
                this.negotiationHost.Abort();
                this.negotiationHost = null;
            }
            if (this.issuedTokenCache != null)
            {
                this.issuedTokenCache.ClearContexts();
            }
            lock (this.ThisLock)
            {
                if ((this.idlingNegotiationSessionTimer != null) && !this.isTimerCancelled)
                {
                    this.isTimerCancelled = true;
                    this.idlingNegotiationSessionTimer.Cancel();
                }
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.negotiationHost != null)
            {
                this.negotiationHost.Close(helper.RemainingTime());
                this.negotiationHost = null;
            }
            if (this.issuedTokenCache != null)
            {
                this.issuedTokenCache.ClearContexts();
            }
            lock (this.ThisLock)
            {
                if ((this.idlingNegotiationSessionTimer != null) && !this.isTimerCancelled)
                {
                    this.isTimerCancelled = true;
                    this.idlingNegotiationSessionTimer.Cancel();
                }
            }
            base.OnClose(helper.RemainingTime());
        }

        private void OnIdlingNegotiationSessionTimer(object state)
        {
            lock (this.ThisLock)
            {
                if (!this.isTimerCancelled && ((base.CommunicationObject.State == CommunicationState.Opened) || (base.CommunicationObject.State == CommunicationState.Opening)))
                {
                    try
                    {
                        for (int i = 0; i < this.activeNegotiationChannels2.Count; i++)
                        {
                            this.activeNegotiationChannels2[i].Abort();
                        }
                        List<IChannel> list = this.activeNegotiationChannels2;
                        list.Clear();
                        this.activeNegotiationChannels2 = this.activeNegotiationChannels1;
                        this.activeNegotiationChannels1 = list;
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        if ((base.CommunicationObject.State == CommunicationState.Opened) || (base.CommunicationObject.State == CommunicationState.Opening))
                        {
                            if ((this.activeNegotiationChannels1.Count == 0) && (this.activeNegotiationChannels2.Count == 0))
                            {
                                this.isTimerCancelled = true;
                                this.idlingNegotiationSessionTimer.Cancel();
                            }
                            else
                            {
                                this.idlingNegotiationSessionTimer.Set(this.negotiationTimeout);
                            }
                        }
                    }
                }
            }
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.IssuerBindingContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuerBuildContextNotSet", new object[] { base.GetType() })));
            }
            if (this.IssuedSecurityTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedSecurityTokenParametersNotSet", new object[] { base.GetType() })));
            }
            if (this.SecurityAlgorithmSuite == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityAlgorithmSuiteNotSet", new object[] { base.GetType() })));
            }
            if (this.IssuedTokenCache == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedTokenCacheNotSet", new object[] { base.GetType() })));
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.SetupServiceHost();
            this.negotiationHost.Open(helper.RemainingTime());
            this.stateCache = new NegotiationTokenAuthenticatorStateCache<T>(this.NegotiationTimeout, this.MaximumCachedNegotiationState);
            this.sctUri = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
            if (this.SecurityStateEncoder != null)
            {
                this.cookieSerializer = new SecurityContextCookieSerializer(this.SecurityStateEncoder, this.KnownTypes);
            }
            if (this.negotiationTimeout < TimeSpan.MaxValue)
            {
                lock (this.ThisLock)
                {
                    this.activeNegotiationChannels1 = new List<IChannel>();
                    this.activeNegotiationChannels2 = new List<IChannel>();
                    this.idlingNegotiationSessionTimer = new IOThreadTimer(new Action<object>(this.OnIdlingNegotiationSessionTimer), this, false);
                    this.isTimerCancelled = false;
                    this.idlingNegotiationSessionTimer.Set(this.negotiationTimeout);
                }
            }
            base.OnOpen(helper.RemainingTime());
        }

        private void OnTokenIssued(SecurityToken token)
        {
            if (this.issuedSecurityTokenHandler != null)
            {
                this.issuedSecurityTokenHandler(token, null);
            }
        }

        protected virtual void ParseMessageBody(Message message, out string context, out RequestSecurityToken requestSecurityToken, out RequestSecurityTokenResponse requestSecurityTokenResponse)
        {
            requestSecurityToken = null;
            requestSecurityTokenResponse = null;
            if (message.Headers.Action == this.RequestSecurityTokenAction.Value)
            {
                XmlDictionaryReader readerAtBodyContents = message.GetReaderAtBodyContents();
                using (readerAtBodyContents)
                {
                    requestSecurityToken = RequestSecurityToken.CreateFrom(this.StandardsManager, readerAtBodyContents);
                    message.ReadFromBodyContentsToEnd(readerAtBodyContents);
                }
                context = requestSecurityToken.Context;
            }
            else
            {
                if (message.Headers.Action != this.RequestSecurityTokenResponseAction.Value)
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidActionForNegotiationMessage", new object[] { message.Headers.Action })), message);
                }
                XmlDictionaryReader reader = message.GetReaderAtBodyContents();
                using (reader)
                {
                    requestSecurityTokenResponse = RequestSecurityTokenResponse.CreateFrom(this.StandardsManager, reader);
                    message.ReadFromBodyContentsToEnd(reader);
                }
                context = requestSecurityTokenResponse.Context;
            }
        }

        private Message ProcessRequestCore(Message request)
        {
            Message message;
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            RequestSecurityToken requestSecurityToken = null;
            RequestSecurityTokenResponse requestSecurityTokenResponse = null;
            string context = null;
            bool flag = false;
            bool flag2 = true;
            T negotiationState = default(T);
            try
            {
                if (this.maxMessageSize < 0x7fffffff)
                {
                    string action = request.Headers.Action;
                    try
                    {
                        using (MessageBuffer buffer = request.CreateBufferedCopy(this.maxMessageSize))
                        {
                            request = buffer.CreateMessage();
                            flag = true;
                        }
                    }
                    catch (QuotaExceededException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("SecurityNegotiationMessageTooLarge", new object[] { action, this.maxMessageSize }), exception));
                    }
                }
                try
                {
                    BodyWriter writer;
                    Uri to = request.Headers.To;
                    this.ParseMessageBody(request, out context, out requestSecurityToken, out requestSecurityTokenResponse);
                    if (context != null)
                    {
                        negotiationState = this.stateCache.GetState(context);
                    }
                    else
                    {
                        negotiationState = default(T);
                    }
                    bool flag3 = false;
                    try
                    {
                        if (requestSecurityToken != null)
                        {
                            if (negotiationState != null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationStateAlreadyPresent", new object[] { context })));
                            }
                            writer = this.ProcessRequestSecurityToken(request, requestSecurityToken, out negotiationState);
                            lock (negotiationState.ThisLock)
                            {
                                if (negotiationState.IsNegotiationCompleted)
                                {
                                    if (!negotiationState.ServiceToken.IsCookieMode)
                                    {
                                        this.IssuedTokenCache.AddContext(negotiationState.ServiceToken);
                                    }
                                    this.OnTokenIssued(negotiationState.ServiceToken);
                                    SecurityTraceRecordHelper.TraceServiceSecurityNegotiationCompleted<T>((NegotiationTokenAuthenticator<T>) this, negotiationState.ServiceToken);
                                    flag3 = true;
                                }
                                else
                                {
                                    this.stateCache.AddState(context, negotiationState);
                                    flag3 = false;
                                }
                                this.AddNegotiationChannelForIdleTracking();
                                goto Label_0299;
                            }
                        }
                        if (negotiationState == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("CannotFindNegotiationState", new object[] { context })));
                        }
                        lock (negotiationState.ThisLock)
                        {
                            writer = this.ProcessRequestSecurityTokenResponse(negotiationState, request, requestSecurityTokenResponse);
                            if (negotiationState.IsNegotiationCompleted)
                            {
                                if (!negotiationState.ServiceToken.IsCookieMode)
                                {
                                    this.IssuedTokenCache.AddContext(negotiationState.ServiceToken);
                                }
                                this.OnTokenIssued(negotiationState.ServiceToken);
                                SecurityTraceRecordHelper.TraceServiceSecurityNegotiationCompleted<T>((NegotiationTokenAuthenticator<T>) this, negotiationState.ServiceToken);
                                flag3 = true;
                            }
                            else
                            {
                                flag3 = false;
                            }
                        }
                    Label_0299:
                        if ((negotiationState.IsNegotiationCompleted && (null != this.ListenUri)) && (AuditLevel.Success == (this.messageAuthenticationAuditLevel & AuditLevel.Success)))
                        {
                            string remoteIdentityName = negotiationState.GetRemoteIdentityName();
                            SecurityAuditHelper.WriteSecurityNegotiationSuccessEvent(this.auditLogLocation, this.suppressAuditFailure, request, request.Headers.To, request.Headers.Action, remoteIdentityName, base.GetType().Name);
                        }
                        flag2 = false;
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        if (PerformanceCounters.PerformanceCountersEnabled && (null != this.ListenUri))
                        {
                            PerformanceCounters.AuthenticationFailed(request, this.ListenUri);
                        }
                        if (AuditLevel.Failure == (this.messageAuthenticationAuditLevel & AuditLevel.Failure))
                        {
                            try
                            {
                                string clientIdentity = (negotiationState != null) ? negotiationState.GetRemoteIdentityName() : string.Empty;
                                SecurityAuditHelper.WriteSecurityNegotiationFailureEvent(this.auditLogLocation, this.suppressAuditFailure, request, request.Headers.To, request.Headers.Action, clientIdentity, base.GetType().Name, exception2);
                            }
                            catch (Exception exception3)
                            {
                                if (Fx.IsFatal(exception3))
                                {
                                    throw;
                                }
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Error);
                            }
                        }
                        flag3 = true;
                        throw;
                    }
                    finally
                    {
                        if (flag3 && (negotiationState != null))
                        {
                            if (context != null)
                            {
                                this.stateCache.RemoveState(context);
                            }
                            negotiationState.Dispose();
                        }
                    }
                    return NegotiationTokenAuthenticator<T>.CreateReply(request, (writer is RequestSecurityTokenResponseCollection) ? this.RequestSecurityTokenResponseFinalAction : this.RequestSecurityTokenResponseAction, writer);
                }
                finally
                {
                    if (flag)
                    {
                        request.Close();
                    }
                }
            }
            finally
            {
                if (flag2)
                {
                    this.AddNegotiationChannelForIdleTracking();
                }
                else if ((negotiationState != null) && negotiationState.IsNegotiationCompleted)
                {
                    this.RemoveNegotiationChannelFromIdleTracking();
                }
            }
            return message;
        }

        protected abstract BodyWriter ProcessRequestSecurityToken(Message request, RequestSecurityToken requestSecurityToken, out T negotiationState);
        protected abstract BodyWriter ProcessRequestSecurityTokenResponse(T negotiationState, Message request, RequestSecurityTokenResponse requestSecurityTokenResponse);
        private void RemoveNegotiationChannelFromIdleTracking()
        {
            if (OperationContext.Current.SessionId != null)
            {
                lock (this.ThisLock)
                {
                    if (this.idlingNegotiationSessionTimer != null)
                    {
                        IChannel item = OperationContext.Current.Channel;
                        this.activeNegotiationChannels1.Remove(item);
                        this.activeNegotiationChannels2.Remove(item);
                        if ((this.activeNegotiationChannels1.Count == 0) && (this.activeNegotiationChannels2.Count == 0))
                        {
                            this.isTimerCancelled = true;
                            this.idlingNegotiationSessionTimer.Cancel();
                        }
                    }
                }
            }
        }

        private void SetupServiceHost()
        {
            ChannelBuilder channelBuilder = new ChannelBuilder(this.IssuerBindingContext.Clone(), true);
            channelBuilder.Binding.Elements.Insert(0, new ReplyAdapterBindingElement());
            channelBuilder.Binding = new CustomBinding(this.GetNegotiationBinding(channelBuilder.Binding));
            this.negotiationHost = new NegotiationHost<T>((NegotiationTokenAuthenticator<T>) this, this.ListenUri, channelBuilder, this.GetListenerFilter());
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            SecurityContextSecurityToken token2 = (SecurityContextSecurityToken) token;
            return token2.AuthorizationPolicies;
        }

        public System.ServiceModel.AuditLogLocation AuditLogLocation
        {
            get
            {
                return this.auditLogLocation;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.auditLogLocation = value;
            }
        }

        public bool EncryptStateInServiceToken
        {
            get
            {
                return this.encryptStateInServiceToken;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.encryptStateInServiceToken = value;
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
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.endpointFilterTable = value;
            }
        }

        public bool IsClientAnonymous
        {
            get
            {
                return this.isClientAnonymous;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.isClientAnonymous = value;
            }
        }

        protected abstract bool IsMultiLegNegotiation { get; }

        public System.ServiceModel.Security.Tokens.IssuedSecurityTokenHandler IssuedSecurityTokenHandler
        {
            get
            {
                return this.issuedSecurityTokenHandler;
            }
            set
            {
                this.issuedSecurityTokenHandler = value;
            }
        }

        public SecurityTokenParameters IssuedSecurityTokenParameters
        {
            get
            {
                return this.issuedSecurityTokenParameters;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuedSecurityTokenParameters = value;
            }
        }

        public ISecurityContextSecurityTokenCache IssuedTokenCache
        {
            get
            {
                return this.issuedTokenCache;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuedTokenCache = value;
            }
        }

        public BindingContext IssuerBindingContext
        {
            get
            {
                return this.issuerBindingContext;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.issuerBindingContext = value.Clone();
            }
        }

        public IList<System.Type> KnownTypes
        {
            get
            {
                return this.knownTypes;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value != null)
                {
                    this.knownTypes = new Collection<System.Type>(value);
                }
                else
                {
                    this.knownTypes = null;
                }
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
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.listenUri = value;
            }
        }

        public int MaximumCachedNegotiationState
        {
            get
            {
                return this.maximumCachedNegotiationState;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.maximumCachedNegotiationState = value;
            }
        }

        public int MaximumConcurrentNegotiations
        {
            get
            {
                return this.maximumConcurrentNegotiations;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.maximumConcurrentNegotiations = value;
            }
        }

        public int MaxMessageSize
        {
            get
            {
                return this.maxMessageSize;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.maxMessageSize = value;
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
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.messageAuthenticationAuditLevel = value;
            }
        }

        public TimeSpan NegotiationTimeout
        {
            get
            {
                return this.negotiationTimeout;
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
                this.negotiationTimeout = value;
            }
        }

        public System.ServiceModel.Security.Tokens.RenewedSecurityTokenHandler RenewedSecurityTokenHandler
        {
            get
            {
                return this.renewedSecurityTokenHandler;
            }
            set
            {
                this.renewedSecurityTokenHandler = value;
            }
        }

        public virtual XmlDictionaryString RequestSecurityTokenAction
        {
            get
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenAction;
            }
        }

        public virtual XmlDictionaryString RequestSecurityTokenResponseAction
        {
            get
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction;
            }
        }

        public virtual XmlDictionaryString RequestSecurityTokenResponseFinalAction
        {
            get
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenResponseFinalAction;
            }
        }

        public System.ServiceModel.Security.SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.securityAlgorithmSuite;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.securityAlgorithmSuite = value;
            }
        }

        protected string SecurityContextTokenUri
        {
            get
            {
                base.CommunicationObject.ThrowIfNotOpened();
                return this.sctUri;
            }
        }

        public System.ServiceModel.Security.SecurityStateEncoder SecurityStateEncoder
        {
            get
            {
                return this.securityStateEncoder;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.securityStateEncoder = value;
            }
        }

        public TimeSpan ServiceTokenLifetime
        {
            get
            {
                return this.serviceTokenLifetime;
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
                this.serviceTokenLifetime = value;
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
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.standardsManager = (value != null) ? value : SecurityStandardsManager.DefaultInstance;
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
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.suppressAuditFailure = value;
            }
        }

        ISecurityContextSecurityTokenCache ISecurityContextSecurityTokenCacheProvider.TokenCache
        {
            get
            {
                return this.IssuedTokenCache;
            }
        }

        private object ThisLock
        {
            get
            {
                return base.CommunicationObject;
            }
        }

        private class NegotiationHost : ServiceHostBase
        {
            private NegotiationTokenAuthenticator<T> authenticator;
            private ChannelBuilder channelBuilder;
            private MessageFilter listenerFilter;
            private Uri listenUri;

            public NegotiationHost(NegotiationTokenAuthenticator<T> authenticator, Uri listenUri, ChannelBuilder channelBuilder, MessageFilter listenerFilter)
            {
                this.authenticator = authenticator;
                this.listenUri = listenUri;
                this.channelBuilder = channelBuilder;
                this.listenerFilter = listenerFilter;
            }

            protected override System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
            {
                implementedContracts = null;
                return null;
            }

            protected override void InitializeRuntime()
            {
                MessageFilter listenerFilter = this.listenerFilter;
                int priority = 10;
                System.Type[] supportedChannels = new System.Type[] { typeof(IReplyChannel), typeof(IDuplexChannel), typeof(IReplySessionChannel), typeof(IDuplexSessionChannel) };
                IChannelListener result = null;
                BindingParameterCollection parameters = new BindingParameterCollection(this.channelBuilder.BindingParameters);
                Binding binding = this.channelBuilder.Binding;
                binding.ReceiveTimeout = this.authenticator.NegotiationTimeout;
                parameters.Add(new ChannelDemuxerFilter(listenerFilter, priority));
                DispatcherBuilder.MaybeCreateListener(true, supportedChannels, binding, parameters, this.listenUri, "", ListenUriMode.Explicit, base.ServiceThrottle, out result);
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotCreateTwoWayListenerForNegotiation")));
                }
                ChannelDispatcher item = new ChannelDispatcher(result, null, binding) {
                    MessageVersion = binding.MessageVersion,
                    ManualAddressing = true,
                    ServiceThrottle = new ServiceThrottle(this)
                };
                item.ServiceThrottle.MaxConcurrentCalls = this.authenticator.MaximumConcurrentNegotiations;
                item.ServiceThrottle.MaxConcurrentSessions = this.authenticator.MaximumConcurrentNegotiations;
                EndpointDispatcher dispatcher2 = new EndpointDispatcher(new EndpointAddress(this.listenUri, new AddressHeader[0]), "SecurityNegotiationContract", "http://tempuri.org/", true) {
                    DispatchRuntime = { SingletonInstanceContext = new InstanceContext(null, this.authenticator, false), ConcurrencyMode = ConcurrencyMode.Multiple },
                    AddressFilter = new MatchAllMessageFilter(),
                    ContractFilter = listenerFilter,
                    FilterPriority = priority
                };
                dispatcher2.DispatchRuntime.PrincipalPermissionMode = PrincipalPermissionMode.None;
                dispatcher2.DispatchRuntime.InstanceContextProvider = new SingletonInstanceContextProvider(dispatcher2.DispatchRuntime);
                dispatcher2.DispatchRuntime.SynchronizationContext = null;
                DispatchOperation operation = new DispatchOperation(dispatcher2.DispatchRuntime, "*", "*", "*") {
                    Formatter = new MessageOperationFormatter(),
                    Invoker = new NegotiationSyncInvoker<T>(this.authenticator)
                };
                dispatcher2.DispatchRuntime.UnhandledDispatchOperation = operation;
                item.Endpoints.Add(dispatcher2);
                base.ChannelDispatchers.Add(item);
            }

            private class NegotiationSyncInvoker : IOperationInvoker
            {
                private NegotiationTokenAuthenticator<T> parent;

                internal NegotiationSyncInvoker(NegotiationTokenAuthenticator<T> parent)
                {
                    this.parent = parent;
                }

                public object[] AllocateInputs()
                {
                    return EmptyArray<object>.Allocate(1);
                }

                public object Invoke(object instance, object[] inputs, out object[] outputs)
                {
                    Message request = (Message) inputs[0];
                    outputs = EmptyArray<object>.Allocate(0);
                    try
                    {
                        return this.parent.ProcessRequestCore(request);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        return this.parent.HandleNegotiationException(request, exception);
                    }
                }

                public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }

                public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }

                public bool IsSynchronous
                {
                    get
                    {
                        return true;
                    }
                }
            }
        }
    }
}

