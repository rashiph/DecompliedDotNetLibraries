namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class BinXmlDateTime
    {
        internal static int[] KatmaiTimeScaleMultiplicator = new int[] { 0x989680, 0xf4240, 0x186a0, 0x2710, 0x3e8, 100, 10, 1 };
        private const int MaxFractionDigits = 7;
        private static readonly int SQLTicksPerDay = (SQLTicksPerHour * 0x18);
        public static readonly int SQLTicksPerHour = (SQLTicksPerMinute * 60);
        private static readonly double SQLTicksPerMillisecond = 0.3;
        public static readonly int SQLTicksPerMinute = (SQLTicksPerSecond * 60);
        public static readonly int SQLTicksPerSecond = 300;

        protected BinXmlDateTime()
        {
        }

        private static void BreakDownXsdDate(long val, out int yr, out int mnth, out int day, out bool negTimeZone, out int hr, out int min)
        {
            if (val >= 0L)
            {
                val /= 4L;
                int num = ((int) (val % 0x6ccL)) - 840;
                long num2 = val / 0x6ccL;
                if (negTimeZone = num < 0)
                {
                    num = -num;
                }
                min = num % 60;
                hr = num / 60;
                day = ((int) (num2 % 0x1fL)) + 1;
                num2 /= 0x1fL;
                mnth = ((int) (num2 % 12L)) + 1;
                yr = ((int) (num2 / 12L)) - 0x270f;
                if ((yr >= -9999) && (yr <= 0x270f))
                {
                    return;
                }
            }
            throw new XmlException("SqlTypes_ArithOverflow", null);
        }

        private static void BreakDownXsdDateTime(long val, out int yr, out int mnth, out int day, out int hr, out int min, out int sec, out int ms)
        {
            if (val >= 0L)
            {
                long num = val / 4L;
                ms = (int) (num % 0x3e8L);
                num /= 0x3e8L;
                sec = (int) (num % 60L);
                num /= 60L;
                min = (int) (num % 60L);
                num /= 60L;
                hr = (int) (num % 0x18L);
                num /= 0x18L;
                day = ((int) (num % 0x1fL)) + 1;
                num /= 0x1fL;
                mnth = ((int) (num % 12L)) + 1;
                num /= 12L;
                yr = (int) (num - 0x270fL);
                if ((yr >= -9999) && (yr <= 0x270f))
                {
                    return;
                }
            }
            throw new XmlException("SqlTypes_ArithOverflow", null);
        }

        private static void BreakDownXsdTime(long val, out int hr, out int min, out int sec, out int ms)
        {
            if (val >= 0L)
            {
                val /= 4L;
                ms = (int) (val % 0x3e8L);
                val /= 0x3e8L;
                sec = (int) (val % 60L);
                val /= 60L;
                min = (int) (val % 60L);
                hr = (int) (val / 60L);
                if ((0 <= hr) && (hr <= 0x17))
                {
                    return;
                }
            }
            throw new XmlException("SqlTypes_ArithOverflow", null);
        }

        private static int GetFractions(DateTime dt)
        {
            DateTime time = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            return (int) (dt.Ticks - time.Ticks);
        }

        private static int GetFractions(DateTimeOffset dt)
        {
            DateTime time = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            return (int) (dt.Ticks - time.Ticks);
        }

        private static long GetKatmaiDateTicks(byte[] data, ref int pos)
        {
            int index = pos;
            pos = index + 3;
            return (((data[index] | (data[index + 1] << 8)) | (data[index + 2] << 0x10)) * 0xc92a69c000L);
        }

        private static long GetKatmaiTimeTicks(byte[] data, ref int pos)
        {
            long num3;
            int index = pos;
            byte num2 = data[index];
            index++;
            if (num2 <= 2)
            {
                num3 = (data[index] | (data[index + 1] << 8)) | (data[index + 2] << 0x10);
                pos = index + 3;
            }
            else if (num2 <= 4)
            {
                num3 = (data[index] | (data[index + 1] << 8)) | (data[index + 2] << 0x10);
                num3 |= data[index + 3] << 0x18;
                pos = index + 4;
            }
            else
            {
                if (num2 > 7)
                {
                    throw new XmlException("SqlTypes_ArithOverflow", null);
                }
                num3 = (data[index] | (data[index + 1] << 8)) | (data[index + 2] << 0x10);
                num3 |= (data[index + 3] << 0x18) | (data[index + 4] << 0x20);
                pos = index + 5;
            }
            return (num3 * KatmaiTimeScaleMultiplicator[num2]);
        }

        private static long GetKatmaiTimeZoneTicks(byte[] data, int pos)
        {
            return (((short) (data[pos] | (data[pos + 1] << 8))) * 0x23c34600L);
        }

        public static DateTime SqlDateTimeToDateTime(int dateticks, uint timeticks)
        {
            DateTime time = new DateTime(0x76c, 1, 1);
            long num = (long) ((((double) timeticks) / SQLTicksPerMillisecond) + 0.5);
            return time.Add(new TimeSpan((dateticks * 0xc92a69c000L) + (num * 0x2710L)));
        }

        public static string SqlDateTimeToString(int dateticks, uint timeticks)
        {
            DateTime time = SqlDateTimeToDateTime(dateticks, timeticks);
            string format = (time.Millisecond != 0) ? @"yyyy/MM/dd\THH:mm:ss.ffff" : @"yyyy/MM/dd\THH:mm:ss";
            return time.ToString(format, CultureInfo.InvariantCulture);
        }

        public static DateTime SqlSmallDateTimeToDateTime(short dateticks, ushort timeticks)
        {
            return SqlDateTimeToDateTime(dateticks, (uint) (timeticks * SQLTicksPerMinute));
        }

        public static string SqlSmallDateTimeToString(short dateticks, ushort timeticks)
        {
            return SqlSmallDateTimeToDateTime(dateticks, timeticks).ToString(@"yyyy/MM/dd\THH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static void Write2Dig(StringBuilder sb, int val)
        {
            sb.Append((char) (0x30 + (val / 10)));
            sb.Append((char) (0x30 + (val % 10)));
        }

        private static void Write3Dec(StringBuilder sb, int val)
        {
            int num = val % 10;
            val /= 10;
            int num2 = val % 10;
            val /= 10;
            int num3 = val;
            sb.Append('.');
            sb.Append((char) (0x30 + num3));
            sb.Append((char) (0x30 + num2));
            sb.Append((char) (0x30 + num));
        }

        private static void Write4DigNeg(StringBuilder sb, int val)
        {
            if (val < 0)
            {
                val = -val;
                sb.Append('-');
            }
            Write2Dig(sb, val / 100);
            Write2Dig(sb, val % 100);
        }

        private static void WriteDate(StringBuilder sb, int yr, int mnth, int day)
        {
            Write4DigNeg(sb, yr);
            sb.Append('-');
            Write2Dig(sb, mnth);
            sb.Append('-');
            Write2Dig(sb, day);
        }

        private static void WriteTime(StringBuilder sb, int hr, int min, int sec, int ms)
        {
            Write2Dig(sb, hr);
            sb.Append(':');
            Write2Dig(sb, min);
            sb.Append(':');
            Write2Dig(sb, sec);
            if (ms != 0)
            {
                Write3Dec(sb, ms);
            }
        }

        private static void WriteTimeFullPrecision(StringBuilder sb, int hr, int min, int sec, int fraction)
        {
            Write2Dig(sb, hr);
            sb.Append(':');
            Write2Dig(sb, min);
            sb.Append(':');
            Write2Dig(sb, sec);
            if (fraction != 0)
            {
                int index = 7;
                while ((fraction % 10) == 0)
                {
                    index--;
                    fraction /= 10;
                }
                char[] chArray = new char[index];
                while (index > 0)
                {
                    index--;
                    chArray[index] = (char) ((fraction % 10) + 0x30);
                    fraction /= 10;
                }
                sb.Append('.');
                sb.Append(chArray);
            }
        }

        private static void WriteTimeZone(StringBuilder sb, TimeSpan zone)
        {
            bool negTimeZone = true;
            if (zone.Ticks < 0L)
            {
                negTimeZone = false;
                zone = zone.Negate();
            }
            WriteTimeZone(sb, negTimeZone, zone.Hours, zone.Minutes);
        }

        private static void WriteTimeZone(StringBuilder sb, bool negTimeZone, int hr, int min)
        {
            if ((hr == 0) && (min == 0))
            {
                sb.Append('Z');
            }
            else
            {
                sb.Append(negTimeZone ? '+' : '-');
                Write2Dig(sb, hr);
                sb.Append(':');
                Write2Dig(sb, min);
            }
        }

        public static DateTime XsdDateTimeToDateTime(long val)
        {
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            BreakDownXsdDateTime(val, out num, out num2, out num3, out num4, out num5, out num6, out num7);
            return new DateTime(num, num2, num3, num4, num5, num6, num7, DateTimeKind.Utc);
        }

        public static string XsdDateTimeToString(long val)
        {
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            BreakDownXsdDateTime(val, out num, out num2, out num3, out num4, out num5, out num6, out num7);
            StringBuilder sb = new StringBuilder(20);
            WriteDate(sb, num, num2, num3);
            sb.Append('T');
            WriteTime(sb, num4, num5, num6, num7);
            sb.Append('Z');
            return sb.ToString();
        }

        public static DateTime XsdDateToDateTime(long val)
        {
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            bool flag;
            BreakDownXsdDate(val, out num, out num2, out num3, out flag, out num4, out num5);
            DateTime time = new DateTime(num, num2, num3, 0, 0, 0, DateTimeKind.Utc);
            int num6 = (flag ? -1 : 1) * ((num4 * 60) + num5);
            return TimeZone.CurrentTimeZone.ToLocalTime(time.AddMinutes((double) num6));
        }

        public static string XsdDateToString(long val)
        {
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            bool flag;
            BreakDownXsdDate(val, out num, out num2, out num3, out flag, out num4, out num5);
            StringBuilder sb = new StringBuilder(20);
            WriteDate(sb, num, num2, num3);
            WriteTimeZone(sb, flag, num4, num5);
            return sb.ToString();
        }

        public static DateTime XsdKatmaiDateOffsetToDateTime(byte[] data, int offset)
        {
            return XsdKatmaiDateOffsetToDateTimeOffset(data, offset).LocalDateTime;
        }

        public static DateTimeOffset XsdKatmaiDateOffsetToDateTimeOffset(byte[] data, int offset)
        {
            return XsdKatmaiDateTimeOffsetToDateTimeOffset(data, offset);
        }

        public static string XsdKatmaiDateOffsetToString(byte[] data, int offset)
        {
            DateTimeOffset offset2 = XsdKatmaiDateOffsetToDateTimeOffset(data, offset);
            StringBuilder sb = new StringBuilder(0x10);
            WriteDate(sb, offset2.Year, offset2.Month, offset2.Day);
            WriteTimeZone(sb, offset2.Offset);
            return sb.ToString();
        }

        public static DateTime XsdKatmaiDateTimeOffsetToDateTime(byte[] data, int offset)
        {
            return XsdKatmaiDateTimeOffsetToDateTimeOffset(data, offset).LocalDateTime;
        }

        public static DateTimeOffset XsdKatmaiDateTimeOffsetToDateTimeOffset(byte[] data, int offset)
        {
            long katmaiTimeTicks = GetKatmaiTimeTicks(data, ref offset);
            long katmaiDateTicks = GetKatmaiDateTicks(data, ref offset);
            long katmaiTimeZoneTicks = GetKatmaiTimeZoneTicks(data, offset);
            return new DateTimeOffset((katmaiDateTicks + katmaiTimeTicks) + katmaiTimeZoneTicks, new TimeSpan(katmaiTimeZoneTicks));
        }

        public static string XsdKatmaiDateTimeOffsetToString(byte[] data, int offset)
        {
            DateTimeOffset dt = XsdKatmaiDateTimeOffsetToDateTimeOffset(data, offset);
            StringBuilder sb = new StringBuilder(0x27);
            WriteDate(sb, dt.Year, dt.Month, dt.Day);
            sb.Append('T');
            WriteTimeFullPrecision(sb, dt.Hour, dt.Minute, dt.Second, GetFractions(dt));
            WriteTimeZone(sb, dt.Offset);
            return sb.ToString();
        }

        public static DateTime XsdKatmaiDateTimeToDateTime(byte[] data, int offset)
        {
            long katmaiTimeTicks = GetKatmaiTimeTicks(data, ref offset);
            long katmaiDateTicks = GetKatmaiDateTicks(data, ref offset);
            return new DateTime(katmaiDateTicks + katmaiTimeTicks);
        }

        public static DateTimeOffset XsdKatmaiDateTimeToDateTimeOffset(byte[] data, int offset)
        {
            return XsdKatmaiDateTimeToDateTime(data, offset);
        }

        public static string XsdKatmaiDateTimeToString(byte[] data, int offset)
        {
            DateTime dt = XsdKatmaiDateTimeToDateTime(data, offset);
            StringBuilder sb = new StringBuilder(0x21);
            WriteDate(sb, dt.Year, dt.Month, dt.Day);
            sb.Append('T');
            WriteTimeFullPrecision(sb, dt.Hour, dt.Minute, dt.Second, GetFractions(dt));
            return sb.ToString();
        }

        public static DateTime XsdKatmaiDateToDateTime(byte[] data, int offset)
        {
            return new DateTime(GetKatmaiDateTicks(data, ref offset));
        }

        public static DateTimeOffset XsdKatmaiDateToDateTimeOffset(byte[] data, int offset)
        {
            return XsdKatmaiDateToDateTime(data, offset);
        }

        public static string XsdKatmaiDateToString(byte[] data, int offset)
        {
            DateTime time = XsdKatmaiDateToDateTime(data, offset);
            StringBuilder sb = new StringBuilder(10);
            WriteDate(sb, time.Year, time.Month, time.Day);
            return sb.ToString();
        }

        public static DateTime XsdKatmaiTimeOffsetToDateTime(byte[] data, int offset)
        {
            return XsdKatmaiTimeOffsetToDateTimeOffset(data, offset).LocalDateTime;
        }

        public static DateTimeOffset XsdKatmaiTimeOffsetToDateTimeOffset(byte[] data, int offset)
        {
            return XsdKatmaiDateTimeOffsetToDateTimeOffset(data, offset);
        }

        public static string XsdKatmaiTimeOffsetToString(byte[] data, int offset)
        {
            DateTimeOffset dt = XsdKatmaiTimeOffsetToDateTimeOffset(data, offset);
            StringBuilder sb = new StringBuilder(0x16);
            WriteTimeFullPrecision(sb, dt.Hour, dt.Minute, dt.Second, GetFractions(dt));
            WriteTimeZone(sb, dt.Offset);
            return sb.ToString();
        }

        public static DateTime XsdKatmaiTimeToDateTime(byte[] data, int offset)
        {
            return XsdKatmaiDateTimeToDateTime(data, offset);
        }

        public static DateTimeOffset XsdKatmaiTimeToDateTimeOffset(byte[] data, int offset)
        {
            return XsdKatmaiTimeToDateTime(data, offset);
        }

        public static string XsdKatmaiTimeToString(byte[] data, int offset)
        {
            DateTime dt = XsdKatmaiTimeToDateTime(data, offset);
            StringBuilder sb = new StringBuilder(0x10);
            WriteTimeFullPrecision(sb, dt.Hour, dt.Minute, dt.Second, GetFractions(dt));
            return sb.ToString();
        }

        public static DateTime XsdTimeToDateTime(long val)
        {
            int num;
            int num2;
            int num3;
            int num4;
            BreakDownXsdTime(val, out num, out num2, out num3, out num4);
            return new DateTime(1, 1, 1, num, num2, num3, num4, DateTimeKind.Utc);
        }

        public static string XsdTimeToString(long val)
        {
            int num;
            int num2;
            int num3;
            int num4;
            BreakDownXsdTime(val, out num, out num2, out num3, out num4);
            StringBuilder sb = new StringBuilder(0x10);
            WriteTime(sb, num, num2, num3, num4);
            sb.Append('Z');
            return sb.ToString();
        }
    }
}

