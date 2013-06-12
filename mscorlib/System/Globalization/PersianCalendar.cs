namespace System.Globalization
{
    using System;

    [Serializable]
    public class PersianCalendar : Calendar
    {
        internal const int DateCycle = 0x21;
        internal const int DatePartDay = 3;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartYear = 0;
        internal const long DaysPerCycle = 0x2f15L;
        internal static int[] DaysToMonth = new int[] { 0, 0x1f, 0x3e, 0x5d, 0x7c, 0x9b, 0xba, 0xd8, 0xf6, 0x114, 0x132, 0x150 };
        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 0x582;
        internal const long GregorianOffset = 0x3764eL;
        internal static int[] LeapYears33 = new int[] { 
            0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 
            0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 
            0
         };
        internal const int LeapYearsPerCycle = 8;
        internal const int MaxCalendarDay = 10;
        internal const int MaxCalendarMonth = 10;
        internal const int MaxCalendarYear = 0x24a2;
        internal static DateTime maxDate = DateTime.MaxValue;
        internal static DateTime minDate = new DateTime(0x26e, 3, 0x15);
        public static readonly int PersianEra = 1;

        public override DateTime AddMonths(DateTime time, int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { -120000, 0x1d4c0 }));
            }
            int datePart = this.GetDatePart(time.Ticks, 0);
            int month = this.GetDatePart(time.Ticks, 2);
            int day = this.GetDatePart(time.Ticks, 3);
            int num4 = (month - 1) + months;
            if (num4 >= 0)
            {
                month = (num4 % 12) + 1;
                datePart += num4 / 12;
            }
            else
            {
                month = 12 + ((num4 + 1) % 12);
                datePart += (num4 - 11) / 12;
            }
            int daysInMonth = this.GetDaysInMonth(datePart, month);
            if (day > daysInMonth)
            {
                day = daysInMonth;
            }
            long ticks = (this.GetAbsoluteDatePersian(datePart, month, day) * 0xc92a69c000L) + (time.Ticks % 0xc92a69c000L);
            Calendar.CheckAddResult(ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return new DateTime(ticks);
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            return this.AddMonths(time, years * 12);
        }

        internal static void CheckEraRange(int era)
        {
            if ((era != 0) && (era != PersianEra))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
        }

        internal static void CheckTicksRange(long ticks)
        {
            if ((ticks < minDate.Ticks) || (ticks > maxDate.Ticks))
            {
                throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_CalendarRange"), new object[] { minDate, maxDate }));
            }
        }

        internal static void CheckYearMonthRange(int year, int month, int era)
        {
            CheckYearRange(year, era);
            if ((year == 0x24a2) && (month > 10))
            {
                throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 10 }));
            }
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
        }

        internal static void CheckYearRange(int year, int era)
        {
            CheckEraRange(era);
            if ((year < 1) || (year > 0x24a2))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x24a2 }));
            }
        }

        private long DaysUpToPersianYear(int PersianYear)
        {
            int num2 = (PersianYear - 1) / 0x21;
            int year = (PersianYear - 1) % 0x21;
            long num = (num2 * 0x2f15L) + 0x3764eL;
            while (year > 0)
            {
                num += 0x16dL;
                if (this.IsLeapYear(year, 0))
                {
                    num += 1L;
                }
                year--;
            }
            return num;
        }

        private long GetAbsoluteDatePersian(int year, int month, int day)
        {
            if (((year < 1) || (year > 0x24a2)) || ((month < 1) || (month > 12)))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
            }
            return (((this.DaysUpToPersianYear(year) + DaysToMonth[month - 1]) + day) - 1L);
        }

        internal int GetDatePart(long ticks, int part)
        {
            CheckTicksRange(ticks);
            long num4 = (ticks / 0xc92a69c000L) + 1L;
            int persianYear = ((int) (((num4 - 0x3764eL) * 0x21L) / 0x2f15L)) + 1;
            long num5 = this.DaysUpToPersianYear(persianYear);
            long daysInYear = this.GetDaysInYear(persianYear, 0);
            if (num4 < num5)
            {
                num5 -= daysInYear;
                persianYear--;
            }
            else if (num4 == num5)
            {
                persianYear--;
                num5 -= this.GetDaysInYear(persianYear, 0);
            }
            else if (num4 > (num5 + daysInYear))
            {
                num5 += daysInYear;
                persianYear++;
            }
            if (part == 0)
            {
                return persianYear;
            }
            num4 -= num5;
            if (part == 1)
            {
                return (int) num4;
            }
            int index = 0;
            while ((index < 12) && (num4 > DaysToMonth[index]))
            {
                index++;
            }
            if (part == 2)
            {
                return index;
            }
            int num3 = ((int) num4) - DaysToMonth[index - 1];
            if (part != 3)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DateTimeParsing"));
            }
            return num3;
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
            CheckYearMonthRange(year, month, era);
            if ((month == 10) && (year == 0x24a2))
            {
                return 10;
            }
            if (month == 12)
            {
                if (!this.IsLeapYear(year, 0))
                {
                    return 0x1d;
                }
                return 30;
            }
            if (month <= 6)
            {
                return 0x1f;
            }
            return 30;
        }

        public override int GetDaysInYear(int year, int era)
        {
            CheckYearRange(year, era);
            if (year == 0x24a2)
            {
                return (DaysToMonth[9] + 10);
            }
            if (!this.IsLeapYear(year, 0))
            {
                return 0x16d;
            }
            return 0x16e;
        }

        public override int GetEra(DateTime time)
        {
            CheckTicksRange(time.Ticks);
            return PersianEra;
        }

        public override int GetLeapMonth(int year, int era)
        {
            CheckYearRange(year, era);
            return 0;
        }

        public override int GetMonth(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 2);
        }

        public override int GetMonthsInYear(int year, int era)
        {
            CheckYearRange(year, era);
            if (year == 0x24a2)
            {
                return 10;
            }
            return 12;
        }

        public override int GetYear(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 0);
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            int num = this.GetDaysInMonth(year, month, era);
            if ((day < 1) || (day > num))
            {
                throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), new object[] { num, month }));
            }
            return ((this.IsLeapYear(year, era) && (month == 12)) && (day == 30));
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            CheckYearMonthRange(year, month, era);
            return false;
        }

        public override bool IsLeapYear(int year, int era)
        {
            CheckYearRange(year, era);
            return (LeapYears33[year % 0x21] == 1);
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            int num = this.GetDaysInMonth(year, month, era);
            if ((day < 1) || (day > num))
            {
                throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), new object[] { num, month }));
            }
            long num2 = this.GetAbsoluteDatePersian(year, month, day);
            if (num2 < 0L)
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
            }
            return new DateTime((num2 * 0xc92a69c000L) + Calendar.TimeToTicks(hour, minute, second, millisecond));
        }

        public override int ToFourDigitYear(int year)
        {
            if (year < 100)
            {
                return base.ToFourDigitYear(year);
            }
            if (year > 0x24a2)
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x24a2 }));
            }
            return year;
        }

        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.SolarCalendar;
            }
        }

        internal override int BaseCalendarID
        {
            get
            {
                return 1;
            }
        }

        public override int[] Eras
        {
            get
            {
                return new int[] { PersianEra };
            }
        }

        internal override int ID
        {
            get
            {
                return 0x16;
            }
        }

        public override DateTime MaxSupportedDateTime
        {
            get
            {
                return maxDate;
            }
        }

        public override DateTime MinSupportedDateTime
        {
            get
            {
                return minDate;
            }
        }

        public override int TwoDigitYearMax
        {
            get
            {
                if (base.twoDigitYearMax == -1)
                {
                    base.twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(this.ID, 0x582);
                }
                return base.twoDigitYearMax;
            }
            set
            {
                base.VerifyWritable();
                if ((value < 0x63) || (value > 0x24a2))
                {
                    throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x63, 0x24a2 }));
                }
                base.twoDigitYearMax = value;
            }
        }
    }
}

