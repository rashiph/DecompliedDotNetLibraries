namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct OracleTimeSpan : IComparable, INullable
    {
        private const int FractionalSecondsPerTick = 100;
        private byte[] _value;
        public static readonly OracleTimeSpan MaxValue;
        public static readonly OracleTimeSpan MinValue;
        public static readonly OracleTimeSpan Null;
        private OracleTimeSpan(bool isNull)
        {
            this._value = null;
        }

        public OracleTimeSpan(TimeSpan ts)
        {
            this._value = new byte[11];
            Pack(this._value, ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ((int) (ts.Ticks % 0x989680L)) * 100);
        }

        public OracleTimeSpan(long ticks)
        {
            this._value = new byte[11];
            TimeSpan span = new TimeSpan(ticks);
            Pack(this._value, span.Days, span.Hours, span.Minutes, span.Seconds, ((int) (span.Ticks % 0x989680L)) * 100);
        }

        public OracleTimeSpan(int hours, int minutes, int seconds) : this(0, hours, minutes, seconds, 0)
        {
        }

        public OracleTimeSpan(int days, int hours, int minutes, int seconds) : this(days, hours, minutes, seconds, 0)
        {
        }

        public OracleTimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            this._value = new byte[11];
            Pack(this._value, days, hours, minutes, seconds, ((int) (milliseconds * 0x2710L)) * 100);
        }

        public OracleTimeSpan(OracleTimeSpan from)
        {
            this._value = new byte[from._value.Length];
            from._value.CopyTo(this._value, 0);
        }

        internal OracleTimeSpan(NativeBuffer buffer, int valueOffset) : this(true)
        {
            this._value = buffer.ReadBytes(valueOffset, 11);
        }

        private static void Pack(byte[] spanval, int days, int hours, int minutes, int seconds, int fsecs)
        {
            days += (int) 0x80000000L;
            fsecs += (int) 0x80000000L;
            spanval[0] = (byte) (days >> 0x18);
            spanval[1] = (byte) ((days >> 0x10) & 0xff);
            spanval[2] = (byte) ((days >> 8) & 0xff);
            spanval[3] = (byte) (days & 0xff);
            spanval[4] = (byte) (hours + 60);
            spanval[5] = (byte) (minutes + 60);
            spanval[6] = (byte) (seconds + 60);
            spanval[7] = (byte) (fsecs >> 0x18);
            spanval[8] = (byte) ((fsecs >> 0x10) & 0xff);
            spanval[9] = (byte) ((fsecs >> 8) & 0xff);
            spanval[10] = (byte) (fsecs & 0xff);
        }

        private static void Unpack(byte[] spanval, out int days, out int hours, out int minutes, out int seconds, out int fsecs)
        {
            days = ((((spanval[0] << 0x18) | (spanval[1] << 0x10)) | (spanval[2] << 8)) | spanval[3]) - ((int) 0x80000000L);
            hours = spanval[4] - 60;
            minutes = spanval[5] - 60;
            seconds = spanval[6] - 60;
            fsecs = ((((spanval[7] << 0x18) | (spanval[8] << 0x10)) | (spanval[9] << 8)) | spanval[10]) - ((int) 0x80000000L);
        }

        public bool IsNull
        {
            get
            {
                return (null == this._value);
            }
        }
        public TimeSpan Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                return ToTimeSpan(this._value);
            }
        }
        public int Days
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num, out num5, out num4, out num3, out num2);
                return num;
            }
        }
        public int Hours
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num5, out num, out num4, out num3, out num2);
                return num;
            }
        }
        public int Minutes
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num5, out num4, out num, out num3, out num2);
                return num;
            }
        }
        public int Seconds
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num5, out num4, out num3, out num, out num2);
                return num;
            }
        }
        public int Milliseconds
        {
            get
            {
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num6, out num5, out num4, out num3, out num2);
                return (int) (((long) (num2 / 100)) / 0x2710L);
            }
        }
        public int CompareTo(object obj)
        {
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            int num8;
            int num9;
            int num10;
            int num11;
            if (!(obj.GetType() == typeof(OracleTimeSpan)))
            {
                throw System.Data.Common.ADP.WrongType(obj.GetType(), typeof(OracleTimeSpan));
            }
            OracleTimeSpan span = (OracleTimeSpan) obj;
            if (this.IsNull)
            {
                if (!span.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (span.IsNull)
            {
                return 1;
            }
            Unpack(this._value, out num11, out num10, out num9, out num8, out num7);
            Unpack(span._value, out num6, out num5, out num4, out num3, out num2);
            int num = num11 - num6;
            if (num != 0)
            {
                return num;
            }
            num = num10 - num5;
            if (num != 0)
            {
                return num;
            }
            num = num9 - num4;
            if (num != 0)
            {
                return num;
            }
            num = num8 - num3;
            if (num != 0)
            {
                return num;
            }
            num = num7 - num2;
            if (num != 0)
            {
                return num;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (value is OracleTimeSpan)
            {
                OracleBoolean flag = this == ((OracleTimeSpan) value);
                return flag.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (!this.IsNull)
            {
                return this._value.GetHashCode();
            }
            return 0;
        }

        internal static TimeSpan MarshalToTimeSpan(NativeBuffer buffer, int valueOffset)
        {
            return ToTimeSpan(buffer.ReadBytes(valueOffset, 11));
        }

        internal static int MarshalToNative(object value, NativeBuffer buffer, int offset)
        {
            byte[] buffer2;
            if (value is OracleTimeSpan)
            {
                buffer2 = ((OracleTimeSpan) value)._value;
            }
            else
            {
                TimeSpan span = (TimeSpan) value;
                buffer2 = new byte[11];
                Pack(buffer2, span.Days, span.Hours, span.Minutes, span.Seconds, ((int) (span.Ticks % 0x989680L)) * 100);
            }
            buffer.WriteBytes(offset, buffer2, 0, 11);
            return 11;
        }

        public static OracleTimeSpan Parse(string s)
        {
            return new OracleTimeSpan(TimeSpan.Parse(s));
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return System.Data.Common.ADP.NullString;
            }
            return this.Value.ToString();
        }

        private static TimeSpan ToTimeSpan(byte[] rawValue)
        {
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            Unpack(rawValue, out num6, out num5, out num4, out num3, out num2);
            long ticks = (((num6 * 0xc92a69c000L) + (num5 * 0x861c46800L)) + (num4 * 0x23c34600L)) + (num3 * 0x989680L);
            if ((num2 < 100) || (num2 > 100))
            {
                ticks += ((long) num2) / 100L;
            }
            return new TimeSpan(ticks);
        }

        public static OracleBoolean Equals(OracleTimeSpan x, OracleTimeSpan y)
        {
            return (x == y);
        }

        public static OracleBoolean GreaterThan(OracleTimeSpan x, OracleTimeSpan y)
        {
            return (x > y);
        }

        public static OracleBoolean GreaterThanOrEqual(OracleTimeSpan x, OracleTimeSpan y)
        {
            return (x >= y);
        }

        public static OracleBoolean LessThan(OracleTimeSpan x, OracleTimeSpan y)
        {
            return (x < y);
        }

        public static OracleBoolean LessThanOrEqual(OracleTimeSpan x, OracleTimeSpan y)
        {
            return (x <= y);
        }

        public static OracleBoolean NotEquals(OracleTimeSpan x, OracleTimeSpan y)
        {
            return (x != y);
        }

        public static explicit operator TimeSpan(OracleTimeSpan x)
        {
            if (x.IsNull)
            {
                throw System.Data.Common.ADP.DataIsNull();
            }
            return x.Value;
        }

        public static explicit operator OracleTimeSpan(string x)
        {
            return Parse(x);
        }

        public static OracleBoolean operator ==(OracleTimeSpan x, OracleTimeSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) == 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >(OracleTimeSpan x, OracleTimeSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) > 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >=(OracleTimeSpan x, OracleTimeSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) >= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <(OracleTimeSpan x, OracleTimeSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) < 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <=(OracleTimeSpan x, OracleTimeSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) <= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator !=(OracleTimeSpan x, OracleTimeSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) != 0);
            }
            return OracleBoolean.Null;
        }

        static OracleTimeSpan()
        {
            MaxValue = new OracleTimeSpan(TimeSpan.MaxValue);
            MinValue = new OracleTimeSpan(TimeSpan.MinValue);
            Null = new OracleTimeSpan(true);
        }
    }
}

