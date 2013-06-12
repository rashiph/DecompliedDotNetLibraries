namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class ThaiBuddhistCalendar : Calendar
    {
        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 0xa0c;
        internal GregorianCalendarHelper helper;
        public const int ThaiBuddhistEra = 1;
        internal static EraInfo[] thaiBuddhistEraInfo = InitEraInfo();

        public ThaiBuddhistCalendar()
        {
            this.helper = new GregorianCalendarHelper(this, thaiBuddhistEraInfo);
        }

        public override DateTime AddMonths(DateTime time, int months)
        {
            return this.helper.AddMonths(time, months);
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            return this.helper.AddYears(time, years);
        }

        public override int GetDayOfMonth(DateTime time)
        {
            return this.helper.GetDayOfMonth(time);
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            return this.helper.GetDayOfWeek(time);
        }

        public override int GetDayOfYear(DateTime time)
        {
            return this.helper.GetDayOfYear(time);
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            return this.helper.GetDaysInMonth(year, month, era);
        }

        public override int GetDaysInYear(int year, int era)
        {
            return this.helper.GetDaysInYear(year, era);
        }

        public override int GetEra(DateTime time)
        {
            return this.helper.GetEra(time);
        }

        [ComVisible(false)]
        public override int GetLeapMonth(int year, int era)
        {
            return this.helper.GetLeapMonth(year, era);
        }

        public override int GetMonth(DateTime time)
        {
            return this.helper.GetMonth(time);
        }

        public override int GetMonthsInYear(int year, int era)
        {
            return this.helper.GetMonthsInYear(year, era);
        }

        [ComVisible(false)]
        public override int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            return this.helper.GetWeekOfYear(time, rule, firstDayOfWeek);
        }

        public override int GetYear(DateTime time)
        {
            return this.helper.GetYear(time);
        }

        private static EraInfo[] InitEraInfo()
        {
            return new EraInfo[] { new EraInfo(1, 1, 1, 1, -543, 0x220, 0x292e) };
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            return this.helper.IsLeapDay(year, month, day, era);
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            return this.helper.IsLeapMonth(year, month, era);
        }

        public override bool IsLeapYear(int year, int era)
        {
            return this.helper.IsLeapYear(year, era);
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            return this.helper.ToDateTime(year, month, day, hour, minute, second, millisecond, era);
        }

        public override int ToFourDigitYear(int year)
        {
            return this.helper.ToFourDigitYear(year, this.TwoDigitYearMax);
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
                return this.helper.Eras;
            }
        }

        internal override int ID
        {
            get
            {
                return 7;
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
                    base.twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(this.ID, 0xa0c);
                }
                return base.twoDigitYearMax;
            }
            set
            {
                base.VerifyWritable();
                if ((value < 0x63) || (value > this.helper.MaxYear))
                {
                    throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x63, this.helper.MaxYear }));
                }
                base.twoDigitYearMax = value;
            }
        }
    }
}

