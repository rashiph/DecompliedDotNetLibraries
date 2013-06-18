namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal sealed class QuotaThrottle
    {
        private bool didTraceThrottleLimit;
        private int limit = 0x7fffffff;
        private object mutex;
        private string owner;
        private string propertyName = "ManualFlowControlLimit";
        private WaitCallback release;
        private Queue<object> waiters;

        internal QuotaThrottle(WaitCallback release, object mutex)
        {
            this.mutex = mutex;
            this.release = release;
            this.waiters = new Queue<object>();
        }

        internal bool Acquire(object o)
        {
            lock (this.mutex)
            {
                if (this.IsEnabled)
                {
                    if (this.limit > 0)
                    {
                        this.limit--;
                        if (((this.limit == 0) && DiagnosticUtility.ShouldTraceWarning) && !this.didTraceThrottleLimit)
                        {
                            this.didTraceThrottleLimit = true;
                            TraceUtility.TraceEvent(TraceEventType.Warning, 0x80028, System.ServiceModel.SR.GetString("TraceCodeManualFlowThrottleLimitReached", new object[] { this.propertyName, this.owner }));
                        }
                        return true;
                    }
                    this.waiters.Enqueue(o);
                    return false;
                }
                return true;
            }
        }

        internal int IncrementLimit(int incrementBy)
        {
            int limit;
            if (incrementBy < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("incrementBy", incrementBy, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            object[] released = null;
            lock (this.mutex)
            {
                if (this.IsEnabled)
                {
                    this.limit += incrementBy;
                    released = this.LimitChanged();
                }
                limit = this.limit;
            }
            if (released != null)
            {
                this.Release(released);
            }
            return limit;
        }

        private object[] LimitChanged()
        {
            object[] objArray = null;
            if (this.IsEnabled)
            {
                if ((this.waiters.Count > 0) && (this.limit > 0))
                {
                    if (this.limit < this.waiters.Count)
                    {
                        objArray = new object[this.limit];
                        for (int i = 0; i < this.limit; i++)
                        {
                            objArray[i] = this.waiters.Dequeue();
                        }
                        this.limit = 0;
                    }
                    else
                    {
                        objArray = this.waiters.ToArray();
                        this.waiters.Clear();
                        this.waiters.TrimExcess();
                        this.limit -= objArray.Length;
                    }
                }
                this.didTraceThrottleLimit = false;
                return objArray;
            }
            objArray = this.waiters.ToArray();
            this.waiters.Clear();
            this.waiters.TrimExcess();
            return objArray;
        }

        internal void Release(object[] released)
        {
            for (int i = 0; i < released.Length; i++)
            {
                ActionItem.Schedule(new Action<object>(this.ReleaseAsync), released[i]);
            }
        }

        private void ReleaseAsync(object state)
        {
            this.release(state);
        }

        internal void SetLimit(int messageLimit)
        {
            if (messageLimit < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageLimit", messageLimit, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            object[] released = null;
            lock (this.mutex)
            {
                this.limit = messageLimit;
                released = this.LimitChanged();
            }
            if (released != null)
            {
                this.Release(released);
            }
        }

        private bool IsEnabled
        {
            get
            {
                return (this.limit != 0x7fffffff);
            }
        }

        internal int Limit
        {
            get
            {
                return this.limit;
            }
        }

        internal string Owner
        {
            set
            {
                this.owner = value;
            }
        }
    }
}

