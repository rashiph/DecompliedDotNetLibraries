namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlInt64 : INullable, IComparable, IXmlSerializable
    {
        private bool m_fNotNull;
        private long m_value;
        private static readonly long x_lLowIntMask;
        private static readonly long x_lHighIntMask;
        public static readonly SqlInt64 Null;
        public static readonly SqlInt64 Zero;
        public static readonly SqlInt64 MinValue;
        public static readonly SqlInt64 MaxValue;
        private SqlInt64(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_value = 0L;
        }

        public SqlInt64(long value)
        {
            this.m_value = value;
            this.m_fNotNull = true;
        }

        public bool IsNull
        {
            get
            {
                return !this.m_fNotNull;
            }
        }
        public long Value
        {
            get
            {
                if (!this.m_fNotNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_value;
            }
        }
        public static implicit operator SqlInt64(long x)
        {
            return new SqlInt64(x);
        }

        public static explicit operator long(SqlInt64 x)
        {
            return x.Value;
        }

        public override string ToString()
        {
            if (!this.IsNull)
            {
                return this.m_value.ToString((IFormatProvider) null);
            }
            return SQLResource.NullString;
        }

        public static SqlInt64 Parse(string s)
        {
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            return new SqlInt64(long.Parse(s, (IFormatProvider) null));
        }

        public static SqlInt64 operator -(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64(-x.m_value);
            }
            return Null;
        }

        public static SqlInt64 operator ~(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64(~x.m_value);
            }
            return Null;
        }

        public static SqlInt64 operator +(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long num = x.m_value + y.m_value;
            if (SameSignLong(x.m_value, y.m_value) && !SameSignLong(x.m_value, num))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt64(num);
        }

        public static SqlInt64 operator -(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long num = x.m_value - y.m_value;
            if (!SameSignLong(x.m_value, y.m_value) && SameSignLong(y.m_value, num))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt64(num);
        }

        public static SqlInt64 operator *(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            bool flag = false;
            long num4 = x.m_value;
            long num3 = y.m_value;
            long num2 = 0L;
            if (num4 < 0L)
            {
                flag = true;
                num4 = -num4;
            }
            if (num3 < 0L)
            {
                flag = !flag;
                num3 = -num3;
            }
            long num8 = num4 & x_lLowIntMask;
            long num6 = (num4 >> 0x20) & x_lLowIntMask;
            long num7 = num3 & x_lLowIntMask;
            long num5 = (num3 >> 0x20) & x_lLowIntMask;
            if ((num6 != 0L) && (num5 != 0L))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            long num = num8 * num7;
            if (num < 0L)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            if (num6 != 0L)
            {
                num2 = num6 * num7;
                if ((num2 < 0L) || (num2 > 0x7fffffffffffffffL))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
            }
            else if (num5 != 0L)
            {
                num2 = num8 * num5;
                if ((num2 < 0L) || (num2 > 0x7fffffffffffffffL))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
            }
            num += num2 << 0x20;
            if (num < 0L)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            if (flag)
            {
                num = -num;
            }
            return new SqlInt64(num);
        }

        public static SqlInt64 operator /(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0L)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            if ((x.m_value == -9223372036854775808L) && (y.m_value == -1L))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt64(x.m_value / y.m_value);
        }

        public static SqlInt64 operator %(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0L)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            if ((x.m_value == -9223372036854775808L) && (y.m_value == -1L))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt64(x.m_value % y.m_value);
        }

        public static SqlInt64 operator &(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt64(x.m_value & y.m_value);
            }
            return Null;
        }

        public static SqlInt64 operator |(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt64(x.m_value | y.m_value);
            }
            return Null;
        }

        public static SqlInt64 operator ^(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt64(x.m_value ^ y.m_value);
            }
            return Null;
        }

        public static explicit operator SqlInt64(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64((long) x.ByteValue);
            }
            return Null;
        }

        public static implicit operator SqlInt64(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64((long) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlInt64(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64((long) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlInt64(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64((long) x.Value);
            }
            return Null;
        }

        public static explicit operator SqlInt64(SqlSingle x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            float num = x.Value;
            if ((num > 9.223372E+18f) || (num < -9.223372E+18f))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt64((long) num);
        }

        public static explicit operator SqlInt64(SqlDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            double num = x.Value;
            if ((num > 9.2233720368547758E+18) || (num < -9.2233720368547758E+18))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt64((long) num);
        }

        public static explicit operator SqlInt64(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64(x.ToInt64());
            }
            return Null;
        }

        public static explicit operator SqlInt64(SqlDecimal x)
        {
            long num;
            if (x.IsNull)
            {
                return Null;
            }
            SqlDecimal num3 = x;
            num3.AdjustScale(-num3.m_bScale, false);
            if (num3.m_bLen > 2)
            {
                throw new OverflowException(SQLResource.ConversionOverflowMessage);
            }
            if (num3.m_bLen == 2)
            {
                ulong num2 = SqlDecimal.DWL(num3.m_data1, num3.m_data2);
                if ((num2 > SqlDecimal.x_llMax) && (num3.IsPositive || (num2 != (((ulong) 1L) + SqlDecimal.x_llMax))))
                {
                    throw new OverflowException(SQLResource.ConversionOverflowMessage);
                }
                num = (long) num2;
            }
            else
            {
                num = num3.m_data1;
            }
            if (!num3.IsPositive)
            {
                num = -num;
            }
            return new SqlInt64(num);
        }

        public static explicit operator SqlInt64(SqlString x)
        {
            if (!x.IsNull)
            {
                return new SqlInt64(long.Parse(x.Value, (IFormatProvider) null));
            }
            return Null;
        }

        private static bool SameSignLong(long x, long y)
        {
            return (((x ^ y) & -9223372036854775808L) == 0L);
        }

        public static SqlBoolean operator ==(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlInt64 x, SqlInt64 y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlInt64 x, SqlInt64 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlInt64 OnesComplement(SqlInt64 x)
        {
            return ~x;
        }

        public static SqlInt64 Add(SqlInt64 x, SqlInt64 y)
        {
            return (x + y);
        }

        public static SqlInt64 Subtract(SqlInt64 x, SqlInt64 y)
        {
            return (x - y);
        }

        public static SqlInt64 Multiply(SqlInt64 x, SqlInt64 y)
        {
            return (x * y);
        }

        public static SqlInt64 Divide(SqlInt64 x, SqlInt64 y)
        {
            return (x / y);
        }

        public static SqlInt64 Mod(SqlInt64 x, SqlInt64 y)
        {
            return (x % y);
        }

        public static SqlInt64 Modulus(SqlInt64 x, SqlInt64 y)
        {
            return (x % y);
        }

        public static SqlInt64 BitwiseAnd(SqlInt64 x, SqlInt64 y)
        {
            return (x & y);
        }

        public static SqlInt64 BitwiseOr(SqlInt64 x, SqlInt64 y)
        {
            return (x | y);
        }

        public static SqlInt64 Xor(SqlInt64 x, SqlInt64 y)
        {
            return (x ^ y);
        }

        public static SqlBoolean Equals(SqlInt64 x, SqlInt64 y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlInt64 x, SqlInt64 y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlInt64 x, SqlInt64 y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlInt64 x, SqlInt64 y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlInt64 x, SqlInt64 y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlInt64 x, SqlInt64 y)
        {
            return (x >= y);
        }

        public SqlBoolean ToSqlBoolean()
        {
            return (SqlBoolean) this;
        }

        public SqlByte ToSqlByte()
        {
            return (SqlByte) this;
        }

        public SqlDouble ToSqlDouble()
        {
            return this;
        }

        public SqlInt16 ToSqlInt16()
        {
            return (SqlInt16) this;
        }

        public SqlInt32 ToSqlInt32()
        {
            return (SqlInt32) this;
        }

        public SqlMoney ToSqlMoney()
        {
            return this;
        }

        public SqlDecimal ToSqlDecimal()
        {
            return this;
        }

        public SqlSingle ToSqlSingle()
        {
            return this;
        }

        public SqlString ToSqlString()
        {
            return (SqlString) this;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlInt64))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlInt64));
            }
            SqlInt64 num = (SqlInt64) value;
            return this.CompareTo(num);
        }

        public int CompareTo(SqlInt64 value)
        {
            if (this.IsNull)
            {
                if (!value.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (value.IsNull)
            {
                return 1;
            }
            if (SqlBoolean.op_True(this < value))
            {
                return -1;
            }
            if (SqlBoolean.op_True(this > value))
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (!(value is SqlInt64))
            {
                return false;
            }
            SqlInt64 num = (SqlInt64) value;
            if (num.IsNull || this.IsNull)
            {
                return (num.IsNull && this.IsNull);
            }
            SqlBoolean flag = this == num;
            return flag.Value;
        }

        public override int GetHashCode()
        {
            if (!this.IsNull)
            {
                return this.Value.GetHashCode();
            }
            return 0;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            string attribute = reader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
            if ((attribute != null) && XmlConvert.ToBoolean(attribute))
            {
                reader.ReadElementString();
                this.m_fNotNull = false;
            }
            else
            {
                this.m_value = XmlConvert.ToInt64(reader.ReadElementString());
                this.m_fNotNull = true;
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.IsNull)
            {
                writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            }
            else
            {
                writer.WriteString(XmlConvert.ToString(this.m_value));
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("long", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlInt64()
        {
            x_lLowIntMask = 0xffffffffL;
            x_lHighIntMask = -4294967296L;
            Null = new SqlInt64(true);
            Zero = new SqlInt64(0L);
            MinValue = new SqlInt64(-9223372036854775808L);
            MaxValue = new SqlInt64(0x7fffffffffffffffL);
        }
    }
}

