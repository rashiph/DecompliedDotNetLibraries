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
    public struct SqlMoney : INullable, IComparable, IXmlSerializable
    {
        private bool m_fNotNull;
        private long m_value;
        internal static readonly int x_iMoneyScale;
        private static readonly long x_lTickBase;
        private static readonly double x_dTickBase;
        private static readonly long MinLong;
        private static readonly long MaxLong;
        public static readonly SqlMoney Null;
        public static readonly SqlMoney Zero;
        public static readonly SqlMoney MinValue;
        public static readonly SqlMoney MaxValue;
        private SqlMoney(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_value = 0L;
        }

        internal SqlMoney(long value, int ignored)
        {
            this.m_value = value;
            this.m_fNotNull = true;
        }

        public SqlMoney(int value)
        {
            this.m_value = value * x_lTickBase;
            this.m_fNotNull = true;
        }

        public SqlMoney(long value)
        {
            if ((value < MinLong) || (value > MaxLong))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            this.m_value = value * x_lTickBase;
            this.m_fNotNull = true;
        }

        public SqlMoney(decimal value)
        {
            SqlDecimal num2 = new SqlDecimal(value);
            num2.AdjustScale(x_iMoneyScale - num2.Scale, true);
            if ((num2.m_data3 != 0) || (num2.m_data4 != 0))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            bool isPositive = num2.IsPositive;
            ulong num = num2.m_data1 + (num2.m_data2 << 0x20);
            if ((isPositive && (num > 0x7fffffffffffffffL)) || (!isPositive && (num > 9223372036854775808L)))
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            this.m_value = isPositive ? ((long) num) : ((long) -num);
            this.m_fNotNull = true;
        }

        public SqlMoney(double value) : this(new decimal(value))
        {
        }

        public bool IsNull
        {
            get
            {
                return !this.m_fNotNull;
            }
        }
        public decimal Value
        {
            get
            {
                if (!this.m_fNotNull)
                {
                    throw new SqlNullValueException();
                }
                return this.ToDecimal();
            }
        }
        public decimal ToDecimal()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            bool isNegative = false;
            long num = this.m_value;
            if (this.m_value < 0L)
            {
                isNegative = true;
                num = -this.m_value;
            }
            return new decimal((int) num, (int) (num >> 0x20), 0, isNegative, (byte) x_iMoneyScale);
        }

        public long ToInt64()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            long num = this.m_value / (x_lTickBase / 10L);
            bool flag = num >= 0L;
            long num2 = num % 10L;
            num /= 10L;
            if (num2 < 5L)
            {
                return num;
            }
            if (flag)
            {
                return (num + 1L);
            }
            return (num - 1L);
        }

        internal long ToSqlInternalRepresentation()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            return this.m_value;
        }

        public int ToInt32()
        {
            return (int) this.ToInt64();
        }

        public double ToDouble()
        {
            return decimal.ToDouble(this.ToDecimal());
        }

        public static implicit operator SqlMoney(decimal x)
        {
            return new SqlMoney(x);
        }

        public static explicit operator SqlMoney(double x)
        {
            return new SqlMoney(x);
        }

        public static implicit operator SqlMoney(long x)
        {
            return new SqlMoney(new decimal(x));
        }

        public static explicit operator decimal(SqlMoney x)
        {
            return x.Value;
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return SQLResource.NullString;
            }
            return this.ToDecimal().ToString("#0.00##", null);
        }

        public static SqlMoney Parse(string s)
        {
            decimal num;
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            if (decimal.TryParse(s, NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint | NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num))
            {
                return new SqlMoney(num);
            }
            return new SqlMoney(decimal.Parse(s, NumberStyles.Currency, NumberFormatInfo.CurrentInfo));
        }

        public static SqlMoney operator -(SqlMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.m_value == MinLong)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return new SqlMoney(-x.m_value, 0);
        }

        public static SqlMoney operator +(SqlMoney x, SqlMoney y)
        {
            SqlMoney money;
            try
            {
                money = (x.IsNull || y.IsNull) ? Null : new SqlMoney(x.m_value + y.m_value, 0);
            }
            catch (OverflowException)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return money;
        }

        public static SqlMoney operator -(SqlMoney x, SqlMoney y)
        {
            SqlMoney money;
            try
            {
                money = (x.IsNull || y.IsNull) ? Null : new SqlMoney(x.m_value - y.m_value, 0);
            }
            catch (OverflowException)
            {
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            return money;
        }

        public static SqlMoney operator *(SqlMoney x, SqlMoney y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlMoney(decimal.Multiply(x.ToDecimal(), y.ToDecimal()));
            }
            return Null;
        }

        public static SqlMoney operator /(SqlMoney x, SqlMoney y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlMoney(decimal.Divide(x.ToDecimal(), y.ToDecimal()));
            }
            return Null;
        }

        public static explicit operator SqlMoney(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(x.ByteValue);
            }
            return Null;
        }

        public static implicit operator SqlMoney(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlMoney(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlMoney(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(x.Value);
            }
            return Null;
        }

        public static implicit operator SqlMoney(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlMoney(SqlSingle x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney((double) x.Value);
            }
            return Null;
        }

        public static explicit operator SqlMoney(SqlDouble x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlMoney(SqlDecimal x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlMoney(SqlString x)
        {
            if (!x.IsNull)
            {
                return new SqlMoney(decimal.Parse(x.Value, NumberStyles.Currency, null));
            }
            return Null;
        }

        public static SqlBoolean operator ==(SqlMoney x, SqlMoney y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value == y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlMoney x, SqlMoney y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlMoney x, SqlMoney y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value < y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlMoney x, SqlMoney y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value > y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlMoney x, SqlMoney y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value <= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlMoney x, SqlMoney y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(x.m_value >= y.m_value);
            }
            return SqlBoolean.Null;
        }

        public static SqlMoney Add(SqlMoney x, SqlMoney y)
        {
            return (x + y);
        }

        public static SqlMoney Subtract(SqlMoney x, SqlMoney y)
        {
            return (x - y);
        }

        public static SqlMoney Multiply(SqlMoney x, SqlMoney y)
        {
            return (x * y);
        }

        public static SqlMoney Divide(SqlMoney x, SqlMoney y)
        {
            return (x / y);
        }

        public static SqlBoolean Equals(SqlMoney x, SqlMoney y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlMoney x, SqlMoney y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlMoney x, SqlMoney y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlMoney x, SqlMoney y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlMoney x, SqlMoney y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlMoney x, SqlMoney y)
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
            if (!(value is SqlMoney))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlMoney));
            }
            SqlMoney money = (SqlMoney) value;
            return this.CompareTo(money);
        }

        public int CompareTo(SqlMoney value)
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
            if (!(value is SqlMoney))
            {
                return false;
            }
            SqlMoney money = (SqlMoney) value;
            if (money.IsNull || this.IsNull)
            {
                return (money.IsNull && this.IsNull);
            }
            SqlBoolean flag = this == money;
            return flag.Value;
        }

        public override int GetHashCode()
        {
            if (!this.IsNull)
            {
                return this.m_value.GetHashCode();
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
                SqlMoney money = new SqlMoney(XmlConvert.ToDecimal(reader.ReadElementString()));
                this.m_fNotNull = money.m_fNotNull;
                this.m_value = money.m_value;
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
                writer.WriteString(XmlConvert.ToString(this.ToDecimal()));
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("decimal", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlMoney()
        {
            x_iMoneyScale = 4;
            x_lTickBase = 0x2710L;
            x_dTickBase = x_lTickBase;
            MinLong = -9223372036854775808L / x_lTickBase;
            MaxLong = 0x7fffffffffffffffL / x_lTickBase;
            Null = new SqlMoney(true);
            Zero = new SqlMoney(0);
            MinValue = new SqlMoney(-9223372036854775808L, 0);
            MaxValue = new SqlMoney(0x7fffffffffffffffL, 0);
        }
    }
}

