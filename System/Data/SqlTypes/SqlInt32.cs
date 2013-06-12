namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlInt32 : INullable, IComparable, IXmlSerializable
    {
        private bool m_fNotNull;
        private int m_value;
        private static readonly long x_iIntMin;
        private static readonly long x_lBitNotIntMax;
        public static readonly SqlInt32 Null;
        public static readonly SqlInt32 Zero;
        public static readonly SqlInt32 MinValue;
        public static readonly SqlInt32 MaxValue;
        private SqlInt32(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_value = 0;
        }

        public SqlInt32(int value)
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
        public int Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_value;
            }
        }
        public static implicit operator SqlInt32(int x)
        {
            return new SqlInt32(x);
        }

        public static explicit operator int(SqlInt32 x)
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

        public static SqlInt32 Parse(string s)
        {
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            return new SqlInt32(int.Parse(s, (IFormatProvider) null));
        }

        public static SqlInt32 operator -(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt32(-x.m_value);
            }
            return Null;
        }

        public static SqlInt32 operator ~(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt32(~x.m_value);
            }
            return Null;
        }

        public static SqlInt32 operator +(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num = x.m_value + y.m_value;
            if (SameSignInt(x.m_value, y.m_value) && !SameSignInt(x.m_value, num))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32(num);
        }

        public static SqlInt32 operator -(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num = x.m_value - y.m_value;
            if (!SameSignInt(x.m_value, y.m_value) && SameSignInt(y.m_value, num))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32(num);
        }

        public static SqlInt32 operator *(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long num2 = x.m_value * y.m_value;
            long num = num2 & x_lBitNotIntMax;
            if ((num != 0L) && (num != x_lBitNotIntMax))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32((int) num2);
        }

        public static SqlInt32 operator /(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            if ((x.m_value == x_iIntMin) && (y.m_value == -1))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32(x.m_value / y.m_value);
        }

        public static SqlInt32 operator %(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            if ((x.m_value == x_iIntMin) && (y.m_value == -1))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32(x.m_value % y.m_value);
        }

        public static SqlInt32 operator &(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt32(x.m_value & y.m_value);
            }
            return Null;
        }

        public static SqlInt32 operator |(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt32(x.m_value | y.m_value);
            }
            return Null;
        }

        public static SqlInt32 operator ^(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt32(x.m_value ^ y.m_value);
            }
            return Null;
        }

        public static explicit operator SqlInt32(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlInt32(x.ByteValue);
            }
            return Null;
        }

        public static implicit operator SqlInt32(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlInt32(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlInt32(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt32(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlInt32(SqlInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            long num = x.Value;
            if ((num > 0x7fffffffL) || (num < -2147483648L))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32((int) num);
        }

        public static explicit operator SqlInt32(SqlSingle x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            float num = x.Value;
            if ((num > 2.147484E+09f) || (num < -2.147484E+09f))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32((int) num);
        }

        public static explicit operator SqlInt32(SqlDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            double num = x.Value;
            if ((num > 2147483647.0) || (num < -2147483648.0))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt32((int) num);
        }

        public static explicit operator SqlInt32(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlInt32(x.ToInt32());
            }
            return Null;
        }

        public static explicit operator SqlInt32(SqlDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            x.AdjustScale(-x.Scale, true);
            long num = x.m_data1;
            if (!x.IsPositive)
            {
                num = -num;
            }
            if (((x.m_bLen > 1) || (num > 0x7fffffffL)) || (num < -2147483648L))
            {
                throw new OverflowException(SQLResource.ConversionOverflowMessage);
            }
            return new SqlInt32((int) num);
        }

        public static explicit operator SqlInt32(SqlString x)
        {
            if (!x.IsNull)
            {
                return new SqlInt32(int.Parse(x.Value, (IFormatProvider) null));
            }
            return Null;
        }

        private static bool SameSignInt(int x, int y)
        {
            return (((x ^ y) & 0x80000000L) == 0L);
        }

        public static SqlBoolean operator ==(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlInt32 x, SqlInt32 y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlInt32 x, SqlInt32 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlInt32 OnesComplement(SqlInt32 x)
        {
            return ~x;
        }

        public static SqlInt32 Add(SqlInt32 x, SqlInt32 y)
        {
            return (x + y);
        }

        public static SqlInt32 Subtract(SqlInt32 x, SqlInt32 y)
        {
            return (x - y);
        }

        public static SqlInt32 Multiply(SqlInt32 x, SqlInt32 y)
        {
            return (x * y);
        }

        public static SqlInt32 Divide(SqlInt32 x, SqlInt32 y)
        {
            return (x / y);
        }

        public static SqlInt32 Mod(SqlInt32 x, SqlInt32 y)
        {
            return (x % y);
        }

        public static SqlInt32 Modulus(SqlInt32 x, SqlInt32 y)
        {
            return (x % y);
        }

        public static SqlInt32 BitwiseAnd(SqlInt32 x, SqlInt32 y)
        {
            return (x & y);
        }

        public static SqlInt32 BitwiseOr(SqlInt32 x, SqlInt32 y)
        {
            return (x | y);
        }

        public static SqlInt32 Xor(SqlInt32 x, SqlInt32 y)
        {
            return (x ^ y);
        }

        public static SqlBoolean Equals(SqlInt32 x, SqlInt32 y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlInt32 x, SqlInt32 y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlInt32 x, SqlInt32 y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlInt32 x, SqlInt32 y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlInt32 x, SqlInt32 y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlInt32 x, SqlInt32 y)
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

        public SqlInt64 ToSqlInt64()
        {
            return this;
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
            if (!(value is SqlInt32))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlInt32));
            }
            SqlInt32 num = (SqlInt32) value;
            return this.CompareTo(num);
        }

        public int CompareTo(SqlInt32 value)
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
            if (!(value is SqlInt32))
            {
                return false;
            }
            SqlInt32 num = (SqlInt32) value;
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
                this.m_value = XmlConvert.ToInt32(reader.ReadElementString());
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
            return new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlInt32()
        {
            x_iIntMin = -2147483648L;
            x_lBitNotIntMax = -2147483648L;
            Null = new SqlInt32(true);
            Zero = new SqlInt32(0);
            MinValue = new SqlInt32(-2147483648);
            MaxValue = new SqlInt32(0x7fffffff);
        }
    }
}

