namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct OracleNumber : IComparable, INullable
    {
        private const string WholeDigitPattern = "999999999999999999999999999999999999999999999999999999999999999";
        private const int WholeDigitPattern_Length = 0x3f;
        private static double doubleMinValue;
        private static double doubleMaxValue;
        private static readonly byte[] OciNumberValue_DecimalMaxValue;
        private static readonly byte[] OciNumberValue_DecimalMinValue;
        private static readonly byte[] OciNumberValue_E;
        private static readonly byte[] OciNumberValue_MaxValue;
        private static readonly byte[] OciNumberValue_MinValue;
        private static readonly byte[] OciNumberValue_MinusOne;
        private static readonly byte[] OciNumberValue_One;
        private static readonly byte[] OciNumberValue_Pi;
        private static readonly byte[] OciNumberValue_TwoPow64;
        private static readonly byte[] OciNumberValue_Zero;
        private byte[] _value;
        public static readonly OracleNumber E;
        public static readonly int MaxPrecision;
        public static readonly int MaxScale;
        public static readonly int MinScale;
        public static readonly OracleNumber MaxValue;
        public static readonly OracleNumber MinValue;
        public static readonly OracleNumber MinusOne;
        public static readonly OracleNumber Null;
        public static readonly OracleNumber One;
        public static readonly OracleNumber PI;
        public static readonly OracleNumber Zero;
        private OracleNumber(bool isNull)
        {
            this._value = isNull ? null : new byte[0x16];
        }

        private OracleNumber(byte[] bits)
        {
            this._value = bits;
        }

        public OracleNumber(decimal decValue) : this(false)
        {
            OracleConnection.ExecutePermission.Demand();
            FromDecimal(TempEnvironment.GetErrorHandle(), decValue, this._value);
        }

        public OracleNumber(double dblValue) : this(false)
        {
            OracleConnection.ExecutePermission.Demand();
            FromDouble(TempEnvironment.GetErrorHandle(), dblValue, this._value);
        }

        public OracleNumber(int intValue) : this(false)
        {
            OracleConnection.ExecutePermission.Demand();
            FromInt32(TempEnvironment.GetErrorHandle(), intValue, this._value);
        }

        public OracleNumber(long longValue) : this(false)
        {
            OracleConnection.ExecutePermission.Demand();
            FromInt64(TempEnvironment.GetErrorHandle(), longValue, this._value);
        }

        public OracleNumber(OracleNumber from)
        {
            byte[] buffer = from._value;
            if (buffer != null)
            {
                this._value = (byte[]) buffer.Clone();
            }
            else
            {
                this._value = null;
            }
        }

        internal OracleNumber(string s) : this(false)
        {
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            this.FromString(errorHandle, s, this._value);
        }

        internal OracleNumber(NativeBuffer buffer, int valueOffset) : this(false)
        {
            buffer.ReadBytes(valueOffset, this._value, 0, 0x16);
        }

        public bool IsNull
        {
            get
            {
                return (null == this._value);
            }
        }
        public decimal Value
        {
            get
            {
                return (decimal) this;
            }
        }
        public int CompareTo(object obj)
        {
            if (!(obj.GetType() == typeof(OracleNumber)))
            {
                throw System.Data.Common.ADP.WrongType(obj.GetType(), typeof(OracleNumber));
            }
            OracleNumber number = (OracleNumber) obj;
            if (this.IsNull)
            {
                if (!number.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (number.IsNull)
            {
                return 1;
            }
            OracleConnection.ExecutePermission.Demand();
            return InternalCmp(TempEnvironment.GetErrorHandle(), this._value, number._value);
        }

        public override bool Equals(object value)
        {
            if (value is OracleNumber)
            {
                OracleBoolean flag = this == ((OracleNumber) value);
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

        internal static decimal MarshalToDecimal(NativeBuffer buffer, int valueOffset, OracleConnection connection)
        {
            byte[] buffer2 = buffer.ReadBytes(valueOffset, 0x16);
            return ToDecimal(connection.ErrorHandle, buffer2);
        }

        internal static int MarshalToInt32(NativeBuffer buffer, int valueOffset, OracleConnection connection)
        {
            byte[] buffer2 = buffer.ReadBytes(valueOffset, 0x16);
            return ToInt32(connection.ErrorHandle, buffer2);
        }

        internal static long MarshalToInt64(NativeBuffer buffer, int valueOffset, OracleConnection connection)
        {
            byte[] buffer2 = buffer.ReadBytes(valueOffset, 0x16);
            return ToInt64(connection.ErrorHandle, buffer2);
        }

        internal static int MarshalToNative(object value, NativeBuffer buffer, int offset, OracleConnection connection)
        {
            byte[] buffer2;
            if (value is OracleNumber)
            {
                buffer2 = ((OracleNumber) value)._value;
            }
            else
            {
                OciErrorHandle errorHandle = connection.ErrorHandle;
                buffer2 = new byte[0x16];
                if (value is decimal)
                {
                    FromDecimal(errorHandle, (decimal) value, buffer2);
                }
                else if (value is int)
                {
                    FromInt32(errorHandle, (int) value, buffer2);
                }
                else if (value is long)
                {
                    FromInt64(errorHandle, (long) value, buffer2);
                }
                else
                {
                    FromDouble(errorHandle, (double) value, buffer2);
                }
            }
            buffer.WriteBytes(offset, buffer2, 0, 0x16);
            return 0x16;
        }

        public static OracleNumber Parse(string s)
        {
            if (s == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("s");
            }
            return new OracleNumber(s);
        }

        private static void InternalAdd(OciErrorHandle errorHandle, byte[] x, byte[] y, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberAdd(errorHandle, x, y, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static int InternalCmp(OciErrorHandle errorHandle, byte[] value1, byte[] value2)
        {
            int num2;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberCmp(errorHandle, value1, value2, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return num2;
        }

        private static void InternalDiv(OciErrorHandle errorHandle, byte[] x, byte[] y, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberDiv(errorHandle, x, y, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static bool InternalIsInt(OciErrorHandle errorHandle, byte[] n)
        {
            int num2;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberIsInt(errorHandle, n, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return (0 != num2);
        }

        private static void InternalMod(OciErrorHandle errorHandle, byte[] x, byte[] y, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberMod(errorHandle, x, y, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void InternalMul(OciErrorHandle errorHandle, byte[] x, byte[] y, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberMul(errorHandle, x, y, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void InternalNeg(OciErrorHandle errorHandle, byte[] x, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberNeg(errorHandle, x, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static int InternalSign(OciErrorHandle errorHandle, byte[] n)
        {
            int num2;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberSign(errorHandle, n, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return num2;
        }

        private static void InternalShift(OciErrorHandle errorHandle, byte[] n, int digits, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberShift(errorHandle, n, digits, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void InternalSub(OciErrorHandle errorHandle, byte[] x, byte[] y, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberSub(errorHandle, x, y, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void InternalTrunc(OciErrorHandle errorHandle, byte[] n, int position, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberTrunc(errorHandle, n, position, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void FromDecimal(OciErrorHandle errorHandle, decimal decimalValue, byte[] result)
        {
            int[] bits = decimal.GetBits(decimalValue);
            ulong ulongValue = (((ulong) bits[1]) << 0x20) | ((ulong) bits[0]);
            uint uintValue = (uint) bits[2];
            int num3 = bits[3] >> 0x1f;
            int num = (bits[3] >> 0x10) & 0x7f;
            FromUInt64(errorHandle, ulongValue, result);
            if (uintValue != 0)
            {
                byte[] buffer = new byte[0x16];
                FromUInt32(errorHandle, uintValue, buffer);
                InternalMul(errorHandle, buffer, OciNumberValue_TwoPow64, buffer);
                InternalAdd(errorHandle, result, buffer, result);
            }
            if (num3 != 0)
            {
                InternalNeg(errorHandle, result, result);
            }
            if (num != 0)
            {
                InternalShift(errorHandle, result, -num, result);
            }
        }

        private static void FromDouble(OciErrorHandle errorHandle, double dblValue, byte[] result)
        {
            if ((dblValue < doubleMinValue) || (dblValue > doubleMaxValue))
            {
                throw System.Data.Common.ADP.OperationResultedInOverflow();
            }
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberFromReal(errorHandle, ref dblValue, 8, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void FromInt32(OciErrorHandle errorHandle, int intValue, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberFromInt(errorHandle, ref intValue, 4, OCI.SIGN.OCI_NUMBER_SIGNED, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void FromUInt32(OciErrorHandle errorHandle, uint uintValue, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberFromInt(errorHandle, ref uintValue, 4, OCI.SIGN.OCI_NUMBER_UNSIGNED, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void FromInt64(OciErrorHandle errorHandle, long longValue, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberFromInt(errorHandle, ref longValue, 8, OCI.SIGN.OCI_NUMBER_SIGNED, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private static void FromUInt64(OciErrorHandle errorHandle, ulong ulongValue, byte[] result)
        {
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberFromInt(errorHandle, ref ulongValue, 8, OCI.SIGN.OCI_NUMBER_UNSIGNED, result);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        private void FromStringOfDigits(OciErrorHandle errorHandle, string s, byte[] result)
        {
            if (s.Length <= 0x3f)
            {
                int rc = System.Data.Common.UnsafeNativeMethods.OCINumberFromText(errorHandle, s, (uint) s.Length, "999999999999999999999999999999999999999999999999999999999999999", 0x3f, IntPtr.Zero, 0, result);
                if (rc != 0)
                {
                    OracleException.Check(errorHandle, rc);
                }
            }
            else
            {
                byte[] buffer = new byte[0x16];
                string str2 = s.Substring(0, 0x3f);
                string str = s.Substring(0x3f);
                this.FromStringOfDigits(errorHandle, str2, buffer);
                this.FromStringOfDigits(errorHandle, str, result);
                InternalShift(errorHandle, buffer, str.Length, buffer);
                InternalAdd(errorHandle, result, buffer, result);
            }
        }

        private void FromString(OciErrorHandle errorHandle, string s, byte[] result)
        {
            byte[] buffer = new byte[0x16];
            int digits = 0;
            s = s.Trim();
            int length = s.IndexOfAny("eE".ToCharArray());
            if (length > 0)
            {
                digits = int.Parse(s.Substring(length + 1), CultureInfo.InvariantCulture);
                s = s.Substring(0, length);
            }
            bool flag = false;
            if ('-' == s[0])
            {
                flag = true;
                s = s.Substring(1);
            }
            else if ('+' == s[0])
            {
                s = s.Substring(1);
            }
            int index = s.IndexOf('.');
            if (0 <= index)
            {
                string str = s.Substring(index + 1);
                this.FromStringOfDigits(errorHandle, str, result);
                InternalShift(errorHandle, result, -str.Length, result);
                if (index != 0)
                {
                    this.FromStringOfDigits(errorHandle, s.Substring(0, index), buffer);
                    InternalAdd(errorHandle, result, buffer, result);
                }
            }
            else
            {
                this.FromStringOfDigits(errorHandle, s, result);
            }
            if (digits != 0)
            {
                InternalShift(errorHandle, result, digits, result);
            }
            if (flag)
            {
                InternalNeg(errorHandle, result, result);
            }
            GC.KeepAlive(s);
        }

        private static decimal ToDecimal(OciErrorHandle errorHandle, byte[] value)
        {
            byte[] n = (byte[]) value.Clone();
            byte[] result = new byte[0x16];
            byte scale = 0;
            int num4 = InternalSign(errorHandle, n);
            if (num4 < 0)
            {
                InternalNeg(errorHandle, n, n);
            }
            if (!InternalIsInt(errorHandle, n))
            {
                int digits = 2 * ((n[0] - ((n[1] & 0x7f) - 0x40)) - 1);
                InternalShift(errorHandle, n, digits, n);
                scale = (byte) (scale + ((byte) digits));
                while (!InternalIsInt(errorHandle, n))
                {
                    InternalShift(errorHandle, n, 1, n);
                    scale = (byte) (scale + 1);
                }
            }
            InternalMod(errorHandle, n, OciNumberValue_TwoPow64, result);
            ulong num2 = ToUInt64(errorHandle, result);
            InternalDiv(errorHandle, n, OciNumberValue_TwoPow64, result);
            InternalTrunc(errorHandle, result, 0, result);
            uint num6 = ToUInt32(errorHandle, result);
            return new decimal((int) (num2 & 0xffffffffL), (int) (num2 >> 0x20), (int) num6, num4 < 0, scale);
        }

        private static int ToInt32(OciErrorHandle errorHandle, byte[] value)
        {
            int num2;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberToInt(errorHandle, value, 4, OCI.SIGN.OCI_NUMBER_SIGNED, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return num2;
        }

        private static uint ToUInt32(OciErrorHandle errorHandle, byte[] value)
        {
            uint num2;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberToInt(errorHandle, value, 4, OCI.SIGN.OCI_NUMBER_UNSIGNED, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return num2;
        }

        private static long ToInt64(OciErrorHandle errorHandle, byte[] value)
        {
            long num2;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberToInt(errorHandle, value, 8, OCI.SIGN.OCI_NUMBER_SIGNED, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return num2;
        }

        private static ulong ToUInt64(OciErrorHandle errorHandle, byte[] value)
        {
            ulong num2;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberToInt(errorHandle, value, 8, OCI.SIGN.OCI_NUMBER_UNSIGNED, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return num2;
        }

        private static string ToString(OciErrorHandle errorHandle, byte[] value)
        {
            byte[] buffer = new byte[0x40];
            uint length = (uint) buffer.Length;
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberToText(errorHandle, value, "TM9", 3, IntPtr.Zero, 0, ref length, buffer);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            int index = Array.IndexOf<byte>(buffer, 0x3a);
            index = (index > 0) ? index : Array.LastIndexOf(buffer, 0);
            return Encoding.Default.GetString(buffer, 0, (index > 0) ? index : ((int) length));
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return System.Data.Common.ADP.NullString;
            }
            OracleConnection.ExecutePermission.Demand();
            return ToString(TempEnvironment.GetErrorHandle(), this._value);
        }

        public static OracleBoolean operator ==(OracleNumber x, OracleNumber y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) == 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >(OracleNumber x, OracleNumber y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) > 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >=(OracleNumber x, OracleNumber y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) >= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <(OracleNumber x, OracleNumber y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) < 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <=(OracleNumber x, OracleNumber y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) <= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator !=(OracleNumber x, OracleNumber y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) != 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleNumber operator -(OracleNumber x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalNeg(errorHandle, x._value, number._value);
            return number;
        }

        public static OracleNumber operator +(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalAdd(errorHandle, x._value, y._value, number._value);
            return number;
        }

        public static OracleNumber operator -(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalSub(errorHandle, x._value, y._value, number._value);
            return number;
        }

        public static OracleNumber operator *(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalMul(errorHandle, x._value, y._value, number._value);
            return number;
        }

        public static OracleNumber operator /(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalDiv(errorHandle, x._value, y._value, number._value);
            return number;
        }

        public static OracleNumber operator %(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalMod(errorHandle, x._value, y._value, number._value);
            return number;
        }

        public static explicit operator decimal(OracleNumber x)
        {
            if (x.IsNull)
            {
                throw System.Data.Common.ADP.DataIsNull();
            }
            OracleConnection.ExecutePermission.Demand();
            return ToDecimal(TempEnvironment.GetErrorHandle(), x._value);
        }

        public static explicit operator double(OracleNumber x)
        {
            double num2;
            if (x.IsNull)
            {
                throw System.Data.Common.ADP.DataIsNull();
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberToReal(errorHandle, x._value, 8, out num2);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return num2;
        }

        public static explicit operator int(OracleNumber x)
        {
            if (x.IsNull)
            {
                throw System.Data.Common.ADP.DataIsNull();
            }
            OracleConnection.ExecutePermission.Demand();
            return ToInt32(TempEnvironment.GetErrorHandle(), x._value);
        }

        public static explicit operator long(OracleNumber x)
        {
            if (x.IsNull)
            {
                throw System.Data.Common.ADP.DataIsNull();
            }
            OracleConnection.ExecutePermission.Demand();
            return ToInt64(TempEnvironment.GetErrorHandle(), x._value);
        }

        public static explicit operator OracleNumber(decimal x)
        {
            return new OracleNumber(x);
        }

        public static explicit operator OracleNumber(double x)
        {
            return new OracleNumber(x);
        }

        public static explicit operator OracleNumber(int x)
        {
            return new OracleNumber(x);
        }

        public static explicit operator OracleNumber(long x)
        {
            return new OracleNumber(x);
        }

        public static explicit operator OracleNumber(string x)
        {
            return new OracleNumber(x);
        }

        public static OracleNumber Abs(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberAbs(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Acos(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberArcCos(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Add(OracleNumber x, OracleNumber y)
        {
            return (x + y);
        }

        public static OracleNumber Asin(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberArcSin(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Atan(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberArcTan(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Atan2(OracleNumber y, OracleNumber x)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberArcTan2(errorHandle, y._value, x._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Ceiling(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberCeil(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Cos(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberCos(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Cosh(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberHypCos(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Divide(OracleNumber x, OracleNumber y)
        {
            return (x / y);
        }

        public static OracleBoolean Equals(OracleNumber x, OracleNumber y)
        {
            return (x == y);
        }

        public static OracleNumber Exp(OracleNumber p)
        {
            if (p.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberExp(errorHandle, p._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Floor(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberFloor(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleBoolean GreaterThan(OracleNumber x, OracleNumber y)
        {
            return (x > y);
        }

        public static OracleBoolean GreaterThanOrEqual(OracleNumber x, OracleNumber y)
        {
            return (x >= y);
        }

        public static OracleBoolean LessThan(OracleNumber x, OracleNumber y)
        {
            return (x < y);
        }

        public static OracleBoolean LessThanOrEqual(OracleNumber x, OracleNumber y)
        {
            return (x <= y);
        }

        public static OracleNumber Log(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberLn(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Log(OracleNumber n, int newBase)
        {
            return Log(n, new OracleNumber(newBase));
        }

        public static OracleNumber Log(OracleNumber n, OracleNumber newBase)
        {
            if (n.IsNull || newBase.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberLog(errorHandle, newBase._value, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Log10(OracleNumber n)
        {
            return Log(n, new OracleNumber(10));
        }

        public static OracleNumber Max(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (!OracleBoolean.op_True(x > y))
            {
                return y;
            }
            return x;
        }

        public static OracleNumber Min(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (!OracleBoolean.op_True(x < y))
            {
                return y;
            }
            return x;
        }

        public static OracleNumber Modulo(OracleNumber x, OracleNumber y)
        {
            return (x % y);
        }

        public static OracleNumber Multiply(OracleNumber x, OracleNumber y)
        {
            return (x * y);
        }

        public static OracleNumber Negate(OracleNumber x)
        {
            return -x;
        }

        public static OracleBoolean NotEquals(OracleNumber x, OracleNumber y)
        {
            return (x != y);
        }

        public static OracleNumber Pow(OracleNumber x, int y)
        {
            if (x.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberIntPower(errorHandle, x._value, y, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Pow(OracleNumber x, OracleNumber y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberPower(errorHandle, x._value, y._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Round(OracleNumber n, int position)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberRound(errorHandle, n._value, position, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Shift(OracleNumber n, int digits)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalShift(errorHandle, n._value, digits, number._value);
            return number;
        }

        public static OracleNumber Sign(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            return ((InternalSign(TempEnvironment.GetErrorHandle(), n._value) > 0) ? One : MinusOne);
        }

        public static OracleNumber Sin(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberSin(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Sinh(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberHypSin(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Sqrt(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberSqrt(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Subtract(OracleNumber x, OracleNumber y)
        {
            return (x - y);
        }

        public static OracleNumber Tan(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberTan(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Tanh(OracleNumber n)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            int rc = System.Data.Common.UnsafeNativeMethods.OCINumberHypTan(errorHandle, n._value, number._value);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            return number;
        }

        public static OracleNumber Truncate(OracleNumber n, int position)
        {
            if (n.IsNull)
            {
                return Null;
            }
            OracleConnection.ExecutePermission.Demand();
            OciErrorHandle errorHandle = TempEnvironment.GetErrorHandle();
            OracleNumber number = new OracleNumber(false);
            InternalTrunc(errorHandle, n._value, position, number._value);
            return number;
        }

        static OracleNumber()
        {
            doubleMinValue = -9.99999999999999E+125;
            doubleMaxValue = 9.99999999999999E+125;
            OciNumberValue_DecimalMaxValue = new byte[] { 
                0x10, 0xcf, 8, 0x5d, 0x1d, 0x11, 0x1a, 15, 0x1b, 0x2c, 0x26, 0x3b, 0x5d, 0x31, 0x63, 0x1f, 
                40
             };
            OciNumberValue_DecimalMinValue = new byte[] { 
                0x11, 0x30, 0x5e, 9, 0x49, 0x55, 0x4c, 0x57, 0x4b, 0x3a, 0x40, 0x2b, 9, 0x35, 3, 0x47, 
                0x3e, 0x66
             };
            OciNumberValue_E = new byte[] { 
                0x15, 0xc1, 3, 0x48, 0x53, 0x52, 0x53, 0x55, 60, 5, 0x35, 0x24, 0x25, 3, 0x58, 0x30, 
                14, 0x35, 0x43, 0x19, 0x62, 0x4d
             };
            OciNumberValue_MaxValue = new byte[] { 
                20, 0xff, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 
                100, 100, 100, 100, 100
             };
            OciNumberValue_MinValue = new byte[] { 
                0x15, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
                2, 2, 2, 2, 2, 0x66
             };
            OciNumberValue_MinusOne = new byte[] { 3, 0x3e, 100, 0x66 };
            OciNumberValue_One = new byte[] { 2, 0xc1, 2 };
            OciNumberValue_Pi = new byte[] { 
                0x15, 0xc1, 4, 15, 0x10, 0x5d, 0x42, 0x24, 90, 80, 0x21, 0x27, 0x2f, 0x1b, 0x2c, 0x27, 
                0x21, 80, 0x33, 0x1d, 0x55, 0x15
             };
            OciNumberValue_TwoPow64 = new byte[] { 11, 0xca, 0x13, 0x2d, 0x44, 0x2d, 8, 0x26, 10, 0x38, 0x11, 0x11 };
            OciNumberValue_Zero = new byte[] { 1, 0x80 };
            E = new OracleNumber(OciNumberValue_E);
            MaxPrecision = 0x26;
            MaxScale = 0x7f;
            MinScale = -84;
            MaxValue = new OracleNumber(OciNumberValue_MaxValue);
            MinValue = new OracleNumber(OciNumberValue_MinValue);
            MinusOne = new OracleNumber(OciNumberValue_MinusOne);
            Null = new OracleNumber(true);
            One = new OracleNumber(OciNumberValue_One);
            PI = new OracleNumber(OciNumberValue_Pi);
            Zero = new OracleNumber(OciNumberValue_Zero);
        }
    }
}

