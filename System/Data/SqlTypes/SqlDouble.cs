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
    public struct SqlDouble : INullable, IComparable, IXmlSerializable
    {
        private bool m_fNotNull;
        private double m_value;
        public static readonly SqlDouble Null;
        public static readonly SqlDouble Zero;
        public static readonly SqlDouble MinValue;
        public static readonly SqlDouble MaxValue;
        private SqlDouble(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_value = 0.0;
        }

        public SqlDouble(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
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
        public double Value
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
        public static implicit operator SqlDouble(double x)
        {
            return new SqlDouble(x);
        }

        public static explicit operator double(SqlDouble x)
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

        public static SqlDouble Parse(string s)
        {
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            return new SqlDouble(double.Parse(s, CultureInfo.InvariantCulture));
        }

        public static SqlDouble operator -(SqlDouble x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble(-x.m_value);
            }
            return Null;
        }

        public static SqlDouble operator +(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            double d = x.m_value + y.m_value;
            if (double.IsInfinity(d))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlDouble(d);
        }

        public static SqlDouble operator -(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            double d = x.m_value - y.m_value;
            if (double.IsInfinity(d))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlDouble(d);
        }

        public static SqlDouble operator *(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            double d = x.m_value * y.m_value;
            if (double.IsInfinity(d))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlDouble(d);
        }

        public static SqlDouble operator /(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if (y.m_value == 0.0)
            {
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
            }
            double d = x.m_value / y.m_value;
            if (double.IsInfinity(d))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlDouble(d);
        }

        public static explicit operator SqlDouble(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble((double) x.ByteValue);
            }
            return Null;
        }

        public static implicit operator SqlDouble(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble((double) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDouble(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble((double) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDouble(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble((double) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDouble(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble((double) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDouble(SqlSingle x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble((double) x.Value);
            }
            return Null;
        }

        public static implicit operator SqlDouble(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble(x.ToDouble());
            }
            return Null;
        }

        public static implicit operator SqlDouble(SqlDecimal x)
        {
            if (!x.IsNull)
            {
                return new SqlDouble(x.ToDouble());
            }
            return Null;
        }

        public static explicit operator SqlDouble(SqlString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static SqlBoolean operator ==(SqlDouble x, SqlDouble y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlDouble x, SqlDouble y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlDouble x, SqlDouble y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlDouble x, SqlDouble y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlDouble x, SqlDouble y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlDouble x, SqlDouble y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlDouble Add(SqlDouble x, SqlDouble y)
        {
            return (x + y);
        }

        public static SqlDouble Subtract(SqlDouble x, SqlDouble y)
        {
            return (x - y);
        }

        public static SqlDouble Multiply(SqlDouble x, SqlDouble y)
        {
            return (x * y);
        }

        public static SqlDouble Divide(SqlDouble x, SqlDouble y)
        {
            return (x / y);
        }

        public static SqlBoolean Equals(SqlDouble x, SqlDouble y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlDouble x, SqlDouble y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlDouble x, SqlDouble y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlDouble x, SqlDouble y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlDouble x, SqlDouble y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlDouble x, SqlDouble y)
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

        public SqlSingle ToSqlSingle()
        {
            return (SqlSingle) this;
        }

        public SqlString ToSqlString()
        {
            return (SqlString) this;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlDouble))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlDouble));
            }
            SqlDouble num = (SqlDouble) value;
            return this.CompareTo(num);
        }

        public int CompareTo(SqlDouble value)
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
            if (!(value is SqlDouble))
            {
                return false;
            }
            SqlDouble num = (SqlDouble) value;
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
                this.m_value = XmlConvert.ToDouble(reader.ReadElementString());
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
            return new XmlQualifiedName("double", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlDouble()
        {
            Null = new SqlDouble(true);
            Zero = new SqlDouble(0.0);
            MinValue = new SqlDouble(double.MinValue);
            MaxValue = new SqlDouble(double.MaxValue);
        }
    }
}

