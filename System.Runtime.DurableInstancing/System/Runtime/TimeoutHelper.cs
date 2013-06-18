namespace System.Runtime
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TimeoutHelper
    {
        private DateTime deadline;
        private bool deadlineSet;
        private TimeSpan originalTimeout;
        public static readonly TimeSpan MaxWait;
        public TimeoutHelper(TimeSpan timeout)
        {
            this.originalTimeout = timeout;
            this.deadline = DateTime.MaxValue;
            this.deadlineSet = timeout == TimeSpan.MaxValue;
        }

        public TimeSpan OriginalTimeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.originalTimeout;
            }
        }
        public static bool IsTooLarge(TimeSpan timeout)
        {
            return ((timeout > MaxWait) && (timeout != TimeSpan.MaxValue));
        }

        public static TimeSpan FromMilliseconds(int milliseconds)
        {
            if (milliseconds == -1)
            {
                return TimeSpan.MaxValue;
            }
            return TimeSpan.FromMilliseconds((double) milliseconds);
        }

        public static int ToMilliseconds(TimeSpan timeout)
        {
            if (timeout == TimeSpan.MaxValue)
            {
                return -1;
            }
            long ticks = Ticks.FromTimeSpan(timeout);
            if ((ticks / 0x2710L) > 0x7fffffffL)
            {
                return 0x7fffffff;
            }
            return Ticks.ToMilliseconds(ticks);
        }

        public static TimeSpan Min(TimeSpan val1, TimeSpan val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        public static TimeSpan Add(TimeSpan timeout1, TimeSpan timeout2)
        {
            return Ticks.ToTimeSpan(Ticks.Add(Ticks.FromTimeSpan(timeout1), Ticks.FromTimeSpan(timeout2)));
        }

        public static DateTime Add(DateTime time, TimeSpan timeout)
        {
            if ((timeout >= TimeSpan.Zero) && ((DateTime.MaxValue - time) <= timeout))
            {
                return DateTime.MaxValue;
            }
            if ((timeout <= TimeSpan.Zero) && ((DateTime.MinValue - time) >= timeout))
            {
                return DateTime.MinValue;
            }
            return (time + timeout);
        }

        public static DateTime Subtract(DateTime time, TimeSpan timeout)
        {
            return Add(time, TimeSpan.Zero - timeout);
        }

        public static TimeSpan Divide(TimeSpan timeout, int factor)
        {
            if (timeout == TimeSpan.MaxValue)
            {
                return TimeSpan.MaxValue;
            }
            return Ticks.ToTimeSpan((Ticks.FromTimeSpan(timeout) / ((long) factor)) + 1L);
        }

        public TimeSpan RemainingTime()
        {
            if (!this.deadlineSet)
            {
                this.SetDeadline();
                return this.originalTimeout;
            }
            if (this.deadline == DateTime.MaxValue)
            {
                return TimeSpan.MaxValue;
            }
            TimeSpan span = (TimeSpan) (this.deadline - DateTime.UtcNow);
            if (span <= TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }
            return span;
        }

        public TimeSpan ElapsedTime()
        {
            return (this.originalTimeout - this.RemainingTime());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowIfNegativeArgument(TimeSpan timeout)
        {
            ThrowIfNegativeArgument(timeout, "timeout");
        }

        public static void ThrowIfNegativeArgument(TimeSpan timeout, string argumentName)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw Fx.Exception.ArgumentOutOfRange(argumentName, timeout, SRCore.TimeoutMustBeNonNegative(argumentName, timeout));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void ThrowIfNonPositiveArgument(TimeSpan timeout)
        {
            ThrowIfNonPositiveArgument(timeout, "timeout");
        }

        public static void ThrowIfNonPositiveArgument(TimeSpan timeout, string argumentName)
        {
            if (timeout <= TimeSpan.Zero)
            {
                throw Fx.Exception.ArgumentOutOfRange(argumentName, timeout, SRCore.TimeoutMustBePositive(argumentName, timeout));
            }
        }

        public static bool WaitOne(WaitHandle waitHandle, TimeSpan timeout)
        {
            ThrowIfNegativeArgument(timeout);
            if (timeout == TimeSpan.MaxValue)
            {
                waitHandle.WaitOne();
                return true;
            }
            return waitHandle.WaitOne(timeout, false);
        }

        private void SetDeadline()
        {
            this.deadline = DateTime.UtcNow + this.originalTimeout;
            this.deadlineSet = true;
        }

        static TimeoutHelper()
        {
            MaxWait = TimeSpan.FromMilliseconds(2147483647.0);
        }
    }
}

