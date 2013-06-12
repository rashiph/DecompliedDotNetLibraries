namespace System
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct TimeSpan : IComparable, IComparable<TimeSpan>, IEquatable<TimeSpan>, IFormattable
    {
        public const long TicksPerMillisecond = 0x2710L;
        private const double MillisecondsPerTick = 0.0001;
        public const long TicksPerSecond = 0x989680L;
        private const double SecondsPerTick = 1E-07;
        public const long TicksPerMinute = 0x23c34600L;
        private const double MinutesPerTick = 1.6666666666666667E-09;
        public const long TicksPerHour = 0x861c46800L;
        private const double HoursPerTick = 2.7777777777777777E-11;
        public const long TicksPerDay = 0xc92a69c000L;
        private const double DaysPerTick = 1.1574074074074074E-12;
        private const int MillisPerSecond = 0x3e8;
        private const int MillisPerMinute = 0xea60;
        private const int MillisPerHour = 0x36ee80;
        private const int MillisPerDay = 0x5265c00;
        internal const long MaxSeconds = 0xd6bf94d5e5L;
        internal const long MinSeconds = -922337203685L;
        internal const long MaxMilliSeconds = 0x346dc5d638865L;
        internal const long MinMilliSeconds = -922337203685477L;
        internal const long TicksPerTenthSecond = 0xf4240L;
        public static readonly TimeSpan Zero;
        public static readonly TimeSpan MaxValue;
        public static readonly TimeSpan MinValue;
        internal long _ticks;
        private static bool _legacyConfigChecked;
        private static bool _legacyMode;
        public TimeSpan(long ticks)
        {
            this._ticks = ticks;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public TimeSpan(int hours, int minutes, int seconds)
        {
            this._ticks = TimeToTicks(hours, minutes, seconds);
        }

        public TimeSpan(int days, int hours, int minutes, int seconds) : this(days, hours, minutes, seconds, 0)
        {
        }

        public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            long num = ((((((days * 0xe10L) * 0x18L) + (hours * 0xe10L)) + (minutes * 60L)) + seconds) * 0x3e8L) + milliseconds;
            if ((num > 0x346dc5d638865L) || (num < -922337203685477L))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Overflow_TimeSpanTooLong"));
            }
            this._ticks = num * 0x2710L;
        }

        public long Ticks
        {
            get
            {
                return this._ticks;
            }
        }
        public int Days
        {
            get
            {
                return (int) (this._ticks / 0xc92a69c000L);
            }
        }
        public int Hours
        {
            get
            {
                return (int) ((this._ticks / 0x861c46800L) % 0x18L);
            }
        }
        public int Milliseconds
        {
            get
            {
                return (int) ((this._ticks / 0x2710L) % 0x3e8L);
            }
        }
        public int Minutes
        {
            get
            {
                return (int) ((this._ticks / 0x23c34600L) % 60L);
            }
        }
        public int Seconds
        {
            get
            {
                return (int) ((this._ticks / 0x989680L) % 60L);
            }
        }
        public double TotalDays
        {
            get
            {
                return (this._ticks * 1.1574074074074074E-12);
            }
        }
        public double TotalHours
        {
            get
            {
                return (this._ticks * 2.7777777777777777E-11);
            }
        }
        public double TotalMilliseconds
        {
            get
            {
                double num = this._ticks * 0.0001;
                if (num > 922337203685477)
                {
                    return 922337203685477;
                }
                if (num < -922337203685477)
                {
                    return -922337203685477;
                }
                return num;
            }
        }
        public double TotalMinutes
        {
            get
            {
                return (this._ticks * 1.6666666666666667E-09);
            }
        }
        public double TotalSeconds
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this._ticks * 1E-07);
            }
        }
        public TimeSpan Add(TimeSpan ts)
        {
            long ticks = this._ticks + ts._ticks;
            if (((this._ticks >> 0x3f) == (ts._ticks >> 0x3f)) && ((this._ticks >> 0x3f) != (ticks >> 0x3f)))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong"));
            }
            return new TimeSpan(ticks);
        }

        public static int Compare(TimeSpan t1, TimeSpan t2)
        {
            if (t1._ticks > t2._ticks)
            {
                return 1;
            }
            if (t1._ticks < t2._ticks)
            {
                return -1;
            }
            return 0;
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is TimeSpan))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeTimeSpan"));
            }
            long num = ((TimeSpan) value)._ticks;
            if (this._ticks > num)
            {
                return 1;
            }
            if (this._ticks < num)
            {
                return -1;
            }
            return 0;
        }

        public int CompareTo(TimeSpan value)
        {
            long num = value._ticks;
            if (this._ticks > num)
            {
                return 1;
            }
            if (this._ticks < num)
            {
                return -1;
            }
            return 0;
        }

        public static TimeSpan FromDays(double value)
        {
            return Interval(value, 0x5265c00);
        }

        public TimeSpan Duration()
        {
            if (this.Ticks == MinValue.Ticks)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Duration"));
            }
            return new TimeSpan((this._ticks >= 0L) ? this._ticks : -this._ticks);
        }

        public override bool Equals(object value)
        {
            return ((value is TimeSpan) && (this._ticks == ((TimeSpan) value)._ticks));
        }

        public bool Equals(TimeSpan obj)
        {
            return (this._ticks == obj._ticks);
        }

        public static bool Equals(TimeSpan t1, TimeSpan t2)
        {
            return (t1._ticks == t2._ticks);
        }

        public override int GetHashCode()
        {
            return (((int) this._ticks) ^ ((int) (this._ticks >> 0x20)));
        }

        public static TimeSpan FromHours(double value)
        {
            return Interval(value, 0x36ee80);
        }

        private static TimeSpan Interval(double value, int scale)
        {
            if (double.IsNaN(value))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_CannotBeNaN"));
            }
            double num = value * scale;
            double num2 = num + ((value >= 0.0) ? 0.5 : -0.5);
            if ((num2 > 922337203685477) || (num2 < -922337203685477))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong"));
            }
            return new TimeSpan(((long) num2) * 0x2710L);
        }

        public static TimeSpan FromMilliseconds(double value)
        {
            return Interval(value, 1);
        }

        public static TimeSpan FromMinutes(double value)
        {
            return Interval(value, 0xea60);
        }

        public TimeSpan Negate()
        {
            if (this.Ticks == MinValue.Ticks)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
            }
            return new TimeSpan(-this._ticks);
        }

        public static TimeSpan FromSeconds(double value)
        {
            return Interval(value, 0x3e8);
        }

        public TimeSpan Subtract(TimeSpan ts)
        {
            long ticks = this._ticks - ts._ticks;
            if (((this._ticks >> 0x3f) != (ts._ticks >> 0x3f)) && ((this._ticks >> 0x3f) != (ticks >> 0x3f)))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong"));
            }
            return new TimeSpan(ticks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static TimeSpan FromTicks(long value)
        {
            return new TimeSpan(value);
        }

        internal static long TimeToTicks(int hour, int minute, int second)
        {
            long num = ((hour * 0xe10L) + (minute * 60L)) + second;
            if ((num > 0xd6bf94d5e5L) || (num < -922337203685L))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("Overflow_TimeSpanTooLong"));
            }
            return (num * 0x989680L);
        }

        public static TimeSpan Parse(string s)
        {
            return TimeSpanParse.Parse(s, null);
        }

        public static TimeSpan Parse(string input, IFormatProvider formatProvider)
        {
            return TimeSpanParse.Parse(input, formatProvider);
        }

        public static TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider)
        {
            return TimeSpanParse.ParseExact(input, format, formatProvider, TimeSpanStyles.None);
        }

        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider)
        {
            return TimeSpanParse.ParseExactMultiple(input, formats, formatProvider, TimeSpanStyles.None);
        }

        public static TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles)
        {
            TimeSpanParse.ValidateStyles(styles, "styles");
            return TimeSpanParse.ParseExact(input, format, formatProvider, styles);
        }

        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles)
        {
            TimeSpanParse.ValidateStyles(styles, "styles");
            return TimeSpanParse.ParseExactMultiple(input, formats, formatProvider, styles);
        }

        public static bool TryParse(string s, out TimeSpan result)
        {
            return TimeSpanParse.TryParse(s, null, out result);
        }

        public static bool TryParse(string input, IFormatProvider formatProvider, out TimeSpan result)
        {
            return TimeSpanParse.TryParse(input, formatProvider, out result);
        }

        public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, out TimeSpan result)
        {
            return TimeSpanParse.TryParseExact(input, format, formatProvider, TimeSpanStyles.None, out result);
        }

        public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, out TimeSpan result)
        {
            return TimeSpanParse.TryParseExactMultiple(input, formats, formatProvider, TimeSpanStyles.None, out result);
        }

        public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result)
        {
            TimeSpanParse.ValidateStyles(styles, "styles");
            return TimeSpanParse.TryParseExact(input, format, formatProvider, styles, out result);
        }

        public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result)
        {
            TimeSpanParse.ValidateStyles(styles, "styles");
            return TimeSpanParse.TryParseExactMultiple(input, formats, formatProvider, styles, out result);
        }

        public override string ToString()
        {
            return TimeSpanFormat.Format(this, null, null);
        }

        public string ToString(string format)
        {
            return TimeSpanFormat.Format(this, format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (LegacyMode)
            {
                return TimeSpanFormat.Format(this, null, null);
            }
            return TimeSpanFormat.Format(this, format, formatProvider);
        }

        public static TimeSpan operator -(TimeSpan t)
        {
            if (t._ticks == MinValue._ticks)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
            }
            return new TimeSpan(-t._ticks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static TimeSpan operator -(TimeSpan t1, TimeSpan t2)
        {
            return t1.Subtract(t2);
        }

        public static TimeSpan operator +(TimeSpan t)
        {
            return t;
        }

        public static TimeSpan operator +(TimeSpan t1, TimeSpan t2)
        {
            return t1.Add(t2);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(TimeSpan t1, TimeSpan t2)
        {
            return (t1._ticks == t2._ticks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(TimeSpan t1, TimeSpan t2)
        {
            return (t1._ticks != t2._ticks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <(TimeSpan t1, TimeSpan t2)
        {
            return (t1._ticks < t2._ticks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <=(TimeSpan t1, TimeSpan t2)
        {
            return (t1._ticks <= t2._ticks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >(TimeSpan t1, TimeSpan t2)
        {
            return (t1._ticks > t2._ticks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >=(TimeSpan t1, TimeSpan t2)
        {
            return (t1._ticks >= t2._ticks);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool LegacyFormatMode();
        [SecuritySafeCritical]
        private static bool GetLegacyFormatMode()
        {
            if (LegacyFormatMode())
            {
                return true;
            }
            bool? nullable = AppDomain.CurrentDomain.IsCompatibilitySwitchSet("NetFx40_TimeSpanLegacyFormatMode");
            if (nullable.HasValue)
            {
                return nullable.Value;
            }
            return false;
        }

        private static bool LegacyMode
        {
            [SecuritySafeCritical]
            get
            {
                if (!_legacyConfigChecked)
                {
                    _legacyMode = GetLegacyFormatMode();
                    _legacyConfigChecked = true;
                }
                return _legacyMode;
            }
        }
        static TimeSpan()
        {
            Zero = new TimeSpan(0L);
            MaxValue = new TimeSpan(0x7fffffffffffffffL);
            MinValue = new TimeSpan(-9223372036854775808L);
        }
    }
}

