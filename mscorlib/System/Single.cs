namespace System
{
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Single : IComparable, IFormattable, IConvertible, IComparable<float>, IEquatable<float>
    {
        public const float MinValue = -3.402823E+38f;
        public const float Epsilon = 1.401298E-45f;
        public const float MaxValue = 3.402823E+38f;
        public const float PositiveInfinity = (float) 1.0 / (float) 0.0;
        public const float NegativeInfinity = (float) -1.0 / (float) 0.0;
        public const float NaN = (float) 1.0 / (float) 0.0;
        internal float m_value;
        [SecuritySafeCritical]
        public static unsafe bool IsInfinity(float f)
        {
            return ((*(((int*) &f)) & 0x7fffffff) == 0x7f800000);
        }

        [SecuritySafeCritical]
        public static unsafe bool IsPositiveInfinity(float f)
        {
            return (*(((int*) &f)) == 0x7f800000);
        }

        [SecuritySafeCritical]
        public static unsafe bool IsNegativeInfinity(float f)
        {
            return (*(((int*) &f)) == -8388608);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool IsNaN(float f)
        {
            return (f != f);
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is float))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeSingle"));
            }
            float f = (float) value;
            if (this < f)
            {
                return -1;
            }
            if (this > f)
            {
                return 1;
            }
            if (this != f)
            {
                if (!IsNaN(this))
                {
                    return 1;
                }
                if (!IsNaN(f))
                {
                    return -1;
                }
            }
            return 0;
        }

        public int CompareTo(float value)
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

        public static bool operator ==(float left, float right)
        {
            return (left == right);
        }

        public static bool operator !=(float left, float right)
        {
            return !(left == right);
        }

        public static bool operator <(float left, float right)
        {
            return (left < right);
        }

        public static bool operator >(float left, float right)
        {
            return (left > right);
        }

        public static bool operator <=(float left, float right)
        {
            return (left <= right);
        }

        public static bool operator >=(float left, float right)
        {
            return (left >= right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is float))
            {
                return false;
            }
            float f = (float) obj;
            return ((f == this) || (IsNaN(f) && IsNaN(this)));
        }

        public bool Equals(float obj)
        {
            return ((obj == this) || (IsNaN(obj) && IsNaN(this)));
        }

        [SecuritySafeCritical]
        public override unsafe int GetHashCode()
        {
            float num = this;
            if (num == 0f)
            {
                return 0;
            }
            return *(((int*) &num));
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return Number.FormatSingle(this, null, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Number.FormatSingle(this, null, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return Number.FormatSingle(this, format, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return Number.FormatSingle(this, format, NumberFormatInfo.GetInstance(provider));
        }

        public static float Parse(string s)
        {
            return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
        }

        public static float Parse(string s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static float Parse(string s, IFormatProvider provider)
        {
            return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
        }

        public static float Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }

        private static float Parse(string s, NumberStyles style, NumberFormatInfo info)
        {
            return Number.ParseSingle(s, style, info);
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, out float result)
        {
            return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out float result)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out float result)
        {
            if (s == null)
            {
                result = 0f;
                return false;
            }
            if (!Number.TryParseSingle(s, style, info, out result))
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
            return TypeCode.Single;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Single", "Char" }));
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
            return this;
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Single", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }
    }
}

