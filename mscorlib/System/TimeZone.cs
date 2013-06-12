namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public abstract class TimeZone
    {
        private static TimeZone currentTimeZone = null;
        private static object s_InternalSyncObject;

        protected TimeZone()
        {
        }

        internal static TimeSpan CalculateUtcOffset(DateTime time, DaylightTime daylightTimes)
        {
            if (daylightTimes != null)
            {
                DateTime time4;
                DateTime time5;
                if (time.Kind == DateTimeKind.Utc)
                {
                    return TimeSpan.Zero;
                }
                DateTime time2 = daylightTimes.Start + daylightTimes.Delta;
                DateTime end = daylightTimes.End;
                if (daylightTimes.Delta.Ticks > 0L)
                {
                    time4 = end - daylightTimes.Delta;
                    time5 = end;
                }
                else
                {
                    time4 = time2;
                    time5 = time2 - daylightTimes.Delta;
                }
                bool flag = false;
                if (time2 > end)
                {
                    if ((time >= time2) || (time < end))
                    {
                        flag = true;
                    }
                }
                else if ((time >= time2) && (time < end))
                {
                    flag = true;
                }
                if ((flag && (time >= time4)) && (time < time5))
                {
                    flag = time.IsAmbiguousDaylightSavingTime();
                }
                if (flag)
                {
                    return daylightTimes.Delta;
                }
            }
            return TimeSpan.Zero;
        }

        public abstract DaylightTime GetDaylightChanges(int year);
        public abstract TimeSpan GetUtcOffset(DateTime time);
        public virtual bool IsDaylightSavingTime(DateTime time)
        {
            return IsDaylightSavingTime(time, this.GetDaylightChanges(time.Year));
        }

        public static bool IsDaylightSavingTime(DateTime time, DaylightTime daylightTimes)
        {
            return (CalculateUtcOffset(time, daylightTimes) != TimeSpan.Zero);
        }

        internal static void ResetTimeZone()
        {
            if (currentTimeZone != null)
            {
                lock (InternalSyncObject)
                {
                    currentTimeZone = null;
                }
            }
        }

        public virtual DateTime ToLocalTime(DateTime time)
        {
            if (time.Kind == DateTimeKind.Local)
            {
                return time;
            }
            bool isAmbiguousLocalDst = false;
            long utcOffsetFromUniversalTime = ((CurrentSystemTimeZone) CurrentTimeZone).GetUtcOffsetFromUniversalTime(time, ref isAmbiguousLocalDst);
            return new DateTime(time.Ticks + utcOffsetFromUniversalTime, DateTimeKind.Local, isAmbiguousLocalDst);
        }

        public virtual DateTime ToUniversalTime(DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc)
            {
                return time;
            }
            long ticks = time.Ticks - this.GetUtcOffset(time).Ticks;
            if (ticks > 0x2bca2875f4373fffL)
            {
                return new DateTime(0x2bca2875f4373fffL, DateTimeKind.Utc);
            }
            if (ticks < 0L)
            {
                return new DateTime(0L, DateTimeKind.Utc);
            }
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        public static TimeZone CurrentTimeZone
        {
            get
            {
                TimeZone currentTimeZone = TimeZone.currentTimeZone;
                if (currentTimeZone != null)
                {
                    return currentTimeZone;
                }
                lock (InternalSyncObject)
                {
                    if (TimeZone.currentTimeZone == null)
                    {
                        TimeZone.currentTimeZone = new CurrentSystemTimeZone();
                    }
                    return TimeZone.currentTimeZone;
                }
            }
        }

        public abstract string DaylightName { get; }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        public abstract string StandardName { get; }
    }
}

