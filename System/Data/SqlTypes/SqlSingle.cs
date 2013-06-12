namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlSingle : INullable, IComparable, IXmlSerializable
    {
        private bool m_fNotNull;
        private float m_value;
        public static readonly SqlSingle Null;
        public static readonly SqlSingle Zero;
        public static readonly SqlSingle MinValue;
        public static readonly SqlSingle MaxValue;
        private SqlSingle(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_value = 0f;
        }

        public SqlSingle(float value)
        {
            if (float.IsInfinity(value) || float.IsNaN(value))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            this.m_fNotNull = true;
            this.m_value = value;
        }

        public SqlSingle(double value) : this((float) value)
        {
        }

        public bool IsNull
        {
            get
            {
                return !this.m_fNotNull;
            }
        }
        public float Value
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
        public static implicit operator SqlSingle(float x)
        {
            return new SqlSingle(x);
        }

        public static explicit operator float(SqlSingle x)
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

        public static SqlSingle Parse(string s)
        {
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            return new SqlSingle(float.Parse(s, CultureInfo.InvariantCulture));
        }

        public static SqlSingle operator -(SqlSingle x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle(-x.m_value);
            }
            return Null;
        }

        public static SqlSingle operator +(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            float f = x.m_value + y.m_value;
            if (float.IsInfinity(f))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlSingle(f);
        }

        public static SqlSingle operator -(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            float f = x.m_value - y.m_value;
            if (float.IsInfinity(f))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlSingle(f);
        }

        public static SqlSingle operator *(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            float f = x.m_value * y.m_value;
            if (float.IsInfinity(f))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlSingle(f);
        }

        public static SqlSingle operator /(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0f)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            float f = x.m_value / y.m_value;
            if (float.IsInfinity(f))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlSingle(f);
        }

        public static explicit operator SqlSingle(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle((float) x.ByteValue);
            }
            return Null;
        }

        public static implicit operator SqlSingle(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle((float) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlSingle(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle((float) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlSingle(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle((float) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlSingle(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle((float) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlSingle(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle(x.ToDouble());
            }
            return Null;
        }

        public static implicit operator SqlSingle(SqlDecimal x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle(x.ToDouble());
            }
            return Null;
        }

        public static explicit operator SqlSingle(SqlDouble x)
        {
            if (!x.IsNull)
            {
                return new SqlSingle(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlSingle(SqlString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static SqlBoolean operator ==(SqlSingle x, SqlSingle y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlSingle x, SqlSingle y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlSingle x, SqlSingle y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlSingle x, SqlSingle y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlSingle x, SqlSingle y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlSingle x, SqlSingle y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlSingle Add(SqlSingle x, SqlSingle y)
        {
            return (x + y);
        }

        public static SqlSingle Subtract(SqlSingle x, SqlSingle y)
        {
            return (x - y);
        }

        public static SqlSingle Multiply(SqlSingle x, SqlSingle y)
        {
            return (x * y);
        }

        public static SqlSingle Divide(SqlSingle x, SqlSingle y)
        {
            return (x / y);
        }

        public static SqlBoolean Equals(SqlSingle x, SqlSingle y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlSingle x, SqlSingle y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlSingle x, SqlSingle y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlSingle x, SqlSingle y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlSingle x, SqlSingle y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlSingle x, SqlSingle y)
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

        public SqlInt64 ToSqlInt64()
        {
            return (SqlInt64) this;
        }

        public SqlMoney ToSqlMoney()
        {
            return (SqlMoney) this;
        }

        public SqlDecimal ToSqlDecimal()
        {
            return (SqlDecimal) this;
        }

        public SqlString ToSqlString()
        {
            return (SqlString) this;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlSingle))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlSingle));
            }
            SqlSingle num = (SqlSingle) value;
            return this.CompareTo(num);
        }

        public int CompareTo(SqlSingle value)
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
            if (!(value is SqlSingle))
            {
                return false;
            }
            SqlSingle num = (SqlSingle) value;
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
                this.m_value = XmlConvert.ToSingle(reader.ReadElementString());
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
            return new XmlQualifiedName("float", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlSingle()
        {
            Null = new SqlSingle(true);
            Zero = new SqlSingle(0f);
            MinValue = new SqlSingle(float.MinValue);
            MaxValue = new SqlSingle(float.MaxValue);
        }
    }
}

