namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public sealed class Timer : MarshalByRefObject, IDisposable
    {
        private const uint MAX_SUPPORTED_TIMEOUT = 0xfffffffe;
        private TimerBase timerBase;

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Timer(TimerCallback callback)
        {
            int num = -1;
            int num2 = -1;
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.TimerSetup(callback, this, (uint) num, (uint) num2, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Timer(TimerCallback callback, object state, int dueTime, int period)
        {
            if (dueTime < -1)
            {
                throw new ArgumentOutOfRangeException("dueTime", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (period < -1)
            {
                throw new ArgumentOutOfRangeException("period", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.TimerSetup(callback, state, (uint) dueTime, (uint) period, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Timer(TimerCallback callback, object state, long dueTime, long period)
        {
            if (dueTime < -1L)
            {
                throw new ArgumentOutOfRangeException("dueTime", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (period < -1L)
            {
                throw new ArgumentOutOfRangeException("period", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (dueTime > 0xfffffffeL)
            {
                throw new ArgumentOutOfRangeException("dueTime", Environment.GetResourceString("ArgumentOutOfRange_TimeoutTooLarge"));
            }
            if (period > 0xfffffffeL)
            {
                throw new ArgumentOutOfRangeException("period", Environment.GetResourceString("ArgumentOutOfRange_PeriodTooLarge"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.TimerSetup(callback, state, (uint) dueTime, (uint) period, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            long totalMilliseconds = (long) dueTime.TotalMilliseconds;
            if (totalMilliseconds < -1L)
            {
                throw new ArgumentOutOfRangeException("dueTm", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (totalMilliseconds > 0xfffffffeL)
            {
                throw new ArgumentOutOfRangeException("dueTm", Environment.GetResourceString("ArgumentOutOfRange_TimeoutTooLarge"));
            }
            long num2 = (long) period.TotalMilliseconds;
            if (num2 < -1L)
            {
                throw new ArgumentOutOfRangeException("periodTm", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (num2 > 0xfffffffeL)
            {
                throw new ArgumentOutOfRangeException("periodTm", Environment.GetResourceString("ArgumentOutOfRange_PeriodTooLarge"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.TimerSetup(callback, state, (uint) totalMilliseconds, (uint) num2, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, CLSCompliant(false)]
        public Timer(TimerCallback callback, object state, uint dueTime, uint period)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.TimerSetup(callback, state, dueTime, period, ref lookForMyCaller);
        }

        [SecuritySafeCritical]
        public bool Change(int dueTime, int period)
        {
            if (dueTime < -1)
            {
                throw new ArgumentOutOfRangeException("dueTime", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (period < -1)
            {
                throw new ArgumentOutOfRangeException("period", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return this.timerBase.ChangeTimer((uint) dueTime, (uint) period);
        }

        [SecuritySafeCritical]
        public bool Change(long dueTime, long period)
        {
            if (dueTime < -1L)
            {
                throw new ArgumentOutOfRangeException("dueTime", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (period < -1L)
            {
                throw new ArgumentOutOfRangeException("period", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (dueTime > 0xfffffffeL)
            {
                throw new ArgumentOutOfRangeException("dueTime", Environment.GetResourceString("ArgumentOutOfRange_TimeoutTooLarge"));
            }
            if (period > 0xfffffffeL)
            {
                throw new ArgumentOutOfRangeException("period", Environment.GetResourceString("ArgumentOutOfRange_PeriodTooLarge"));
            }
            return this.timerBase.ChangeTimer((uint) dueTime, (uint) period);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return this.Change((long) dueTime.TotalMilliseconds, (long) period.TotalMilliseconds);
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public bool Change(uint dueTime, uint period)
        {
            return this.timerBase.ChangeTimer(dueTime, period);
        }

        public void Dispose()
        {
            this.timerBase.Dispose();
        }

        [SecuritySafeCritical]
        public bool Dispose(WaitHandle notifyObject)
        {
            if (notifyObject == null)
            {
                throw new ArgumentNullException("notifyObject");
            }
            return this.timerBase.Dispose(notifyObject);
        }

        [SecurityCritical]
        private void TimerSetup(TimerCallback callback, object state, uint dueTime, uint period, ref StackCrawlMark stackMark)
        {
            this.timerBase = new TimerBase();
            this.timerBase.AddTimer(callback, state, dueTime, period, ref stackMark);
        }
    }
}

