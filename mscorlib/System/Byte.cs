namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Byte : IComparable, IFormattable, IConvertible, IComparable<byte>, IEquatable<byte>
    {
        public const byte MaxValue = 0xff;
        public const byte MinValue = 0;
        private byte m_value;
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is byte))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeByte"));
            }
            return (this - ((byte) value));
        }

        public int CompareTo(byte value)
        {
            return (this - value);
        }

        public override bool Equals(object obj)
        {
            return ((obj is byte) && (this == ((byte) obj)));
        }

        public bool Equals(byte obj)
        {
            return (this == obj);
        }

        public override int GetHashCode()
        {
            return this;
        }

        public static byte Parse(string s)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static byte Parse(string s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static byte Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static byte Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static byte Parse(string s, NumberStyles style, NumberFormatInfo info)
        {
            int num = 0;
            try
            {
                num = Number.ParseInt32(s, style, info);
            }
            catch (OverflowException exception)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"), exception);
            }
            if ((num < 0) || (num > 0xff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) num;
        }

        public static bool TryParse(string s, out byte result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out byte result)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out byte result)
        {
            int num;
            result = 0;
            if (!Number.TryParseInt32(s, style, info, out num))
            {
                return false;
            }
            if ((num < 0) || (num > 0xff))
            {
                return false;
            }
            result = (byte) num;
            return true;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return Number.FormatInt32(this, null, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return Number.FormatInt32(this, format, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Number.FormatInt32(this, null, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return Number.FormatInt32(this, format, NumberFormatInfo.GetInstance(provider));
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Byte;
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
            return this;
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Byte", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }
    }
}

