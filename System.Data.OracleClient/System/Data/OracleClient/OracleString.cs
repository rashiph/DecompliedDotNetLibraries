namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Sequential)]
    public struct OracleString : IComparable, INullable
    {
        private string _value;
        public static readonly OracleString Empty;
        public static readonly OracleString Null;
        private OracleString(bool isNull)
        {
            this._value = isNull ? null : string.Empty;
        }

        public OracleString(string s)
        {
            this._value = s;
        }

        internal OracleString(NativeBuffer buffer, int valueOffset, int lengthOffset, MetaType metaType, OracleConnection connection, bool boundAsUCS2, bool outputParameterBinding)
        {
            this._value = MarshalToString(buffer, valueOffset, lengthOffset, metaType, connection, boundAsUCS2, outputParameterBinding);
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
        public string Value
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
        public char this[int index]
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
            if (!(obj.GetType() == typeof(OracleString)))
            {
                throw System.Data.Common.ADP.WrongType(obj.GetType(), typeof(OracleString));
            }
            OracleString str = (OracleString) obj;
            if (this.IsNull)
            {
                if (!str.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (str.IsNull)
            {
                return 1;
            }
            return CultureInfo.CurrentCulture.CompareInfo.Compare(this._value, str._value);
        }

        public override bool Equals(object value)
        {
            if (value is OracleString)
            {
                OracleBoolean flag = this == ((OracleString) value);
                return flag.Value;
            }
            return false;
        }

        internal static int GetChars(NativeBuffer buffer, int valueOffset, int lengthOffset, MetaType metaType, OracleConnection connection, bool boundAsUCS2, int sourceOffset, char[] destinationBuffer, int destinationOffset, int charCount)
        {
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                buffer.DangerousAddRef(ref success);
                if (boundAsUCS2)
                {
                    if (!metaType.IsLong)
                    {
                        Marshal.Copy(buffer.DangerousGetDataPtrWithBaseOffset(valueOffset + (System.Data.Common.ADP.CharSize * sourceOffset)), destinationBuffer, destinationOffset, charCount);
                        return charCount;
                    }
                    NativeBuffer_LongColumnData.CopyOutOfLineChars(buffer.ReadIntPtr(valueOffset), sourceOffset, destinationBuffer, destinationOffset, charCount);
                    return charCount;
                }
                string str = MarshalToString(buffer, valueOffset, lengthOffset, metaType, connection, boundAsUCS2, false);
                int length = str.Length;
                int num = ((sourceOffset + charCount) > length) ? (length - sourceOffset) : charCount;
                Buffer.BlockCopy(str.ToCharArray(sourceOffset, num), 0, destinationBuffer, destinationOffset * System.Data.Common.ADP.CharSize, num * System.Data.Common.ADP.CharSize);
                charCount = num;
            }
            finally
            {
                if (success)
                {
                    buffer.DangerousRelease();
                }
            }
            return charCount;
        }

        public override int GetHashCode()
        {
            if (!this.IsNull)
            {
                return this._value.GetHashCode();
            }
            return 0;
        }

        internal static int GetLength(NativeBuffer buffer, int lengthOffset, MetaType metaType)
        {
            int num;
            if (metaType.IsLong)
            {
                num = buffer.ReadInt32(lengthOffset);
            }
            else
            {
                num = buffer.ReadInt16(lengthOffset);
            }
            GC.KeepAlive(buffer);
            return num;
        }

        internal static string MarshalToString(NativeBuffer buffer, int valueOffset, int lengthOffset, MetaType metaType, OracleConnection connection, bool boundAsUCS2, bool outputParameterBinding)
        {
            string str;
            int length = GetLength(buffer, lengthOffset, metaType);
            if (boundAsUCS2 && outputParameterBinding)
            {
                length /= 2;
            }
            bool flag = metaType.IsLong && !outputParameterBinding;
            if (boundAsUCS2)
            {
                if (flag)
                {
                    byte[] destinationBuffer = new byte[length * System.Data.Common.ADP.CharSize];
                    NativeBuffer_LongColumnData.CopyOutOfLineBytes(buffer.ReadIntPtr(valueOffset), 0, destinationBuffer, 0, length * System.Data.Common.ADP.CharSize);
                    str = Encoding.Unicode.GetString(destinationBuffer);
                }
                else
                {
                    str = buffer.PtrToStringUni(valueOffset, length);
                }
            }
            else
            {
                byte[] buffer2;
                if (flag)
                {
                    buffer2 = new byte[length];
                    NativeBuffer_LongColumnData.CopyOutOfLineBytes(buffer.ReadIntPtr(valueOffset), 0, buffer2, 0, length);
                }
                else
                {
                    buffer2 = buffer.ReadBytes(valueOffset, length);
                }
                str = connection.GetString(buffer2, metaType.UsesNationalCharacterSet);
            }
            GC.KeepAlive(buffer);
            return str;
        }

        internal static int MarshalToNative(object value, int offset, int size, NativeBuffer buffer, int bufferOffset, OCI.DATATYPE ociType, bool bindAsUCS2)
        {
            string str;
            string str2;
            Encoding encoding = bindAsUCS2 ? Encoding.Unicode : Encoding.UTF8;
            if (value is OracleString)
            {
                str = ((OracleString) value)._value;
            }
            else
            {
                str = (string) value;
            }
            if ((offset == 0) && (size == 0))
            {
                str2 = str;
            }
            else if ((size == 0) || ((offset + size) > str.Length))
            {
                str2 = str.Substring(offset);
            }
            else
            {
                str2 = str.Substring(offset, size);
            }
            byte[] bytes = encoding.GetBytes(str2);
            int length = bytes.Length;
            int num3 = length;
            if (length != 0)
            {
                int num2 = length;
                if (bindAsUCS2)
                {
                    num2 /= 2;
                }
                if (OCI.DATATYPE.LONGVARCHAR == ociType)
                {
                    buffer.WriteInt32(bufferOffset, num2);
                    bufferOffset += 4;
                    num3 += 4;
                }
                buffer.WriteBytes(bufferOffset, bytes, 0, length);
            }
            return num3;
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return System.Data.Common.ADP.NullString;
            }
            return this._value;
        }

        public static OracleString Concat(OracleString x, OracleString y)
        {
            return (x + y);
        }

        public static OracleBoolean Equals(OracleString x, OracleString y)
        {
            return (x == y);
        }

        public static OracleBoolean GreaterThan(OracleString x, OracleString y)
        {
            return (x > y);
        }

        public static OracleBoolean GreaterThanOrEqual(OracleString x, OracleString y)
        {
            return (x >= y);
        }

        public static OracleBoolean LessThan(OracleString x, OracleString y)
        {
            return (x < y);
        }

        public static OracleBoolean LessThanOrEqual(OracleString x, OracleString y)
        {
            return (x <= y);
        }

        public static OracleBoolean NotEquals(OracleString x, OracleString y)
        {
            return (x != y);
        }

        public static implicit operator OracleString(string s)
        {
            return new OracleString(s);
        }

        public static explicit operator string(OracleString x)
        {
            return x.Value;
        }

        public static OracleString operator +(OracleString x, OracleString y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return new OracleString(x._value + y._value);
        }

        public static OracleBoolean operator ==(OracleString x, OracleString y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) == 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >(OracleString x, OracleString y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) > 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >=(OracleString x, OracleString y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) >= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <(OracleString x, OracleString y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) < 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <=(OracleString x, OracleString y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) <= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator !=(OracleString x, OracleString y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) != 0);
            }
            return OracleBoolean.Null;
        }

        static OracleString()
        {
            Empty = new OracleString(false);
            Null = new OracleString(true);
        }
    }
}

