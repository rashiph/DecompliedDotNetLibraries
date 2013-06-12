namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct XsdDuration
    {
        private const uint NegativeBit = 0x80000000;
        private int years;
        private int months;
        private int days;
        private int hours;
        private int minutes;
        private int seconds;
        private uint nanoseconds;
        public XsdDuration(bool isNegative, int years, int months, int days, int hours, int minutes, int seconds, int nanoseconds)
        {
            if (years < 0)
            {
                throw new ArgumentOutOfRangeException("years");
            }
            if (months < 0)
            {
                throw new ArgumentOutOfRangeException("months");
            }
            if (days < 0)
            {
                throw new ArgumentOutOfRangeException("days");
            }
            if (hours < 0)
            {
                throw new ArgumentOutOfRangeException("hours");
            }
            if (minutes < 0)
            {
                throw new ArgumentOutOfRangeException("minutes");
            }
            if (seconds < 0)
            {
                throw new ArgumentOutOfRangeException("seconds");
            }
            if ((nanoseconds < 0) || (nanoseconds > 0x3b9ac9ff))
            {
                throw new ArgumentOutOfRangeException("nanoseconds");
            }
            this.years = years;
            this.months = months;
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.nanoseconds = (uint) nanoseconds;
            if (isNegative)
            {
                this.nanoseconds |= 0x80000000;
            }
        }

        public XsdDuration(TimeSpan timeSpan) : this(timeSpan, DurationType.Duration)
        {
        }

        public XsdDuration(TimeSpan timeSpan, DurationType durationType)
        {
            ulong num2;
            bool flag;
            long ticks = timeSpan.Ticks;
            if (ticks < 0L)
            {
                flag = true;
                num2 = (ulong) -ticks;
            }
            else
            {
                flag = false;
                num2 = (ulong) ticks;
            }
            if (durationType == DurationType.YearMonthDuration)
            {
                int years = (int) (num2 / ((ulong) 0x11ed178c6c000L));
                int months = (int) ((num2 % ((ulong) 0x11ed178c6c000L)) / ((ulong) 0x1792f8648000L));
                if (months == 12)
                {
                    years++;
                    months = 0;
                }
                this = new XsdDuration(flag, years, months, 0, 0, 0, 0, 0);
            }
            else
            {
                this.nanoseconds = ((uint) (num2 % ((ulong) 0x989680L))) * 100;
                if (flag)
                {
                    this.nanoseconds |= 0x80000000;
                }
                this.years = 0;
                this.months = 0;
                this.days = (int) (num2 / ((ulong) 0xc92a69c000L));
                this.hours = (int) ((num2 / ((ulong) 0x861c46800L)) % ((ulong) 0x18L));
                this.minutes = (int) ((num2 / ((ulong) 0x23c34600L)) % ((ulong) 60L));
                this.seconds = (int) ((num2 / ((ulong) 0x989680L)) % ((ulong) 60L));
            }
        }

        public XsdDuration(string s) : this(s, DurationType.Duration)
        {
        }

        public XsdDuration(string s, DurationType durationType)
        {
            XsdDuration duration;
            Exception exception = TryParse(s, durationType, out duration);
            if (exception != null)
            {
                throw exception;
            }
            this.years = duration.Years;
            this.months = duration.Months;
            this.days = duration.Days;
            this.hours = duration.Hours;
            this.minutes = duration.Minutes;
            this.seconds = duration.Seconds;
            this.nanoseconds = (uint) duration.Nanoseconds;
            if (duration.IsNegative)
            {
                this.nanoseconds |= 0x80000000;
            }
        }

        public bool IsNegative
        {
            get
            {
                return ((this.nanoseconds & 0x80000000) != 0);
            }
        }
        public int Years
        {
            get
            {
                return this.years;
            }
        }
        public int Months
        {
            get
            {
                return this.months;
            }
        }
        public int Days
        {
            get
            {
                return this.days;
            }
        }
        public int Hours
        {
            get
            {
                return this.hours;
            }
        }
        public int Minutes
        {
            get
            {
                return this.minutes;
            }
        }
        public int Seconds
        {
            get
            {
                return this.seconds;
            }
        }
        public int Nanoseconds
        {
            get
            {
                return (((int) this.nanoseconds) & 0x7fffffff);
            }
        }
        public int Microseconds
        {
            get
            {
                return (this.Nanoseconds / 0x3e8);
            }
        }
        public int Milliseconds
        {
            get
            {
                return (this.Nanoseconds / 0xf4240);
            }
        }
        public XsdDuration Normalize()
        {
            int years = this.Years;
            int months = this.Months;
            int days = this.Days;
            int hours = this.Hours;
            int minutes = this.Minutes;
            int seconds = this.Seconds;
            try
            {
                if (months >= 12)
                {
                    years += months / 12;
                    months = months % 12;
                }
                if (seconds >= 60)
                {
                    minutes += seconds / 60;
                    seconds = seconds % 60;
                }
                if (minutes >= 60)
                {
                    hours += minutes / 60;
                    minutes = minutes % 60;
                }
                if (hours >= 0x18)
                {
                    days += hours / 0x18;
                    hours = hours % 0x18;
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException(Res.GetString("XmlConvert_Overflow", new object[] { this.ToString(), "Duration" }));
            }
            return new XsdDuration(this.IsNegative, years, months, days, hours, minutes, seconds, this.Nanoseconds);
        }

        public TimeSpan ToTimeSpan()
        {
            return this.ToTimeSpan(DurationType.Duration);
        }

        public TimeSpan ToTimeSpan(DurationType durationType)
        {
            TimeSpan span;
            Exception exception = this.TryToTimeSpan(durationType, out span);
            if (exception != null)
            {
                throw exception;
            }
            return span;
        }

        internal Exception TryToTimeSpan(out TimeSpan result)
        {
            return this.TryToTimeSpan(DurationType.Duration, out result);
        }

        internal Exception TryToTimeSpan(DurationType durationType, out TimeSpan result)
        {
            Exception exception = null;
            ulong num = 0L;
            try
            {
                if (durationType != DurationType.DayTimeDuration)
                {
                    num += (((ulong) this.years) + (((ulong) this.months) / 12L)) * ((ulong) 0x16dL);
                    num += (((ulong) this.months) % 12L) * ((ulong) 30L);
                }
                if (durationType != DurationType.YearMonthDuration)
                {
                    num += (ulong) this.days;
                    num *= (ulong) 0x18L;
                    num += (ulong) this.hours;
                    num *= (ulong) 60L;
                    num += (ulong) this.minutes;
                    num *= (ulong) 60L;
                    num += (ulong) this.seconds;
                    num *= (ulong) 0x989680L;
                    num += ((ulong) this.Nanoseconds) / 100L;
                }
                else
                {
                    num *= (ulong) 0xc92a69c000L;
                }
                if (this.IsNegative)
                {
                    if (num == 9223372036854775808L)
                    {
                        result = new TimeSpan(-9223372036854775808L);
                    }
                    else
                    {
                        result = new TimeSpan(0L - ((long) num));
                    }
                }
                else
                {
                    result = new TimeSpan((long) num);
                }
                return null;
            }
            catch (OverflowException)
            {
                result = TimeSpan.MinValue;
                exception = new OverflowException(Res.GetString("XmlConvert_Overflow", new object[] { durationType, "TimeSpan" }));
            }
            return exception;
        }

        public override string ToString()
        {
            return this.ToString(DurationType.Duration);
        }

        internal string ToString(DurationType durationType)
        {
            StringBuilder builder = new StringBuilder(20);
            if (this.IsNegative)
            {
                builder.Append('-');
            }
            builder.Append('P');
            if (durationType != DurationType.DayTimeDuration)
            {
                if (this.years != 0)
                {
                    builder.Append(XmlConvert.ToString(this.years));
                    builder.Append('Y');
                }
                if (this.months != 0)
                {
                    builder.Append(XmlConvert.ToString(this.months));
                    builder.Append('M');
                }
            }
            if (durationType != DurationType.YearMonthDuration)
            {
                if (this.days != 0)
                {
                    builder.Append(XmlConvert.ToString(this.days));
                    builder.Append('D');
                }
                if (((this.hours != 0) || (this.minutes != 0)) || ((this.seconds != 0) || (this.Nanoseconds != 0)))
                {
                    builder.Append('T');
                    if (this.hours != 0)
                    {
                        builder.Append(XmlConvert.ToString(this.hours));
                        builder.Append('H');
                    }
                    if (this.minutes != 0)
                    {
                        builder.Append(XmlConvert.ToString(this.minutes));
                        builder.Append('M');
                    }
                    int nanoseconds = this.Nanoseconds;
                    if ((this.seconds != 0) || (nanoseconds != 0))
                    {
                        builder.Append(XmlConvert.ToString(this.seconds));
                        if (nanoseconds != 0)
                        {
                            builder.Append('.');
                            int length = builder.Length;
                            builder.Length += 9;
                            int num3 = builder.Length - 1;
                            for (int i = num3; i >= length; i--)
                            {
                                int num2 = nanoseconds % 10;
                                builder[i] = (char) (num2 + 0x30);
                                if ((num3 == i) && (num2 == 0))
                                {
                                    num3--;
                                }
                                nanoseconds /= 10;
                            }
                            builder.Length = num3 + 1;
                        }
                        builder.Append('S');
                    }
                }
                if (builder[builder.Length - 1] == 'P')
                {
                    builder.Append("T0S");
                }
            }
            else if (builder[builder.Length - 1] == 'P')
            {
                builder.Append("0M");
            }
            return builder.ToString();
        }

        internal static Exception TryParse(string s, out XsdDuration result)
        {
            return TryParse(s, DurationType.Duration, out result);
        }

        internal static Exception TryParse(string s, DurationType durationType, out XsdDuration result)
        {
            int num2;
            Parts hasNone = Parts.HasNone;
            result = new XsdDuration();
            s = s.Trim();
            int length = s.Length;
            int offset = 0;
            int numDigits = 0;
            if (offset >= length)
            {
                goto Label_02D8;
            }
            if (s[offset] == '-')
            {
                offset++;
                result.nanoseconds = 0x80000000;
            }
            else
            {
                result.nanoseconds = 0;
            }
            if ((offset >= length) || (s[offset++] != 'P'))
            {
                goto Label_02D8;
            }
            if (TryParseDigits(s, ref offset, false, out num2, out numDigits) != null)
            {
                goto Label_0301;
            }
            if (offset >= length)
            {
                goto Label_02D8;
            }
            if (s[offset] == 'Y')
            {
                if (numDigits == 0)
                {
                    goto Label_02D8;
                }
                hasNone |= Parts.HasYears;
                result.years = num2;
                if (++offset == length)
                {
                    goto Label_02BB;
                }
                if (TryParseDigits(s, ref offset, false, out num2, out numDigits) != null)
                {
                    goto Label_0301;
                }
                if (offset >= length)
                {
                    goto Label_02D8;
                }
            }
            if (s[offset] == 'M')
            {
                if (numDigits == 0)
                {
                    goto Label_02D8;
                }
                hasNone |= Parts.HasMonths;
                result.months = num2;
                if (++offset == length)
                {
                    goto Label_02BB;
                }
                if (TryParseDigits(s, ref offset, false, out num2, out numDigits) != null)
                {
                    goto Label_0301;
                }
                if (offset >= length)
                {
                    goto Label_02D8;
                }
            }
            if (s[offset] == 'D')
            {
                if (numDigits == 0)
                {
                    goto Label_02D8;
                }
                hasNone |= Parts.HasDays;
                result.days = num2;
                if (++offset == length)
                {
                    goto Label_02BB;
                }
                if (TryParseDigits(s, ref offset, false, out num2, out numDigits) != null)
                {
                    goto Label_0301;
                }
                if (offset >= length)
                {
                    goto Label_02D8;
                }
            }
            if (s[offset] == 'T')
            {
                if (numDigits != 0)
                {
                    goto Label_02D8;
                }
                offset++;
                if (TryParseDigits(s, ref offset, false, out num2, out numDigits) != null)
                {
                    goto Label_0301;
                }
                if (offset >= length)
                {
                    goto Label_02D8;
                }
                if (s[offset] == 'H')
                {
                    if (numDigits == 0)
                    {
                        goto Label_02D8;
                    }
                    hasNone |= Parts.HasHours;
                    result.hours = num2;
                    if (++offset == length)
                    {
                        goto Label_02BB;
                    }
                    if (TryParseDigits(s, ref offset, false, out num2, out numDigits) != null)
                    {
                        goto Label_0301;
                    }
                    if (offset >= length)
                    {
                        goto Label_02D8;
                    }
                }
                if (s[offset] == 'M')
                {
                    if (numDigits == 0)
                    {
                        goto Label_02D8;
                    }
                    hasNone |= Parts.HasMinutes;
                    result.minutes = num2;
                    if (++offset == length)
                    {
                        goto Label_02BB;
                    }
                    if (TryParseDigits(s, ref offset, false, out num2, out numDigits) != null)
                    {
                        goto Label_0301;
                    }
                    if (offset >= length)
                    {
                        goto Label_02D8;
                    }
                }
                if (s[offset] == '.')
                {
                    offset++;
                    hasNone |= Parts.HasSeconds;
                    result.seconds = num2;
                    if (TryParseDigits(s, ref offset, true, out num2, out numDigits) != null)
                    {
                        goto Label_0301;
                    }
                    if (numDigits == 0)
                    {
                        num2 = 0;
                    }
                    while (numDigits > 9)
                    {
                        num2 /= 10;
                        numDigits--;
                    }
                    while (numDigits < 9)
                    {
                        num2 *= 10;
                        numDigits++;
                    }
                    result.nanoseconds |= (uint) num2;
                    if ((offset >= length) || (s[offset] != 'S'))
                    {
                        goto Label_02D8;
                    }
                    if (++offset != length)
                    {
                        goto Label_02B3;
                    }
                    goto Label_02BB;
                }
                if (s[offset] == 'S')
                {
                    if (numDigits == 0)
                    {
                        goto Label_02D8;
                    }
                    hasNone |= Parts.HasSeconds;
                    result.seconds = num2;
                    if (++offset == length)
                    {
                        goto Label_02BB;
                    }
                }
            }
        Label_02B3:
            if ((numDigits != 0) || (offset != length))
            {
                goto Label_02D8;
            }
        Label_02BB:
            if (hasNone == Parts.HasNone)
            {
                goto Label_02D8;
            }
            if (durationType == DurationType.DayTimeDuration)
            {
                if ((hasNone & (Parts.HasMonths | Parts.HasYears)) == Parts.HasNone)
                {
                    goto Label_02D6;
                }
                goto Label_02D8;
            }
            if ((durationType == DurationType.YearMonthDuration) && ((hasNone & ~(Parts.HasMonths | Parts.HasYears)) != Parts.HasNone))
            {
                goto Label_02D8;
            }
        Label_02D6:
            return null;
        Label_02D8:;
            return new FormatException(Res.GetString("XmlConvert_BadFormat", new object[] { s, durationType }));
        Label_0301:;
            return new OverflowException(Res.GetString("XmlConvert_Overflow", new object[] { s, durationType }));
        }

        private static string TryParseDigits(string s, ref int offset, bool eatDigits, out int result, out int numDigits)
        {
            int num = offset;
            int length = s.Length;
            result = 0;
            numDigits = 0;
            while (((offset < length) && (s[offset] >= '0')) && (s[offset] <= '9'))
            {
                int num3 = s[offset] - '0';
                if (result > ((0x7fffffff - num3) / 10))
                {
                    if (!eatDigits)
                    {
                        return "XmlConvert_Overflow";
                    }
                    numDigits = offset - num;
                    while (((offset < length) && (s[offset] >= '0')) && (s[offset] <= '9'))
                    {
                        offset++;
                    }
                    return null;
                }
                result = (result * 10) + num3;
                offset++;
            }
            numDigits = offset - num;
            return null;
        }
        public enum DurationType
        {
            Duration,
            YearMonthDuration,
            DayTimeDuration
        }

        private enum Parts
        {
            HasDays = 4,
            HasHours = 8,
            HasMinutes = 0x10,
            HasMonths = 2,
            HasNone = 0,
            HasSeconds = 0x20,
            HasYears = 1
        }
    }
}

