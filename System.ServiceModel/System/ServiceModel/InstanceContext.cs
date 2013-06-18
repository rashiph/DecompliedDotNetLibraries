namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    public sealed class InstanceContext : CommunicationObject, IExtensibleObject<InstanceContext>
    {
        private bool autoClose;
        private InstanceBehavior behavior;
        private ServiceChannelManager channels;
        private ConcurrencyInstanceContextFacet concurrency;
        private ExtensionCollection<InstanceContext> extensions;
        private readonly ServiceHostBase host;
        private int instanceContextManagerIndex;
        private bool isUserCreated;
        internal static InstanceContextEmptyCallback NotifyEmptyCallback = new InstanceContextEmptyCallback(InstanceContext.NotifyEmpty);
        internal static InstanceContextIdleCallback NotifyIdleCallback = new InstanceContextIdleCallback(InstanceContext.NotifyIdle);
        private System.ServiceModel.Dispatcher.QuotaThrottle quotaThrottle;
        private object serviceInstanceLock;
        private System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle;
        private System.Threading.SynchronizationContext synchronizationContext;
        private TransactionInstanceContextFacet transaction;
        private object userObject;
        private bool wellKnown;
        private SynchronizedCollection<IChannel> wmiChannels;

        public InstanceContext(object implementation) : this(null, implementation)
        {
        }

        public InstanceContext(ServiceHostBase host) : this(host, true)
        {
        }

        internal InstanceContext(ServiceHostBase host, bool isUserCreated)
        {
            this.serviceInstanceLock = new object();
            if (host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("host"));
            }
            this.host = host;
            this.autoClose = true;
            this.channels = new ServiceChannelManager(this, NotifyEmptyCallback);
            this.isUserCreated = isUserCreated;
        }

        public InstanceContext(ServiceHostBase host, object implementation) : this(host, implementation, true)
        {
        }

        internal InstanceContext(ServiceHostBase host, object implementation, bool isUserCreated) : this(host, implementation, true, isUserCreated)
        {
        }

        internal InstanceContext(ServiceHostBase host, object implementation, bool wellKnown, bool isUserCreated)
        {
            this.serviceInstanceLock = new object();
            this.host = host;
            if (implementation != null)
            {
                this.userObject = implementation;
                this.wellKnown = wellKnown;
            }
            this.autoClose = false;
            this.channels = new ServiceChannelManager(this);
            this.isUserCreated = isUserCreated;
        }

        internal IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channels.BeginCloseInput(timeout, callback, state);
        }

        internal void BindIncomingChannel(ServiceChannel channel)
        {
            base.ThrowIfDisposed();
            channel.InstanceContext = this;
            this.channels.AddIncomingChannel((IChannel) channel.Proxy);
        }

        internal void BindRpc(ref MessageRpc rpc)
        {
            base.ThrowIfClosed();
            this.channels.IncrementActivityCount();
            rpc.SuccessfullyBoundInstance = true;
        }

        private void CloseIfNotBusy()
        {
            if (base.State != CommunicationState.Created)
            {
                CommunicationState state = base.State;
            }
            if (((base.State == CommunicationState.Opened) && !this.IsBusy) && this.behavior.CanUnload(this))
            {
                try
                {
                    if (base.State == CommunicationState.Opened)
                    {
                        base.Close();
                    }
                }
                catch (ObjectDisposedException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (InvalidOperationException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception4)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                }
            }
        }

        internal void CloseInput(TimeSpan timeout)
        {
            this.channels.CloseInput(timeout);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void CompleteAttachedTransaction()
        {
            Exception error = null;
            if (!this.behavior.TransactionAutoCompleteOnSessionClose)
            {
                error = new Exception();
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0xe000b, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusAbortedOnSessionClose", new object[] { this.transaction.Attached.TransactionInformation.LocalIdentifier }));
                }
            }
            else if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xe0008, System.ServiceModel.SR.GetString("TraceCodeTxCompletionStatusCompletedForTACOSC", new object[] { this.transaction.Attached.TransactionInformation.LocalIdentifier }));
            }
            this.transaction.CompletePendingTransaction(this.transaction.Attached, error);
            this.transaction.Attached = null;
        }

        internal void EndCloseInput(IAsyncResult result)
        {
            this.channels.EndCloseInput(result);
        }

        private System.ServiceModel.Dispatcher.QuotaThrottle EnsureQuotaThrottle()
        {
            lock (this.ThisLock)
            {
                if (this.quotaThrottle == null)
                {
                    this.quotaThrottle = new System.ServiceModel.Dispatcher.QuotaThrottle(new WaitCallback(ImmutableDispatchRuntime.GotDynamicInstanceContext), this.ThisLock);
                    this.quotaThrottle.Owner = "InstanceContext";
                }
                return this.quotaThrottle;
            }
        }

        internal void FaultInternal()
        {
            base.Fault();
        }

        public object GetServiceInstance()
        {
            return this.GetServiceInstance(null);
        }

        public object GetServiceInstance(Message message)
        {
            lock (this.serviceInstanceLock)
            {
                object instance;
                base.ThrowIfClosedOrNotOpen();
                object userObject = this.userObject;
                if (userObject != null)
                {
                    return userObject;
                }
                if (this.behavior == null)
                {
                    Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInstanceNotInitialized"));
                    if (message != null)
                    {
                        throw TraceUtility.ThrowHelperError(exception, message);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
                if (message != null)
                {
                    instance = this.behavior.GetInstance(this, message);
                }
                else
                {
                    instance = this.behavior.GetInstance(this);
                }
                if (instance != null)
                {
                    this.SetUserObject(instance);
                }
                return instance;
            }
        }

        public int IncrementManualFlowControlLimit(int incrementBy)
        {
            return this.EnsureQuotaThrottle().IncrementLimit(incrementBy);
        }

        private void Load()
        {
            if (this.behavior != null)
            {
                this.behavior.Initialize(this);
            }
            if (this.host != null)
            {
                this.host.BindInstance(this);
            }
        }

        private static void NotifyEmpty(InstanceContext instanceContext)
        {
            if (instanceContext.autoClose)
            {
                instanceContext.CloseIfNotBusy();
            }
        }

        private static void NotifyIdle(InstanceContext instanceContext)
        {
            instanceContext.CloseIfNotBusy();
        }

        protected override void OnAbort()
        {
            this.channels.Abort();
            this.Unload();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(timeout, callback, state, this);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.channels.Close(timeout);
            this.Unload();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle != null)
            {
                serviceThrottle.DeactivateInstanceContext();
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            if (this.IsSingleton && (this.host != null))
            {
                this.host.FaultInternal();
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            new TimeoutHelper(timeout);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
        }

        protected override void OnOpening()
        {
            this.Load();
            base.OnOpening();
        }

        public void ReleaseServiceInstance()
        {
            base.ThrowIfDisposedOrNotOpen();
            this.SetUserObject(null);
        }

        private void SetUserObject(object newUserObject)
        {
            if ((this.behavior != null) && !this.wellKnown)
            {
                object objA = Interlocked.Exchange(ref this.userObject, newUserObject);
                if (((objA != null) && (this.host != null)) && !object.Equals(objA, this.host.DisposableInstance))
                {
                    this.behavior.ReleaseInstance(this, objA);
                }
            }
        }

        internal void UnbindIncomingChannel(ServiceChannel channel)
        {
            this.channels.RemoveChannel((IChannel) channel.Proxy);
        }

        internal void UnbindRpc(ref MessageRpc rpc)
        {
            if ((rpc.InstanceContext == this) && rpc.SuccessfullyBoundInstance)
            {
                this.channels.DecrementActivityCount();
            }
        }

        private void Unload()
        {
            this.SetUserObject(null);
            if (this.host != null)
            {
                this.host.UnbindInstance(this);
            }
        }

        internal bool AutoClose
        {
            get
            {
                return this.autoClose;
            }
            set
            {
                this.autoClose = value;
            }
        }

        internal InstanceBehavior Behavior
        {
            get
            {
                return this.behavior;
            }
            set
            {
                if (this.behavior == null)
                {
                    this.behavior = value;
                }
            }
        }

        internal ConcurrencyInstanceContextFacet Concurrency
        {
            get
            {
                if (this.concurrency == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.concurrency == null)
                        {
                            this.concurrency = new ConcurrencyInstanceContextFacet();
                        }
                    }
                }
                return this.concurrency;
            }
        }

        internal static InstanceContext Current
        {
            get
            {
                if (OperationContext.Current == null)
                {
                    return null;
                }
                return OperationContext.Current.InstanceContext;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                if (this.host != null)
                {
                    return this.host.CloseTimeout;
                }
                return ServiceDefaults.CloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                if (this.host != null)
                {
                    return this.host.OpenTimeout;
                }
                return ServiceDefaults.OpenTimeout;
            }
        }

        public IExtensionCollection<InstanceContext> Extensions
        {
            get
            {
                base.ThrowIfClosed();
                lock (this.ThisLock)
                {
                    if (this.extensions == null)
                    {
                        this.extensions = new ExtensionCollection<InstanceContext>(this, this.ThisLock);
                    }
                    return this.extensions;
                }
            }
        }

        internal bool HasTransaction
        {
            get
            {
                return ((this.transaction != null) && !object.Equals(this.transaction.Attached, null));
            }
        }

        public ServiceHostBase Host
        {
            get
            {
                base.ThrowIfClosed();
                return this.host;
            }
        }

        public ICollection<IChannel> IncomingChannels
        {
            get
            {
                base.ThrowIfClosed();
                return this.channels.IncomingChannels;
            }
        }

        internal int InstanceContextManagerIndex
        {
            get
            {
                return this.instanceContextManagerIndex;
            }
            set
            {
                this.instanceContextManagerIndex = value;
            }
        }

        private bool IsBusy
        {
            get
            {
                if (base.State == CommunicationState.Closed)
                {
                    return false;
                }
                return this.channels.IsBusy;
            }
        }

        private bool IsSingleton
        {
            get
            {
                return ((this.behavior != null) && InstanceContextProviderBase.IsProviderSingleton(this.behavior.InstanceContextProvider));
            }
        }

        internal bool IsUserCreated
        {
            get
            {
                return this.isUserCreated;
            }
            set
            {
                this.isUserCreated = value;
            }
        }

        internal bool IsWellKnown
        {
            get
            {
                return this.wellKnown;
            }
        }

        public int ManualFlowControlLimit
        {
            get
            {
                return this.EnsureQuotaThrottle().Limit;
            }
            set
            {
                this.EnsureQuotaThrottle().SetLimit(value);
            }
        }

        public ICollection<IChannel> OutgoingChannels
        {
            get
            {
                base.ThrowIfClosed();
                return this.channels.OutgoingChannels;
            }
        }

        internal System.ServiceModel.Dispatcher.QuotaThrottle QuotaThrottle
        {
            get
            {
                return this.quotaThrottle;
            }
        }

        internal System.ServiceModel.Dispatcher.ServiceThrottle ServiceThrottle
        {
            get
            {
                return this.serviceThrottle;
            }
            set
            {
                base.ThrowIfDisposed();
                this.serviceThrottle = value;
            }
        }

        public System.Threading.SynchronizationContext SynchronizationContext
        {
            get
            {
                return this.synchronizationContext;
            }
            set
            {
                base.ThrowIfClosedOrOpened();
                this.synchronizationContext = value;
            }
        }

        internal object ThisLock
        {
            get
            {
                return base.ThisLock;
            }
        }

        internal TransactionInstanceContextFacet Transaction
        {
            get
            {
                if (this.transaction == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.transaction == null)
                        {
                            this.transaction = new TransactionInstanceContextFacet(this);
                        }
                    }
                }
                return this.transaction;
            }
        }

        internal object UserObject
        {
            get
            {
                return this.userObject;
            }
        }

        internal ICollection<IChannel> WmiChannels
        {
            get
            {
                if (this.wmiChannels == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.wmiChannels == null)
                        {
                            this.wmiChannels = new SynchronizedCollection<IChannel>();
                        }
                    }
                }
                return this.wmiChannels;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private InstanceContext instanceContext;
            private TimeoutHelper timeoutHelper;

            public CloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, InstanceContext instanceContext) : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.instanceContext = instanceContext;
                IAsyncResult result = this.instanceContext.channels.BeginClose(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(new AsyncResult.AsyncCompletion(this.CloseChannelsCallback)), this);
                if (result.CompletedSynchronously && this.CloseChannelsCallback(result))
                {
                    base.Complete(true);
                }
            }

            private bool CloseChannelsCallback(IAsyncResult result)
            {
                this.instanceContext.channels.EndClose(result);
                this.instanceContext.Unload();
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<InstanceContext.CloseAsyncResult>(result);
            }
        }
    }
}

