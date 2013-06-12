namespace System.Globalization
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class HijriCalendar : Calendar
    {
        internal static readonly DateTime calendarMaxValue = DateTime.MaxValue;
        internal static readonly DateTime calendarMinValue = new DateTime(0x26e, 7, 0x12);
        internal const int DatePartDay = 3;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartYear = 0;
        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 0x5ab;
        private const string HijriAdvanceRegKeyEntry = "AddHijriDate";
        public static readonly int HijriEra = 1;
        internal static readonly int[] HijriMonthDays = new int[] { 0, 30, 0x3b, 0x59, 0x76, 0x94, 0xb1, 0xcf, 0xec, 0x10a, 0x127, 0x145, 0x163 };
        private const string InternationalRegKey = @"Control Panel\International";
        private int m_HijriAdvance = -2147483648;
        internal const int MaxAdvancedHijri = 2;
        internal const int MaxCalendarDay = 3;
        internal const int MaxCalendarMonth = 4;
        internal const int MaxCalendarYear = 0x25c2;
        internal const int MinAdvancedHijri = -2;

        public override DateTime AddMonths(DateTime time, int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { -120000, 0x1d4c0 }));
            }
            int datePart = this.GetDatePart(time.Ticks, 0);
            int month = this.GetDatePart(time.Ticks, 2);
            int d = this.GetDatePart(time.Ticks, 3);
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
            if (d > daysInMonth)
            {
                d = daysInMonth;
            }
            long ticks = (this.GetAbsoluteDateHijri(datePart, month, d) * 0xc92a69c000L) + (time.Ticks % 0xc92a69c000L);
            Calendar.CheckAddResult(ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return new DateTime(ticks);
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            return this.AddMonths(time, years * 12);
        }

        internal static void CheckEraRange(int era)
        {
            if ((era != 0) && (era != HijriEra))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
        }

        internal static void CheckTicksRange(long ticks)
        {
            if ((ticks < calendarMinValue.Ticks) || (ticks > calendarMaxValue.Ticks))
            {
                throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_CalendarRange"), new object[] { calendarMinValue, calendarMaxValue }));
            }
        }

        internal static void CheckYearMonthRange(int year, int month, int era)
        {
            CheckYearRange(year, era);
            if ((year == 0x25c2) && (month > 4))
            {
                throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 4 }));
            }
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
        }

        internal static void CheckYearRange(int year, int era)
        {
            CheckEraRange(era);
            if ((year < 1) || (year > 0x25c2))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x25c2 }));
            }
        }

        private long DaysUpToHijriYear(int HijriYear)
        {
            int num2 = ((HijriYear - 1) / 30) * 30;
            int year = (HijriYear - num2) - 1;
            long num = ((num2 * 0x2987L) / 30L) + 0x376c5L;
            while (year > 0)
            {
                num += 0x162 + (this.IsLeapYear(year, 0) ? 1 : 0);
                year--;
            }
            return num;
        }

        private long GetAbsoluteDateHijri(int y, int m, int d)
        {
            return ((((this.DaysUpToHijriYear(y) + HijriMonthDays[m - 1]) + d) - 1L) - this.HijriAdjustment);
        }

        [SecurityCritical]
        private static int GetAdvanceHijriDate()
        {
            int num = 0;
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.InternalOpenSubKey(@"Control Panel\International", false);
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }
            catch (ArgumentException)
            {
                return 0;
            }
            if (key != null)
            {
                try
                {
                    object obj2 = key.InternalGetValue("AddHijriDate", null, false, false);
                    if (obj2 == null)
                    {
                        return 0;
                    }
                    string strA = obj2.ToString();
                    if (string.Compare(strA, 0, "AddHijriDate", 0, "AddHijriDate".Length, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return num;
                    }
                    if (strA.Length == "AddHijriDate".Length)
                    {
                        return -1;
                    }
                    strA = strA.Substring("AddHijriDate".Length);
                    try
                    {
                        int num2 = int.Parse(strA.ToString(), CultureInfo.InvariantCulture);
                        if ((num2 >= -2) && (num2 <= 2))
                        {
                            num = num2;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (FormatException)
                    {
                    }
                    catch (OverflowException)
                    {
                    }
                }
                finally
                {
                    key.Close();
                }
            }
            return num;
        }

        internal virtual int GetDatePart(long ticks, int part)
        {
            CheckTicksRange(ticks);
            long num4 = (ticks / 0xc92a69c000L) + 1L;
            num4 += this.HijriAdjustment;
            int hijriYear = ((int) (((num4 - 0x376c5L) * 30L) / 0x2987L)) + 1;
            long num5 = this.DaysUpToHijriYear(hijriYear);
            long daysInYear = this.GetDaysInYear(hijriYear, 0);
            if (num4 < num5)
            {
                num5 -= daysInYear;
                hijriYear--;
            }
            else if (num4 == num5)
            {
                hijriYear--;
                num5 -= this.GetDaysInYear(hijriYear, 0);
            }
            else if (num4 > (num5 + daysInYear))
            {
                num5 += daysInYear;
                hijriYear++;
            }
            if (part == 0)
            {
                return hijriYear;
            }
            int num2 = 1;
            num4 -= num5;
            if (part == 1)
            {
                return (int) num4;
            }
            while (true)
            {
                if ((num2 > 12) || (num4 <= HijriMonthDays[num2 - 1]))
                {
                    num2--;
                    if (part == 2)
                    {
                        return num2;
                    }
                    int num3 = ((int) num4) - HijriMonthDays[num2 - 1];
                    if (part != 3)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DateTimeParsing"));
                    }
                    return num3;
                }
                num2++;
            }
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
            if (month == 12)
            {
                if (!this.IsLeapYear(year, 0))
                {
                    return 0x1d;
                }
                return 30;
            }
            if ((month % 2) != 1)
            {
                return 0x1d;
            }
            return 30;
        }

        public override int GetDaysInYear(int year, int era)
        {
            CheckYearRange(year, era);
            if (!this.IsLeapYear(year, 0))
            {
                return 0x162;
            }
            return 0x163;
        }

        public override int GetEra(DateTime time)
        {
            CheckTicksRange(time.Ticks);
            return HijriEra;
        }

        [ComVisible(false)]
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
            return ((((year * 11) + 14) % 30) < 11);
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            int num = this.GetDaysInMonth(year, month, era);
            if ((day < 1) || (day > num))
            {
                throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Day"), new object[] { num, month }));
            }
            long num2 = this.GetAbsoluteDateHijri(year, month, day);
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
            if (year > 0x25c2)
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, 0x25c2 }));
            }
            return year;
        }

        [ComVisible(false)]
        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.LunarCalendar;
            }
        }

        public override int[] Eras
        {
            get
            {
                return new int[] { HijriEra };
            }
        }

        public int HijriAdjustment
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_HijriAdvance == -2147483648)
                {
                    this.m_HijriAdvance = GetAdvanceHijriDate();
                }
                return this.m_HijriAdvance;
            }
            set
            {
                if ((value < -2) || (value > 2))
                {
                    throw new ArgumentOutOfRangeException("HijriAdjustment", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), new object[] { -2, 2 }));
                }
                base.VerifyWritable();
                this.m_HijriAdvance = value;
            }
        }

        internal override int ID
        {
            get
            {
                return 6;
            }
        }

        [ComVisible(false)]
        public override DateTime MaxSupportedDateTime
        {
            get
            {
                return calendarMaxValue;
            }
        }

        [ComVisible(false)]
        public override DateTime MinSupportedDateTime
        {
            get
            {
                return calendarMinValue;
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
                base.VerifyWritable();
                if ((value < 0x63) || (value > 0x25c2))
                {
                    throw new ArgumentOutOfRangeException("value", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x63, 0x25c2 }));
                }
                base.twoDigitYearMax = value;
            }
        }
    }
}

