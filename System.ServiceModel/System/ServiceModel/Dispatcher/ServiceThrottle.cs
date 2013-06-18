namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    public sealed class ServiceThrottle
    {
        private FlowThrottle calls;
        internal const int DefaultMaxConcurrentCalls = 0x10;
        internal static int DefaultMaxConcurrentCallsCpuCount = (0x10 * OSEnvironmentHelper.ProcessorCount);
        internal const int DefaultMaxConcurrentSessions = 100;
        internal static int DefaultMaxConcurrentSessionsCpuCount = (100 * OSEnvironmentHelper.ProcessorCount);
        private QuotaThrottle dynamic;
        private ServiceHostBase host;
        private FlowThrottle instanceContexts;
        private bool isActive;
        private const string MaxConcurrentCallsConfigName = "maxConcurrentCalls";
        private const string MaxConcurrentCallsPropertyName = "MaxConcurrentCalls";
        private const string MaxConcurrentInstancesConfigName = "maxConcurrentInstances";
        private const string MaxConcurrentInstancesPropertyName = "MaxConcurrentInstances";
        private const string MaxConcurrentSessionsConfigName = "maxConcurrentSessions";
        private const string MaxConcurrentSessionsPropertyName = "MaxConcurrentSessions";
        private ServicePerformanceCountersBase servicePerformanceCounters;
        private FlowThrottle sessions;
        private object thisLock = new object();

        internal ServiceThrottle(ServiceHostBase host)
        {
            if (host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }
            this.host = host;
            this.MaxConcurrentCalls = DefaultMaxConcurrentCallsCpuCount;
            this.MaxConcurrentSessions = DefaultMaxConcurrentSessionsCpuCount;
            this.isActive = true;
        }

        internal bool AcquireCall(ChannelHandler channel)
        {
            lock (this.ThisLock)
            {
                return this.PrivateAcquireCall(channel);
            }
        }

        internal void AcquiredCallsToken()
        {
            this.servicePerformanceCounters.IncrementThrottlePercent(0x21);
        }

        internal void AcquiredInstancesToken()
        {
            this.servicePerformanceCounters.IncrementThrottlePercent(0x23);
        }

        internal void AcquiredSessionsToken()
        {
            this.servicePerformanceCounters.IncrementThrottlePercent(0x25);
        }

        internal bool AcquireInstanceContextAndDynamic(ChannelHandler channel, bool acquireInstanceContextThrottle)
        {
            lock (this.ThisLock)
            {
                if (!acquireInstanceContextThrottle)
                {
                    return this.PrivateAcquireDynamic(channel);
                }
                return (this.PrivateAcquireInstanceContext(channel) && this.PrivateAcquireDynamic(channel));
            }
        }

        internal bool AcquireSession(ISessionThrottleNotification source)
        {
            lock (this.ThisLock)
            {
                return this.PrivateAcquireSession(source);
            }
        }

        internal bool AcquireSession(ListenerHandler listener)
        {
            lock (this.ThisLock)
            {
                return this.PrivateAcquireSessionListenerHandler(listener);
            }
        }

        internal void DeactivateCall()
        {
            if (this.isActive && (this.calls != null))
            {
                this.calls.Release();
            }
        }

        internal void DeactivateChannel()
        {
            if (this.isActive && (this.sessions != null))
            {
                this.sessions.Release();
            }
        }

        internal void DeactivateInstanceContext()
        {
            if (this.isActive && (this.instanceContexts != null))
            {
                this.instanceContexts.Release();
            }
        }

        private void GotCall(object state)
        {
            ChannelHandler handler = (ChannelHandler) state;
            lock (this.ThisLock)
            {
                handler.ThrottleAcquiredForCall();
            }
        }

        private void GotDynamic(object state)
        {
            ((ChannelHandler) state).ThrottleAcquired();
        }

        private void GotInstanceContext(object state)
        {
            ChannelHandler channel = (ChannelHandler) state;
            lock (this.ThisLock)
            {
                if (this.PrivateAcquireDynamic(channel))
                {
                    channel.ThrottleAcquired();
                }
            }
        }

        private void GotSession(object state)
        {
            ((ISessionThrottleNotification) state).ThrottleAcquired();
        }

        internal int IncrementManualFlowControlLimit(int incrementBy)
        {
            return this.Dynamic.IncrementLimit(incrementBy);
        }

        private void InitializeCallsPerfCounterSettings()
        {
            this.calls.SetAcquired(new Action(this.AcquiredCallsToken));
            this.calls.SetReleased(new Action(this.ReleasedCallsToken));
            this.servicePerformanceCounters.SetThrottleBase(0x22, (long) this.calls.Capacity);
        }

        private void InitializeInstancePerfCounterSettings()
        {
            this.instanceContexts.SetAcquired(new Action(this.AcquiredInstancesToken));
            this.instanceContexts.SetReleased(new Action(this.ReleasedInstancesToken));
            this.servicePerformanceCounters.SetThrottleBase(0x24, (long) this.instanceContexts.Capacity);
        }

        private void InitializeSessionsPerfCounterSettings()
        {
            this.sessions.SetAcquired(new Action(this.AcquiredSessionsToken));
            this.sessions.SetReleased(new Action(this.ReleasedSessionsToken));
            this.servicePerformanceCounters.SetThrottleBase(0x26, (long) this.sessions.Capacity);
        }

        private bool PrivateAcquireCall(ChannelHandler channel)
        {
            if (this.calls != null)
            {
                return this.calls.Acquire(channel);
            }
            return true;
        }

        private bool PrivateAcquireDynamic(ChannelHandler channel)
        {
            if (this.dynamic != null)
            {
                return this.dynamic.Acquire(channel);
            }
            return true;
        }

        private bool PrivateAcquireInstanceContext(ChannelHandler channel)
        {
            if ((this.instanceContexts != null) && (channel.InstanceContext == null))
            {
                channel.InstanceContextServiceThrottle = this;
                return this.instanceContexts.Acquire(channel);
            }
            return true;
        }

        private bool PrivateAcquireSession(ISessionThrottleNotification source)
        {
            if (this.sessions != null)
            {
                return this.sessions.Acquire(source);
            }
            return true;
        }

        private bool PrivateAcquireSessionListenerHandler(ListenerHandler listener)
        {
            if (((this.sessions != null) && (listener.Channel != null)) && (listener.Channel.Throttle == null))
            {
                listener.Channel.Throttle = this;
                return this.sessions.Acquire(listener);
            }
            return true;
        }

        internal void ReleasedCallsToken()
        {
            this.servicePerformanceCounters.DecrementThrottlePercent(0x21);
        }

        internal void ReleasedInstancesToken()
        {
            this.servicePerformanceCounters.DecrementThrottlePercent(0x23);
        }

        internal void ReleasedSessionsToken()
        {
            this.servicePerformanceCounters.DecrementThrottlePercent(0x25);
        }

        internal void SetServicePerformanceCounters(ServicePerformanceCountersBase counters)
        {
            this.servicePerformanceCounters = counters;
            if (this.instanceContexts != null)
            {
                this.InitializeInstancePerfCounterSettings();
            }
            this.InitializeCallsPerfCounterSettings();
            this.InitializeSessionsPerfCounterSettings();
        }

        private void ThrowIfClosedOrOpened(string memberName)
        {
            if (this.host.State == CommunicationState.Opened)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxImmutableThrottle1", new object[] { memberName })));
            }
            this.host.ThrowIfClosedOrOpened();
        }

        private void UpdateIsActive()
        {
            this.isActive = (((this.dynamic != null) || ((this.calls != null) && (this.calls.Capacity != 0x7fffffff))) || ((this.sessions != null) && (this.sessions.Capacity != 0x7fffffff))) || ((this.instanceContexts != null) && (this.instanceContexts.Capacity != 0x7fffffff));
        }

        private FlowThrottle Calls
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.calls == null)
                    {
                        this.calls = new FlowThrottle(new WaitCallback(this.GotCall), DefaultMaxConcurrentCallsCpuCount, "MaxConcurrentCalls", "maxConcurrentCalls");
                    }
                    return this.calls;
                }
            }
        }

        private QuotaThrottle Dynamic
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.dynamic == null)
                    {
                        this.dynamic = new QuotaThrottle(new WaitCallback(this.GotDynamic), new object());
                        this.dynamic.Owner = "ServiceHost";
                    }
                    this.UpdateIsActive();
                    return this.dynamic;
                }
            }
        }

        private FlowThrottle InstanceContexts
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.instanceContexts == null)
                    {
                        this.instanceContexts = new FlowThrottle(new WaitCallback(this.GotInstanceContext), 0x7fffffff, "MaxConcurrentInstances", "maxConcurrentInstances");
                        if (this.servicePerformanceCounters != null)
                        {
                            this.InitializeInstancePerfCounterSettings();
                        }
                    }
                    return this.instanceContexts;
                }
            }
        }

        internal bool IsActive
        {
            get
            {
                return this.isActive;
            }
        }

        internal int ManualFlowControlLimit
        {
            get
            {
                return this.Dynamic.Limit;
            }
            set
            {
                this.Dynamic.SetLimit(value);
            }
        }

        public int MaxConcurrentCalls
        {
            get
            {
                return this.Calls.Capacity;
            }
            set
            {
                this.ThrowIfClosedOrOpened("MaxConcurrentCalls");
                this.Calls.Capacity = value;
                this.UpdateIsActive();
                if (this.servicePerformanceCounters != null)
                {
                    this.servicePerformanceCounters.SetThrottleBase(0x22, (long) this.Calls.Capacity);
                }
            }
        }

        public int MaxConcurrentInstances
        {
            get
            {
                return this.InstanceContexts.Capacity;
            }
            set
            {
                this.ThrowIfClosedOrOpened("MaxConcurrentInstances");
                this.InstanceContexts.Capacity = value;
                this.UpdateIsActive();
                if (this.servicePerformanceCounters != null)
                {
                    this.servicePerformanceCounters.SetThrottleBase(0x24, (long) this.InstanceContexts.Capacity);
                }
            }
        }

        public int MaxConcurrentSessions
        {
            get
            {
                return this.Sessions.Capacity;
            }
            set
            {
                this.ThrowIfClosedOrOpened("MaxConcurrentSessions");
                this.Sessions.Capacity = value;
                this.UpdateIsActive();
                if (this.servicePerformanceCounters != null)
                {
                    this.servicePerformanceCounters.SetThrottleBase(0x26, (long) this.Sessions.Capacity);
                }
            }
        }

        private FlowThrottle Sessions
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.sessions == null)
                    {
                        this.sessions = new FlowThrottle(new WaitCallback(this.GotSession), DefaultMaxConcurrentSessionsCpuCount, "MaxConcurrentSessions", "maxConcurrentSessions");
                    }
                    return this.sessions;
                }
            }
        }

        internal object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

