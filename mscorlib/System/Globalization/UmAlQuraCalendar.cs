namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable]
    public class UmAlQuraCalendar : Calendar
    {
        internal const int DateCycle = 30;
        internal const int DatePartDay = 3;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartYear = 0;
        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 0x5ab;
        internal static short[] gmonth = new short[] { 0x1f, 0x1f, 0x1c, 0x1f, 30, 0x1f, 30, 0x1f, 0x1f, 30, 0x1f, 30, 0x1f, 0x1f };
        private static readonly DateMapping[] HijriYearInfo = new DateMapping[] { 
            new DateMapping(0x2ea, 0x76c, 4, 30), new DateMapping(0x6e9, 0x76d, 4, 0x13), new DateMapping(0xed2, 0x76e, 4, 9), new DateMapping(0xea4, 0x76f, 3, 30), new DateMapping(0xd4a, 0x770, 3, 0x12), new DateMapping(0xa96, 0x771, 3, 7), new DateMapping(0x536, 0x772, 2, 0x18), new DateMapping(0xab5, 0x773, 2, 13), new DateMapping(0xdaa, 0x774, 2, 3), new DateMapping(0xba4, 0x775, 1, 0x17), new DateMapping(0xb49, 0x776, 1, 12), new DateMapping(0xa93, 0x777, 1, 1), new DateMapping(0x52b, 0x777, 12, 0x15), new DateMapping(0xa57, 0x778, 12, 9), new DateMapping(0x4b6, 0x779, 11, 0x1d), new DateMapping(0xab5, 0x77a, 11, 0x12), 
            new DateMapping(0x5aa, 0x77b, 11, 8), new DateMapping(0xd55, 0x77c, 10, 0x1b), new DateMapping(0xd2a, 0x77d, 10, 0x11), new DateMapping(0xa56, 0x77e, 10, 6), new DateMapping(0x4ae, 0x77f, 9, 0x19), new DateMapping(0x95d, 0x780, 9, 13), new DateMapping(0x2ec, 0x781, 9, 3), new DateMapping(0x6d5, 0x782, 8, 0x17), new DateMapping(0x6aa, 0x783, 8, 13), new DateMapping(0x555, 0x784, 8, 1), new DateMapping(0x4ab, 0x785, 7, 0x15), new DateMapping(0x95b, 0x786, 7, 10), new DateMapping(0x2ba, 0x787, 6, 30), new DateMapping(0x575, 0x788, 6, 0x12), new DateMapping(0xbb2, 0x789, 6, 8), new DateMapping(0x764, 0x78a, 5, 0x1d), 
            new DateMapping(0x749, 0x78b, 5, 0x12), new DateMapping(0x655, 0x78c, 5, 6), new DateMapping(0x2ab, 0x78d, 4, 0x19), new DateMapping(0x55b, 0x78e, 4, 14), new DateMapping(0xada, 0x78f, 4, 4), new DateMapping(0x6d4, 0x790, 3, 0x18), new DateMapping(0xec9, 0x791, 3, 13), new DateMapping(0xd92, 0x792, 3, 3), new DateMapping(0xd25, 0x793, 2, 20), new DateMapping(0xa4d, 0x794, 2, 9), new DateMapping(0x2ad, 0x795, 1, 0x1c), new DateMapping(0x56d, 0x796, 1, 0x11), new DateMapping(0xb6a, 0x797, 1, 7), new DateMapping(0xb52, 0x797, 12, 0x1c), new DateMapping(0xaa5, 0x798, 12, 0x10), new DateMapping(0xa4b, 0x799, 12, 5), 
            new DateMapping(0x497, 0x79a, 11, 0x18), new DateMapping(0x937, 0x79b, 11, 13), new DateMapping(0x2b6, 0x79c, 11, 2), new DateMapping(0x575, 0x79d, 10, 0x16), new DateMapping(0xd6a, 0x79e, 10, 12), new DateMapping(0xd52, 0x79f, 10, 2), new DateMapping(0xa96, 0x7a0, 9, 20), new DateMapping(0x92d, 0x7a1, 9, 9), new DateMapping(0x25d, 0x7a2, 8, 0x1d), new DateMapping(0x4dd, 0x7a3, 8, 0x12), new DateMapping(0xada, 0x7a4, 8, 7), new DateMapping(0x5d4, 0x7a5, 7, 0x1c), new DateMapping(0xda9, 0x7a6, 7, 0x11), new DateMapping(0xd52, 0x7a7, 7, 7), new DateMapping(0xaaa, 0x7a8, 6, 0x19), new DateMapping(0x4d6, 0x7a9, 6, 14), 
            new DateMapping(0x9b6, 0x7aa, 6, 3), new DateMapping(0x374, 0x7ab, 5, 0x18), new DateMapping(0x769, 0x7ac, 5, 12), new DateMapping(0x752, 0x7ad, 5, 2), new DateMapping(0x6a5, 0x7ae, 4, 0x15), new DateMapping(0x54b, 0x7af, 4, 10), new DateMapping(0xaab, 0x7b0, 3, 0x1d), new DateMapping(0x55a, 0x7b1, 3, 0x13), new DateMapping(0xad5, 0x7b2, 3, 8), new DateMapping(0xdd2, 0x7b3, 2, 0x1a), new DateMapping(0xda4, 0x7b4, 2, 0x10), new DateMapping(0xd49, 0x7b5, 2, 4), new DateMapping(0xa95, 0x7b6, 1, 0x18), new DateMapping(0x52d, 0x7b7, 1, 13), new DateMapping(0xa5d, 0x7b8, 1, 2), new DateMapping(0x55a, 0x7b8, 12, 0x16), 
            new DateMapping(0xad5, 0x7b9, 12, 11), new DateMapping(0x6aa, 0x7ba, 12, 1), new DateMapping(0x695, 0x7bb, 11, 20), new DateMapping(0x52b, 0x7bc, 11, 8), new DateMapping(0xa57, 0x7bd, 10, 0x1c), new DateMapping(0x4ae, 0x7be, 10, 0x12), new DateMapping(0x976, 0x7bf, 10, 7), new DateMapping(0x56c, 0x7c0, 9, 0x1a), new DateMapping(0xb55, 0x7c1, 9, 15), new DateMapping(0xaaa, 0x7c2, 9, 5), new DateMapping(0xa55, 0x7c3, 8, 0x19), new DateMapping(0x4ad, 0x7c4, 8, 13), new DateMapping(0x95d, 0x7c5, 8, 2), new DateMapping(730, 0x7c6, 7, 0x17), new DateMapping(0x5d9, 0x7c7, 7, 12), new DateMapping(0xdb2, 0x7c8, 7, 1), 
            new DateMapping(0xba4, 0x7c9, 6, 0x15), new DateMapping(0xb4a, 0x7ca, 6, 10), new DateMapping(0xa55, 0x7cb, 5, 30), new DateMapping(0x2b5, 0x7cc, 5, 0x12), new DateMapping(0x575, 0x7cd, 5, 7), new DateMapping(0xb6a, 0x7ce, 4, 0x1b), new DateMapping(0xbd2, 0x7cf, 4, 0x11), new DateMapping(0xbc4, 0x7d0, 4, 6), new DateMapping(0xb89, 0x7d1, 3, 0x1a), new DateMapping(0xa95, 0x7d2, 3, 15), new DateMapping(0x52d, 0x7d3, 3, 4), new DateMapping(0x5ad, 0x7d4, 2, 0x15), new DateMapping(0xb6a, 0x7d5, 2, 10), new DateMapping(0x6d4, 0x7d6, 1, 0x1f), new DateMapping(0xdc9, 0x7d7, 1, 20), new DateMapping(0xd92, 0x7d8, 1, 10), 
            new DateMapping(0xaa6, 0x7d8, 12, 0x1d), new DateMapping(0x956, 0x7d9, 12, 0x12), new DateMapping(0x2ae, 0x7da, 12, 7), new DateMapping(0x56d, 0x7db, 11, 0x1a), new DateMapping(0x36a, 0x7dc, 11, 15), new DateMapping(0xb55, 0x7dd, 11, 4), new DateMapping(0xaaa, 0x7de, 10, 0x19), new DateMapping(0x94d, 0x7df, 10, 14), new DateMapping(0x49d, 0x7e0, 10, 2), new DateMapping(0x95d, 0x7e1, 9, 0x15), new DateMapping(0x2ba, 0x7e2, 9, 11), new DateMapping(0x5b5, 0x7e3, 8, 0x1f), new DateMapping(0x5aa, 0x7e4, 8, 20), new DateMapping(0xd55, 0x7e5, 8, 9), new DateMapping(0xa9a, 0x7e6, 7, 30), new DateMapping(0x92e, 0x7e7, 7, 0x13), 
            new DateMapping(0x26e, 0x7e8, 7, 7), new DateMapping(0x55d, 0x7e9, 6, 0x1a), new DateMapping(0xada, 0x7ea, 6, 0x10), new DateMapping(0x6d4, 0x7eb, 6, 6), new DateMapping(0x6a5, 0x7ec, 5, 0x19), new DateMapping(0, 0x7ed, 5, 14)
         };
        internal const int MaxCalendarYear = 0x5aa;
        internal static DateTime maxDate;
        internal const int MinCalendarYear = 0x526;
        internal static DateTime minDate = new DateTime(0x76c, 4, 30);
        public const int UmAlQuraEra = 1;

        static UmAlQuraCalendar()
        {
            DateTime time = new DateTime(0x7ed, 5, 13, 0x17, 0x3b, 0x3b, 0x3e7);
            maxDate = new DateTime(time.Ticks + 0x270fL);
        }

        public override DateTime AddMonths(DateTime time, int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { -120000, 0x1d4c0 }));
            }
            int datePart = this.GetDatePart(time, 0);
            int month = this.GetDatePart(time, 2);
            int day = this.GetDatePart(time, 3);
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
            if (day > 0x1d)
            {
                int daysInMonth = this.GetDaysInMonth(datePart, month);
                if (day > daysInMonth)
                {
                    day = daysInMonth;
                }
            }
            CheckYearRange(datePart, 1);
            DateTime time2 = new DateTime((GetAbsoluteDateUmAlQura(datePart, month, day) * 0xc92a69c000L) + (time.Ticks % 0xc92a69c000L));
            Calendar.CheckAddResult(time2.Ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return time2;
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            return this.AddMonths(time, years * 12);
        }

        internal static void CheckEraRange(int era)
        {
            if ((era != 0) && (era != 1))
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
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
        }

        internal static void CheckYearRange(int year, int era)
        {
            CheckEraRange(era);
            if ((year < 0x526) || (year > 0x5aa))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x526, 0x5aa }));
            }
        }

        private static void ConvertGregorianToHijri(DateTime time, ref int HijriYear, ref int HijriMonth, ref int HijriDay)
        {
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int index = ((int) ((time.Ticks - minDate.Ticks) / 0xc92a69c000L)) / 0x163;
            while (time.CompareTo(HijriYearInfo[++index].GregorianDate) > 0)
            {
            }
            if (time.CompareTo(HijriYearInfo[index].GregorianDate) != 0)
            {
                index--;
            }
            TimeSpan span = time.Subtract(HijriYearInfo[index].GregorianDate);
            num5 = index + 0x526;
            num6 = 1;
            num7 = 1;
            double totalDays = span.TotalDays;
            int hijriMonthsLengthFlags = HijriYearInfo[index].HijriMonthsLengthFlags;
            int num3 = 0x1d + (hijriMonthsLengthFlags & 1);
            while (totalDays >= num3)
            {
                totalDays -= num3;
                hijriMonthsLengthFlags = hijriMonthsLengthFlags >> 1;
                num3 = 0x1d + (hijriMonthsLengthFlags & 1);
                num6++;
            }
            num7 += (int) totalDays;
            HijriDay = num7;
            HijriMonth = num6;
            HijriYear = num5;
        }

        private static void ConvertHijriToGregorian(int HijriYear, int HijriMonth, int HijriDay, ref int yg, ref int mg, ref int dg)
        {
            int num3 = HijriDay - 1;
            int index = HijriYear - 0x526;
            DateTime gregorianDate = HijriYearInfo[index].GregorianDate;
            int hijriMonthsLengthFlags = HijriYearInfo[index].HijriMonthsLengthFlags;
            for (int i = 1; i < HijriMonth; i++)
            {
                num3 = (num3 + 0x1d) + (hijriMonthsLengthFlags & 1);
                hijriMonthsLengthFlags = hijriMonthsLengthFlags >> 1;
            }
            gregorianDate = gregorianDate.AddDays((double) num3);
            yg = gregorianDate.Year;
            mg = gregorianDate.Month;
            dg = gregorianDate.Day;
        }

        private static long GetAbsoluteDateUmAlQura(int year, int month, int day)
        {
            int yg = 0;
            int mg = 0;
            int dg = 0;
            ConvertHijriToGregorian(year, month, day, ref yg, ref mg, ref dg);
            return GregorianCalendar.GetAbsoluteDate(yg, mg, dg);
        }

        internal virtual int GetDatePart(DateTime time, int part)
        {
            int hijriYear = 0;
            int hijriMonth = 0;
            int hijriDay = 0;
            CheckTicksRange(time.Ticks);
            ConvertGregorianToHijri(time, ref hijriYear, ref hijriMonth, ref hijriDay);
            if (part == 0)
            {
                return hijriYear;
            }
            if (part == 2)
            {
                return hijriMonth;
            }
            if (part == 3)
            {
                return hijriDay;
            }
            if (part != 1)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DateTimeParsing"));
            }
            return (int) ((GetAbsoluteDateUmAlQura(hijriYear, hijriMonth, hijriDay) - GetAbsoluteDateUmAlQura(hijriYear, 1, 1)) + 1L);
        }

        public override int GetDayOfMonth(DateTime time)
        {
            return this.GetDatePart(time, 3);
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            return (DayOfWeek) (((int) ((time.Ticks / 0xc92a69c000L) + 1L)) % 7);
        }

        public override int GetDayOfYear(DateTime time)
        {
            return this.GetDatePart(time, 1);
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            CheckYearMonthRange(year, month, era);
            if ((HijriYearInfo[year - 0x526].HijriMonthsLengthFlags & (((int) 1) << (month - 1))) == 0)
            {
                return 0x1d;
            }
            return 30;
        }

        public override int GetDaysInYear(int year, int era)
        {
            CheckYearRange(year, era);
            return RealGetDaysInYear(year);
        }

        public override int GetEra(DateTime time)
        {
            CheckTicksRange(time.Ticks);
            return 1;
        }

        public override int GetLeapMonth(int year, int era)
        {
            CheckYearRange(year, era);
            return 0;
        }

        public override int GetMonth(DateTime time)
        {
            return this.GetDatePart(time, 2);
        }

        public override int GetMonthsInYear(int year, int era)
        {
            CheckYearRange(year, era);
            return 12;
        }

        public override int GetYear(DateTime time)
        {
            return this.GetDatePart(time, 0);
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            if ((day >= 1) && (day <= 0x1d))
            {
                CheckYearMonthRange(year, month, era);
                return false;
            }
            int num = this.GetDaysInMonth(year, month, era);
            if ((day < 1) || (day > num))
            {
                throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), new object[] { num, month }));
            }
            return false;
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            CheckYearMonthRange(year, month, era);
            return false;
        }

        public override bool IsLeapYear(int year, int era)
        {
            CheckYearRange(year, era);
            return (RealGetDaysInYear(year) == 0x163);
        }

        internal static int RealGetDaysInYear(int year)
        {
            int num = 0;
            int hijriMonthsLengthFlags = HijriYearInfo[year - 0x526].HijriMonthsLengthFlags;
            for (int i = 1; i <= 12; i++)
            {
                num = (num + 0x1d) + (hijriMonthsLengthFlags & 1);
                hijriMonthsLengthFlags = hijriMonthsLengthFlags >> 1;
            }
            return num;
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            if ((day >= 1) && (day <= 0x1d))
            {
                CheckYearMonthRange(year, month, era);
            }
            else
            {
                int num = this.GetDaysInMonth(year, month, era);
                if ((day < 1) || (day > num))
                {
                    throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), new object[] { num, month }));
                }
            }
            long num2 = GetAbsoluteDateUmAlQura(year, month, day);
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
            if ((year < 0x526) || (year > 0x5aa))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x526, 0x5aa }));
            }
            return year;
        }

        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.LunarCalendar;
            }
        }

        internal override int BaseCalendarID
        {
            get
            {
                return 6;
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
                return 0x17;
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
                    base.twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(this.ID, 0x5ab);
                }
                return base.twoDigitYearMax;
            }
            set
            {
                if ((value != 0x63) && ((value < 0x526) || (value > 0x5aa)))
                {
                    throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x526, 0x5aa }));
                }
                base.VerifyWritable();
                base.twoDigitYearMax = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DateMapping
        {
            internal int HijriMonthsLengthFlags;
            internal DateTime GregorianDate;
            internal DateMapping(int MonthsLengthFlags, int GYear, int GMonth, int GDay)
            {
                this.HijriMonthsLengthFlags = MonthsLengthFlags;
                this.GregorianDate = new DateTime(GYear, GMonth, GDay);
            }
        }
    }
}

