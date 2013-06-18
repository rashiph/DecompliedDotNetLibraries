namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlBinary : INullable, IComparable, IXmlSerializable
    {
        private byte[] m_value;
        public static readonly SqlBinary Null;
        private SqlBinary(bool fNull)
        {
            this.m_value = null;
        }

        public SqlBinary(byte[] value)
        {
            if (value == null)
            {
                this.m_value = null;
            }
            else
            {
                this.m_value = new byte[value.Length];
                value.CopyTo(this.m_value, 0);
            }
        }

        internal SqlBinary(byte[] value, bool ignored)
        {
            if (value == null)
            {
                this.m_value = null;
            }
            else
            {
                this.m_value = value;
            }
        }

        public bool IsNull
        {
            get
            {
                return (this.m_value == null);
            }
        }
        public byte[] Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                byte[] array = new byte[this.m_value.Length];
                this.m_value.CopyTo(array, 0);
                return array;
            }
        }
        public byte this[int index]
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_value[index];
            }
        }
        public int Length
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_value.Length;
            }
        }
        public static implicit operator SqlBinary(byte[] x)
        {
            return new SqlBinary(x);
        }

        public static explicit operator byte[](SqlBinary x)
        {
            return x.Value;
        }

        public override string ToString()
        {
            if (!this.IsNull)
            {
                int length = this.m_value.Length;
                return ("SqlBinary(" + length.ToString(CultureInfo.InvariantCulture) + ")");
            }
            return SQLResource.NullString;
        }

        public static SqlBinary operator +(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            byte[] array = new byte[x.Value.Length + y.Value.Length];
            x.Value.CopyTo(array, 0);
            y.Value.CopyTo(array, x.Value.Length);
            return new SqlBinary(array);
        }

        private static EComparison PerformCompareByte(byte[] x, byte[] y)
        {
            int num;
            int num2 = (x.Length < y.Length) ? x.Length : y.Length;
            for (num = 0; num < num2; num++)
            {
                if (x[num] != y[num])
                {
                    if (x[num] < y[num])
                    {
                        return EComparison.LT;
                    }
                    return EComparison.GT;
                }
            }
            if (x.Length != y.Length)
            {
                byte num3 = 0;
                if (x.Length < y.Length)
                {
                    for (num = num2; num < y.Length; num++)
                    {
                        if (y[num] != num3)
                        {
                            return EComparison.LT;
                        }
                    }
                }
                else
                {
                    for (num = num2; num < x.Length; num++)
                    {
                        if (x[num] != num3)
                        {
                            return EComparison.GT;
                        }
                    }
                }
            }
            return EComparison.EQ;
        }

        public static explicit operator SqlBinary(SqlGuid x)
        {
            if (!x.IsNull)
            {
                return new SqlBinary(x.ToByteArray());
            }
            return Null;
        }

        public static SqlBoolean operator ==(SqlBinary x, SqlBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(PerformCompareByte(x.Value, y.Value) == EComparison.EQ);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlBinary x, SqlBinary y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlBinary x, SqlBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(PerformCompareByte(x.Value, y.Value) == EComparison.LT);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlBinary x, SqlBinary y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(PerformCompareByte(x.Value, y.Value) == EComparison.GT);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
            {
                return SqlBoolean.Null;
            }
            EComparison comparison = PerformCompareByte(x.Value, y.Value);
            return new SqlBoolean((comparison == EComparison.LT) || (comparison == EComparison.EQ));
        }

        public static SqlBoolean operator >=(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
            {
                return SqlBoolean.Null;
            }
            EComparison comparison = PerformCompareByte(x.Value, y.Value);
            return new SqlBoolean((comparison == EComparison.GT) || (comparison == EComparison.EQ));
        }

        public static SqlBinary Add(SqlBinary x, SqlBinary y)
        {
            return (x + y);
        }

        public static SqlBinary Concat(SqlBinary x, SqlBinary y)
        {
            return (x + y);
        }

        public static SqlBoolean Equals(SqlBinary x, SqlBinary y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlBinary x, SqlBinary y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlBinary x, SqlBinary y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlBinary x, SqlBinary y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlBinary x, SqlBinary y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlBinary x, SqlBinary y)
        {
            return (x >= y);
        }

        public SqlGuid ToSqlGuid()
        {
            return (SqlGuid) this;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlBinary))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlBinary));
            }
            SqlBinary binary = (SqlBinary) value;
            return this.CompareTo(binary);
        }

        public int CompareTo(SqlBinary value)
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
            if (!(value is SqlBinary))
            {
                return false;
            }
            SqlBinary binary = (SqlBinary) value;
            if (binary.IsNull || this.IsNull)
            {
                return (binary.IsNull && this.IsNull);
            }
            SqlBoolean flag = this == binary;
            return flag.Value;
        }

        internal static int HashByteArray(byte[] rgbValue, int length)
        {
            if (length <= 0)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < length; i++)
            {
                int num3 = (num >> 0x1c) & 0xff;
                num = num << 4;
                num = (num ^ rgbValue[i]) ^ num3;
            }
            return num;
        }

        public override int GetHashCode()
        {
            if (this.IsNull)
            {
                return 0;
            }
            int length = this.m_value.Length;
            while ((length > 0) && (this.m_value[length - 1] == 0))
            {
                length--;
            }
            return HashByteArray(this.m_value, length);
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
                this.m_value = null;
            }
            else
            {
                string s = reader.ReadElementString();
                if (s == null)
                {
                    this.m_value = new byte[0];
                }
                else
                {
                    s = s.Trim();
                    if (s.Length == 0)
                    {
                        this.m_value = new byte[0];
                    }
                    else
                    {
                        this.m_value = Convert.FromBase64String(s);
                    }
                }
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
                writer.WriteString(Convert.ToBase64String(this.m_value));
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("base64Binary", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlBinary()
        {
            Null = new SqlBinary(true);
        }
    }
}

