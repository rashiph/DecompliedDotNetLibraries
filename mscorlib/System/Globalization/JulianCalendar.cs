namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class JulianCalendar : Calendar
    {
        private const int DatePartDay = 3;
        private const int DatePartDayOfYear = 1;
        private const int DatePartMonth = 2;
        private const int DatePartYear = 0;
        private static readonly int[] DaysToMonth365 = new int[] { 0, 0x1f, 0x3b, 90, 120, 0x97, 0xb5, 0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d };
        private static readonly int[] DaysToMonth366 = new int[] { 0, 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6, 0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e };
        private const int JulianDaysPer4Years = 0x5b5;
        private const int JulianDaysPerYear = 0x16d;
        public static readonly int JulianEra = 1;
        internal int MaxYear = 0x270f;

        public JulianCalendar()
        {
            base.twoDigitYearMax = 0x7ed;
        }

        public override DateTime AddMonths(DateTime time, int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { -120000, 0x1d4c0 }));
            }
            int datePart = GetDatePart(time.Ticks, 0);
            int index = GetDatePart(time.Ticks, 2);
            int day = GetDatePart(time.Ticks, 3);
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
            Calendar.CheckAddResult(ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return new DateTime(ticks);
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            return this.AddMonths(time, years * 12);
        }

        internal static void CheckDayRange(int year, int month, int day)
        {
            if (((year == 1) && (month == 1)) && (day < 3))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
            }
            int[] numArray = ((year % 4) == 0) ? DaysToMonth366 : DaysToMonth365;
            int num = numArray[month] - numArray[month - 1];
            if ((day < 1) || (day > num))
            {
                throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, num }));
            }
        }

        internal static void CheckEraRange(int era)
        {
            if ((era != 0) && (era != JulianEra))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
        }

        internal static void CheckMonthRange(int month)
        {
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
        }

        internal void CheckYearEraRange(int year, int era)
        {
            CheckEraRange(era);
            if ((year <= 0) || (year > this.MaxYear))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, this.MaxYear }));
            }
        }

        internal static long DateToTicks(int year, int month, int day)
        {
            int[] numArray = ((year % 4) == 0) ? DaysToMonth366 : DaysToMonth365;
            int num = year - 1;
            int num2 = ((((num * 0x16d) + (num / 4)) + numArray[month - 1]) + day) - 1;
            return ((num2 - 2) * 0xc92a69c000L);
        }

        internal static int GetDatePart(long ticks, int part)
        {
            long num = ticks + 0x19254d38000L;
            int num2 = (int) (num / 0xc92a69c000L);
            int num3 = num2 / 0x5b5;
            num2 -= num3 * 0x5b5;
            int num4 = num2 / 0x16d;
            if (num4 == 4)
            {
                num4 = 3;
            }
            if (part == 0)
            {
                return (((num3 * 4) + num4) + 1);
            }
            num2 -= num4 * 0x16d;
            if (part == 1)
            {
                return (num2 + 1);
            }
            int[] numArray = (num4 == 3) ? DaysToMonth366 : DaysToMonth365;
            int index = num2 >> 6;
            while (num2 >= numArray[index])
            {
                index++;
            }
            if (part == 2)
            {
                return index;
            }
            return ((num2 - numArray[index - 1]) + 1);
        }

        public override int GetDayOfMonth(DateTime time)
        {
            return GetDatePart(time.Ticks, 3);
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            return (DayOfWeek) (((int) ((time.Ticks / 0xc92a69c000L) + 1L)) % 7);
        }

        public override int GetDayOfYear(DateTime time)
        {
            return GetDatePart(time.Ticks, 1);
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            this.CheckYearEraRange(year, era);
            CheckMonthRange(month);
            int[] numArray = ((year % 4) == 0) ? DaysToMonth366 : DaysToMonth365;
            return (numArray[month] - numArray[month - 1]);
        }

        public override int GetDaysInYear(int year, int era)
        {
            if (!this.IsLeapYear(year, era))
            {
                return 0x16d;
            }
            return 0x16e;
        }

        public override int GetEra(DateTime time)
        {
            return JulianEra;
        }

        [ComVisible(false)]
        public override int GetLeapMonth(int year, int era)
        {
            this.CheckYearEraRange(year, era);
            return 0;
        }

        public override int GetMonth(DateTime time)
        {
            return GetDatePart(time.Ticks, 2);
        }

        public override int GetMonthsInYear(int year, int era)
        {
            this.CheckYearEraRange(year, era);
            return 12;
        }

        public override int GetYear(DateTime time)
        {
            return GetDatePart(time.Ticks, 0);
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            CheckMonthRange(month);
            if (this.IsLeapYear(year, era))
            {
                CheckDayRange(year, month, day);
                return ((month == 2) && (day == 0x1d));
            }
            CheckDayRange(year, month, day);
            return false;
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            this.CheckYearEraRange(year, era);
            CheckMonthRange(month);
            return false;
        }

        public override bool IsLeapYear(int year, int era)
        {
            this.CheckYearEraRange(year, era);
            return ((year % 4) == 0);
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            this.CheckYearEraRange(year, era);
            CheckMonthRange(month);
            CheckDayRange(year, month, day);
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                throw new ArgumentOutOfRangeException("millisecond", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 0x3e7 }));
            }
            if ((((hour < 0) || (hour >= 0x18)) || ((minute < 0) || (minute >= 60))) || ((second < 0) || (second >= 60)))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadHourMinuteSecond"));
            }
            TimeSpan span = new TimeSpan(0, hour, minute, second, millisecond);
            return new DateTime(DateToTicks(year, month, day) + span.Ticks);
        }

        public override int ToFourDigitYear(int year)
        {
            if (year > this.MaxYear)
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), new object[] { 1, this.MaxYear }));
            }
            return base.ToFourDigitYear(year);
        }

        [ComVisible(false)]
        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.SolarCalendar;
            }
        }

        public override int[] Eras
        {
            get
            {
                return new int[] { JulianEra };
            }
        }

        internal override int ID
        {
            get
            {
                return 13;
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
                return base.twoDigitYearMax;
            }
            set
            {
                base.VerifyWritable();
                if ((value < 0x63) || (value > this.MaxYear))
                {
                    throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x63, this.MaxYear }));
                }
                base.twoDigitYearMax = value;
            }
        }
    }
}

