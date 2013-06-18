namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct OracleBinary : IComparable, INullable
    {
        private byte[] _value;
        public static readonly OracleBinary Null;
        private OracleBinary(bool isNull)
        {
            this._value = isNull ? null : new byte[0];
        }

        public OracleBinary(byte[] b)
        {
            this._value = (b == null) ? b : ((byte[]) b.Clone());
        }

        internal OracleBinary(NativeBuffer buffer, int valueOffset, int lengthOffset, MetaType metaType)
        {
            int byteCount = GetLength(buffer, lengthOffset, metaType);
            this._value = new byte[byteCount];
            GetBytes(buffer, valueOffset, metaType, 0, this._value, 0, byteCount);
        }

        public bool IsNull
        {
            get
            {
                return (null == this._value);
            }
        }
        public int Length
        {
            get
            {
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                return this._value.Length;
            }
        }
        public byte[] Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                return (byte[]) this._value.Clone();
            }
        }
        public byte this[int index]
        {
            get
            {
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                return this._value[index];
            }
        }
        public int CompareTo(object obj)
        {
            if (!(obj.GetType() == typeof(OracleBinary)))
            {
                throw System.Data.Common.ADP.WrongType(obj.GetType(), typeof(OracleBinary));
            }
            OracleBinary binary = (OracleBinary) obj;
            if (this.IsNull)
            {
                if (!binary.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (binary.IsNull)
            {
                return 1;
            }
            return PerformCompareByte(this._value, binary._value);
        }

        public override bool Equals(object value)
        {
            if (value is OracleBinary)
            {
                OracleBoolean flag = this == ((OracleBinary) value);
                return flag.Value;
            }
            return false;
        }

        internal static int GetBytes(NativeBuffer buffer, int valueOffset, MetaType metaType, int sourceOffset, byte[] destinationBuffer, int destinationOffset, int byteCount)
        {
            if (!metaType.IsLong)
            {
                buffer.ReadBytes(valueOffset + sourceOffset, destinationBuffer, destinationOffset, byteCount);
                return byteCount;
            }
            NativeBuffer_LongColumnData.CopyOutOfLineBytes(buffer.ReadIntPtr(valueOffset), sourceOffset, destinationBuffer, destinationOffset, byteCount);
            return byteCount;
        }

        internal static int GetLength(NativeBuffer buffer, int lengthOffset, MetaType metaType)
        {
            if (metaType.IsLong)
            {
                return buffer.ReadInt32(lengthOffset);
            }
            return buffer.ReadInt16(lengthOffset);
        }

        public override int GetHashCode()
        {
            if (!this.IsNull)
            {
                return this._value.GetHashCode();
            }
            return 0;
        }

        private static int PerformCompareByte(byte[] x, byte[] y)
        {
            int num;
            int length = x.Length;
            int num2 = y.Length;
            bool flag = length < num2;
            int num4 = flag ? length : num2;
            for (num = 0; num < num4; num++)
            {
                if (x[num] != y[num])
                {
                    if (x[num] < y[num])
                    {
                        return -1;
                    }
                    return 1;
                }
            }
            if (length != num2)
            {
                byte num5 = 0;
                if (flag)
                {
                    for (num = num4; num < num2; num++)
                    {
                        if (y[num] != num5)
                        {
                            return -1;
                        }
                    }
                }
                else
                {
                    for (num = num4; num < length; num++)
                    {
                        if (x[num] != num5)
                        {
                            return 1;
                        }
                    }
                }
            }
            return 0;
        }

        public static OracleBinary Concat(OracleBinary x, OracleBinary y)
        {
            return (x + y);
        }

        public static OracleBoolean Equals(OracleBinary x, OracleBinary y)
        {
            return (x == y);
        }

        public static OracleBoolean GreaterThan(OracleBinary x, OracleBinary y)
        {
            return (x > y);
        }

        public static OracleBoolean GreaterThanOrEqual(OracleBinary x, OracleBinary y)
        {
            return (x >= y);
        }

        public static OracleBoolean LessThan(OracleBinary x, OracleBinary y)
        {
            return (x < y);
        }

        public static OracleBoolean LessThanOrEqual(OracleBinary x, OracleBinary y)
        {
            return (x <= y);
        }

        public static OracleBoolean NotEquals(OracleBinary x, OracleBinary y)
        {
            return (x != y);
        }

        public static implicit operator OracleBinary(byte[] b)
        {
            return new OracleBinary(b);
        }

        public static explicit operator byte[](OracleBinary x)
        {
            return x.Value;
        }

        public static OracleBinary operator +(OracleBinary x, OracleBinary y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            byte[] array = new byte[x._value.Length + y._value.Length];
            x._value.CopyTo(array, 0);
            y._value.CopyTo(array, x.Value.Length);
            return new OracleBinary(array);
        }

        public static OracleBoolean operator ==(OracleBinary x, OracleBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) == 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >(OracleBinary x, OracleBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) > 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >=(OracleBinary x, OracleBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) >= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <(OracleBinary x, OracleBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) < 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <=(OracleBinary x, OracleBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) <= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator !=(OracleBinary x, OracleBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) != 0);
            }
            return OracleBoolean.Null;
        }

        static OracleBinary()
        {
            Null = new OracleBinary(true);
        }
    }
}

