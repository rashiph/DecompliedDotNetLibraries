namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct OracleMonthSpan : IComparable, INullable
    {
        private const int MaxMonth = 0x2b1ac;
        private const int MinMonth = -176556;
        private const int NullValue = 0x7fffffff;
        private int _value;
        public static readonly OracleMonthSpan MaxValue;
        public static readonly OracleMonthSpan MinValue;
        public static readonly OracleMonthSpan Null;
        internal OracleMonthSpan(bool isNull)
        {
            this._value = 0x7fffffff;
        }

        public OracleMonthSpan(int months)
        {
            this._value = months;
            AssertValid(this._value);
        }

        public OracleMonthSpan(int years, int months)
        {
            try
            {
                this._value = (years * 12) + months;
            }
            catch (OverflowException)
            {
                throw System.Data.Common.ADP.MonthOutOfRange();
            }
            AssertValid(this._value);
        }

        public OracleMonthSpan(OracleMonthSpan from)
        {
            this._value = from._value;
        }

        internal OracleMonthSpan(NativeBuffer buffer, int valueOffset)
        {
            this._value = MarshalToInt32(buffer, valueOffset);
        }

        public bool IsNull
        {
            get
            {
                return (0x7fffffff == this._value);
            }
        }
        public int Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                return this._value;
            }
        }
        private static void AssertValid(int monthSpan)
        {
            if ((monthSpan < -176556) || (monthSpan > 0x2b1ac))
            {
                throw System.Data.Common.ADP.MonthOutOfRange();
            }
        }

        public int CompareTo(object obj)
        {
            if (!(obj.GetType() == typeof(OracleMonthSpan)))
            {
                throw System.Data.Common.ADP.WrongType(obj.GetType(), typeof(OracleMonthSpan));
            }
            OracleMonthSpan span = (OracleMonthSpan) obj;
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
            return this._value.CompareTo(span._value);
        }

        public override bool Equals(object value)
        {
            if (value is OracleMonthSpan)
            {
                OracleBoolean flag = this == ((OracleMonthSpan) value);
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

        internal static int MarshalToInt32(NativeBuffer buffer, int valueOffset)
        {
            byte[] buffer2 = buffer.ReadBytes(valueOffset, 5);
            int num3 = ((((buffer2[0] << 0x18) | (buffer2[1] << 0x10)) | (buffer2[2] << 8)) | buffer2[3]) - ((int) 0x80000000L);
            int num2 = buffer2[4] - 60;
            int monthSpan = (num3 * 12) + num2;
            AssertValid(monthSpan);
            return monthSpan;
        }

        internal static int MarshalToNative(object value, NativeBuffer buffer, int offset)
        {
            int num2;
            if (value is OracleMonthSpan)
            {
                num2 = ((OracleMonthSpan) value)._value;
            }
            else
            {
                num2 = (int) value;
            }
            byte[] source = new byte[5];
            int num = (num2 / 12) + ((int) 0x80000000L);
            int num3 = num2 % 12;
            source[0] = (byte) (num >> 0x18);
            source[1] = (byte) ((num >> 0x10) & 0xff);
            source[2] = (byte) ((num >> 8) & 0xff);
            source[3] = (byte) (num & 0xff);
            source[4] = (byte) (num3 + 60);
            buffer.WriteBytes(offset, source, 0, 5);
            return 5;
        }

        public static OracleMonthSpan Parse(string s)
        {
            return new OracleMonthSpan(int.Parse(s, CultureInfo.InvariantCulture));
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return System.Data.Common.ADP.NullString;
            }
            return this.Value.ToString(CultureInfo.CurrentCulture);
        }

        public static OracleBoolean Equals(OracleMonthSpan x, OracleMonthSpan y)
        {
            return (x == y);
        }

        public static OracleBoolean GreaterThan(OracleMonthSpan x, OracleMonthSpan y)
        {
            return (x > y);
        }

        public static OracleBoolean GreaterThanOrEqual(OracleMonthSpan x, OracleMonthSpan y)
        {
            return (x >= y);
        }

        public static OracleBoolean LessThan(OracleMonthSpan x, OracleMonthSpan y)
        {
            return (x < y);
        }

        public static OracleBoolean LessThanOrEqual(OracleMonthSpan x, OracleMonthSpan y)
        {
            return (x <= y);
        }

        public static OracleBoolean NotEquals(OracleMonthSpan x, OracleMonthSpan y)
        {
            return (x != y);
        }

        public static explicit operator int(OracleMonthSpan x)
        {
            if (x.IsNull)
            {
                throw System.Data.Common.ADP.DataIsNull();
            }
            return x.Value;
        }

        public static explicit operator OracleMonthSpan(string x)
        {
            return Parse(x);
        }

        public static OracleBoolean operator ==(OracleMonthSpan x, OracleMonthSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) == 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >(OracleMonthSpan x, OracleMonthSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) > 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >=(OracleMonthSpan x, OracleMonthSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) >= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <(OracleMonthSpan x, OracleMonthSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) < 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <=(OracleMonthSpan x, OracleMonthSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) <= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator !=(OracleMonthSpan x, OracleMonthSpan y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) != 0);
            }
            return OracleBoolean.Null;
        }

        static OracleMonthSpan()
        {
            MaxValue = new OracleMonthSpan(0x2b1ac);
            MinValue = new OracleMonthSpan(-176556);
            Null = new OracleMonthSpan(true);
        }
    }
}

