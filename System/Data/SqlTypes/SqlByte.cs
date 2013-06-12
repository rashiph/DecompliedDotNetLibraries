namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlByte : INullable, IComparable, IXmlSerializable
    {
        private bool m_fNotNull;
        private byte m_value;
        private static readonly int x_iBitNotByteMax;
        public static readonly SqlByte Null;
        public static readonly SqlByte Zero;
        public static readonly SqlByte MinValue;
        public static readonly SqlByte MaxValue;
        private SqlByte(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_value = 0;
        }

        public SqlByte(byte value)
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
        public byte Value
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
        public static implicit operator SqlByte(byte x)
        {
            return new SqlByte(x);
        }

        public static explicit operator byte(SqlByte x)
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

        public static SqlByte Parse(string s)
        {
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            return new SqlByte(byte.Parse(s, (IFormatProvider) null));
        }

        public static SqlByte operator ~(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlByte(~x.m_value);
            }
            return Null;
        }

        public static SqlByte operator +(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num = x.m_value + y.m_value;
            if ((num & x_iBitNotByteMax) != 0)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlByte((byte) num);
        }

        public static SqlByte operator -(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num = x.m_value - y.m_value;
            if ((num & x_iBitNotByteMax) != 0)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlByte((byte) num);
        }

        public static SqlByte operator *(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num = x.m_value * y.m_value;
            if ((num & x_iBitNotByteMax) != 0)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlByte((byte) num);
        }

        public static SqlByte operator /(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            return new SqlByte((byte) (x.m_value / y.m_value));
        }

        public static SqlByte operator %(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            return new SqlByte((byte) (x.m_value % y.m_value));
        }

        public static SqlByte operator &(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlByte((byte) (x.m_value & y.m_value));
            }
            return Null;
        }

        public static SqlByte operator |(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlByte((byte) (x.m_value | y.m_value));
            }
            return Null;
        }

        public static SqlByte operator ^(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlByte((byte) (x.m_value ^ y.m_value));
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlByte(x.ByteValue);
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlByte((byte) x.ToInt32());
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                if ((x.Value > 0xff) || (x.Value < 0))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                if (!x.IsNull)
                {
                    return new SqlByte((byte) x.Value);
                }
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                if ((x.Value > 0xff) || (x.Value < 0))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                if (!x.IsNull)
                {
                    return new SqlByte((byte) x.Value);
                }
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                if ((x.Value > 0xffL) || (x.Value < 0L))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                if (!x.IsNull)
                {
                    return new SqlByte((byte) x.Value);
                }
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlSingle x)
        {
            if (!x.IsNull)
            {
                if ((x.Value > 255f) || (x.Value < 0f))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                if (!x.IsNull)
                {
                    return new SqlByte((byte) x.Value);
                }
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlDouble x)
        {
            if (!x.IsNull)
            {
                if ((x.Value > 255.0) || (x.Value < 0.0))
                {
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
                }
                if (!x.IsNull)
                {
                    return new SqlByte((byte) x.Value);
                }
            }
            return Null;
        }

        public static explicit operator SqlByte(SqlDecimal x)
        {
            return (SqlByte) ((SqlInt32) x);
        }

        public static explicit operator SqlByte(SqlString x)
        {
            if (!x.IsNull)
            {
                return new SqlByte(byte.Parse(x.Value, (IFormatProvider) null));
            }
            return Null;
        }

        public static SqlBoolean operator ==(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlByte x, SqlByte y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlByte x, SqlByte y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlByte OnesComplement(SqlByte x)
        {
            return ~x;
        }

        public static SqlByte Add(SqlByte x, SqlByte y)
        {
            return (x + y);
        }

        public static SqlByte Subtract(SqlByte x, SqlByte y)
        {
            return (x - y);
        }

        public static SqlByte Multiply(SqlByte x, SqlByte y)
        {
            return (x * y);
        }

        public static SqlByte Divide(SqlByte x, SqlByte y)
        {
            return (x / y);
        }

        public static SqlByte Mod(SqlByte x, SqlByte y)
        {
            return (x % y);
        }

        public static SqlByte Modulus(SqlByte x, SqlByte y)
        {
            return (x % y);
        }

        public static SqlByte BitwiseAnd(SqlByte x, SqlByte y)
        {
            return (x & y);
        }

        public static SqlByte BitwiseOr(SqlByte x, SqlByte y)
        {
            return (x | y);
        }

        public static SqlByte Xor(SqlByte x, SqlByte y)
        {
            return (x ^ y);
        }

        public static SqlBoolean Equals(SqlByte x, SqlByte y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlByte x, SqlByte y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlByte x, SqlByte y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlByte x, SqlByte y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlByte x, SqlByte y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlByte x, SqlByte y)
        {
            return (x >= y);
        }

        public SqlBoolean ToSqlBoolean()
        {
            return (SqlBoolean) this;
        }

        public SqlDouble ToSqlDouble()
        {
            return this;
        }

        public SqlInt16 ToSqlInt16()
        {
            return this;
        }

        public SqlInt32 ToSqlInt32()
        {
            return this;
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
            if (!(value is SqlByte))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlByte));
            }
            SqlByte num = (SqlByte) value;
            return this.CompareTo(num);
        }

        public int CompareTo(SqlByte value)
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
            if (!(value is SqlByte))
            {
                return false;
            }
            SqlByte num = (SqlByte) value;
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
                this.m_value = XmlConvert.ToByte(reader.ReadElementString());
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
            return new XmlQualifiedName("unsignedByte", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlByte()
        {
            x_iBitNotByteMax = -256;
            Null = new SqlByte(true);
            Zero = new SqlByte(0);
            MinValue = new SqlByte(0);
            MaxValue = new SqlByte(0xff);
        }
    }
}

