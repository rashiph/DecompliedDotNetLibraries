namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Int64 : IComparable, IFormattable, IConvertible, IComparable<long>, IEquatable<long>
    {
        public const long MaxValue = 0x7fffffffffffffffL;
        public const long MinValue = -9223372036854775808L;
        internal long m_value;
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is long))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeInt64"));
            }
            long num = (long) value;
            if (this < num)
            {
                return -1;
            }
            if (this > num)
            {
                return 1;
            }
            return 0;
        }

        public int CompareTo(long value)
        {
            if (this < value)
            {
                return -1;
            }
            if (this > value)
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            return ((obj is long) && (this == ((long) obj)));
        }

        public bool Equals(long obj)
        {
            return (this == obj);
        }

        public override int GetHashCode()
        {
            return (((int) this) ^ ((int) (this >> 0x20)));
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return Number.FormatInt64(this, null, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Number.FormatInt64(this, null, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return Number.FormatInt64(this, format, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return Number.FormatInt64(this, format, NumberFormatInfo.GetInstance(provider));
        }

        public static long Parse(string s)
        {
            return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static long Parse(string s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseInt64(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static long Parse(string s, IFormatProvider provider)
        {
            return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static long Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseInt64(s, style, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, out long result)
        {
            return Number.TryParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out long result)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.TryParseInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int64;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return this;
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(this);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Int64", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }
    }
}

