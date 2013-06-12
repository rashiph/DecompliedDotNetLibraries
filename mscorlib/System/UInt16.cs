namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true), CLSCompliant(false)]
    public struct UInt16 : IComparable, IFormattable, IConvertible, IComparable<ushort>, IEquatable<ushort>
    {
        public const ushort MaxValue = 0xffff;
        public const ushort MinValue = 0;
        private ushort m_value;
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is ushort))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeUInt16"));
            }
            return (this - ((ushort) value));
        }

        public int CompareTo(ushort value)
        {
            return (this - value);
        }

        public override bool Equals(object obj)
        {
            return ((obj is ushort) && (this == ((ushort) obj)));
        }

        public bool Equals(ushort obj)
        {
            return (this == obj);
        }

        public override int GetHashCode()
        {
            return this;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return Number.FormatUInt32(this, null, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Number.FormatUInt32(this, null, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return Number.FormatUInt32(this, format, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return Number.FormatUInt32(this, format, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static ushort Parse(string s)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        [CLSCompliant(false)]
        public static ushort Parse(string s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        [CLSCompliant(false)]
        public static ushort Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static ushort Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static ushort Parse(string s, NumberStyles style, NumberFormatInfo info)
        {
            uint num = 0;
            try
            {
                num = Number.ParseUInt32(s, style, info);
            }
            catch (OverflowException exception)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"), exception);
            }
            if (num > 0xffff)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) num;
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public static bool TryParse(string s, out ushort result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ushort result)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out ushort result)
        {
            uint num;
            result = 0;
            if (!Number.TryParseUInt32(s, style, info, out num))
            {
                return false;
            }
            if (num > 0xffff)
            {
                return false;
            }
            result = (ushort) num;
            return true;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.UInt16;
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
            return this;
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
            return Convert.ToInt64(this);
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "UInt16", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }
    }
}

