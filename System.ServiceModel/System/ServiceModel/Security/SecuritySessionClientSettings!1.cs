namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal sealed class SecuritySessionClientSettings<TChannel> : IChannelSecureConversationSessionSettings, ISecurityCommunicationObject
    {
        private bool canRenewSession;
        private System.ServiceModel.Channels.ChannelBuilder channelBuilder;
        private WrapperSecurityCommunicationObject communicationObject;
        private IChannelFactory innerChannelFactory;
        private SecurityTokenParameters issuedTokenParameters;
        private int issuedTokenRenewalThreshold;
        private TimeSpan keyRenewalInterval;
        private TimeSpan keyRolloverInterval;
        private SecurityChannelFactory<TChannel> securityChannelFactory;
        private SecurityProtocolFactory sessionProtocolFactory;
        private System.ServiceModel.Security.SecurityStandardsManager standardsManager;
        private object thisLock;
        private bool tolerateTransportFailures;

        public SecuritySessionClientSettings()
        {
            this.canRenewSession = true;
            this.thisLock = new object();
            this.keyRenewalInterval = SecuritySessionClientSettings.defaultKeyRenewalInterval;
            this.keyRolloverInterval = SecuritySessionClientSettings.defaultKeyRolloverInterval;
            this.tolerateTransportFailures = true;
            this.communicationObject = new WrapperSecurityCommunicationObject(this);
        }

        internal void Abort()
        {
            this.communicationObject.Abort();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.communicationObject.BeginClose(timeout, callback, state);
        }

        internal void Close(TimeSpan timeout)
        {
            this.communicationObject.Close(timeout);
        }

        private void ConfigureSessionProtocolFactory()
        {
            if (this.sessionProtocolFactory is SessionSymmetricMessageSecurityProtocolFactory)
            {
                AddressingVersion addressing = MessageVersion.Default.Addressing;
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
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, addressing.FaultAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, addressing.DefaultFaultAction);
                sessionProtocolFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(parts, "http://schemas.microsoft.com/ws/2006/05/security/SecureConversationFault");
                sessionProtocolFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                sessionProtocolFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(parts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                if (sessionProtocolFactory.ApplyConfidentiality)
                {
                    sessionProtocolFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    sessionProtocolFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                }
                if (sessionProtocolFactory.RequireConfidentiality)
                {
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(MessagePartSpecification.NoParts, this.SecurityStandardsManager.SecureConversationDriver.CloseAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, addressing.FaultAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, addressing.DefaultFaultAction);
                    sessionProtocolFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(parts, "http://schemas.microsoft.com/ws/2006/05/security/SecureConversationFault");
                }
            }
            else
            {
                if (!(this.sessionProtocolFactory is SessionSymmetricTransportSecurityProtocolFactory))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                SessionSymmetricTransportSecurityProtocolFactory factory2 = (SessionSymmetricTransportSecurityProtocolFactory) this.sessionProtocolFactory;
                factory2.AddTimestamp = true;
                factory2.SecurityTokenParameters.RequireDerivedKeys = false;
            }
        }

        internal IChannelFactory CreateInnerChannelFactory()
        {
            if (this.ChannelBuilder.CanBuildChannelFactory<IDuplexSessionChannel>())
            {
                return this.ChannelBuilder.BuildChannelFactory<IDuplexSessionChannel>();
            }
            if (this.ChannelBuilder.CanBuildChannelFactory<IDuplexChannel>())
            {
                return this.ChannelBuilder.BuildChannelFactory<IDuplexChannel>();
            }
            if (!this.ChannelBuilder.CanBuildChannelFactory<IRequestChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return this.ChannelBuilder.BuildChannelFactory<IRequestChannel>();
        }

        public void EndClose(IAsyncResult result)
        {
            this.communicationObject.EndClose(result);
        }

        public void OnAbort()
        {
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(true, TimeSpan.Zero);
            }
        }

        public void OnClose(TimeSpan timeout)
        {
            if (this.sessionProtocolFactory != null)
            {
                this.sessionProtocolFactory.Close(false, timeout);
            }
        }

        public void OnClosed()
        {
        }

        public void OnClosing()
        {
        }

        internal TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            return this.OnCreateChannel(remoteAddress, via, null);
        }

        internal TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via, MessageFilter filter)
        {
            this.communicationObject.ThrowIfClosed();
            if (filter != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (TChannel) new SecurityRequestSessionChannel<TChannel>((SecuritySessionClientSettings<TChannel>) this, remoteAddress, via);
            }
            if (typeof(TChannel) != typeof(IDuplexSessionChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }), "TChannel"));
            }
            return (TChannel) new ClientSecurityDuplexSessionChannel<TChannel>((SecuritySessionClientSettings<TChannel>) this, remoteAddress, via);
        }

        public void OnFaulted()
        {
        }

        public void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.sessionProtocolFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecuritySessionProtocolFactoryShouldBeSetBeforeThisOperation")));
            }
            if (this.standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityStandardsManagerNotSet", new object[] { base.GetType().ToString() })));
            }
            if (this.issuedTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedSecurityTokenParametersNotSet", new object[] { base.GetType() })));
            }
            if (this.keyRenewalInterval < this.keyRolloverInterval)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("KeyRolloverGreaterThanKeyRenewal")));
            }
            this.issuedTokenRenewalThreshold = this.sessionProtocolFactory.SecurityBindingElement.LocalClientSettings.CookieRenewalThresholdPercentage;
            this.ConfigureSessionProtocolFactory();
            this.sessionProtocolFactory.Open(true, helper.RemainingTime());
        }

        public void OnOpened()
        {
        }

        public void OnOpening()
        {
        }

        internal void Open(SecurityChannelFactory<TChannel> securityChannelFactory, IChannelFactory innerChannelFactory, System.ServiceModel.Channels.ChannelBuilder channelBuilder, TimeSpan timeout)
        {
            this.securityChannelFactory = securityChannelFactory;
            this.innerChannelFactory = innerChannelFactory;
            this.channelBuilder = channelBuilder;
            this.communicationObject.Open(timeout);
        }

        IAsyncResult ISecurityCommunicationObject.OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnClose), timeout, callback, state);
        }

        IAsyncResult ISecurityCommunicationObject.OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        void ISecurityCommunicationObject.OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        void ISecurityCommunicationObject.OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
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
                this.channelBuilder = value;
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

        private IChannelFactory InnerChannelFactory
        {
            get
            {
                return this.innerChannelFactory;
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

        public TimeSpan KeyRenewalInterval
        {
            get
            {
                return this.keyRenewalInterval;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
                }
                this.communicationObject.ThrowIfDisposedOrImmutable();
                this.keyRenewalInterval = value;
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

        private SecurityChannelFactory<TChannel> SecurityChannelFactory
        {
            get
            {
                return this.securityChannelFactory;
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

        private class ClientSecurityDuplexSessionChannel : SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
        {
            private Action<object> completeLater;
            private static AsyncCallback onReceive;
            private InputQueue<Message> queue;
            private SoapSecurityClientDuplexSession<TChannel> session;
            private Action startReceiving;

            static ClientSecurityDuplexSessionChannel()
            {
                SecuritySessionClientSettings<TChannel>.ClientSecurityDuplexSessionChannel.onReceive = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecurityDuplexSessionChannel.OnReceive));
            }

            public ClientSecurityDuplexSessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via) : base(settings, to, via)
            {
                this.session = new SoapSecurityClientDuplexSession<TChannel>((SecuritySessionClientSettings<TChannel>.ClientSecurityDuplexSessionChannel) this);
                this.queue = TraceUtility.CreateInputQueue<Message>();
                this.startReceiving = new Action(this.StartReceiving);
                this.completeLater = new Action<object>(this.CompleteLater);
            }

            protected override void AbortCore()
            {
                try
                {
                    this.queue.Dispose();
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
                base.AbortCore();
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return InputChannel.HelpBeginReceive(this, timeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                base.CheckOutputOpen();
                return new SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult(message, this, timeout, callback, state, false);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                return this.queue.BeginDequeue(timeout, callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.queue.BeginWaitForItem(timeout, callback, state);
            }

            private void CompleteLater(object obj)
            {
                this.CompleteReceive((IAsyncResult) obj);
            }

            private void CompleteReceive(IAsyncResult result)
            {
                Message item = null;
                bool flag = false;
                try
                {
                    item = base.EndReceiveInternal(result);
                    flag = true;
                }
                catch (MessageSecurityException)
                {
                    flag = false;
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
                IAsyncResult state = null;
                if (flag)
                {
                    state = this.IssueReceive();
                    if ((state != null) && state.CompletedSynchronously)
                    {
                        ActionItem.Schedule(this.completeLater, state);
                    }
                }
                if (item != null)
                {
                    try
                    {
                        this.queue.EnqueueAndDispatch(item);
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                    }
                }
            }

            public Message EndReceive(IAsyncResult result)
            {
                return InputChannel.HelpEndReceive(result);
            }

            public void EndSend(IAsyncResult result)
            {
                SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult.End(result);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                bool flag = this.queue.EndDequeue(result, out message);
                if (message == null)
                {
                    base.ThrowIfFaulted();
                }
                return flag;
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.queue.EndWaitForItem(result);
            }

            protected override void InitializeSession(SecurityToken sessionToken)
            {
                this.session.Initialize(sessionToken, base.Settings);
            }

            private IAsyncResult IssueReceive()
            {
                IAsyncResult result;
            Label_0000:
                if (((base.State == CommunicationState.Closed) || (base.State == CommunicationState.Faulted)) || base.IsInputClosed)
                {
                    return null;
                }
                try
                {
                    result = base.BeginReceiveInternal(TimeSpan.MaxValue, null, SecuritySessionClientSettings<TChannel>.ClientSecurityDuplexSessionChannel.onReceive, this);
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    goto Label_0000;
                }
                catch (TimeoutException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                    goto Label_0000;
                }
                return result;
            }

            protected override bool OnCloseReceived()
            {
                if (base.OnCloseReceived())
                {
                    this.queue.Shutdown();
                    return true;
                }
                return false;
            }

            protected override bool OnCloseResponseReceived()
            {
                if (base.OnCloseResponseReceived())
                {
                    this.queue.Shutdown();
                    return true;
                }
                return false;
            }

            protected override void OnFaulted()
            {
                this.queue.Shutdown(() => base.GetPendingException());
                base.OnFaulted();
            }

            protected override void OnOpened()
            {
                base.OnOpened();
                this.StartReceiving();
            }

            private static void OnReceive(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((SecuritySessionClientSettings<TChannel>.ClientSecurityDuplexSessionChannel) result.AsyncState).CompleteReceive(result);
                }
            }

            public Message Receive()
            {
                return this.Receive(base.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return InputChannel.HelpReceive(this, timeout);
            }

            public void Send(Message message)
            {
                this.Send(message, base.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                base.ThrowIfFaulted();
                base.CheckOutputOpen();
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.SecureOutgoingMessage(ref message, helper.RemainingTime());
                base.ChannelBinder.Send(message, helper.RemainingTime());
            }

            private void StartReceiving()
            {
                IAsyncResult state = this.IssueReceive();
                if ((state != null) && state.CompletedSynchronously)
                {
                    ActionItem.Schedule(this.completeLater, state);
                }
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                base.ThrowIfFaulted();
                bool flag = this.queue.Dequeue(timeout, out message);
                if (message == null)
                {
                    base.ThrowIfFaulted();
                }
                return flag;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.queue.WaitForItem(timeout);
            }

            protected override bool ExpectClose
            {
                get
                {
                    return true;
                }
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return base.InternalLocalAddress;
                }
            }

            public IDuplexSession Session
            {
                get
                {
                    return this.session;
                }
            }

            protected override string SessionId
            {
                get
                {
                    return this.session.Id;
                }
            }

            private class SoapSecurityClientDuplexSession : SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SoapSecurityOutputSession, IDuplexSession, IInputSession, IOutputSession, ISession
            {
                private SecuritySessionClientSettings<TChannel>.ClientSecurityDuplexSessionChannel channel;
                private bool initialized;

                public SoapSecurityClientDuplexSession(SecuritySessionClientSettings<TChannel>.ClientSecurityDuplexSessionChannel channel) : base(channel)
                {
                    this.channel = channel;
                }

                public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
                {
                    return this.BeginCloseOutputSession(this.channel.DefaultCloseTimeout, callback, state);
                }

                public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    this.CheckInitialized();
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception exception = null;
                    try
                    {
                        return this.channel.BeginCloseOutputSession(timeout, callback, state);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        return new CompletedAsyncResult(callback, state);
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }

                private void CheckInitialized()
                {
                    if (!this.initialized)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ChannelNotOpen")));
                    }
                }

                public void CloseOutputSession()
                {
                    this.CloseOutputSession(this.channel.DefaultCloseTimeout);
                }

                public void CloseOutputSession(TimeSpan timeout)
                {
                    this.CheckInitialized();
                    this.channel.ThrowIfFaulted();
                    this.channel.ThrowIfNotOpened();
                    Exception exception = null;
                    try
                    {
                        this.channel.CloseOutputSession(timeout);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.channel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
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
                        throw exception;
                    }
                }

                public void EndCloseOutputSession(IAsyncResult result)
                {
                    if (result is CompletedAsyncResult)
                    {
                        CompletedAsyncResult.End(result);
                    }
                    else
                    {
                        Exception exception = null;
                        try
                        {
                            this.channel.EndCloseOutputSession(result);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (this.channel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
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
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                        }
                    }
                }

                internal void Initialize(SecurityToken sessionToken, SecuritySessionClientSettings<TChannel> settings)
                {
                    base.Initialize(sessionToken, settings);
                    this.initialized = true;
                }
            }
        }

        private abstract class ClientSecuritySessionChannel : ChannelBase
        {
            private IClientReliableChannelBinder channelBinder;
            private ChannelParameterCollection channelParameters;
            private Message closeResponse;
            private SecurityToken currentSessionToken;
            private InterruptibleWaitObject inputSessionClosedHandle;
            private bool isCompositeDuplexConnection;
            private volatile bool isInputClosed;
            private bool isKeyRenewalOngoing;
            private volatile bool isOutputClosed;
            private InterruptibleWaitObject keyRenewalCompletedEvent;
            private DateTime keyRenewalTime;
            private DateTime keyRolloverTime;
            private System.ServiceModel.Channels.MessageVersion messageVersion;
            private InterruptibleWaitObject outputSessionCloseHandle;
            private SecurityToken previousSessionToken;
            private bool receivedClose;
            private SecurityProtocol securityProtocol;
            private bool sendCloseHandshake;
            private bool sentClose;
            private SecurityTokenProvider sessionTokenProvider;
            private SecuritySessionClientSettings<TChannel> settings;
            private EndpointAddress to;
            private Uri via;

            protected ClientSecuritySessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via) : base(settings.SecurityChannelFactory)
            {
                this.inputSessionClosedHandle = new InterruptibleWaitObject(false);
                this.outputSessionCloseHandle = new InterruptibleWaitObject(true);
                this.settings = settings;
                this.to = to;
                this.via = via;
                this.keyRenewalCompletedEvent = new InterruptibleWaitObject(false);
                this.messageVersion = settings.SecurityChannelFactory.MessageVersion;
                this.channelParameters = new ChannelParameterCollection(this);
                this.InitializeChannelBinder();
            }

            protected virtual void AbortCore()
            {
                if (this.channelBinder != null)
                {
                    this.channelBinder.Abort();
                }
                if (this.sessionTokenProvider != null)
                {
                    System.ServiceModel.Security.SecurityUtils.AbortTokenProviderIfRequired(this.sessionTokenProvider);
                }
            }

            protected virtual IAsyncResult BeginCloseCore(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseCoreAsyncResult<TChannel>((SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, timeout, callback, state);
            }

            protected virtual IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                if (this.SendCloseHandshake)
                {
                    bool flag;
                    bool flag2;
                    this.DetermineCloseMessageToSend(out flag, out flag2);
                    if (flag || flag2)
                    {
                        bool flag3 = true;
                        try
                        {
                            IAsyncResult result;
                            if (flag)
                            {
                                result = this.BeginSendCloseMessage(timeout, callback, state);
                            }
                            else
                            {
                                result = this.BeginSendCloseResponseMessage(timeout, callback, state);
                            }
                            flag3 = false;
                            return result;
                        }
                        finally
                        {
                            if (flag3)
                            {
                                this.outputSessionCloseHandle.Set();
                            }
                        }
                    }
                }
                return new CompletedAsyncResult(callback, state);
            }

            protected IAsyncResult BeginCloseSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateAsyncActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivitySecurityClose"), ActivityType.SecuritySetup);
                    }
                    return new CloseSessionAsyncResult<TChannel>(timeout, (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, callback, state);
                }
            }

            protected IAsyncResult BeginReceiveInternal(TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
            {
                return new ReceiveAsyncResult<TChannel>((SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, timeout, correlationState, callback, state);
            }

            protected IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.sendCloseHandshake = true;
                if (!this.CheckIfKeyRenewalNeeded())
                {
                    return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, this.securityProtocol.SecureOutgoingMessage(ref message, timeout, null), callback, state);
                }
                return new KeyRenewalAsyncResult<TChannel>(message, (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, timeout, callback, state);
            }

            private IAsyncResult BeginSendCloseMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivitySecurityClose"), ActivityType.SecuritySetup);
                    }
                    return new SecureSendAsyncResult<TChannel>(this.PrepareCloseMessage(), (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, timeout, callback, state, true);
                }
            }

            private IAsyncResult BeginSendCloseResponseMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SecureSendAsyncResult<TChannel>(this.closeResponse, (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, timeout, callback, state, true);
            }

            private bool CheckIfKeyRenewalNeeded()
            {
                bool flag = false;
                lock (base.ThisLock)
                {
                    flag = this.IsKeyRenewalNeeded();
                    this.DoKeyRolloverIfNeeded();
                }
                return flag;
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

            protected virtual void CloseCore(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                try
                {
                    if (this.channelBinder != null)
                    {
                        this.channelBinder.Close(helper.RemainingTime());
                    }
                    if (this.sessionTokenProvider != null)
                    {
                        System.ServiceModel.Security.SecurityUtils.CloseTokenProviderIfRequired(this.sessionTokenProvider, helper.RemainingTime());
                    }
                    this.keyRenewalCompletedEvent.Abort(this);
                    this.inputSessionClosedHandle.Abort(this);
                }
                catch (CommunicationObjectAbortedException)
                {
                    if (base.State != CommunicationState.Closed)
                    {
                        throw;
                    }
                }
            }

            protected virtual SecurityProtocolCorrelationState CloseOutputSession(TimeSpan timeout)
            {
                base.ThrowIfFaulted();
                if (this.SendCloseHandshake)
                {
                    bool flag;
                    bool flag2;
                    this.DetermineCloseMessageToSend(out flag, out flag2);
                    if (flag || flag2)
                    {
                        try
                        {
                            if (flag)
                            {
                                return this.SendCloseMessage(timeout);
                            }
                            this.SendCloseResponseMessage(timeout);
                            return null;
                        }
                        finally
                        {
                            this.outputSessionCloseHandle.Set();
                        }
                    }
                }
                return null;
            }

            protected bool CloseSession(TimeSpan timeout, out bool wasAborted)
            {
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivitySecurityClose"), ActivityType.SecuritySetup);
                    }
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    wasAborted = false;
                    try
                    {
                        this.CloseOutputSession(helper.RemainingTime());
                        return this.inputSessionClosedHandle.Wait(helper.RemainingTime(), false);
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
            }

            private void DetermineCloseMessageToSend(out bool sendClose, out bool sendCloseResponse)
            {
                sendClose = false;
                sendCloseResponse = false;
                lock (base.ThisLock)
                {
                    if (!this.isOutputClosed)
                    {
                        this.isOutputClosed = true;
                        if (this.receivedClose)
                        {
                            sendCloseResponse = true;
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

            private bool DoesSkiClauseMatchSigningToken(SecurityContextKeyIdentifierClause skiClause, Message request)
            {
                if (this.SessionId == null)
                {
                    return false;
                }
                return (skiClause.ContextId.ToString() == this.SessionId);
            }

            private void DoKeyRolloverIfNeeded()
            {
                if ((DateTime.UtcNow >= this.keyRolloverTime) && (this.previousSessionToken != null))
                {
                    SecurityTraceRecordHelper.TracePreviousSessionKeyDiscarded(this.previousSessionToken, this.currentSessionToken, this.RemoteAddress);
                    this.previousSessionToken = null;
                    List<SecurityToken> tokens = new List<SecurityToken>(1) {
                        this.currentSessionToken
                    };
                    ((IInitiatorSecuritySessionProtocol) this.securityProtocol).SetIncomingSessionTokens(tokens);
                }
            }

            protected virtual void EndCloseCore(IAsyncResult result)
            {
                CloseCoreAsyncResult<TChannel>.End(result);
            }

            protected virtual SecurityProtocolCorrelationState EndCloseOutputSession(IAsyncResult result)
            {
                bool sentClose;
                SecurityProtocolCorrelationState state;
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                    return null;
                }
                lock (base.ThisLock)
                {
                    sentClose = this.sentClose;
                }
                try
                {
                    if (sentClose)
                    {
                        return this.EndSendCloseMessage(result);
                    }
                    this.EndSendCloseResponseMessage(result);
                    state = null;
                }
                finally
                {
                    this.outputSessionCloseHandle.Set();
                }
                return state;
            }

            protected bool EndCloseSession(IAsyncResult result, out bool wasAborted)
            {
                return CloseSessionAsyncResult<TChannel>.End(result, out wasAborted);
            }

            protected Message EndReceiveInternal(IAsyncResult result)
            {
                return ReceiveAsyncResult<TChannel>.End(result);
            }

            protected Message EndSecureOutgoingMessage(IAsyncResult result, out SecurityProtocolCorrelationState correlationState)
            {
                TimeSpan span;
                if (result is CompletedAsyncResult<Message, SecurityProtocolCorrelationState>)
                {
                    return CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out correlationState);
                }
                Message message = KeyRenewalAsyncResult<TChannel>.End(result, out span);
                correlationState = this.securityProtocol.SecureOutgoingMessage(ref message, span, null);
                return message;
            }

            private SecurityProtocolCorrelationState EndSendCloseMessage(IAsyncResult result)
            {
                SecurityProtocolCorrelationState state = SecureSendAsyncResult<TChannel>.End(result);
                SecurityTraceRecordHelper.TraceCloseMessageSent(this.currentSessionToken, this.RemoteAddress);
                return state;
            }

            private void EndSendCloseResponseMessage(IAsyncResult result)
            {
                SecureSendAsyncResult<TChannel>.End(result);
                SecurityTraceRecordHelper.TraceCloseResponseMessageSent(this.currentSessionToken, this.RemoteAddress);
            }

            private DateTime GetKeyRenewalTime(SecurityToken token)
            {
                TimeSpan timeout = TimeSpan.FromTicks(((token.ValidTo.Ticks - token.ValidFrom.Ticks) * this.settings.issuedTokenRenewalThreshold) / 100L);
                DateTime time = TimeoutHelper.Add(token.ValidFrom, timeout);
                DateTime time2 = TimeoutHelper.Add(token.ValidFrom, this.settings.keyRenewalInterval);
                if (time < time2)
                {
                    return time;
                }
                return time2;
            }

            public override T GetProperty<T>() where T: class
            {
                if (typeof(T) == typeof(ChannelParameterCollection))
                {
                    return (this.channelParameters as T);
                }
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

            private MessageFault GetProtocolFault(ref Message message, out bool isKeyRenewalFault, out bool isSessionAbortedFault)
            {
                isKeyRenewalFault = false;
                isSessionAbortedFault = false;
                MessageFault fault = null;
                using (MessageBuffer buffer = message.CreateBufferedCopy(0x7fffffff))
                {
                    message = buffer.CreateMessage();
                    MessageFault fault2 = MessageFault.CreateFault(buffer.CreateMessage(), 0x4000);
                    if (!fault2.Code.IsSenderFault)
                    {
                        return fault;
                    }
                    FaultCode subCode = fault2.Code.SubCode;
                    if (subCode == null)
                    {
                        return fault;
                    }
                    SecureConversationDriver secureConversationDriver = this.securityProtocol.SecurityProtocolFactory.StandardsManager.SecureConversationDriver;
                    if ((subCode.Namespace == secureConversationDriver.Namespace.Value) && (subCode.Name == secureConversationDriver.RenewNeededFaultCode.Value))
                    {
                        fault = fault2;
                        isKeyRenewalFault = true;
                        return fault;
                    }
                    if ((subCode.Namespace == "http://schemas.microsoft.com/ws/2006/05/security") && (subCode.Name == "SecuritySessionAborted"))
                    {
                        fault = fault2;
                        isSessionAbortedFault = true;
                    }
                }
                return fault;
            }

            private void InitializeChannelBinder()
            {
                ChannelBuilder channelBuilder = this.Settings.ChannelBuilder;
                TolerateFaultsMode faultMode = this.Settings.TolerateTransportFailures ? TolerateFaultsMode.Always : TolerateFaultsMode.Never;
                if (channelBuilder.CanBuildChannelFactory<IDuplexSessionChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IDuplexSessionChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IDuplexSessionChannel>) this.Settings.InnerChannelFactory, MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, base.DefaultSendTimeout);
                }
                else if (channelBuilder.CanBuildChannelFactory<IDuplexChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IDuplexChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IDuplexChannel>) this.Settings.InnerChannelFactory, MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, base.DefaultSendTimeout);
                    this.isCompositeDuplexConnection = true;
                }
                else if (channelBuilder.CanBuildChannelFactory<IRequestChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IRequestChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IRequestChannel>) this.Settings.InnerChannelFactory, MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, base.DefaultSendTimeout);
                }
                else if (channelBuilder.CanBuildChannelFactory<IRequestSessionChannel>())
                {
                    this.channelBinder = ClientReliableChannelBinder<IRequestSessionChannel>.CreateBinder(this.RemoteAddress, this.Via, (IChannelFactory<IRequestSessionChannel>) this.Settings.InnerChannelFactory, MaskingMode.None, faultMode, this.channelParameters, this.DefaultCloseTimeout, base.DefaultSendTimeout);
                }
                this.channelBinder.Faulted += new BinderExceptionHandler(this.OnInnerFaulted);
            }

            private void InitializeSecurityState(SecurityToken sessionToken)
            {
                this.InitializeSession(sessionToken);
                this.currentSessionToken = sessionToken;
                this.previousSessionToken = null;
                List<SecurityToken> tokens = new List<SecurityToken>(1) {
                    sessionToken
                };
                ((IInitiatorSecuritySessionProtocol) this.securityProtocol).SetIdentityCheckAuthenticator(new GenericXmlSecurityTokenAuthenticator());
                ((IInitiatorSecuritySessionProtocol) this.securityProtocol).SetIncomingSessionTokens(tokens);
                ((IInitiatorSecuritySessionProtocol) this.securityProtocol).SetOutgoingSessionToken(sessionToken);
                if (this.CanDoSecurityCorrelation)
                {
                    ((IInitiatorSecuritySessionProtocol) this.securityProtocol).ReturnCorrelationState = true;
                }
                this.keyRenewalTime = this.GetKeyRenewalTime(sessionToken);
            }

            protected abstract void InitializeSession(SecurityToken sessionToken);
            private bool IsKeyRenewalNeeded()
            {
                return (DateTime.UtcNow >= this.keyRenewalTime);
            }

            protected override void OnAbort()
            {
                this.AbortCore();
                this.inputSessionClosedHandle.Abort(this);
                this.keyRenewalCompletedEvent.Abort(this);
                this.outputSessionCloseHandle.Abort(this);
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseAsyncResult<TChannel>((SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateAsyncActivity() : null;
                using (ServiceModelActivity.BoundOperation(activity, true))
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivitySecuritySetup"), ActivityType.SecuritySetup);
                    }
                    return new OpenAsyncResult<TChannel>((SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel) this, timeout, callback, state);
                }
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (this.SendCloseHandshake)
                {
                    bool flag;
                    bool flag2 = this.CloseSession(timeout, out flag);
                    if (flag)
                    {
                        return;
                    }
                    if (!flag2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ClientSecurityCloseTimeout", new object[] { timeout })));
                    }
                    try
                    {
                        if (!this.outputSessionCloseHandle.Wait(helper.RemainingTime(), false))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ClientSecurityOutputSessionCloseTimeout", new object[] { helper.OriginalTimeout })));
                        }
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (base.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        return;
                    }
                }
                this.CloseCore(helper.RemainingTime());
            }

            protected virtual bool OnCloseReceived()
            {
                if (!this.ExpectClose)
                {
                    base.Fault(new ProtocolException(System.ServiceModel.SR.GetString("UnexpectedSecuritySessionClose")));
                    return false;
                }
                bool flag = false;
                lock (base.ThisLock)
                {
                    if (!this.isInputClosed)
                    {
                        this.isInputClosed = true;
                        this.receivedClose = true;
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.inputSessionClosedHandle.Set();
                }
                return true;
            }

            protected virtual bool OnCloseResponseReceived()
            {
                bool flag = false;
                bool sentClose = false;
                lock (base.ThisLock)
                {
                    sentClose = this.sentClose;
                    if (sentClose && !this.isInputClosed)
                    {
                        this.isInputClosed = true;
                        flag = true;
                    }
                }
                if (!sentClose)
                {
                    base.Fault(new ProtocolException(System.ServiceModel.SR.GetString("UnexpectedSecuritySessionCloseResponse")));
                    return false;
                }
                if (flag)
                {
                    this.inputSessionClosedHandle.Set();
                }
                return true;
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CloseAsyncResult<TChannel>.End(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                OpenAsyncResult<TChannel>.End(result);
            }

            protected override void OnFaulted()
            {
                this.AbortCore();
                this.inputSessionClosedHandle.Fault(this);
                this.keyRenewalCompletedEvent.Fault(this);
                this.outputSessionCloseHandle.Fault(this);
                base.OnFaulted();
            }

            private void OnInnerFaulted(IReliableChannelBinder sender, Exception exception)
            {
                base.Fault(exception);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.SetupSessionTokenProvider();
                System.ServiceModel.Security.SecurityUtils.OpenTokenProviderIfRequired(this.sessionTokenProvider, helper.RemainingTime());
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivitySecuritySetup"), ActivityType.SecuritySetup);
                    }
                    SecurityToken sessionToken = this.sessionTokenProvider.GetToken(helper.RemainingTime());
                    this.OpenCore(sessionToken, helper.RemainingTime());
                }
            }

            private void OpenCore(SecurityToken sessionToken, TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.securityProtocol = this.Settings.SessionProtocolFactory.CreateSecurityProtocol(this.to, this.Via, null, true, helper.RemainingTime());
                if (!(this.securityProtocol is IInitiatorSecuritySessionProtocol))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProtocolMisMatch", new object[] { "IInitiatorSecuritySessionProtocol", base.GetType().ToString() })));
                }
                this.securityProtocol.Open(helper.RemainingTime());
                this.channelBinder.Open(helper.RemainingTime());
                this.InitializeSecurityState(sessionToken);
            }

            private Message PrepareCloseMessage()
            {
                SecurityToken currentSessionToken;
                lock (base.ThisLock)
                {
                    currentSessionToken = this.currentSessionToken;
                }
                RequestSecurityToken body = new RequestSecurityToken(this.Settings.SecurityStandardsManager) {
                    RequestType = this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose,
                    CloseTarget = this.Settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(currentSessionToken, SecurityTokenReferenceStyle.External)
                };
                body.MakeReadOnly();
                Message request = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction, this.MessageVersion.Addressing), body);
                RequestReplyCorrelator.PrepareRequest(request);
                if (this.InternalLocalAddress != null)
                {
                    request.Headers.ReplyTo = this.InternalLocalAddress;
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
                if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
                {
                    TraceUtility.AddAmbientActivityToMessage(request);
                }
                return request;
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

            private void ProcessCloseMessage(Message message)
            {
                RequestSecurityToken token;
                XmlDictionaryReader readerAtBodyContents = message.GetReaderAtBodyContents();
                using (readerAtBodyContents)
                {
                    token = this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityToken(readerAtBodyContents);
                    message.ReadFromBodyContentsToEnd(readerAtBodyContents);
                }
                if ((token.RequestType != null) && (token.RequestType != this.Settings.SecurityStandardsManager.TrustDriver.RequestTypeClose))
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidRstRequestType", new object[] { token.RequestType })), message);
                }
                if (token.CloseTarget == null)
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("NoCloseTargetSpecified")), message);
                }
                SecurityContextKeyIdentifierClause closeTarget = token.CloseTarget as SecurityContextKeyIdentifierClause;
                if ((closeTarget == null) || !this.DoesSkiClauseMatchSigningToken(closeTarget, message))
                {
                    throw TraceUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("BadCloseTarget", new object[] { token.CloseTarget })), message);
                }
                RequestSecurityTokenResponse body = new RequestSecurityTokenResponse(this.Settings.SecurityStandardsManager) {
                    Context = token.Context,
                    IsRequestedTokenClosed = true
                };
                body.MakeReadOnly();
                Message reply = null;
                if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                {
                    reply = Message.CreateMessage(message.Version, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction, message.Version.Addressing), body);
                }
                else
                {
                    if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion != TrustVersion.WSTrust13)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }
                    RequestSecurityTokenResponseCollection responses = new RequestSecurityTokenResponseCollection(new List<RequestSecurityTokenResponse> { body }, this.Settings.SecurityStandardsManager);
                    reply = Message.CreateMessage(message.Version, ActionHeader.Create(this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction, message.Version.Addressing), responses);
                }
                this.PrepareReply(message, reply);
                this.closeResponse = reply;
            }

            private void ProcessCloseResponse(Message response)
            {
                if (response.Headers.Action != this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction.Value)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidCloseResponseAction", new object[] { response.Headers.Action })), response);
                }
                RequestSecurityTokenResponse response2 = null;
                XmlDictionaryReader readerAtBodyContents = response.GetReaderAtBodyContents();
                using (readerAtBodyContents)
                {
                    if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                    {
                        response2 = this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityTokenResponse(readerAtBodyContents);
                    }
                    else
                    {
                        if (this.Settings.SecurityStandardsManager.MessageSecurityVersion.TrustVersion != TrustVersion.WSTrust13)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                        }
                        foreach (RequestSecurityTokenResponse response3 in this.Settings.SecurityStandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(readerAtBodyContents).RstrCollection)
                        {
                            if (response2 != null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MoreThanOneRSTRInRSTRC")));
                            }
                            response2 = response3;
                        }
                    }
                    response.ReadFromBodyContentsToEnd(readerAtBodyContents);
                }
                if (!response2.IsRequestedTokenClosed)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SessionTokenWasNotClosed")), response);
                }
            }

            protected Message ProcessIncomingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, out MessageFault protocolFault)
            {
                protocolFault = null;
                lock (base.ThisLock)
                {
                    this.DoKeyRolloverIfNeeded();
                }
                try
                {
                    this.VerifyIncomingMessage(ref message, timeout, correlationState);
                    string action = message.Headers.Action;
                    if (action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseResponseAction.Value)
                    {
                        SecurityTraceRecordHelper.TraceCloseResponseReceived(this.currentSessionToken, this.RemoteAddress);
                        this.ProcessCloseResponse(message);
                        this.OnCloseResponseReceived();
                    }
                    else if (action == this.Settings.SecurityStandardsManager.SecureConversationDriver.CloseAction.Value)
                    {
                        SecurityTraceRecordHelper.TraceCloseMessageReceived(this.currentSessionToken, this.RemoteAddress);
                        this.ProcessCloseMessage(message);
                        this.OnCloseReceived();
                    }
                    else
                    {
                        if (action == "http://schemas.microsoft.com/ws/2006/05/security/SecureConversationFault")
                        {
                            bool flag;
                            bool flag2;
                            protocolFault = this.GetProtocolFault(ref message, out flag, out flag2);
                            if (flag)
                            {
                                this.ProcessKeyRenewalFault();
                                goto Label_014A;
                            }
                            if (flag2)
                            {
                                this.ProcessSessionAbortedFault(protocolFault);
                                goto Label_014A;
                            }
                        }
                        return message;
                    }
                }
                catch (Exception exception)
                {
                    if ((!(exception is CommunicationException) && !(exception is TimeoutException)) && (!Fx.IsFatal(exception) && this.ShouldWrapException(exception)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageSecurityVerificationFailed"), exception));
                    }
                    throw;
                }
            Label_014A:
                message.Close();
                return null;
            }

            private void ProcessKeyRenewalFault()
            {
                SecurityTraceRecordHelper.TraceSessionKeyRenewalFault(this.currentSessionToken, this.RemoteAddress);
                lock (base.ThisLock)
                {
                    this.keyRenewalTime = DateTime.UtcNow;
                }
            }

            protected Message ProcessRequestContext(RequestContext requestContext, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                Message message3;
                if (requestContext == null)
                {
                    return null;
                }
                TimeoutHelper helper = new TimeoutHelper(timeout);
                Message requestMessage = requestContext.RequestMessage;
                Message message = requestMessage;
                try
                {
                    Exception exception = null;
                    try
                    {
                        MessageFault fault;
                        return this.ProcessIncomingMessage(requestMessage, helper.RemainingTime(), correlationState, out fault);
                    }
                    catch (MessageSecurityException exception2)
                    {
                        if (!this.isCompositeDuplexConnection)
                        {
                            if (message.IsFault)
                            {
                                MessageFault fault2 = MessageFault.CreateFault(message, 0x4000);
                                if (System.ServiceModel.Security.SecurityUtils.IsSecurityFault(fault2, this.settings.sessionProtocolFactory.StandardsManager))
                                {
                                    exception = System.ServiceModel.Security.SecurityUtils.CreateSecurityFaultException(fault2);
                                }
                            }
                            else
                            {
                                exception = exception2;
                            }
                        }
                    }
                    if (exception != null)
                    {
                        base.Fault(exception);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                    message3 = null;
                }
                finally
                {
                    requestContext.Close(helper.RemainingTime());
                }
                return message3;
            }

            private void ProcessSessionAbortedFault(MessageFault sessionAbortedFault)
            {
                SecurityTraceRecordHelper.TraceRemoteSessionAbortedFault(this.currentSessionToken, this.RemoteAddress);
                base.Fault(new FaultException(sessionAbortedFault));
            }

            protected Message ReceiveInternal(TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                while (!this.isInputClosed)
                {
                    RequestContext context;
                    if (this.ChannelBinder.TryReceive(helper.RemainingTime(), out context))
                    {
                        if (context == null)
                        {
                            return null;
                        }
                        Message message = this.ProcessRequestContext(context, helper.RemainingTime(), correlationState);
                        if (message != null)
                        {
                            return message;
                        }
                    }
                    if (helper.RemainingTime() == TimeSpan.Zero)
                    {
                        break;
                    }
                }
                return null;
            }

            private void RenewKey(TimeSpan timeout)
            {
                bool flag;
                if (!this.settings.CanRenewSession)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SessionKeyExpiredException(System.ServiceModel.SR.GetString("SessionKeyRenewalNotSupported")));
                }
                lock (base.ThisLock)
                {
                    if (!this.isKeyRenewalOngoing)
                    {
                        this.isKeyRenewalOngoing = true;
                        this.keyRenewalCompletedEvent.Reset();
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    try
                    {
                        using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
                        {
                            if (DiagnosticUtility.ShouldUseActivity)
                            {
                                ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivitySecurityRenew"), ActivityType.SecuritySetup);
                            }
                            SecurityToken newToken = this.sessionTokenProvider.RenewToken(timeout, this.currentSessionToken);
                            this.UpdateSessionTokens(newToken);
                        }
                        return;
                    }
                    finally
                    {
                        lock (base.ThisLock)
                        {
                            this.isKeyRenewalOngoing = false;
                            this.keyRenewalCompletedEvent.Set();
                        }
                    }
                }
                this.keyRenewalCompletedEvent.Wait(timeout);
                lock (base.ThisLock)
                {
                    if (this.IsKeyRenewalNeeded())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SessionKeyExpiredException(System.ServiceModel.SR.GetString("UnableToRenewSessionKey")));
                    }
                }
            }

            protected SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout)
            {
                this.sendCloseHandshake = true;
                bool flag = this.CheckIfKeyRenewalNeeded();
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (flag)
                {
                    this.RenewKey(helper.RemainingTime());
                }
                return this.securityProtocol.SecureOutgoingMessage(ref message, helper.RemainingTime(), null);
            }

            protected SecurityProtocolCorrelationState SendCloseMessage(TimeSpan timeout)
            {
                SecurityProtocolCorrelationState state;
                TimeoutHelper helper = new TimeoutHelper(timeout);
                Message message = this.PrepareCloseMessage();
                try
                {
                    state = this.securityProtocol.SecureOutgoingMessage(ref message, helper.RemainingTime(), null);
                    this.ChannelBinder.Send(message, helper.RemainingTime());
                }
                finally
                {
                    message.Close();
                }
                SecurityTraceRecordHelper.TraceCloseMessageSent(this.currentSessionToken, this.RemoteAddress);
                return state;
            }

            protected void SendCloseResponseMessage(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                Message closeResponse = null;
                try
                {
                    closeResponse = this.closeResponse;
                    this.securityProtocol.SecureOutgoingMessage(ref closeResponse, helper.RemainingTime(), null);
                    this.ChannelBinder.Send(closeResponse, helper.RemainingTime());
                    SecurityTraceRecordHelper.TraceCloseResponseMessageSent(this.currentSessionToken, this.RemoteAddress);
                }
                finally
                {
                    closeResponse.Close();
                }
            }

            private void SetupSessionTokenProvider()
            {
                InitiatorServiceModelSecurityTokenRequirement requirement = new InitiatorServiceModelSecurityTokenRequirement();
                this.Settings.IssuedSecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
                requirement.KeyUsage = SecurityKeyUsage.Signature;
                requirement.SupportSecurityContextCancellation = true;
                requirement.SecurityAlgorithmSuite = this.Settings.SessionProtocolFactory.OutgoingAlgorithmSuite;
                requirement.SecurityBindingElement = this.Settings.SessionProtocolFactory.SecurityBindingElement;
                requirement.TargetAddress = this.to;
                requirement.Via = this.Via;
                requirement.MessageSecurityVersion = this.Settings.SessionProtocolFactory.MessageSecurityVersion.SecurityTokenVersion;
                requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty] = this.Settings.SessionProtocolFactory.PrivacyNoticeUri;
                if (this.channelParameters != null)
                {
                    requirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = this.channelParameters;
                }
                requirement.Properties[ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty] = this.Settings.SessionProtocolFactory.PrivacyNoticeVersion;
                if (this.channelBinder.LocalAddress != null)
                {
                    requirement.DuplexClientLocalAddress = this.channelBinder.LocalAddress;
                }
                this.sessionTokenProvider = this.Settings.SessionProtocolFactory.SecurityTokenManager.CreateSecurityTokenProvider(requirement);
            }

            private bool ShouldWrapException(Exception e)
            {
                return ((e is FormatException) || (e is XmlException));
            }

            private void UpdateSessionTokens(SecurityToken newToken)
            {
                lock (base.ThisLock)
                {
                    this.previousSessionToken = this.currentSessionToken;
                    this.keyRolloverTime = TimeoutHelper.Add(DateTime.UtcNow, this.Settings.KeyRolloverInterval);
                    this.currentSessionToken = newToken;
                    this.keyRenewalTime = this.GetKeyRenewalTime(newToken);
                    List<SecurityToken> tokens = new List<SecurityToken>(2) {
                        this.previousSessionToken,
                        this.currentSessionToken
                    };
                    ((IInitiatorSecuritySessionProtocol) this.securityProtocol).SetIncomingSessionTokens(tokens);
                    ((IInitiatorSecuritySessionProtocol) this.securityProtocol).SetOutgoingSessionToken(this.currentSessionToken);
                    SecurityTraceRecordHelper.TraceSessionKeyRenewed(this.currentSessionToken, this.previousSessionToken, this.RemoteAddress);
                }
            }

            protected void VerifyIncomingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                this.securityProtocol.VerifyIncomingMessage(ref message, timeout, new SecurityProtocolCorrelationState[] { correlationState });
            }

            protected virtual bool CanDoSecurityCorrelation
            {
                get
                {
                    return false;
                }
            }

            protected IClientReliableChannelBinder ChannelBinder
            {
                get
                {
                    return this.channelBinder;
                }
            }

            protected abstract bool ExpectClose { get; }

            protected EndpointAddress InternalLocalAddress
            {
                get
                {
                    if (this.channelBinder != null)
                    {
                        return this.channelBinder.LocalAddress;
                    }
                    return null;
                }
            }

            protected bool IsInputClosed
            {
                get
                {
                    return this.isInputClosed;
                }
            }

            protected bool IsOutputClosed
            {
                get
                {
                    return this.isOutputClosed;
                }
            }

            public System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.to;
                }
            }

            protected bool SendCloseHandshake
            {
                get
                {
                    return this.sendCloseHandshake;
                }
            }

            protected abstract string SessionId { get; }

            protected SecuritySessionClientSettings<TChannel> Settings
            {
                get
                {
                    return this.settings;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.via;
                }
            }

            private class CloseAsyncResult : TraceAsyncResult
            {
                private static readonly AsyncCallback closeCoreCallback;
                private static readonly AsyncCallback closeSessionCallback;
                private static readonly AsyncCallback outputSessionClosedCallback;
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel;
                private TimeoutHelper timeoutHelper;

                static CloseAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.closeSessionCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.CloseSessionCallback));
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.outputSessionClosedCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.OutputSessionClosedCallback));
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.closeCoreCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.CloseCoreCallback));
                }

                public CloseAsyncResult(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    sessionChannel.ThrowIfFaulted();
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    if (!sessionChannel.SendCloseHandshake)
                    {
                        if (this.CloseCore())
                        {
                            base.Complete(true);
                        }
                    }
                    else
                    {
                        bool wasAborted = false;
                        IAsyncResult result = this.sessionChannel.BeginCloseSession(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.closeSessionCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            bool flag = this.sessionChannel.EndCloseSession(result, out wasAborted);
                            if (wasAborted)
                            {
                                base.Complete(true);
                            }
                            else
                            {
                                if (!flag)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ClientSecurityCloseTimeout", new object[] { timeout })));
                                }
                                bool flag3 = this.OnWaitForOutputSessionClose(out wasAborted);
                                if (wasAborted || flag3)
                                {
                                    base.Complete(true);
                                }
                            }
                        }
                    }
                }

                private bool CloseCore()
                {
                    IAsyncResult result = this.sessionChannel.BeginCloseCore(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.closeCoreCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.sessionChannel.EndCloseCore(result);
                    return true;
                }

                private static void CloseCoreCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult) result.AsyncState;
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

                private static void CloseSessionCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            bool flag2;
                            bool flag3 = asyncState.sessionChannel.EndCloseSession(result, out flag2);
                            if (flag2)
                            {
                                flag = true;
                            }
                            else
                            {
                                if (!flag3)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ClientSecurityCloseTimeout", new object[] { asyncState.timeoutHelper.OriginalTimeout })));
                                }
                                flag = asyncState.OnWaitForOutputSessionClose(out flag2);
                                if (flag2)
                                {
                                    flag = true;
                                }
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
                    AsyncResult.End<SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult>(result);
                }

                private bool OnWaitForOutputSessionClose(out bool wasAborted)
                {
                    wasAborted = false;
                    bool flag = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.outputSessionCloseHandle.BeginWait(this.timeoutHelper.RemainingTime(), true, SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult.outputSessionClosedCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.outputSessionCloseHandle.EndWait(result);
                        flag = true;
                    }
                    catch (TimeoutException)
                    {
                        flag = false;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        wasAborted = true;
                    }
                    if (wasAborted)
                    {
                        return true;
                    }
                    if (!flag)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ClientSecurityOutputSessionCloseTimeout", new object[] { this.timeoutHelper.OriginalTimeout })));
                    }
                    return this.CloseCore();
                }

                private static void OutputSessionClosedCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseAsyncResult) result.AsyncState;
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            bool flag2 = false;
                            bool flag3 = false;
                            try
                            {
                                asyncState.sessionChannel.outputSessionCloseHandle.EndWait(result);
                                flag2 = true;
                            }
                            catch (TimeoutException)
                            {
                                flag2 = false;
                            }
                            catch (CommunicationObjectFaultedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                flag3 = true;
                            }
                            if (!flag3)
                            {
                                if (!flag2)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new TimeoutException(System.ServiceModel.SR.GetString("ClientSecurityOutputSessionCloseTimeout", new object[] { asyncState.timeoutHelper.OriginalTimeout })));
                                }
                                flag = asyncState.CloseCore();
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                            flag = true;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }
            }

            private class CloseCoreAsyncResult : TraceAsyncResult
            {
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel channel;
                private static AsyncCallback closeChannelBinderCallback;
                private static AsyncCallback closeTokenProviderCallback;
                private TimeoutHelper timeoutHelper;

                static CloseCoreAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult.closeChannelBinderCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult.ChannelBinderCloseCallback));
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult.closeTokenProviderCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult.CloseTokenProviderCallback));
                }

                public CloseCoreAsyncResult(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    bool flag = false;
                    if (channel.channelBinder != null)
                    {
                        try
                        {
                            IAsyncResult result = this.channel.channelBinder.BeginClose(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult.closeChannelBinderCallback, this);
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
                        base.Complete(true);
                    }
                }

                private static void ChannelBinderCloseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult) result.AsyncState;
                        Exception exception = null;
                        bool flag = false;
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

                private static void CloseTokenProviderCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult) result.AsyncState;
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            try
                            {
                                System.ServiceModel.Security.SecurityUtils.EndCloseTokenProviderIfRequired(result);
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
                                flag = asyncState.OnTokenProviderClosed();
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
                    AsyncResult.End<SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult>(result);
                }

                private bool OnChannelBinderClosed()
                {
                    if (this.channel.sessionTokenProvider != null)
                    {
                        try
                        {
                            IAsyncResult result = System.ServiceModel.Security.SecurityUtils.BeginCloseTokenProviderIfRequired(this.channel.sessionTokenProvider, this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseCoreAsyncResult.closeTokenProviderCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return false;
                            }
                            System.ServiceModel.Security.SecurityUtils.EndCloseTokenProviderIfRequired(result);
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
                    return this.OnTokenProviderClosed();
                }

                private bool OnTokenProviderClosed()
                {
                    this.channel.keyRenewalCompletedEvent.Abort(this.channel);
                    this.channel.inputSessionClosedHandle.Abort(this.channel);
                    return true;
                }
            }

            private class CloseSessionAsyncResult : TraceAsyncResult
            {
                private bool closeCompleted;
                private static readonly AsyncCallback closeOutputSessionCallback;
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel;
                private static readonly AsyncCallback shutdownWaitCallback;
                private TimeoutHelper timeoutHelper;
                private bool wasAborted;

                static CloseSessionAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult.closeOutputSessionCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult.CloseOutputSessionCallback));
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult.shutdownWaitCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult.ShutdownWaitCallback));
                }

                public CloseSessionAsyncResult(TimeSpan timeout, SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    bool flag = false;
                    try
                    {
                        IAsyncResult result = this.sessionChannel.BeginCloseOutputSession(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult.closeOutputSessionCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        this.sessionChannel.EndCloseOutputSession(result);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        flag = true;
                        this.wasAborted = true;
                    }
                    if (!this.wasAborted)
                    {
                        flag = this.OnOutputSessionClosed();
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }

                private static void CloseOutputSessionCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            try
                            {
                                asyncState.sessionChannel.EndCloseOutputSession(result);
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                if (asyncState.sessionChannel.State != CommunicationState.Closed)
                                {
                                    throw;
                                }
                                asyncState.wasAborted = true;
                                flag = true;
                            }
                            if (!asyncState.wasAborted)
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

                public static bool End(IAsyncResult result, out bool wasAborted)
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult result2 = AsyncResult.End<SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult>(result);
                    wasAborted = result2.wasAborted;
                    ServiceModelActivity.Stop(result2.CallbackActivity);
                    return result2.closeCompleted;
                }

                private bool OnOutputSessionClosed()
                {
                    try
                    {
                        IAsyncResult result = this.sessionChannel.inputSessionClosedHandle.BeginWait(this.timeoutHelper.RemainingTime(), true, SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult.shutdownWaitCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.sessionChannel.inputSessionClosedHandle.EndWait(result);
                        this.closeCompleted = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        if (this.sessionChannel.State != CommunicationState.Closed)
                        {
                            throw;
                        }
                        this.wasAborted = true;
                    }
                    catch (TimeoutException)
                    {
                        this.closeCompleted = false;
                    }
                    return true;
                }

                private static void ShutdownWaitCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.CloseSessionAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.sessionChannel.inputSessionClosedHandle.EndWait(result);
                            asyncState.closeCompleted = true;
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            if (asyncState.sessionChannel.State != CommunicationState.Closed)
                            {
                                throw;
                            }
                            asyncState.wasAborted = true;
                        }
                        catch (TimeoutException)
                        {
                            asyncState.closeCompleted = false;
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

            private class KeyRenewalAsyncResult : TraceAsyncResult
            {
                private Message message;
                private static readonly Action<object> renewKeyCallback;
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel;
                private TimeoutHelper timeoutHelper;

                static KeyRenewalAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.KeyRenewalAsyncResult.renewKeyCallback = new Action<object>(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.KeyRenewalAsyncResult.RenewKeyCallback);
                }

                public KeyRenewalAsyncResult(Message message, SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.message = message;
                    this.sessionChannel = sessionChannel;
                    ActionItem.Schedule(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.KeyRenewalAsyncResult.renewKeyCallback, this);
                }

                public static Message End(IAsyncResult result, out TimeSpan remainingTime)
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.KeyRenewalAsyncResult result2 = AsyncResult.End<SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.KeyRenewalAsyncResult>(result);
                    remainingTime = result2.timeoutHelper.RemainingTime();
                    return result2.message;
                }

                private static void RenewKeyCallback(object state)
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.KeyRenewalAsyncResult result = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.KeyRenewalAsyncResult) state;
                    Exception exception = null;
                    try
                    {
                        using ((result.CallbackActivity == null) ? null : ServiceModelActivity.BoundOperation(result.CallbackActivity))
                        {
                            result.sessionChannel.RenewKey(result.timeoutHelper.RemainingTime());
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    result.Complete(false, exception);
                }
            }

            private class OpenAsyncResult : TraceAsyncResult
            {
                private static readonly AsyncCallback getTokenCallback;
                private static readonly AsyncCallback openTokenProviderCallback;
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel;
                private TimeoutHelper timeoutHelper;

                static OpenAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult.getTokenCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult.GetTokenCallback));
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult.openTokenProviderCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult.OpenTokenProviderCallback));
                }

                public OpenAsyncResult(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.sessionChannel = sessionChannel;
                    this.sessionChannel.SetupSessionTokenProvider();
                    IAsyncResult result = System.ServiceModel.Security.SecurityUtils.BeginOpenTokenProviderIfRequired(this.sessionChannel.sessionTokenProvider, this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult.openTokenProviderCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        System.ServiceModel.Security.SecurityUtils.EndOpenTokenProviderIfRequired(result);
                        if (this.OnTokenProviderOpened())
                        {
                            base.Complete(true);
                        }
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult>(result);
                    ServiceModelActivity.Stop(((SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult) result).CallbackActivity);
                }

                private static void GetTokenCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult) result.AsyncState;
                        try
                        {
                            using (ServiceModelActivity.BoundOperation(asyncState.CallbackActivity))
                            {
                                bool flag = false;
                                Exception exception = null;
                                try
                                {
                                    SecurityToken sessionToken = asyncState.sessionChannel.sessionTokenProvider.EndGetToken(result);
                                    flag = asyncState.OnTokenObtained(sessionToken);
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
                        finally
                        {
                            if (asyncState.CallbackActivity != null)
                            {
                                asyncState.CallbackActivity.Dispose();
                            }
                        }
                    }
                }

                private bool OnTokenObtained(SecurityToken sessionToken)
                {
                    this.sessionChannel.OpenCore(sessionToken, this.timeoutHelper.RemainingTime());
                    return true;
                }

                private bool OnTokenProviderOpened()
                {
                    IAsyncResult result = this.sessionChannel.sessionTokenProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult.getTokenCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    SecurityToken sessionToken = this.sessionChannel.sessionTokenProvider.EndGetToken(result);
                    return this.OnTokenObtained(sessionToken);
                }

                private static void OpenTokenProviderCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.OpenAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            System.ServiceModel.Security.SecurityUtils.EndOpenTokenProviderIfRequired(result);
                            flag = asyncState.OnTokenProviderOpened();
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

            private class ReceiveAsyncResult : TraceAsyncResult
            {
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel channel;
                private SecurityProtocolCorrelationState correlationState;
                private Message message;
                private static AsyncCallback onReceive;
                private TimeoutHelper timeoutHelper;

                static ReceiveAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.ReceiveAsyncResult.onReceive = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.ReceiveAsyncResult.OnReceive));
                }

                public ReceiveAsyncResult(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel channel, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.channel = channel;
                    this.correlationState = correlationState;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    IAsyncResult result = channel.ChannelBinder.BeginTryReceive(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.ReceiveAsyncResult.onReceive, this);
                    if (result.CompletedSynchronously && this.CompleteReceive(result))
                    {
                        base.Complete(true);
                    }
                }

                private bool CompleteReceive(IAsyncResult result)
                {
                    while (!this.channel.isInputClosed)
                    {
                        RequestContext context;
                        if (this.channel.ChannelBinder.EndTryReceive(result, out context))
                        {
                            if (context == null)
                            {
                                break;
                            }
                            this.message = this.channel.ProcessRequestContext(context, this.timeoutHelper.RemainingTime(), this.correlationState);
                            if ((this.message != null) || this.channel.isInputClosed)
                            {
                                break;
                            }
                        }
                        if (this.timeoutHelper.RemainingTime() == TimeSpan.Zero)
                        {
                            break;
                        }
                        result = this.channel.ChannelBinder.BeginTryReceive(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.ReceiveAsyncResult.onReceive, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                    }
                    return true;
                }

                public static Message End(IAsyncResult result)
                {
                    return AsyncResult.End<SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.ReceiveAsyncResult>(result).message;
                }

                private static void OnReceive(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.ReceiveAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.ReceiveAsyncResult) result.AsyncState;
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
            }

            internal sealed class SecureSendAsyncResult : SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResultBase
            {
                private bool autoCloseMessage;
                private static readonly AsyncCallback sendCallback;

                static SecureSendAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult.sendCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult.SendCallback));
                }

                public SecureSendAsyncResult(Message message, SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state, bool autoCloseMessage) : base(message, sessionChannel, timeout, callback, state)
                {
                    this.autoCloseMessage = autoCloseMessage;
                    if (base.DidSecureOutgoingMessageCompleteSynchronously && this.OnMessageSecured())
                    {
                        base.Complete(true);
                    }
                }

                public static SecurityProtocolCorrelationState End(IAsyncResult result)
                {
                    return AsyncResult.End<SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult>(result).SecurityCorrelationState;
                }

                protected override bool OnMessageSecured()
                {
                    bool flag2;
                    bool flag = true;
                    try
                    {
                        IAsyncResult result = base.ChannelBinder.BeginSend(base.Message, base.TimeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult.sendCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            flag = false;
                            return false;
                        }
                        base.ChannelBinder.EndSend(result);
                        flag2 = true;
                    }
                    finally
                    {
                        if ((flag && this.autoCloseMessage) && (base.Message != null))
                        {
                            base.Message.Close();
                        }
                    }
                    return flag2;
                }

                private static void SendCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.ChannelBinder.EndSend(result);
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
                            if (asyncState.autoCloseMessage && (asyncState.Message != null))
                            {
                                asyncState.Message.Close();
                            }
                            if ((asyncState.CallbackActivity != null) && DiagnosticUtility.ShouldUseActivity)
                            {
                                asyncState.CallbackActivity.Stop();
                            }
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }

            internal abstract class SecureSendAsyncResultBase : TraceAsyncResult
            {
                private SecurityProtocolCorrelationState correlationState;
                private bool didSecureOutgoingMessageCompleteSynchronously;
                private System.ServiceModel.Channels.Message message;
                private static readonly AsyncCallback secureOutgoingMessageCallback;
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel;
                private System.Runtime.TimeoutHelper timeoutHelper;

                static SecureSendAsyncResultBase()
                {
                    SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResultBase.secureOutgoingMessageCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResultBase.SecureOutgoingMessageCallback));
                }

                protected SecureSendAsyncResultBase(System.ServiceModel.Channels.Message message, SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.message = message;
                    this.sessionChannel = sessionChannel;
                    this.timeoutHelper = new System.Runtime.TimeoutHelper(timeout);
                    IAsyncResult result = this.sessionChannel.BeginSecureOutgoingMessage(message, this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResultBase.secureOutgoingMessageCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        this.message = this.sessionChannel.EndSecureOutgoingMessage(result, out this.correlationState);
                        this.didSecureOutgoingMessageCompleteSynchronously = true;
                    }
                }

                protected abstract bool OnMessageSecured();
                private static void SecureOutgoingMessageCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResultBase asyncState = (SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResultBase) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            asyncState.message = asyncState.sessionChannel.EndSecureOutgoingMessage(result, out asyncState.correlationState);
                            flag = asyncState.OnMessageSecured();
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

                protected IClientReliableChannelBinder ChannelBinder
                {
                    get
                    {
                        return this.sessionChannel.ChannelBinder;
                    }
                }

                protected bool DidSecureOutgoingMessageCompleteSynchronously
                {
                    get
                    {
                        return this.didSecureOutgoingMessageCompleteSynchronously;
                    }
                }

                protected System.ServiceModel.Channels.Message Message
                {
                    get
                    {
                        return this.message;
                    }
                }

                protected SecurityProtocolCorrelationState SecurityCorrelationState
                {
                    get
                    {
                        return this.correlationState;
                    }
                }

                protected System.Runtime.TimeoutHelper TimeoutHelper
                {
                    get
                    {
                        return this.timeoutHelper;
                    }
                }
            }

            protected class SoapSecurityOutputSession : ISecureConversationSession, ISecuritySession, IOutputSession, ISession
            {
                private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel channel;
                private EndpointIdentity remoteIdentity;
                private UniqueId sessionId;
                private SecurityKeyIdentifierClause sessionTokenIdentifier;
                private SecurityStandardsManager standardsManager;

                public SoapSecurityOutputSession(SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel channel)
                {
                    this.channel = channel;
                }

                private UniqueId GetSessionId(SecurityToken sessionToken, SecurityStandardsManager standardsManager)
                {
                    GenericXmlSecurityToken token = sessionToken as GenericXmlSecurityToken;
                    if (token == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SessionTokenIsNotGenericXmlToken", new object[] { sessionToken, typeof(GenericXmlSecurityToken) })));
                    }
                    return standardsManager.SecureConversationDriver.GetSecurityContextTokenId(XmlDictionaryReader.CreateDictionaryReader(new XmlNodeReader(token.TokenXml)));
                }

                internal void Initialize(SecurityToken sessionToken, SecuritySessionClientSettings<TChannel> settings)
                {
                    if (sessionToken == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sessionToken");
                    }
                    if (settings == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
                    }
                    Claim primaryIdentityClaim = System.ServiceModel.Security.SecurityUtils.GetPrimaryIdentityClaim(((GenericXmlSecurityToken) sessionToken).AuthorizationPolicies);
                    if (primaryIdentityClaim != null)
                    {
                        this.remoteIdentity = EndpointIdentity.CreateIdentity(primaryIdentityClaim);
                    }
                    this.standardsManager = settings.SessionProtocolFactory.StandardsManager;
                    this.sessionId = this.GetSessionId(sessionToken, this.standardsManager);
                    this.sessionTokenIdentifier = settings.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(sessionToken, SecurityTokenReferenceStyle.External);
                }

                public bool TryReadSessionTokenIdentifier(XmlReader reader)
                {
                    this.channel.ThrowIfDisposedOrNotOpen();
                    if (!this.standardsManager.SecurityTokenSerializer.CanReadKeyIdentifierClause(reader))
                    {
                        return false;
                    }
                    SecurityContextKeyIdentifierClause clause = this.standardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(reader) as SecurityContextKeyIdentifierClause;
                    return ((clause != null) && clause.Matches(this.sessionId, null));
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
                        if (this.sessionId == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ChannelMustBeOpenedToGetSessionId")));
                        }
                        return this.sessionId.ToString();
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

        private abstract class ClientSecuritySimplexSessionChannel : SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel
        {
            private SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SoapSecurityOutputSession outputSession;

            protected ClientSecuritySimplexSessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via) : base(settings, to, via)
            {
                this.outputSession = new SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SoapSecurityOutputSession(this);
            }

            protected override void InitializeSession(SecurityToken sessionToken)
            {
                this.outputSession.Initialize(sessionToken, base.Settings);
            }

            protected override bool ExpectClose
            {
                get
                {
                    return false;
                }
            }

            public IOutputSession Session
            {
                get
                {
                    return this.outputSession;
                }
            }

            protected override string SessionId
            {
                get
                {
                    return this.Session.Id;
                }
            }
        }

        private sealed class SecurityRequestSessionChannel : SecuritySessionClientSettings<TChannel>.ClientSecuritySimplexSessionChannel, IRequestSessionChannel, IRequestChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
        {
            public SecurityRequestSessionChannel(SecuritySessionClientSettings<TChannel> settings, EndpointAddress to, Uri via) : base(settings, to, via)
            {
            }

            private IAsyncResult BeginBaseCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.BeginCloseOutputSession(timeout, callback, state);
            }

            protected override IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                return new CloseOutputSessionAsyncResult<TChannel>((SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel) this, timeout, callback, state);
            }

            public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
            {
                return this.BeginRequest(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                base.CheckOutputOpen();
                return new SecureRequestAsyncResult<TChannel>(message, this, timeout, callback, state);
            }

            protected override SecurityProtocolCorrelationState CloseOutputSession(TimeSpan timeout)
            {
                base.ThrowIfFaulted();
                TimeoutHelper helper = new TimeoutHelper(timeout);
                SecurityProtocolCorrelationState correlationState = base.CloseOutputSession(helper.RemainingTime());
                Message message = base.ReceiveInternal(helper.RemainingTime(), correlationState);
                if (message != null)
                {
                    using (message)
                    {
                        throw TraceUtility.ThrowHelperWarning(ProtocolException.ReceiveShutdownReturnedNonNull(message), message);
                    }
                }
                return null;
            }

            private SecurityProtocolCorrelationState EndBaseCloseOutputSession(IAsyncResult result)
            {
                return base.EndCloseOutputSession(result);
            }

            protected override SecurityProtocolCorrelationState EndCloseOutputSession(IAsyncResult result)
            {
                CloseOutputSessionAsyncResult<TChannel>.End(result);
                return null;
            }

            public Message EndRequest(IAsyncResult result)
            {
                SecurityProtocolCorrelationState state;
                TimeSpan span;
                Message reply = SecureRequestAsyncResult<TChannel>.EndAsReply(result, out state, out span);
                return this.ProcessReply(reply, span, state);
            }

            private Message ProcessReply(Message reply, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                if (reply == null)
                {
                    return null;
                }
                Message message = reply;
                Message message2 = null;
                MessageFault protocolFault = null;
                Exception exception = null;
                try
                {
                    message2 = base.ProcessIncomingMessage(reply, timeout, correlationState, out protocolFault);
                }
                catch (MessageSecurityException)
                {
                    if (message.IsFault)
                    {
                        MessageFault fault = MessageFault.CreateFault(message, 0x4000);
                        if (System.ServiceModel.Security.SecurityUtils.IsSecurityFault(fault, base.Settings.standardsManager))
                        {
                            exception = System.ServiceModel.Security.SecurityUtils.CreateSecurityFaultException(fault);
                        }
                    }
                    if (exception == null)
                    {
                        throw;
                    }
                }
                if (exception != null)
                {
                    base.Fault(exception);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
                if ((message2 == null) && (protocolFault != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SecuritySessionFaultReplyWasSent"), new FaultException(protocolFault)));
                }
                return message2;
            }

            public Message Request(Message message)
            {
                return this.Request(message, base.DefaultSendTimeout);
            }

            public Message Request(Message message, TimeSpan timeout)
            {
                base.ThrowIfFaulted();
                base.CheckOutputOpen();
                TimeoutHelper helper = new TimeoutHelper(timeout);
                SecurityProtocolCorrelationState correlationState = base.SecureOutgoingMessage(ref message, helper.RemainingTime());
                Message reply = base.ChannelBinder.Request(message, helper.RemainingTime());
                return this.ProcessReply(reply, helper.RemainingTime(), correlationState);
            }

            protected override bool CanDoSecurityCorrelation
            {
                get
                {
                    return true;
                }
            }

            private class CloseOutputSessionAsyncResult : TraceAsyncResult
            {
                private static readonly AsyncCallback baseCloseOutputSessionCallback;
                private SecurityProtocolCorrelationState correlationState;
                private static readonly AsyncCallback receiveInternalCallback;
                private SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel requestChannel;
                private TimeoutHelper timeoutHelper;

                static CloseOutputSessionAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult.baseCloseOutputSessionCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult.BaseCloseOutputSessionCallback));
                    SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult.receiveInternalCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult.ReceiveInternalCallback));
                }

                public CloseOutputSessionAsyncResult(SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel requestChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.requestChannel = requestChannel;
                    IAsyncResult result = this.requestChannel.BeginBaseCloseOutputSession(this.timeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult.baseCloseOutputSessionCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        this.correlationState = this.requestChannel.EndBaseCloseOutputSession(result);
                        if (this.OnBaseOutputSessionClosed())
                        {
                            base.Complete(true);
                        }
                    }
                }

                private static void BaseCloseOutputSessionCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            asyncState.correlationState = asyncState.requestChannel.EndBaseCloseOutputSession(result);
                            flag = asyncState.OnBaseOutputSessionClosed();
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
                    AsyncResult.End<SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult>(result);
                }

                private bool OnBaseOutputSessionClosed()
                {
                    IAsyncResult result = this.requestChannel.BeginReceiveInternal(this.timeoutHelper.RemainingTime(), this.correlationState, SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult.receiveInternalCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    Message message = this.requestChannel.EndReceiveInternal(result);
                    return this.OnMessageReceived(message);
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
                    return true;
                }

                private static void ReceiveInternalCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.CloseOutputSessionAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            Message message = asyncState.requestChannel.EndReceiveInternal(result);
                            flag = asyncState.OnMessageReceived(message);
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

            private sealed class SecureRequestAsyncResult : SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel.SecureSendAsyncResultBase
            {
                private Message reply;
                private static readonly AsyncCallback requestCallback;

                static SecureRequestAsyncResult()
                {
                    SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.SecureRequestAsyncResult.requestCallback = Fx.ThunkCallback(new AsyncCallback(SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.SecureRequestAsyncResult.RequestCallback));
                }

                public SecureRequestAsyncResult(Message request, SecuritySessionClientSettings<TChannel>.ClientSecuritySessionChannel sessionChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(request, sessionChannel, timeout, callback, state)
                {
                    if (base.DidSecureOutgoingMessageCompleteSynchronously && this.OnMessageSecured())
                    {
                        base.Complete(true);
                    }
                }

                public static Message EndAsReply(IAsyncResult result, out SecurityProtocolCorrelationState correlationState, out TimeSpan remainingTime)
                {
                    SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.SecureRequestAsyncResult result2 = AsyncResult.End<SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.SecureRequestAsyncResult>(result);
                    correlationState = result2.SecurityCorrelationState;
                    remainingTime = result2.TimeoutHelper.RemainingTime();
                    return result2.reply;
                }

                protected override bool OnMessageSecured()
                {
                    IAsyncResult result = base.ChannelBinder.BeginRequest(base.Message, base.TimeoutHelper.RemainingTime(), SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.SecureRequestAsyncResult.requestCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.reply = base.ChannelBinder.EndRequest(result);
                    return true;
                }

                private static void RequestCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.SecureRequestAsyncResult asyncState = (SecuritySessionClientSettings<TChannel>.SecurityRequestSessionChannel.SecureRequestAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.reply = asyncState.ChannelBinder.EndRequest(result);
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
    }
}

