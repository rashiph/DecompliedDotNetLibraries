namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Text;

    public class DatePrototype : DateObject
    {
        internal static DateConstructor _constructor;
        private static readonly int[] daysToMonthEnd = new int[] { 0x1f, 0x3b, 90, 120, 0x97, 0xb5, 0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d };
        private const double HoursPerDay = 24.0;
        private static readonly int[] leapDaysToMonthEnd = new int[] { 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6, 0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e };
        private static readonly double localDaylightTZA;
        private static readonly double localStandardTZA;
        internal const double maxDate = 8.64E+15;
        internal const double minDate = -8.64E+15;
        private const double MinutesPerHour = 60.0;
        private static readonly string[] MonthName = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        private const double msPerDay = 86400000.0;
        private const double msPerHour = 3600000.0;
        private const double msPerMinute = 60000.0;
        private const double msPerSecond = 1000.0;
        internal const double msTo1970 = 62135596800000;
        internal static readonly DatePrototype ob = new DatePrototype(ObjectPrototype.ob);
        private const double SecondsPerMinute = 60.0;
        private static readonly string[] Strings = new string[] { 
            "bc", "b.c", "ad", "a.d", "am", "a.m", "pm", "p.m", "est", "edt", "cst", "cdt", "mst", "mdt", "pst", "pdt", 
            "gmt", "utc", "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "january", "february", "march", "april", "may", "june", "july", 
            "august", "september", "october", "november", "december"
         };
        internal const double ticksPerMillisecond = 10000.0;
        private static readonly Tk[] Tokens = new Tk[] { 
            Tk.BcAd, Tk.BcAd, Tk.BcAd, Tk.BcAd, Tk.AmPm, Tk.AmPm, Tk.AmPm, Tk.AmPm, Tk.Zone, Tk.Zone, Tk.Zone, Tk.Zone, Tk.Zone, Tk.Zone, Tk.Zone, Tk.Zone, 
            Tk.Zone, Tk.Zone, Tk.Day, Tk.Day, Tk.Day, Tk.Day, Tk.Day, Tk.Day, Tk.Day, Tk.Month, Tk.Month, Tk.Month, Tk.Month, Tk.Month, Tk.Month, Tk.Month, 
            Tk.Month, Tk.Month, Tk.Month, Tk.Month, Tk.Month
         };
        private static readonly bool useDST;
        private static readonly int[] Values = new int[] { 
            -1, -1, 1, 1, -1, -1, 1, 1, -300, -240, -360, -300, -420, -360, -480, -420, 
            0, 0, 0, 1, 2, 3, 4, 5, 6, 0, 1, 2, 3, 4, 5, 6, 
            7, 8, 9, 10, 11
         };
        private static readonly string[] WeekDayName = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        static DatePrototype()
        {
            DateTime time = new DateTime(DateTime.Now.Year, 1, 1);
            double num = ((double) (time.Ticks - time.ToUniversalTime().Ticks)) / 10000.0;
            DateTime time2 = new DateTime(DateTime.Now.Year, 7, 1);
            double num2 = ((double) (time2.Ticks - time2.ToUniversalTime().Ticks)) / 10000.0;
            if (num < num2)
            {
                localStandardTZA = num;
                localDaylightTZA = num2;
            }
            else
            {
                localStandardTZA = num2;
                localDaylightTZA = num;
            }
            useDST = !(localStandardTZA == localDaylightTZA);
        }

        internal DatePrototype(ObjectPrototype parent) : base(parent, 0.0)
        {
            base.noExpando = true;
        }

        private static void AppendTime(double time, StringBuilder sb)
        {
            int num = HourFromTime(time);
            if (num < 10)
            {
                sb.Append("0");
            }
            sb.Append(num);
            sb.Append(":");
            int num2 = MinFromTime(time);
            if (num2 < 10)
            {
                sb.Append("0");
            }
            sb.Append(num2);
            sb.Append(":");
            int num3 = SecFromTime(time);
            if (num3 < 10)
            {
                sb.Append("0");
            }
            sb.Append(num3);
        }

        private static int DateFromTime(double time)
        {
            int index = 0;
            int num2 = DayWithinYear(time) + 1;
            if (num2 <= 0x1f)
            {
                return num2;
            }
            if (!InLeapYear(YearFromTime(time)))
            {
                while (num2 > daysToMonthEnd[index])
                {
                    index++;
                }
                return (num2 - daysToMonthEnd[index - 1]);
            }
            while (num2 > leapDaysToMonthEnd[index])
            {
                index++;
            }
            return (num2 - leapDaysToMonthEnd[index - 1]);
        }

        internal static string DateToDateString(double utcTime)
        {
            if (double.IsNaN(utcTime))
            {
                return "NaN";
            }
            StringBuilder builder = new StringBuilder();
            double time = LocalTime(utcTime);
            builder.Append(WeekDayName[WeekDay(time)]);
            builder.Append(" ");
            int index = MonthFromTime(time);
            builder.Append(MonthName[index]);
            builder.Append(" ");
            builder.Append(DateFromTime(time));
            builder.Append(" ");
            builder.Append(YearString(time));
            return builder.ToString();
        }

        internal static string DateToLocaleDateString(double time)
        {
            if (double.IsNaN(time))
            {
                return "NaN";
            }
            StringBuilder builder = new StringBuilder();
            int num = MonthFromTime(time) + 1;
            if (num < 10)
            {
                builder.Append("0");
            }
            builder.Append(num);
            builder.Append("/");
            int num2 = DateFromTime(time);
            if (num2 < 10)
            {
                builder.Append("0");
            }
            builder.Append(num2);
            builder.Append("/");
            builder.Append(YearString(time));
            return builder.ToString();
        }

        internal static string DateToLocaleString(double time)
        {
            if (double.IsNaN(time))
            {
                return "NaN";
            }
            StringBuilder sb = new StringBuilder();
            int num = MonthFromTime(time) + 1;
            if (num < 10)
            {
                sb.Append("0");
            }
            sb.Append(num);
            sb.Append("/");
            int num2 = DateFromTime(time);
            if (num2 < 10)
            {
                sb.Append("0");
            }
            sb.Append(num2);
            sb.Append("/");
            sb.Append(YearString(time));
            sb.Append(" ");
            AppendTime(time, sb);
            return sb.ToString();
        }

        internal static string DateToLocaleTimeString(double time)
        {
            if (double.IsNaN(time))
            {
                return "NaN";
            }
            StringBuilder sb = new StringBuilder();
            AppendTime(time, sb);
            return sb.ToString();
        }

        internal static string DateToString(double utcTime)
        {
            if (double.IsNaN(utcTime))
            {
                return "NaN";
            }
            StringBuilder sb = new StringBuilder();
            double time = LocalTime(utcTime);
            sb.Append(WeekDayName[WeekDay(time)]);
            sb.Append(" ");
            int index = MonthFromTime(time);
            sb.Append(MonthName[index]);
            sb.Append(" ");
            sb.Append(DateFromTime(time));
            sb.Append(" ");
            AppendTime(time, sb);
            sb.Append(" ");
            sb.Append(TimeZoneID(utcTime));
            sb.Append(" ");
            sb.Append(YearString(time));
            return sb.ToString();
        }

        internal static string DateToTimeString(double utcTime)
        {
            if (double.IsNaN(utcTime))
            {
                return "NaN";
            }
            StringBuilder sb = new StringBuilder();
            AppendTime(LocalTime(utcTime), sb);
            sb.Append(" ");
            sb.Append(TimeZoneID(utcTime));
            return sb.ToString();
        }

        private static double Day(double time)
        {
            return Math.Floor((double) (time / 86400000.0));
        }

        private static double DayFromYear(double year)
        {
            return ((((365.0 * (year - 1970.0)) + Math.Floor((double) ((year - 1969.0) / 4.0))) - Math.Floor((double) ((year - 1901.0) / 100.0))) + Math.Floor((double) ((year - 1601.0) / 400.0)));
        }

        private static bool DaylightSavingsTime(double localTime)
        {
            if (!useDST)
            {
                return false;
            }
            double num = (localTime + 62135596800000) * 10000.0;
            if ((-9.2233720368547758E+18 <= num) && (num <= 9.2233720368547758E+18))
            {
                try
                {
                    DateTime time = new DateTime((long) num);
                    return TimeZone.CurrentTimeZone.IsDaylightSavingTime(time);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
            int num2 = MonthFromTime(localTime);
            if ((num2 < 3) || (num2 > 9))
            {
                return false;
            }
            if ((num2 > 3) && (num2 < 9))
            {
                return true;
            }
            int num3 = DateFromTime(localTime);
            if (num2 == 3)
            {
                if (num3 > 7)
                {
                    return true;
                }
                int num4 = WeekDay(localTime);
                if (num4 > 0)
                {
                    return (num3 > num4);
                }
                return (HourFromTime(localTime) > 1);
            }
            if (num3 < 0x19)
            {
                return true;
            }
            int num5 = WeekDay(localTime);
            if (num5 > 0)
            {
                return ((num3 - num5) < 0x19);
            }
            return (HourFromTime(localTime) < 1);
        }

        private static int DaysInYear(double year)
        {
            if ((year % 4.0) != 0.0)
            {
                return 0x16d;
            }
            if (((year % 100.0) == 0.0) && ((year % 400.0) != 0.0))
            {
                return 0x16d;
            }
            return 0x16e;
        }

        private static int DayWithinYear(double time)
        {
            return (int) (Day(time) - DayFromYear(YearFromTime(time)));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getDate)]
        public static double getDate(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return (double) DateFromTime(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getDay)]
        public static double getDay(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return (double) WeekDay(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getFullYear)]
        public static double getFullYear(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return YearFromTime(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getHours)]
        public static double getHours(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return (double) HourFromTime(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getMilliseconds)]
        public static double getMilliseconds(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return (double) msFromTime(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getMinutes)]
        public static double getMinutes(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return (double) MinFromTime(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getMonth)]
        public static double getMonth(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return (double) MonthFromTime(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getSeconds)]
        public static double getSeconds(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return (double) SecFromTime(LocalTime(utcTime));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getTime)]
        public static double getTime(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            return ((DateObject) thisob).value;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getTimezoneOffset)]
        public static double getTimezoneOffset(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            return ((utcTime - LocalTime(utcTime)) / 60000.0);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCDate)]
        public static double getUTCDate(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return (double) DateFromTime(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCDay)]
        public static double getUTCDay(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return (double) WeekDay(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCFullYear)]
        public static double getUTCFullYear(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return YearFromTime(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCHours)]
        public static double getUTCHours(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return (double) HourFromTime(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCMilliseconds)]
        public static double getUTCMilliseconds(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return (double) msFromTime(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCMinutes)]
        public static double getUTCMinutes(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return (double) MinFromTime(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCMonth)]
        public static double getUTCMonth(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return (double) MonthFromTime(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCSeconds)]
        public static double getUTCSeconds(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            if (time != time)
            {
                return time;
            }
            return (double) SecFromTime(time);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getVarDate)]
        public static object getVarDate(object thisob)
        {
            long num2;
            DateTime time;
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return null;
            }
            try
            {
                num2 = ((long) (LocalTime(utcTime) + 62135596800000)) * 0x2710L;
            }
            catch (OverflowException)
            {
                return null;
            }
            if ((num2 < DateTime.MinValue.Ticks) || (num2 > DateTime.MaxValue.Ticks))
            {
                return null;
            }
            try
            {
                time = new DateTime(num2);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
            return time;
        }

        [NotRecommended("getYear"), JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getYear)]
        public static double getYear(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double utcTime = ((DateObject) thisob).value;
            if (utcTime != utcTime)
            {
                return utcTime;
            }
            double num2 = YearFromTime(LocalTime(utcTime));
            if ((1900.0 <= num2) && (num2 <= 1999.0))
            {
                return (num2 - 1900.0);
            }
            return num2;
        }

        private static int HourFromTime(double time)
        {
            double num = Math.Floor((double) (time / 3600000.0)) % 24.0;
            if (num < 0.0)
            {
                num += 24.0;
            }
            return (int) num;
        }

        private static bool InLeapYear(double year)
        {
            if ((year % 4.0) != 0.0)
            {
                return false;
            }
            if (((year % 100.0) == 0.0) && ((year % 400.0) != 0.0))
            {
                return false;
            }
            return true;
        }

        private static bool isalpha(char ch)
        {
            return ((('A' <= ch) && (ch <= 'Z')) || (('a' <= ch) && (ch <= 'z')));
        }

        private static bool isASCII(char ch)
        {
            return (ch < '\x0080');
        }

        private static bool isdigit(char ch)
        {
            return (('0' <= ch) && (ch <= '9'));
        }

        private static double LocalTime(double utcTime)
        {
            return (utcTime + (DaylightSavingsTime(utcTime + localStandardTZA) ? localDaylightTZA : localStandardTZA));
        }

        internal static double MakeDate(double day, double time)
        {
            if (!double.IsInfinity(day) && !double.IsInfinity(time))
            {
                return ((day * 86400000.0) + time);
            }
            return double.NaN;
        }

        internal static double MakeDay(double year, double month, double date)
        {
            if (((double.IsInfinity(year) || double.IsInfinity(month)) || (double.IsInfinity(date) || (year != year))) || ((month != month) || (date != date)))
            {
                return double.NaN;
            }
            year = (int) Runtime.DoubleToInt64(year);
            month = (int) Runtime.DoubleToInt64(month);
            date = (int) Runtime.DoubleToInt64(date);
            year += Math.Floor((double) (month / 12.0));
            month = month % 12.0;
            if (month < 0.0)
            {
                month += 12.0;
            }
            double num = 0.0;
            if (month > 0.0)
            {
                if (InLeapYear((double) ((int) Runtime.DoubleToInt64(year))))
                {
                    num = leapDaysToMonthEnd[(int) (month - 1.0)];
                }
                else
                {
                    num = daysToMonthEnd[(int) (month - 1.0)];
                }
            }
            return (((DayFromYear(year) - 1.0) + num) + date);
        }

        internal static double MakeTime(double hour, double min, double sec, double ms)
        {
            if (((double.IsInfinity(hour) || double.IsInfinity(min)) || (double.IsInfinity(sec) || double.IsInfinity(ms))) || (((hour != hour) || (min != min)) || ((sec != sec) || (ms != ms))))
            {
                return double.NaN;
            }
            hour = (int) Runtime.DoubleToInt64(hour);
            min = (int) Runtime.DoubleToInt64(min);
            sec = (int) Runtime.DoubleToInt64(sec);
            ms = (int) Runtime.DoubleToInt64(ms);
            return ((((hour * 3600000.0) + (min * 60000.0)) + (sec * 1000.0)) + ms);
        }

        private static int MinFromTime(double time)
        {
            double num = Math.Floor((double) (time / 60000.0)) % 60.0;
            if (num < 0.0)
            {
                num += 60.0;
            }
            return (int) num;
        }

        private static int MonthFromTime(double time)
        {
            int index = 0;
            int num2 = DayWithinYear(time) + 1;
            if (!InLeapYear(YearFromTime(time)))
            {
                while (num2 > daysToMonthEnd[index])
                {
                    index++;
                }
                return index;
            }
            while (num2 > leapDaysToMonthEnd[index])
            {
                index++;
            }
            return index;
        }

        private static int msFromTime(double time)
        {
            double num = time % 1000.0;
            if (num < 0.0)
            {
                num += 1000.0;
            }
            return (int) num;
        }

        private static bool NotSpecified(object value)
        {
            if (value != null)
            {
                return (value is Missing);
            }
            return true;
        }

        internal static double ParseDate(string str)
        {
            long num = 0x80000000L;
            int num2 = 0;
            int num3 = 0;
            Ps initial = Ps.Initial;
            long num4 = num;
            long num5 = num;
            long num6 = num;
            long num7 = num;
            long num8 = num;
            long num9 = num;
            str = str.ToLowerInvariant();
            int num10 = 0;
            int length = str.Length;
            while (num10 < length)
            {
                int num12;
                int num16;
                char ch = str[num10++];
                if (ch <= ' ')
                {
                    continue;
                }
                switch (ch)
                {
                    case '(':
                        num12 = 1;
                        goto Label_00BD;

                    case '+':
                    {
                        if (num != num4)
                        {
                            initial = Ps.AddOffset;
                        }
                        continue;
                    }
                    case ',':
                    case '/':
                    case ':':
                    {
                        continue;
                    }
                    case '-':
                    {
                        if (num != num4)
                        {
                            initial = Ps.SubOffset;
                        }
                        continue;
                    }
                    default:
                        goto Label_00E6;
                }
            Label_008D:
                ch = str[num10++];
                if (ch == '(')
                {
                    num12++;
                }
                else if ((ch == ')') && (--num12 <= 0))
                {
                    continue;
                }
            Label_00BD:
                if (num10 < length)
                {
                    goto Label_008D;
                }
                continue;
            Label_00E6:
                if (!isalpha(ch))
                {
                    goto Label_032F;
                }
                int indexA = num10 - 1;
                while (num10 < length)
                {
                    ch = str[num10++];
                    if (!isalpha(ch) && ('.' != ch))
                    {
                        break;
                    }
                }
                int num14 = (num10 - indexA) - ((num10 < length) ? 1 : 0);
                if ('.' == str[num10 - ((num10 < length) ? 2 : 1)])
                {
                    num14--;
                }
                while ((ch == ' ') && (num10 < length))
                {
                    ch = str[num10++];
                }
                if (1 == num14)
                {
                    if (num != num8)
                    {
                        return double.NaN;
                    }
                    char ch2 = str[indexA];
                    if (ch2 <= 'm')
                    {
                        if ((ch2 == 'j') || (ch2 < 'a'))
                        {
                            return double.NaN;
                        }
                        num8 = -((long) ((ch2 - 'a') + ((ch2 < 'j') ? 1 : 0))) * 60L;
                    }
                    else if (ch2 <= 'y')
                    {
                        num8 = (ch2 - 'm') * 60L;
                    }
                    else if (ch2 == 'z')
                    {
                        num8 = 0L;
                    }
                    else
                    {
                        return double.NaN;
                    }
                    if ('+' == ch)
                    {
                        initial = Ps.AddOffset;
                    }
                    else if ('-' == ch)
                    {
                        initial = Ps.SubOffset;
                    }
                    else
                    {
                        initial = Ps.Initial;
                    }
                    continue;
                }
                for (int i = Strings.Length - 1; i >= 0; i--)
                {
                    string strB = Strings[i];
                    if (strB.Length < num14)
                    {
                        continue;
                    }
                    if (string.CompareOrdinal(str, indexA, strB, 0, num14) != 0)
                    {
                        if (i == 0)
                        {
                            return double.NaN;
                        }
                        continue;
                    }
                    switch (Tokens[i])
                    {
                        case Tk.BcAd:
                            if (num3 == 0)
                            {
                                break;
                            }
                            return double.NaN;

                        case Tk.AmPm:
                            if (num2 == 0)
                            {
                                goto Label_02A8;
                            }
                            return double.NaN;

                        case Tk.Zone:
                            if (num == num8)
                            {
                                goto Label_02DE;
                            }
                            return double.NaN;

                        case Tk.Month:
                            if (num == num5)
                            {
                                goto Label_02C2;
                            }
                            return double.NaN;

                        default:
                            goto Label_031B;
                    }
                    num3 = Values[i];
                    break;
                Label_02A8:
                    num2 = Values[i];
                    break;
                Label_02C2:
                    num5 = Values[i];
                    break;
                Label_02DE:
                    num8 = Values[i];
                    if ('+' == ch)
                    {
                        initial = Ps.AddOffset;
                        num10++;
                    }
                    else if ('-' == ch)
                    {
                        initial = Ps.SubOffset;
                        num10++;
                    }
                    else
                    {
                        initial = Ps.Initial;
                    }
                    break;
                }
            Label_031B:
                if (num10 < length)
                {
                    num10--;
                }
                continue;
            Label_032F:
                if (isdigit(ch))
                {
                    num16 = 0;
                    int num17 = num10;
                    do
                    {
                        num16 = ((num16 * 10) + ch) - 0x30;
                        if (num10 >= length)
                        {
                            break;
                        }
                        ch = str[num10++];
                    }
                    while (isdigit(ch));
                    if ((num10 - num17) <= 6)
                    {
                        goto Label_0395;
                    }
                }
                return double.NaN;
            Label_0386:
                ch = str[num10++];
            Label_0395:
                if ((ch == ' ') && (num10 < length))
                {
                    goto Label_0386;
                }
                switch (initial)
                {
                    case Ps.Minutes:
                        if (num16 < 60)
                        {
                            goto Label_0462;
                        }
                        return double.NaN;

                    case Ps.Seconds:
                        if (num16 < 60)
                        {
                            goto Label_04A0;
                        }
                        return double.NaN;

                    case Ps.AddOffset:
                        if (num == num9)
                        {
                            break;
                        }
                        return double.NaN;

                    case Ps.SubOffset:
                        if (num == num9)
                        {
                            goto Label_041C;
                        }
                        return double.NaN;

                    case Ps.Date:
                        if (num == num6)
                        {
                            goto Label_04CD;
                        }
                        return double.NaN;

                    case Ps.Year:
                        if (num == num4)
                        {
                            goto Label_050A;
                        }
                        return double.NaN;

                    default:
                        goto Label_0525;
                }
                num9 = (num16 < 0x18) ? ((long) (num16 * 60)) : ((long) ((num16 % 100) + ((num16 / 100) * 60)));
                initial = Ps.Initial;
                if (num10 < length)
                {
                    num10--;
                }
                continue;
            Label_041C:
                num9 = (num16 < 0x18) ? ((long) (-num16 * 60)) : ((long) -((num16 % 100) + ((num16 / 100) * 60)));
                initial = Ps.Initial;
                if (num10 < length)
                {
                    num10--;
                }
                continue;
            Label_0462:
                num7 += num16 * 60;
                if (ch == ':')
                {
                    initial = Ps.Seconds;
                }
                else
                {
                    initial = Ps.Initial;
                    if (num10 < length)
                    {
                        num10--;
                    }
                }
                continue;
            Label_04A0:
                num7 += num16;
                initial = Ps.Initial;
                if (num10 < length)
                {
                    num10--;
                }
                continue;
            Label_04CD:
                num6 = num16;
                if (('/' == ch) || ('-' == ch))
                {
                    initial = Ps.Year;
                }
                else
                {
                    initial = Ps.Initial;
                    if (num10 < length)
                    {
                        num10--;
                    }
                }
                continue;
            Label_050A:
                num4 = num16;
                initial = Ps.Initial;
                if (num10 < length)
                {
                    num10--;
                }
                continue;
            Label_0525:
                if (num16 >= 70)
                {
                    if (num != num4)
                    {
                        return double.NaN;
                    }
                    num4 = num16;
                    if (num10 < length)
                    {
                        num10--;
                    }
                }
                else
                {
                    char ch4 = ch;
                    switch (ch4)
                    {
                        case '-':
                        case '/':
                            if (num == num5)
                            {
                                goto Label_05B0;
                            }
                            return double.NaN;

                        case '.':
                            goto Label_05BB;
                    }
                    if (ch4 != ':')
                    {
                        goto Label_05BB;
                    }
                    if (num != num7)
                    {
                        return double.NaN;
                    }
                    if (num16 >= 0x18)
                    {
                        return double.NaN;
                    }
                    num7 = num16 * 0xe10;
                    initial = Ps.Minutes;
                }
                continue;
            Label_05B0:
                num5 = num16 - 1;
                initial = Ps.Date;
                continue;
            Label_05BB:
                if (num != num6)
                {
                    return double.NaN;
                }
                num6 = num16;
                if (num10 < length)
                {
                    num10--;
                }
            }
            if (((num == num4) || (num == num5)) || (num == num6))
            {
                return double.NaN;
            }
            if (num3 != 0)
            {
                if (num3 < 0)
                {
                    num4 = -num4 + 1L;
                }
            }
            else if (num4 < 100L)
            {
                num4 += 0x76cL;
            }
            if (num2 != 0)
            {
                if (num == num7)
                {
                    return double.NaN;
                }
                if ((num7 >= 0xa8c0L) && (num7 < 0xb6d0L))
                {
                    if (num2 < 0)
                    {
                        num7 -= 0xa8c0L;
                    }
                }
                else if (num2 > 0)
                {
                    if (num7 >= 0xa8c0L)
                    {
                        return double.NaN;
                    }
                    num7 += 0xa8c0L;
                }
            }
            else if (num == num7)
            {
                num7 = 0L;
            }
            bool flag = false;
            if (num != num8)
            {
                num7 -= num8 * 60L;
                flag = true;
            }
            if (num != num9)
            {
                num7 -= num9 * 60L;
            }
            double localTime = MakeDate(MakeDay((double) num4, (double) num5, (double) num6), (double) (num7 * 0x3e8L));
            if (!flag)
            {
                localTime = UTC(localTime);
            }
            return localTime;
        }

        private static int SecFromTime(double time)
        {
            double num = Math.Floor((double) (time / 1000.0)) % 60.0;
            if (num < 0.0)
            {
                num += 60.0;
            }
            return (int) num;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setDate)]
        public static double setDate(object thisob, double ddate)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            time = TimeClip(UTC(MakeDate(MakeDay(YearFromTime(time), (double) MonthFromTime(time), ddate), TimeWithinDay(time))));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setFullYear)]
        public static double setFullYear(object thisob, double dyear, object month, object date)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            double num2 = NotSpecified(month) ? ((double) MonthFromTime(time)) : Microsoft.JScript.Convert.ToNumber(month);
            double num3 = NotSpecified(date) ? ((double) DateFromTime(time)) : Microsoft.JScript.Convert.ToNumber(date);
            time = TimeClip(UTC(MakeDate(MakeDay(dyear, num2, num3), TimeWithinDay(time))));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setHours)]
        public static double setHours(object thisob, double dhour, object min, object sec, object msec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            double num2 = NotSpecified(min) ? ((double) MinFromTime(time)) : Microsoft.JScript.Convert.ToNumber(min);
            double num3 = NotSpecified(sec) ? ((double) SecFromTime(time)) : Microsoft.JScript.Convert.ToNumber(sec);
            double ms = NotSpecified(msec) ? ((double) msFromTime(time)) : Microsoft.JScript.Convert.ToNumber(msec);
            time = TimeClip(UTC(MakeDate(Day(time), MakeTime(dhour, num2, num3, ms))));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setMilliseconds)]
        public static double setMilliseconds(object thisob, double dmsec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            time = TimeClip(UTC(MakeDate(Day(time), MakeTime((double) HourFromTime(time), (double) MinFromTime(time), (double) SecFromTime(time), dmsec))));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setMinutes)]
        public static double setMinutes(object thisob, double dmin, object sec, object msec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            double num2 = NotSpecified(sec) ? ((double) SecFromTime(time)) : Microsoft.JScript.Convert.ToNumber(sec);
            double ms = NotSpecified(msec) ? ((double) msFromTime(time)) : Microsoft.JScript.Convert.ToNumber(msec);
            time = TimeClip(UTC(MakeDate(Day(time), MakeTime((double) HourFromTime(time), dmin, num2, ms))));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setMonth)]
        public static double setMonth(object thisob, double dmonth, object date)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            double num2 = NotSpecified(date) ? ((double) DateFromTime(time)) : Microsoft.JScript.Convert.ToNumber(date);
            time = TimeClip(UTC(MakeDate(MakeDay(YearFromTime(time), dmonth, num2), TimeWithinDay(time))));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setSeconds)]
        public static double setSeconds(object thisob, double dsec, object msec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            double ms = NotSpecified(msec) ? ((double) msFromTime(time)) : Microsoft.JScript.Convert.ToNumber(msec);
            time = TimeClip(UTC(MakeDate(Day(time), MakeTime((double) HourFromTime(time), (double) MinFromTime(time), dsec, ms))));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setTime)]
        public static double setTime(object thisob, double time)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            time = TimeClip(time);
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCDate)]
        public static double setUTCDate(object thisob, double ddate)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            time = TimeClip(MakeDate(MakeDay(YearFromTime(time), (double) MonthFromTime(time), ddate), TimeWithinDay(time)));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCFullYear)]
        public static double setUTCFullYear(object thisob, double dyear, object month, object date)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            double num2 = NotSpecified(month) ? ((double) MonthFromTime(time)) : Microsoft.JScript.Convert.ToNumber(month);
            double num3 = NotSpecified(date) ? ((double) DateFromTime(time)) : Microsoft.JScript.Convert.ToNumber(date);
            time = TimeClip(MakeDate(MakeDay(dyear, num2, num3), TimeWithinDay(time)));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCHours)]
        public static double setUTCHours(object thisob, double dhour, object min, object sec, object msec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            double num2 = NotSpecified(min) ? ((double) MinFromTime(time)) : Microsoft.JScript.Convert.ToNumber(min);
            double num3 = NotSpecified(sec) ? ((double) SecFromTime(time)) : Microsoft.JScript.Convert.ToNumber(sec);
            double ms = NotSpecified(msec) ? ((double) msFromTime(time)) : Microsoft.JScript.Convert.ToNumber(msec);
            time = TimeClip(MakeDate(Day(time), MakeTime(dhour, num2, num3, ms)));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCMilliseconds)]
        public static double setUTCMilliseconds(object thisob, double dmsec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            time = TimeClip(MakeDate(Day(time), MakeTime((double) HourFromTime(time), (double) MinFromTime(time), (double) SecFromTime(time), dmsec)));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCMinutes)]
        public static double setUTCMinutes(object thisob, double dmin, object sec, object msec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            double num2 = NotSpecified(sec) ? ((double) SecFromTime(time)) : Microsoft.JScript.Convert.ToNumber(sec);
            double ms = NotSpecified(msec) ? ((double) msFromTime(time)) : Microsoft.JScript.Convert.ToNumber(msec);
            time = TimeClip(MakeDate(Day(time), MakeTime((double) HourFromTime(time), dmin, num2, ms)));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCMonth)]
        public static double setUTCMonth(object thisob, double dmonth, object date)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            double num2 = NotSpecified(date) ? ((double) DateFromTime(time)) : Microsoft.JScript.Convert.ToNumber(date);
            time = TimeClip(MakeDate(MakeDay(YearFromTime(time), dmonth, num2), TimeWithinDay(time)));
            ((DateObject) thisob).value = time;
            return time;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCSeconds)]
        public static double setUTCSeconds(object thisob, double dsec, object msec)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = ((DateObject) thisob).value;
            double ms = NotSpecified(msec) ? ((double) msFromTime(time)) : Microsoft.JScript.Convert.ToNumber(msec);
            time = TimeClip(MakeDate(Day(time), MakeTime((double) HourFromTime(time), (double) MinFromTime(time), dsec, ms)));
            ((DateObject) thisob).value = time;
            return time;
        }

        [NotRecommended("setYear"), JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setYear)]
        public static double setYear(object thisob, double dyear)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            double time = LocalTime(((DateObject) thisob).value);
            if (double.IsNaN(dyear))
            {
                ((DateObject) thisob).value = dyear;
                return dyear;
            }
            dyear = Microsoft.JScript.Convert.ToInteger(dyear);
            if ((0.0 <= dyear) && (dyear <= 99.0))
            {
                dyear += 1900.0;
            }
            time = TimeClip(UTC(MakeDate(MakeDay(dyear, (double) MonthFromTime(time), (double) DateFromTime(time)), TimeWithinDay(time))));
            ((DateObject) thisob).value = time;
            return time;
        }

        internal static double TimeClip(double time)
        {
            if (!double.IsInfinity(time) && ((-8.64E+15 <= time) && (time <= 8.64E+15)))
            {
                return (double) ((long) time);
            }
            return double.NaN;
        }

        private static double TimeFromYear(double year)
        {
            return (86400000.0 * DayFromYear(year));
        }

        private static double TimeWithinDay(double time)
        {
            double num = time % 86400000.0;
            if (num < 0.0)
            {
                num += 86400000.0;
            }
            return num;
        }

        private static string TimeZoneID(double utcTime)
        {
            int num = (int) (localStandardTZA / 3600000.0);
            if (DaylightSavingsTime(utcTime + localStandardTZA))
            {
                switch (num)
                {
                    case -8:
                        return "PDT";

                    case -7:
                        return "MDT";

                    case -6:
                        return "CDT";

                    case -5:
                        return "EDT";
                }
            }
            else
            {
                switch (num)
                {
                    case -8:
                        return "PST";

                    case -7:
                        return "MST";

                    case -6:
                        return "CST";

                    case -5:
                        return "EST";
                }
            }
            return (((num >= 0) ? "UTC+" : "UTC") + num.ToString(CultureInfo.InvariantCulture));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toDateString)]
        public static string toDateString(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            return DateToDateString(((DateObject) thisob).value);
        }

        [NotRecommended("toGMTString"), JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toGMTString)]
        public static string toGMTString(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            return UTCDateToString(((DateObject) thisob).value);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toLocaleDateString)]
        public static string toLocaleDateString(object thisob)
        {
            object obj2 = getVarDate(thisob);
            if (obj2 != null)
            {
                DateTime time = (DateTime) obj2;
                return time.ToLongDateString();
            }
            return DateToDateString(((DateObject) thisob).value);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toLocaleString)]
        public static string toLocaleString(object thisob)
        {
            object obj2 = getVarDate(thisob);
            if (obj2 != null)
            {
                DateTime time = (DateTime) obj2;
                DateTime time2 = (DateTime) obj2;
                return (time.ToLongDateString() + " " + time2.ToLongTimeString());
            }
            return DateToString(((DateObject) thisob).value);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toLocaleTimeString)]
        public static string toLocaleTimeString(object thisob)
        {
            object obj2 = getVarDate(thisob);
            if (obj2 != null)
            {
                DateTime time = (DateTime) obj2;
                return time.ToLongTimeString();
            }
            return DateToTimeString(((DateObject) thisob).value);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toString)]
        public static string toString(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            return DateToString(((DateObject) thisob).value);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toTimeString)]
        public static string toTimeString(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            return DateToTimeString(((DateObject) thisob).value);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toUTCString)]
        public static string toUTCString(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            return UTCDateToString(((DateObject) thisob).value);
        }

        internal static double UTC(double localTime)
        {
            return (localTime - (DaylightSavingsTime(localTime) ? localDaylightTZA : localStandardTZA));
        }

        internal static string UTCDateToString(double utcTime)
        {
            if (double.IsNaN(utcTime))
            {
                return "NaN";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(WeekDayName[WeekDay(utcTime)]);
            sb.Append(", ");
            sb.Append(DateFromTime(utcTime));
            sb.Append(" ");
            sb.Append(MonthName[MonthFromTime(utcTime)]);
            sb.Append(" ");
            sb.Append(YearString(utcTime));
            sb.Append(" ");
            AppendTime(utcTime, sb);
            sb.Append(" UTC");
            return sb.ToString();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_valueOf)]
        public static double valueOf(object thisob)
        {
            if (!(thisob is DateObject))
            {
                throw new JScriptException(JSError.DateExpected);
            }
            return ((DateObject) thisob).value;
        }

        private static int WeekDay(double time)
        {
            double num = (Day(time) + 4.0) % 7.0;
            if (num < 0.0)
            {
                num += 7.0;
            }
            return (int) num;
        }

        private static double YearFromTime(double time)
        {
            double num = Math.Floor((double) (time / 86400000.0));
            double year = 1970.0 + Math.Floor((double) (((400.0 * num) + 398.0) / 146097.0));
            double num3 = DayFromYear(year);
            if (num < num3)
            {
                year--;
            }
            return year;
        }

        private static string YearString(double time)
        {
            double num = YearFromTime(time);
            if (num > 0.0)
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }
            double num2 = 1.0 - num;
            return (num2.ToString(CultureInfo.InvariantCulture) + " B.C.");
        }

        public static DateConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

