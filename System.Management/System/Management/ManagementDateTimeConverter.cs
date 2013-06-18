namespace System.Management
{
    using System;
    using System.Globalization;

    public sealed class ManagementDateTimeConverter
    {
        private const long MAXDATE_INTIMESPAN = 0x5f5e0ffL;
        private const int MAXSIZE_UTC_DMTF = 0x3e7;
        private const int SIZEOFDMTFDATETIME = 0x19;

        private ManagementDateTimeConverter()
        {
        }

        public static DateTime ToDateTime(string dmtfDate)
        {
            int year = DateTime.MinValue.Year;
            int month = DateTime.MinValue.Month;
            int day = DateTime.MinValue.Day;
            int hour = DateTime.MinValue.Hour;
            int minute = DateTime.MinValue.Minute;
            int second = DateTime.MinValue.Second;
            int millisecond = 0;
            string str = dmtfDate;
            DateTime minValue = DateTime.MinValue;
            if (str == null)
            {
                throw new ArgumentOutOfRangeException("dmtfDate");
            }
            if (str.Length == 0)
            {
                throw new ArgumentOutOfRangeException("dmtfDate");
            }
            if (str.Length != 0x19)
            {
                throw new ArgumentOutOfRangeException("dmtfDate");
            }
            IFormatProvider format = (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int));
            long num8 = 0L;
            try
            {
                string str2 = string.Empty;
                str2 = str.Substring(0, 4);
                if ("****" != str2)
                {
                    year = int.Parse(str2, format);
                }
                str2 = str.Substring(4, 2);
                if ("**" != str2)
                {
                    month = int.Parse(str2, format);
                }
                str2 = str.Substring(6, 2);
                if ("**" != str2)
                {
                    day = int.Parse(str2, format);
                }
                str2 = str.Substring(8, 2);
                if ("**" != str2)
                {
                    hour = int.Parse(str2, format);
                }
                str2 = str.Substring(10, 2);
                if ("**" != str2)
                {
                    minute = int.Parse(str2, format);
                }
                str2 = str.Substring(12, 2);
                if ("**" != str2)
                {
                    second = int.Parse(str2, format);
                }
                str2 = str.Substring(15, 6);
                if ("******" != str2)
                {
                    num8 = long.Parse(str2, (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(long))) * 10L;
                }
                if ((((year < 0) || (month < 0)) || ((day < 0) || (hour < 0))) || (((minute < 0) || (second < 0)) || (num8 < 0L)))
                {
                    throw new ArgumentOutOfRangeException("dmtfDate");
                }
            }
            catch
            {
                throw new ArgumentOutOfRangeException("dmtfDate");
            }
            minValue = new DateTime(year, month, day, hour, minute, second, millisecond);
            minValue = minValue.AddTicks(num8);
            long num9 = TimeZone.CurrentTimeZone.GetUtcOffset(minValue).Ticks / 0x23c34600L;
            int num10 = 0;
            string s = str.Substring(0x16, 3);
            long num11 = 0L;
            if (!("***" != s))
            {
                return minValue;
            }
            s = str.Substring(0x15, 4);
            try
            {
                num10 = int.Parse(s, format);
            }
            catch
            {
                throw new ArgumentOutOfRangeException();
            }
            num11 = num10 - num9;
            return minValue.AddMinutes((double) (num11 * -1L));
        }

        public static string ToDmtfDateTime(DateTime date)
        {
            string str = string.Empty;
            TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(date);
            long num = utcOffset.Ticks / 0x23c34600L;
            IFormatProvider format = (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int));
            if (Math.Abs(num) > 0x3e7L)
            {
                date = date.ToUniversalTime();
                str = "+000";
            }
            else if (utcOffset.Ticks >= 0L)
            {
                long num3 = utcOffset.Ticks / 0x23c34600L;
                str = "+" + num3.ToString(format).PadLeft(3, '0');
            }
            else
            {
                string str2 = num.ToString(format);
                str = "-" + str2.Substring(1, str2.Length - 1).PadLeft(3, '0');
            }
            string str3 = ((date.Year.ToString(format).PadLeft(4, '0') + date.Month.ToString(format).PadLeft(2, '0') + date.Day.ToString(format).PadLeft(2, '0')) + date.Hour.ToString(format).PadLeft(2, '0') + date.Minute.ToString(format).PadLeft(2, '0')) + date.Second.ToString(format).PadLeft(2, '0') + ".";
            DateTime time = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
            string str4 = (((date.Ticks - time.Ticks) * 0x3e8L) / 0x2710L).ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(long)));
            if (str4.Length > 6)
            {
                str4 = str4.Substring(0, 6);
            }
            return (str3 + str4.PadLeft(6, '0') + str);
        }

        public static string ToDmtfTimeInterval(TimeSpan timespan)
        {
            string str = timespan.Days.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int))).PadLeft(8, '0');
            IFormatProvider format = (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int));
            if ((timespan.Days > 0x5f5e0ffL) || (timespan < TimeSpan.Zero))
            {
                throw new ArgumentOutOfRangeException();
            }
            str = (str + timespan.Hours.ToString(format).PadLeft(2, '0') + timespan.Minutes.ToString(format).PadLeft(2, '0')) + timespan.Seconds.ToString(format).PadLeft(2, '0') + ".";
            TimeSpan span = new TimeSpan(timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds, 0);
            string str2 = (((timespan.Ticks - span.Ticks) * 0x3e8L) / 0x2710L).ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(long)));
            if (str2.Length > 6)
            {
                str2 = str2.Substring(0, 6);
            }
            return (str + str2.PadLeft(6, '0') + ":000");
        }

        public static TimeSpan ToTimeSpan(string dmtfTimespan)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            IFormatProvider format = (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(int));
            string str = dmtfTimespan;
            TimeSpan minValue = TimeSpan.MinValue;
            if (str == null)
            {
                throw new ArgumentOutOfRangeException("dmtfTimespan");
            }
            if (str.Length == 0)
            {
                throw new ArgumentOutOfRangeException("dmtfTimespan");
            }
            if (str.Length != 0x19)
            {
                throw new ArgumentOutOfRangeException("dmtfTimespan");
            }
            if (str.Substring(0x15, 4) != ":000")
            {
                throw new ArgumentOutOfRangeException("dmtfTimespan");
            }
            long num5 = 0L;
            try
            {
                days = int.Parse(str.Substring(0, 8), format);
                hours = int.Parse(str.Substring(8, 2), format);
                minutes = int.Parse(str.Substring(10, 2), format);
                seconds = int.Parse(str.Substring(12, 2), format);
                num5 = long.Parse(str.Substring(15, 6), (IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(long))) * 10L;
            }
            catch
            {
                throw new ArgumentOutOfRangeException("dmtfTimespan");
            }
            if (((days < 0) || (hours < 0)) || (((minutes < 0) || (seconds < 0)) || (num5 < 0L)))
            {
                throw new ArgumentOutOfRangeException("dmtfTimespan");
            }
            minValue = new TimeSpan(days, hours, minutes, seconds, 0);
            TimeSpan span2 = TimeSpan.FromTicks(num5);
            return (minValue + span2);
        }
    }
}

