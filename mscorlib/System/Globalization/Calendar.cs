namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public abstract class Calendar : ICloneable
    {
        internal const int CAL_CHINESELUNISOLAR = 15;
        internal const int CAL_GREGORIAN = 1;
        internal const int CAL_GREGORIAN_ARABIC = 10;
        internal const int CAL_GREGORIAN_ME_FRENCH = 9;
        internal const int CAL_GREGORIAN_US = 2;
        internal const int CAL_GREGORIAN_XLIT_ENGLISH = 11;
        internal const int CAL_GREGORIAN_XLIT_FRENCH = 12;
        internal const int CAL_HEBREW = 8;
        internal const int CAL_HIJRI = 6;
        internal const int CAL_JAPAN = 3;
        internal const int CAL_JAPANESELUNISOLAR = 14;
        internal const int CAL_JULIAN = 13;
        internal const int CAL_KOREA = 5;
        internal const int CAL_KOREANLUNISOLAR = 20;
        internal const int CAL_LUNAR_ETO_CHN = 0x11;
        internal const int CAL_LUNAR_ETO_KOR = 0x12;
        internal const int CAL_LUNAR_ETO_ROKUYOU = 0x13;
        internal const int CAL_PERSIAN = 0x16;
        internal const int CAL_SAKA = 0x10;
        internal const int CAL_TAIWAN = 4;
        internal const int CAL_TAIWANLUNISOLAR = 0x15;
        internal const int CAL_THAI = 7;
        internal const int CAL_UMALQURA = 0x17;
        public const int CurrentEra = 0;
        internal const int DaysPer100Years = 0x8eac;
        internal const int DaysPer400Years = 0x23ab1;
        internal const int DaysPer4Years = 0x5b5;
        internal const int DaysPerYear = 0x16d;
        internal const int DaysTo10000 = 0x37b9db;
        internal int m_currentEraValue = -1;
        [OptionalField(VersionAdded=2)]
        private bool m_isReadOnly;
        internal const long MaxMillis = 0x11efae44cb400L;
        internal const int MillisPerDay = 0x5265c00;
        internal const int MillisPerHour = 0x36ee80;
        internal const int MillisPerMinute = 0xea60;
        internal const int MillisPerSecond = 0x3e8;
        internal const long TicksPerDay = 0xc92a69c000L;
        internal const long TicksPerHour = 0x861c46800L;
        internal const long TicksPerMillisecond = 0x2710L;
        internal const long TicksPerMinute = 0x23c34600L;
        internal const long TicksPerSecond = 0x989680L;
        internal int twoDigitYearMax = -1;

        protected Calendar()
        {
        }

        internal DateTime Add(DateTime time, double value, int scale)
        {
            long num = (long) ((value * scale) + ((value >= 0.0) ? 0.5 : -0.5));
            if ((num <= -315537897600000L) || (num >= 0x11efae44cb400L))
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_AddValue"));
            }
            long ticks = time.Ticks + (num * 0x2710L);
            CheckAddResult(ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return new DateTime(ticks);
        }

        public virtual DateTime AddDays(DateTime time, int days)
        {
            return this.Add(time, (double) days, 0x5265c00);
        }

        public virtual DateTime AddHours(DateTime time, int hours)
        {
            return this.Add(time, (double) hours, 0x36ee80);
        }

        public virtual DateTime AddMilliseconds(DateTime time, double milliseconds)
        {
            return this.Add(time, milliseconds, 1);
        }

        public virtual DateTime AddMinutes(DateTime time, int minutes)
        {
            return this.Add(time, (double) minutes, 0xea60);
        }

        public abstract DateTime AddMonths(DateTime time, int months);
        public virtual DateTime AddSeconds(DateTime time, int seconds)
        {
            return this.Add(time, (double) seconds, 0x3e8);
        }

        public virtual DateTime AddWeeks(DateTime time, int weeks)
        {
            return this.AddDays(time, weeks * 7);
        }

        public abstract DateTime AddYears(DateTime time, int years);
        internal static void CheckAddResult(long ticks, DateTime minValue, DateTime maxValue)
        {
            if ((ticks < minValue.Ticks) || (ticks > maxValue.Ticks))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Argument_ResultCalendarRange"), new object[] { minValue, maxValue }));
            }
        }

        [SecuritySafeCritical, ComVisible(false)]
        public virtual object Clone()
        {
            object obj2 = base.MemberwiseClone();
            ((Calendar) obj2).SetReadOnlyState(false);
            return obj2;
        }

        public abstract int GetDayOfMonth(DateTime time);
        public abstract DayOfWeek GetDayOfWeek(DateTime time);
        public abstract int GetDayOfYear(DateTime time);
        public virtual int GetDaysInMonth(int year, int month)
        {
            return this.GetDaysInMonth(year, month, 0);
        }

        public abstract int GetDaysInMonth(int year, int month, int era);
        public virtual int GetDaysInYear(int year)
        {
            return this.GetDaysInYear(year, 0);
        }

        public abstract int GetDaysInYear(int year, int era);
        public abstract int GetEra(DateTime time);
        internal int GetFirstDayWeekOfYear(DateTime time, int firstDayOfWeek)
        {
            int num = this.GetDayOfYear(time) - 1;
            int num2 = ((int) this.GetDayOfWeek(time)) - (num % 7);
            int num3 = ((num2 - firstDayOfWeek) + 14) % 7;
            return (((num + num3) / 7) + 1);
        }

        public virtual int GetHour(DateTime time)
        {
            return (int) ((time.Ticks / 0x861c46800L) % 0x18L);
        }

        [ComVisible(false)]
        public virtual int GetLeapMonth(int year)
        {
            return this.GetLeapMonth(year, 0);
        }

        [ComVisible(false)]
        public virtual int GetLeapMonth(int year, int era)
        {
            if (this.IsLeapYear(year, era))
            {
                int monthsInYear = this.GetMonthsInYear(year, era);
                for (int i = 1; i <= monthsInYear; i++)
                {
                    if (this.IsLeapMonth(year, i, era))
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        public virtual double GetMilliseconds(DateTime time)
        {
            return (double) ((time.Ticks / 0x2710L) % 0x3e8L);
        }

        public virtual int GetMinute(DateTime time)
        {
            return (int) ((time.Ticks / 0x23c34600L) % 60L);
        }

        public abstract int GetMonth(DateTime time);
        public virtual int GetMonthsInYear(int year)
        {
            return this.GetMonthsInYear(year, 0);
        }

        public abstract int GetMonthsInYear(int year, int era);
        public virtual int GetSecond(DateTime time)
        {
            return (int) ((time.Ticks / 0x989680L) % 60L);
        }

        [SecuritySafeCritical]
        internal static int GetSystemTwoDigitYearSetting(int CalID, int defaultYearValue)
        {
            int num = CalendarData.nativeGetTwoDigitYearMax(CalID);
            if (num < 0)
            {
                num = defaultYearValue;
            }
            return num;
        }

        public virtual int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            if ((firstDayOfWeek < DayOfWeek.Sunday) || (firstDayOfWeek > DayOfWeek.Saturday))
            {
                throw new ArgumentOutOfRangeException("firstDayOfWeek", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { DayOfWeek.Sunday, DayOfWeek.Saturday }));
            }
            switch (rule)
            {
                case CalendarWeekRule.FirstDay:
                    return this.GetFirstDayWeekOfYear(time, (int) firstDayOfWeek);

                case CalendarWeekRule.FirstFullWeek:
                    return this.GetWeekOfYearFullDays(time, rule, (int) firstDayOfWeek, 7);

                case CalendarWeekRule.FirstFourDayWeek:
                    return this.GetWeekOfYearFullDays(time, rule, (int) firstDayOfWeek, 4);
            }
            throw new ArgumentOutOfRangeException("rule", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { CalendarWeekRule.FirstDay, CalendarWeekRule.FirstFourDayWeek }));
        }

        internal int GetWeekOfYearFullDays(DateTime time, CalendarWeekRule rule, int firstDayOfWeek, int fullDays)
        {
            int num4 = this.GetDayOfYear(time) - 1;
            int num = ((int) this.GetDayOfWeek(time)) - (num4 % 7);
            int num2 = ((firstDayOfWeek - num) + 14) % 7;
            if ((num2 != 0) && (num2 >= fullDays))
            {
                num2 -= 7;
            }
            int num3 = num4 - num2;
            if (num3 >= 0)
            {
                return ((num3 / 7) + 1);
            }
            return this.GetWeekOfYearFullDays(time.AddDays((double) -(num4 + 1)), rule, firstDayOfWeek, fullDays);
        }

        public abstract int GetYear(DateTime time);
        public virtual bool IsLeapDay(int year, int month, int day)
        {
            return this.IsLeapDay(year, month, day, 0);
        }

        public abstract bool IsLeapDay(int year, int month, int day, int era);
        public virtual bool IsLeapMonth(int year, int month)
        {
            return this.IsLeapMonth(year, month, 0);
        }

        public abstract bool IsLeapMonth(int year, int month, int era);
        public virtual bool IsLeapYear(int year)
        {
            return this.IsLeapYear(year, 0);
        }

        public abstract bool IsLeapYear(int year, int era);
        internal virtual bool IsValidDay(int year, int month, int day, int era)
        {
            return ((this.IsValidMonth(year, month, era) && (day >= 1)) && (day <= this.GetDaysInMonth(year, month, era)));
        }

        internal virtual bool IsValidMonth(int year, int month, int era)
        {
            return ((this.IsValidYear(year, era) && (month >= 1)) && (month <= this.GetMonthsInYear(year, era)));
        }

        internal virtual bool IsValidYear(int year, int era)
        {
            return ((year >= this.GetYear(this.MinSupportedDateTime)) && (year <= this.GetYear(this.MaxSupportedDateTime)));
        }

        [ComVisible(false), SecuritySafeCritical]
        public static Calendar ReadOnly(Calendar calendar)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException("calendar");
            }
            if (calendar.IsReadOnly)
            {
                return calendar;
            }
            Calendar calendar2 = (Calendar) calendar.MemberwiseClone();
            calendar2.SetReadOnlyState(true);
            return calendar2;
        }

        internal void SetReadOnlyState(bool readOnly)
        {
            this.m_isReadOnly = readOnly;
        }

        internal static long TimeToTicks(int hour, int minute, int second, int millisecond)
        {
            if ((((hour < 0) || (hour >= 0x18)) || ((minute < 0) || (minute >= 60))) || ((second < 0) || (second >= 60)))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadHourMinuteSecond"));
            }
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                throw new ArgumentOutOfRangeException("millisecond", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 0x3e7 }));
            }
            return (TimeSpan.TimeToTicks(hour, minute, second) + (millisecond * 0x2710L));
        }

        public virtual DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            return this.ToDateTime(year, month, day, hour, minute, second, millisecond, 0);
        }

        public abstract DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era);
        public virtual int ToFourDigitYear(int year)
        {
            if (year < 0)
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (year < 100)
            {
                return ((((this.TwoDigitYearMax / 100) - ((year > (this.TwoDigitYearMax % 100)) ? 1 : 0)) * 100) + year);
            }
            return year;
        }

        internal virtual bool TryToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era, out DateTime result)
        {
            result = DateTime.MinValue;
            try
            {
                result = this.ToDateTime(year, month, day, hour, minute, second, millisecond, era);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        internal void VerifyWritable()
        {
            if (this.m_isReadOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
            }
        }

        [ComVisible(false)]
        public virtual CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.Unknown;
            }
        }

        internal virtual int BaseCalendarID
        {
            get
            {
                return this.ID;
            }
        }

        internal virtual int CurrentEraValue
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_currentEraValue == -1)
                {
                    this.m_currentEraValue = CalendarData.GetCalendarData(this.BaseCalendarID).iCurrentEra;
                }
                return this.m_currentEraValue;
            }
        }

        public abstract int[] Eras { get; }

        internal virtual int ID
        {
            get
            {
                return -1;
            }
        }

        [ComVisible(false)]
        public bool IsReadOnly
        {
            get
            {
                return this.m_isReadOnly;
            }
        }

        [ComVisible(false)]
        public virtual DateTime MaxSupportedDateTime
        {
            get
            {
                return DateTime.MaxValue;
            }
        }

        [ComVisible(false)]
        public virtual DateTime MinSupportedDateTime
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        public virtual int TwoDigitYearMax
        {
            get
            {
                return this.twoDigitYearMax;
            }
            set
            {
                this.VerifyWritable();
                this.twoDigitYearMax = value;
            }
        }
    }
}

