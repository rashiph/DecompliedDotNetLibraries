namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    [ToolboxItem(false)]
    public class ManagementEventWatcher : Component
    {
        private uint cachedCount;
        private IWbemClassObjectFreeThreaded[] cachedObjects;
        private uint cacheIndex;
        private WmiDelegateInvoker delegateInvoker;
        private IEnumWbemClassObject enumWbem;
        private EventWatcherOptions options;
        private EventQuery query;
        private ManagementScope scope;
        private SinkForEventQuery sink;

        public event EventArrivedEventHandler EventArrived;

        public event StoppedEventHandler Stopped;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementEventWatcher() : this((ManagementScope) null, (EventQuery) null, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementEventWatcher(EventQuery query) : this(null, query, null)
        {
        }

        public ManagementEventWatcher(string query) : this(null, new EventQuery(query), null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementEventWatcher(ManagementScope scope, EventQuery query) : this(scope, query, null)
        {
        }

        public ManagementEventWatcher(string scope, string query) : this(new ManagementScope(scope), new EventQuery(query), null)
        {
        }

        public ManagementEventWatcher(ManagementScope scope, EventQuery query, EventWatcherOptions options)
        {
            if (scope != null)
            {
                this.scope = ManagementScope._Clone(scope, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
            }
            else
            {
                this.scope = ManagementScope._Clone(null, new IdentifierChangedEventHandler(this.HandleIdentifierChange));
            }
            if (query != null)
            {
                this.query = (EventQuery) query.Clone();
            }
            else
            {
                this.query = new EventQuery();
            }
            this.query.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
            if (options != null)
            {
                this.options = (EventWatcherOptions) options.Clone();
            }
            else
            {
                this.options = new EventWatcherOptions();
            }
            this.options.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
            this.enumWbem = null;
            this.cachedCount = 0;
            this.cacheIndex = 0;
            this.sink = null;
            this.delegateInvoker = new WmiDelegateInvoker(this);
        }

        public ManagementEventWatcher(string scope, string query, EventWatcherOptions options) : this(new ManagementScope(scope), new EventQuery(query), options)
        {
        }

        ~ManagementEventWatcher()
        {
            this.Stop();
            if (this.scope != null)
            {
                this.scope.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
            }
            if (this.options != null)
            {
                this.options.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
            }
            if (this.query != null)
            {
                this.query.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
            }
        }

        internal void FireEventArrived(EventArrivedEventArgs args)
        {
            try
            {
                this.delegateInvoker.FireEventToDelegates(this.EventArrived, args);
            }
            catch
            {
            }
        }

        internal void FireStopped(StoppedEventArgs args)
        {
            try
            {
                this.delegateInvoker.FireEventToDelegates(this.Stopped, args);
            }
            catch
            {
            }
        }

        private void HandleIdentifierChange(object sender, IdentifierChangedEventArgs e)
        {
            this.Stop();
        }

        private void Initialize()
        {
            if (this.query == null)
            {
                throw new InvalidOperationException();
            }
            if (this.options == null)
            {
                this.Options = new EventWatcherOptions();
            }
            lock (this)
            {
                if (this.scope == null)
                {
                    this.Scope = new ManagementScope();
                }
                if (this.cachedObjects == null)
                {
                    this.cachedObjects = new IWbemClassObjectFreeThreaded[this.options.BlockSize];
                }
            }
            lock (this.scope)
            {
                this.scope.Initialize();
            }
        }

        public void Start()
        {
            this.Initialize();
            this.Stop();
            SecurityHandler securityHandler = this.Scope.GetSecurityHandler();
            IWbemServices iWbemServices = this.scope.GetIWbemServices();
            try
            {
                this.sink = new SinkForEventQuery(this, this.options.Context, iWbemServices);
                if (this.sink.Status < 0)
                {
                    Marshal.ThrowExceptionForHR(this.sink.Status);
                }
                int errorCode = this.scope.GetSecuredIWbemServicesHandler(iWbemServices).ExecNotificationQueryAsync_(this.query.QueryLanguage, this.query.QueryString, 0, this.options.GetContext(), this.sink.Stub);
                if (errorCode < 0)
                {
                    if (this.sink != null)
                    {
                        this.sink.ReleaseStub();
                        this.sink = null;
                    }
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
            finally
            {
                securityHandler.Reset();
            }
        }

        public void Stop()
        {
            if (this.enumWbem != null)
            {
                Marshal.ReleaseComObject(this.enumWbem);
                this.enumWbem = null;
                this.FireStopped(new StoppedEventArgs(this.options.Context, 0x40006));
            }
            if (this.sink != null)
            {
                this.sink.Cancel();
                this.sink = null;
            }
        }

        public ManagementBaseObject WaitForNextEvent()
        {
            ManagementBaseObject obj2 = null;
            this.Initialize();
            lock (this)
            {
                SecurityHandler securityHandler = this.Scope.GetSecurityHandler();
                int errorCode = 0;
                try
                {
                    if (this.enumWbem == null)
                    {
                        errorCode = this.scope.GetSecuredIWbemServicesHandler(this.Scope.GetIWbemServices()).ExecNotificationQuery_(this.query.QueryLanguage, this.query.QueryString, this.options.Flags, this.options.GetContext(), ref this.enumWbem);
                    }
                    if (errorCode >= 0)
                    {
                        if ((this.cachedCount - this.cacheIndex) == 0)
                        {
                            IWbemClassObject_DoNotMarshal[] ppOutParams = new IWbemClassObject_DoNotMarshal[this.options.BlockSize];
                            int lTimeout = (ManagementOptions.InfiniteTimeout == this.options.Timeout) ? -1 : ((int) this.options.Timeout.TotalMilliseconds);
                            errorCode = this.scope.GetSecuredIEnumWbemClassObjectHandler(this.enumWbem).Next_(lTimeout, (uint) this.options.BlockSize, ppOutParams, ref this.cachedCount);
                            this.cacheIndex = 0;
                            if (errorCode >= 0)
                            {
                                if (this.cachedCount == 0)
                                {
                                    ManagementException.ThrowWithExtendedInfo(ManagementStatus.Timedout);
                                }
                                for (int i = 0; i < this.cachedCount; i++)
                                {
                                    this.cachedObjects[i] = new IWbemClassObjectFreeThreaded(Marshal.GetIUnknownForObject(ppOutParams[i]));
                                }
                            }
                        }
                        if (errorCode >= 0)
                        {
                            obj2 = new ManagementBaseObject(this.cachedObjects[this.cacheIndex]);
                            this.cacheIndex++;
                        }
                    }
                }
                finally
                {
                    securityHandler.Reset();
                }
                if (errorCode >= 0)
                {
                    return obj2;
                }
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return obj2;
                }
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return obj2;
        }

        public EventWatcherOptions Options
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.options;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                EventWatcherOptions options = this.options;
                this.options = (EventWatcherOptions) value.Clone();
                if (options != null)
                {
                    options.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.cachedObjects = new IWbemClassObjectFreeThreaded[this.options.BlockSize];
                this.options.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                this.HandleIdentifierChange(this, null);
            }
        }

        public EventQuery Query
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.query;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ManagementQuery query = this.query;
                this.query = (EventQuery) value.Clone();
                if (query != null)
                {
                    query.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.query.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                this.HandleIdentifierChange(this, null);
            }
        }

        public ManagementScope Scope
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.scope;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ManagementScope scope = this.scope;
                this.scope = value.Clone();
                if (scope != null)
                {
                    scope.IdentifierChanged -= new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                }
                this.scope.IdentifierChanged += new IdentifierChangedEventHandler(this.HandleIdentifierChange);
                this.HandleIdentifierChange(this, null);
            }
        }
    }
}

