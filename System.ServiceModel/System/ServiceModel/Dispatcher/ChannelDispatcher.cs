namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Transactions;

    public class ChannelDispatcher : ChannelDispatcherBase
    {
        private ThreadSafeMessageFilterTable<EndpointAddress> addressTable;
        private string bindingName;
        private SynchronizedCollection<IChannelInitializer> channelInitializers;
        private CommunicationObjectManager<IChannel> channels;
        private EndpointDispatcherCollection endpointDispatchers;
        private ErrorBehavior errorBehavior;
        private Collection<IErrorHandler> errorHandlers;
        private System.ServiceModel.Dispatcher.EndpointDispatcherTable filterTable;
        private ServiceHostBase host;
        private bool includeExceptionDetailInFaults;
        private bool isTransactedReceive;
        private readonly IChannelListener listener;
        private ListenerHandler listenerHandler;
        private int maxPendingReceives;
        private int maxTransactedBatchSize;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private SynchronizedChannelCollection<IChannel> pendingChannels;
        private bool performDefaultCloseInput;
        private bool receiveContextEnabled;
        private bool receiveSynchronously;
        private bool sendAsynchronously;
        private System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle;
        private bool session;
        private SharedRuntimeState shared;
        private IDefaultCommunicationTimeouts timeouts;
        private IsolationLevel transactionIsolationLevel;
        private bool transactionIsolationLevelSet;
        private TimeSpan transactionTimeout;

        public ChannelDispatcher(IChannelListener listener) : this(listener, null, null)
        {
        }

        internal ChannelDispatcher(SharedRuntimeState shared)
        {
            this.transactionIsolationLevel = ServiceBehaviorAttribute.DefaultIsolationLevel;
            this.Initialize(shared);
        }

        public ChannelDispatcher(IChannelListener listener, string bindingName) : this(listener, bindingName, null)
        {
        }

        public ChannelDispatcher(IChannelListener listener, string bindingName, IDefaultCommunicationTimeouts timeouts)
        {
            this.transactionIsolationLevel = ServiceBehaviorAttribute.DefaultIsolationLevel;
            if (listener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listener");
            }
            this.listener = listener;
            this.bindingName = bindingName;
            this.timeouts = new ImmutableCommunicationTimeouts(timeouts);
            this.session = ((listener is IChannelListener<IInputSessionChannel>) || (listener is IChannelListener<IReplySessionChannel>)) || (listener is IChannelListener<IDuplexSessionChannel>);
            this.Initialize(new SharedRuntimeState(true));
        }

        private void AbortPendingChannels()
        {
            lock (base.ThisLock)
            {
                for (int i = this.pendingChannels.Count - 1; i >= 0; i--)
                {
                    this.pendingChannels[i].Abort();
                }
            }
        }

        protected override void Attach(ServiceHostBase host)
        {
            if (host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }
            ServiceHostBase base2 = host;
            this.ThrowIfDisposedOrImmutable();
            if (this.host != null)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelDispatcherMultipleHost0"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            this.host = base2;
        }

        public override void CloseInput()
        {
            this.performDefaultCloseInput = true;
        }

        internal override void CloseInput(TimeSpan timeout)
        {
            this.CloseInput();
            if (this.performDefaultCloseInput)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                lock (base.ThisLock)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        for (int i = 0; i < this.endpointDispatchers.Count; i++)
                        {
                            EndpointDispatcher endpoint = this.endpointDispatchers[i];
                            this.TraceEndpointLifetime(endpoint, 0x40007, System.ServiceModel.SR.GetString("TraceCodeEndpointListenerClose"));
                        }
                    }
                    ListenerHandler listenerHandler = this.listenerHandler;
                    if (listenerHandler != null)
                    {
                        listenerHandler.CloseInput(helper.RemainingTime());
                    }
                }
                if (!this.session)
                {
                    ListenerHandler handler2 = this.listenerHandler;
                    if (handler2 != null)
                    {
                        handler2.Close(helper.RemainingTime());
                    }
                }
            }
        }

        internal string CreateContractListString()
        {
            Collection<string> collection = new Collection<string>();
            StringBuilder builder = new StringBuilder();
            lock (base.ThisLock)
            {
                foreach (EndpointDispatcher dispatcher in this.Endpoints)
                {
                    if (!collection.Contains(dispatcher.ContractName))
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                            builder.Append(" ");
                        }
                        builder.Append("\"");
                        builder.Append(dispatcher.ContractName);
                        builder.Append("\"");
                        collection.Add(dispatcher.ContractName);
                    }
                }
            }
            return builder.ToString();
        }

        private InvalidOperationException CreateOuterExceptionWithEndpointsInformation(InvalidOperationException e)
        {
            string str = this.CreateContractListString();
            if (string.IsNullOrEmpty(str))
            {
                return new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelDispatcherUnableToOpen1", new object[] { this.listener.Uri }), e);
            }
            return new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelDispatcherUnableToOpen2", new object[] { this.listener.Uri, str }), e);
        }

        protected override void Detach(ServiceHostBase host)
        {
            if (host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }
            if (this.host != host)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelDispatcherDifferentHost0"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            this.ThrowIfDisposedOrImmutable();
            this.host = null;
        }

        internal bool HandleError(Exception error)
        {
            ErrorHandlerFaultInfo faultInfo = new ErrorHandlerFaultInfo();
            return this.HandleError(error, ref faultInfo);
        }

        internal bool HandleError(Exception error, ref ErrorHandlerFaultInfo faultInfo)
        {
            ErrorBehavior errorBehavior;
            lock (base.ThisLock)
            {
                if (this.errorBehavior != null)
                {
                    errorBehavior = this.errorBehavior;
                }
                else
                {
                    errorBehavior = new ErrorBehavior(this);
                }
            }
            return ((errorBehavior != null) && errorBehavior.HandleError(error, ref faultInfo));
        }

        internal bool HasApplicationEndpoints()
        {
            foreach (EndpointDispatcher dispatcher in this.Endpoints)
            {
                if (!dispatcher.IsSystemEndpoint)
                {
                    return true;
                }
            }
            return false;
        }

        private void Initialize(SharedRuntimeState shared)
        {
            this.shared = shared;
            this.endpointDispatchers = new EndpointDispatcherCollection(this);
            this.channelInitializers = this.NewBehaviorCollection<IChannelInitializer>();
            this.channels = new CommunicationObjectManager<IChannel>(base.ThisLock);
            this.pendingChannels = new SynchronizedChannelCollection<IChannel>(base.ThisLock);
            this.errorHandlers = new Collection<IErrorHandler>();
            this.isTransactedReceive = false;
            this.receiveSynchronously = false;
            this.serviceThrottle = null;
            this.transactionTimeout = TimeSpan.Zero;
            this.maxPendingReceives = 1;
            if (this.listener != null)
            {
                this.listener.Faulted += new EventHandler(this.OnListenerFaulted);
            }
        }

        internal void InitializeChannel(IClientChannel channel)
        {
            base.ThrowIfDisposedOrNotOpen();
            try
            {
                for (int i = 0; i < this.channelInitializers.Count; i++)
                {
                    this.channelInitializers[i].Initialize(channel);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
        }

        internal EndpointDispatcher Match(Message message, out bool addressMatched)
        {
            lock (base.ThisLock)
            {
                return this.filterTable.Lookup(message, out addressMatched);
            }
        }

        internal SynchronizedCollection<T> NewBehaviorCollection<T>()
        {
            return new ChannelDispatcherBehaviorCollection<T>(this);
        }

        protected override void OnAbort()
        {
            if (this.listener != null)
            {
                this.listener.Abort();
            }
            ListenerHandler listenerHandler = this.listenerHandler;
            if (listenerHandler != null)
            {
                listenerHandler.Abort();
            }
            this.AbortPendingChannels();
        }

        private void OnAddEndpoint(EndpointDispatcher endpoint)
        {
            lock (base.ThisLock)
            {
                endpoint.Attach(this);
                if (base.State == CommunicationState.Opened)
                {
                    if (this.addressTable != null)
                    {
                        this.addressTable.Add(endpoint.AddressFilter, endpoint.EndpointAddress, endpoint.FilterPriority);
                    }
                    this.filterTable.AddEndpoint(endpoint);
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<ICommunicationObject> collection = new List<ICommunicationObject>();
            if (this.listener != null)
            {
                collection.Add(this.listener);
            }
            ListenerHandler listenerHandler = this.listenerHandler;
            if (listenerHandler != null)
            {
                collection.Add(listenerHandler);
            }
            return new CloseCollectionAsyncResult(timeout, callback, state, collection);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfNotAttachedToHost();
            this.ThrowIfNoMessageVersion();
            if (this.listener != null)
            {
                try
                {
                    return this.listener.BeginOpen(timeout, callback, state);
                }
                catch (InvalidOperationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateOuterExceptionWithEndpointsInformation(exception));
                }
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.listener != null)
            {
                this.listener.Close(helper.RemainingTime());
            }
            ListenerHandler listenerHandler = this.listenerHandler;
            if (listenerHandler != null)
            {
                listenerHandler.Close(helper.RemainingTime());
            }
            this.AbortPendingChannels();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                for (int i = 0; i < this.endpointDispatchers.Count; i++)
                {
                    EndpointDispatcher endpoint = this.endpointDispatchers[i];
                    this.TraceEndpointLifetime(endpoint, 0x40007, System.ServiceModel.SR.GetString("TraceCodeEndpointListenerClose"));
                }
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            try
            {
                CloseCollectionAsyncResult.End(result);
            }
            finally
            {
                this.AbortPendingChannels();
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (this.listener != null)
            {
                try
                {
                    this.listener.EndOpen(result);
                    return;
                }
                catch (InvalidOperationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateOuterExceptionWithEndpointsInformation(exception));
                }
            }
            CompletedAsyncResult.End(result);
        }

        private void OnListenerFaulted(object sender, EventArgs e)
        {
            base.Fault();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.ThrowIfNotAttachedToHost();
            this.ThrowIfNoMessageVersion();
            if (this.listener != null)
            {
                try
                {
                    this.listener.Open(timeout);
                }
                catch (InvalidOperationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateOuterExceptionWithEndpointsInformation(exception));
                }
            }
        }

        protected override void OnOpened()
        {
            this.ThrowIfNotAttachedToHost();
            base.OnOpened();
            this.errorBehavior = new ErrorBehavior(this);
            this.filterTable = new System.ServiceModel.Dispatcher.EndpointDispatcherTable(base.ThisLock);
            for (int i = 0; i < this.endpointDispatchers.Count; i++)
            {
                EndpointDispatcher endpoint = this.endpointDispatchers[i];
                endpoint.DispatchRuntime.GetRuntime();
                endpoint.DispatchRuntime.LockDownProperties();
                this.filterTable.AddEndpoint(endpoint);
                if ((this.addressTable != null) && (endpoint.OriginalAddress != null))
                {
                    this.addressTable.Add(endpoint.AddressFilter, endpoint.OriginalAddress, endpoint.FilterPriority);
                }
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    this.TraceEndpointLifetime(endpoint, 0x40008, System.ServiceModel.SR.GetString("TraceCodeEndpointListenerOpen"));
                }
            }
            System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle = this.serviceThrottle;
            if (serviceThrottle == null)
            {
                serviceThrottle = this.host.ServiceThrottle;
            }
            IListenerBinder listenerBinder = ListenerBinder.GetBinder(this.listener, this.messageVersion);
            this.listenerHandler = new ListenerHandler(listenerBinder, this, this.host, serviceThrottle, this.timeouts);
            this.listenerHandler.Open();
        }

        protected override void OnOpening()
        {
            this.ThrowIfNotAttachedToHost();
            base.OnOpening();
        }

        private void OnRemoveEndpoint(EndpointDispatcher endpoint)
        {
            lock (base.ThisLock)
            {
                if (base.State == CommunicationState.Opened)
                {
                    this.filterTable.RemoveEndpoint(endpoint);
                    if (this.addressTable != null)
                    {
                        this.addressTable.Remove(endpoint.AddressFilter);
                    }
                }
                endpoint.Detach(this);
            }
        }

        internal void ProvideFault(Exception e, FaultConverter faultConverter, ref ErrorHandlerFaultInfo faultInfo)
        {
            ErrorBehavior errorBehavior;
            lock (base.ThisLock)
            {
                if (this.errorBehavior != null)
                {
                    errorBehavior = this.errorBehavior;
                }
                else
                {
                    errorBehavior = new ErrorBehavior(this);
                }
            }
            errorBehavior.ProvideFault(e, faultConverter, ref faultInfo);
        }

        internal void ReleasePerformanceCounters()
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                for (int i = 0; i < this.endpointDispatchers.Count; i++)
                {
                    if (this.endpointDispatchers[i] != null)
                    {
                        this.endpointDispatchers[i].ReleasePerformanceCounters();
                    }
                }
            }
        }

        internal void SetEndpointAddressTable(ThreadSafeMessageFilterTable<EndpointAddress> table)
        {
            if (table == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("table");
            }
            this.ThrowIfDisposedOrImmutable();
            this.addressTable = table;
        }

        internal void ThrowIfDisposedOrImmutable()
        {
            base.ThrowIfDisposedOrImmutable();
            this.shared.ThrowIfImmutable();
        }

        private void ThrowIfNoMessageVersion()
        {
            if (this.messageVersion == null)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelDispatcherNoMessageVersion"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        private void ThrowIfNotAttachedToHost()
        {
            if (this.host == null)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelDispatcherNoHost0"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        private void TraceEndpointLifetime(EndpointDispatcher endpoint, int traceCode, string traceDescription)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, object> dictionary2 = new Dictionary<string, object>(3);
                dictionary2.Add("ContractNamespace", endpoint.ContractNamespace);
                dictionary2.Add("ContractName", endpoint.ContractName);
                dictionary2.Add("Endpoint", endpoint.ListenUri);
                Dictionary<string, object> dictionary = dictionary2;
                TraceUtility.TraceEvent(TraceEventType.Information, traceCode, traceDescription, new DictionaryTraceRecord(dictionary), endpoint, null);
            }
        }

        public string BindingName
        {
            get
            {
                return this.bindingName;
            }
        }

        internal bool BufferedReceiveEnabled { get; set; }

        public SynchronizedCollection<IChannelInitializer> ChannelInitializers
        {
            get
            {
                return this.channelInitializers;
            }
        }

        internal CommunicationObjectManager<IChannel> Channels
        {
            get
            {
                return this.channels;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                if (this.timeouts != null)
                {
                    return this.timeouts.CloseTimeout;
                }
                return ServiceDefaults.CloseTimeout;
            }
        }

        internal IDefaultCommunicationTimeouts DefaultCommunicationTimeouts
        {
            get
            {
                return this.timeouts;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                if (this.timeouts != null)
                {
                    return this.timeouts.OpenTimeout;
                }
                return ServiceDefaults.OpenTimeout;
            }
        }

        internal bool EnableFaults
        {
            get
            {
                return this.shared.EnableFaults;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.shared.EnableFaults = value;
            }
        }

        internal System.ServiceModel.Dispatcher.EndpointDispatcherTable EndpointDispatcherTable
        {
            get
            {
                return this.filterTable;
            }
        }

        public SynchronizedCollection<EndpointDispatcher> Endpoints
        {
            get
            {
                return this.endpointDispatchers;
            }
        }

        public Collection<IErrorHandler> ErrorHandlers
        {
            get
            {
                return this.errorHandlers;
            }
        }

        public override ServiceHostBase Host
        {
            get
            {
                return this.host;
            }
        }

        public bool IncludeExceptionDetailInFaults
        {
            get
            {
                return this.includeExceptionDetailInFaults;
            }
            set
            {
                lock (base.ThisLock)
                {
                    this.ThrowIfDisposedOrImmutable();
                    this.includeExceptionDetailInFaults = value;
                }
            }
        }

        internal bool IsOnServer
        {
            get
            {
                return this.shared.IsOnServer;
            }
        }

        public bool IsTransactedAccept
        {
            get
            {
                return (this.isTransactedReceive && this.session);
            }
        }

        public bool IsTransactedReceive
        {
            get
            {
                return this.isTransactedReceive;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.isTransactedReceive = value;
            }
        }

        public override IChannelListener Listener
        {
            get
            {
                return this.listener;
            }
        }

        public bool ManualAddressing
        {
            get
            {
                return this.shared.ManualAddressing;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.shared.ManualAddressing = value;
            }
        }

        public int MaxPendingReceives
        {
            get
            {
                return this.maxPendingReceives;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.maxPendingReceives = value;
            }
        }

        public int MaxTransactedBatchSize
        {
            get
            {
                return this.maxTransactedBatchSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.ThrowIfDisposedOrImmutable();
                this.maxTransactedBatchSize = value;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
            set
            {
                this.messageVersion = value;
                this.ThrowIfDisposedOrImmutable();
            }
        }

        internal SynchronizedChannelCollection<IChannel> PendingChannels
        {
            get
            {
                return this.pendingChannels;
            }
        }

        public bool ReceiveContextEnabled
        {
            get
            {
                return this.receiveContextEnabled;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.receiveContextEnabled = value;
            }
        }

        public bool ReceiveSynchronously
        {
            get
            {
                return this.receiveSynchronously;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.receiveSynchronously = value;
            }
        }

        public bool SendAsynchronously
        {
            get
            {
                return this.sendAsynchronously;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.sendAsynchronously = value;
            }
        }

        public System.ServiceModel.Dispatcher.ServiceThrottle ServiceThrottle
        {
            get
            {
                return this.serviceThrottle;
            }
            set
            {
                this.ThrowIfDisposedOrImmutable();
                this.serviceThrottle = value;
            }
        }

        internal bool Session
        {
            get
            {
                return this.session;
            }
        }

        public IsolationLevel TransactionIsolationLevel
        {
            get
            {
                return this.transactionIsolationLevel;
            }
            set
            {
                switch (value)
                {
                    case IsolationLevel.Serializable:
                    case IsolationLevel.RepeatableRead:
                    case IsolationLevel.ReadCommitted:
                    case IsolationLevel.ReadUncommitted:
                    case IsolationLevel.Snapshot:
                    case IsolationLevel.Chaos:
                    case IsolationLevel.Unspecified:
                        this.ThrowIfDisposedOrImmutable();
                        this.transactionIsolationLevel = value;
                        this.transactionIsolationLevelSet = true;
                        return;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
            }
        }

        internal bool TransactionIsolationLevelSet
        {
            get
            {
                return this.transactionIsolationLevelSet;
            }
        }

        public TimeSpan TransactionTimeout
        {
            get
            {
                return this.transactionTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.ThrowIfDisposedOrImmutable();
                this.transactionTimeout = value;
            }
        }

        private class ChannelDispatcherBehaviorCollection<T> : SynchronizedCollection<T>
        {
            private ChannelDispatcher outer;

            internal ChannelDispatcherBehaviorCollection(ChannelDispatcher outer) : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.ThrowIfDisposedOrImmutable();
                base.ClearItems();
            }

            protected override void InsertItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                this.outer.ThrowIfDisposedOrImmutable();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.ThrowIfDisposedOrImmutable();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                this.outer.ThrowIfDisposedOrImmutable();
                base.SetItem(index, item);
            }
        }

        private class EndpointDispatcherCollection : SynchronizedCollection<EndpointDispatcher>
        {
            private ChannelDispatcher owner;

            internal EndpointDispatcherCollection(ChannelDispatcher owner) : base(owner.ThisLock)
            {
                this.owner = owner;
            }

            protected override void ClearItems()
            {
                foreach (EndpointDispatcher dispatcher in base.Items)
                {
                    this.owner.OnRemoveEndpoint(dispatcher);
                }
                base.ClearItems();
            }

            protected override void InsertItem(int index, EndpointDispatcher item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                this.owner.OnAddEndpoint(item);
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                EndpointDispatcher endpoint = base.Items[index];
                base.RemoveItem(index);
                this.owner.OnRemoveEndpoint(endpoint);
            }

            protected override void SetItem(int index, EndpointDispatcher item)
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCollectionDoesNotSupportSet0"));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }
    }
}

