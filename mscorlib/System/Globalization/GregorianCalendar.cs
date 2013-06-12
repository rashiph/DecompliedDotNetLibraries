namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public class GregorianCalendar : Calendar
    {
        public const int ADEra = 1;
        internal const int DatePartDay = 3;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartYear = 0;
        internal static readonly int[] DaysToMonth365 = new int[] { 0, 0x1f, 0x3b, 90, 120, 0x97, 0xb5, 0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d };
        internal static readonly int[] DaysToMonth366 = new int[] { 0, 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6, 0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e };
        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 0x7ed;
        internal GregorianCalendarTypes m_type;
        internal const int MaxYear = 0x270f;
        private static Calendar s_defaultInstance;

        public GregorianCalendar() : this(GregorianCalendarTypes.Localized)
        {
        }

        public GregorianCalendar(GregorianCalendarTypes type)
        {
            if ((type < GregorianCalendarTypes.Localized) || (type > GregorianCalendarTypes.TransliteratedFrench))
            {
                throw new ArgumentOutOfRangeException("type", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { GregorianCalendarTypes.Localized, GregorianCalendarTypes.TransliteratedFrench }));
            }
            this.m_type = type;
        }

        public override DateTime AddMonths(DateTime time, int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { -120000, 0x1d4c0 }));
            }
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
            long ticks = this.DateToTicks(datePart, index, day) + (time.Ticks % 0xc92a69c000L);
            Calendar.CheckAddResult(ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return new DateTime(ticks);
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            return this.AddMonths(time, years * 12);
        }

        internal virtual long DateToTicks(int year, int month, int day)
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

        public override int GetDayOfMonth(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 3);
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            return (DayOfWeek) (((int) ((time.Ticks / 0xc92a69c000L) + 1L)) % 7);
        }

        public override int GetDayOfYear(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 1);
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 0x270f }));
            }
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
            int[] numArray = (((year % 4) == 0) && (((year % 100) != 0) || ((year % 400) == 0))) ? DaysToMonth366 : DaysToMonth365;
            return (numArray[month] - numArray[month - 1]);
        }

        public override int GetDaysInYear(int year, int era)
        {
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x270f }));
            }
            if (((year % 4) != 0) || (((year % 100) == 0) && ((year % 400) != 0)))
            {
                return 0x16d;
            }
            return 0x16e;
        }

        internal static Calendar GetDefaultInstance()
        {
            if (s_defaultInstance == null)
            {
                s_defaultInstance = new GregorianCalendar();
            }
            return s_defaultInstance;
        }

        public override int GetEra(DateTime time)
        {
            return 1;
        }

        [ComVisible(false)]
        public override int GetLeapMonth(int year, int era)
        {
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x270f }));
            }
            return 0;
        }

        public override int GetMonth(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 2);
        }

        public override int GetMonthsInYear(int year, int era)
        {
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x270f }));
            }
            return 12;
        }

        [ComVisible(false)]
        public override int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            if ((firstDayOfWeek < DayOfWeek.Sunday) || (firstDayOfWeek > DayOfWeek.Saturday))
            {
                throw new ArgumentOutOfRangeException("firstDayOfWeek", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { DayOfWeek.Sunday, DayOfWeek.Saturday }));
            }
            switch (rule)
            {
                case CalendarWeekRule.FirstDay:
                    return base.GetFirstDayWeekOfYear(time, (int) firstDayOfWeek);

                case CalendarWeekRule.FirstFullWeek:
                    return InternalGetWeekOfYearFullDays(this, time, (int) firstDayOfWeek, 7, 0x16d);

                case CalendarWeekRule.FirstFourDayWeek:
                    return InternalGetWeekOfYearFullDays(this, time, (int) firstDayOfWeek, 4, 0x16d);
            }
            throw new ArgumentOutOfRangeException("rule", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { CalendarWeekRule.FirstDay, CalendarWeekRule.FirstFourDayWeek }));
        }

        public override int GetYear(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 0);
        }

        internal static int InternalGetWeekOfYearFullDays(Calendar cal, DateTime time, int firstDayOfWeek, int fullDays, int daysOfMinYearMinusOne)
        {
            int daysInYear = cal.GetDayOfYear(time) - 1;
            int num = ((int) cal.GetDayOfWeek(time)) - (daysInYear % 7);
            int num2 = ((firstDayOfWeek - num) + 14) % 7;
            if ((num2 != 0) && (num2 >= fullDays))
            {
                num2 -= 7;
            }
            int num3 = daysInYear - num2;
            if (num3 < 0)
            {
                int year = cal.GetYear(time);
                if (year <= cal.GetYear(cal.MinSupportedDateTime))
                {
                    daysInYear = daysOfMinYearMinusOne;
                }
                else
                {
                    daysInYear = cal.GetDaysInYear(year - 1);
                }
                num -= daysInYear % 7;
                num2 = ((firstDayOfWeek - num) + 14) % 7;
                if ((num2 != 0) && (num2 >= fullDays))
                {
                    num2 -= 7;
                }
                num3 = daysInYear - num2;
            }
            return ((num3 / 7) + 1);
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 12 }));
            }
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 0x270f }));
            }
            if ((day < 1) || (day > this.GetDaysInMonth(year, month)))
            {
                throw new ArgumentOutOfRangeException("day", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, this.GetDaysInMonth(year, month) }));
            }
            if (!this.IsLeapYear(year))
            {
                return false;
            }
            return ((month == 2) && (day == 0x1d));
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x270f }));
            }
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 12 }));
            }
            return false;
        }

        public override bool IsLeapYear(int year, int era)
        {
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x270f }));
            }
            if ((year % 4) != 0)
            {
                return false;
            }
            return (((year % 100) != 0) || ((year % 400) == 0));
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if ((this.m_type < GregorianCalendarTypes.Localized) || (this.m_type > GregorianCalendarTypes.TransliteratedFrench))
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_MemberOutOfRange"), new object[] { "type", "GregorianCalendar" }));
            }
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            if ((era != 0) && (era != 1))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        public override int ToFourDigitYear(int year)
        {
            if (year > 0x270f)
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x270f }));
            }
            return base.ToFourDigitYear(year);
        }

        internal override bool TryToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era, out DateTime result)
        {
            if ((era == 0) || (era == 1))
            {
                return DateTime.TryCreate(year, month, day, hour, minute, second, millisecond, out result);
            }
            result = DateTime.MinValue;
            return false;
        }

        [ComVisible(false)]
        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.SolarCalendar;
            }
        }

        public virtual GregorianCalendarTypes CalendarType
        {
            get
            {
                return this.m_type;
            }
            set
            {
                base.VerifyWritable();
                switch (value)
                {
                    case GregorianCalendarTypes.Localized:
                    case GregorianCalendarTypes.USEnglish:
                    case GregorianCalendarTypes.MiddleEastFrench:
                    case GregorianCalendarTypes.Arabic:
                    case GregorianCalendarTypes.TransliteratedEnglish:
                    case GregorianCalendarTypes.TransliteratedFrench:
                        this.m_type = value;
                        return;
                }
                throw new ArgumentOutOfRangeException("m_type", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
        }

        public override int[] Eras
        {
            get
            {
                return new int[] { 1 };
            }
        }

        internal override int ID
        {
            get
            {
                return (int) this.m_type;
            }
        }

        [ComVisible(false)]
        public override DateTime MaxSupportedDateTime
        {
            get
            {
                return DateTime.MaxValue;
            }
        }

        [ComVisible(false)]
        public override DateTime MinSupportedDateTime
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        public override int TwoDigitYearMax
        {
            get
            {
                if (base.twoDigitYearMax == -1)
                {
                    base.twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(this.ID, 0x7ed);
                }
                return base.twoDigitYearMax;
            }
            set
            {
                base.VerifyWritable();
                if ((value < 0x63) || (value > 0x270f))
                {
                    throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x63, 0x270f }));
                }
                base.twoDigitYearMax = value;
            }
        }
    }
}

