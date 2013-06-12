namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Int16 : IComparable, IFormattable, IConvertible, IComparable<short>, IEquatable<short>
    {
        public const short MaxValue = 0x7fff;
        public const short MinValue = -32768;
        internal short m_value;
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is short))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeInt16"));
            }
            return (this - ((short) value));
        }

        public int CompareTo(short value)
        {
            return (this - value);
        }

        public override bool Equals(object obj)
        {
            return ((obj is short) && (this == ((short) obj)));
        }

        public bool Equals(short obj)
        {
            return (this == obj);
        }

        public override int GetHashCode()
        {
            return (((ushort) this) | (this << 0x10));
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return Number.FormatInt32(this, null, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Number.FormatInt32(this, null, NumberFormatInfo.GetInstance(provider));
        }

        public string ToString(string format)
        {
            return this.ToString(format, NumberFormatInfo.CurrentInfo);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return this.ToString(format, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        private string ToString(string format, NumberFormatInfo info)
        {
            if ((((this >= 0) || (format == null)) || (format.Length <= 0)) || ((format[0] != 'X') && (format[0] != 'x')))
            {
                return Number.FormatInt32(this, format, info);
            }
            uint num = (uint) (this & 0xffff);
            return Number.FormatUInt32(num, format, info);
        }

        public static short Parse(string s)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        public static short Parse(string s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static short Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        public static short Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static short Parse(string s, NumberStyles style, NumberFormatInfo info)
        {
            int num = 0;
            try
            {
                num = Number.ParseInt32(s, style, info);
            }
            catch (OverflowException exception)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"), exception);
            }
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if ((num < 0) || (num > 0xffff))
                {
                    throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
                }
                return (short) num;
            }
            if ((num < -32768) || (num > 0x7fff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) num;
        }

        public static bool TryParse(string s, out short result)
        {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out short result)
        {
            int num;
            result = 0;
            if (!Number.TryParseInt32(s, style, info, out num))
            {
                return false;
            }
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if ((num < 0) || (num > 0xffff))
                {
                    return false;
                }
                result = (short) num;
                return true;
            }
            if ((num < -32768) || (num > 0x7fff))
            {
                return false;
            }
            result = (short) num;
            return true;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int16;
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
            return this;
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Int16", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }
    }
}

