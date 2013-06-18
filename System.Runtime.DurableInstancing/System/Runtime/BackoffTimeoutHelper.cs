namespace System.Runtime
{
    using System;
    using System.Threading;

    internal sealed class BackoffTimeoutHelper
    {
        private Action<object> backoffCallback;
        private object backoffState;
        private IOThreadTimer backoffTimer;
        private DateTime deadline;
        private static readonly TimeSpan defaultInitialWaitTime = TimeSpan.FromMilliseconds(1.0);
        private static readonly TimeSpan defaultMaxWaitTime = TimeSpan.FromMinutes(1.0);
        private static readonly long maxDriftTicks = (IOThreadTimer.SystemTimeResolutionTicks * 2L);
        private static readonly int maxSkewMilliseconds = ((int) (IOThreadTimer.SystemTimeResolutionTicks / 0x2710L));
        private TimeSpan maxWaitTime;
        private TimeSpan originalTimeout;
        private Random random;
        private TimeSpan waitTime;

        internal BackoffTimeoutHelper(TimeSpan timeout) : this(timeout, defaultMaxWaitTime)
        {
        }

        internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime) : this(timeout, maxWaitTime, defaultInitialWaitTime)
        {
        }

        internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime, TimeSpan initialWaitTime)
        {
            this.random = new Random(this.GetHashCode());
            this.maxWaitTime = maxWaitTime;
            this.originalTimeout = timeout;
            this.Reset(timeout, initialWaitTime);
        }

        private void Backoff()
        {
            if (this.waitTime.Ticks >= (this.maxWaitTime.Ticks / 2L))
            {
                this.waitTime = this.maxWaitTime;
            }
            else
            {
                this.waitTime = TimeSpan.FromTicks(this.waitTime.Ticks * 2L);
            }
            if (this.deadline != DateTime.MaxValue)
            {
                TimeSpan span = (TimeSpan) (this.deadline - DateTime.UtcNow);
                if (this.waitTime > span)
                {
                    this.waitTime = span;
                    if (this.waitTime < TimeSpan.Zero)
                    {
                        this.waitTime = TimeSpan.Zero;
                    }
                }
            }
        }

        public bool IsExpired()
        {
            if (this.deadline == DateTime.MaxValue)
            {
                return false;
            }
            return (DateTime.UtcNow >= this.deadline);
        }

        private void Reset(TimeSpan timeout, TimeSpan initialWaitTime)
        {
            if (timeout == TimeSpan.MaxValue)
            {
                this.deadline = DateTime.MaxValue;
            }
            else
            {
                this.deadline = DateTime.UtcNow + timeout;
            }
            this.waitTime = initialWaitTime;
        }

        public void WaitAndBackoff()
        {
            Thread.Sleep(this.WaitTimeWithDrift());
            this.Backoff();
        }

        public void WaitAndBackoff(Action<object> callback, object state)
        {
            if ((this.backoffCallback != callback) || (this.backoffState != state))
            {
                if (this.backoffTimer != null)
                {
                    this.backoffTimer.Cancel();
                }
                this.backoffCallback = callback;
                this.backoffState = state;
                this.backoffTimer = new IOThreadTimer(callback, state, false, maxSkewMilliseconds);
            }
            TimeSpan timeFromNow = this.WaitTimeWithDrift();
            this.Backoff();
            this.backoffTimer.Set(timeFromNow);
        }

        private TimeSpan WaitTimeWithDrift()
        {
            return Ticks.ToTimeSpan(Math.Max(Ticks.FromTimeSpan(defaultInitialWaitTime), Ticks.Add(Ticks.FromTimeSpan(this.waitTime), ((long) (((ulong) this.random.Next()) % ((2L * maxDriftTicks) + 1L))) - maxDriftTicks)));
        }

        public TimeSpan OriginalTimeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.originalTimeout;
            }
        }
    }
}

