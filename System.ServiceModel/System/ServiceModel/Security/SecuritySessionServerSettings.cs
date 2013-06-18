namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal sealed class SecuritySessionServerSettings : IListenerSecureConversationSessionSettings, ISecurityCommunicationObject
    {
        private volatile bool acceptNewWork;
        private Dictionary<UniqueId, IServerSecuritySessionChannel> activeSessions = new Dictionary<UniqueId, IServerSecuritySessionChannel>();
        private bool canRenewSession = true;
        private ICommunicationObject channelAcceptor;
        private System.ServiceModel.Channels.ChannelBuilder channelBuilder;
        private TimeSpan closeTimeout;
        private WrapperSecurityCommunicationObject communicationObject;
        internal static readonly TimeSpan defaultInactivityTimeout = TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture);
        internal const string defaultInactivityTimeoutString = "00:02:00";
        internal static readonly TimeSpan defaultKeyRenewalInterval = TimeSpan.Parse("15:00:00", CultureInfo.InvariantCulture);
        internal const string defaultKeyRenewalIntervalString = "15:00:00";
        internal static readonly TimeSpan defaultKeyRolloverInterval = TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture);
        internal const string defaultKeyRolloverIntervalString = "00:05:00";
        internal const int defaultMaximumPendingSessions = 0x80;
        internal const bool defaultTolerateTransportFailures = true;
        private TimeSpan inactivityTimeout = defaultInactivityTimeout;
        private IOThreadTimer inactivityTimer;
        private SecurityTokenParameters issuedTokenParameters;
        private TimeSpan keyRolloverInterval = defaultKeyRolloverInterval;
        private System.Uri listenUri;
        private TimeSpan maximumKeyRenewalInterval = defaultKeyRenewalInterval;
        private int maximumPendingKeysPerSession = 5;
        private int maximumPendingSessions = 0x80;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private TimeSpan openTimeout;
        private Dictionary<UniqueId, IServerReliableChannelBinder> pendingSessions1;
        private Dictionary<UniqueId, IServerReliableChannelBinder> pendingSessions2;
        private ChannelListenerBase securityChannelListener;
        private TimeSpan sendTimeout;
        private SecurityProtocolFactory sessionProtocolFactory;
        private SecurityTokenAuthenticator sessionTokenAuthenticator;
        private ISecurityContextSecurityTokenCache sessionTokenCache;
        private SecurityTokenResolver sessionTokenResolver;
        private SecurityListenerSettingsLifetimeManager settingsLifetimeManager;
        private System.ServiceModel.Security.SecurityStandardsManager standardsManager;
        private object thisLock = new object();
        private bool tolerateTransportFailures = true;

        public SecuritySessionServerSettings()
        {
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        internal void Abort()
        {
            this.communicationObject.Abort();
        }

        private void AbortPendingChannels()
        {
            lock (this.ThisLock)
            {
                if (this.pendingSessions1 != null)
                {
                    foreach (IServerReliableChannelBinder binder in this.pendingSessions1.Values)
                    {
                        binder.Abort();
                    }
                }
                if (this.pendingSessions2 != null)
                {
                    foreach (IServerReliableChannelBinder binder2 in this.pendingSessions2.Values)
                    {
                        binder2.Abort();
                    }
                }
            }
        }

        private void AddPendingSession(UniqueId sessionId, IServerReliableChannelBinder channelBinder)
        {
            lock (this.ThisLock)
            {
                if ((this.GetPendingSessionCount() + 1) > this.MaximumPendingSessions)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(System.ServiceModel.SR.GetString("SecuritySessionLimitReached")));
                }
                if (this.pendingSessions1.ContainsKey(sessionId) || this.pendingSessions2.ContainsKey(sessionId))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SecuritySessionAlreadyPending", new object[] { sessionId })));
                }
                this.pendingSessions1.Add(sessionId, channelBinder);
            }
            SecurityTraceRecordHelper.TracePendingSessionAdded(sessionId, this.Uri);
        }

        private void AddSessionChannel(UniqueId sessionId, IServerSecuritySessionChannel channel)
        {
            lock (this.ThisLock)
            {
                this.activeSessions.Add(sessionId, channel);
            }
        }

        internal IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        internal IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginOpen(timeout, callback, state);
        }

        private void ClearPendingSessions()
        {
            lock (this.ThisLock)
            {
                if ((this.pendingSessions1.Count != 0) || (this.pendingSessions2.Count != 0))
                {
                    foreach (UniqueId id in this.pendingSessions2.Keys)
                    {
                        IServerReliableChannelBinder binder = this.pendingSessions2[id];
                        try
                        {
                            this.TryCloseBinder(binder, this.CloseTimeout);
                            this.SessionTokenCache.RemoveAllContexts(id);
                        }
                        catch (CommunicationException exception)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            }
                        }
                        catch (TimeoutException exception2)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                            }
                        }
                        catch (ObjectDisposedException exception3)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                            }
                        }
                        SecurityTraceRecordHelper.TracePendingSessionClosed(id, this.Uri);
                    }
                    this.pendingSessions2.Clear();
                    Dictionary<UniqueId, IServerReliableChannelBinder> dictionary = this.pendingSessions2;
                    this.pendingSessions2 = this.pendingSessions1;
                    this.pendingSessions1 = dictionary;
                }
            }
        }

        internal void Close(TimeSpan timeout)
        {
            this.communicationObject.Close(timeout);
        }

        private void ClosePendingChannels(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            lock (this.ThisLock)
            {
                foreach (IServerReliableChannelBinder binder in this.pendingSessions1.Values)
                {
                    binder.Close(helper.RemainingTime());
                }
                foreach (IServerReliableChannelBinder binder2 in this.pendingSessions2.Values)
                {
                    binder2.Close(helper.RemainingTime());
                }
            }
        }

        private void ConfigureSessionSecurityProtocolFactory()
        {
            if (this.sessionProtocolFactory is SessionSymmetricMessageSecurityProtocolFactory)
            {
                AddressingVersion addressing = System.ServiceModel.Channels.MessageVersion.Default.Addressing;
                if (this.channelBuilder != null)
                {
                    MessageEncodingBindingElement element = this.channelBuilder.Binding.Elements.Find<MessageEncodingBindingElement>();
                    if (element != null)
                    {
                        addressing = element.MessageVersion.Addressing;
                    }
                }
                if ((addressing != AddressingVersion.WSAddressing10) && (addressing != AddressingVersion.WSAddressingAugust2004))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressing })));
                }
                SessionSymmetricMessageSecurityProtocolFactory sessionProtocolFactory = (SessionSymmetricMessageSecurityProtocolFactory) this.sessionProtocolFactory;
                if (!sessionProtocolFactory.ApplyIntegrity || !sessionProtocolFactory.RequireIntegrity)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecuritySessionRequiresMessageIntegrity")));
                }
                MessagePartSpecification parts = new MessagePartSpecification(true);
                sessionProtocolFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                sessionProtocolFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, addressing.FaultAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, addressing.DefaultFaultAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, "http://schemas.microsoft.com/ws/2006/05/security/SecureConversationFault");
                if (sessionProtocolFactory.ApplyConfidentiality)
                {
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, addressing.FaultAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, addressing.DefaultFaultAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, "http://schemas.microsoft.com/ws/2006/05/security/SecureConversationFault");
                }
                if (sessionProtocolFactory.RequireConfidentiality)
                {
                    sessionProtocolFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    sessionProtocolFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                }
                sessionProtocolFactory.SecurityTokenParameters = this.IssuedSecurityTokenParameters;
            }
            else
            {
                if (!(this.sessionProtocolFactory is SessionSymmetricTransportSecurityProtocolFactory))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                SessionSymmetricTransportSecurityProtocolFactory factory2 = (SessionSymmetricTransportSecurityProtocolFactory) this.sessionProtocolFactory;
                factory2.AddTimestamp = true;
                factory2.SecurityTokenParameters = this.IssuedSecurityTokenParameters;
                factory2.SecurityTokenParameters.RequireDerivedKeys = false;
            }
        }

        internal IChannelAcceptor<TChannel> CreateAcceptor<TChannel>() where TChannel: class, IChannel
        {
            if (this.channelAcceptor != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SSSSCreateAcceptor")));
            }
            object listenerState = this.sessionProtocolFactory.CreateListenerSecurityState();
            if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                this.channelAcceptor = new SecuritySessionChannelAcceptor<IReplySessionChannel>(this.SecurityChannelListener, listenerState);
            }
            else
            {
                if (typeof(TChannel) != typeof(IDuplexSessionChannel))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                this.channelAcceptor = new SecuritySessionChannelAcceptor<IDuplexSessionChannel>(this.SecurityChannelListener, listenerState);
            }
            return (IChannelAcceptor<TChannel>) this.channelAcceptor;
        }

        private IServerReliableChannelBinder CreateChannelBinder(SecurityContextSecurityToken sessionToken, EndpointAddress remoteAddress)
        {
            IServerReliableChannelBinder channelBinder = null;
            MessageFilter filter = new SecuritySessionFilter(sessionToken.ContextId, this.sessionProtocolFactory.StandardsManager, this.sessionProtocolFactory.SecurityHeaderLayout == SecurityHeaderLayout.Strict, new string[] { this.SecurityStandardsManager.SecureConversationDriver.RenewAction.Value, this.SecurityStandardsManager.SecureConversationDriver.RenewResponseAction.Value });
            int priority = 0x7fffffff;
            TolerateFaultsMode faultMode = this.TolerateTransportFailures ? TolerateFaultsMode.Always : TolerateFaultsMode.Never;
            lock (this.ThisLock)
            {
                if (this.ChannelBuilder.CanBuildChannelListener<IDuplexSessionChannel>())
                {
                    channelBinder = ServerReliableChannelBinder<IDuplexSessionChannel>.CreateBinder(this.ChannelBuilder, remoteAddress, filter, priority, faultMode, this.CloseTimeout, this.SendTimeout);
                }
                else if (this.ChannelBuilder.CanBuildChannelListener<IDuplexChannel>())
                {
                    channelBinder = ServerReliableChannelBinder<IDuplexChannel>.CreateBinder(this.ChannelBuilder, remoteAddress, filter, priority, faultMode, this.CloseTimeout, this.SendTimeout);
                }
                else if (this.ChannelBuilder.CanBuildChannelListener<IReplyChannel>())
                {
                    channelBinder = ServerReliableChannelBinder<IReplyChannel>.CreateBinder(this.ChannelBuilder, remoteAddress, filter, priority, faultMode, this.CloseTimeout, this.SendTimeout);
                }
            }
            if (channelBinder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            channelBinder.Open(this.OpenTimeout);
            new SessionInitiationMessageHandler(channelBinder, this, sessionToken).BeginReceive(TimeSpan.MaxValue);
            return channelBinder;
        }

        internal IChannelListener CreateInnerChannelListener()
        {
            if (this.ChannelBuilder.CanBuildChannelListener<IDuplexSessionChannel>())
            {
                return this.ChannelBuilder.BuildChannelListener<IDuplexSessionChannel>(new MatchNoneMessageFilter(), -2147483648);
            }
            if (this.ChannelBuilder.CanBuildChannelListener<IDuplexChannel>())
            {
                return this.ChannelBuilder.BuildChannelListener<IDuplexChannel>(new MatchNoneMessageFilter(), -2147483648);
            }
            if (!this.ChannelBuilder.CanBuildChannelListener<IReplyChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return this.ChannelBuilder.BuildChannelListener<IReplyChannel>(new MatchNoneMessageFilter(), -2147483648);
        }

        internal void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        internal void EndOpen(IAsyncResult result)
        {
            this.communicationObject.EndOpen(result);
        }

        private IServerSecuritySessionChannel FindSessionChannel(UniqueId sessionId)
        {
            IServerSecuritySessionChannel channel;
            lock (this.ThisLock)
            {
                this.activeSessions.TryGetValue(sessionId, out channel);
            }
            return channel;
        }

        private int GetPendingSessionCount()
        {
            return ((this.pendingSessions1.Count + this.pendingSessions2.Count) + ((IInputQueueChannelAcceptor) this.channelAcceptor).PendingCount);
        }

        public void OnAbort()
        {
            this.AbortPendingChannels();
            this.OnAbortCore();
        }

        private void OnAbortCore()
        {
            if (this.inactivityTimer != null)
            {
                this.inactivityTimer.Cancel();
            }
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(true, TimeSpan.Zero);
            }
            if (this.sessionTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.AbortTokenAuthenticatorIfRequired(this.sessionTokenAuthenticator);
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

        public void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.ClosePendingChannels(helper.RemainingTime());
            this.OnCloseCore(helper.RemainingTime());
        }

        private void OnCloseCore(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.inactivityTimer != null)
            {
                this.inactivityTimer.Cancel();
            }
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(false, helper.RemainingTime());
            }
            if (this.sessionTokenAuthenticator != null)
            {
                System.ServiceModel.Security.SecurityUtils.CloseTokenAuthenticatorIfRequired(this.sessionTokenAuthenticator, helper.RemainingTime());
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

        public void OnOpen(TimeSpan timeout)
        {
            if (this.sessionProtocolFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecuritySessionProtocolFactoryShouldBeSetBeforeThisOperation")));
            }
            if (this.standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityStandardsManagerNotSet", new object[] { base.GetType() })));
            }
            if (this.issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedSecurityTokenParametersNotSet", new object[] { base.GetType() })));
            }
            if (this.maximumKeyRenewalInterval < this.keyRolloverInterval)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("KeyRolloverGreaterThanKeyRenewal")));
            }
            if (this.securityChannelListener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityChannelListenerNotSet", new object[] { base.GetType() })));
            }
            if (this.settingsLifetimeManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecuritySettingsLifetimeManagerNotSet", new object[] { base.GetType() })));
            }
            this.messageVersion = this.channelBuilder.Binding.MessageVersion;
            this.listenUri = this.securityChannelListener.Uri;
            this.openTimeout = this.securityChannelListener.InternalOpenTimeout;
            this.closeTimeout = this.securityChannelListener.InternalCloseTimeout;
            this.sendTimeout = this.securityChannelListener.InternalSendTimeout;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.pendingSessions1 = new Dictionary<UniqueId, IServerReliableChannelBinder>();
            this.pendingSessions2 = new Dictionary<UniqueId, IServerReliableChannelBinder>();
            if (this.inactivityTimeout < TimeSpan.MaxValue)
            {
                this.inactivityTimer = new IOThreadTimer(new Action<object>(this.OnTimer), this, false);
                this.inactivityTimer.Set(this.inactivityTimeout);
            }
            this.ConfigureSessionSecurityProtocolFactory();
            this.sessionProtocolFactory.Open(false, helper.RemainingTime());
            this.SetupSessionTokenAuthenticator();
            ((IIssuanceSecurityTokenAuthenticator) this.sessionTokenAuthenticator).IssuedSecurityTokenHandler = new IssuedSecurityTokenHandler(this.OnTokenIssued);
            ((IIssuanceSecurityTokenAuthenticator) this.sessionTokenAuthenticator).RenewedSecurityTokenHandler = new RenewedSecurityTokenHandler(this.OnTokenRenewed);
            this.acceptNewWork = true;
            System.ServiceModel.Security.SecurityUtils.OpenTokenAuthenticatorIfRequired(this.sessionTokenAuthenticator, helper.RemainingTime());
        }

        public void OnOpened()
        {
        }

        public void OnOpening()
        {
        }

        private void OnTimer(object state)
        {
            if ((this.communicationObject.State != CommunicationState.Closed) && (this.communicationObject.State != CommunicationState.Faulted))
            {
                try
                {
                    this.ClearPendingSessions();
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
                    if (((this.communicationObject.State != CommunicationState.Closed) && (this.communicationObject.State != CommunicationState.Closing)) && (this.communicationObject.State != CommunicationState.Faulted))
                    {
                        this.inactivityTimer.Set(this.inactivityTimeout);
                    }
                }
            }
        }

        private void OnTokenIssued(SecurityToken issuedToken, EndpointAddress tokenRequestor)
        {
            this.communicationObject.ThrowIfClosed();
            if (!this.acceptNewWork)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("SecurityListenerClosing")));
            }
            SecurityContextSecurityToken sessionToken = issuedToken as SecurityContextSecurityToken;
            if (sessionToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SessionTokenIsNotSecurityContextToken", new object[] { issuedToken.GetType(), typeof(SecurityContextSecurityToken) })));
            }
            IServerReliableChannelBinder channelBinder = this.CreateChannelBinder(sessionToken, tokenRequestor ?? EndpointAddress.AnonymousAddress);
            bool flag = false;
            try
            {
                this.AddPendingSession(sessionToken.ContextId, channelBinder);
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    channelBinder.Abort();
                }
            }
        }

        private void OnTokenRenewed(SecurityToken newToken, SecurityToken oldToken)
        {
            this.communicationObject.ThrowIfClosed();
            if (!this.acceptNewWork)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("SecurityListenerClosing")));
            }
            SecurityContextSecurityToken token = newToken as SecurityContextSecurityToken;
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SessionTokenIsNotSecurityContextToken", new object[] { newToken.GetType(), typeof(SecurityContextSecurityToken) })));
            }
            SecurityContextSecurityToken supportingToken = oldToken as SecurityContextSecurityToken;
            if (supportingToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SessionTokenIsNotSecurityContextToken", new object[] { oldToken.GetType(), typeof(SecurityContextSecurityToken) })));
            }
            IServerSecuritySessionChannel channel = this.FindSessionChannel(token.ContextId);
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("CannotFindSecuritySession", new object[] { token.ContextId })));
            }
            channel.RenewSessionToken(token, supportingToken);
        }

        internal void Open(TimeSpan timeout)
        {
            this.communicationObject.Open(timeout);
        }

        private bool RemovePendingSession(UniqueId sessionId)
        {
            bool flag;
            lock (this.ThisLock)
            {
                if (this.pendingSessions1.ContainsKey(sessionId))
                {
                    this.pendingSessions1.Remove(sessionId);
                    flag = true;
                }
                else if (this.pendingSessions2.ContainsKey(sessionId))
                {
                    this.pendingSessions2.Remove(sessionId);
                    flag = true;
                }
                else
                {
                    flag = false;
                }
            }
            if (flag)
            {
                SecurityTraceRecordHelper.TracePendingSessionActivated(sessionId, this.Uri);
            }
            return flag;
        }

        private void RemoveSessionChannel(string sessionId)
        {
            this.RemoveSessionChannel(new UniqueId(sessionId));
        }

        private void RemoveSessionChannel(UniqueId sessionId)
        {
            lock (this.ThisLock)
            {
                this.activeSessions.Remove(sessionId);
            }
            SecurityTraceRecordHelper.TraceActiveSessionRemoved(sessionId, this.Uri);
        }

        private void SetupSessionTokenAuthenticator()
        {
            RecipientServiceModelSecurityTokenRequirement requirement = new RecipientServiceModelSecurityTokenRequirement();
            this.issuedTokenParameters.InitializeSecurityTokenRequirement(requirement);
            requirement.KeyUsage = SecurityKeyUsage.Signature;
            requirement.ListenUri = this.listenUri;
            requirement.SecurityBindingElement = this.sessionProtocolFactory.SecurityBindingElement;
            requirement.SecurityAlgorithmSuite = this.sessionProtocolFactory.IncomingAlgorithmSuite;
            requirement.SupportSecurityContextCancellation = true;
            requirement.MessageSecurityVersion = this.sessionProtocolFactory.MessageSecurityVersion.SecurityTokenVersion;
            requirement.AuditLogLocation = this.sessionProtocolFactory.AuditLogLocation;
            requirement.SuppressAuditFailure = this.sessionProtocolFactory.SuppressAuditFailure;
            requirement.MessageAuthenticationAuditLevel = this.sessionProtocolFactory.MessageAuthenticationAuditLevel;
            requirement.Properties[ServiceModelSecurityTokenRequirement.MessageDirectionProperty] = MessageDirection.Input;
            if (this.sessionProtocolFactory.EndpointFilterTable != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.EndpointFilterTableProperty] = this.sessionProtocolFactory.EndpointFilterTable;
            }
            this.sessionTokenAuthenticator = this.sessionProtocolFactory.SecurityTokenManager.CreateSecurityTokenAuthenticator(requirement, out this.sessionTokenResolver);
            if (!(this.sessionTokenAuthenticator is IIssuanceSecurityTokenAuthenticator))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecuritySessionRequiresIssuanceAuthenticator", new object[] { typeof(IIssuanceSecurityTokenAuthenticator), this.sessionTokenAuthenticator.GetType() })));
            }
            if ((this.sessionTokenResolver == null) || !(this.sessionTokenResolver is ISecurityContextSecurityTokenCache))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecuritySessionRequiresSecurityContextTokenCache", new object[] { this.sessionTokenResolver.GetType(), typeof(ISecurityContextSecurityTokenCache) })));
            }
            this.sessionTokenCache = (ISecurityContextSecurityTokenCache) this.sessionTokenResolver;
        }

        public void StopAcceptingNewWork()
        {
            this.acceptNewWork = false;
        }

        private void TryCloseBinder(IServerReliableChannelBinder binder, TimeSpan timeout)
        {
            bool flag = false;
            try
            {
                binder.Close(timeout);
            }
            catch (CommunicationException exception)
            {
                flag = true;
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception2)
            {
                flag = true;
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            finally
            {
                if (flag)
                {
                    binder.Abort();
                }
            }
        }

        public bool CanRenewSession
        {
            get
            {
                return this.canRenewSession;
            }
            set
            {
                this.canRenewSession = value;
            }
        }

        internal System.ServiceModel.Channels.ChannelBuilder ChannelBuilder
        {
            get
            {
                return this.channelBuilder;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.channelBuilder = value;
            }
        }

        public TimeSpan CloseTimeout
        {
            get
            {
                return this.closeTimeout;
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

        public TimeSpan InactivityTimeout
        {
            get
            {
                return this.inactivityTimeout;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.inactivityTimeout = value;
            }
        }

        public SecurityTokenParameters IssuedSecurityTokenParameters
        {
            get
            {
                return this.issuedTokenParameters;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.issuedTokenParameters = value;
            }
        }

        public TimeSpan KeyRolloverInterval
        {
            get
            {
                return this.keyRolloverInterval;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.keyRolloverInterval = value;
            }
        }

        public TimeSpan MaximumKeyRenewalInterval
        {
            get
            {
                return this.maximumKeyRenewalInterval;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.maximumKeyRenewalInterval = value;
            }
        }

        public int MaximumPendingKeysPerSession
        {
            get
            {
                return this.maximumPendingKeysPerSession;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeGreaterThanZero")));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.maximumPendingKeysPerSession = value;
            }
        }

        public int MaximumPendingSessions
        {
            get
            {
                return this.maximumPendingSessions;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.maximumPendingSessions = value;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public TimeSpan OpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
        }

        internal ChannelListenerBase SecurityChannelListener
        {
            get
            {
                return this.securityChannelListener;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.securityChannelListener = value;
            }
        }

        public System.ServiceModel.Security.SecurityStandardsManager SecurityStandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.standardsManager = value;
            }
        }

        public TimeSpan SendTimeout
        {
            get
            {
                return this.sendTimeout;
            }
        }

        public SecurityProtocolFactory SessionProtocolFactory
        {
            get
            {
                return this.sessionProtocolFactory;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.sessionProtocolFactory = value;
            }
        }

        public SecurityTokenAuthenticator SessionTokenAuthenticator
        {
            get
            {
                return this.sessionTokenAuthenticator;
            }
        }

        public ISecurityContextSecurityTokenCache SessionTokenCache
        {
            get
            {
                return this.sessionTokenCache;
            }
        }

        public SecurityTokenResolver SessionTokenResolver
        {
            get
            {
                return this.sessionTokenResolver;
            }
        }

        internal SecurityListenerSettingsLifetimeManager SettingsLifetimeManager
        {
            get
            {
                return this.settingsLifetimeManager;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.settingsLifetimeManager = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public bool TolerateTransportFailures
        {
            get
            {
                return this.tolerateTransportFailures;
            }
            set
            {
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.tolerateTransportFailures = value;
            }
        }

        private System.Uri Uri
        {
            get
            {
                this.communicationObject.ThrowIfNotOpened();
                return this.listenUri;
            }
        }

        private interface IInputQueueChannelAcceptor
        {
            int PendingCount { get; }
        }

        private interface IServerSecuritySessionChannel
        {
            void RenewSessionToken(SecurityContextSecurityToken newToken, SecurityContextSecurityToken supportingToken);
        }

        private class SecurityReplySessionChannel : SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel, IReplySessionChannel, IReplyChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
        {
            public SecurityReplySessionChannel(SecuritySessionServerSettings settings, IServerReliableChannelBinder channelBinder, SecurityContextSecurityToken sessionToken, object listenerSecurityState, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(settings, channelBinder, sessionToken, listenerSecurityState, settingsLifetimeManager)
            {
            }

            public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.ChannelBinder.BeginWaitForRequest(timeout, callback, state);
            }

            public bool EndWaitForRequest(IAsyncResult result)
            {
                return base.ChannelBinder.EndWaitForRequest(result);
            }

            public bool WaitForRequest(TimeSpan timeout)
            {
                return base.ChannelBinder.WaitForRequest(timeout);
            }

            protected override bool CanDoSecurityCorrelation
            {
                get
                {
                    return true;
                }
            }
        }

        private class SecuritySessionChannelAcceptor<T> : InputQueueChannelAcceptor<T>, SecuritySessionServerSettings.IInputQueueChannelAcceptor where T: class, IChannel
        {
            private object listenerState;

            public SecuritySessionChannelAcceptor(ChannelListenerBase manager, object listenerState) : base(manager)
            {
                this.listenerState = listenerState;
            }

            public object ListenerSecurityState
            {
                get
                {
                    return this.listenerState;
                }
            }

            int SecuritySessionServerSettings.IInputQueueChannelAcceptor.PendingCount
            {
                get
                {
                    return base.PendingCount;
                }
            }
        }

        internal class SecuritySessionDemuxFailureHandler : IChannelDemuxFailureHandler
        {
            private SecurityStandardsManager standardsManager;

            public SecuritySessionDemuxFailureHandler(SecurityStandardsManager standardsManager)
            {
                if (standardsManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("standardsManager");
                }
                this.standardsManager = standardsManager;
            }

            public IAsyncResult BeginHandleDemuxFailure(Message message, IOutputChannel faultContext, AsyncCallback callback, object state)
            {
                return this.BeginHandleDemuxFailure<IOutputChannel>(message, faultContext, callback, state);
            }

            public IAsyncResult BeginHandleDemuxFailure(Message message, RequestContext faultContext, AsyncCallback callback, object state)
            {
                return this.BeginHandleDemuxFailure<RequestContext>(message, faultContext, callback, state);
            }

            private IAsyncResult BeginHandleDemuxFailure<TFaultContext>(Message message, TFaultContext faultContext, AsyncCallback callback, object state)
            {
                this.HandleDemuxFailure(message);
                return new SendFaultAsyncResult<TFaultContext>(this.CreateSessionDemuxFaultMessage(message), faultContext, callback, state);
            }

            public Message CreateSessionDemuxFaultMessage(Message message)
            {
                MessageFault fault = System.ServiceModel.Security.SecurityUtils.CreateSecurityContextNotFoundFault(this.standardsManager, message.Headers.Action);
                Message message2 = Message.CreateMessage(message.Version, fault, message.Version.Addressing.DefaultFaultAction);
                if (message.Headers.MessageId != null)
                {
                    message2.InitializeReply(message);
                }
                return message2;
            }

            public void EndHandleDemuxFailure(IAsyncResult result)
            {
                if (result is SendFaultAsyncResult<RequestContext>)
                {
                    SendFaultAsyncResult<RequestContext>.End(result);
                }
                else
                {
                    if (!(result is SendFaultAsyncResult<IOutputChannel>))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidAsyncResult"), "result"));
                    }
                    SendFaultAsyncResult<IOutputChannel>.End(result);
                }
            }

            public void HandleDemuxFailure(Message message)
            {
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70052, System.ServiceModel.SR.GetString("TraceCodeSecuritySessionDemuxFailure"), message);
                }
            }

            private class SendFaultAsyncResult<TFaultContext> : AsyncResult
            {
                private TFaultContext faultContext;
                private Message message;
                private static AsyncCallback sendCallback;

                static SendFaultAsyncResult()
                {
                    SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler.SendFaultAsyncResult<TFaultContext>.sendCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler.SendFaultAsyncResult<TFaultContext>.SendCallback));
                }

                public SendFaultAsyncResult(Message fault, TFaultContext faultContext, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.faultContext = faultContext;
                    this.message = fault;
                    IAsyncResult result = this.BeginSend(fault);
                    if (result.CompletedSynchronously)
                    {
                        this.EndSend(result);
                        base.Complete(true);
                    }
                }

                private IAsyncResult BeginSend(Message message)
                {
                    IAsyncResult result2;
                    bool flag = true;
                    try
                    {
                        IAsyncResult result = null;
                        if (this.faultContext is RequestContext)
                        {
                            result = ((RequestContext) this.faultContext).BeginReply(message, SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler.SendFaultAsyncResult<TFaultContext>.sendCallback, this);
                        }
                        else
                        {
                            result = ((IOutputChannel) this.faultContext).BeginSend(message, SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler.SendFaultAsyncResult<TFaultContext>.sendCallback, this);
                        }
                        flag = false;
                        result2 = result;
                    }
                    finally
                    {
                        if (flag && (message != null))
                        {
                            message.Close();
                        }
                    }
                    return result2;
                }

                internal static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler.SendFaultAsyncResult<TFaultContext>>(result);
                }

                private void EndSend(IAsyncResult result)
                {
                    using (this.message)
                    {
                        if (this.faultContext is RequestContext)
                        {
                            ((RequestContext) this.faultContext).EndReply(result);
                        }
                        else
                        {
                            ((IOutputChannel) this.faultContext).EndSend(result);
                        }
                    }
                }

                private static void SendCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler.SendFaultAsyncResult<TFaultContext> asyncState = (SecuritySessionServerSettings.SecuritySessionDemuxFailureHandler.SendFaultAsyncResult<TFaultContext>) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.EndSend(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }

        private class SecuritySessionRequestContext : RequestContextBase
        {
            private SecuritySessionServerSettings.ServerSecuritySessionChannel channel;
            private SecurityProtocolCorrelationState correlationState;
            private RequestContext requestContext;

            public SecuritySessionRequestContext(RequestContext requestContext, Message requestMessage, SecurityProtocolCorrelationState correlationState, SecuritySessionServerSettings.ServerSecuritySessionChannel channel) : base(requestMessage, channel.InternalCloseTimeout, channel.InternalSendTimeout)
            {
                this.requestContext = requestContext;
                this.correlationState = correlationState;
                this.channel = channel;
            }

            protected override void OnAbort()
            {
                this.requestContext.Abort();
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (message != null)
                {
                    this.channel.SecureApplicationMessage(ref message, helper.RemainingTime(), this.correlationState);
                }
                return this.requestContext.BeginReply(message, helper.RemainingTime(), callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.requestContext.Close(timeout);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                this.requestContext.EndReply(result);
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (message != null)
                {
                    this.channel.SecureApplicationMessage(ref message, helper.RemainingTime(), this.correlationState);
                }
                this.requestContext.Reply(message, helper.RemainingTime());
            }
        }

        private class ServerSecurityDuplexSessionChannel : SecuritySessionServerSettings.ServerSecuritySessionChannel, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
        {
            private RequestContext closeRequestContext;
            private Message closeResponseMessage;
            private InterruptibleWaitObject inputSessionCloseHandle;
            private bool isInputClosed;
            private bool isOutputClosed;
            private InterruptibleWaitObject outputSessionCloseHandle;
            private bool receivedClose;
            private bool sentClose;
            private SoapSecurityServerDuplexSession session;

            public ServerSecurityDuplexSessionChannel(SecuritySessionServerSettings settings, IServerReliableChannelBinder channelBinder, SecurityContextSecurityToken sessionToken, object listenerSecurityState, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(settings, channelBinder, sessionToken, listenerSecurityState, settingsLifetimeManager)
            {
                this.outputSessionCloseHandle = new InterruptibleWaitObject(true);
                this.inputSessionCloseHandle = new InterruptibleWaitObject(false);
                this.session = new SoapSecurityServerDuplexSession(sessionToken, settings, this);
            }

            protected override void AbortCore()
            {
                base.AbortCore();
                base.Settings.RemoveSessionChannel(this.session.Id);
                this.CleanupPendingCloseState();
            }

            private IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseOutputSessionAsyncResult(this, timeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.CheckOutputOpen();
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.SecureApplicationMessage(ref message, helper.RemainingTime(), null);
                return base.ChannelBinder.BeginSend(message, helper.RemainingTime(), callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.ChannelBinder.BeginWaitForRequest(timeout, callback, state);
            }

            protected void CheckOutputOpen()
            {
                base.ThrowIfClosedOrNotOpen();
                lock (base.ThisLock)
                {
                    if (this.isOutputClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(System.ServiceModel.SR.GetString("OutputNotExpected")));
                    }
                }
            }

            private void CleanupPendingCloseState()
            {
                lock (base.ThisLock)
                {
                    if (this.closeResponseMessage != null)
                    {
                        this.closeResponseMessage.Close();
                        this.closeResponseMessage = null;
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                        this.closeRequestContext = null;
                    }
                }
            }

            protected override void CloseCore(TimeSpan timeout)
            {
                base.CloseCore(timeout);
                this.inputSessionCloseHandle.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
                base.Settings.RemoveSessionChannel(this.session.Id);
            }

            private void CloseOutputSession(TimeSpan timeout)
            {
                bool sendClose = false;
                bool sendCloseResponse = false;
                try
                {
                    Message message;
                    RequestContext context;
                    this.DetermineCloseOutputSessionMessage(out sendClose, out sendCloseResponse, out message, out context);
                    if (sendCloseResponse)
                    {
                        bool flag3 = true;
                        try
                        {
                            base.SendCloseResponse(context, message, timeout);
                            flag3 = false;
                            return;
                        }
                        finally
                        {
                            if (flag3)
                            {
                                message.Close();
                                context.Abort();
                            }
                        }
                    }
                    if (sendClose)
                    {
                        base.SendClose(timeout);
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (base.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                }
                finally
                {
                    if (sendClose || sendCloseResponse)
                    {
                        this.outputSessionCloseHandle.Set();
                    }
                }
            }

            private void DetermineCloseOutputSessionMessage(out bool sendClose, out bool sendCloseResponse, out Message pendingCloseResponseMessage, out RequestContext pendingCloseRequestContext)
            {
                sendClose = false;
                sendCloseResponse = false;
                pendingCloseResponseMessage = null;
                pendingCloseRequestContext = null;
                lock (base.ThisLock)
                {
                    if (!this.isOutputClosed)
                    {
                        this.isOutputClosed = true;
                        if (this.receivedClose)
                        {
                            if (this.closeResponseMessage != null)
                            {
                                pendingCloseResponseMessage = this.closeResponseMessage;
                                pendingCloseRequestContext = this.closeRequestContext;
                                this.closeResponseMessage = null;
                                this.closeRequestContext = null;
                                sendCloseResponse = true;
                            }
                        }
                        else
                        {
                            sendClose = true;
                            this.sentClose = true;
                        }
                        this.outputSessionCloseHandle.Reset();
                    }
                }
            }

            protected override void EndCloseCore(IAsyncResult result)
            {
                base.EndCloseCore(result);
                this.inputSessionCloseHandle.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
                base.Settings.RemoveSessionChannel(this.session.Id);
            }

            private void EndCloseOutputSession(IAsyncResult result)
            {
                CloseOutputSessionAsyncResult.End(result);
            }

            public void EndSend(IAsyncResult result)
            {
                base.ChannelBinder.EndSend(result);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return base.ChannelBinder.EndWaitForRequest(result);
            }

            protected override void OnAbort()
            {
                this.AbortCore();
                this.inputSessionCloseHandle.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseAsyncResult(this, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.CloseOutputSession(helper.RemainingTime());
                if (base.State != CommunicationState.Closed)
                {
                    bool flag;
                    bool flag2 = this.WaitForInputSessionClose(helper.RemainingTime(), out flag);
                    if (!flag)
                    {
                        if (!flag2)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { helper.OriginalTimeout })));
                        }
                        bool flag3 = this.WaitForOutputSessionClose(helper.RemainingTime(), out flag);
                        if (!flag)
                        {
                            if (!flag3)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseOutputSessionTimeout", new object[] { helper.OriginalTimeout })));
                            }
                            this.CloseCore(helper.RemainingTime());
                        }
                    }
                }
            }

            protected override void OnCloseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                if (base.State == CommunicationState.Created)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ServerReceivedCloseMessageStateIsCreated", new object[] { base.GetType().ToString() })));
                }
                TimeoutHelper helper = new TimeoutHelper(timeout);
                bool flag = false;
                bool flag2 = true;
                try
                {
                    lock (base.ThisLock)
                    {
                        this.receivedClose = true;
                        if (!this.isInputClosed)
                        {
                            this.isInputClosed = true;
                            flag = true;
                            if (!this.isOutputClosed)
                            {
                                this.closeRequestContext = requestContext;
                                this.closeResponseMessage = base.CreateCloseResponse(message, null, helper.RemainingTime());
                                flag2 = false;
                            }
                        }
                    }
                    if (flag)
                    {
                        this.inputSessionCloseHandle.Set();
                    }
                    if (flag2)
                    {
                        requestContext.Close(helper.RemainingTime());
                        flag2 = false;
                    }
                }
                finally
                {
                    message.Close();
                    if (flag2)
                    {
                        requestContext.Abort();
                    }
                }
            }

            protected override void OnCloseResponseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                bool flag = true;
                try
                {
                    bool sentClose = false;
                    bool flag3 = false;
                    lock (base.ThisLock)
                    {
                        sentClose = this.sentClose;
                        if (sentClose && !this.isInputClosed)
                        {
                            this.isInputClosed = true;
                            flag3 = true;
                        }
                    }
                    if (!sentClose)
                    {
                        base.Fault(new ProtocolException(System.ServiceModel.SR.GetString("UnexpectedSecuritySessionCloseResponse")));
                    }
                    else
                    {
                        if (flag3)
                        {
                            this.inputSessionCloseHandle.Set();
                        }
                        requestContext.Close(timeout);
                        flag = false;
                    }
                }
                finally
                {
                    message.Close();
                    if (flag)
                    {
                        requestContext.Abort();
                    }
                }
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CloseAsyncResult.End(result);
            }

            protected override void OnFaulted()
            {
                this.AbortCore();
                this.inputSessionCloseHandle.Fault(this);
                this.outputSessionCloseHandle.Fault(this);
                base.OnFaulted();
            }

            public void Send(Message message)
            {
                this.Send(message, base.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                this.CheckOutputOpen();
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.SecureApplicationMessage(ref message, helper.RemainingTime(), null);
                base.ChannelBinder.Send(message, helper.RemainingTime());
            }

            private bool WaitForInputSessionClose(TimeSpan timeout, out bool wasAborted)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                wasAborted = false;
                try
                {
                    Message message;
                    if (!base.TryReceive(helper.RemainingTime(), out message))
                    {
                        return false;
                    }
                    if (message != null)
                    {
                        using (message)
                        {
                            throw TraceUtility.ThrowHelperWarning(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                        }
                    }
                    if (!this.inputSessionCloseHandle.Wait(helper.RemainingTime(), false))
                    {
                        return false;
                    }
                    lock (base.ThisLock)
                    {
                        if (!this.isInputClosed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ShutdownRequestWasNotReceived")));
                        }
                    }
                    return true;
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (base.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                    wasAborted = true;
                }
                return false;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return base.ChannelBinder.WaitForRequest(timeout);
            }

            internal bool WaitForOutputSessionClose(TimeSpan timeout, out bool wasAborted)
            {
                wasAborted = false;
                try
                {
                    return this.outputSessionCloseHandle.Wait(timeout, false);
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (base.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                    wasAborted = true;
                    return true;
                }
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return base.ChannelBinder.RemoteAddress;
                }
            }

            public IDuplexSession Session
            {
                get
                {
                    return this.session;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.RemoteAddress.Uri;
                }
            }

            private class CloseAsyncResult : AsyncResult
            {
                private static readonly AsyncCallback closeCoreCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult.CloseCoreCallback));
                private static readonly AsyncCallback closeOutputSessionCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult.CloseOutputSessionCallback));
                private static readonly AsyncCallback inputSessionWaitCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult.WaitForInputSessionCloseCallback));
                private static readonly AsyncCallback outputSessionWaitCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult.WaitForOutputSessionCloseCallback));
                private static readonly AsyncCallback receiveCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult.ReceiveCallback));
                private SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel sessionChannel;
                private TimeoutHelper timeoutHelper;

                public CloseAsyncResult(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    bool flag = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.BeginCloseOutputSession(this.timeoutHelper.RemainingTime(), closeOutputSessionCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        this.sessionChannel.EndCloseOutputSession(result);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        flag = true;
                    }
                    if (flag || this.OnOutputSessionClosed())
                    {
                        base.Complete(true);
                    }
                }

                private static void CloseCoreCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.sessionChannel.EndCloseCore(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }

                private static void CloseOutputSessionCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            bool flag2 = false;
                            try
                            {
                                asyncState.sessionChannel.Session.EndCloseOutputSession(result);
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag = true;
                                flag2 = true;
                            }
                            if (!flag2)
                            {
                                flag = asyncState.OnOutputSessionClosed();
                            }
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

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult>(result);
                }

                private bool OnInputSessionWaitOver(bool inputSessionClosed)
                {
                    if (!inputSessionClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { this.timeoutHelper.OriginalTimeout })));
                    }
                    lock (this.sessionChannel.ThisLock)
                    {
                        if (!this.sessionChannel.isInputClosed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ShutdownRequestWasNotReceived")));
                        }
                    }
                    bool outputSessionClosed = false;
                    bool flag2 = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.outputSessionCloseHandle.BeginWait(this.timeoutHelper.RemainingTime(), true, outputSessionWaitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.outputSessionCloseHandle.EndWait(result);
                        outputSessionClosed = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        flag2 = true;
                    }
                    catch (TimeoutException)
                    {
                        outputSessionClosed = false;
                    }
                    return (flag2 || this.OnOutputSessionWaitOver(outputSessionClosed));
                }

                private bool OnMessageReceived(Message message)
                {
                    if (message != null)
                    {
                        using (message)
                        {
                            throw TraceUtility.ThrowHelperWarning(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                        }
                    }
                    bool flag = false;
                    bool inputSessionClosed = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.inputSessionCloseHandle.BeginWait(this.timeoutHelper.RemainingTime(), inputSessionWaitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        try
                        {
                            this.sessionChannel.inputSessionCloseHandle.EndWait(result);
                            inputSessionClosed = true;
                        }
                        catch (TimeoutException)
                        {
                            inputSessionClosed = false;
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        flag = true;
                    }
                    return (flag || this.OnInputSessionWaitOver(inputSessionClosed));
                }

                private bool OnOutputSessionClosed()
                {
                    bool flag = false;
                    Message message = null;
                    bool flag2 = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.BeginTryReceive(this.timeoutHelper.RemainingTime(), receiveCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        flag2 = this.sessionChannel.EndTryReceive(result, out message);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        flag = true;
                    }
                    if (flag)
                    {
                        return true;
                    }
                    if (!flag2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { this.timeoutHelper.OriginalTimeout })));
                    }
                    return this.OnMessageReceived(message);
                }

                private bool OnOutputSessionWaitOver(bool outputSessionClosed)
                {
                    if (!outputSessionClosed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseOutputSessionTimeout", new object[] { this.timeoutHelper.OriginalTimeout })));
                    }
                    IAsyncResult result = this.sessionChannel.BeginCloseCore(this.timeoutHelper.RemainingTime(), closeCoreCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.sessionChannel.EndCloseCore(result);
                    return true;
                }

                private static void ReceiveCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            Message message = null;
                            bool flag2 = false;
                            bool flag3 = false;
                            try
                            {
                                flag2 = asyncState.sessionChannel.EndTryReceive(result, out message);
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag = true;
                                flag3 = true;
                            }
                            if (!flag3)
                            {
                                if (!flag2)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { asyncState.timeoutHelper.OriginalTimeout })));
                                }
                                flag = asyncState.OnMessageReceived(message);
                            }
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

                private static void WaitForInputSessionCloseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        bool inputSessionClosed = false;
                        try
                        {
                            bool flag3 = false;
                            try
                            {
                                asyncState.sessionChannel.inputSessionCloseHandle.EndWait(result);
                                inputSessionClosed = true;
                            }
                            catch (TimeoutException)
                            {
                                inputSessionClosed = false;
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag3 = true;
                                flag = true;
                            }
                            if (!flag3)
                            {
                                flag = asyncState.OnInputSessionWaitOver(inputSessionClosed);
                            }
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

                private static void WaitForOutputSessionCloseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        bool outputSessionClosed = false;
                        try
                        {
                            bool flag3 = false;
                            try
                            {
                                asyncState.sessionChannel.outputSessionCloseHandle.EndWait(result);
                                outputSessionClosed = true;
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag3 = true;
                                flag = true;
                            }
                            catch (TimeoutException)
                            {
                                outputSessionClosed = false;
                            }
                            if (!flag3)
                            {
                                flag = asyncState.OnOutputSessionWaitOver(outputSessionClosed);
                            }
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
            }

            private class CloseOutputSessionAsyncResult : AsyncResult
            {
                private RequestContext closeRequestContext;
                private Message closeResponseMessage;
                private static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseOutputSessionAsyncResult.SendCallback));
                private bool sendClose;
                private bool sendCloseResponse;
                private SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel sessionChannel;
                private TimeoutHelper timeoutHelper;

                public CloseOutputSessionAsyncResult(SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.sessionChannel = sessionChannel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel.DetermineCloseOutputSessionMessage(out this.sendClose, out this.sendCloseResponse, out this.closeResponseMessage, out this.closeRequestContext);
                    if (!this.sendClose && !this.sendCloseResponse)
                    {
                        base.Complete(true);
                    }
                    else
                    {
                        bool flag = true;
                        try
                        {
                            IAsyncResult result = this.BeginSend(sendCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                flag = false;
                                return;
                            }
                            this.EndSend(result);
                        }
                        finally
                        {
                            if (flag)
                            {
                                this.Cleanup();
                            }
                        }
                        base.Complete(true);
                    }
                }

                private IAsyncResult BeginSend(AsyncCallback callback, object state)
                {
                    if (this.sendClose)
                    {
                        return this.sessionChannel.BeginSendClose(this.timeoutHelper.RemainingTime(), callback, state);
                    }
                    return this.sessionChannel.BeginSendCloseResponse(this.closeRequestContext, this.closeResponseMessage, this.timeoutHelper.RemainingTime(), callback, state);
                }

                private void Cleanup()
                {
                    if (this.closeResponseMessage != null)
                    {
                        this.closeResponseMessage.Close();
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                    }
                    this.sessionChannel.outputSessionCloseHandle.Set();
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseOutputSessionAsyncResult>(result);
                }

                private void EndSend(IAsyncResult result)
                {
                    if (this.sendClose)
                    {
                        this.sessionChannel.EndSendClose(result);
                    }
                    else
                    {
                        this.sessionChannel.EndSendCloseResponse(result);
                    }
                }

                private static void SendCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseOutputSessionAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel.CloseOutputSessionAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.EndSend(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Cleanup();
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private class SoapSecurityServerDuplexSession : SecuritySessionServerSettings.ServerSecuritySessionChannel.SoapSecurityInputSession, IDuplexSession, IInputSession, IOutputSession, ISession
            {
                private SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel channel;

                public SoapSecurityServerDuplexSession(SecurityContextSecurityToken sessionToken, SecuritySessionServerSettings settings, SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel channel) : base(sessionToken, settings, channel)
                {
                    this.channel = channel;
                }

                public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
                {
                    return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
                }

                public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception exception = null;
                    try
                    {
                        return this.channel.BeginCloseOutputSession(timeout, callback, state);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (exception == null)
                    {
                        return null;
                    }
                    this.channel.Fault(exception);
                    if (exception is CommunicationException)
                    {
                        throw exception;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(exception);
                }

                public void CloseOutputSession()
                {
                    this.CloseOutputSession(this.channel.DefaultCloseTimeout);
                }

                public void CloseOutputSession(TimeSpan timeout)
                {
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception exception = null;
                    try
                    {
                        this.channel.CloseOutputSession(timeout);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (exception != null)
                    {
                        this.channel.Fault(exception);
                        if (exception is CommunicationException)
                        {
                            throw exception;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(exception);
                    }
                }

                public void EndCloseOutputSession(IAsyncResult result)
                {
                    Exception exception = null;
                    try
                    {
                        this.channel.EndCloseOutputSession(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (exception != null)
                    {
                        this.channel.Fault(exception);
                        if (exception is CommunicationException)
                        {
                            throw exception;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(exception);
                    }
                }
            }
        }

        private abstract class ServerSecuritySessionChannel : ChannelBase, SecuritySessionServerSettings.IServerSecuritySessionChannel
        {
            private volatile bool areFaultCodesInitialized;
            private IServerReliableChannelBinder channelBinder;
            private SecurityContextSecurityToken currentSessionToken;
            private List<SecurityContextSecurityToken> futureSessionTokens;
            private volatile bool hasSecurityStateReference;
            private RequestContext initialRequestContext;
            private volatile bool isInputClosed;
            private MessageVersion messageVersion;
            private ThreadNeutralSemaphore receiveLock;
            private FaultCode renewFaultCode;
            private FaultReason renewFaultReason;
            private SecurityProtocol securityProtocol;
            private FaultCode sessionAbortedFaultCode;
            private FaultReason sessionAbortedFaultReason;
            private UniqueId sessionId;
            private SecuritySessionServerSettings settings;
            private SecurityListenerSettingsLifetimeManager settingsLifetimeManager;

            protected ServerSecuritySessionChannel(SecuritySessionServerSettings settings, IServerReliableChannelBinder channelBinder, SecurityContextSecurityToken sessionToken, object listenerSecurityProtocolState, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(settings.SecurityChannelListener)
            {
                this.settings = settings;
                this.channelBinder = channelBinder;
                this.messageVersion = settings.MessageVersion;
                this.channelBinder.Faulted += new BinderExceptionHandler(this.OnInnerFaulted);
                this.securityProtocol = this.Settings.SessionProtocolFactory.CreateSecurityProtocol(null, null, listenerSecurityProtocolState, true, TimeSpan.Zero);
                if (!(this.securityProtocol is IAcceptorSecuritySessionProtocol))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProtocolMisMatch", new object[] { "IAcceptorSecuritySessionProtocol", base.GetType().ToString() })));
                }
                this.currentSessionToken = sessionToken;
                this.sessionId = sessionToken.ContextId;
                this.futureSessionTokens = new List<SecurityContextSecurityToken>(1);
                ((IAcceptorSecuritySessionProtocol) this.securityProtocol).SetOutgoingSessionToken(sessionToken);
                ((IAcceptorSecuritySessionProtocol) this.securityProtocol).SetSessionTokenAuthenticator(this.sessionId, this.settings.SessionTokenAuthenticator, this.settings.SessionTokenResolver);
                this.settingsLifetimeManager = settingsLifetimeManager;
                this.receiveLock = new ThreadNeutralSemaphore(1);
            }

            protected virtual void AbortCore()
            {
                if (this.channelBinder != null)
                {
                    this.channelBinder.Abort();
                }
                if (this.securityProtocol != null)
                {
                    this.securityProtocol.Close(true, TimeSpan.Zero);
                }
                this.Settings.SessionTokenCache.RemoveAllContexts(this.currentSessionToken.ContextId);
                bool flag = false;
                lock (base.ThisLock)
                {
                    if (this.hasSecurityStateReference)
                    {
                        flag = true;
                        this.hasSecurityStateReference = false;
                    }
                }
                if (flag)
                {
                    this.settingsLifetimeManager.Abort();
                }
            }

            protected virtual IAsyncResult BeginCloseCore(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseCoreAsyncResult(this, timeout, callback, state);
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.BeginTryReceive(timeout, callback, state);
            }

            public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
            {
                return this.BeginReceiveRequest(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.BeginTryReceiveRequest(timeout, callback, state);
            }

            internal IAsyncResult BeginSendClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    Message response = this.CreateCloseMessage(helper.RemainingTime());
                    return this.BeginSendMessage(null, response, helper.RemainingTime(), callback, state);
                }
                catch (CommunicationException exception)
                {
                    this.TraceSessionClosedFailure(exception);
                }
                catch (TimeoutException exception2)
                {
                    this.TraceSessionClosedFailure(exception2);
                }
                return new CompletedAsyncResult(callback, state);
            }

            internal IAsyncResult BeginSendCloseResponse(RequestContext requestContext, Message closeResponse, TimeSpan timeout, AsyncCallback callback, object state)
            {
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    return this.BeginSendMessage(requestContext, closeResponse, helper.RemainingTime(), callback, state);
                }
                catch (CommunicationException exception)
                {
                    this.TraceSessionClosedResponseFailure(exception);
                }
                catch (TimeoutException exception2)
                {
                    this.TraceSessionClosedResponseFailure(exception2);
                }
                return new CompletedAsyncResult(callback, state);
            }

            internal IAsyncResult BeginSendMessage(RequestContext requestContext, Message response, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendMessageAsyncResult(this, requestContext, response, timeout, callback, state);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReceiveRequestAsyncResult(this, timeout, callback, state);
            }

            public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReceiveRequestAsyncResult(this, timeout, callback, state);
            }

            private bool CheckIncomingToken(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                SecurityMessageProperty security = message.Properties.Security;
                SecurityContextSecurityToken sessionToken = this.GetSessionToken(security);
                if (sessionToken == null)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSessionTokenPresentInMessage")), message);
                }
                if ((sessionToken.KeyExpirationTime < DateTime.UtcNow) && (message.Headers.Action != this.settings.SecurityStandardsManager.SecureConversationDriver.CloseAction.Value))
                {
                    if (!this.settings.CanRenewSession)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SessionKeyExpiredException(System.ServiceModel.SR.GetString("SecurityContextKeyExpired", new object[] { sessionToken.ContextId, sessionToken.KeyGeneration })));
                    }
                    this.SendRenewFault(requestContext, correlationState, timeout);
                    return false;
                }
                lock (base.ThisLock)
                {
                    if ((this.futureSessionTokens.Count > 0) && (sessionToken.KeyGeneration != this.currentSessionToken.KeyGeneration))
                    {
                        bool flag = false;
                        for (int i = 0; i < this.futureSessionTokens.Count; i++)
                        {
                            if (this.futureSessionTokens[i].KeyGeneration == sessionToken.KeyGeneration)
                            {
                                DateTime expirationTime = TimeoutHelper.Add(DateTime.UtcNow, this.settings.KeyRolloverInterval);
                                this.settings.SessionTokenCache.UpdateContextCachingTime(this.currentSessionToken, expirationTime);
                                this.currentSessionToken = this.futureSessionTokens[i];
                                this.futureSessionTokens.RemoveAt(i);
                                ((IAcceptorSecuritySessionProtocol) this.securityProtocol).SetOutgoingSessionToken(this.currentSessionToken);
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            SecurityTraceRecordHelper.TraceServerSessionKeyUpdated(this.currentSessionToken, this.GetLocalUri());
                            for (int j = 0; j < this.futureSessionTokens.Count; j++)
                            {
                                this.Settings.SessionTokenCache.RemoveContext(this.futureSessionTokens[j].ContextId, this.futureSessionTokens[j].KeyGeneration);
                            }
                            this.futureSessionTokens.Clear();
                        }
                    }
                }
                return true;
            }

            internal void CheckOutgoingToken()
            {
                lock (base.ThisLock)
                {
                    if (this.currentSessionToken.KeyExpirationTime < DateTime.UtcNow)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SessionKeyExpiredException(System.ServiceModel.SR.GetString("SecuritySessionKeyIsStale")));
                    }
                }
            }

            protected virtual void CloseCore(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                try
                {
                    if (this.channelBinder != null)
                    {
                        this.channelBinder.Close(helper.RemainingTime());
                    }
                    if (this.securityProtocol != null)
                    {
                        this.securityProtocol.Close(false, helper.RemainingTime());
                    }
                    bool flag = false;
                    lock (base.ThisLock)
                    {
                        if (this.hasSecurityStateReference)
                        {
                            flag = true;
                            this.hasSecurityStateReference = false;
                        }
                    }
                    if (flag)
                    {
                        this.settingsLifetimeManager.Close(helper.RemainingTime());
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (base.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                }
                this.Settings.SessionTokenCache.RemoveAllContexts(this.currentSessionToken.ContextId);
            }

            internal Message CreateCloseMessage(TimeSpan timeout)
            {
                RequestSecurityToken body = new RequestSecurityToken(this.Settings.SecurityStandardsManager) {
                    RequestType = this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose,
                    CloseTarget = this.Settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(this.currentSessionToken, SecurityTokenReferenceStyle.External)
                };
                body.MakeReadOnly();
                Message request = Message.CreateMessage(this.messageVersion, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction, this.messageVersion.Addressing), body);
                RequestReplyCorrelator.PrepareRequest(request);
                if (this.LocalAddress != null)
                {
                    request.Headers.ReplyTo = this.LocalAddress;
                }
                else if (request.Version.Addressing == AddressingVersion.WSAddressing10)
                {
                    request.Headers.ReplyTo = null;
                }
                else
                {
                    if (request.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { request.Version.Addressing })));
                    }
                    request.Headers.ReplyTo = EndpointAddress.AnonymousAddress;
                }
                this.securityProtocol.SecureOutgoingMessage(ref request, timeout, null);
                request.Properties.AllowOutputBatching = false;
                return request;
            }

            internal Message CreateCloseResponse(Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                using (message)
                {
                    Message message2 = this.ProcessCloseRequest(message);
                    this.securityProtocol.SecureOutgoingMessage(ref message2, timeout, correlationState);
                    message2.Properties.AllowOutputBatching = false;
                    return message2;
                }
            }

            protected virtual void EndCloseCore(IAsyncResult result)
            {
                CloseCoreAsyncResult.End(result);
            }

            public Message EndReceive(IAsyncResult result)
            {
                Message message;
                if (!this.EndTryReceive(result, out message))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
                return message;
            }

            public RequestContext EndReceiveRequest(IAsyncResult result)
            {
                RequestContext context;
                if (!this.EndTryReceiveRequest(result, out context))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
                return context;
            }

            internal void EndSendClose(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    try
                    {
                        this.EndSendMessage(result);
                    }
                    catch (CommunicationException exception)
                    {
                        this.TraceSessionClosedFailure(exception);
                    }
                    catch (TimeoutException exception2)
                    {
                        this.TraceSessionClosedFailure(exception2);
                    }
                }
            }

            internal void EndSendCloseResponse(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    try
                    {
                        this.EndSendMessage(result);
                    }
                    catch (CommunicationException exception)
                    {
                        this.TraceSessionClosedResponseFailure(exception);
                    }
                    catch (TimeoutException exception2)
                    {
                        this.TraceSessionClosedResponseFailure(exception2);
                    }
                }
            }

            internal void EndSendMessage(IAsyncResult result)
            {
                SendMessageAsyncResult.End(result);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return ReceiveRequestAsyncResult.EndAsMessage(result, out message);
            }

            public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext requestContext)
            {
                return ReceiveRequestAsyncResult.EndAsRequestContext(result, out requestContext);
            }

            protected Uri GetLocalUri()
            {
                if (this.channelBinder.LocalAddress == null)
                {
                    return null;
                }
                return this.channelBinder.LocalAddress.Uri;
            }

            public override T GetProperty<T>() where T: class
            {
                if ((typeof(T) == typeof(FaultConverter)) && (this.channelBinder != null))
                {
                    return (new SecurityChannelFaultConverter(this.channelBinder.Channel) as T);
                }
                T property = base.GetProperty<T>();
                if (((property == null) && (this.channelBinder != null)) && (this.channelBinder.Channel != null))
                {
                    property = this.channelBinder.Channel.GetProperty<T>();
                }
                return property;
            }

            private SecurityContextSecurityToken GetSessionToken(SecurityMessageProperty securityProperty)
            {
                SecurityContextSecurityToken securityToken = (securityProperty.ProtectionToken != null) ? (securityProperty.ProtectionToken.SecurityToken as SecurityContextSecurityToken) : null;
                if ((securityToken != null) && (securityToken.ContextId == this.sessionId))
                {
                    return securityToken;
                }
                if (securityProperty.HasIncomingSupportingTokens)
                {
                    for (int i = 0; i < securityProperty.IncomingSupportingTokens.Count; i++)
                    {
                        if (securityProperty.IncomingSupportingTokens[i].SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing)
                        {
                            securityToken = securityProperty.IncomingSupportingTokens[i].SecurityToken as SecurityContextSecurityToken;
                            if ((securityToken != null) && (securityToken.ContextId == this.sessionId))
                            {
                                return securityToken;
                            }
                        }
                    }
                }
                return null;
            }

            protected void InitializeFaultCodesIfRequired()
            {
                if (!this.areFaultCodesInitialized)
                {
                    lock (base.ThisLock)
                    {
                        if (!this.areFaultCodesInitialized)
                        {
                            SecureConversationDriver secureConversationDriver = this.securityProtocol.SecurityProtocolFactory.StandardsManager.SecureConversationDriver;
                            this.renewFaultCode = FaultCode.CreateSenderFaultCode(secureConversationDriver.RenewNeededFaultCode.Value, secureConversationDriver.Namespace.Value);
                            this.renewFaultReason = new FaultReason(System.ServiceModel.SR.GetString("SecurityRenewFaultReason"), CultureInfo.InvariantCulture);
                            this.sessionAbortedFaultCode = FaultCode.CreateSenderFaultCode("SecuritySessionAborted", "http://schemas.microsoft.com/ws/2006/05/security");
                            this.sessionAbortedFaultReason = new FaultReason(System.ServiceModel.SR.GetString("SecuritySessionAbortedFaultReason"), CultureInfo.InvariantCulture);
                            this.areFaultCodesInitialized = true;
                        }
                    }
                }
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.OnOpen(timeout);
                return new CompletedAsyncResult(callback, state);
            }

            protected abstract void OnCloseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout);
            protected abstract void OnCloseResponseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout);
            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            private void OnInnerFaulted(IReliableChannelBinder sender, Exception exception)
            {
                base.Fault(exception);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.securityProtocol.Open(timeout);
                if (this.CanDoSecurityCorrelation)
                {
                    ((IAcceptorSecuritySessionProtocol) this.securityProtocol).ReturnCorrelationState = true;
                }
                lock (base.ThisLock)
                {
                    if ((base.State != CommunicationState.Closed) && (base.State != CommunicationState.Closing))
                    {
                        this.settingsLifetimeManager.AddReference();
                        this.hasSecurityStateReference = true;
                    }
                }
            }

            private void PrepareReply(Message request, Message reply)
            {
                if (request.Headers.ReplyTo != null)
                {
                    request.Headers.ReplyTo.ApplyTo(reply);
                }
                else if (request.Headers.From != null)
                {
                    request.Headers.From.ApplyTo(reply);
                }
                if (request.Headers.MessageId != null)
                {
                    reply.Headers.RelatesTo = request.Headers.MessageId;
                }
                TraceUtility.CopyActivity(request, reply);
                if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
                {
                    TraceUtility.AddActivityHeader(reply);
                }
            }

            private Message ProcessCloseRequest(Message request)
            {
                RequestSecurityToken token;
                XmlDictionaryReader readerAtBodyContents = request.GetReaderAtBodyContents();
                using (readerAtBodyContents)
                {
                    token = this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityToken(readerAtBodyContents);
                    request.ReadFromBodyContentsToEnd(readerAtBodyContents);
                }
                if ((token.RequestType != null) && (token.RequestType != this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose))
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidRstRequestType", new object[] { token.RequestType })), request);
                }
                if (token.CloseTarget == null)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("NoCloseTargetSpecified")), request);
                }
                SecurityContextKeyIdentifierClause closeTarget = token.CloseTarget as SecurityContextKeyIdentifierClause;
                if ((closeTarget == null) || !SecuritySessionSecurityTokenAuthenticator.DoesSkiClauseMatchSigningToken(closeTarget, request))
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("BadCloseTarget", new object[] { token.CloseTarget })), request);
                }
                RequestSecurityTokenResponse response = new RequestSecurityTokenResponse(this.Settings.SecurityStandardsManager) {
                    Context = token.Context,
                    IsRequestedTokenClosed = true
                };
                response.MakeReadOnly();
                BodyWriter body = response;
                if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    RequestSecurityTokenResponseCollection responses = new RequestSecurityTokenResponseCollection(new List<RequestSecurityTokenResponse>(1) { response }, this.Settings.SecurityStandardsManager);
                    body = responses;
                }
                Message reply = Message.CreateMessage(request.Version, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction, request.Version.Addressing), body);
                this.PrepareReply(request, reply);
                return reply;
            }

            private Message ProcessRequestContext(RequestContext requestContext, TimeSpan timeout, out SecurityProtocolCorrelationState correlationState, out bool isSecurityProcessingFailure)
            {
                correlationState = null;
                isSecurityProcessingFailure = false;
                if (requestContext == null)
                {
                    return null;
                }
                Message message = null;
                Message requestMessage = requestContext.RequestMessage;
                bool flag = true;
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    Message unverifiedMessage = requestMessage;
                    Exception e = null;
                    try
                    {
                        correlationState = this.VerifyIncomingMessage(ref requestMessage, helper.RemainingTime());
                    }
                    catch (MessageSecurityException exception2)
                    {
                        isSecurityProcessingFailure = true;
                        e = exception2;
                    }
                    if (e != null)
                    {
                        this.SendFaultIfRequired(e, unverifiedMessage, requestContext, helper.RemainingTime());
                        flag = false;
                        return null;
                    }
                    if (this.CheckIncomingToken(requestContext, requestMessage, correlationState, helper.RemainingTime()))
                    {
                        if (requestMessage.Headers.Action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction.Value)
                        {
                            SecurityTraceRecordHelper.TraceServerSessionCloseReceived(this.currentSessionToken, this.GetLocalUri());
                            this.isInputClosed = true;
                            this.OnCloseMessageReceived(requestContext, requestMessage, correlationState, helper.RemainingTime());
                            correlationState = null;
                        }
                        else if (requestMessage.Headers.Action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction.Value)
                        {
                            SecurityTraceRecordHelper.TraceServerSessionCloseResponseReceived(this.currentSessionToken, this.GetLocalUri());
                            this.isInputClosed = true;
                            this.OnCloseResponseMessageReceived(requestContext, requestMessage, correlationState, helper.RemainingTime());
                            correlationState = null;
                        }
                        else
                        {
                            message = requestMessage;
                        }
                        flag = false;
                    }
                    return message;
                }
                catch (Exception exception3)
                {
                    if ((!(exception3 is CommunicationException) && !(exception3 is TimeoutException)) && (!Fx.IsFatal(exception3) && this.ShouldWrapException(exception3)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageSecurityVerificationFailed"), exception3));
                    }
                    throw;
                }
                finally
                {
                    if (flag)
                    {
                        if (requestContext.RequestMessage != null)
                        {
                            requestContext.RequestMessage.Close();
                        }
                        requestContext.Abort();
                    }
                }
                return message;
            }

            public Message Receive()
            {
                return this.Receive(base.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                Message message;
                if (!this.TryReceive(timeout, out message))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
                return message;
            }

            public RequestContext ReceiveRequest()
            {
                return this.ReceiveRequest(base.DefaultReceiveTimeout);
            }

            public RequestContext ReceiveRequest(TimeSpan timeout)
            {
                RequestContext context;
                if (!this.TryReceiveRequest(timeout, out context))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
                return context;
            }

            public void RenewSessionToken(SecurityContextSecurityToken newToken, SecurityContextSecurityToken supportingToken)
            {
                base.ThrowIfClosedOrNotOpen();
                lock (base.ThisLock)
                {
                    if ((supportingToken.ContextId != this.currentSessionToken.ContextId) || (supportingToken.KeyGeneration != this.currentSessionToken.KeyGeneration))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("CurrentSessionTokenNotRenewed", new object[] { supportingToken.KeyGeneration, this.currentSessionToken.KeyGeneration })));
                    }
                    if (this.futureSessionTokens.Count == this.Settings.MaximumPendingKeysPerSession)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("TooManyPendingSessionKeys")));
                    }
                    this.futureSessionTokens.Add(newToken);
                }
                SecurityTraceRecordHelper.TraceNewServerSessionKeyIssued(newToken, supportingToken, this.GetLocalUri());
            }

            internal void SecureApplicationMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                base.ThrowIfFaulted();
                base.ThrowIfClosedOrNotOpen();
                this.CheckOutgoingToken();
                this.securityProtocol.SecureOutgoingMessage(ref message, timeout, correlationState);
            }

            protected void SendClose(TimeSpan timeout)
            {
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    using (Message message = this.CreateCloseMessage(helper.RemainingTime()))
                    {
                        this.SendMessage(null, message, helper.RemainingTime());
                    }
                    this.TraceSessionClosedSuccess();
                }
                catch (CommunicationException exception)
                {
                    this.TraceSessionClosedFailure(exception);
                }
                catch (TimeoutException exception2)
                {
                    this.TraceSessionClosedFailure(exception2);
                }
            }

            protected void SendCloseResponse(RequestContext requestContext, Message closeResponse, TimeSpan timeout)
            {
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    using (closeResponse)
                    {
                        this.SendMessage(requestContext, closeResponse, helper.RemainingTime());
                    }
                    this.TraceSessionClosedResponseSuccess();
                }
                catch (CommunicationException exception)
                {
                    this.TraceSessionClosedResponseFailure(exception);
                }
                catch (TimeoutException exception2)
                {
                    this.TraceSessionClosedResponseFailure(exception2);
                }
            }

            private void SendFaultIfRequired(Exception e, Message unverifiedMessage, RequestContext requestContext, TimeSpan timeout)
            {
                try
                {
                    if ((this.channelBinder.Channel is IReplyChannel) || (this.channelBinder.Channel is IDuplexSessionChannel))
                    {
                        MessageFault fault = System.ServiceModel.Security.SecurityUtils.CreateSecurityMessageFault(e, this.securityProtocol.SecurityProtocolFactory.StandardsManager);
                        if (fault != null)
                        {
                            TimeoutHelper helper = new TimeoutHelper(timeout);
                            try
                            {
                                using (Message message = Message.CreateMessage(unverifiedMessage.Version, fault, unverifiedMessage.Version.Addressing.DefaultFaultAction))
                                {
                                    if (unverifiedMessage.Headers.MessageId != null)
                                    {
                                        message.InitializeReply(unverifiedMessage);
                                    }
                                    requestContext.Reply(message, helper.RemainingTime());
                                    requestContext.Close(helper.RemainingTime());
                                }
                            }
                            catch (CommunicationException exception)
                            {
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                                }
                            }
                            catch (TimeoutException exception2)
                            {
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    unverifiedMessage.Close();
                    requestContext.Abort();
                }
            }

            protected void SendMessage(RequestContext requestContext, Message message, TimeSpan timeout)
            {
                if (this.channelBinder.CanSendAsynchronously)
                {
                    this.channelBinder.Send(message, timeout);
                }
                else if (requestContext != null)
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    requestContext.Reply(message, helper.RemainingTime());
                    requestContext.Close(helper.RemainingTime());
                }
            }

            private void SendRenewFault(RequestContext requestContext, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                Message requestMessage = requestContext.RequestMessage;
                try
                {
                    Message message2;
                    this.InitializeFaultCodesIfRequired();
                    MessageFault fault = MessageFault.CreateFault(this.renewFaultCode, this.renewFaultReason);
                    if (requestMessage.Headers.MessageId != null)
                    {
                        message2 = Message.CreateMessage(requestMessage.Version, fault, "http://schemas.microsoft.com/ws/2006/05/security/SecureConversationFault");
                        message2.InitializeReply(requestMessage);
                    }
                    else
                    {
                        message2 = Message.CreateMessage(requestMessage.Version, fault, "http://schemas.microsoft.com/ws/2006/05/security/SecureConversationFault");
                    }
                    try
                    {
                        this.PrepareReply(requestMessage, message2);
                        TimeoutHelper helper = new TimeoutHelper(timeout);
                        this.securityProtocol.SecureOutgoingMessage(ref message2, helper.RemainingTime(), correlationState);
                        message2.Properties.AllowOutputBatching = false;
                        this.SendMessage(requestContext, message2, helper.RemainingTime());
                    }
                    finally
                    {
                        message2.Close();
                    }
                    SecurityTraceRecordHelper.TraceSessionRenewalFaultSent(this.currentSessionToken, this.GetLocalUri(), requestMessage);
                }
                catch (CommunicationException exception)
                {
                    SecurityTraceRecordHelper.TraceRenewFaultSendFailure(this.currentSessionToken, this.GetLocalUri(), exception);
                }
                catch (TimeoutException exception2)
                {
                    SecurityTraceRecordHelper.TraceRenewFaultSendFailure(this.currentSessionToken, this.GetLocalUri(), exception2);
                }
            }

            private bool ShouldWrapException(Exception e)
            {
                return ((e is FormatException) || (e is XmlException));
            }

            public void StartReceiving(RequestContext initialRequestContext)
            {
                if (this.initialRequestContext != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AttemptToCreateMultipleRequestContext")));
                }
                this.initialRequestContext = initialRequestContext;
            }

            internal void TraceSessionClosedFailure(Exception e)
            {
                SecurityTraceRecordHelper.TraceSessionCloseSendFailure(this.currentSessionToken, this.GetLocalUri(), e);
            }

            internal void TraceSessionClosedResponseFailure(Exception e)
            {
                SecurityTraceRecordHelper.TraceSessionClosedResponseSendFailure(this.currentSessionToken, this.GetLocalUri(), e);
            }

            internal void TraceSessionClosedResponseSuccess()
            {
                SecurityTraceRecordHelper.TraceSessionClosedResponseSent(this.currentSessionToken, this.GetLocalUri());
            }

            internal void TraceSessionClosedSuccess()
            {
                SecurityTraceRecordHelper.TraceSessionClosedSent(this.currentSessionToken, this.GetLocalUri());
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                RequestContext context;
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (this.TryReceiveRequest(helper.RemainingTime(), out context))
                {
                    if (context != null)
                    {
                        message = context.RequestMessage;
                        try
                        {
                            context.Close(helper.RemainingTime());
                        }
                        catch (TimeoutException exception)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                    else
                    {
                        message = null;
                    }
                    return true;
                }
                message = null;
                return false;
            }

            public bool TryReceiveRequest(TimeSpan timeout, out RequestContext requestContext)
            {
                base.ThrowIfFaulted();
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (!this.receiveLock.TryEnter(helper.RemainingTime()))
                {
                    requestContext = null;
                    return false;
                }
                try
                {
                Label_0027:
                    if (!this.isInputClosed && (base.State != CommunicationState.Faulted))
                    {
                        RequestContext initialRequestContext;
                        if (helper.RemainingTime() == TimeSpan.Zero)
                        {
                            requestContext = null;
                            return false;
                        }
                        if (this.initialRequestContext != null)
                        {
                            initialRequestContext = this.initialRequestContext;
                            this.initialRequestContext = null;
                        }
                        else if (!this.channelBinder.TryReceive(helper.RemainingTime(), out initialRequestContext))
                        {
                            requestContext = null;
                            return false;
                        }
                        if (initialRequestContext != null)
                        {
                            bool flag;
                            if (this.isInputClosed && (initialRequestContext.RequestMessage != null))
                            {
                                Message message = initialRequestContext.RequestMessage;
                                try
                                {
                                    throw TraceUtility.ThrowHelperWarning(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                                }
                                finally
                                {
                                    message.Close();
                                    initialRequestContext.Abort();
                                }
                            }
                            SecurityProtocolCorrelationState correlationState = null;
                            Message requestMessage = this.ProcessRequestContext(initialRequestContext, helper.RemainingTime(), out correlationState, out flag);
                            if (requestMessage == null)
                            {
                                goto Label_0027;
                            }
                            requestContext = new SecuritySessionServerSettings.SecuritySessionRequestContext(initialRequestContext, requestMessage, correlationState, this);
                            return true;
                        }
                    }
                }
                finally
                {
                    this.receiveLock.Exit();
                }
                base.ThrowIfFaulted();
                requestContext = null;
                return true;
            }

            internal SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout)
            {
                return this.securityProtocol.VerifyIncomingMessage(ref message, timeout, null);
            }

            protected virtual bool CanDoSecurityCorrelation
            {
                get
                {
                    return false;
                }
            }

            internal IServerReliableChannelBinder ChannelBinder
            {
                get
                {
                    return this.channelBinder;
                }
            }

            internal TimeSpan InternalSendTimeout
            {
                get
                {
                    return base.DefaultSendTimeout;
                }
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return this.channelBinder.LocalAddress;
                }
            }

            protected SecuritySessionServerSettings Settings
            {
                get
                {
                    return this.settings;
                }
            }

            private class CloseCoreAsyncResult : AsyncResult
            {
                private SecuritySessionServerSettings.ServerSecuritySessionChannel channel;
                private static AsyncCallback channelBinderCloseCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySessionChannel.CloseCoreAsyncResult.ChannelBinderCloseCallback));
                private static AsyncCallback settingsLifetimeManagerCloseCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySessionChannel.CloseCoreAsyncResult.SettingsLifetimeManagerCloseCallback));
                private TimeoutHelper timeoutHelper;

                public CloseCoreAsyncResult(SecuritySessionServerSettings.ServerSecuritySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    bool flag = false;
                    if (this.channel.channelBinder != null)
                    {
                        try
                        {
                            IAsyncResult result = this.channel.channelBinder.BeginClose(this.timeoutHelper.RemainingTime(), channelBinderCloseCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return;
                            }
                            this.channel.channelBinder.EndClose(result);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (this.channel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        flag = this.OnChannelBinderClosed();
                    }
                    if (flag)
                    {
                        this.RemoveSessionTokenFromCache();
                        base.Complete(true);
                    }
                }

                private static void ChannelBinderCloseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySessionChannel.CloseCoreAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySessionChannel.CloseCoreAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            try
                            {
                                asyncState.channel.channelBinder.EndClose(result);
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.channel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag = true;
                            }
                            if (!flag)
                            {
                                flag = asyncState.OnChannelBinderClosed();
                            }
                            if (flag)
                            {
                                asyncState.RemoveSessionTokenFromCache();
                            }
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

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecuritySessionServerSettings.ServerSecuritySessionChannel.CloseCoreAsyncResult>(result);
                }

                private bool OnChannelBinderClosed()
                {
                    try
                    {
                        if (this.channel.securityProtocol != null)
                        {
                            this.channel.securityProtocol.Close(false, this.timeoutHelper.RemainingTime());
                        }
                        bool flag = false;
                        lock (this.channel.ThisLock)
                        {
                            if (this.channel.hasSecurityStateReference)
                            {
                                flag = true;
                                this.channel.hasSecurityStateReference = false;
                            }
                        }
                        if (flag)
                        {
                            IAsyncResult result = this.channel.settingsLifetimeManager.BeginClose(this.timeoutHelper.RemainingTime(), settingsLifetimeManagerCloseCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return false;
                            }
                            this.channel.settingsLifetimeManager.EndClose(result);
                        }
                        return true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        return true;
                    }
                }

                private void RemoveSessionTokenFromCache()
                {
                    this.channel.Settings.SessionTokenCache.RemoveAllContexts(this.channel.currentSessionToken.ContextId);
                }

                private static void SettingsLifetimeManagerCloseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySessionChannel.CloseCoreAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySessionChannel.CloseCoreAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            asyncState.channel.settingsLifetimeManager.EndClose(result);
                            flag = true;
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (asyncState.channel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            flag = true;
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        finally
                        {
                            if (flag)
                            {
                                asyncState.RemoveSessionTokenFromCache();
                            }
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private class ReceiveRequestAsyncResult : AsyncResult
            {
                private SecuritySessionServerSettings.ServerSecuritySessionChannel channel;
                private SecurityProtocolCorrelationState correlationState;
                private bool expired;
                private RequestContext innerRequestContext;
                private static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult.OnReceive));
                private static FastAsyncCallback onWait = new FastAsyncCallback(SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult.OnWait);
                private Message requestMessage;
                private TimeoutHelper timeoutHelper;

                public ReceiveRequestAsyncResult(SecuritySessionServerSettings.ServerSecuritySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    channel.ThrowIfFaulted();
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.channel = channel;
                    if (channel.receiveLock.EnterAsync(this.timeoutHelper.RemainingTime(), onWait, this))
                    {
                        bool flag = false;
                        bool flag2 = true;
                        try
                        {
                            flag = this.WaitComplete();
                            flag2 = false;
                        }
                        finally
                        {
                            if (flag2)
                            {
                                this.channel.receiveLock.Exit();
                            }
                        }
                        if (flag)
                        {
                            this.Complete(true);
                        }
                    }
                }

                private void Complete(bool synchronous)
                {
                    try
                    {
                        this.channel.receiveLock.Exit();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, 0x80001, System.ServiceModel.SR.GetString("TraceCodeAsyncCallbackThrewException"), exception.ToString());
                        }
                    }
                    base.Complete(synchronous);
                }

                private void Complete(bool synchronous, Exception exception)
                {
                    try
                    {
                        this.channel.receiveLock.Exit();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, 0x80001, System.ServiceModel.SR.GetString("TraceCodeAsyncCallbackThrewException"), exception2.ToString());
                        }
                    }
                    base.Complete(synchronous, exception);
                }

                private bool CompleteReceive(IAsyncResult result)
                {
                Label_0000:
                    this.expired = !this.channel.ChannelBinder.EndTryReceive(result, out this.innerRequestContext);
                    if (!this.expired && (this.innerRequestContext != null))
                    {
                        bool flag;
                        this.requestMessage = this.channel.ProcessRequestContext(this.innerRequestContext, this.timeoutHelper.RemainingTime(), out this.correlationState, out flag);
                        if (this.requestMessage != null)
                        {
                            if (!this.channel.isInputClosed)
                            {
                                goto Label_0117;
                            }
                            ProtocolException exception = ProtocolException.ReceiveShutdownReturnedNonNull(this.requestMessage);
                            try
                            {
                                throw TraceUtility.ThrowHelperWarning(exception, this.requestMessage);
                            }
                            finally
                            {
                                this.requestMessage.Close();
                                this.innerRequestContext.Abort();
                            }
                        }
                        if (!this.channel.isInputClosed && (this.channel.State != CommunicationState.Faulted))
                        {
                            if (this.timeoutHelper.RemainingTime() != TimeSpan.Zero)
                            {
                                result = this.channel.ChannelBinder.BeginTryReceive(this.timeoutHelper.RemainingTime(), onReceive, this);
                                if (!result.CompletedSynchronously)
                                {
                                    return false;
                                }
                                goto Label_0000;
                            }
                            this.expired = true;
                        }
                    }
                Label_0117:
                    this.channel.ThrowIfFaulted();
                    return true;
                }

                private static SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult End(IAsyncResult result)
                {
                    return AsyncResult.End<SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult>(result);
                }

                public static bool EndAsMessage(IAsyncResult result, out Message message)
                {
                    SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult result2 = End(result);
                    message = result2.requestMessage;
                    if ((message != null) && (result2.innerRequestContext != null))
                    {
                        try
                        {
                            result2.innerRequestContext.Close(result2.timeoutHelper.RemainingTime());
                        }
                        catch (TimeoutException exception)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                    return !result2.expired;
                }

                public static bool EndAsRequestContext(IAsyncResult result, out RequestContext requestContext)
                {
                    SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult result2 = End(result);
                    if (result2.requestMessage == null)
                    {
                        requestContext = null;
                    }
                    else
                    {
                        requestContext = new SecuritySessionServerSettings.SecuritySessionRequestContext(result2.innerRequestContext, result2.requestMessage, result2.correlationState, result2.channel);
                    }
                    return !result2.expired;
                }

                private static void OnReceive(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.CompleteReceive(result);
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

                private static void OnWait(object state, Exception asyncException)
                {
                    SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult result = (SecuritySessionServerSettings.ServerSecuritySessionChannel.ReceiveRequestAsyncResult) state;
                    bool flag = false;
                    Exception exception = asyncException;
                    if (exception != null)
                    {
                        flag = true;
                    }
                    else
                    {
                        try
                        {
                            flag = result.WaitComplete();
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
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }

                private bool WaitComplete()
                {
                    if (this.channel.isInputClosed)
                    {
                        return true;
                    }
                    this.channel.ThrowIfFaulted();
                    ServiceModelActivity activity = (DiagnosticUtility.ShouldUseActivity && (this.channel.initialRequestContext != null)) ? TraceUtility.ExtractActivity(this.channel.initialRequestContext.RequestMessage) : null;
                    using (ServiceModelActivity.BoundOperation(activity))
                    {
                        if (this.channel.initialRequestContext != null)
                        {
                            bool flag;
                            this.innerRequestContext = this.channel.initialRequestContext;
                            this.channel.initialRequestContext = null;
                            this.requestMessage = this.channel.ProcessRequestContext(this.innerRequestContext, this.timeoutHelper.RemainingTime(), out this.correlationState, out flag);
                            if ((this.requestMessage != null) || this.channel.isInputClosed)
                            {
                                this.expired = false;
                                return true;
                            }
                        }
                        if (this.timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            this.expired = true;
                            return true;
                        }
                        IAsyncResult result = this.channel.ChannelBinder.BeginTryReceive(this.timeoutHelper.RemainingTime(), onReceive, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        return this.CompleteReceive(result);
                    }
                }
            }

            private class SendMessageAsyncResult : AsyncResult
            {
                private Message message;
                private RequestContext requestContext;
                private static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySessionChannel.SendMessageAsyncResult.SendCallback));
                private SecuritySessionServerSettings.ServerSecuritySessionChannel sessionChannel;
                private TimeoutHelper timeoutHelper;

                public SendMessageAsyncResult(SecuritySessionServerSettings.ServerSecuritySessionChannel sessionChannel, RequestContext requestContext, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.sessionChannel = sessionChannel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.requestContext = requestContext;
                    this.message = message;
                    bool flag = true;
                    try
                    {
                        IAsyncResult result = this.BeginSend(message);
                        if (!result.CompletedSynchronously)
                        {
                            flag = false;
                            return;
                        }
                        this.EndSend(result);
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.message.Close();
                        }
                    }
                    base.Complete(true);
                }

                private IAsyncResult BeginSend(Message response)
                {
                    if (this.sessionChannel.channelBinder.CanSendAsynchronously)
                    {
                        return this.sessionChannel.channelBinder.BeginSend(response, this.timeoutHelper.RemainingTime(), sendCallback, this);
                    }
                    if (this.requestContext != null)
                    {
                        return this.requestContext.BeginReply(response, sendCallback, this);
                    }
                    return new SendCompletedAsyncResult(sendCallback, this);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecuritySessionServerSettings.ServerSecuritySessionChannel.SendMessageAsyncResult>(result);
                }

                private void EndSend(IAsyncResult result)
                {
                    try
                    {
                        if (result is SendCompletedAsyncResult)
                        {
                            SendCompletedAsyncResult.End(result);
                        }
                        else if (this.sessionChannel.channelBinder.CanSendAsynchronously)
                        {
                            this.sessionChannel.channelBinder.EndSend(result);
                        }
                        else
                        {
                            this.requestContext.EndReply(result);
                            this.requestContext.Close(this.timeoutHelper.RemainingTime());
                        }
                    }
                    finally
                    {
                        if (this.message != null)
                        {
                            this.message.Close();
                        }
                    }
                }

                private static void SendCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySessionChannel.SendMessageAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySessionChannel.SendMessageAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.EndSend(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }

                private class SendCompletedAsyncResult : CompletedAsyncResult
                {
                    public SendCompletedAsyncResult(AsyncCallback callback, object state) : base(callback, state)
                    {
                    }

                    public static void End(IAsyncResult result)
                    {
                        AsyncResult.End<SecuritySessionServerSettings.ServerSecuritySessionChannel.SendMessageAsyncResult.SendCompletedAsyncResult>(result);
                    }
                }
            }

            protected class SoapSecurityInputSession : ISecureConversationSession, ISecuritySession, IInputSession, ISession
            {
                private SecuritySessionServerSettings.ServerSecuritySessionChannel channel;
                private EndpointIdentity remoteIdentity;
                private UniqueId securityContextTokenId;
                private SecurityKeyIdentifierClause sessionTokenIdentifier;
                private SecurityStandardsManager standardsManager;

                public SoapSecurityInputSession(SecurityContextSecurityToken sessionToken, SecuritySessionServerSettings settings, SecuritySessionServerSettings.ServerSecuritySessionChannel channel)
                {
                    this.channel = channel;
                    this.securityContextTokenId = sessionToken.ContextId;
                    Claim primaryIdentityClaim = System.ServiceModel.Security.SecurityUtils.GetPrimaryIdentityClaim(sessionToken.AuthorizationPolicies);
                    if (primaryIdentityClaim != null)
                    {
                        this.remoteIdentity = EndpointIdentity.CreateIdentity(primaryIdentityClaim);
                    }
                    this.sessionTokenIdentifier = settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(sessionToken, SecurityTokenReferenceStyle.External);
                    this.standardsManager = settings.SessionProtocolFactory.StandardsManager;
                }

                public bool TryReadSessionTokenIdentifier(XmlReader reader)
                {
                    this.channel.ThrowIfDisposedOrNotOpen();
                    if (!this.standardsManager.SecurityTokenSerializer.CanReadKeyIdentifierClause(reader))
                    {
                        return false;
                    }
                    SecurityContextKeyIdentifierClause clause = this.standardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(reader) as SecurityContextKeyIdentifierClause;
                    return ((clause != null) && clause.Matches(this.securityContextTokenId, null));
                }

                public void WriteSessionTokenIdentifier(XmlDictionaryWriter writer)
                {
                    this.channel.ThrowIfDisposedOrNotOpen();
                    this.standardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, this.sessionTokenIdentifier);
                }

                public string Id
                {
                    get
                    {
                        return this.securityContextTokenId.ToString();
                    }
                }

                public EndpointIdentity RemoteIdentity
                {
                    get
                    {
                        return this.remoteIdentity;
                    }
                }
            }
        }

        private abstract class ServerSecuritySimplexSessionChannel : SecuritySessionServerSettings.ServerSecuritySessionChannel
        {
            private bool canSendCloseResponse;
            private RequestContext closeRequestContext;
            private Message closeResponse;
            private InterruptibleWaitObject inputSessionClosedHandle;
            private bool receivedClose;
            private bool sentCloseResponse;
            private SecuritySessionServerSettings.ServerSecuritySessionChannel.SoapSecurityInputSession session;

            public ServerSecuritySimplexSessionChannel(SecuritySessionServerSettings settings, IServerReliableChannelBinder channelBinder, SecurityContextSecurityToken sessionToken, object listenerSecurityState, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(settings, channelBinder, sessionToken, listenerSecurityState, settingsLifetimeManager)
            {
                this.inputSessionClosedHandle = new InterruptibleWaitObject(false);
                this.session = new SecuritySessionServerSettings.ServerSecuritySessionChannel.SoapSecurityInputSession(sessionToken, settings, this);
            }

            protected override void AbortCore()
            {
                base.AbortCore();
                base.Settings.RemoveSessionChannel(this.session.Id);
                this.CleanupPendingCloseState();
            }

            private void CleanupPendingCloseState()
            {
                lock (base.ThisLock)
                {
                    if (this.closeResponse != null)
                    {
                        this.closeResponse.Close();
                        this.closeResponse = null;
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                        this.closeRequestContext = null;
                    }
                }
            }

            protected override void CloseCore(TimeSpan timeout)
            {
                base.CloseCore(timeout);
                this.inputSessionClosedHandle.Abort(this);
                base.Settings.RemoveSessionChannel(this.session.Id);
            }

            protected override void EndCloseCore(IAsyncResult result)
            {
                base.EndCloseCore(result);
                this.inputSessionClosedHandle.Abort(this);
                base.Settings.RemoveSessionChannel(this.session.Id);
            }

            protected override void OnAbort()
            {
                this.AbortCore();
                this.inputSessionClosedHandle.Abort(this);
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                RequestContext context;
                Message message;
                return new CloseAsyncResult(this, this.ShouldSendCloseResponseOnClose(out context, out message), context, message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                bool wasAborted = this.SendCloseResponseOnCloseIfRequired(helper.RemainingTime());
                if (!wasAborted)
                {
                    bool flag2 = this.WaitForInputSessionClose(helper.RemainingTime(), out wasAborted);
                    if (!wasAborted)
                    {
                        if (!flag2)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { helper.OriginalTimeout })));
                        }
                        this.CloseCore(helper.RemainingTime());
                    }
                }
            }

            protected override void OnCloseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                if (base.State == CommunicationState.Created)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ServerReceivedCloseMessageStateIsCreated", new object[] { base.GetType().ToString() })));
                }
                if (this.SendCloseResponseOnCloseReceivedIfRequired(requestContext, message, correlationState, timeout))
                {
                    this.inputSessionClosedHandle.Set();
                }
            }

            protected override void OnCloseResponseMessageReceived(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                message.Close();
                requestContext.Abort();
                base.Fault(new ProtocolException(System.ServiceModel.SR.GetString("UnexpectedSecuritySessionCloseResponse")));
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CloseAsyncResult.End(result);
            }

            protected override void OnFaulted()
            {
                this.AbortCore();
                this.inputSessionClosedHandle.Fault(this);
                base.OnFaulted();
            }

            private bool SendCloseResponseOnCloseIfRequired(TimeSpan timeout)
            {
                RequestContext context;
                Message message;
                bool flag = false;
                bool flag2 = this.ShouldSendCloseResponseOnClose(out context, out message);
                TimeoutHelper helper = new TimeoutHelper(timeout);
                bool flag3 = true;
                if (flag2)
                {
                    try
                    {
                        base.SendCloseResponse(context, message, helper.RemainingTime());
                        this.inputSessionClosedHandle.Set();
                        flag3 = false;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (base.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        flag = true;
                    }
                    finally
                    {
                        if (flag3)
                        {
                            if (message != null)
                            {
                                message.Close();
                            }
                            if (context != null)
                            {
                                context.Abort();
                            }
                        }
                    }
                }
                return flag;
            }

            private bool SendCloseResponseOnCloseReceivedIfRequired(RequestContext requestContext, Message message, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                bool flag4;
                bool flag = false;
                ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(message) : null;
                bool flag2 = true;
                try
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    Message closeResponse = null;
                    lock (base.ThisLock)
                    {
                        if (!this.receivedClose)
                        {
                            this.receivedClose = true;
                            closeResponse = base.CreateCloseResponse(message, correlationState, helper.RemainingTime());
                            if (this.canSendCloseResponse)
                            {
                                this.sentCloseResponse = true;
                                flag = true;
                            }
                            else
                            {
                                this.closeRequestContext = requestContext;
                                this.closeResponse = closeResponse;
                                flag2 = false;
                            }
                        }
                    }
                    if (flag)
                    {
                        base.SendCloseResponse(requestContext, closeResponse, helper.RemainingTime());
                        flag2 = false;
                    }
                    else if (flag2)
                    {
                        requestContext.Close(helper.RemainingTime());
                        flag2 = false;
                    }
                    flag4 = flag;
                }
                finally
                {
                    message.Close();
                    if (flag2)
                    {
                        requestContext.Abort();
                    }
                    if (DiagnosticUtility.ShouldUseActivity && (activity != null))
                    {
                        activity.Stop();
                    }
                }
                return flag4;
            }

            private bool ShouldSendCloseResponseOnClose(out RequestContext pendingCloseRequestContext, out Message pendingCloseResponse)
            {
                bool flag = false;
                lock (base.ThisLock)
                {
                    this.canSendCloseResponse = true;
                    if ((!this.sentCloseResponse && this.receivedClose) && (this.closeResponse != null))
                    {
                        this.sentCloseResponse = true;
                        flag = true;
                        pendingCloseRequestContext = this.closeRequestContext;
                        pendingCloseResponse = this.closeResponse;
                        this.closeResponse = null;
                        this.closeRequestContext = null;
                        return flag;
                    }
                    this.canSendCloseResponse = false;
                    pendingCloseRequestContext = null;
                    pendingCloseResponse = null;
                }
                return flag;
            }

            private bool WaitForInputSessionClose(TimeSpan timeout, out bool wasAborted)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                wasAborted = false;
                try
                {
                    Message message;
                    if (base.TryReceive(helper.RemainingTime(), out message))
                    {
                        if (message != null)
                        {
                            using (message)
                            {
                                throw TraceUtility.ThrowHelperWarning(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                            }
                        }
                        return this.inputSessionClosedHandle.Wait(helper.RemainingTime(), false);
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (base.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                    wasAborted = true;
                }
                return false;
            }

            public IInputSession Session
            {
                get
                {
                    return this.session;
                }
            }

            private class CloseAsyncResult : AsyncResult
            {
                private static readonly AsyncCallback closeCoreCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult.CloseCoreCallback));
                private RequestContext closeRequestContext;
                private Message closeResponse;
                private static readonly AsyncCallback receiveCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult.ReceiveCallback));
                private static readonly AsyncCallback sendCloseResponseCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult.SendCloseResponseCallback));
                private SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel sessionChannel;
                private TimeoutHelper timeoutHelper;
                private static readonly AsyncCallback waitCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult.WaitForInputSessionCloseCallback));

                public CloseAsyncResult(SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel sessionChannel, bool sendCloseResponse, RequestContext closeRequestContext, Message closeResponse, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    this.closeRequestContext = closeRequestContext;
                    this.closeResponse = closeResponse;
                    bool wasChannelAborted = false;
                    bool flag2 = this.OnSendCloseResponse(sendCloseResponse, out wasChannelAborted);
                    if (wasChannelAborted || flag2)
                    {
                        base.Complete(true);
                    }
                }

                private void CleanupCloseState()
                {
                    if (this.closeResponse != null)
                    {
                        this.closeResponse.Close();
                    }
                    if (this.closeRequestContext != null)
                    {
                        this.closeRequestContext.Abort();
                    }
                }

                private static void CloseCoreCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.sessionChannel.EndCloseCore(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult>(result);
                }

                private bool OnMessageReceived(Message message)
                {
                    if (message != null)
                    {
                        using (message)
                        {
                            throw TraceUtility.ThrowHelperWarning(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                        }
                    }
                    bool closeCompleted = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.inputSessionClosedHandle.BeginWait(this.timeoutHelper.RemainingTime(), true, waitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.inputSessionClosedHandle.EndWait(result);
                        closeCompleted = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        return true;
                    }
                    catch (TimeoutException)
                    {
                        closeCompleted = false;
                    }
                    return this.OnWaitOver(closeCompleted);
                }

                private bool OnReceiveNullMessage(out bool wasChannelAborted)
                {
                    wasChannelAborted = false;
                    bool flag = false;
                    Message message = null;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.BeginTryReceive(this.timeoutHelper.RemainingTime(), receiveCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        flag = this.sessionChannel.EndTryReceive(result, out message);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        wasChannelAborted = true;
                    }
                    if (wasChannelAborted)
                    {
                        return true;
                    }
                    if (!flag)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { this.timeoutHelper.OriginalTimeout })));
                    }
                    return this.OnMessageReceived(message);
                }

                private bool OnSendCloseResponse(bool shouldSendCloseResponse, out bool wasChannelAborted)
                {
                    wasChannelAborted = false;
                    try
                    {
                        if (shouldSendCloseResponse)
                        {
                            bool flag = true;
                            try
                            {
                                IAsyncResult result = this.sessionChannel.BeginSendCloseResponse(this.closeRequestContext, this.closeResponse, this.timeoutHelper.RemainingTime(), sendCloseResponseCallback, this);
                                if (!result.CompletedSynchronously)
                                {
                                    flag = false;
                                    return false;
                                }
                                this.sessionChannel.EndSendCloseResponse(result);
                                this.sessionChannel.inputSessionClosedHandle.Set();
                            }
                            finally
                            {
                                if (flag)
                                {
                                    this.CleanupCloseState();
                                }
                            }
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        wasChannelAborted = true;
                    }
                    return (wasChannelAborted || this.OnReceiveNullMessage(out wasChannelAborted));
                }

                private bool OnWaitOver(bool closeCompleted)
                {
                    if (!closeCompleted)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { this.timeoutHelper.OriginalTimeout })));
                    }
                    IAsyncResult result = this.sessionChannel.BeginCloseCore(this.timeoutHelper.RemainingTime(), closeCoreCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.sessionChannel.EndCloseCore(result);
                    return true;
                }

                private static void ReceiveCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            Message message = null;
                            bool flag2 = false;
                            bool flag3 = false;
                            try
                            {
                                flag3 = asyncState.sessionChannel.EndTryReceive(result, out message);
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag2 = true;
                                flag = true;
                            }
                            if (!flag2)
                            {
                                if (!flag3)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("ServiceSecurityCloseTimeout", new object[] { asyncState.timeoutHelper.OriginalTimeout })));
                                }
                                flag = asyncState.OnMessageReceived(message);
                            }
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

                private static void SendCloseResponseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            bool wasChannelAborted = false;
                            try
                            {
                                asyncState.sessionChannel.EndSendCloseResponse(result);
                                asyncState.sessionChannel.inputSessionClosedHandle.Set();
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                wasChannelAborted = true;
                                flag = true;
                            }
                            finally
                            {
                                asyncState.CleanupCloseState();
                            }
                            if (!wasChannelAborted)
                            {
                                flag = asyncState.OnReceiveNullMessage(out wasChannelAborted);
                            }
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

                private static void WaitForInputSessionCloseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult asyncState = (SecuritySessionServerSettings.ServerSecuritySimplexSessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            bool closeCompleted = false;
                            bool flag3 = false;
                            try
                            {
                                asyncState.sessionChannel.inputSessionClosedHandle.EndWait(result);
                                closeCompleted = true;
                            }
                            catch (TimeoutException)
                            {
                                closeCompleted = false;
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag3 = true;
                                flag = true;
                            }
                            if (!flag3)
                            {
                                flag = asyncState.OnWaitOver(closeCompleted);
                            }
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
            }
        }

        private class SessionInitiationMessageHandler
        {
            private IServerReliableChannelBinder channelBinder;
            private bool processedInitiation;
            private static AsyncCallback receiveCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionServerSettings.SessionInitiationMessageHandler.ReceiveCallback));
            private SecurityContextSecurityToken sessionToken;
            private SecuritySessionServerSettings settings;

            public SessionInitiationMessageHandler(IServerReliableChannelBinder channelBinder, SecuritySessionServerSettings settings, SecurityContextSecurityToken sessionToken)
            {
                this.channelBinder = channelBinder;
                this.settings = settings;
                this.sessionToken = sessionToken;
            }

            public IAsyncResult BeginReceive(TimeSpan timeout)
            {
                return this.channelBinder.BeginTryReceive(timeout, receiveCallback, this);
            }

            public void ProcessMessage(IAsyncResult result)
            {
                bool flag = false;
                try
                {
                    RequestContext context;
                    if (!this.channelBinder.EndTryReceive(result, out context))
                    {
                        this.BeginReceive(TimeSpan.MaxValue);
                    }
                    else if (context != null)
                    {
                        Message requestMessage = context.RequestMessage;
                        lock (this.settings.ThisLock)
                        {
                            if (this.settings.communicationObject.State != CommunicationState.Opened)
                            {
                                ((IDisposable) context).Dispose();
                                return;
                            }
                            if (this.processedInitiation)
                            {
                                return;
                            }
                            this.processedInitiation = true;
                        }
                        if (!this.settings.RemovePendingSession(this.sessionToken.ContextId))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new CommunicationException(System.ServiceModel.SR.GetString("SecuritySessionNotPending", new object[] { this.sessionToken.ContextId })));
                        }
                        if (this.settings.channelAcceptor is SecuritySessionServerSettings.SecuritySessionChannelAcceptor<IReplySessionChannel>)
                        {
                            SecuritySessionServerSettings.SecuritySessionChannelAcceptor<IReplySessionChannel> channelAcceptor = (SecuritySessionServerSettings.SecuritySessionChannelAcceptor<IReplySessionChannel>) this.settings.channelAcceptor;
                            SecuritySessionServerSettings.SecurityReplySessionChannel channel = new SecuritySessionServerSettings.SecurityReplySessionChannel(this.settings, this.channelBinder, this.sessionToken, channelAcceptor.ListenerSecurityState, this.settings.SettingsLifetimeManager);
                            this.settings.AddSessionChannel(this.sessionToken.ContextId, channel);
                            channel.StartReceiving(context);
                            channelAcceptor.EnqueueAndDispatch(channel);
                        }
                        else
                        {
                            if (!(this.settings.channelAcceptor is SecuritySessionServerSettings.SecuritySessionChannelAcceptor<IDuplexSessionChannel>))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new EndpointNotFoundException(System.ServiceModel.SR.GetString("SecuritySessionListenerNotFound", new object[] { requestMessage.Headers.Action })));
                            }
                            SecuritySessionServerSettings.SecuritySessionChannelAcceptor<IDuplexSessionChannel> acceptor2 = (SecuritySessionServerSettings.SecuritySessionChannelAcceptor<IDuplexSessionChannel>) this.settings.channelAcceptor;
                            SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel channel2 = new SecuritySessionServerSettings.ServerSecurityDuplexSessionChannel(this.settings, this.channelBinder, this.sessionToken, acceptor2.ListenerSecurityState, this.settings.SettingsLifetimeManager);
                            this.settings.AddSessionChannel(this.sessionToken.ContextId, channel2);
                            channel2.StartReceiving(context);
                            acceptor2.EnqueueAndDispatch(channel2);
                        }
                    }
                }
                catch (Exception exception)
                {
                    flag = true;
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                finally
                {
                    if (flag)
                    {
                        this.channelBinder.Abort();
                    }
                }
            }

            private static void ReceiveCallback(IAsyncResult result)
            {
                ((SecuritySessionServerSettings.SessionInitiationMessageHandler) result.AsyncState).ProcessMessage(result);
            }
        }
    }
}

