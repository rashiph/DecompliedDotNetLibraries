namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Sequential), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public struct SpinWait
    {
        internal const int YIELD_THRESHOLD = 10;
        internal const int SLEEP_0_EVERY_HOW_MANY_TIMES = 5;
        internal const int SLEEP_1_EVERY_HOW_MANY_TIMES = 20;
        private int m_count;
        public int Count
        {
            get
            {
                return this.m_count;
            }
        }
        public bool NextSpinWillYield
        {
            get
            {
                if (this.m_count <= 10)
                {
                    return PlatformHelper.IsSingleProcessor;
                }
                return true;
            }
        }
        public void SpinOnce()
        {
            if (this.NextSpinWillYield)
            {
                CdsSyncEtwBCLProvider.Log.SpinWait_NextSpinWillYield();
                int num = (this.m_count >= 10) ? (this.m_count - 10) : this.m_count;
                if ((num % 20) == 0x13)
                {
                    Thread.Sleep(1);
                }
                else if ((num % 5) == 4)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Yield();
                }
            }
            else
            {
                Thread.SpinWait(((int) 4) << this.m_count);
            }
            this.m_count = (this.m_count == 0x7fffffff) ? 10 : (this.m_count + 1);
        }

        public void Reset()
        {
            this.m_count = 0;
        }

        public static void SpinUntil(Func<bool> condition)
        {
            SpinUntil(condition, -1);
        }

        public static bool SpinUntil(Func<bool> condition, TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, Environment.GetResourceString("SpinWait_SpinUntil_TimeoutWrong"));
            }
            return SpinUntil(condition, (int) timeout.TotalMilliseconds);
        }

        public static bool SpinUntil(Func<bool> condition, int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, Environment.GetResourceString("SpinWait_SpinUntil_TimeoutWrong"));
            }
            if (condition == null)
            {
                throw new ArgumentNullException("condition", Environment.GetResourceString("SpinWait_SpinUntil_ArgumentNull"));
            }
            long ticks = 0L;
            if ((millisecondsTimeout != 0) && (millisecondsTimeout != -1))
            {
                ticks = DateTime.UtcNow.Ticks;
            }
            SpinWait wait = new SpinWait();
            while (!condition())
            {
                if (millisecondsTimeout == 0)
                {
                    return false;
                }
                wait.SpinOnce();
                if (((millisecondsTimeout != -1) && wait.NextSpinWillYield) && (millisecondsTimeout <= ((DateTime.UtcNow.Ticks - ticks) / 0x2710L)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

