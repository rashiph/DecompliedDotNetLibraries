namespace System
{
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Double : IComparable, IFormattable, IConvertible, IComparable<double>, IEquatable<double>
    {
        public const double MinValue = -1.7976931348623157E+308;
        public const double MaxValue = 1.7976931348623157E+308;
        public const double Epsilon = 4.94065645841247E-324;
        public const double NegativeInfinity = (double) -1.0 / (double) 0.0;
        public const double PositiveInfinity = (double) 1.0 / (double) 0.0;
        public const double NaN = (double) 1.0 / (double) 0.0;
        internal double m_value;
        internal static double NegativeZero;
        [SecuritySafeCritical]
        public static unsafe bool IsInfinity(double d)
        {
            return ((*(((long*) &d)) & 0x7fffffffffffffffL) == 0x7ff0000000000000L);
        }

        public static bool IsPositiveInfinity(double d)
        {
            return (d == PositiveInfinity);
        }

        public static bool IsNegativeInfinity(double d)
        {
            return (d == NegativeInfinity);
        }

        [SecuritySafeCritical]
        internal static unsafe bool IsNegative(double d)
        {
            return ((*(((long*) &d)) & -9223372036854775808L) == -9223372036854775808L);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool IsNaN(double d)
        {
            return (d != d);
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is double))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDouble"));
            }
            double d = (double) value;
            if (this < d)
            {
                return -1;
            }
            if (this > d)
            {
                return 1;
            }
            if (this != d)
            {
                if (!IsNaN(this))
                {
                    return 1;
                }
                if (!IsNaN(d))
                {
                    return -1;
                }
            }
            return 0;
        }

        public int CompareTo(double value)
        {
            if (this < value)
            {
                return -1;
            }
            if (this > value)
            {
                return 1;
            }
            if (this != value)
            {
                if (!IsNaN(this))
                {
                    return 1;
                }
                if (!IsNaN(value))
                {
                    return -1;
                }
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is double))
            {
                return false;
            }
            double d = (double) obj;
            return ((d == this) || (IsNaN(d) && IsNaN(this)));
        }

        public static bool operator ==(double left, double right)
        {
            return (left == right);
        }

        public static bool operator !=(double left, double right)
        {
            return !(left == right);
        }

        public static bool operator <(double left, double right)
        {
            return (left < right);
        }

        public static bool operator >(double left, double right)
        {
            return (left > right);
        }

        public static bool operator <=(double left, double right)
        {
            return (left <= right);
        }

        public static bool operator >=(double left, double right)
        {
            return (left >= right);
        }

        public bool Equals(double obj)
        {
            return ((obj == this) || (IsNaN(obj) && IsNaN(this)));
        }

        [SecuritySafeCritical]
        public override unsafe int GetHashCode()
        {
            double num = this;
            if (num == 0.0)
            {
                return 0;
            }
            long num2 = *((long*) &num);
            return (((int) num2) ^ ((int) (num2 >> 0x20)));
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return Number.FormatDouble(this, null, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return Number.FormatDouble(this, format, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Number.FormatDouble(this, null, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return Number.FormatDouble(this, format, NumberFormatInfo.GetInstance(provider));
        }

        public static double Parse(string s)
        {
            return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
        }

        public static double Parse(string s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static double Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
        }

        public static double Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static double Parse(string s, NumberStyles style, NumberFormatInfo info)
        {
            return Number.ParseDouble(s, style, info);
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, out double result)
        {
            return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out double result)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out double result)
        {
            if (s == null)
            {
                result = 0.0;
                return false;
            }
            if (!Number.TryParseDouble(s, style, info, out result))
            {
                string str = s.Trim();
                if (!str.Equals(info.PositiveInfinitySymbol))
                {
                    if (!str.Equals(info.NegativeInfinitySymbol))
                    {
                        if (!str.Equals(info.NaNSymbol))
                        {
                            return false;
                        }
                        result = NaN;
                    }
                    else
                    {
                        result = NegativeInfinity;
                    }
                }
                else
                {
                    result = PositiveInfinity;
                }
            }
            return true;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Double;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Double", "Char" }));
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
            return this;
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(this);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Double", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }

        static Double()
        {
            NegativeZero = BitConverter.Int64BitsToDouble(-9223372036854775808L);
        }
    }
}

