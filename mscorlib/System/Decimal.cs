namespace System
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Decimal : IFormattable, IComparable, IConvertible, IDeserializationCallback, IComparable<decimal>, IEquatable<decimal>
    {
        private const int SignMask = -2147483648;
        private const byte DECIMAL_NEG = 0x80;
        private const byte DECIMAL_ADD = 0;
        private const int ScaleMask = 0xff0000;
        private const int ScaleShift = 0x10;
        private const int MaxInt32Scale = 9;
        [DecimalConstant(0, 0, (uint) 0, (uint) 0, (uint) 0)]
        public static readonly decimal Zero;
        [DecimalConstant(0, 0, (uint) 0, (uint) 0, (uint) 1)]
        public static readonly decimal One;
        [DecimalConstant(0, 0x80, (uint) 0, (uint) 0, (uint) 1)]
        public static readonly decimal MinusOne;
        [DecimalConstant(0, 0, uint.MaxValue, uint.MaxValue, uint.MaxValue)]
        public static readonly decimal MaxValue;
        [DecimalConstant(0, 0x80, uint.MaxValue, uint.MaxValue, uint.MaxValue)]
        public static readonly decimal MinValue;
        [DecimalConstant(0x1b, 0x80, (uint) 0, (uint) 0, (uint) 1)]
        private static readonly decimal NearNegativeZero;
        [DecimalConstant(0x1b, 0, (uint) 0, (uint) 0, (uint) 1)]
        private static readonly decimal NearPositiveZero;
        private static uint[] Powers10;
        private int flags;
        private int hi;
        private int lo;
        private int mid;
        public Decimal(int value)
        {
            int num = value;
            if (num >= 0)
            {
                this.flags = 0;
            }
            else
            {
                this.flags = -2147483648;
                num = -num;
            }
            this.lo = num;
            this.mid = 0;
            this.hi = 0;
        }

        [CLSCompliant(false)]
        public Decimal(uint value)
        {
            this.flags = 0;
            this.lo = (int) value;
            this.mid = 0;
            this.hi = 0;
        }

        public Decimal(long value)
        {
            long num = value;
            if (num >= 0L)
            {
                this.flags = 0;
            }
            else
            {
                this.flags = -2147483648;
                num = -num;
            }
            this.lo = (int) num;
            this.mid = (int) (num >> 0x20);
            this.hi = 0;
        }

        [CLSCompliant(false)]
        public Decimal(ulong value)
        {
            this.flags = 0;
            this.lo = (int) value;
            this.mid = (int) (value >> 0x20);
            this.hi = 0;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern Decimal(float value);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern Decimal(double value);
        [ForceTokenStabilization]
        internal Decimal(Currency value)
        {
            decimal num = Currency.ToDecimal(value);
            this.lo = num.lo;
            this.mid = num.mid;
            this.hi = num.hi;
            this.flags = num.flags;
        }

        public static long ToOACurrency(decimal value)
        {
            Currency currency = new Currency(value);
            return currency.ToOACurrency();
        }

        public static decimal FromOACurrency(long cy)
        {
            return Currency.ToDecimal(Currency.FromOACurrency(cy));
        }

        public Decimal(int[] bits)
        {
            this.lo = 0;
            this.mid = 0;
            this.hi = 0;
            this.flags = 0;
            this.SetBits(bits);
        }

        private void SetBits(int[] bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException("bits");
            }
            if (bits.Length == 4)
            {
                int num = bits[3];
                if (((num & 0x7f00ffff) == 0) && ((num & 0xff0000) <= 0x1c0000))
                {
                    this.lo = bits[0];
                    this.mid = bits[1];
                    this.hi = bits[2];
                    this.flags = num;
                    return;
                }
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_DecBitCtor"));
        }

        public Decimal(int lo, int mid, int hi, bool isNegative, byte scale)
        {
            if (scale > 0x1c)
            {
                throw new ArgumentOutOfRangeException("scale", Environment.GetResourceString("ArgumentOutOfRange_DecimalScale"));
            }
            this.lo = lo;
            this.mid = mid;
            this.hi = hi;
            this.flags = scale << 0x10;
            if (isNegative)
            {
                this.flags |= -2147483648;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            try
            {
                this.SetBits(GetBits(this));
            }
            catch (ArgumentException exception)
            {
                throw new SerializationException(Environment.GetResourceString("Overflow_Decimal"), exception);
            }
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            try
            {
                this.SetBits(GetBits(this));
            }
            catch (ArgumentException exception)
            {
                throw new SerializationException(Environment.GetResourceString("Overflow_Decimal"), exception);
            }
        }

        private Decimal(int lo, int mid, int hi, int flags)
        {
            if (((flags & 0x7f00ffff) != 0) || ((flags & 0xff0000) > 0x1c0000))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DecBitCtor"));
            }
            this.lo = lo;
            this.mid = mid;
            this.hi = hi;
            this.flags = flags;
        }

        internal static decimal Abs(decimal d)
        {
            return new decimal(d.lo, d.mid, d.hi, d.flags & 0x7fffffff);
        }

        [SecuritySafeCritical]
        public static decimal Add(decimal d1, decimal d2)
        {
            FCallAddSub(ref d1, ref d2, 0);
            return d1;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallAddSub(ref decimal d1, ref decimal d2, byte bSign);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallAddSubOverflowed(ref decimal d1, ref decimal d2, byte bSign, ref bool overflowed);
        public static decimal Ceiling(decimal d)
        {
            return -Floor(-d);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int Compare(decimal d1, decimal d2)
        {
            return FCallCompare(ref d1, ref d2);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        private static extern int FCallCompare(ref decimal d1, ref decimal d2);
        [SecuritySafeCritical]
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is decimal))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDecimal"));
            }
            decimal num = (decimal) value;
            return FCallCompare(ref this, ref num);
        }

        [SecuritySafeCritical]
        public int CompareTo(decimal value)
        {
            return FCallCompare(ref this, ref value);
        }

        [SecuritySafeCritical]
        public static decimal Divide(decimal d1, decimal d2)
        {
            FCallDivide(ref d1, ref d2);
            return d1;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallDivide(ref decimal d1, ref decimal d2);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallDivideOverflowed(ref decimal d1, ref decimal d2, ref bool overflowed);
        [SecuritySafeCritical]
        public override bool Equals(object value)
        {
            if (value is decimal)
            {
                decimal num = (decimal) value;
                return (FCallCompare(ref this, ref num) == 0);
            }
            return false;
        }

        [SecuritySafeCritical]
        public bool Equals(decimal value)
        {
            return (FCallCompare(ref this, ref value) == 0);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public override extern int GetHashCode();
        [SecuritySafeCritical]
        public static bool Equals(decimal d1, decimal d2)
        {
            return (FCallCompare(ref d1, ref d2) == 0);
        }

        [SecuritySafeCritical]
        public static decimal Floor(decimal d)
        {
            FCallFloor(ref d);
            return d;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallFloor(ref decimal d);
        [SecuritySafeCritical]
        public override string ToString()
        {
            return Number.FormatDecimal(this, null, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            return Number.FormatDecimal(this, format, NumberFormatInfo.CurrentInfo);
        }

        [SecuritySafeCritical]
        public string ToString(IFormatProvider provider)
        {
            return Number.FormatDecimal(this, null, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public string ToString(string format, IFormatProvider provider)
        {
            return Number.FormatDecimal(this, format, NumberFormatInfo.GetInstance(provider));
        }

        public static decimal Parse(string s)
        {
            return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo);
        }

        public static decimal Parse(string s, NumberStyles style)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return Number.ParseDecimal(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static decimal Parse(string s, IFormatProvider provider)
        {
            return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.GetInstance(provider));
        }

        public static decimal Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return Number.ParseDecimal(s, style, NumberFormatInfo.GetInstance(provider));
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, out decimal result)
        {
            return Number.TryParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result);
        }

        [SecuritySafeCritical]
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out decimal result)
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
            return Number.TryParseDecimal(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static int[] GetBits(decimal d)
        {
            return new int[] { d.lo, d.mid, d.hi, d.flags };
        }

        internal static void GetBytes(decimal d, byte[] buffer)
        {
            buffer[0] = (byte) d.lo;
            buffer[1] = (byte) (d.lo >> 8);
            buffer[2] = (byte) (d.lo >> 0x10);
            buffer[3] = (byte) (d.lo >> 0x18);
            buffer[4] = (byte) d.mid;
            buffer[5] = (byte) (d.mid >> 8);
            buffer[6] = (byte) (d.mid >> 0x10);
            buffer[7] = (byte) (d.mid >> 0x18);
            buffer[8] = (byte) d.hi;
            buffer[9] = (byte) (d.hi >> 8);
            buffer[10] = (byte) (d.hi >> 0x10);
            buffer[11] = (byte) (d.hi >> 0x18);
            buffer[12] = (byte) d.flags;
            buffer[13] = (byte) (d.flags >> 8);
            buffer[14] = (byte) (d.flags >> 0x10);
            buffer[15] = (byte) (d.flags >> 0x18);
        }

        internal static decimal ToDecimal(byte[] buffer)
        {
            int lo = ((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18);
            int mid = ((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 0x10)) | (buffer[7] << 0x18);
            int hi = ((buffer[8] | (buffer[9] << 8)) | (buffer[10] << 0x10)) | (buffer[11] << 0x18);
            return new decimal(lo, mid, hi, ((buffer[12] | (buffer[13] << 8)) | (buffer[14] << 0x10)) | (buffer[15] << 0x18));
        }

        private static void InternalAddUInt32RawUnchecked(ref decimal value, uint i)
        {
            uint lo = (uint) value.lo;
            uint num2 = lo + i;
            value.lo = (int) num2;
            if ((num2 < lo) || (num2 < i))
            {
                lo = (uint) value.mid;
                num2 = lo + 1;
                value.mid = (int) num2;
                if ((num2 < lo) || (num2 < 1))
                {
                    value.hi++;
                }
            }
        }

        private static uint InternalDivRemUInt32(ref decimal value, uint divisor)
        {
            ulong hi;
            uint num = 0;
            if (value.hi != 0)
            {
                hi = (ulong) value.hi;
                value.hi = (int) ((uint) (hi / ((ulong) divisor)));
                num = (uint) (hi % ((ulong) divisor));
            }
            if ((value.mid != 0) || (num != 0))
            {
                hi = (num << 0x20) | ((ulong) value.mid);
                value.mid = (int) ((uint) (hi / ((ulong) divisor)));
                num = (uint) (hi % ((ulong) divisor));
            }
            if ((value.lo == 0) && (num == 0))
            {
                return num;
            }
            hi = (num << 0x20) | ((ulong) value.lo);
            value.lo = (int) ((uint) (hi / ((ulong) divisor)));
            return (uint) (hi % ((ulong) divisor));
        }

        private static void InternalRoundFromZero(ref decimal d, int decimalCount)
        {
            int num = (d.flags & 0xff0000) >> 0x10;
            int num2 = num - decimalCount;
            if (num2 > 0)
            {
                uint num3;
                uint num4;
                do
                {
                    int index = (num2 > 9) ? 9 : num2;
                    num4 = Powers10[index];
                    num3 = InternalDivRemUInt32(ref d, num4);
                    num2 -= index;
                }
                while (num2 > 0);
                if (num3 >= (num4 >> 1))
                {
                    InternalAddUInt32RawUnchecked(ref d, 1);
                }
                d.flags = ((decimalCount << 0x10) & 0xff0000) | (d.flags & -2147483648);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        internal static decimal Max(decimal d1, decimal d2)
        {
            if (FCallCompare(ref d1, ref d2) < 0)
            {
                return d2;
            }
            return d1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        internal static decimal Min(decimal d1, decimal d2)
        {
            if (FCallCompare(ref d1, ref d2) >= 0)
            {
                return d2;
            }
            return d1;
        }

        public static decimal Remainder(decimal d1, decimal d2)
        {
            d2.flags = (d2.flags & 0x7fffffff) | (d1.flags & -2147483648);
            if (Abs(d1) < Abs(d2))
            {
                return d1;
            }
            d1 -= d2;
            if (d1 == 0M)
            {
                d1.flags = (d1.flags & 0x7fffffff) | (d2.flags & -2147483648);
            }
            decimal num2 = Truncate(d1 / d2) * d2;
            decimal num3 = d1 - num2;
            if ((d1.flags & -2147483648) == (num3.flags & -2147483648))
            {
                return num3;
            }
            if ((-0.000000000000000000000000001M <= num3) && (num3 <= 0.000000000000000000000000001M))
            {
                num3.flags = (num3.flags & 0x7fffffff) | (d1.flags & -2147483648);
                return num3;
            }
            return (num3 + d2);
        }

        [SecuritySafeCritical]
        public static decimal Multiply(decimal d1, decimal d2)
        {
            FCallMultiply(ref d1, ref d2);
            return d1;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallMultiply(ref decimal d1, ref decimal d2);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallMultiplyOverflowed(ref decimal d1, ref decimal d2, ref bool overflowed);
        public static decimal Negate(decimal d)
        {
            return new decimal(d.lo, d.mid, d.hi, d.flags ^ -2147483648);
        }

        public static decimal Round(decimal d)
        {
            return Round(d, 0);
        }

        [SecuritySafeCritical]
        public static decimal Round(decimal d, int decimals)
        {
            FCallRound(ref d, decimals);
            return d;
        }

        [SecuritySafeCritical]
        public static decimal Round(decimal d, MidpointRounding mode)
        {
            return Round(d, 0, mode);
        }

        [SecuritySafeCritical]
        public static decimal Round(decimal d, int decimals, MidpointRounding mode)
        {
            if ((decimals < 0) || (decimals > 0x1c))
            {
                throw new ArgumentOutOfRangeException("decimals", Environment.GetResourceString("ArgumentOutOfRange_DecimalRound"));
            }
            if ((mode < MidpointRounding.ToEven) || (mode > MidpointRounding.AwayFromZero))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { mode, "MidpointRounding" }), "mode");
            }
            if (mode == MidpointRounding.ToEven)
            {
                FCallRound(ref d, decimals);
                return d;
            }
            InternalRoundFromZero(ref d, decimals);
            return d;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallRound(ref decimal d, int decimals);
        [SecuritySafeCritical]
        public static decimal Subtract(decimal d1, decimal d2)
        {
            FCallAddSub(ref d1, ref d2, 0x80);
            return d1;
        }

        public static byte ToByte(decimal value)
        {
            uint num;
            try
            {
                num = ToUInt32(value);
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

        [CLSCompliant(false)]
        public static sbyte ToSByte(decimal value)
        {
            int num;
            try
            {
                num = ToInt32(value);
            }
            catch (OverflowException exception)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"), exception);
            }
            if ((num < -128) || (num > 0x7f))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) num;
        }

        public static short ToInt16(decimal value)
        {
            int num;
            try
            {
                num = ToInt32(value);
            }
            catch (OverflowException exception)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"), exception);
            }
            if ((num < -32768) || (num > 0x7fff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) num;
        }

        [SecuritySafeCritical]
        internal static Currency ToCurrency(decimal d)
        {
            Currency result = new Currency();
            FCallToCurrency(ref result, d);
            return result;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallToCurrency(ref Currency result, decimal d);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double ToDouble(decimal d);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int FCallToInt32(decimal d);
        [SecuritySafeCritical]
        public static int ToInt32(decimal d)
        {
            if ((d.flags & 0xff0000) != 0)
            {
                FCallTruncate(ref d);
            }
            if ((d.hi == 0) && (d.mid == 0))
            {
                int lo = d.lo;
                if (d.flags >= 0)
                {
                    if (lo >= 0)
                    {
                        return lo;
                    }
                }
                else
                {
                    lo = -lo;
                    if (lo <= 0)
                    {
                        return lo;
                    }
                }
            }
            throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
        }

        [SecuritySafeCritical]
        public static long ToInt64(decimal d)
        {
            if ((d.flags & 0xff0000) != 0)
            {
                FCallTruncate(ref d);
            }
            if (d.hi == 0)
            {
                long num = (d.lo & ((long) 0xffffffffL)) | (d.mid << 0x20);
                if (d.flags >= 0)
                {
                    if (num >= 0L)
                    {
                        return num;
                    }
                }
                else
                {
                    num = -num;
                    if (num <= 0L)
                    {
                        return num;
                    }
                }
            }
            throw new OverflowException(Environment.GetResourceString("Overflow_Int64"));
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(decimal value)
        {
            uint num;
            try
            {
                num = ToUInt32(value);
            }
            catch (OverflowException exception)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"), exception);
            }
            if ((num < 0) || (num > 0xffff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) num;
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public static uint ToUInt32(decimal d)
        {
            if ((d.flags & 0xff0000) != 0)
            {
                FCallTruncate(ref d);
            }
            if ((d.hi == 0) && (d.mid == 0))
            {
                uint lo = (uint) d.lo;
                if ((d.flags >= 0) || (lo == 0))
                {
                    return lo;
                }
            }
            throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public static ulong ToUInt64(decimal d)
        {
            if ((d.flags & 0xff0000) != 0)
            {
                FCallTruncate(ref d);
            }
            if (d.hi == 0)
            {
                ulong num = ((ulong) d.lo) | (((ulong) d.mid) << 0x20);
                if ((d.flags >= 0) || (num == 0L))
                {
                    return num;
                }
            }
            throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern float ToSingle(decimal d);
        [SecuritySafeCritical]
        public static decimal Truncate(decimal d)
        {
            FCallTruncate(ref d);
            return d;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FCallTruncate(ref decimal d);
        public static implicit operator decimal(byte value)
        {
            return new decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator decimal(sbyte value)
        {
            return new decimal(value);
        }

        public static implicit operator decimal(short value)
        {
            return new decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator decimal(ushort value)
        {
            return new decimal(value);
        }

        public static implicit operator decimal(char value)
        {
            return new decimal(value);
        }

        public static implicit operator decimal(int value)
        {
            return new decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator decimal(uint value)
        {
            return new decimal(value);
        }

        public static implicit operator decimal(long value)
        {
            return new decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator decimal(ulong value)
        {
            return new decimal(value);
        }

        public static explicit operator decimal(float value)
        {
            return new decimal(value);
        }

        public static explicit operator decimal(double value)
        {
            return new decimal(value);
        }

        public static explicit operator byte(decimal value)
        {
            return ToByte(value);
        }

        [CLSCompliant(false)]
        public static explicit operator sbyte(decimal value)
        {
            return ToSByte(value);
        }

        public static explicit operator char(decimal value)
        {
            ushort num;
            try
            {
                num = ToUInt16(value);
            }
            catch (OverflowException exception)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Char"), exception);
            }
            return (char) num;
        }

        public static explicit operator short(decimal value)
        {
            return ToInt16(value);
        }

        [CLSCompliant(false)]
        public static explicit operator ushort(decimal value)
        {
            return ToUInt16(value);
        }

        public static explicit operator int(decimal value)
        {
            return ToInt32(value);
        }

        [CLSCompliant(false)]
        public static explicit operator uint(decimal value)
        {
            return ToUInt32(value);
        }

        public static explicit operator long(decimal value)
        {
            return ToInt64(value);
        }

        [CLSCompliant(false)]
        public static explicit operator ulong(decimal value)
        {
            return ToUInt64(value);
        }

        public static explicit operator float(decimal value)
        {
            return ToSingle(value);
        }

        public static explicit operator double(decimal value)
        {
            return ToDouble(value);
        }

        public static decimal operator +(decimal d)
        {
            return d;
        }

        public static decimal operator -(decimal d)
        {
            return Negate(d);
        }

        public static decimal operator ++(decimal d)
        {
            return Add(d, 1M);
        }

        public static decimal operator --(decimal d)
        {
            return Subtract(d, 1M);
        }

        [SecuritySafeCritical]
        public static decimal operator +(decimal d1, decimal d2)
        {
            FCallAddSub(ref d1, ref d2, 0);
            return d1;
        }

        [SecuritySafeCritical]
        public static decimal operator -(decimal d1, decimal d2)
        {
            FCallAddSub(ref d1, ref d2, 0x80);
            return d1;
        }

        [SecuritySafeCritical]
        public static decimal operator *(decimal d1, decimal d2)
        {
            FCallMultiply(ref d1, ref d2);
            return d1;
        }

        [SecuritySafeCritical]
        public static decimal operator /(decimal d1, decimal d2)
        {
            FCallDivide(ref d1, ref d2);
            return d1;
        }

        public static decimal operator %(decimal d1, decimal d2)
        {
            return Remainder(d1, d2);
        }

        [SecuritySafeCritical]
        public static bool operator ==(decimal d1, decimal d2)
        {
            return (FCallCompare(ref d1, ref d2) == 0);
        }

        [SecuritySafeCritical]
        public static bool operator !=(decimal d1, decimal d2)
        {
            return (FCallCompare(ref d1, ref d2) != 0);
        }

        [SecuritySafeCritical]
        public static bool operator <(decimal d1, decimal d2)
        {
            return (FCallCompare(ref d1, ref d2) < 0);
        }

        [SecuritySafeCritical]
        public static bool operator <=(decimal d1, decimal d2)
        {
            return (FCallCompare(ref d1, ref d2) <= 0);
        }

        [SecuritySafeCritical]
        public static bool operator >(decimal d1, decimal d2)
        {
            return (FCallCompare(ref d1, ref d2) > 0);
        }

        [SecuritySafeCritical]
        public static bool operator >=(decimal d1, decimal d2)
        {
            return (FCallCompare(ref d1, ref d2) >= 0);
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Decimal;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Decimal", "Char" }));
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

        [SecuritySafeCritical]
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this);
        }

        [SecuritySafeCritical]
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this);
        }

        [SecuritySafeCritical]
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
            return this;
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Decimal", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }

        static Decimal()
        {
            Powers10 = new uint[] { 1, 10, 100, 0x3e8, 0x2710, 0x186a0, 0xf4240, 0x989680, 0x5f5e100, 0x3b9aca00 };
            Zero = 0M;
            One = 1M;
            MinusOne = -1M;
            MaxValue = 79228162514264337593543950335M;
            MinValue = -79228162514264337593543950335M;
            NearNegativeZero = -0.000000000000000000000000001M;
            NearPositiveZero = 0.000000000000000000000000001M;
        }
    }
}

