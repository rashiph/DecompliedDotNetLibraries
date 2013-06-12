namespace System
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public struct DateTime : IComparable, IFormattable, IConvertible, ISerializable, IComparable<DateTime>, IEquatable<DateTime>
    {
        private ulong dateData;
        private const string DateDataField = "dateData";
        private const int DatePartDay = 3;
        private const int DatePartDayOfYear = 1;
        private const int DatePartMonth = 2;
        private const int DatePartYear = 0;
        private const int DaysPer100Years = 0x8eac;
        private const int DaysPer400Years = 0x23ab1;
        private const int DaysPer4Years = 0x5b5;
        private const int DaysPerYear = 0x16d;
        private const int DaysTo10000 = 0x37b9db;
        private const int DaysTo1601 = 0x8eac4;
        private const int DaysTo1899 = 0xa9559;
        private static readonly int[] DaysToMonth365;
        private static readonly int[] DaysToMonth366;
        private const long DoubleDateOffset = 0x85103c0cb83c000L;
        private const long FileTimeOffset = 0x701ce1722770000L;
        private const ulong FlagsMask = 13835058055282163712L;
        private const ulong KindLocal = 9223372036854775808L;
        private const ulong KindLocalAmbiguousDst = 13835058055282163712L;
        private const int KindShift = 0x3e;
        private const ulong KindUnspecified = 0L;
        private const ulong KindUtc = 0x4000000000000000L;
        private const ulong LocalMask = 9223372036854775808L;
        private const long MaxMillis = 0x11efae44cb400L;
        internal const long MaxTicks = 0x2bca2875f4373fffL;
        public static readonly DateTime MaxValue;
        private const int MillisPerDay = 0x5265c00;
        private const int MillisPerHour = 0x36ee80;
        private const int MillisPerMinute = 0xea60;
        private const int MillisPerSecond = 0x3e8;
        internal const long MinTicks = 0L;
        public static readonly DateTime MinValue;
        private const double OADateMaxAsDouble = 2958466.0;
        private const double OADateMinAsDouble = -657435.0;
        private const long OADateMinAsTicks = 0x6efdddaec64000L;
        private const long TicksCeiling = 0x4000000000000000L;
        private const string TicksField = "ticks";
        private const ulong TicksMask = 0x3fffffffffffffffL;
        private const long TicksPerDay = 0xc92a69c000L;
        private const long TicksPerHour = 0x861c46800L;
        private const long TicksPerMillisecond = 0x2710L;
        private const long TicksPerMinute = 0x23c34600L;
        private const long TicksPerSecond = 0x989680L;

        static DateTime()
        {
            DaysToMonth365 = new int[] { 0, 0x1f, 0x3b, 90, 120, 0x97, 0xb5, 0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d };
            DaysToMonth366 = new int[] { 0, 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6, 0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e };
            MinValue = new DateTime(0L, DateTimeKind.Unspecified);
            MaxValue = new DateTime(0x2bca2875f4373fffL, DateTimeKind.Unspecified);
        }

        public DateTime(long ticks)
        {
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                throw new ArgumentOutOfRangeException("ticks", Environment.GetResourceString("ArgumentOutOfRange_DateTimeBadTicks"));
            }
            this.dateData = (ulong) ticks;
        }

        private DateTime(ulong dateData)
        {
            this.dateData = dateData;
        }

        public DateTime(long ticks, DateTimeKind kind)
        {
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                throw new ArgumentOutOfRangeException("ticks", Environment.GetResourceString("ArgumentOutOfRange_DateTimeBadTicks"));
            }
            if ((kind < DateTimeKind.Unspecified) || (kind > DateTimeKind.Local))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDateTimeKind"), "kind");
            }
            this.dateData = (ulong) (ticks | (((long) kind) << 0x3e));
        }

        private DateTime(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            bool flag = false;
            bool flag2 = false;
            long num = 0L;
            ulong num2 = 0L;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Name;
                if (name != null)
                {
                    if (!(name == "ticks"))
                    {
                        if (name == "dateData")
                        {
                            goto Label_0062;
                        }
                    }
                    else
                    {
                        num = Convert.ToInt64(enumerator.Value, CultureInfo.InvariantCulture);
                        flag = true;
                    }
                }
                continue;
            Label_0062:
                num2 = Convert.ToUInt64(enumerator.Value, CultureInfo.InvariantCulture);
                flag2 = true;
            }
            if (flag2)
            {
                this.dateData = num2;
            }
            else
            {
                if (!flag)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_MissingDateTimeData"));
                }
                this.dateData = (ulong) num;
            }
            long internalTicks = this.InternalTicks;
            if ((internalTicks < 0L) || (internalTicks > 0x2bca2875f4373fffL))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_DateTimeTicksOutOfRange"));
            }
        }

        public DateTime(int year, int month, int day)
        {
            this.dateData = (ulong) DateToTicks(year, month, day);
        }

        internal DateTime(long ticks, DateTimeKind kind, bool isAmbiguousDst)
        {
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                throw new ArgumentOutOfRangeException("ticks", Environment.GetResourceString("ArgumentOutOfRange_DateTimeBadTicks"));
            }
            this.dateData = (ulong) (ticks | (isAmbiguousDst ? -4611686018427387904L : -9223372036854775808L));
        }

        public DateTime(int year, int month, int day, Calendar calendar) : this(year, month, day, 0, 0, 0, calendar)
        {
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second)
        {
            this.dateData = (ulong) (DateToTicks(year, month, day) + TimeToTicks(hour, minute, second));
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            if ((kind < DateTimeKind.Unspecified) || (kind > DateTimeKind.Local))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDateTimeKind"), "kind");
            }
            long num = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
            this.dateData = (ulong) (num | (((long) kind) << 0x3e));
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second, Calendar calendar)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException("calendar");
            }
            this.dateData = (ulong) calendar.ToDateTime(year, month, day, hour, minute, second, 0).Ticks;
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0, 0x3e7 }));
            }
            long num = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
            num += millisecond * 0x2710L;
            if ((num < 0L) || (num > 0x2bca2875f4373fffL))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DateTimeRange"));
            }
            this.dateData = (ulong) num;
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
        {
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0, 0x3e7 }));
            }
            if ((kind < DateTimeKind.Unspecified) || (kind > DateTimeKind.Local))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDateTimeKind"), "kind");
            }
            long num = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
            num += millisecond * 0x2710L;
            if ((num < 0L) || (num > 0x2bca2875f4373fffL))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DateTimeRange"));
            }
            this.dateData = (ulong) (num | (((long) kind) << 0x3e));
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException("calendar");
            }
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0, 0x3e7 }));
            }
            long num = calendar.ToDateTime(year, month, day, hour, minute, second, 0).Ticks + (millisecond * 0x2710L);
            if ((num < 0L) || (num > 0x2bca2875f4373fffL))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DateTimeRange"));
            }
            this.dateData = (ulong) num;
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, DateTimeKind kind)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException("calendar");
            }
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                throw new ArgumentOutOfRangeException("millisecond", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0, 0x3e7 }));
            }
            if ((kind < DateTimeKind.Unspecified) || (kind > DateTimeKind.Local))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDateTimeKind"), "kind");
            }
            long num = calendar.ToDateTime(year, month, day, hour, minute, second, 0).Ticks + (millisecond * 0x2710L);
            if ((num < 0L) || (num > 0x2bca2875f4373fffL))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DateTimeRange"));
            }
            this.dateData = (ulong) (num | (((long) kind) << 0x3e));
        }

        public DateTime Add(TimeSpan value)
        {
            return this.AddTicks(value._ticks);
        }

        private DateTime Add(double value, int scale)
        {
            long num = (long) ((value * scale) + ((value >= 0.0) ? 0.5 : -0.5));
            if ((num <= -315537897600000L) || (num >= 0x11efae44cb400L))
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_AddValue"));
            }
            return this.AddTicks(num * 0x2710L);
        }

        public DateTime AddDays(double value)
        {
            return this.Add(value, 0x5265c00);
        }

        public DateTime AddHours(double value)
        {
            return this.Add(value, 0x36ee80);
        }

        public DateTime AddMilliseconds(double value)
        {
            return this.Add(value, 1);
        }

        public DateTime AddMinutes(double value)
        {
            return this.Add(value, 0xea60);
        }

        public DateTime AddMonths(int months)
        {
            if ((months < -120000) || (months > 0x1d4c0))
            {
                throw new ArgumentOutOfRangeException("months", Environment.GetResourceString("ArgumentOutOfRange_DateTimeBadMonths"));
            }
            int datePart = this.GetDatePart(0);
            int month = this.GetDatePart(2);
            int day = this.GetDatePart(3);
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
            if ((datePart < 1) || (datePart > 0x270f))
            {
                throw new ArgumentOutOfRangeException("months", Environment.GetResourceString("ArgumentOutOfRange_DateArithmetic"));
            }
            int num5 = DaysInMonth(datePart, month);
            if (day > num5)
            {
                day = num5;
            }
            return new DateTime(((ulong) (DateToTicks(datePart, month, day) + (this.InternalTicks % 0xc92a69c000L))) | this.InternalKind);
        }

        public DateTime AddSeconds(double value)
        {
            return this.Add(value, 0x3e8);
        }

        public DateTime AddTicks(long value)
        {
            long internalTicks = this.InternalTicks;
            if ((value > (0x2bca2875f4373fffL - internalTicks)) || (value < -internalTicks))
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_DateArithmetic"));
            }
            return new DateTime(((ulong) (internalTicks + value)) | this.InternalKind);
        }

        public DateTime AddYears(int value)
        {
            if ((value < -10000) || (value > 0x2710))
            {
                throw new ArgumentOutOfRangeException("years", Environment.GetResourceString("ArgumentOutOfRange_DateTimeBadYears"));
            }
            return this.AddMonths(value * 12);
        }

        public static int Compare(DateTime t1, DateTime t2)
        {
            long internalTicks = t1.InternalTicks;
            long num2 = t2.InternalTicks;
            if (internalTicks > num2)
            {
                return 1;
            }
            if (internalTicks < num2)
            {
                return -1;
            }
            return 0;
        }

        public int CompareTo(DateTime value)
        {
            long internalTicks = value.InternalTicks;
            long num2 = this.InternalTicks;
            if (num2 > internalTicks)
            {
                return 1;
            }
            if (num2 < internalTicks)
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
            if (!(value is DateTime))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDateTime"));
            }
            DateTime time = (DateTime) value;
            long internalTicks = time.InternalTicks;
            long num2 = this.InternalTicks;
            if (num2 > internalTicks)
            {
                return 1;
            }
            if (num2 < internalTicks)
            {
                return -1;
            }
            return 0;
        }

        private static long DateToTicks(int year, int month, int day)
        {
            if (((year >= 1) && (year <= 0x270f)) && ((month >= 1) && (month <= 12)))
            {
                int[] numArray = IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
                if ((day >= 1) && (day <= (numArray[month] - numArray[month - 1])))
                {
                    int num = year - 1;
                    int num2 = ((((((num * 0x16d) + (num / 4)) - (num / 100)) + (num / 400)) + numArray[month - 1]) + day) - 1;
                    return (num2 * 0xc92a69c000L);
                }
            }
            throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadYearMonthDay"));
        }

        public static int DaysInMonth(int year, int month)
        {
            if ((month < 1) || (month > 12))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Month"));
            }
            int[] numArray = IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
            return (numArray[month] - numArray[month - 1]);
        }

        internal static long DoubleDateToTicks(double value)
        {
            if ((value >= 2958466.0) || (value <= -657435.0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_OleAutDateInvalid"));
            }
            long num = (long) ((value * 86400000.0) + ((value >= 0.0) ? 0.5 : -0.5));
            if (num < 0L)
            {
                num -= (num % 0x5265c00L) * 2L;
            }
            num += 0x3680b5e1fc00L;
            if ((num < 0L) || (num >= 0x11efae44cb400L))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_OleAutDateScale"));
            }
            return (num * 0x2710L);
        }

        public bool Equals(DateTime value)
        {
            return (this.InternalTicks == value.InternalTicks);
        }

        public override bool Equals(object value)
        {
            if (value is DateTime)
            {
                DateTime time = (DateTime) value;
                return (this.InternalTicks == time.InternalTicks);
            }
            return false;
        }

        public static bool Equals(DateTime t1, DateTime t2)
        {
            return (t1.InternalTicks == t2.InternalTicks);
        }

        public static DateTime FromBinary(long dateData)
        {
            long num2;
            if ((dateData & -9223372036854775808L) == 0L)
            {
                return FromBinaryRaw(dateData);
            }
            long ticks = dateData & 0x3fffffffffffffffL;
            if (ticks > 0x3fffff36d5964000L)
            {
                ticks -= 0x4000000000000000L;
            }
            bool isAmbiguousLocalDst = false;
            if (ticks < 0L)
            {
                num2 = TimeZoneInfo.Local.GetUtcOffset(MinValue, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
            }
            else if (ticks > 0x2bca2875f4373fffL)
            {
                num2 = TimeZoneInfo.Local.GetUtcOffset(MaxValue, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
            }
            else
            {
                DateTime time = new DateTime(ticks, DateTimeKind.Utc);
                bool isDaylightSavings = false;
                num2 = TimeZoneInfo.GetUtcOffsetFromUtc(time, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst).Ticks;
            }
            ticks += num2;
            if (ticks < 0L)
            {
                ticks += 0xc92a69c000L;
            }
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeBadBinaryData"), "dateData");
            }
            return new DateTime(ticks, DateTimeKind.Local, isAmbiguousLocalDst);
        }

        internal static DateTime FromBinaryRaw(long dateData)
        {
            long num = dateData & 0x3fffffffffffffffL;
            if ((num < 0L) || (num > 0x2bca2875f4373fffL))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_DateTimeBadBinaryData"), "dateData");
            }
            return new DateTime((ulong) dateData);
        }

        public static DateTime FromFileTime(long fileTime)
        {
            return FromFileTimeUtc(fileTime).ToLocalTime();
        }

        public static DateTime FromFileTimeUtc(long fileTime)
        {
            if ((fileTime < 0L) || (fileTime > 0x24c85a5ed1c03fffL))
            {
                throw new ArgumentOutOfRangeException("fileTime", Environment.GetResourceString("ArgumentOutOfRange_FileTimeInvalid"));
            }
            return new DateTime(fileTime + 0x701ce1722770000L, DateTimeKind.Utc);
        }

        public static DateTime FromOADate(double d)
        {
            return new DateTime(DoubleDateToTicks(d), DateTimeKind.Unspecified);
        }

        private int GetDatePart(int part)
        {
            int num2 = (int) (this.InternalTicks / 0xc92a69c000L);
            int num3 = num2 / 0x23ab1;
            num2 -= num3 * 0x23ab1;
            int num4 = num2 / 0x8eac;
            if (num4 == 4)
            {
                num4 = 3;
            }
            num2 -= num4 * 0x8eac;
            int num5 = num2 / 0x5b5;
            num2 -= num5 * 0x5b5;
            int num6 = num2 / 0x16d;
            if (num6 == 4)
            {
                num6 = 3;
            }
            if (part == 0)
            {
                return (((((num3 * 400) + (num4 * 100)) + (num5 * 4)) + num6) + 1);
            }
            num2 -= num6 * 0x16d;
            if (part == 1)
            {
                return (num2 + 1);
            }
            int[] numArray = ((num6 == 3) && ((num5 != 0x18) || (num4 == 3))) ? DaysToMonth366 : DaysToMonth365;
            int index = num2 >> 6;
            while (num2 >= numArray[index])
            {
                index++;
            }
            if (part == 2)
            {
                return index;
            }
            return ((num2 - numArray[index - 1]) + 1);
        }

        public string[] GetDateTimeFormats()
        {
            return this.GetDateTimeFormats(CultureInfo.CurrentCulture);
        }

        public string[] GetDateTimeFormats(char format)
        {
            return this.GetDateTimeFormats(format, CultureInfo.CurrentCulture);
        }

        public string[] GetDateTimeFormats(IFormatProvider provider)
        {
            return DateTimeFormat.GetAllDateTimes(this, DateTimeFormatInfo.GetInstance(provider));
        }

        public string[] GetDateTimeFormats(char format, IFormatProvider provider)
        {
            return DateTimeFormat.GetAllDateTimes(this, format, DateTimeFormatInfo.GetInstance(provider));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override int GetHashCode()
        {
            long internalTicks = this.InternalTicks;
            return (((int) internalTicks) ^ ((int) (internalTicks >> 0x20)));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern long GetSystemTimeAsFileTime();
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public TypeCode GetTypeCode()
        {
            return TypeCode.DateTime;
        }

        internal bool IsAmbiguousDaylightSavingTime()
        {
            return (this.InternalKind == 13835058055282163712L);
        }

        public bool IsDaylightSavingTime()
        {
            if (this.Kind == DateTimeKind.Utc)
            {
                return false;
            }
            return TimeZoneInfo.Local.IsDaylightSavingTime(this, TimeZoneInfoOptions.NoThrowOnInvalidTime);
        }

        public static bool IsLeapYear(int year)
        {
            if ((year < 1) || (year > 0x270f))
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_Year"));
            }
            if ((year % 4) != 0)
            {
                return false;
            }
            if ((year % 100) == 0)
            {
                return ((year % 400) == 0);
            }
            return true;
        }

        public static DateTime operator +(DateTime d, TimeSpan t)
        {
            long internalTicks = d.InternalTicks;
            long num2 = t._ticks;
            if ((num2 > (0x2bca2875f4373fffL - internalTicks)) || (num2 < -internalTicks))
            {
                throw new ArgumentOutOfRangeException("t", Environment.GetResourceString("ArgumentOutOfRange_DateArithmetic"));
            }
            return new DateTime(((ulong) (internalTicks + num2)) | d.InternalKind);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(DateTime d1, DateTime d2)
        {
            return (d1.InternalTicks == d2.InternalTicks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >(DateTime t1, DateTime t2)
        {
            return (t1.InternalTicks > t2.InternalTicks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >=(DateTime t1, DateTime t2)
        {
            return (t1.InternalTicks >= t2.InternalTicks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(DateTime d1, DateTime d2)
        {
            return (d1.InternalTicks != d2.InternalTicks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <(DateTime t1, DateTime t2)
        {
            return (t1.InternalTicks < t2.InternalTicks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <=(DateTime t1, DateTime t2)
        {
            return (t1.InternalTicks <= t2.InternalTicks);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static TimeSpan operator -(DateTime d1, DateTime d2)
        {
            return new TimeSpan(d1.InternalTicks - d2.InternalTicks);
        }

        public static DateTime operator -(DateTime d, TimeSpan t)
        {
            long internalTicks = d.InternalTicks;
            long num2 = t._ticks;
            if ((internalTicks < num2) || ((internalTicks - 0x2bca2875f4373fffL) > num2))
            {
                throw new ArgumentOutOfRangeException("t", Environment.GetResourceString("ArgumentOutOfRange_DateArithmetic"));
            }
            return new DateTime(((ulong) (internalTicks - num2)) | d.InternalKind);
        }

        [SecuritySafeCritical]
        public static DateTime Parse(string s)
        {
            return DateTimeParse.Parse(s, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None);
        }

        [SecuritySafeCritical]
        public static DateTime Parse(string s, IFormatProvider provider)
        {
            return DateTimeParse.Parse(s, DateTimeFormatInfo.GetInstance(provider), DateTimeStyles.None);
        }

        [SecuritySafeCritical]
        public static DateTime Parse(string s, IFormatProvider provider, DateTimeStyles styles)
        {
            DateTimeFormatInfo.ValidateStyles(styles, "styles");
            return DateTimeParse.Parse(s, DateTimeFormatInfo.GetInstance(provider), styles);
        }

        public static DateTime ParseExact(string s, string format, IFormatProvider provider)
        {
            return DateTimeParse.ParseExact(s, format, DateTimeFormatInfo.GetInstance(provider), DateTimeStyles.None);
        }

        public static DateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style)
        {
            DateTimeFormatInfo.ValidateStyles(style, "style");
            return DateTimeParse.ParseExact(s, format, DateTimeFormatInfo.GetInstance(provider), style);
        }

        public static DateTime ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style)
        {
            DateTimeFormatInfo.ValidateStyles(style, "style");
            return DateTimeParse.ParseExactMultiple(s, formats, DateTimeFormatInfo.GetInstance(provider), style);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static DateTime SpecifyKind(DateTime value, DateTimeKind kind)
        {
            return new DateTime(value.InternalTicks, kind);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public TimeSpan Subtract(DateTime value)
        {
            return new TimeSpan(this.InternalTicks - value.InternalTicks);
        }

        public DateTime Subtract(TimeSpan value)
        {
            long internalTicks = this.InternalTicks;
            long num2 = value._ticks;
            if ((internalTicks < num2) || ((internalTicks - 0x2bca2875f4373fffL) > num2))
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_DateArithmetic"));
            }
            return new DateTime(((ulong) (internalTicks - num2)) | this.InternalKind);
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Boolean" }));
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Byte" }));
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Char" }));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return this;
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Decimal" }));
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Double" }));
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Int16" }));
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Int32" }));
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Int64" }));
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "SByte" }));
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "Single" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "UInt16" }));
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "UInt32" }));
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "DateTime", "UInt64" }));
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("ticks", this.InternalTicks);
            info.AddValue("dateData", this.dateData);
        }

        private static double TicksToOADate(long value)
        {
            if (value == 0L)
            {
                return 0.0;
            }
            if (value < 0xc92a69c000L)
            {
                value += 0x85103c0cb83c000L;
            }
            if (value < 0x6efdddaec64000L)
            {
                throw new OverflowException(Environment.GetResourceString("Arg_OleAutDateInvalid"));
            }
            long num = (value - 0x85103c0cb83c000L) / 0x2710L;
            if (num < 0L)
            {
                long num2 = num % 0x5265c00L;
                if (num2 != 0L)
                {
                    num -= (0x5265c00L + num2) * 2L;
                }
            }
            return (((double) num) / 86400000.0);
        }

        private static long TimeToTicks(int hour, int minute, int second)
        {
            if ((((hour < 0) || (hour >= 0x18)) || ((minute < 0) || (minute >= 60))) || ((second < 0) || (second >= 60)))
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_BadHourMinuteSecond"));
            }
            return TimeSpan.TimeToTicks(hour, minute, second);
        }

        public long ToBinary()
        {
            if (this.Kind != DateTimeKind.Local)
            {
                return (long) this.dateData;
            }
            TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(this, TimeZoneInfoOptions.NoThrowOnInvalidTime);
            long num2 = this.Ticks - utcOffset.Ticks;
            if (num2 < 0L)
            {
                num2 = 0x4000000000000000L + num2;
            }
            return (num2 | -9223372036854775808L);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal long ToBinaryRaw()
        {
            return (long) this.dateData;
        }

        public long ToFileTime()
        {
            return this.ToUniversalTime().ToFileTimeUtc();
        }

        public long ToFileTimeUtc()
        {
            long num = ((this.InternalKind & 9223372036854775808L) != 0L) ? this.ToUniversalTime().InternalTicks : this.InternalTicks;
            num -= 0x701ce1722770000L;
            if (num < 0L)
            {
                throw new ArgumentOutOfRangeException(null, Environment.GetResourceString("ArgumentOutOfRange_FileTimeInvalid"));
            }
            return num;
        }

        public DateTime ToLocalTime()
        {
            if (this.Kind == DateTimeKind.Local)
            {
                return this;
            }
            bool isDaylightSavings = false;
            bool isAmbiguousLocalDst = false;
            long ticks = TimeZoneInfo.GetUtcOffsetFromUtc(this, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst).Ticks;
            long num2 = this.Ticks + ticks;
            if (num2 > 0x2bca2875f4373fffL)
            {
                return new DateTime(0x2bca2875f4373fffL, DateTimeKind.Local);
            }
            if (num2 < 0L)
            {
                return new DateTime(0L, DateTimeKind.Local);
            }
            return new DateTime(num2, DateTimeKind.Local, isAmbiguousLocalDst);
        }

        [SecuritySafeCritical]
        public string ToLongDateString()
        {
            return DateTimeFormat.Format(this, "D", DateTimeFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToLongTimeString()
        {
            return DateTimeFormat.Format(this, "T", DateTimeFormatInfo.CurrentInfo);
        }

        public double ToOADate()
        {
            return TicksToOADate(this.InternalTicks);
        }

        [SecuritySafeCritical]
        public string ToShortDateString()
        {
            return DateTimeFormat.Format(this, "d", DateTimeFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToShortTimeString()
        {
            return DateTimeFormat.Format(this, "t", DateTimeFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return DateTimeFormat.Format(this, null, DateTimeFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string ToString(IFormatProvider provider)
        {
            return DateTimeFormat.Format(this, null, DateTimeFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return DateTimeFormat.Format(this, format, DateTimeFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return DateTimeFormat.Format(this, format, DateTimeFormatInfo.GetInstance(provider));
        }

        public DateTime ToUniversalTime()
        {
            return TimeZoneInfo.ConvertTimeToUtc(this, TimeZoneInfoOptions.NoThrowOnInvalidTime);
        }

        internal static bool TryCreate(int year, int month, int day, int hour, int minute, int second, int millisecond, out DateTime result)
        {
            result = MinValue;
            if (((year < 1) || (year > 0x270f)) || ((month < 1) || (month > 12)))
            {
                return false;
            }
            int[] numArray = IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
            if ((day < 1) || (day > (numArray[month] - numArray[month - 1])))
            {
                return false;
            }
            if ((((hour < 0) || (hour >= 0x18)) || ((minute < 0) || (minute >= 60))) || ((second < 0) || (second >= 60)))
            {
                return false;
            }
            if ((millisecond < 0) || (millisecond >= 0x3e8))
            {
                return false;
            }
            long ticks = DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
            ticks += millisecond * 0x2710L;
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                return false;
            }
            result = new DateTime(ticks, DateTimeKind.Unspecified);
            return true;
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, out DateTime result)
        {
            return DateTimeParse.TryParse(s, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out result);
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
        {
            DateTimeFormatInfo.ValidateStyles(styles, "styles");
            return DateTimeParse.TryParse(s, DateTimeFormatInfo.GetInstance(provider), styles, out result);
        }

        public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out DateTime result)
        {
            DateTimeFormatInfo.ValidateStyles(style, "style");
            return DateTimeParse.TryParseExact(s, format, DateTimeFormatInfo.GetInstance(provider), style, out result);
        }

        public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out DateTime result)
        {
            DateTimeFormatInfo.ValidateStyles(style, "style");
            return DateTimeParse.TryParseExactMultiple(s, formats, DateTimeFormatInfo.GetInstance(provider), style, out result);
        }

        public DateTime Date
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                long internalTicks = this.InternalTicks;
                return new DateTime(((ulong) (internalTicks - (internalTicks % 0xc92a69c000L))) | this.InternalKind);
            }
        }

        public int Day
        {
            get
            {
                return this.GetDatePart(3);
            }
        }

        public System.DayOfWeek DayOfWeek
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (System.DayOfWeek) ((int) (((this.InternalTicks / 0xc92a69c000L) + 1L) % 7L));
            }
        }

        public int DayOfYear
        {
            get
            {
                return this.GetDatePart(1);
            }
        }

        public int Hour
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (int) ((this.InternalTicks / 0x861c46800L) % 0x18L);
            }
        }

        private ulong InternalKind
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.dateData & 13835058055282163712L);
            }
        }

        internal long InternalTicks
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (((long) this.dateData) & 0x3fffffffffffffffL);
            }
        }

        public DateTimeKind Kind
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                switch (this.InternalKind)
                {
                    case 0L:
                        return DateTimeKind.Unspecified;

                    case 0x4000000000000000L:
                        return DateTimeKind.Utc;
                }
                return DateTimeKind.Local;
            }
        }

        public int Millisecond
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (int) ((this.InternalTicks / 0x2710L) % 0x3e8L);
            }
        }

        public int Minute
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (int) ((this.InternalTicks / 0x23c34600L) % 60L);
            }
        }

        public int Month
        {
            get
            {
                return this.GetDatePart(2);
            }
        }

        public static DateTime Now
        {
            get
            {
                DateTime utcNow = UtcNow;
                bool isAmbiguousLocalDst = false;
                long ticks = TimeZoneInfo.GetDateTimeNowUtcOffsetFromUtc(utcNow, out isAmbiguousLocalDst).Ticks;
                long num2 = utcNow.Ticks + ticks;
                if (num2 > 0x2bca2875f4373fffL)
                {
                    return new DateTime(0x2bca2875f4373fffL, DateTimeKind.Local);
                }
                if (num2 < 0L)
                {
                    return new DateTime(0L, DateTimeKind.Local);
                }
                return new DateTime(num2, DateTimeKind.Local, isAmbiguousLocalDst);
            }
        }

        public int Second
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (int) ((this.InternalTicks / 0x989680L) % 60L);
            }
        }

        public long Ticks
        {
            get
            {
                return this.InternalTicks;
            }
        }

        public TimeSpan TimeOfDay
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new TimeSpan(this.InternalTicks % 0xc92a69c000L);
            }
        }

        public static DateTime Today
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return Now.Date;
            }
        }

        public static DateTime UtcNow
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return new DateTime((ulong) ((GetSystemTimeAsFileTime() + 0x701ce1722770000L) | 0x4000000000000000L));
            }
        }

        public int Year
        {
            get
            {
                return this.GetDatePart(0);
            }
        }
    }
}

