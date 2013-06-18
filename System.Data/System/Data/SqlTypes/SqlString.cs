namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlString : INullable, IComparable, IXmlSerializable
    {
        private string m_value;
        private System.Globalization.CompareInfo m_cmpInfo;
        private int m_lcid;
        private System.Data.SqlTypes.SqlCompareOptions m_flag;
        private bool m_fNotNull;
        public static readonly SqlString Null;
        internal static readonly UnicodeEncoding x_UnicodeEncoding;
        public static readonly int IgnoreCase;
        public static readonly int IgnoreWidth;
        public static readonly int IgnoreNonSpace;
        public static readonly int IgnoreKanaType;
        public static readonly int BinarySort;
        public static readonly int BinarySort2;
        private static readonly System.Data.SqlTypes.SqlCompareOptions x_iDefaultFlag;
        private static readonly CompareOptions x_iValidCompareOptionMask;
        internal static readonly System.Data.SqlTypes.SqlCompareOptions x_iValidSqlCompareOptionMask;
        internal static readonly int x_lcidUSEnglish;
        private static readonly int x_lcidBinary;
        private SqlString(bool fNull)
        {
            this.m_value = null;
            this.m_cmpInfo = null;
            this.m_lcid = 0;
            this.m_flag = System.Data.SqlTypes.SqlCompareOptions.None;
            this.m_fNotNull = false;
        }

        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode)
        {
            this.m_lcid = lcid;
            ValidateSqlCompareOptions(compareOptions);
            this.m_flag = compareOptions;
            if (data == null)
            {
                this.m_fNotNull = false;
                this.m_value = null;
                this.m_cmpInfo = null;
            }
            else
            {
                this.m_fNotNull = true;
                this.m_cmpInfo = null;
                if (fUnicode)
                {
                    this.m_value = x_UnicodeEncoding.GetString(data, index, count);
                }
                else
                {
                    System.Globalization.CultureInfo info = new System.Globalization.CultureInfo(this.m_lcid);
                    this.m_value = Encoding.GetEncoding(info.TextInfo.ANSICodePage).GetString(data, index, count);
                }
            }
        }

        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data, bool fUnicode) : this(lcid, compareOptions, data, 0, data.Length, fUnicode)
        {
        }

        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data, int index, int count) : this(lcid, compareOptions, data, index, count, true)
        {
        }

        public SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, byte[] data) : this(lcid, compareOptions, data, 0, data.Length, true)
        {
        }

        public SqlString(string data, int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions)
        {
            this.m_lcid = lcid;
            ValidateSqlCompareOptions(compareOptions);
            this.m_flag = compareOptions;
            this.m_cmpInfo = null;
            if (data == null)
            {
                this.m_fNotNull = false;
                this.m_value = null;
            }
            else
            {
                this.m_fNotNull = true;
                this.m_value = data;
            }
        }

        public SqlString(string data, int lcid) : this(data, lcid, x_iDefaultFlag)
        {
        }

        public SqlString(string data) : this(data, System.Globalization.CultureInfo.CurrentCulture.LCID, x_iDefaultFlag)
        {
        }

        private SqlString(int lcid, System.Data.SqlTypes.SqlCompareOptions compareOptions, string data, System.Globalization.CompareInfo cmpInfo)
        {
            this.m_lcid = lcid;
            ValidateSqlCompareOptions(compareOptions);
            this.m_flag = compareOptions;
            if (data == null)
            {
                this.m_fNotNull = false;
                this.m_value = null;
                this.m_cmpInfo = null;
            }
            else
            {
                this.m_value = data;
                this.m_cmpInfo = cmpInfo;
                this.m_fNotNull = true;
            }
        }

        public bool IsNull
        {
            get
            {
                return !this.m_fNotNull;
            }
        }
        public string Value
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
        public int LCID
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_lcid;
            }
        }
        public System.Globalization.CultureInfo CultureInfo
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return System.Globalization.CultureInfo.GetCultureInfo(this.m_lcid);
            }
        }
        private void SetCompareInfo()
        {
            if (this.m_cmpInfo == null)
            {
                this.m_cmpInfo = System.Globalization.CultureInfo.GetCultureInfo(this.m_lcid).CompareInfo;
            }
        }

        public System.Globalization.CompareInfo CompareInfo
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                this.SetCompareInfo();
                return this.m_cmpInfo;
            }
        }
        public System.Data.SqlTypes.SqlCompareOptions SqlCompareOptions
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_flag;
            }
        }
        public static implicit operator SqlString(string x)
        {
            return new SqlString(x);
        }

        public static explicit operator string(SqlString x)
        {
            return x.Value;
        }

        public override string ToString()
        {
            if (!this.IsNull)
            {
                return this.m_value;
            }
            return SQLResource.NullString;
        }

        public byte[] GetUnicodeBytes()
        {
            if (this.IsNull)
            {
                return null;
            }
            return x_UnicodeEncoding.GetBytes(this.m_value);
        }

        public byte[] GetNonUnicodeBytes()
        {
            if (this.IsNull)
            {
                return null;
            }
            System.Globalization.CultureInfo info = new System.Globalization.CultureInfo(this.m_lcid);
            return Encoding.GetEncoding(info.TextInfo.ANSICodePage).GetBytes(this.m_value);
        }

        public static SqlString operator +(SqlString x, SqlString y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            if ((x.m_lcid != y.m_lcid) || (x.m_flag != y.m_flag))
            {
                throw new SqlTypeException(SQLResource.ConcatDiffCollationMessage);
            }
            return new SqlString(x.m_lcid, x.m_flag, x.m_value + y.m_value, (x.m_cmpInfo == null) ? y.m_cmpInfo : x.m_cmpInfo);
        }

        private static int StringCompare(SqlString x, SqlString y)
        {
            if ((x.m_lcid != y.m_lcid) || (x.m_flag != y.m_flag))
            {
                throw new SqlTypeException(SQLResource.CompareDiffCollationMessage);
            }
            x.SetCompareInfo();
            y.SetCompareInfo();
            if ((x.m_flag & System.Data.SqlTypes.SqlCompareOptions.BinarySort) != System.Data.SqlTypes.SqlCompareOptions.None)
            {
                return CompareBinary(x, y);
            }
            if ((x.m_flag & System.Data.SqlTypes.SqlCompareOptions.BinarySort2) != System.Data.SqlTypes.SqlCompareOptions.None)
            {
                return CompareBinary2(x, y);
            }
            char[] chArray2 = x.m_value.ToCharArray();
            char[] chArray = y.m_value.ToCharArray();
            int length = chArray2.Length;
            int num = chArray.Length;
            while ((length > 0) && (chArray2[length - 1] == ' '))
            {
                length--;
            }
            while ((num > 0) && (chArray[num - 1] == ' '))
            {
                num--;
            }
            CompareOptions options = CompareOptionsFromSqlCompareOptions(x.m_flag);
            return x.m_cmpInfo.Compare(x.m_value, 0, length, y.m_value, 0, num, options);
        }

        private static SqlBoolean Compare(SqlString x, SqlString y, EComparison ecExpectedResult)
        {
            bool flag;
            if (!x.IsNull && !y.IsNull)
            {
                int num = StringCompare(x, y);
                flag = false;
                switch (ecExpectedResult)
                {
                    case EComparison.LT:
                        flag = num < 0;
                        goto Label_006F;

                    case EComparison.LE:
                        flag = num <= 0;
                        goto Label_006F;

                    case EComparison.EQ:
                        flag = num == 0;
                        goto Label_006F;

                    case EComparison.GE:
                        flag = num >= 0;
                        goto Label_006F;

                    case EComparison.GT:
                        flag = num > 0;
                        goto Label_006F;
                }
            }
            return SqlBoolean.Null;
        Label_006F:
            return new SqlBoolean(flag);
        }

        public static explicit operator SqlString(SqlBoolean x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.Value.ToString());
            }
            return Null;
        }

        public static explicit operator SqlString(SqlByte x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.Value.ToString((IFormatProvider) null));
            }
            return Null;
        }

        public static explicit operator SqlString(SqlInt16 x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.Value.ToString((IFormatProvider) null));
            }
            return Null;
        }

        public static explicit operator SqlString(SqlInt32 x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.Value.ToString((IFormatProvider) null));
            }
            return Null;
        }

        public static explicit operator SqlString(SqlInt64 x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.Value.ToString((IFormatProvider) null));
            }
            return Null;
        }

        public static explicit operator SqlString(SqlSingle x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.Value.ToString((IFormatProvider) null));
            }
            return Null;
        }

        public static explicit operator SqlString(SqlDouble x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.Value.ToString((IFormatProvider) null));
            }
            return Null;
        }

        public static explicit operator SqlString(SqlDecimal x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.ToString());
            }
            return Null;
        }

        public static explicit operator SqlString(SqlMoney x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.ToString());
            }
            return Null;
        }

        public static explicit operator SqlString(SqlDateTime x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.ToString());
            }
            return Null;
        }

        public static explicit operator SqlString(SqlGuid x)
        {
            if (!x.IsNull)
            {
                return new SqlString(x.ToString());
            }
            return Null;
        }

        public SqlString Clone()
        {
            if (this.IsNull)
            {
                return new SqlString(true);
            }
            return new SqlString(this.m_value, this.m_lcid, this.m_flag);
        }

        public static SqlBoolean operator ==(SqlString x, SqlString y)
        {
            return Compare(x, y, EComparison.EQ);
        }

        public static SqlBoolean operator !=(SqlString x, SqlString y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlString x, SqlString y)
        {
            return Compare(x, y, EComparison.LT);
        }

        public static SqlBoolean operator >(SqlString x, SqlString y)
        {
            return Compare(x, y, EComparison.GT);
        }

        public static SqlBoolean operator <=(SqlString x, SqlString y)
        {
            return Compare(x, y, EComparison.LE);
        }

        public static SqlBoolean operator >=(SqlString x, SqlString y)
        {
            return Compare(x, y, EComparison.GE);
        }

        public static SqlString Concat(SqlString x, SqlString y)
        {
            return (x + y);
        }

        public static SqlString Add(SqlString x, SqlString y)
        {
            return (x + y);
        }

        public static SqlBoolean Equals(SqlString x, SqlString y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlString x, SqlString y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlString x, SqlString y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlString x, SqlString y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlString x, SqlString y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlString x, SqlString y)
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

        public SqlDateTime ToSqlDateTime()
        {
            return (SqlDateTime) this;
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

        public SqlGuid ToSqlGuid()
        {
            return (SqlGuid) this;
        }

        private static void ValidateSqlCompareOptions(System.Data.SqlTypes.SqlCompareOptions compareOptions)
        {
            if ((compareOptions & x_iValidSqlCompareOptionMask) != compareOptions)
            {
                throw new ArgumentOutOfRangeException("compareOptions");
            }
        }

        public static CompareOptions CompareOptionsFromSqlCompareOptions(System.Data.SqlTypes.SqlCompareOptions compareOptions)
        {
            CompareOptions none = CompareOptions.None;
            ValidateSqlCompareOptions(compareOptions);
            if ((compareOptions & (System.Data.SqlTypes.SqlCompareOptions.BinarySort | System.Data.SqlTypes.SqlCompareOptions.BinarySort2)) != System.Data.SqlTypes.SqlCompareOptions.None)
            {
                throw ADP.ArgumentOutOfRange("compareOptions");
            }
            if ((compareOptions & System.Data.SqlTypes.SqlCompareOptions.IgnoreCase) != System.Data.SqlTypes.SqlCompareOptions.None)
            {
                none |= CompareOptions.IgnoreCase;
            }
            if ((compareOptions & System.Data.SqlTypes.SqlCompareOptions.IgnoreNonSpace) != System.Data.SqlTypes.SqlCompareOptions.None)
            {
                none |= CompareOptions.IgnoreNonSpace;
            }
            if ((compareOptions & System.Data.SqlTypes.SqlCompareOptions.IgnoreKanaType) != System.Data.SqlTypes.SqlCompareOptions.None)
            {
                none |= CompareOptions.IgnoreKanaType;
            }
            if ((compareOptions & System.Data.SqlTypes.SqlCompareOptions.IgnoreWidth) != System.Data.SqlTypes.SqlCompareOptions.None)
            {
                none |= CompareOptions.IgnoreWidth;
            }
            return none;
        }

        private bool FBinarySort()
        {
            return (!this.IsNull && ((this.m_flag & (System.Data.SqlTypes.SqlCompareOptions.BinarySort | System.Data.SqlTypes.SqlCompareOptions.BinarySort2)) != System.Data.SqlTypes.SqlCompareOptions.None));
        }

        private static int CompareBinary(SqlString x, SqlString y)
        {
            int num;
            int num2;
            byte[] bytes = x_UnicodeEncoding.GetBytes(x.m_value);
            byte[] buffer = x_UnicodeEncoding.GetBytes(y.m_value);
            int length = bytes.Length;
            int num4 = buffer.Length;
            int num6 = (length < num4) ? length : num4;
            for (num = 0; num < num6; num++)
            {
                if (bytes[num] < buffer[num])
                {
                    return -1;
                }
                if (bytes[num] > buffer[num])
                {
                    return 1;
                }
            }
            num = num6;
            int num3 = 0x20;
            if (length >= num4)
            {
                while (num < length)
                {
                    num2 = bytes[num + 1] << (8 + bytes[num]);
                    if (num2 != num3)
                    {
                        if (num2 <= num3)
                        {
                            return -1;
                        }
                        return 1;
                    }
                    num += 2;
                }
            }
            else
            {
                while (num < num4)
                {
                    num2 = buffer[num + 1] << (8 + buffer[num]);
                    if (num2 != num3)
                    {
                        if (num3 <= num2)
                        {
                            return -1;
                        }
                        return 1;
                    }
                    num += 2;
                }
            }
            return 0;
        }

        private static int CompareBinary2(SqlString x, SqlString y)
        {
            int num;
            char[] chArray2 = x.m_value.ToCharArray();
            char[] chArray = y.m_value.ToCharArray();
            int length = chArray2.Length;
            int num2 = chArray.Length;
            int num4 = (length < num2) ? length : num2;
            for (num = 0; num < num4; num++)
            {
                if (chArray2[num] < chArray[num])
                {
                    return -1;
                }
                if (chArray2[num] > chArray[num])
                {
                    return 1;
                }
            }
            char ch = ' ';
            if (length < num2)
            {
                for (num = num4; num < num2; num++)
                {
                    if (chArray[num] != ch)
                    {
                        if (ch <= chArray[num])
                        {
                            return -1;
                        }
                        return 1;
                    }
                }
            }
            else
            {
                for (num = num4; num < length; num++)
                {
                    if (chArray2[num] != ch)
                    {
                        if (chArray2[num] <= ch)
                        {
                            return -1;
                        }
                        return 1;
                    }
                }
            }
            return 0;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlString))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlString));
            }
            SqlString str = (SqlString) value;
            return this.CompareTo(str);
        }

        public int CompareTo(SqlString value)
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
            int num = StringCompare(this, value);
            if (num < 0)
            {
                return -1;
            }
            if (num > 0)
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (!(value is SqlString))
            {
                return false;
            }
            SqlString str = (SqlString) value;
            if (str.IsNull || this.IsNull)
            {
                return (str.IsNull && this.IsNull);
            }
            SqlBoolean flag = this == str;
            return flag.Value;
        }

        public override int GetHashCode()
        {
            byte[] bytes;
            if (this.IsNull)
            {
                return 0;
            }
            if (this.FBinarySort())
            {
                bytes = x_UnicodeEncoding.GetBytes(this.m_value.TrimEnd(new char[0]));
            }
            else
            {
                CompareOptions none;
                System.Globalization.CompareInfo cmpInfo;
                try
                {
                    this.SetCompareInfo();
                    cmpInfo = this.m_cmpInfo;
                    none = CompareOptionsFromSqlCompareOptions(this.m_flag);
                }
                catch (ArgumentException)
                {
                    cmpInfo = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
                    none = CompareOptions.None;
                }
                bytes = cmpInfo.GetSortKey(this.m_value.TrimEnd(new char[0]), none).KeyData;
            }
            return SqlBinary.HashByteArray(bytes, bytes.Length);
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
                this.m_value = reader.ReadElementString();
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
                writer.WriteString(this.m_value);
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlString()
        {
            Null = new SqlString(true);
            x_UnicodeEncoding = new UnicodeEncoding();
            IgnoreCase = 1;
            IgnoreWidth = 0x10;
            IgnoreNonSpace = 2;
            IgnoreKanaType = 8;
            BinarySort = 0x8000;
            BinarySort2 = 0x4000;
            x_iDefaultFlag = System.Data.SqlTypes.SqlCompareOptions.IgnoreWidth | System.Data.SqlTypes.SqlCompareOptions.IgnoreKanaType | System.Data.SqlTypes.SqlCompareOptions.IgnoreCase;
            x_iValidCompareOptionMask = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;
            x_iValidSqlCompareOptionMask = System.Data.SqlTypes.SqlCompareOptions.BinarySort | System.Data.SqlTypes.SqlCompareOptions.BinarySort2 | System.Data.SqlTypes.SqlCompareOptions.IgnoreWidth | System.Data.SqlTypes.SqlCompareOptions.IgnoreKanaType | System.Data.SqlTypes.SqlCompareOptions.IgnoreNonSpace | System.Data.SqlTypes.SqlCompareOptions.IgnoreCase;
            x_lcidUSEnglish = 0x409;
            x_lcidBinary = 0x8200;
        }
    }
}

