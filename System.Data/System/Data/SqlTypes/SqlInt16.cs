namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlInt16 : INullable, IComparable, IXmlSerializable
    {
        private bool m_fNotNull;
        private short m_value;
        private static readonly int O_MASKI2;
        public static readonly SqlInt16 Null;
        public static readonly SqlInt16 Zero;
        public static readonly SqlInt16 MinValue;
        public static readonly SqlInt16 MaxValue;
        private SqlInt16(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_value = 0;
        }

        public SqlInt16(short value)
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
        public short Value
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
        public static implicit operator SqlInt16(short x)
        {
            return new SqlInt16(x);
        }

        public static explicit operator short(SqlInt16 x)
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

        public static SqlInt16 Parse(string s)
        {
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            return new SqlInt16(short.Parse(s, (IFormatProvider) null));
        }

        public static SqlInt16 operator -(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt16(-x.m_value);
            }
            return Null;
        }

        public static SqlInt16 operator ~(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlInt16(~x.m_value);
            }
            return Null;
        }

        public static SqlInt16 operator +(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num = x.m_value + y.m_value;
            if ((((num >> 15) ^ (num >> 0x10)) & 1) != 0)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) num);
        }

        public static SqlInt16 operator -(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num = x.m_value - y.m_value;
            if ((((num >> 15) ^ (num >> 0x10)) & 1) != 0)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) num);
        }

        public static SqlInt16 operator *(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int num2 = x.m_value * y.m_value;
            int num = num2 & O_MASKI2;
            if ((num != 0) && (num != O_MASKI2))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) num2);
        }

        public static SqlInt16 operator /(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            if ((x.m_value == -32768) && (y.m_value == -1))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) (x.m_value / y.m_value));
        }

        public static SqlInt16 operator %(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            if ((x.m_value == -32768) && (y.m_value == -1))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) (x.m_value % y.m_value));
        }

        public static SqlInt16 operator &(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt16((short) (x.m_value & y.m_value));
            }
            return Null;
        }

        public static SqlInt16 operator |(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt16((short) (((ushort) x.m_value) | ((ushort) y.m_value)));
            }
            return Null;
        }

        public static SqlInt16 operator ^(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlInt16((short) (x.m_value ^ y.m_value));
            }
            return Null;
        }

        public static explicit operator SqlInt16(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlInt16(x.ByteValue);
            }
            return Null;
        }

        public static implicit operator SqlInt16(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlInt16(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlInt16(SqlInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            int num = x.Value;
            if ((num > 0x7fff) || (num < -32768))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) num);
        }

        public static explicit operator SqlInt16(SqlInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            long num = x.Value;
            if ((num > 0x7fffL) || (num < -32768L))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) num);
        }

        public static explicit operator SqlInt16(SqlSingle x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            float num = x.Value;
            if ((num < -32768f) || (num > 32767f))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) num);
        }

        public static explicit operator SqlInt16(SqlDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            double num = x.Value;
            if ((num < -32768.0) || (num > 32767.0))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlInt16((short) num);
        }

        public static explicit operator SqlInt16(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlInt16((short) x.ToInt32());
            }
            return Null;
        }

        public static explicit operator SqlInt16(SqlDecimal x)
        {
            return (SqlInt16) ((SqlInt32) x);
        }

        public static explicit operator SqlInt16(SqlString x)
        {
            if (!x.IsNull)
            {
                return new SqlInt16(short.Parse(x.Value, (IFormatProvider) null));
            }
            return Null;
        }

        public static SqlBoolean operator ==(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlInt16 x, SqlInt16 y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlInt16 x, SqlInt16 y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlInt16 OnesComplement(SqlInt16 x)
        {
            return ~x;
        }

        public static SqlInt16 Add(SqlInt16 x, SqlInt16 y)
        {
            return (x + y);
        }

        public static SqlInt16 Subtract(SqlInt16 x, SqlInt16 y)
        {
            return (x - y);
        }

        public static SqlInt16 Multiply(SqlInt16 x, SqlInt16 y)
        {
            return (x * y);
        }

        public static SqlInt16 Divide(SqlInt16 x, SqlInt16 y)
        {
            return (x / y);
        }

        public static SqlInt16 Mod(SqlInt16 x, SqlInt16 y)
        {
            return (x % y);
        }

        public static SqlInt16 Modulus(SqlInt16 x, SqlInt16 y)
        {
            return (x % y);
        }

        public static SqlInt16 BitwiseAnd(SqlInt16 x, SqlInt16 y)
        {
            return (x & y);
        }

        public static SqlInt16 BitwiseOr(SqlInt16 x, SqlInt16 y)
        {
            return (x | y);
        }

        public static SqlInt16 Xor(SqlInt16 x, SqlInt16 y)
        {
            return (x ^ y);
        }

        public static SqlBoolean Equals(SqlInt16 x, SqlInt16 y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlInt16 x, SqlInt16 y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlInt16 x, SqlInt16 y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlInt16 x, SqlInt16 y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlInt16 x, SqlInt16 y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlInt16 x, SqlInt16 y)
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
            if (!(value is SqlInt16))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlInt16));
            }
            SqlInt16 num = (SqlInt16) value;
            return this.CompareTo(num);
        }

        public int CompareTo(SqlInt16 value)
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
            if (!(value is SqlInt16))
            {
                return false;
            }
            SqlInt16 num = (SqlInt16) value;
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
                this.m_value = XmlConvert.ToInt16(reader.ReadElementString());
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
            return new XmlQualifiedName("short", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlInt16()
        {
            O_MASKI2 = -32768;
            Null = new SqlInt16(true);
            Zero = new SqlInt16(0);
            MinValue = new SqlInt16(-32768);
            MaxValue = new SqlInt16(0x7fff);
        }
    }
}

