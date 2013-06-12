namespace System
{
    using System.Collections;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class CurrentSystemTimeZone : TimeZone
    {
        private Hashtable m_CachedDaylightChanges = new Hashtable();
        private string m_daylightName = null;
        private string m_standardName = null;
        private long m_ticksOffset = (nativeGetTimeZoneMinuteOffset() * 0x23c34600L);
        private static object s_InternalSyncObject;
        private const long TicksPerMillisecond = 0x2710L;
        private const long TicksPerMinute = 0x23c34600L;
        private const long TicksPerSecond = 0x989680L;

        [SecuritySafeCritical]
        internal CurrentSystemTimeZone()
        {
        }

        [SecuritySafeCritical]
        public override DaylightTime GetDaylightChanges(int year)
        {
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 0x270f }));
            }
            object key = year;
            if (!this.m_CachedDaylightChanges.Contains(key))
            {
                lock (InternalSyncObject)
                {
                    if (!this.m_CachedDaylightChanges.Contains(key))
                    {
                        short[] numArray = nativeGetDaylightChanges(year);
                        if (numArray == null)
                        {
                            this.m_CachedDaylightChanges.Add(key, new DaylightTime(DateTime.MinValue, DateTime.MinValue, TimeSpan.Zero));
                        }
                        else
                        {
                            DateTime start = GetDayOfWeek(year, numArray[0] != 0, numArray[1], numArray[2], numArray[3], numArray[4], numArray[5], numArray[6], numArray[7]);
                            DateTime end = GetDayOfWeek(year, numArray[8] != 0, numArray[9], numArray[10], numArray[11], numArray[12], numArray[13], numArray[14], numArray[15]);
                            TimeSpan delta = new TimeSpan(numArray[0x10] * 0x23c34600L);
                            DaylightTime time3 = new DaylightTime(start, end, delta);
                            this.m_CachedDaylightChanges.Add(key, time3);
                        }
                    }
                }
            }
            return (DaylightTime) this.m_CachedDaylightChanges[key];
        }

        private static DateTime GetDayOfWeek(int year, bool fixedDate, int month, int targetDayOfWeek, int numberOfSunday, int hour, int minute, int second, int millisecond)
        {
            DateTime time;
            if (fixedDate)
            {
                int num = DateTime.DaysInMonth(year, month);
                return new DateTime(year, month, (num < numberOfSunday) ? num : numberOfSunday, hour, minute, second, millisecond, DateTimeKind.Local);
            }
            if (numberOfSunday <= 4)
            {
                time = new DateTime(year, month, 1, hour, minute, second, millisecond, DateTimeKind.Local);
                int dayOfWeek = (int) time.DayOfWeek;
                int num3 = targetDayOfWeek - dayOfWeek;
                if (num3 < 0)
                {
                    num3 += 7;
                }
                num3 += 7 * (numberOfSunday - 1);
                if (num3 > 0)
                {
                    time = time.AddDays((double) num3);
                }
                return time;
            }
            Calendar defaultInstance = GregorianCalendar.GetDefaultInstance();
            time = new DateTime(year, month, defaultInstance.GetDaysInMonth(year, month), hour, minute, second, millisecond, DateTimeKind.Local);
            int num5 = ((int) time.DayOfWeek) - targetDayOfWeek;
            if (num5 < 0)
            {
                num5 += 7;
            }
            if (num5 > 0)
            {
                time = time.AddDays((double) -num5);
            }
            return time;
        }

        public override TimeSpan GetUtcOffset(DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc)
            {
                return TimeSpan.Zero;
            }
            return new TimeSpan(TimeZone.CalculateUtcOffset(time, this.GetDaylightChanges(time.Year)).Ticks + this.m_ticksOffset);
        }

        internal long GetUtcOffsetFromUniversalTime(DateTime time, ref bool isAmbiguousLocalDst)
        {
            TimeSpan span = new TimeSpan(this.m_ticksOffset);
            DaylightTime daylightChanges = this.GetDaylightChanges(time.Year);
            isAmbiguousLocalDst = false;
            if ((daylightChanges != null) && (daylightChanges.Delta.Ticks != 0L))
            {
                DateTime time5;
                DateTime time6;
                DateTime time3 = daylightChanges.Start - span;
                DateTime time4 = (daylightChanges.End - span) - daylightChanges.Delta;
                if (daylightChanges.Delta.Ticks > 0L)
                {
                    time5 = time4 - daylightChanges.Delta;
                    time6 = time4;
                }
                else
                {
                    time5 = time3;
                    time6 = time3 - daylightChanges.Delta;
                }
                bool flag = false;
                if (time3 > time4)
                {
                    flag = (time < time4) || (time >= time3);
                }
                else
                {
                    flag = (time >= time3) && (time < time4);
                }
                if (flag)
                {
                    span += daylightChanges.Delta;
                    if ((time >= time5) && (time < time6))
                    {
                        isAmbiguousLocalDst = true;
                    }
                }
            }
            return span.Ticks;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern short[] nativeGetDaylightChanges(int year);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string nativeGetDaylightName();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string nativeGetStandardName();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int nativeGetTimeZoneMinuteOffset();
        public override DateTime ToLocalTime(DateTime time)
        {
            if (time.Kind == DateTimeKind.Local)
            {
                return time;
            }
            bool isAmbiguousLocalDst = false;
            long utcOffsetFromUniversalTime = this.GetUtcOffsetFromUniversalTime(time, ref isAmbiguousLocalDst);
            long ticks = time.Ticks + utcOffsetFromUniversalTime;
            if (ticks > 0x2bca2875f4373fffL)
            {
                return new DateTime(0x2bca2875f4373fffL, DateTimeKind.Local);
            }
            if (ticks < 0L)
            {
                return new DateTime(0L, DateTimeKind.Local);
            }
            return new DateTime(ticks, DateTimeKind.Local, isAmbiguousLocalDst);
        }

        public override string DaylightName
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_daylightName == null)
                {
                    this.m_daylightName = nativeGetDaylightName();
                    if (this.m_daylightName == null)
                    {
                        this.m_daylightName = this.StandardName;
                    }
                }
                return this.m_daylightName;
            }
        }

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

        public override string StandardName
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_standardName == null)
                {
                    this.m_standardName = nativeGetStandardName();
                }
                return this.m_standardName;
            }
        }
    }
}

