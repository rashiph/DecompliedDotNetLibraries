namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public abstract class EastAsianLunisolarCalendar : Calendar
    {
        internal const int DatePartDay = 3;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartYear = 0;
        internal static readonly int[] DaysToMonth365 = new int[] { 0, 0x1f, 0x3b, 90, 120, 0x97, 0xb5, 0xd4, 0xf3, 0x111, 0x130, 0x14e };
        internal static readonly int[] DaysToMonth366 = new int[] { 0, 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6, 0xd5, 0xf4, 0x112, 0x131, 0x14f };
        private const int DEFAULT_GREGORIAN_TWO_DIGIT_YEAR_MAX = 0x7ed;
        internal const int Jan1Date = 2;
        internal const int Jan1Month = 1;
        internal const int LeapMonth = 0;
        internal const int MaxCalendarDay = 30;
        internal const int MaxCalendarMonth = 13;
        internal const int nDaysPerMonth = 3;

        internal EastAsianLunisolarCalendar()
        {
        }

        public override DateTime AddMonths(DateTime time, int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { -120000, 0x1d4c0 }));
            }
            this.CheckTicksRange(time.Ticks);
            int year = 0;
            int month = 0;
            int day = 0;
            this.TimeToLunar(time, ref year, ref month, ref day);
            int num4 = month + months;
            if (num4 <= 0)
            {
                while (num4 <= 0)
                {
                    int num6 = this.InternalIsLeapYear(year - 1) ? 13 : 12;
                    num4 += num6;
                    year--;
                }
                month = num4;
            }
            else
            {
                for (int i = this.InternalIsLeapYear(year) ? 13 : 12; (num4 - i) > 0; i = this.InternalIsLeapYear(year) ? 13 : 12)
                {
                    num4 -= i;
                    year++;
                }
                month = num4;
            }
            int daysInMonth = this.InternalGetDaysInMonth(year, month);
            if (day > daysInMonth)
            {
                day = daysInMonth;
            }
            DateTime time2 = this.LunarToTime(time, year, month, day);
            Calendar.CheckAddResult(time2.Ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return time2;
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            this.CheckTicksRange(time.Ticks);
            int year = 0;
            int month = 0;
            int day = 0;
            this.TimeToLunar(time, ref year, ref month, ref day);
            year += years;
            if ((month == 13) && !this.InternalIsLeapYear(year))
            {
                month = 12;
                day = this.InternalGetDaysInMonth(year, month);
            }
            int daysInMonth = this.InternalGetDaysInMonth(year, month);
            if (day > daysInMonth)
            {
                day = daysInMonth;
            }
            DateTime time2 = this.LunarToTime(time, year, month, day);
            Calendar.CheckAddResult(time2.Ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return time2;
        }

        internal void CheckEraRange(int era)
        {
            if (era == 0)
            {
                era = this.CurrentEraValue;
            }
            if ((era < this.GetEra(this.MinDate)) || (era > this.GetEra(this.MaxDate)))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
        }

        internal void CheckTicksRange(long ticks)
        {
            if ((ticks < this.MinSupportedDateTime.Ticks) || (ticks > this.MaxSupportedDateTime.Ticks))
            {
                throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_CalendarRange"), new object[] { this.MinSupportedDateTime, this.MaxSupportedDateTime }));
            }
        }

        internal int CheckYearMonthRange(int year, int month, int era)
        {
            year = this.CheckYearRange(year, era);
            if ((month == 13) && (this.GetYearInfo(year, 0) == 0))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
            if ((month < 1) || (month > 13))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
            return year;
        }

        internal int CheckYearRange(int year, int era)
        {
            this.CheckEraRange(era);
            year = this.GetGregorianYear(year, era);
            if ((year < this.MinCalendarYear) || (year > this.MaxCalendarYear))
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { this.MinEraCalendarYear(era), this.MaxEraCalendarYear(era) }));
            }
            return year;
        }

        public int GetCelestialStem(int sexagenaryYear)
        {
            if ((sexagenaryYear < 1) || (sexagenaryYear > 60))
            {
                throw new ArgumentOutOfRangeException("sexagenaryYear", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 60 }));
            }
            return (((sexagenaryYear - 1) % 10) + 1);
        }

        public override int GetDayOfMonth(DateTime time)
        {
            this.CheckTicksRange(time.Ticks);
            int year = 0;
            int month = 0;
            int day = 0;
            this.TimeToLunar(time, ref year, ref month, ref day);
            return day;
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            this.CheckTicksRange(time.Ticks);
            return (DayOfWeek) (((int) ((time.Ticks / 0xc92a69c000L) + 1L)) % 7);
        }

        public override int GetDayOfYear(DateTime time)
        {
            this.CheckTicksRange(time.Ticks);
            int year = 0;
            int month = 0;
            int day = 0;
            this.TimeToLunar(time, ref year, ref month, ref day);
            for (int i = 1; i < month; i++)
            {
                day += this.InternalGetDaysInMonth(year, i);
            }
            return day;
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            year = this.CheckYearMonthRange(year, month, era);
            return this.InternalGetDaysInMonth(year, month);
        }

        public override int GetDaysInYear(int year, int era)
        {
            year = this.CheckYearRange(year, era);
            int num = 0;
            int num2 = this.InternalIsLeapYear(year) ? 13 : 12;
            while (num2 != 0)
            {
                num += this.InternalGetDaysInMonth(year, num2--);
            }
            return num;
        }

        internal abstract int GetGregorianYear(int year, int era);
        public override int GetLeapMonth(int year, int era)
        {
            year = this.CheckYearRange(year, era);
            int yearInfo = this.GetYearInfo(year, 0);
            if (yearInfo > 0)
            {
                return (yearInfo + 1);
            }
            return 0;
        }

        public override int GetMonth(DateTime time)
        {
            this.CheckTicksRange(time.Ticks);
            int year = 0;
            int month = 0;
            int day = 0;
            this.TimeToLunar(time, ref year, ref month, ref day);
            return month;
        }

        public override int GetMonthsInYear(int year, int era)
        {
            year = this.CheckYearRange(year, era);
            if (!this.InternalIsLeapYear(year))
            {
                return 12;
            }
            return 13;
        }

        public virtual int GetSexagenaryYear(DateTime time)
        {
            this.CheckTicksRange(time.Ticks);
            int year = 0;
            int month = 0;
            int day = 0;
            this.TimeToLunar(time, ref year, ref month, ref day);
            return (((year - 4) % 60) + 1);
        }

        public int GetTerrestrialBranch(int sexagenaryYear)
        {
            if ((sexagenaryYear < 1) || (sexagenaryYear > 60))
            {
                throw new ArgumentOutOfRangeException("sexagenaryYear", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 60 }));
            }
            return (((sexagenaryYear - 1) % 12) + 1);
        }

        public override int GetYear(DateTime time)
        {
            this.CheckTicksRange(time.Ticks);
            int year = 0;
            int month = 0;
            int day = 0;
            this.TimeToLunar(time, ref year, ref month, ref day);
            return this.GetYear(year, time);
        }

        internal abstract int GetYear(int year, DateTime time);
        internal abstract int GetYearInfo(int LunarYear, int Index);
        private static int GregorianIsLeapYear(int y)
        {
            if ((y % 4) == 0)
            {
                if ((y % 100) != 0)
                {
                    return 1;
                }
                if ((y % 400) == 0)
                {
                    return 1;
                }
            }
            return 0;
        }

        internal void GregorianToLunar(int nSYear, int nSMonth, int nSDate, ref int nLYear, ref int nLMonth, ref int nLDate)
        {
            int num7;
            int num8;
            int num = (GregorianIsLeapYear(nSYear) == 1) ? DaysToMonth366[nSMonth - 1] : DaysToMonth365[nSMonth - 1];
            num += nSDate;
            int num2 = num;
            nLYear = nSYear;
            if (nLYear == (this.MaxCalendarYear + 1))
            {
                nLYear--;
                num2 += (GregorianIsLeapYear(nLYear) == 1) ? 0x16e : 0x16d;
                num7 = this.GetYearInfo(nLYear, 1);
                num8 = this.GetYearInfo(nLYear, 2);
            }
            else
            {
                num7 = this.GetYearInfo(nLYear, 1);
                num8 = this.GetYearInfo(nLYear, 2);
                if ((nSMonth < num7) || ((nSMonth == num7) && (nSDate < num8)))
                {
                    nLYear--;
                    num2 += (GregorianIsLeapYear(nLYear) == 1) ? 0x16e : 0x16d;
                    num7 = this.GetYearInfo(nLYear, 1);
                    num8 = this.GetYearInfo(nLYear, 2);
                }
            }
            num2 -= DaysToMonth365[num7 - 1];
            num2 -= num8 - 1;
            int num5 = 0x8000;
            int yearInfo = this.GetYearInfo(nLYear, 3);
            int num6 = ((yearInfo & num5) != 0) ? 30 : 0x1d;
            nLMonth = 1;
            while (num2 > num6)
            {
                num2 -= num6;
                nLMonth++;
                num5 = num5 >> 1;
                num6 = ((yearInfo & num5) != 0) ? 30 : 0x1d;
            }
            nLDate = num2;
        }

        internal int InternalGetDaysInMonth(int year, int month)
        {
            int num2 = 0x8000;
            num2 = num2 >> (month - 1);
            if ((this.GetYearInfo(year, 3) & num2) == 0)
            {
                return 0x1d;
            }
            return 30;
        }

        internal bool InternalIsLeapYear(int year)
        {
            return (this.GetYearInfo(year, 0) != 0);
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            year = this.CheckYearMonthRange(year, month, era);
            int daysInMonth = this.InternalGetDaysInMonth(year, month);
            if ((day < 1) || (day > daysInMonth))
            {
                throw new ArgumentOutOfRangeException("day", Environment.GetResourceString("ArgumentOutOfRange_Day", new object[] { daysInMonth, month }));
            }
            int yearInfo = this.GetYearInfo(year, 0);
            return ((yearInfo != 0) && (month == (yearInfo + 1)));
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            year = this.CheckYearMonthRange(year, month, era);
            int yearInfo = this.GetYearInfo(year, 0);
            return ((yearInfo != 0) && (month == (yearInfo + 1)));
        }

        public override bool IsLeapYear(int year, int era)
        {
            year = this.CheckYearRange(year, era);
            return this.InternalIsLeapYear(year);
        }

        internal bool LunarToGregorian(int nLYear, int nLMonth, int nLDate, ref int nSolarYear, ref int nSolarMonth, ref int nSolarDay)
        {
            if ((nLDate < 1) || (nLDate > 30))
            {
                return false;
            }
            int num = nLDate - 1;
            for (int i = 1; i < nLMonth; i++)
            {
                num += this.InternalGetDaysInMonth(nLYear, i);
            }
            int yearInfo = this.GetYearInfo(nLYear, 1);
            int num4 = this.GetYearInfo(nLYear, 2);
            int num5 = GregorianIsLeapYear(nLYear);
            int[] numArray = (num5 == 1) ? DaysToMonth366 : DaysToMonth365;
            nSolarDay = num4;
            if (yearInfo > 1)
            {
                nSolarDay += numArray[yearInfo - 1];
            }
            nSolarDay += num;
            if (nSolarDay > (num5 + 0x16d))
            {
                nSolarYear = nLYear + 1;
                nSolarDay -= num5 + 0x16d;
            }
            else
            {
                nSolarYear = nLYear;
            }
            nSolarMonth = 1;
            while (nSolarMonth < 12)
            {
                if (numArray[nSolarMonth] >= nSolarDay)
                {
                    break;
                }
                nSolarMonth++;
            }
            nSolarDay -= numArray[nSolarMonth - 1];
            return true;
        }

        internal DateTime LunarToTime(DateTime time, int year, int month, int day)
        {
            int nSolarYear = 0;
            int nSolarMonth = 0;
            int nSolarDay = 0;
            this.LunarToGregorian(year, month, day, ref nSolarYear, ref nSolarMonth, ref nSolarDay);
            return GregorianCalendar.GetDefaultInstance().ToDateTime(nSolarYear, nSolarMonth, nSolarDay, time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        internal int MaxEraCalendarYear(int era)
        {
            EraInfo[] calEraInfo = this.CalEraInfo;
            if (calEraInfo == null)
            {
                return this.MaxCalendarYear;
            }
            if (era == 0)
            {
                era = this.CurrentEraValue;
            }
            if (era == this.GetEra(this.MaxDate))
            {
                return this.GetYear(this.MaxCalendarYear, this.MaxDate);
            }
            for (int i = 0; i < calEraInfo.Length; i++)
            {
                if (era == calEraInfo[i].era)
                {
                    return calEraInfo[i].maxEraYear;
                }
            }
            throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
        }

        internal int MinEraCalendarYear(int era)
        {
            EraInfo[] calEraInfo = this.CalEraInfo;
            if (calEraInfo == null)
            {
                return this.MinCalendarYear;
            }
            if (era == 0)
            {
                era = this.CurrentEraValue;
            }
            if (era == this.GetEra(this.MinDate))
            {
                return this.GetYear(this.MinCalendarYear, this.MinDate);
            }
            for (int i = 0; i < calEraInfo.Length; i++)
            {
                if (era == calEraInfo[i].era)
                {
                    return calEraInfo[i].minEraYear;
                }
            }
            throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
        }

        internal void TimeToLunar(DateTime time, ref int year, ref int month, ref int day)
        {
            int nSYear = 0;
            int nSMonth = 0;
            int nSDate = 0;
            Calendar defaultInstance = GregorianCalendar.GetDefaultInstance();
            nSYear = defaultInstance.GetYear(time);
            nSMonth = defaultInstance.GetMonth(time);
            nSDate = defaultInstance.GetDayOfMonth(time);
            this.GregorianToLunar(nSYear, nSMonth, nSDate, ref year, ref month, ref day);
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            year = this.CheckYearMonthRange(year, month, era);
            int daysInMonth = this.InternalGetDaysInMonth(year, month);
            if ((day < 1) || (day > daysInMonth))
            {
                throw new ArgumentOutOfRangeException("day", Environment.GetResourceString("ArgumentOutOfRange_Day", new object[] { daysInMonth, month }));
            }
            int nSolarYear = 0;
            int nSolarMonth = 0;
            int nSolarDay = 0;
            if (!this.LunarToGregorian(year, month, day, ref nSolarYear, ref nSolarMonth, ref nSolarDay))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
            }
            return new DateTime(nSolarYear, nSolarMonth, nSolarDay, hour, minute, second, millisecond);
        }

        public override int ToFourDigitYear(int year)
        {
            year = base.ToFourDigitYear(year);
            this.CheckYearRange(year, 0);
            return year;
        }

        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.LunisolarCalendar;
            }
        }

        internal abstract EraInfo[] CalEraInfo { get; }

        internal abstract int MaxCalendarYear { get; }

        internal abstract DateTime MaxDate { get; }

        internal abstract int MinCalendarYear { get; }

        internal abstract DateTime MinDate { get; }

        public override int TwoDigitYearMax
        {
            get
            {
                if (base.twoDigitYearMax == -1)
                {
                    base.twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(this.BaseCalendarID, this.GetYear(new DateTime(0x7ed, 1, 1)));
                }
                return base.twoDigitYearMax;
            }
            set
            {
                base.VerifyWritable();
                if ((value < 0x63) || (value > this.MaxCalendarYear))
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0x63, this.MaxCalendarYear }));
                }
                base.twoDigitYearMax = value;
            }
        }
    }
}

