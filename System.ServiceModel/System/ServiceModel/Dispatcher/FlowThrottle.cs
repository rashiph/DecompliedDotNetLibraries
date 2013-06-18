namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;

    internal sealed class FlowThrottle
    {
        private Action acquired;
        private int capacity;
        private string configName;
        private int count;
        private object mutex;
        private string propertyName;
        private WaitCallback release;
        private Action released;
        private Queue<object> waiters;
        private bool warningIssued;
        private int warningRestoreLimit;

        internal FlowThrottle(WaitCallback release, int capacity, string propertyName, string configName)
        {
            if (capacity <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxThrottleLimitMustBeGreaterThanZero0")));
            }
            this.count = 0;
            this.capacity = capacity;
            this.mutex = new object();
            this.release = release;
            this.waiters = new Queue<object>();
            this.propertyName = propertyName;
            this.configName = configName;
            this.warningRestoreLimit = (int) Math.Floor((double) (0.7 * capacity));
        }

        internal bool Acquire(object o)
        {
            bool flag = true;
            lock (this.mutex)
            {
                if (this.count < this.capacity)
                {
                    this.count++;
                }
                else
                {
                    if (this.waiters.Count == 0)
                    {
                        if (TD.MessageThrottleExceededIsEnabled() && !this.warningIssued)
                        {
                            TD.MessageThrottleExceeded(this.propertyName, (long) this.capacity);
                            this.warningIssued = true;
                        }
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            string str;
                            if (this.propertyName != null)
                            {
                                str = System.ServiceModel.SR.GetString("TraceCodeServiceThrottleLimitReached", new object[] { this.propertyName, this.capacity, this.configName });
                            }
                            else
                            {
                                str = System.ServiceModel.SR.GetString("TraceCodeServiceThrottleLimitReachedInternal", new object[] { this.capacity });
                            }
                            TraceUtility.TraceEvent(TraceEventType.Warning, 0x80031, str);
                        }
                    }
                    this.waiters.Enqueue(o);
                    flag = false;
                }
                if (this.acquired != null)
                {
                    this.acquired();
                }
                return flag;
            }
        }

        internal void Release()
        {
            object state = null;
            lock (this.mutex)
            {
                if (this.waiters.Count > 0)
                {
                    state = this.waiters.Dequeue();
                    if (this.waiters.Count == 0)
                    {
                        this.waiters.TrimExcess();
                    }
                }
                else
                {
                    this.count--;
                    if (this.count < this.warningRestoreLimit)
                    {
                        if (TD.MessageThrottleAtSeventyPercentIsEnabled() && this.warningIssued)
                        {
                            TD.MessageThrottleAtSeventyPercent(this.propertyName, (long) this.capacity);
                        }
                        this.warningIssued = false;
                    }
                }
            }
            if (state != null)
            {
                this.release(state);
            }
            if (this.released != null)
            {
                this.released();
            }
        }

        internal void SetAcquired(Action action)
        {
            this.acquired = action;
        }

        internal void SetReleased(Action action)
        {
            this.released = action;
        }

        internal int Capacity
        {
            get
            {
                return this.capacity;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxThrottleLimitMustBeGreaterThanZero0")));
                }
                this.capacity = value;
            }
        }
    }
}

