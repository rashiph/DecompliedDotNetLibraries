namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class HebrewCalendar : Calendar
    {
        internal static readonly DateTime calendarMaxValue;
        internal static readonly DateTime calendarMinValue = new DateTime(0x62f, 1, 1);
        internal const int DatePartDay = 3;
        internal const int DatePartDayOfWeek = 4;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartYear = 0;
        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 0x169e;
        private const int FirstGregorianTableYear = 0x62f;
        public static readonly int HebrewEra = 1;
        private static readonly int[] HebrewTable = new int[] { 
            7, 3, 0x11, 3, 0, 4, 11, 2, 0x15, 6, 1, 3, 13, 2, 0x19, 4, 
            5, 3, 0x10, 2, 0x1b, 6, 9, 1, 20, 2, 0, 6, 11, 3, 0x17, 4, 
            4, 2, 14, 3, 0x1b, 4, 8, 2, 0x12, 3, 0x1c, 6, 11, 1, 0x16, 5, 
            2, 3, 12, 3, 0x19, 4, 6, 2, 0x10, 3, 0x1a, 6, 8, 2, 20, 1, 
            0, 6, 11, 2, 0x18, 4, 4, 3, 15, 2, 0x19, 6, 8, 1, 0x13, 2, 
            0x1d, 6, 9, 3, 0x16, 4, 3, 2, 13, 3, 0x19, 4, 6, 3, 0x11, 2, 
            0x1b, 6, 7, 3, 0x13, 2, 0x1f, 4, 11, 3, 0x17, 4, 5, 2, 15, 3, 
            0x19, 6, 6, 2, 0x13, 1, 0x1d, 6, 10, 2, 0x16, 4, 3, 3, 14, 2, 
            0x18, 6, 6, 1, 0x11, 3, 0x1c, 5, 8, 3, 20, 1, 0x20, 5, 12, 3, 
            0x16, 6, 4, 1, 0x10, 2, 0x1a, 6, 6, 3, 0x11, 2, 0, 4, 10, 3, 
            0x16, 4, 3, 2, 14, 3, 0x18, 6, 5, 2, 0x11, 1, 0x1c, 6, 9, 2, 
            0x13, 3, 0x1f, 4, 13, 2, 0x17, 6, 3, 3, 15, 1, 0x1b, 5, 7, 3, 
            0x11, 3, 0x1d, 4, 11, 2, 0x15, 6, 3, 1, 14, 2, 0x19, 6, 5, 3, 
            0x10, 2, 0x1c, 4, 9, 3, 20, 2, 0, 6, 12, 1, 0x17, 6, 4, 2, 
            14, 3, 0x1a, 4, 8, 2, 0x12, 3, 0, 4, 10, 3, 0x15, 5, 1, 3, 
            13, 1, 0x18, 5, 5, 3, 15, 3, 0x1b, 4, 8, 2, 0x13, 3, 0x1d, 6, 
            10, 2, 0x16, 4, 3, 3, 14, 2, 0x1a, 4, 6, 3, 0x12, 2, 0x1c, 6, 
            10, 1, 20, 6, 2, 2, 12, 3, 0x18, 4, 5, 2, 0x10, 3, 0x1c, 4, 
            8, 3, 0x13, 2, 0, 6, 12, 1, 0x17, 5, 3, 3, 14, 3, 0x1a, 4, 
            7, 2, 0x11, 3, 0x1c, 6, 9, 2, 0x15, 4, 1, 3, 13, 2, 0x19, 4, 
            5, 3, 0x10, 2, 0x1b, 6, 9, 1, 0x13, 3, 0, 5, 11, 3, 0x17, 4, 
            4, 2, 14, 3, 0x19, 6, 7, 1, 0x12, 2, 0x1c, 6, 9, 3, 0x15, 4, 
            2, 2, 12, 3, 0x19, 4, 6, 2, 0x10, 3, 0x1a, 6, 8, 2, 20, 1, 
            0, 6, 11, 2, 0x16, 6, 4, 1, 15, 2, 0x19, 6, 6, 3, 0x12, 1, 
            0x1d, 5, 9, 3, 0x16, 4, 2, 3, 13, 2, 0x17, 6, 4, 3, 15, 2, 
            0x1b, 4, 7, 3, 0x13, 2, 0x1f, 4, 11, 3, 0x15, 6, 3, 2, 15, 1, 
            0x19, 6, 6, 2, 0x11, 3, 0x1d, 4, 10, 2, 20, 6, 3, 1, 13, 3, 
            0x18, 5, 4, 3, 0x10, 1, 0x1b, 5, 7, 3, 0x11, 3, 0, 4, 11, 2, 
            0x15, 6, 1, 3, 13, 2, 0x19, 4, 5, 3, 0x10, 2, 0x1d, 4, 9, 3, 
            0x13, 6, 30, 2, 13, 1, 0x17, 6, 4, 2, 14, 3, 0x1b, 4, 8, 2, 
            0x12, 3, 0, 4, 11, 3, 0x16, 5, 2, 3, 14, 1, 0x1a, 5, 6, 3, 
            0x10, 3, 0x1c, 4, 10, 2, 20, 6, 30, 3, 11, 2, 0x18, 4, 4, 3, 
            15, 2, 0x19, 6, 8, 1, 0x13, 2, 0x1d, 6, 9, 3, 0x16, 4, 3, 2, 
            13, 3, 0x19, 4, 7, 2, 0x11, 3, 0x1b, 6, 9, 1, 0x15, 5, 1, 3, 
            11, 3, 0x17, 4, 5, 2, 15, 3, 0x19, 6, 6, 2, 0x13, 1, 0x1d, 6, 
            10, 2, 0x16, 4, 3, 3, 14, 2, 0x18, 6, 6, 1, 0x12, 2, 0x1c, 6, 
            8, 3, 20, 4, 2, 2, 12, 3, 0x18, 4, 4, 3, 0x10, 2, 0x1a, 6, 
            6, 3, 0x11, 2, 0, 4, 10, 3, 0x16, 4, 3, 2, 14, 3, 0x18, 6, 
            5, 2, 0x11, 1, 0x1c, 6, 9, 2, 0x15, 4, 1, 3, 13, 2, 0x17, 6, 
            5, 1, 15, 3, 0x1b, 5, 7, 3, 0x13, 1, 0, 5, 10, 3, 0x16, 4, 
            2, 3, 13, 2, 0x18, 6, 4, 3, 15, 2, 0x1b, 4, 8, 3, 20, 4, 
            1, 2, 11, 3, 0x16, 6, 3, 2, 15, 1, 0x19, 6, 7, 2, 0x11, 3, 
            0x1d, 4, 10, 2, 0x15, 6, 1, 3, 13, 1, 0x18, 5, 5, 3, 15, 3, 
            0x1b, 4, 8, 2, 0x13, 6, 1, 1, 12, 2, 0x16, 6, 3, 3, 14, 2, 
            0x1a, 4, 6, 3, 0x12, 2, 0x1c, 6, 10, 1, 20, 6, 2, 2, 12, 3, 
            0x18, 4, 5, 2, 0x10, 3, 0x1c, 4, 9, 2, 0x13, 6, 30, 3, 12, 1, 
            0x17, 5, 3, 3, 14, 3, 0x1a, 4, 7, 2, 0x11, 3, 0x1c, 6, 9, 2, 
            0x15, 4, 1, 3, 13, 2, 0x19, 4, 5, 3, 0x10, 2, 0x1b, 6, 9, 1, 
            0x13, 6, 30, 2, 11, 3, 0x17, 4, 4, 2, 14, 3, 0x1b, 4, 7, 3, 
            0x12, 2, 0x1c, 6, 11, 1, 0x16, 5, 2, 3, 12, 3, 0x19, 4, 6, 2, 
            0x10, 3, 0x1a, 6, 8, 2, 20, 4, 30, 3, 11, 2, 0x18, 4, 4, 3, 
            15, 2, 0x19, 6, 8, 1, 0x12, 3, 0x1d, 5, 9, 3, 0x16, 4, 3, 2, 
            13, 3, 0x17, 6, 6, 1, 0x11, 2, 0x1b, 6, 7, 3, 20, 4, 1, 2, 
            11, 3, 0x17, 4, 5, 2, 15, 3, 0x19, 6, 6, 2, 0x13, 1, 0x1d, 6, 
            10, 2, 20, 6, 3, 1, 14, 2, 0x18, 6, 4, 3, 0x11, 1, 0x1c, 5, 
            8, 3, 20, 4, 1, 3, 12, 2, 0x16, 6, 2, 3, 14, 2, 0x1a, 4, 
            6, 3, 0x11, 2, 0, 4, 10, 3, 20, 6, 1, 2, 14, 1, 0x18, 6, 
            5, 2, 15, 3, 0x1c, 4, 9, 2, 0x13, 6, 1, 1, 12, 3, 0x17, 5, 
            3, 3, 15, 1, 0x1b, 5, 7, 3, 0x11, 3, 0x1d, 4, 11, 2, 0x15, 6, 
            1, 3, 12, 2, 0x19, 4, 5, 3, 0x10, 2, 0x1c, 4, 9, 3, 0x13, 6, 
            30, 2, 12, 1, 0x17, 6, 4, 2, 14, 3, 0x1a, 4, 8, 2, 0x12, 3, 
            0, 4, 10, 3, 0x16, 5, 2, 3, 14, 1, 0x19, 5, 6, 3, 0x10, 3, 
            0x1c, 4, 9, 2, 20, 6, 30, 3, 11, 2, 0x17, 4, 4, 3, 15, 2, 
            0x1b, 4, 7, 3, 0x13, 2, 0x1d, 6, 11, 1, 0x15, 6, 3, 2, 13, 3, 
            0x19, 4, 6, 2, 0x11, 3, 0x1b, 6, 9, 1, 20, 5, 30, 3, 10, 3, 
            0x16, 4, 3, 2, 14, 3, 0x18, 6, 5, 2, 0x11, 1, 0x1c, 6, 9, 2, 
            0x15, 4, 1, 3, 13, 2, 0x17, 6, 5, 1, 0x10, 2, 0x1b, 6, 7, 3, 
            0x13, 4, 30, 2, 11, 3, 0x17, 4, 3, 3, 14, 2, 0x19, 6, 5, 3, 
            0x10, 2, 0x1c, 4, 9, 3, 0x15, 4, 2, 2, 12, 3, 0x17, 6, 4, 2, 
            0x10, 1, 0x1a, 6, 8, 2, 20, 4, 30, 3, 11, 2, 0x16, 6, 4, 1, 
            14, 3, 0x19, 5, 6, 3, 0x12, 1, 0x1d, 5, 9, 3, 0x16, 4, 2, 3, 
            13, 2, 0x17, 6, 4, 3, 15, 2, 0x1b, 4, 7, 3, 20, 4, 1, 2, 
            11, 3, 0x15, 6, 3, 2, 15, 1, 0x19, 6, 6, 2, 0x11, 3, 0x1d, 4, 
            10, 2, 20, 6, 3, 1, 13, 3, 0x18, 5, 4, 3, 0x11, 1, 0x1c, 5, 
            8, 3, 0x12, 6, 1, 1, 12, 2, 0x16, 6, 2, 3, 14, 2, 0x1a, 4, 
            6, 3, 0x11, 2, 0x1c, 6, 10, 1, 20, 6, 1, 2, 12, 3, 0x18, 4, 
            5, 2, 15, 3, 0x1c, 4, 9, 2, 0x13, 6, 0x21, 3, 12, 1, 0x17, 5, 
            3, 3, 13, 3, 0x19, 4, 6, 2, 0x10, 3, 0x1a, 6, 8, 2, 20, 4, 
            30, 3, 11, 2, 0x18, 4, 4, 3, 15, 2, 0x19, 6, 8, 1, 0x12, 6, 
            0x21, 2, 9, 3, 0x16, 4, 3, 2, 13, 3, 0x19, 4, 6, 3, 0x11, 2, 
            0x1b, 6, 9, 1, 0x15, 5, 1, 3, 11, 3, 0x17, 4, 5, 2, 15, 3, 
            0x19, 6, 6, 2, 0x13, 4, 0x21, 3, 10, 2, 0x16, 4, 3, 3, 14, 2, 
            0x18, 6, 6, 1
         };
        private const int HebrewYearOf1AD = 0xeb0;
        private const int LastGregorianTableYear = 0x8bf;
        private static readonly int[,] LunarMonthLen = new int[,] { { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 30, 0x1d, 0x1d, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d, 0 }, { 0, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d, 0 }, { 0, 30, 30, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d, 0 }, { 0, 30, 0x1d, 0x1d, 0x1d, 30, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d }, { 0, 30, 0x1d, 30, 0x1d, 30, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d }, { 0, 30, 30, 30, 0x1d, 30, 30, 0x1d, 30, 0x1d, 30, 0x1d, 30, 0x1d } };
        private const int MaxHebrewYear = 0x176f;
        private const int MinHebrewYear = 0x14df;
        private const int TABLESIZE = 0x290;

        static HebrewCalendar()
        {
            DateTime time = new DateTime(0x8bf, 9, 0x1d, 0x17, 0x3b, 0x3b, 0x3e7);
            calendarMaxValue = new DateTime(time.Ticks + 0x270fL);
        }

        public override DateTime AddMonths(DateTime time, int months)
        {
            DateTime time2;
            try
            {
                int num4;
                int num5;
                int datePart = this.GetDatePart(time.Ticks, 0);
                int num2 = this.GetDatePart(time.Ticks, 2);
                int day = this.GetDatePart(time.Ticks, 3);
                if (months >= 0)
                {
                    num5 = num2 + months;
                    while (num5 > (num4 = this.GetMonthsInYear(datePart, 0)))
                    {
                        datePart++;
                        num5 -= num4;
                    }
                }
                else
                {
                    num5 = num2 + months;
                    if (num5 <= 0)
                    {
                        months = -months;
                        months -= num2;
                        datePart--;
                        while (months > (num4 = this.GetMonthsInYear(datePart, 0)))
                        {
                            datePart--;
                            months -= num4;
                        }
                        num5 = this.GetMonthsInYear(datePart, 0) - months;
                    }
                }
                int daysInMonth = this.GetDaysInMonth(datePart, num5);
                if (day > daysInMonth)
                {
                    day = daysInMonth;
                }
                time2 = new DateTime(this.ToDateTime(datePart, num5, day, 0, 0, 0, 0).Ticks + (time.Ticks % 0xc92a69c000L));
            }
            catch (ArgumentException)
            {
                throw new ArgumentOutOfRangeException("months", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_AddValue"), new object[0]));
            }
            return time2;
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            int datePart = this.GetDatePart(time.Ticks, 0);
            int month = this.GetDatePart(time.Ticks, 2);
            int day = this.GetDatePart(time.Ticks, 3);
            datePart += years;
            CheckHebrewYearValue(datePart, 0, "years");
            int monthsInYear = this.GetMonthsInYear(datePart, 0);
            if (month > monthsInYear)
            {
                month = monthsInYear;
            }
            int daysInMonth = this.GetDaysInMonth(datePart, month);
            if (day > daysInMonth)
            {
                day = daysInMonth;
            }
            long ticks = this.ToDateTime(datePart, month, day, 0, 0, 0, 0).Ticks + (time.Ticks % 0xc92a69c000L);
            Calendar.CheckAddResult(ticks, this.MinSupportedDateTime, this.MaxSupportedDateTime);
            return new DateTime(ticks);
        }

        internal static void CheckEraRange(int era)
        {
            if ((era != 0) && (era != HebrewEra))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
        }

        private void CheckHebrewDayValue(int year, int month, int day, int era)
        {
            int num = this.GetDaysInMonth(year, month, era);
            if ((day < 1) || (day > num))
            {
                throw new ArgumentOutOfRangeException("day", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, num }));
            }
        }

        private void CheckHebrewMonthValue(int year, int month, int era)
        {
            int monthsInYear = this.GetMonthsInYear(year, era);
            if ((month < 1) || (month > monthsInYear))
            {
                throw new ArgumentOutOfRangeException("month", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, monthsInYear }));
            }
        }

        private static void CheckHebrewYearValue(int y, int era, string varName)
        {
            CheckEraRange(era);
            if ((y > 0x176f) || (y < 0x14df))
            {
                throw new ArgumentOutOfRangeException(varName, string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x14df, 0x176f }));
            }
        }

        private static void CheckTicksRange(long ticks)
        {
            if ((ticks < calendarMinValue.Ticks) || (ticks > calendarMaxValue.Ticks))
            {
                throw new ArgumentOutOfRangeException("time", string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("ArgumentOutOfRange_CalendarRange"), new object[] { calendarMinValue, calendarMaxValue }));
            }
        }

        internal virtual int GetDatePart(long ticks, int part)
        {
            CheckTicksRange(ticks);
            DateTime time = new DateTime(ticks);
            int year = time.Year;
            int month = time.Month;
            int day = time.Day;
            __DateBuffer lunarDate = new __DateBuffer {
                year = year + 0xeb0
            };
            int lunarMonthDay = GetLunarMonthDay(year, lunarDate);
            __DateBuffer result = new __DateBuffer {
                year = lunarDate.year,
                month = lunarDate.month,
                day = lunarDate.day
            };
            long num5 = GregorianCalendar.GetAbsoluteDate(year, month, day);
            if ((month != 1) || (day != 1))
            {
                long num6 = num5 - GregorianCalendar.GetAbsoluteDate(year, 1, 1);
                if ((num6 + lunarDate.day) <= LunarMonthLen[lunarMonthDay, lunarDate.month])
                {
                    result.day += (int) num6;
                    return GetResult(result, part);
                }
                result.month++;
                result.day = 1;
                num6 -= LunarMonthLen[lunarMonthDay, lunarDate.month] - lunarDate.day;
                if (num6 > 1L)
                {
                    while (num6 > LunarMonthLen[lunarMonthDay, result.month])
                    {
                        num6 -= LunarMonthLen[lunarMonthDay, result.month++];
                        if ((result.month > 13) || (LunarMonthLen[lunarMonthDay, result.month] == 0))
                        {
                            result.year++;
                            lunarMonthDay = HebrewTable[(((year + 1) - 0x62f) * 2) + 1];
                            result.month = 1;
                        }
                    }
                    result.day += (int) (num6 - 1L);
                }
            }
            return GetResult(result, part);
        }

        private static int GetDayDifference(int lunarYearType, int month1, int day1, int month2, int day2)
        {
            if (month1 == month2)
            {
                return (day1 - day2);
            }
            bool flag = month1 > month2;
            if (flag)
            {
                int num = month1;
                int num2 = day1;
                month1 = month2;
                day1 = day2;
                month2 = num;
                day2 = num2;
            }
            int num3 = LunarMonthLen[lunarYearType, month1] - day1;
            month1++;
            while (month1 < month2)
            {
                num3 += LunarMonthLen[lunarYearType, month1++];
            }
            num3 += day2;
            if (!flag)
            {
                return -num3;
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
            int year = this.GetYear(time);
            DateTime time2 = this.ToDateTime(year, 1, 1, 0, 0, 0, 0, 0);
            return (((int) ((time.Ticks - time2.Ticks) / 0xc92a69c000L)) + 1);
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            CheckEraRange(era);
            int hebrewYearType = GetHebrewYearType(year, era);
            this.CheckHebrewMonthValue(year, month, era);
            int num2 = LunarMonthLen[hebrewYearType, month];
            if (num2 == 0)
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
            return num2;
        }

        public override int GetDaysInYear(int year, int era)
        {
            CheckEraRange(era);
            int hebrewYearType = GetHebrewYearType(year, era);
            if (hebrewYearType < 4)
            {
                return (0x160 + hebrewYearType);
            }
            return (0x17e + (hebrewYearType - 3));
        }

        public override int GetEra(DateTime time)
        {
            return HebrewEra;
        }

        internal static int GetHebrewYearType(int year, int era)
        {
            CheckHebrewYearValue(year, era, "year");
            return HebrewTable[(((year - 0xeb0) - 0x62f) * 2) + 1];
        }

        public override int GetLeapMonth(int year, int era)
        {
            if (this.IsLeapYear(year, era))
            {
                return 7;
            }
            return 0;
        }

        internal static int GetLunarMonthDay(int gregorianYear, __DateBuffer lunarDate)
        {
            int index = gregorianYear - 0x62f;
            if ((index < 0) || (index > 0x290))
            {
                throw new ArgumentOutOfRangeException("gregorianYear");
            }
            index *= 2;
            lunarDate.day = HebrewTable[index];
            int num2 = HebrewTable[index + 1];
            switch (lunarDate.day)
            {
                case 30:
                    lunarDate.month = 3;
                    return num2;

                case 0x1f:
                    lunarDate.month = 5;
                    lunarDate.day = 2;
                    return num2;

                case 0x20:
                    lunarDate.month = 5;
                    lunarDate.day = 3;
                    return num2;

                case 0x21:
                    lunarDate.month = 3;
                    lunarDate.day = 0x1d;
                    return num2;

                case 0:
                    lunarDate.month = 5;
                    lunarDate.day = 1;
                    return num2;
            }
            lunarDate.month = 4;
            return num2;
        }

        public override int GetMonth(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 2);
        }

        public override int GetMonthsInYear(int year, int era)
        {
            if (!this.IsLeapYear(year, era))
            {
                return 12;
            }
            return 13;
        }

        internal static int GetResult(__DateBuffer result, int part)
        {
            switch (part)
            {
                case 0:
                    return result.year;

                case 2:
                    return result.month;

                case 3:
                    return result.day;
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DateTimeParsing"));
        }

        public override int GetYear(DateTime time)
        {
            return this.GetDatePart(time.Ticks, 0);
        }

        private static DateTime HebrewToGregorian(int hebrewYear, int hebrewMonth, int hebrewDay, int hour, int minute, int second, int millisecond)
        {
            int gregorianYear = hebrewYear - 0xeb0;
            __DateBuffer lunarDate = new __DateBuffer();
            int lunarMonthDay = GetLunarMonthDay(gregorianYear, lunarDate);
            if ((hebrewMonth == lunarDate.month) && (hebrewDay == lunarDate.day))
            {
                return new DateTime(gregorianYear, 1, 1, hour, minute, second, millisecond);
            }
            int num3 = GetDayDifference(lunarMonthDay, hebrewMonth, hebrewDay, lunarDate.month, lunarDate.day);
            DateTime time = new DateTime(gregorianYear, 1, 1);
            return new DateTime((time.Ticks + (num3 * 0xc92a69c000L)) + Calendar.TimeToTicks(hour, minute, second, millisecond));
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            if (this.IsLeapMonth(year, month, era))
            {
                this.CheckHebrewDayValue(year, month, day, era);
                return true;
            }
            if ((this.IsLeapYear(year, 0) && (month == 6)) && (day == 30))
            {
                return true;
            }
            this.CheckHebrewDayValue(year, month, day, era);
            return false;
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            bool flag = this.IsLeapYear(year, era);
            this.CheckHebrewMonthValue(year, month, era);
            return (flag && (month == 7));
        }

        public override bool IsLeapYear(int year, int era)
        {
            CheckHebrewYearValue(year, era, "year");
            return ((((7L * year) + 1L) % 0x13L) < 7L);
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            CheckHebrewYearValue(year, era, "year");
            this.CheckHebrewMonthValue(year, month, era);
            this.CheckHebrewDayValue(year, month, day, era);
            DateTime time = HebrewToGregorian(year, month, day, hour, minute, second, millisecond);
            CheckTicksRange(time.Ticks);
            return time;
        }

        public override int ToFourDigitYear(int year)
        {
            if (year < 100)
            {
                return base.ToFourDigitYear(year);
            }
            if ((year > 0x176f) || (year < 0x14df))
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x14df, 0x176f }));
            }
            return year;
        }

        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.LunisolarCalendar;
            }
        }

        public override int[] Eras
        {
            get
            {
                return new int[] { HebrewEra };
            }
        }

        internal override int ID
        {
            get
            {
                return 8;
            }
        }

        public override DateTime MaxSupportedDateTime
        {
            get
            {
                return calendarMaxValue;
            }
        }

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
                    base.twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(this.ID, 0x169e);
                }
                return base.twoDigitYearMax;
            }
            set
            {
                base.VerifyWritable();
                if (value != 0x63)
                {
                    CheckHebrewYearValue(value, HebrewEra, "value");
                }
                base.twoDigitYearMax = value;
            }
        }

        internal class __DateBuffer
        {
            internal int day;
            internal int month;
            internal int year;
        }
    }
}

