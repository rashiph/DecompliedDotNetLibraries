namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlBoolean : INullable, IComparable, IXmlSerializable
    {
        private const byte x_Null = 0;
        private const byte x_False = 1;
        private const byte x_True = 2;
        private byte m_value;
        public static readonly SqlBoolean True;
        public static readonly SqlBoolean False;
        public static readonly SqlBoolean Null;
        public static readonly SqlBoolean Zero;
        public static readonly SqlBoolean One;
        public SqlBoolean(bool value)
        {
            this.m_value = value ? ((byte) 2) : ((byte) 1);
        }

        public SqlBoolean(int value) : this(value, false)
        {
        }

        private SqlBoolean(int value, bool fNull)
        {
            if (fNull)
            {
                this.m_value = 0;
            }
            else
            {
                this.m_value = (value != 0) ? ((byte) 2) : ((byte) 1);
            }
        }

        public bool IsNull
        {
            get
            {
                return (this.m_value == 0);
            }
        }
        public bool Value
        {
            get
            {
                switch (this.m_value)
                {
                    case 1:
                        return false;

                    case 2:
                        return true;
                }
                throw new SqlNullValueException();
            }
        }
        public bool IsTrue
        {
            get
            {
                return (this.m_value == 2);
            }
        }
        public bool IsFalse
        {
            get
            {
                return (this.m_value == 1);
            }
        }
        public static implicit operator SqlBoolean(bool x)
        {
            return new SqlBoolean(x);
        }

        public static explicit operator bool(SqlBoolean x)
        {
            return x.Value;
        }

        public static SqlBoolean op_LogicalNot(SqlBoolean x)
        {
            switch (x.m_value)
            {
                case 1:
                    return True;

                case 2:
                    return False;
            }
            return Null;
        }

        public static bool operator true(SqlBoolean x)
        {
            return x.IsTrue;
        }

        public static bool operator false(SqlBoolean x)
        {
            return x.IsFalse;
        }

        public static SqlBoolean operator &(SqlBoolean x, SqlBoolean y)
        {
            if ((x.m_value == 1) || (y.m_value == 1))
            {
                return False;
            }
            if ((x.m_value == 2) && (y.m_value == 2))
            {
                return True;
            }
            return Null;
        }

        public static SqlBoolean operator |(SqlBoolean x, SqlBoolean y)
        {
            if ((x.m_value == 2) || (y.m_value == 2))
            {
                return True;
            }
            if ((x.m_value == 1) && (y.m_value == 1))
            {
                return False;
            }
            return Null;
        }

        public byte ByteValue
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                if (this.m_value != 2)
                {
                    return 0;
                }
                return 1;
            }
        }
        public override string ToString()
        {
            if (!this.IsNull)
            {
                return this.Value.ToString(null);
            }
            return SQLResource.NullString;
        }

        public static SqlBoolean Parse(string s)
        {
            if (s == null)
            {
                return new SqlBoolean(bool.Parse(s));
            }
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            s = s.TrimStart(new char[0]);
            char c = s[0];
            if ((!char.IsNumber(c) && ('-' != c)) && ('+' != c))
            {
                return new SqlBoolean(bool.Parse(s));
            }
            return new SqlBoolean(int.Parse(s, (IFormatProvider) null));
        }

        public static SqlBoolean operator ~(SqlBoolean x)
        {
            return !x;
        }

        public static SqlBoolean operator ^(SqlBoolean x, SqlBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value != y.m_value);
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlBoolean(x.Value != 0);
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlBoolean(x.Value != 0);
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlBoolean(x.Value != 0);
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlBoolean(x.Value != 0L);
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlDouble x)
        {
            if (!x.IsNull)
            {
                return new SqlBoolean(!(x.Value == 0.0));
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlSingle x)
        {
            if (!x.IsNull)
            {
                return new SqlBoolean(x.Value != 0.0);
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return (x != SqlMoney.Zero);
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlDecimal x)
        {
            if (!x.IsNull)
            {
                return new SqlBoolean((((x.m_data1 != 0) || (x.m_data2 != 0)) || (x.m_data3 != 0)) || (x.m_data4 != 0));
            }
            return Null;
        }

        public static explicit operator SqlBoolean(SqlString x)
        {
            if (!x.IsNull)
            {
                return Parse(x.Value);
            }
            return Null;
        }

        public static SqlBoolean operator ==(SqlBoolean x, SqlBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return Null;
        }

        public static SqlBoolean operator !=(SqlBoolean x, SqlBoolean y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlBoolean x, SqlBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return Null;
        }

        public static SqlBoolean operator >(SqlBoolean x, SqlBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return Null;
        }

        public static SqlBoolean operator <=(SqlBoolean x, SqlBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return Null;
        }

        public static SqlBoolean operator >=(SqlBoolean x, SqlBoolean y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return Null;
        }

        public static SqlBoolean OnesComplement(SqlBoolean x)
        {
            return ~x;
        }

        public static SqlBoolean And(SqlBoolean x, SqlBoolean y)
        {
            return (x & y);
        }

        public static SqlBoolean Or(SqlBoolean x, SqlBoolean y)
        {
            return (x | y);
        }

        public static SqlBoolean Xor(SqlBoolean x, SqlBoolean y)
        {
            return (x ^ y);
        }

        public static SqlBoolean Equals(SqlBoolean x, SqlBoolean y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlBoolean x, SqlBoolean y)
        {
            return (x != y);
        }

        public static SqlBoolean GreaterThan(SqlBoolean x, SqlBoolean y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThan(SqlBoolean x, SqlBoolean y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThanOrEquals(SqlBoolean x, SqlBoolean y)
        {
            return (x >= y);
        }

        public static SqlBoolean LessThanOrEquals(SqlBoolean x, SqlBoolean y)
        {
            return (x <= y);
        }

        public SqlByte ToSqlByte()
        {
            return (SqlByte) this;
        }

        public SqlDouble ToSqlDouble()
        {
            return (SqlDouble) this;
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
            if (!(value is SqlBoolean))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlBoolean));
            }
            SqlBoolean flag = (SqlBoolean) value;
            return this.CompareTo(flag);
        }

        public int CompareTo(SqlBoolean value)
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
            if (this.ByteValue < value.ByteValue)
            {
                return -1;
            }
            if (this.ByteValue > value.ByteValue)
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (!(value is SqlBoolean))
            {
                return false;
            }
            SqlBoolean flag = (SqlBoolean) value;
            if (flag.IsNull || this.IsNull)
            {
                return (flag.IsNull && this.IsNull);
            }
            SqlBoolean flag2 = this == flag;
            return flag2.Value;
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
                this.m_value = 0;
            }
            else
            {
                this.m_value = XmlConvert.ToBoolean(reader.ReadElementString()) ? ((byte) 2) : ((byte) 1);
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
                writer.WriteString((this.m_value == 2) ? "true" : "false");
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("boolean", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlBoolean()
        {
            True = new SqlBoolean(true);
            False = new SqlBoolean(false);
            Null = new SqlBoolean(0, true);
            Zero = new SqlBoolean(0);
            One = new SqlBoolean(1);
        }
    }
}

