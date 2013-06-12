namespace System.Globalization
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class GregorianCalendarHelper
    {
        internal const int DatePartDay = 3;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartYear = 0;
        internal const int DaysPer100Years = 0x8eac;
        internal const int DaysPer400Years = 0x23ab1;
        internal const int DaysPer4Years = 0x5b5;
        internal const int DaysPerYear = 0x16d;
        internal const int DaysTo10000 = 0x37b9db;
        internal static readonly int[] DaysToMonth365 = new int[] { 0, 0x1f, 0x3b, 90, 120, 0x97, 0xb5, 0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d };
        internal static readonly int[] DaysToMonth366 = new int[] { 0, 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6, 0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e };
        internal Calendar m_Cal;
        [OptionalField(VersionAdded=1)]
        internal EraInfo[] m_EraInfo;
        [OptionalField(VersionAdded=1)]
        internal int[] m_eras;
        [OptionalField(VersionAdded=1)]
        internal int m_maxYear = 0x270f;
        [OptionalField(VersionAdded=1)]
        internal DateTime m_minDate;
        [OptionalField(VersionAdded=1)]
        internal int m_minYear;
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

        internal GregorianCalendarHelper(Calendar cal, EraInfo[] eraInfo)
        {
            this.m_Cal = cal;
            this.m_EraInfo = eraInfo;
            this.m_minDate = this.m_Cal.MinSupportedDateTime;
            this.m_maxYear = this.m_EraInfo[0].maxEraYear;
            this.m_minYear = this.m_EraInfo[0].minEraYear;
        }

        public DateTime AddMonths(DateTime time, int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { -120000, 0x1d4c0 }));
            }
            this.CheckTicksRange(time.Ticks);
            int datePart = this.GetDatePart(time.Ticks, 0);
            int index = this.GetDatePart(time.Ticks, 2);
            int day = this.GetDatePart(time.Ticks, 3);
            int num4 = (index - 1) + months;
            if (num4 >= 0)
            {
                index = (num4 % 12) + 1;
                datePart += num4 / 12;
            }
            else
            {
                index = 12 + ((num4 + 1) % 12);
                datePart += (num4 - 11) / 12;
            }
            int[] numArray = (((datePart % 4) == 0) && (((datePart % 100) != 0) || ((datePart % 400) == 0))) ? DaysToMonth366 : DaysToMonth365;
            int num5 = numArray[index] - numArray[index - 1];
            if (day > num5)
            {
                day = num5;
            }
            long ticks = DateToTicks(datePart, index, day) + (time.Ticks % 0xc92a69c000L);
            Calendar.CheckAddResult(ticks, this.m_Cal.MinSupportedDateTime, this.m_Cal.MaxSupportedDateTime);
            return new DateTime(ticks);
        }

        public DateTime AddYears(DateTime time, int years)
        {
            return this.AddMonths(time, years * 12);
        }

        internal void CheckTicksRange(long ticks)
        {
            if ((ticks < this.m_Cal.MinSupportedDateTime.Ticks) || (ticks > this.m_Cal.MaxSupportedDateTime.Ticks))
            {
                throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_CalendarRange"), new object[] { this.m_Cal.MinSupportedDateTime, this.m_Cal.MaxSupportedDateTime }));
            }
        }

        internal static long DateToTicks(int year, int month, int day)
        {
            return (GetAbsoluteDate(year, month, day) * 0xc92a69c000L);
        }

        internal static long GetAbsoluteDate(int year, int month, int day)
        {
            if (((year >= 1) && (year <= 0x270f)) && ((month >= 1) && (month <= 12)))
            {
                int[] numArray = (((year % 4) == 0) && (((year % 100) != 0) || ((year % 400) == 0))) ? DaysToMonth366 : DaysToMonth365;
                if ((day >= 1) && (day <= (numArray[month] - numArray[month - 1])))
                {
                    int num = year - 1;
                    int num2 = ((((((num * 0x16d) + (num / 4)) - (num / 100)) + (num / 400)) + numArray[month - 1]) + day) - 1;
                    return (long) num2;
                }
            }
            throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
        }

        internal virtual int GetDatePart(long ticks, int part)
        {
            this.CheckTicksRange(ticks);
            int num = (int) (ticks / 0xc92a69c000L);
            int num2 = num / 0x23ab1;
            num -= num2 * 0x23ab1;
            int num3 = num / 0x8eac;
            if (num3 == 4)
            {
                num3 = 3;
            }
            num -= num3 * 0x8eac;
            int num4 = num / 0x5b5;
            num -= num4 * 0x5b5;
            int num5 = num / 0x16d;
            if (num5 == 4)
            {
                num5 = 3;
            }
            if (part == 0)
            {
                return (((((num2 * 400) + (num3 * 100)) + (num4 * 4)) + num5) + 1);
            }
            num -= num5 * 0x16d;
            if (part == 1)
            {
                return (num + 1);
            }
            int[] numArray = ((num5 == 3) && ((num4 != 0x18) || (num3 == 3))) ? DaysToMonth366 : DaysToMonth365;
            int index = num >> 6;
            while (num >= numArray[index])
            {
                index++;
            }
            if (part == 2)
            {
                return index;
            }
            return ((num - numArray[index - 1]) + 1);
        }

        public int GetDayOfMonth(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 3);
        }

        public DayOfWeek GetDayOfWeek(DateTime time)
        {
            this.CheckTicksRange(time.Ticks);
            return (DayOfWeek) ((int) (((time.Ticks / 0xc92a69c000L) + 1L) % 7L));
        }

        public int GetDayOfYear(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 1);
        }

        public int GetDaysInMonth(int year, int month, int era)
        {
            year = this.GetGregorianYear(year, era);
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
            int[] numArray = (((year % 4) == 0) && (((year % 100) != 0) || ((year % 400) == 0))) ? DaysToMonth366 : DaysToMonth365;
            return (numArray[month] - numArray[month - 1]);
        }

        public int GetDaysInYear(int year, int era)
        {
            year = this.GetGregorianYear(year, era);
            if (((year % 4) == 0) && (((year % 100) != 0) || ((year % 400) == 0)))
            {
                return 0x16e;
            }
            return 0x16d;
        }

        public int GetEra(DateTime time)
        {
            long ticks = time.Ticks;
            for (int i = 0; i < this.m_EraInfo.Length; i++)
            {
                if (ticks >= this.m_EraInfo[i].ticks)
                {
                    return this.m_EraInfo[i].era;
                }
            }
            throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_Era"));
        }

        internal int GetGregorianYear(int year, int era)
        {
            if (year < 0)
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (era == 0)
            {
                era = this.m_Cal.CurrentEraValue;
            }
            for (int i = 0; i < this.m_EraInfo.Length; i++)
            {
                if (era == this.m_EraInfo[i].era)
                {
                    if ((year < this.m_EraInfo[i].minEraYear) || (year > this.m_EraInfo[i].maxEraYear))
                    {
                        throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { this.m_EraInfo[i].minEraYear, this.m_EraInfo[i].maxEraYear }));
                    }
                    return (this.m_EraInfo[i].yearOffset + year);
                }
            }
            throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
        }

        public int GetLeapMonth(int year, int era)
        {
            year = this.GetGregorianYear(year, era);
            return 0;
        }

        public int GetMonth(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 2);
        }

        public int GetMonthsInYear(int year, int era)
        {
            year = this.GetGregorianYear(year, era);
            return 12;
        }

        public virtual int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            this.CheckTicksRange(time.Ticks);
            return GregorianCalendar.GetDefaultInstance().GetWeekOfYear(time, rule, firstDayOfWeek);
        }

        public int GetYear(DateTime time)
        {
            long ticks = time.Ticks;
            int datePart = this.GetDatePart(ticks, 0);
            for (int i = 0; i < this.m_EraInfo.Length; i++)
            {
                if (ticks >= this.m_EraInfo[i].ticks)
                {
                    return (datePart - this.m_EraInfo[i].yearOffset);
                }
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_NoEra"));
        }

        public int GetYear(int year, DateTime time)
        {
            long ticks = time.Ticks;
            for (int i = 0; i < this.m_EraInfo.Length; i++)
            {
                if (ticks >= this.m_EraInfo[i].ticks)
                {
                    return (year - this.m_EraInfo[i].yearOffset);
                }
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_NoEra"));
        }

        public bool IsLeapDay(int year, int month, int day, int era)
        {
            if ((day < 1) || (day > this.GetDaysInMonth(year, month, era)))
            {
                throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, this.GetDaysInMonth(year, month, era) }));
            }
            if (!this.IsLeapYear(year, era))
            {
                return false;
            }
            return ((month == 2) && (day == 0x1d));
        }

        public bool IsLeapMonth(int year, int month, int era)
        {
            year = this.GetGregorianYear(year, era);
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 12 }));
            }
            return false;
        }

        public bool IsLeapYear(int year, int era)
        {
            year = this.GetGregorianYear(year, era);
            if ((year % 4) != 0)
            {
                return false;
            }
            if ((year % 100) == 0)
            {
                return ((year % 400) == 0);
            }
            return true;
        }

        internal bool IsValidYear(int year, int era)
        {
            if (year >= 0)
            {
                if (era == 0)
                {
                    era = this.m_Cal.CurrentEraValue;
                }
                for (int i = 0; i < this.m_EraInfo.Length; i++)
                {
                    if (era == this.m_EraInfo[i].era)
                    {
                        return ((year >= this.m_EraInfo[i].minEraYear) && (year <= this.m_EraInfo[i].maxEraYear));
                    }
                }
            }
            return false;
        }

        internal static long TimeToTicks(int hour, int minute, int second, int millisecond)
        {
            if ((((hour < 0) || (hour >= 0x18)) || ((minute < 0) || (minute >= 60))) || ((second < 0) || (second >= 60)))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadHourMinuteSecond"));
            }
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                throw new ArgumentOutOfRangeException("millisecond", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 0x3e7 }));
            }
            return (TimeSpan.TimeToTicks(hour, minute, second) + (millisecond * 0x2710L));
        }

        public DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            year = this.GetGregorianYear(year, era);
            long ticks = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second, millisecond);
            this.CheckTicksRange(ticks);
            return new DateTime(ticks);
        }

        public int ToFourDigitYear(int year, int twoDigitYearMax)
        {
            if (year < 0)
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (year < 100)
            {
                int num = year % 100;
                return ((((twoDigitYearMax / 100) - ((num > (twoDigitYearMax % 100)) ? 1 : 0)) * 100) + num);
            }
            if ((year < this.m_minYear) || (year > this.m_maxYear))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { this.m_minYear, this.m_maxYear }));
            }
            return year;
        }

        public int[] Eras
        {
            get
            {
                if (this.m_eras == null)
                {
                    this.m_eras = new int[this.m_EraInfo.Length];
                    for (int i = 0; i < this.m_EraInfo.Length; i++)
                    {
                        this.m_eras[i] = this.m_EraInfo[i].era;
                    }
                }
                return (int[]) this.m_eras.Clone();
            }
        }

        internal int MaxYear
        {
            get
            {
                return this.m_maxYear;
            }
        }
    }
}

