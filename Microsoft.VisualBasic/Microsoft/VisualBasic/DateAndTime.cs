namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [StandardModule]
    public sealed class DateAndTime
    {
        private static string[] AcceptedDateFormatsDBCS = new string[] { "yyyy-M-d", "y-M-d", "yyyy/M/d", "y/M/d" };
        private static string[] AcceptedDateFormatsSBCS = new string[] { "M-d-yyyy", "M-d-y", "M/d/yyyy", "M/d/y" };

        public static DateTime DateAdd(DateInterval Interval, double Number, DateTime DateValue)
        {
            int years = (int) Math.Round(Conversion.Fix(Number));
            switch (Interval)
            {
                case DateInterval.Year:
                    return CurrentCalendar.AddYears(DateValue, years);

                case DateInterval.Quarter:
                    return DateValue.AddMonths(years * 3);

                case DateInterval.Month:
                    return CurrentCalendar.AddMonths(DateValue, years);

                case DateInterval.DayOfYear:
                case DateInterval.Day:
                case DateInterval.Weekday:
                    return DateValue.AddDays((double) years);

                case DateInterval.WeekOfYear:
                    return DateValue.AddDays(years * 7.0);

                case DateInterval.Hour:
                    return DateValue.AddHours((double) years);

                case DateInterval.Minute:
                    return DateValue.AddMinutes((double) years);

                case DateInterval.Second:
                    return DateValue.AddSeconds((double) years);
            }
            throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Interval" }));
        }

        public static DateTime DateAdd(string Interval, double Number, object DateValue)
        {
            DateTime time2;
            try
            {
                time2 = Conversions.ToDate(DateValue);
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", new string[] { "DateValue" }));
            }
            return DateAdd(DateIntervalFromString(Interval), Number, time2);
        }

        public static long DateDiff(DateInterval Interval, DateTime Date1, DateTime Date2, FirstDayOfWeek DayOfWeek = 1, FirstWeekOfYear WeekOfYear = 1)
        {
            Calendar currentCalendar;
            TimeSpan span = Date2.Subtract(Date1);
            switch (Interval)
            {
                case DateInterval.Year:
                    currentCalendar = CurrentCalendar;
                    return (long) (currentCalendar.GetYear(Date2) - currentCalendar.GetYear(Date1));

                case DateInterval.Quarter:
                    currentCalendar = CurrentCalendar;
                    return (long) ((((currentCalendar.GetYear(Date2) - currentCalendar.GetYear(Date1)) * 4) + ((currentCalendar.GetMonth(Date2) - 1) / 3)) - ((currentCalendar.GetMonth(Date1) - 1) / 3));

                case DateInterval.Month:
                    currentCalendar = CurrentCalendar;
                    return (long) ((((currentCalendar.GetYear(Date2) - currentCalendar.GetYear(Date1)) * 12) + currentCalendar.GetMonth(Date2)) - currentCalendar.GetMonth(Date1));

                case DateInterval.DayOfYear:
                case DateInterval.Day:
                    return (long) Math.Round(Conversion.Fix(span.TotalDays));

                case DateInterval.WeekOfYear:
                    Date1 = Date1.AddDays((double) (0 - GetDayOfWeek(Date1, DayOfWeek)));
                    Date2 = Date2.AddDays((double) (0 - GetDayOfWeek(Date2, DayOfWeek)));
                    return (((long) Math.Round(Conversion.Fix(Date2.Subtract(Date1).TotalDays))) / 7L);

                case DateInterval.Weekday:
                    return (((long) Math.Round(Conversion.Fix(span.TotalDays))) / 7L);

                case DateInterval.Hour:
                    return (long) Math.Round(Conversion.Fix(span.TotalHours));

                case DateInterval.Minute:
                    return (long) Math.Round(Conversion.Fix(span.TotalMinutes));

                case DateInterval.Second:
                    return (long) Math.Round(Conversion.Fix(span.TotalSeconds));
            }
            throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Interval" }));
        }

        public static long DateDiff(string Interval, object Date1, object Date2, FirstDayOfWeek DayOfWeek = 1, FirstWeekOfYear WeekOfYear = 1)
        {
            DateTime time;
            DateTime time2;
            try
            {
                time = Conversions.ToDate(Date1);
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", new string[] { "Date1" }));
            }
            try
            {
                time2 = Conversions.ToDate(Date2);
            }
            catch (StackOverflowException exception4)
            {
                throw exception4;
            }
            catch (OutOfMemoryException exception5)
            {
                throw exception5;
            }
            catch (ThreadAbortException exception6)
            {
                throw exception6;
            }
            catch (Exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", new string[] { "Date2" }));
            }
            return DateDiff(DateIntervalFromString(Interval), time, time2, DayOfWeek, WeekOfYear);
        }

        private static DateInterval DateIntervalFromString(string Interval)
        {
            if (Interval != null)
            {
                Interval = Interval.ToUpperInvariant();
            }
            string str = Interval;
            switch (str)
            {
                case "YYYY":
                    return DateInterval.Year;

                case "Y":
                    return DateInterval.DayOfYear;

                case "M":
                    return DateInterval.Month;

                case "D":
                    return DateInterval.Day;

                case "H":
                    return DateInterval.Hour;

                case "N":
                    return DateInterval.Minute;

                case "S":
                    return DateInterval.Second;

                case "WW":
                    return DateInterval.WeekOfYear;

                case "W":
                    return DateInterval.Weekday;
            }
            if (str != "Q")
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Interval" }));
            }
            return DateInterval.Quarter;
        }

        public static int DatePart(DateInterval Interval, DateTime DateValue, FirstDayOfWeek FirstDayOfWeekValue = 1, FirstWeekOfYear FirstWeekOfYearValue = 1)
        {
            DayOfWeek firstDayOfWeek;
            CalendarWeekRule calendarWeekRule;
            switch (Interval)
            {
                case DateInterval.Year:
                    return CurrentCalendar.GetYear(DateValue);

                case DateInterval.Quarter:
                    return (((DateValue.Month - 1) / 3) + 1);

                case DateInterval.Month:
                    return CurrentCalendar.GetMonth(DateValue);

                case DateInterval.DayOfYear:
                    return CurrentCalendar.GetDayOfYear(DateValue);

                case DateInterval.Day:
                    return CurrentCalendar.GetDayOfMonth(DateValue);

                case DateInterval.WeekOfYear:
                    if (FirstDayOfWeekValue != FirstDayOfWeek.System)
                    {
                        firstDayOfWeek = (DayOfWeek) (FirstDayOfWeekValue - 1);
                        break;
                    }
                    firstDayOfWeek = Utils.GetCultureInfo().DateTimeFormat.FirstDayOfWeek;
                    break;

                case DateInterval.Weekday:
                    return Weekday(DateValue, FirstDayOfWeekValue);

                case DateInterval.Hour:
                    return CurrentCalendar.GetHour(DateValue);

                case DateInterval.Minute:
                    return CurrentCalendar.GetMinute(DateValue);

                case DateInterval.Second:
                    return CurrentCalendar.GetSecond(DateValue);

                default:
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Interval" }));
            }
            switch (FirstWeekOfYearValue)
            {
                case FirstWeekOfYear.System:
                    calendarWeekRule = Utils.GetCultureInfo().DateTimeFormat.CalendarWeekRule;
                    break;

                case FirstWeekOfYear.Jan1:
                    calendarWeekRule = CalendarWeekRule.FirstDay;
                    break;

                case FirstWeekOfYear.FirstFourDays:
                    calendarWeekRule = CalendarWeekRule.FirstFourDayWeek;
                    break;

                case FirstWeekOfYear.FirstFullWeek:
                    calendarWeekRule = CalendarWeekRule.FirstFullWeek;
                    break;
            }
            return CurrentCalendar.GetWeekOfYear(DateValue, calendarWeekRule, firstDayOfWeek);
        }

        public static int DatePart(string Interval, object DateValue, FirstDayOfWeek DayOfWeek = 1, FirstWeekOfYear WeekOfYear = 1)
        {
            DateTime time;
            try
            {
                time = Conversions.ToDate(DateValue);
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("Argument_InvalidDateValue1", new string[] { "DateValue" }));
            }
            return DatePart(DateIntervalFromString(Interval), time, DayOfWeek, WeekOfYear);
        }

        public static DateTime DateSerial(int Year, int Month, int Day)
        {
            DateTime time2;
            Calendar currentCalendar = CurrentCalendar;
            if (Year < 0)
            {
                Year = currentCalendar.GetYear(DateTime.Today) + Year;
            }
            else if (Year < 100)
            {
                Year = currentCalendar.ToFourDigitYear(Year);
            }
            if ((((currentCalendar is GregorianCalendar) && (Month >= 1)) && ((Month <= 12) && (Day >= 1))) && (Day <= 0x1c))
            {
                return new DateTime(Year, Month, Day);
            }
            try
            {
                time2 = currentCalendar.ToDateTime(Year, 1, 1, 0, 0, 0, 0);
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Year" })), 5);
            }
            try
            {
                time2 = currentCalendar.AddMonths(time2, Month - 1);
            }
            catch (StackOverflowException exception4)
            {
                throw exception4;
            }
            catch (OutOfMemoryException exception5)
            {
                throw exception5;
            }
            catch (ThreadAbortException exception6)
            {
                throw exception6;
            }
            catch (Exception)
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Month" })), 5);
            }
            try
            {
                time2 = currentCalendar.AddDays(time2, Day - 1);
            }
            catch (StackOverflowException exception7)
            {
                throw exception7;
            }
            catch (OutOfMemoryException exception8)
            {
                throw exception8;
            }
            catch (ThreadAbortException exception9)
            {
                throw exception9;
            }
            catch (Exception)
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Day" })), 5);
            }
            return time2;
        }

        public static DateTime DateValue(string StringDate)
        {
            return Conversions.ToDate(StringDate).Date;
        }

        public static int Day(DateTime DateValue)
        {
            return CurrentCalendar.GetDayOfMonth(DateValue);
        }

        private static int GetDayOfWeek(DateTime dt, FirstDayOfWeek weekdayFirst)
        {
            if ((weekdayFirst < FirstDayOfWeek.System) || (weekdayFirst > FirstDayOfWeek.Saturday))
            {
                throw ExceptionUtils.VbMakeException(5);
            }
            if (weekdayFirst == FirstDayOfWeek.System)
            {
                weekdayFirst = (FirstDayOfWeek) (Utils.GetDateTimeFormatInfo().FirstDayOfWeek + 1);
            }
            return (((int) (((dt.DayOfWeek - ((DayOfWeek) ((int) weekdayFirst))) + 8) % (DayOfWeek.Saturday | DayOfWeek.Monday))) + 1);
        }

        public static int Hour(DateTime TimeValue)
        {
            return CurrentCalendar.GetHour(TimeValue);
        }

        private static bool IsDBCSCulture()
        {
            if (Marshal.SystemMaxDBCSCharSize == 1)
            {
                return false;
            }
            return true;
        }

        public static int Minute(DateTime TimeValue)
        {
            return CurrentCalendar.GetMinute(TimeValue);
        }

        public static int Month(DateTime DateValue)
        {
            return CurrentCalendar.GetMonth(DateValue);
        }

        public static string MonthName(int Month, bool Abbreviate = false)
        {
            string abbreviatedMonthName;
            if ((Month < 1) || (Month > 13))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Month" }));
            }
            if (Abbreviate)
            {
                abbreviatedMonthName = Utils.GetDateTimeFormatInfo().GetAbbreviatedMonthName(Month);
            }
            else
            {
                abbreviatedMonthName = Utils.GetDateTimeFormatInfo().GetMonthName(Month);
            }
            if (abbreviatedMonthName.Length == 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Month" }));
            }
            return abbreviatedMonthName;
        }

        public static int Second(DateTime TimeValue)
        {
            return CurrentCalendar.GetSecond(TimeValue);
        }

        public static DateTime TimeSerial(int Hour, int Minute, int Second)
        {
            int num = (((Hour * 60) * 60) + (Minute * 60)) + Second;
            if (num < 0)
            {
                num += 0x15180;
            }
            return new DateTime(num * 0x989680L);
        }

        public static DateTime TimeValue(string StringTime)
        {
            return new DateTime(Conversions.ToDate(StringTime).Ticks % 0xc92a69c000L);
        }

        public static int Weekday(DateTime DateValue, FirstDayOfWeek DayOfWeek = 1)
        {
            if (DayOfWeek == FirstDayOfWeek.System)
            {
                DayOfWeek = (FirstDayOfWeek) (DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek + 1);
            }
            else if ((DayOfWeek < FirstDayOfWeek.Sunday) || (DayOfWeek > FirstDayOfWeek.Saturday))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "DayOfWeek" }));
            }
            int num = ((int) CurrentCalendar.GetDayOfWeek(DateValue)) + 1;
            return ((((num - DayOfWeek) + 7) % 7) + 1);
        }

        public static string WeekdayName(int Weekday, bool Abbreviate = false, FirstDayOfWeek FirstDayOfWeekValue = 0)
        {
            string abbreviatedDayName;
            if ((Weekday < 1) || (Weekday > 7))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Weekday" }));
            }
            if ((FirstDayOfWeekValue < FirstDayOfWeek.System) || (FirstDayOfWeekValue > FirstDayOfWeek.Saturday))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "FirstDayOfWeekValue" }));
            }
            DateTimeFormatInfo format = (DateTimeFormatInfo) Utils.GetCultureInfo().GetFormat(typeof(DateTimeFormatInfo));
            if (FirstDayOfWeekValue == FirstDayOfWeek.System)
            {
                FirstDayOfWeekValue = (FirstDayOfWeek) (format.FirstDayOfWeek + 1);
            }
            try
            {
                if (Abbreviate)
                {
                    abbreviatedDayName = format.GetAbbreviatedDayName((DayOfWeek) (((Weekday + FirstDayOfWeekValue) - 2) % 7));
                }
                else
                {
                    abbreviatedDayName = format.GetDayName((DayOfWeek) (((Weekday + FirstDayOfWeekValue) - 2) % 7));
                }
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Weekday" }));
            }
            if (abbreviatedDayName.Length == 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Weekday" }));
            }
            return abbreviatedDayName;
        }

        public static int Year(DateTime DateValue)
        {
            return CurrentCalendar.GetYear(DateValue);
        }

        private static Calendar CurrentCalendar
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.Calendar;
            }
        }

        public static string DateString
        {
            get
            {
                if (IsDBCSCulture())
                {
                    return DateTime.Today.ToString(@"yyyy\-MM\-dd", Utils.GetInvariantCultureInfo());
                }
                return DateTime.Today.ToString(@"MM\-dd\-yyyy", Utils.GetInvariantCultureInfo());
            }
            [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
            set
            {
                DateTime time;
                try
                {
                    string s = Utils.ToHalfwidthNumbers(value, Utils.GetCultureInfo());
                    if (IsDBCSCulture())
                    {
                        time = DateTime.ParseExact(s, AcceptedDateFormatsDBCS, Utils.GetInvariantCultureInfo(), DateTimeStyles.AllowWhiteSpaces);
                    }
                    else
                    {
                        time = DateTime.ParseExact(s, AcceptedDateFormatsSBCS, Utils.GetInvariantCultureInfo(), DateTimeStyles.AllowWhiteSpaces);
                    }
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    throw ExceptionUtils.VbMakeException(new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(value, 0x20), "Date" })), 5);
                }
                Utils.SetDate(time);
            }
        }

        public static DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        public static DateTime TimeOfDay
        {
            get
            {
                DateTime now = DateTime.Now;
                long ticks = now.TimeOfDay.Ticks;
                return new DateTime(ticks - (ticks % 0x989680L));
            }
            [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
            set
            {
                Utils.SetTime(value);
            }
        }

        public static double Timer
        {
            get
            {
                return (((double) (DateTime.Now.Ticks % 0xc92a69c000L)) / 10000000.0);
            }
        }

        public static string TimeString
        {
            get
            {
                DateTime time3 = new DateTime(DateTime.Now.TimeOfDay.Ticks);
                return time3.ToString("HH:mm:ss", Utils.GetInvariantCultureInfo());
            }
            [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
            set
            {
                DateTime time;
                try
                {
                    time = DateType.FromString(value, Utils.GetInvariantCultureInfo());
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    throw ExceptionUtils.VbMakeException(new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(value, 0x20), "Date" })), 5);
                }
                Utils.SetTime(time);
            }
        }

        public static DateTime Today
        {
            get
            {
                return DateTime.Today;
            }
            [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
            set
            {
                Utils.SetDate(value);
            }
        }
    }
}

