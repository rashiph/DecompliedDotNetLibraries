namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlGuid : INullable, IComparable, IXmlSerializable
    {
        private static readonly int SizeOfGuid;
        private static readonly int[] x_rgiGuidOrder;
        private byte[] m_value;
        public static readonly SqlGuid Null;
        private SqlGuid(bool fNull)
        {
            this.m_value = null;
        }

        public SqlGuid(byte[] value)
        {
            if ((value == null) || (value.Length != SizeOfGuid))
            {
                throw new ArgumentException(SQLResource.InvalidArraySizeMessage);
            }
            this.m_value = new byte[SizeOfGuid];
            value.CopyTo(this.m_value, 0);
        }

        internal SqlGuid(byte[] value, bool ignored)
        {
            if ((value == null) || (value.Length != SizeOfGuid))
            {
                throw new ArgumentException(SQLResource.InvalidArraySizeMessage);
            }
            this.m_value = value;
        }

        public SqlGuid(string s)
        {
            this.m_value = new Guid(s).ToByteArray();
        }

        public SqlGuid(Guid g)
        {
            this.m_value = g.ToByteArray();
        }

        public SqlGuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) : this(new Guid(a, b, c, d, e, f, g, h, i, j, k))
        {
        }

        public bool IsNull
        {
            get
            {
                return (this.m_value == null);
            }
        }
        public Guid Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return new Guid(this.m_value);
            }
        }
        public static implicit operator SqlGuid(Guid x)
        {
            return new SqlGuid(x);
        }

        public static explicit operator Guid(SqlGuid x)
        {
            return x.Value;
        }

        public byte[] ToByteArray()
        {
            byte[] array = new byte[SizeOfGuid];
            this.m_value.CopyTo(array, 0);
            return array;
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return SQLResource.NullString;
            }
            Guid guid = new Guid(this.m_value);
            return guid.ToString();
        }

        public static SqlGuid Parse(string s)
        {
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            return new SqlGuid(s);
        }

        private static EComparison Compare(SqlGuid x, SqlGuid y)
        {
            for (int i = 0; i < SizeOfGuid; i++)
            {
                byte num3 = x.m_value[x_rgiGuidOrder[i]];
                byte num2 = y.m_value[x_rgiGuidOrder[i]];
                if (num3 != num2)
                {
                    if (num3 >= num2)
                    {
                        return EComparison.GT;
                    }
                    return EComparison.LT;
                }
            }
            return EComparison.EQ;
        }

        public static explicit operator SqlGuid(SqlString x)
        {
            if (!x.IsNull)
            {
                return new SqlGuid(x.Value);
            }
            return Null;
        }

        public static explicit operator SqlGuid(SqlBinary x)
        {
            if (!x.IsNull)
            {
                return new SqlGuid(x.Value);
            }
            return Null;
        }

        public static SqlBoolean operator ==(SqlGuid x, SqlGuid y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(Compare(x, y) == EComparison.EQ);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlGuid x, SqlGuid y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlGuid x, SqlGuid y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(Compare(x, y) == EComparison.LT);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlGuid x, SqlGuid y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean(Compare(x, y) == EComparison.GT);
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlGuid x, SqlGuid y)
        {
            if (x.IsNull || y.IsNull)
            {
                return SqlBoolean.Null;
            }
            EComparison comparison = Compare(x, y);
            return new SqlBoolean((comparison == EComparison.LT) || (comparison == EComparison.EQ));
        }

        public static SqlBoolean operator >=(SqlGuid x, SqlGuid y)
        {
            if (x.IsNull || y.IsNull)
            {
                return SqlBoolean.Null;
            }
            EComparison comparison = Compare(x, y);
            return new SqlBoolean((comparison == EComparison.GT) || (comparison == EComparison.EQ));
        }

        public static SqlBoolean Equals(SqlGuid x, SqlGuid y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlGuid x, SqlGuid y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlGuid x, SqlGuid y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlGuid x, SqlGuid y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlGuid x, SqlGuid y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlGuid x, SqlGuid y)
        {
            return (x >= y);
        }

        public SqlString ToSqlString()
        {
            return (SqlString) this;
        }

        public SqlBinary ToSqlBinary()
        {
            return (SqlBinary) this;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlGuid))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlGuid));
            }
            SqlGuid guid = (SqlGuid) value;
            return this.CompareTo(guid);
        }

        public int CompareTo(SqlGuid value)
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
            if (!(value is SqlGuid))
            {
                return false;
            }
            SqlGuid guid = (SqlGuid) value;
            if (guid.IsNull || this.IsNull)
            {
                return (guid.IsNull && this.IsNull);
            }
            SqlBoolean flag = this == guid;
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
                this.m_value = null;
            }
            else
            {
                this.m_value = new Guid(reader.ReadElementString()).ToByteArray();
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
                writer.WriteString(XmlConvert.ToString(new Guid(this.m_value)));
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlGuid()
        {
            SizeOfGuid = 0x10;
            x_rgiGuidOrder = new int[] { 10, 11, 12, 13, 14, 15, 8, 9, 6, 7, 4, 5, 0, 1, 2, 3 };
            Null = new SqlGuid(true);
        }
    }
}

