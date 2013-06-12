namespace System.Net.Mime
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal class SmtpDateTime
    {
        internal static readonly char[] allowedWhiteSpaceChars = new char[] { ' ', '\t' };
        private readonly DateTime date;
        internal const string dateFormatWithDayOfWeek = "ddd, dd MMM yyyy HH:mm:ss";
        internal const string dateFormatWithDayOfWeekAndNoSeconds = "ddd, dd MMM yyyy HH:mm";
        internal const string dateFormatWithoutDayOfWeek = "dd MMM yyyy HH:mm:ss";
        internal const string dateFormatWithoutDayOfWeekAndNoSeconds = "dd MMM yyyy HH:mm";
        internal const int maxMinuteValue = 0x3b;
        internal const int offsetLength = 5;
        internal static readonly int offsetMaxValue = 0x26e7;
        internal static readonly long timeSpanMaxTicks = 0x3460cf55a00L;
        private readonly TimeSpan timeZone;
        internal static IDictionary<string, TimeSpan> timeZoneOffsetLookup = InitializeShortHandLookups();
        private readonly bool unknownTimeZone;
        internal const string unknownTimeZoneDefaultOffset = "-0000";
        internal const string utcDefaultTimeZoneOffset = "+0000";
        internal static readonly string[] validDateTimeFormats = new string[] { "ddd, dd MMM yyyy HH:mm:ss", "dd MMM yyyy HH:mm:ss", "ddd, dd MMM yyyy HH:mm", "dd MMM yyyy HH:mm" };

        internal SmtpDateTime(DateTime value)
        {
            this.date = value;
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    this.unknownTimeZone = true;
                    return;

                case DateTimeKind.Utc:
                    this.timeZone = TimeSpan.Zero;
                    return;

                case DateTimeKind.Local:
                {
                    TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(value);
                    this.timeZone = this.ValidateAndGetSanitizedTimeSpan(utcOffset);
                    return;
                }
            }
        }

        internal SmtpDateTime(string value)
        {
            string str;
            this.date = this.ParseValue(value, out str);
            if (!this.TryParseTimeZoneString(str, out this.timeZone))
            {
                this.unknownTimeZone = true;
            }
        }

        internal string FormatDate(DateTime value)
        {
            return value.ToString("ddd, dd MMM yyyy H:mm:ss");
        }

        internal static IDictionary<string, TimeSpan> InitializeShortHandLookups()
        {
            timeZoneOffsetLookup = new Dictionary<string, TimeSpan>();
            timeZoneOffsetLookup.Add("UT", TimeSpan.Zero);
            timeZoneOffsetLookup.Add("GMT", TimeSpan.Zero);
            timeZoneOffsetLookup.Add("EDT", new TimeSpan(-4, 0, 0));
            timeZoneOffsetLookup.Add("EST", new TimeSpan(-5, 0, 0));
            timeZoneOffsetLookup.Add("CDT", new TimeSpan(-5, 0, 0));
            timeZoneOffsetLookup.Add("CST", new TimeSpan(-6, 0, 0));
            timeZoneOffsetLookup.Add("MDT", new TimeSpan(-6, 0, 0));
            timeZoneOffsetLookup.Add("MST", new TimeSpan(-7, 0, 0));
            timeZoneOffsetLookup.Add("PDT", new TimeSpan(-7, 0, 0));
            timeZoneOffsetLookup.Add("PST", new TimeSpan(-8, 0, 0));
            return timeZoneOffsetLookup;
        }

        internal DateTime ParseValue(string data, out string timeZone)
        {
            DateTime time;
            if (string.IsNullOrEmpty(data))
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
            int index = data.IndexOf(':');
            if (index == -1)
            {
                throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
            }
            int length = data.IndexOfAny(allowedWhiteSpaceChars, index);
            if (length == -1)
            {
                throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
            }
            if (!DateTime.TryParseExact(data.Substring(0, length).Trim(), validDateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out time))
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
            string str2 = data.Substring(length).Trim();
            int num3 = str2.IndexOfAny(allowedWhiteSpaceChars);
            if (num3 != -1)
            {
                str2 = str2.Substring(0, num3);
            }
            if (string.IsNullOrEmpty(str2))
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
            timeZone = str2;
            return time;
        }

        internal string TimeSpanToOffset(TimeSpan span)
        {
            if (span.Ticks == 0L)
            {
                return "+0000";
            }
            uint num = (uint) Math.Abs(Math.Floor(span.TotalHours));
            uint num2 = (uint) Math.Abs(span.Minutes);
            string str = (span.Ticks > 0L) ? "+" : "-";
            if (num < 10)
            {
                str = str + "0";
            }
            str = str + num.ToString();
            if (num2 < 10)
            {
                str = str + "0";
            }
            return (str + num2.ToString());
        }

        public override string ToString()
        {
            if (this.unknownTimeZone)
            {
                return string.Format("{0} {1}", this.FormatDate(this.date), "-0000");
            }
            return string.Format("{0} {1}", this.FormatDate(this.date), this.TimeSpanToOffset(this.timeZone));
        }

        internal bool TryParseTimeZoneString(string timeZoneString, out TimeSpan timeZone)
        {
            timeZone = TimeSpan.Zero;
            if (timeZoneString != "-0000")
            {
                if ((timeZoneString[0] == '+') || (timeZoneString[0] == '-'))
                {
                    bool flag;
                    int num;
                    int num2;
                    this.ValidateAndGetTimeZoneOffsetValues(timeZoneString, out flag, out num, out num2);
                    if (!flag)
                    {
                        if (num != 0)
                        {
                            num *= -1;
                        }
                        else if (num2 != 0)
                        {
                            num2 *= -1;
                        }
                    }
                    timeZone = new TimeSpan(num, num2, 0);
                    return true;
                }
                this.ValidateTimeZoneShortHandValue(timeZoneString);
                if (timeZoneOffsetLookup.ContainsKey(timeZoneString))
                {
                    timeZone = timeZoneOffsetLookup[timeZoneString];
                    return true;
                }
            }
            return false;
        }

        internal TimeSpan ValidateAndGetSanitizedTimeSpan(TimeSpan span)
        {
            TimeSpan span2 = new TimeSpan(span.Days, span.Hours, span.Minutes, 0, 0);
            if (Math.Abs(span2.Ticks) > timeSpanMaxTicks)
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
            return span2;
        }

        internal void ValidateAndGetTimeZoneOffsetValues(string offset, out bool positive, out int hours, out int minutes)
        {
            if (offset.Length != 5)
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
            positive = offset.StartsWith("+");
            if (!int.TryParse(offset.Substring(1, 2), NumberStyles.None, CultureInfo.InvariantCulture, out hours))
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
            if (!int.TryParse(offset.Substring(3, 2), NumberStyles.None, CultureInfo.InvariantCulture, out minutes))
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
            if (minutes > 0x3b)
            {
                throw new FormatException(SR.GetString("MailDateInvalidFormat"));
            }
        }

        internal void ValidateTimeZoneShortHandValue(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsLetter(value, i))
                {
                    throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
                }
            }
        }

        internal DateTime Date
        {
            get
            {
                if (this.unknownTimeZone)
                {
                    return DateTime.SpecifyKind(this.date, DateTimeKind.Unspecified);
                }
                DateTimeOffset offset = new DateTimeOffset(this.date, this.timeZone);
                return offset.LocalDateTime;
            }
        }
    }
}

