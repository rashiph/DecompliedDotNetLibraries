namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Globalization;

    public sealed class SqlMetaData
    {
        private static byte[] __maxLenFromPrecision = new byte[] { 
            5, 5, 5, 5, 5, 5, 5, 5, 5, 9, 9, 9, 9, 9, 9, 9, 
            9, 9, 9, 13, 13, 13, 13, 13, 13, 13, 13, 13, 0x11, 0x11, 0x11, 0x11, 
            0x11, 0x11, 0x11, 0x11, 0x11, 0x11
         };
        private static byte[] __maxVarTimeLenOffsetFromScale = new byte[] { 2, 2, 2, 1, 1, 0, 0, 0 };
        private static readonly long[] __unitTicksFromScale = new long[] { 0x989680L, 0xf4240L, 0x186a0L, 0x2710L, 0x3e8L, 100L, 10L, 1L };
        private bool m_bPartialLength;
        private byte m_bPrecision;
        private byte m_bScale;
        private System.Data.SqlClient.SortOrder m_columnSortOrder;
        private SqlCompareOptions m_eCompareOptions;
        private bool m_isUniqueKey;
        private long m_lLocale;
        private long m_lMaxLength;
        private string m_serverTypeName;
        private int m_sortOrdinal;
        private System.Data.SqlDbType m_sqlDbType;
        private string m_strName;
        private System.Type m_udttype;
        private bool m_useServerDefault;
        private string m_XmlSchemaCollectionDatabase;
        private string m_XmlSchemaCollectionName;
        private string m_XmlSchemaCollectionOwningSchema;
        private const byte MaxTimeScale = 7;
        internal static SqlMetaData[] sxm_rgDefaults;
        private static System.Data.DbType[] sxm_rgSqlDbTypeToDbType;
        private const System.Data.SqlClient.SortOrder x_defaultColumnSortOrder = System.Data.SqlClient.SortOrder.Unspecified;
        private const bool x_defaultIsUniqueKey = false;
        private const int x_defaultSortOrdinal = -1;
        private const bool x_defaultUseServerDefault = false;
        private static readonly DateTime x_dtSmallMax = new DateTime(0x81f, 6, 6, 0x17, 0x3b, 0x1d, 0x3e6);
        private static readonly DateTime x_dtSmallMin = new DateTime(0x76b, 12, 0x1f, 0x17, 0x3b, 0x1d, 0x3e7);
        private const SqlCompareOptions x_eDefaultStringCompareOptions = (SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        private const long x_lMax = -1L;
        private const long x_lServerMaxANSI = 0x1f40L;
        private const long x_lServerMaxBinary = 0x1f40L;
        private const long x_lServerMaxUnicode = 0xfa0L;
        private static readonly SqlMoney x_smSmallMax = new SqlMoney(214748.3647M);
        private static readonly SqlMoney x_smSmallMin = new SqlMoney(-214748.3648M);
        private static readonly TimeSpan x_timeMax = new TimeSpan(0xc92a69bfffL);
        private static readonly TimeSpan x_timeMin = TimeSpan.Zero;

        static SqlMetaData()
        {
            System.Data.DbType[] typeArray = new System.Data.DbType[0x23];
            typeArray[0] = System.Data.DbType.Int64;
            typeArray[1] = System.Data.DbType.Binary;
            typeArray[2] = System.Data.DbType.Boolean;
            typeArray[4] = System.Data.DbType.DateTime;
            typeArray[5] = System.Data.DbType.Decimal;
            typeArray[6] = System.Data.DbType.Double;
            typeArray[7] = System.Data.DbType.Binary;
            typeArray[8] = System.Data.DbType.Int32;
            typeArray[9] = System.Data.DbType.Currency;
            typeArray[10] = System.Data.DbType.String;
            typeArray[11] = System.Data.DbType.String;
            typeArray[12] = System.Data.DbType.String;
            typeArray[13] = System.Data.DbType.Single;
            typeArray[14] = System.Data.DbType.Guid;
            typeArray[15] = System.Data.DbType.DateTime;
            typeArray[0x10] = System.Data.DbType.Int16;
            typeArray[0x11] = System.Data.DbType.Currency;
            typeArray[0x13] = System.Data.DbType.Binary;
            typeArray[20] = System.Data.DbType.Byte;
            typeArray[0x15] = System.Data.DbType.Binary;
            typeArray[0x17] = System.Data.DbType.Object;
            typeArray[0x18] = System.Data.DbType.Object;
            typeArray[0x19] = System.Data.DbType.Xml;
            typeArray[0x1a] = System.Data.DbType.String;
            typeArray[0x1b] = System.Data.DbType.String;
            typeArray[0x1c] = System.Data.DbType.String;
            typeArray[0x1d] = System.Data.DbType.Object;
            typeArray[30] = System.Data.DbType.Object;
            typeArray[0x1f] = System.Data.DbType.Date;
            typeArray[0x20] = System.Data.DbType.Time;
            typeArray[0x21] = System.Data.DbType.DateTime2;
            typeArray[0x22] = System.Data.DbType.DateTimeOffset;
            sxm_rgSqlDbTypeToDbType = typeArray;
            sxm_rgDefaults = new SqlMetaData[] { 
                new SqlMetaData("bigint", System.Data.SqlDbType.BigInt, 8L, 0x13, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("binary", System.Data.SqlDbType.Binary, 1L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("bit", System.Data.SqlDbType.Bit, 1L, 1, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("char", System.Data.SqlDbType.Char, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("datetime", System.Data.SqlDbType.DateTime, 8L, 0x17, 3, 0L, SqlCompareOptions.None, false), new SqlMetaData("decimal", System.Data.SqlDbType.Decimal, 9L, 0x12, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("float", System.Data.SqlDbType.Float, 8L, 0x35, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("image", System.Data.SqlDbType.Image, -1L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("int", System.Data.SqlDbType.Int, 4L, 10, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("money", System.Data.SqlDbType.Money, 8L, 0x13, 4, 0L, SqlCompareOptions.None, false), new SqlMetaData("nchar", System.Data.SqlDbType.NChar, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("ntext", System.Data.SqlDbType.NText, -1L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("nvarchar", System.Data.SqlDbType.NVarChar, 0xfa0L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("real", System.Data.SqlDbType.Real, 4L, 0x18, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("uniqueidentifier", System.Data.SqlDbType.UniqueIdentifier, 0x10L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("smalldatetime", System.Data.SqlDbType.SmallDateTime, 4L, 0x10, 0, 0L, SqlCompareOptions.None, false), 
                new SqlMetaData("smallint", System.Data.SqlDbType.SmallInt, 2L, 5, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("smallmoney", System.Data.SqlDbType.SmallMoney, 4L, 10, 4, 0L, SqlCompareOptions.None, false), new SqlMetaData("text", System.Data.SqlDbType.Text, -1L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("timestamp", System.Data.SqlDbType.Timestamp, 8L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("tinyint", System.Data.SqlDbType.TinyInt, 1L, 3, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("varbinary", System.Data.SqlDbType.VarBinary, 0x1f40L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("varchar", System.Data.SqlDbType.VarChar, 0x1f40L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("sql_variant", System.Data.SqlDbType.Variant, 0x1f50L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("nvarchar", System.Data.SqlDbType.NVarChar, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("xml", System.Data.SqlDbType.Xml, -1L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, true), new SqlMetaData("nvarchar", System.Data.SqlDbType.NVarChar, 1L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("nvarchar", System.Data.SqlDbType.NVarChar, 0xfa0L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("nvarchar", System.Data.SqlDbType.NVarChar, 0xfa0L, 0, 0, 0L, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, false), new SqlMetaData("udt", System.Data.SqlDbType.Udt, 0L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("table", System.Data.SqlDbType.Structured, 0L, 0, 0, 0L, SqlCompareOptions.None, false), new SqlMetaData("date", System.Data.SqlDbType.Date, 3L, 10, 0, 0L, SqlCompareOptions.None, false), 
                new SqlMetaData("time", System.Data.SqlDbType.Time, 5L, 0, 7, 0L, SqlCompareOptions.None, false), new SqlMetaData("datetime2", System.Data.SqlDbType.DateTime2, 8L, 0, 7, 0L, SqlCompareOptions.None, false), new SqlMetaData("datetimeoffset", System.Data.SqlDbType.DateTimeOffset, 10L, 0, 7, 0L, SqlCompareOptions.None, false)
             };
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType)
        {
            this.Construct(name, dbType, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, long maxLength)
        {
            this.Construct(name, dbType, maxLength, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, System.Type userDefinedType)
        {
            this.Construct(name, dbType, userDefinedType, null, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, byte precision, byte scale)
        {
            this.Construct(name, dbType, precision, scale, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, System.Type userDefinedType, string serverTypeName)
        {
            this.Construct(name, dbType, userDefinedType, serverTypeName, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions)
        {
            this.Construct(name, dbType, maxLength, locale, compareOptions, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, string database, string owningSchema, string objectName)
        {
            this.Construct(name, dbType, database, owningSchema, objectName, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.Construct(name, dbType, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, long maxLength, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.Construct(name, dbType, maxLength, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, byte precision, byte scale, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.Construct(name, dbType, precision, scale, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
        }

        private SqlMetaData(string name, System.Data.SqlDbType sqlDbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, bool partialLength)
        {
            this.AssertNameIsValid(name);
            this.m_strName = name;
            this.m_sqlDbType = sqlDbType;
            this.m_lMaxLength = maxLength;
            this.m_bPrecision = precision;
            this.m_bScale = scale;
            this.m_lLocale = localeId;
            this.m_eCompareOptions = compareOptions;
            this.m_bPartialLength = partialLength;
            this.m_udttype = null;
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, long maxLength, byte precision, byte scale, long locale, SqlCompareOptions compareOptions, System.Type userDefinedType) : this(name, dbType, maxLength, precision, scale, locale, compareOptions, userDefinedType, false, false, System.Data.SqlClient.SortOrder.Unspecified, -1)
        {
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, System.Type userDefinedType, string serverTypeName, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.Construct(name, dbType, userDefinedType, serverTypeName, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.Construct(name, dbType, maxLength, locale, compareOptions, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, string database, string owningSchema, string objectName, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.Construct(name, dbType, database, owningSchema, objectName, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
        }

        internal SqlMetaData(string name, System.Data.SqlDbType sqlDBType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, string xmlSchemaCollectionDatabase, string xmlSchemaCollectionOwningSchema, string xmlSchemaCollectionName, bool partialLength, System.Type udtType)
        {
            this.AssertNameIsValid(name);
            this.m_strName = name;
            this.m_sqlDbType = sqlDBType;
            this.m_lMaxLength = maxLength;
            this.m_bPrecision = precision;
            this.m_bScale = scale;
            this.m_lLocale = localeId;
            this.m_eCompareOptions = compareOptions;
            this.m_XmlSchemaCollectionDatabase = xmlSchemaCollectionDatabase;
            this.m_XmlSchemaCollectionOwningSchema = xmlSchemaCollectionOwningSchema;
            this.m_XmlSchemaCollectionName = xmlSchemaCollectionName;
            this.m_bPartialLength = partialLength;
            this.m_udttype = udtType;
        }

        public SqlMetaData(string name, System.Data.SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, System.Type userDefinedType, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            switch (dbType)
            {
                case System.Data.SqlDbType.BigInt:
                case System.Data.SqlDbType.Bit:
                case System.Data.SqlDbType.DateTime:
                case System.Data.SqlDbType.Float:
                case System.Data.SqlDbType.Image:
                case System.Data.SqlDbType.Int:
                case System.Data.SqlDbType.Money:
                case System.Data.SqlDbType.Real:
                case System.Data.SqlDbType.UniqueIdentifier:
                case System.Data.SqlDbType.SmallDateTime:
                case System.Data.SqlDbType.SmallInt:
                case System.Data.SqlDbType.SmallMoney:
                case System.Data.SqlDbType.Timestamp:
                case System.Data.SqlDbType.TinyInt:
                case System.Data.SqlDbType.Xml:
                case System.Data.SqlDbType.Date:
                    this.Construct(name, dbType, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
                    return;

                case System.Data.SqlDbType.Binary:
                case System.Data.SqlDbType.VarBinary:
                    this.Construct(name, dbType, maxLength, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
                    return;

                case System.Data.SqlDbType.Char:
                case System.Data.SqlDbType.NChar:
                case System.Data.SqlDbType.NVarChar:
                case System.Data.SqlDbType.VarChar:
                    this.Construct(name, dbType, maxLength, localeId, compareOptions, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
                    return;

                case System.Data.SqlDbType.Decimal:
                case System.Data.SqlDbType.Time:
                case System.Data.SqlDbType.DateTime2:
                case System.Data.SqlDbType.DateTimeOffset:
                    this.Construct(name, dbType, precision, scale, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
                    return;

                case System.Data.SqlDbType.NText:
                case System.Data.SqlDbType.Text:
                    this.Construct(name, dbType, Max, localeId, compareOptions, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
                    return;

                case System.Data.SqlDbType.Variant:
                    this.Construct(name, dbType, useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
                    return;

                case System.Data.SqlDbType.Udt:
                    this.Construct(name, dbType, userDefinedType, "", useServerDefault, isUniqueKey, columnSortOrder, sortOrdinal);
                    return;
            }
            SQL.InvalidSqlDbTypeForConstructor(dbType);
        }

        public bool Adjust(bool value)
        {
            if (System.Data.SqlDbType.Bit != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public byte Adjust(byte value)
        {
            if (System.Data.SqlDbType.TinyInt != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public char Adjust(char value)
        {
            if ((System.Data.SqlDbType.Char == this.SqlDbType) || (System.Data.SqlDbType.NChar == this.SqlDbType))
            {
                if (1L != this.MaxLength)
                {
                    ThrowInvalidType();
                }
                return value;
            }
            if ((1L > this.MaxLength) || (((System.Data.SqlDbType.VarChar != this.SqlDbType) && (System.Data.SqlDbType.NVarChar != this.SqlDbType)) && ((System.Data.SqlDbType.Text != this.SqlDbType) && (System.Data.SqlDbType.NText != this.SqlDbType))))
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlBinary Adjust(SqlBinary value)
        {
            if ((System.Data.SqlDbType.Binary == this.SqlDbType) || (System.Data.SqlDbType.Timestamp == this.SqlDbType))
            {
                if (!value.IsNull && (value.Length < this.MaxLength))
                {
                    byte[] sourceArray = value.Value;
                    byte[] destinationArray = new byte[this.MaxLength];
                    Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                    Array.Clear(destinationArray, sourceArray.Length, destinationArray.Length - sourceArray.Length);
                    return new SqlBinary(destinationArray);
                }
            }
            else if ((System.Data.SqlDbType.VarBinary != this.SqlDbType) && (System.Data.SqlDbType.Image != this.SqlDbType))
            {
                ThrowInvalidType();
            }
            if (!value.IsNull && ((value.Length > this.MaxLength) && (Max != this.MaxLength)))
            {
                byte[] buffer4 = value.Value;
                byte[] buffer3 = new byte[this.MaxLength];
                Array.Copy(buffer4, buffer3, (int) this.MaxLength);
                value = new SqlBinary(buffer3);
            }
            return value;
        }

        public SqlBoolean Adjust(SqlBoolean value)
        {
            if (System.Data.SqlDbType.Bit != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlByte Adjust(SqlByte value)
        {
            if (System.Data.SqlDbType.TinyInt != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlBytes Adjust(SqlBytes value)
        {
            if ((System.Data.SqlDbType.Binary == this.SqlDbType) || (System.Data.SqlDbType.Timestamp == this.SqlDbType))
            {
                if ((value != null) && !value.IsNull)
                {
                    int length = (int) value.Length;
                    if (length < this.MaxLength)
                    {
                        if (value.MaxLength < this.MaxLength)
                        {
                            byte[] destinationArray = new byte[this.MaxLength];
                            Array.Copy(value.Buffer, destinationArray, length);
                            value = new SqlBytes(destinationArray);
                        }
                        byte[] array = value.Buffer;
                        Array.Clear(array, length, array.Length - length);
                        value.SetLength(this.MaxLength);
                        return value;
                    }
                }
            }
            else if ((System.Data.SqlDbType.VarBinary != this.SqlDbType) && (System.Data.SqlDbType.Image != this.SqlDbType))
            {
                ThrowInvalidType();
            }
            if (((value != null) && !value.IsNull) && ((value.Length > this.MaxLength) && (Max != this.MaxLength)))
            {
                value.SetLength(this.MaxLength);
            }
            return value;
        }

        public SqlChars Adjust(SqlChars value)
        {
            if ((System.Data.SqlDbType.Char == this.SqlDbType) || (System.Data.SqlDbType.NChar == this.SqlDbType))
            {
                if ((value != null) && !value.IsNull)
                {
                    long length = value.Length;
                    if (length < this.MaxLength)
                    {
                        if (value.MaxLength < this.MaxLength)
                        {
                            char[] destinationArray = new char[(int) this.MaxLength];
                            Array.Copy(value.Buffer, destinationArray, (int) length);
                            value = new SqlChars(destinationArray);
                        }
                        char[] buffer = value.Buffer;
                        for (long i = length; i < this.MaxLength; i += 1L)
                        {
                            buffer[(int) ((IntPtr) i)] = ' ';
                        }
                        value.SetLength(this.MaxLength);
                        return value;
                    }
                }
            }
            else if (((System.Data.SqlDbType.VarChar != this.SqlDbType) && (System.Data.SqlDbType.NVarChar != this.SqlDbType)) && ((System.Data.SqlDbType.Text != this.SqlDbType) && (System.Data.SqlDbType.NText != this.SqlDbType)))
            {
                ThrowInvalidType();
            }
            if (((value != null) && !value.IsNull) && ((value.Length > this.MaxLength) && (Max != this.MaxLength)))
            {
                value.SetLength(this.MaxLength);
            }
            return value;
        }

        public SqlDateTime Adjust(SqlDateTime value)
        {
            if ((System.Data.SqlDbType.DateTime != this.SqlDbType) && (System.Data.SqlDbType.SmallDateTime != this.SqlDbType))
            {
                ThrowInvalidType();
            }
            if (!value.IsNull)
            {
                this.VerifyDateTimeRange(value.Value);
            }
            return value;
        }

        public SqlDecimal Adjust(SqlDecimal value)
        {
            if (System.Data.SqlDbType.Decimal != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return this.InternalAdjustSqlDecimal(value);
        }

        public SqlDouble Adjust(SqlDouble value)
        {
            if (System.Data.SqlDbType.Float != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlGuid Adjust(SqlGuid value)
        {
            if (System.Data.SqlDbType.UniqueIdentifier != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlInt16 Adjust(SqlInt16 value)
        {
            if (System.Data.SqlDbType.SmallInt != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlInt32 Adjust(SqlInt32 value)
        {
            if (System.Data.SqlDbType.Int != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlInt64 Adjust(SqlInt64 value)
        {
            if (this.SqlDbType != System.Data.SqlDbType.BigInt)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlMoney Adjust(SqlMoney value)
        {
            if ((System.Data.SqlDbType.Money != this.SqlDbType) && (System.Data.SqlDbType.SmallMoney != this.SqlDbType))
            {
                ThrowInvalidType();
            }
            if (!value.IsNull)
            {
                this.VerifyMoneyRange(value);
            }
            return value;
        }

        public SqlSingle Adjust(SqlSingle value)
        {
            if (System.Data.SqlDbType.Real != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public SqlString Adjust(SqlString value)
        {
            if ((System.Data.SqlDbType.Char == this.SqlDbType) || (System.Data.SqlDbType.NChar == this.SqlDbType))
            {
                if (!value.IsNull && (value.Value.Length < this.MaxLength))
                {
                    return new SqlString(value.Value.PadRight((int) this.MaxLength));
                }
            }
            else if (((System.Data.SqlDbType.VarChar != this.SqlDbType) && (System.Data.SqlDbType.NVarChar != this.SqlDbType)) && ((System.Data.SqlDbType.Text != this.SqlDbType) && (System.Data.SqlDbType.NText != this.SqlDbType)))
            {
                ThrowInvalidType();
            }
            if (!value.IsNull && ((value.Value.Length > this.MaxLength) && (Max != this.MaxLength)))
            {
                value = new SqlString(value.Value.Remove((int) this.MaxLength, value.Value.Length - ((int) this.MaxLength)));
            }
            return value;
        }

        public SqlXml Adjust(SqlXml value)
        {
            if (System.Data.SqlDbType.Xml != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public DateTime Adjust(DateTime value)
        {
            if ((System.Data.SqlDbType.DateTime == this.SqlDbType) || (System.Data.SqlDbType.SmallDateTime == this.SqlDbType))
            {
                this.VerifyDateTimeRange(value);
                return value;
            }
            if (System.Data.SqlDbType.DateTime2 == this.SqlDbType)
            {
                return new DateTime(this.InternalAdjustTimeTicks(value.Ticks));
            }
            if (System.Data.SqlDbType.Date == this.SqlDbType)
            {
                return value.Date;
            }
            ThrowInvalidType();
            return value;
        }

        public DateTimeOffset Adjust(DateTimeOffset value)
        {
            if (System.Data.SqlDbType.DateTimeOffset != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return new DateTimeOffset(this.InternalAdjustTimeTicks(value.Ticks), value.Offset);
        }

        public decimal Adjust(decimal value)
        {
            if (((System.Data.SqlDbType.Decimal != this.SqlDbType) && (System.Data.SqlDbType.Money != this.SqlDbType)) && (System.Data.SqlDbType.SmallMoney != this.SqlDbType))
            {
                ThrowInvalidType();
            }
            if (System.Data.SqlDbType.Decimal != this.SqlDbType)
            {
                this.VerifyMoneyRange(new SqlMoney(value));
                return value;
            }
            return this.InternalAdjustSqlDecimal(new SqlDecimal(value)).Value;
        }

        public double Adjust(double value)
        {
            if (System.Data.SqlDbType.Float != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public Guid Adjust(Guid value)
        {
            if (System.Data.SqlDbType.UniqueIdentifier != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public short Adjust(short value)
        {
            if (System.Data.SqlDbType.SmallInt != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public int Adjust(int value)
        {
            if (System.Data.SqlDbType.Int != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public long Adjust(long value)
        {
            if (this.SqlDbType != System.Data.SqlDbType.BigInt)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public object Adjust(object value)
        {
            if (value == null)
            {
                return null;
            }
            System.Type type = value.GetType();
            switch (System.Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    throw ADP.InvalidDataType(TypeCode.Empty);

                case TypeCode.Object:
                    if (!(type == typeof(byte[])))
                    {
                        if (type == typeof(char[]))
                        {
                            value = this.Adjust((char[]) value);
                            return value;
                        }
                        if (type == typeof(Guid))
                        {
                            value = this.Adjust((Guid) value);
                            return value;
                        }
                        if (type == typeof(object))
                        {
                            throw ADP.InvalidDataType(TypeCode.UInt64);
                        }
                        if (type == typeof(SqlBinary))
                        {
                            value = this.Adjust((SqlBinary) value);
                            return value;
                        }
                        if (type == typeof(SqlBoolean))
                        {
                            value = this.Adjust((SqlBoolean) value);
                            return value;
                        }
                        if (type == typeof(SqlByte))
                        {
                            value = this.Adjust((SqlByte) value);
                            return value;
                        }
                        if (type == typeof(SqlDateTime))
                        {
                            value = this.Adjust((SqlDateTime) value);
                            return value;
                        }
                        if (type == typeof(SqlDouble))
                        {
                            value = this.Adjust((SqlDouble) value);
                            return value;
                        }
                        if (type == typeof(SqlGuid))
                        {
                            value = this.Adjust((SqlGuid) value);
                            return value;
                        }
                        if (type == typeof(SqlInt16))
                        {
                            value = this.Adjust((SqlInt16) value);
                            return value;
                        }
                        if (type == typeof(SqlInt32))
                        {
                            value = this.Adjust((SqlInt32) value);
                            return value;
                        }
                        if (type == typeof(SqlInt64))
                        {
                            value = this.Adjust((SqlInt64) value);
                            return value;
                        }
                        if (type == typeof(SqlMoney))
                        {
                            value = this.Adjust((SqlMoney) value);
                            return value;
                        }
                        if (type == typeof(SqlDecimal))
                        {
                            value = this.Adjust((SqlDecimal) value);
                            return value;
                        }
                        if (type == typeof(SqlSingle))
                        {
                            value = this.Adjust((SqlSingle) value);
                            return value;
                        }
                        if (type == typeof(SqlString))
                        {
                            value = this.Adjust((SqlString) value);
                            return value;
                        }
                        if (type == typeof(SqlChars))
                        {
                            value = this.Adjust((SqlChars) value);
                            return value;
                        }
                        if (type == typeof(SqlBytes))
                        {
                            value = this.Adjust((SqlBytes) value);
                            return value;
                        }
                        if (type == typeof(SqlXml))
                        {
                            value = this.Adjust((SqlXml) value);
                            return value;
                        }
                        if (type == typeof(TimeSpan))
                        {
                            value = this.Adjust((TimeSpan) value);
                            return value;
                        }
                        if (type != typeof(DateTimeOffset))
                        {
                            throw ADP.UnknownDataType(type);
                        }
                        value = this.Adjust((DateTimeOffset) value);
                        return value;
                    }
                    value = this.Adjust((byte[]) value);
                    return value;

                case TypeCode.DBNull:
                    return value;

                case TypeCode.Boolean:
                    value = this.Adjust((bool) value);
                    return value;

                case TypeCode.Char:
                    value = this.Adjust((char) value);
                    return value;

                case TypeCode.SByte:
                    throw ADP.InvalidDataType(TypeCode.SByte);

                case TypeCode.Byte:
                    value = this.Adjust((byte) value);
                    return value;

                case TypeCode.Int16:
                    value = this.Adjust((short) value);
                    return value;

                case TypeCode.UInt16:
                    throw ADP.InvalidDataType(TypeCode.UInt16);

                case TypeCode.Int32:
                    value = this.Adjust((int) value);
                    return value;

                case TypeCode.UInt32:
                    throw ADP.InvalidDataType(TypeCode.UInt32);

                case TypeCode.Int64:
                    value = this.Adjust((long) value);
                    return value;

                case TypeCode.UInt64:
                    throw ADP.InvalidDataType(TypeCode.UInt64);

                case TypeCode.Single:
                    value = this.Adjust((float) value);
                    return value;

                case TypeCode.Double:
                    value = this.Adjust((double) value);
                    return value;

                case TypeCode.Decimal:
                    value = this.Adjust((decimal) value);
                    return value;

                case TypeCode.DateTime:
                    value = this.Adjust((DateTime) value);
                    return value;

                case TypeCode.String:
                    value = this.Adjust((string) value);
                    return value;
            }
            throw ADP.UnknownDataTypeCode(type, System.Type.GetTypeCode(type));
        }

        public float Adjust(float value)
        {
            if (System.Data.SqlDbType.Real != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            return value;
        }

        public string Adjust(string value)
        {
            if ((System.Data.SqlDbType.Char == this.SqlDbType) || (System.Data.SqlDbType.NChar == this.SqlDbType))
            {
                if ((value != null) && (value.Length < this.MaxLength))
                {
                    value = value.PadRight((int) this.MaxLength);
                }
            }
            else if (((System.Data.SqlDbType.VarChar != this.SqlDbType) && (System.Data.SqlDbType.NVarChar != this.SqlDbType)) && ((System.Data.SqlDbType.Text != this.SqlDbType) && (System.Data.SqlDbType.NText != this.SqlDbType)))
            {
                ThrowInvalidType();
            }
            if (value == null)
            {
                return null;
            }
            if ((value.Length > this.MaxLength) && (Max != this.MaxLength))
            {
                value = value.Remove((int) this.MaxLength, value.Length - ((int) this.MaxLength));
            }
            return value;
        }

        public TimeSpan Adjust(TimeSpan value)
        {
            if (System.Data.SqlDbType.Time != this.SqlDbType)
            {
                ThrowInvalidType();
            }
            this.VerifyTimeRange(value);
            return new TimeSpan(this.InternalAdjustTimeTicks(value.Ticks));
        }

        public byte[] Adjust(byte[] value)
        {
            if ((System.Data.SqlDbType.Binary == this.SqlDbType) || (System.Data.SqlDbType.Timestamp == this.SqlDbType))
            {
                if ((value != null) && (value.Length < this.MaxLength))
                {
                    byte[] destinationArray = new byte[this.MaxLength];
                    Array.Copy(value, destinationArray, value.Length);
                    Array.Clear(destinationArray, value.Length, destinationArray.Length - value.Length);
                    return destinationArray;
                }
            }
            else if ((System.Data.SqlDbType.VarBinary != this.SqlDbType) && (System.Data.SqlDbType.Image != this.SqlDbType))
            {
                ThrowInvalidType();
            }
            if (value == null)
            {
                return null;
            }
            if ((value.Length > this.MaxLength) && (Max != this.MaxLength))
            {
                byte[] buffer2 = new byte[this.MaxLength];
                Array.Copy(value, buffer2, (int) this.MaxLength);
                value = buffer2;
            }
            return value;
        }

        public char[] Adjust(char[] value)
        {
            if ((System.Data.SqlDbType.Char == this.SqlDbType) || (System.Data.SqlDbType.NChar == this.SqlDbType))
            {
                if (value != null)
                {
                    long length = value.Length;
                    if (length < this.MaxLength)
                    {
                        char[] destinationArray = new char[(int) this.MaxLength];
                        Array.Copy(value, destinationArray, (int) length);
                        for (long i = length; i < destinationArray.Length; i += 1L)
                        {
                            destinationArray[(int) ((IntPtr) i)] = ' ';
                        }
                        return destinationArray;
                    }
                }
            }
            else if (((System.Data.SqlDbType.VarChar != this.SqlDbType) && (System.Data.SqlDbType.NVarChar != this.SqlDbType)) && ((System.Data.SqlDbType.Text != this.SqlDbType) && (System.Data.SqlDbType.NText != this.SqlDbType)))
            {
                ThrowInvalidType();
            }
            if (value == null)
            {
                return null;
            }
            if ((value.Length > this.MaxLength) && (Max != this.MaxLength))
            {
                char[] chArray2 = new char[this.MaxLength];
                Array.Copy(value, chArray2, (int) this.MaxLength);
                value = chArray2;
            }
            return value;
        }

        private void AssertNameIsValid(string name)
        {
            if (name == null)
            {
                throw ADP.ArgumentNull("name");
            }
            if (0x80L < name.Length)
            {
                throw SQL.NameTooLong("name");
            }
        }

        private void Construct(string name, System.Data.SqlDbType dbType, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.AssertNameIsValid(name);
            this.ValidateSortOrder(columnSortOrder, sortOrdinal);
            if ((((((dbType != System.Data.SqlDbType.BigInt) && (System.Data.SqlDbType.Bit != dbType)) && ((System.Data.SqlDbType.DateTime != dbType) && (System.Data.SqlDbType.Date != dbType))) && (((System.Data.SqlDbType.DateTime2 != dbType) && (System.Data.SqlDbType.DateTimeOffset != dbType)) && ((System.Data.SqlDbType.Decimal != dbType) && (System.Data.SqlDbType.Float != dbType)))) && ((((System.Data.SqlDbType.Image != dbType) && (System.Data.SqlDbType.Int != dbType)) && ((System.Data.SqlDbType.Money != dbType) && (System.Data.SqlDbType.NText != dbType))) && (((System.Data.SqlDbType.Real != dbType) && (System.Data.SqlDbType.SmallDateTime != dbType)) && ((System.Data.SqlDbType.SmallInt != dbType) && (System.Data.SqlDbType.SmallMoney != dbType))))) && ((((System.Data.SqlDbType.Text != dbType) && (System.Data.SqlDbType.Time != dbType)) && ((System.Data.SqlDbType.Timestamp != dbType) && (System.Data.SqlDbType.TinyInt != dbType))) && (((System.Data.SqlDbType.UniqueIdentifier != dbType) && (System.Data.SqlDbType.Variant != dbType)) && (System.Data.SqlDbType.Xml != dbType))))
            {
                throw SQL.InvalidSqlDbTypeForConstructor(dbType);
            }
            this.SetDefaultsForType(dbType);
            if ((System.Data.SqlDbType.NText == dbType) || (System.Data.SqlDbType.Text == dbType))
            {
                this.m_lLocale = CultureInfo.CurrentCulture.LCID;
            }
            this.m_strName = name;
            this.m_useServerDefault = useServerDefault;
            this.m_isUniqueKey = isUniqueKey;
            this.m_columnSortOrder = columnSortOrder;
            this.m_sortOrdinal = sortOrdinal;
        }

        private void Construct(string name, System.Data.SqlDbType dbType, long maxLength, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.AssertNameIsValid(name);
            this.ValidateSortOrder(columnSortOrder, sortOrdinal);
            long lCID = 0L;
            if (System.Data.SqlDbType.Char == dbType)
            {
                if ((maxLength > 0x1f40L) || (maxLength < 0L))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
                lCID = CultureInfo.CurrentCulture.LCID;
            }
            else if (System.Data.SqlDbType.VarChar == dbType)
            {
                if (((maxLength > 0x1f40L) || (maxLength < 0L)) && (maxLength != Max))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
                lCID = CultureInfo.CurrentCulture.LCID;
            }
            else if (System.Data.SqlDbType.NChar == dbType)
            {
                if ((maxLength > 0xfa0L) || (maxLength < 0L))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
                lCID = CultureInfo.CurrentCulture.LCID;
            }
            else if (System.Data.SqlDbType.NVarChar == dbType)
            {
                if (((maxLength > 0xfa0L) || (maxLength < 0L)) && (maxLength != Max))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
                lCID = CultureInfo.CurrentCulture.LCID;
            }
            else if ((System.Data.SqlDbType.NText == dbType) || (System.Data.SqlDbType.Text == dbType))
            {
                if (Max != maxLength)
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
                lCID = CultureInfo.CurrentCulture.LCID;
            }
            else if (System.Data.SqlDbType.Binary == dbType)
            {
                if ((maxLength > 0x1f40L) || (maxLength < 0L))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            else if (System.Data.SqlDbType.VarBinary == dbType)
            {
                if (((maxLength > 0x1f40L) || (maxLength < 0L)) && (maxLength != Max))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            else
            {
                if (System.Data.SqlDbType.Image != dbType)
                {
                    throw SQL.InvalidSqlDbTypeForConstructor(dbType);
                }
                if (Max != maxLength)
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            this.SetDefaultsForType(dbType);
            this.m_strName = name;
            this.m_lMaxLength = maxLength;
            this.m_lLocale = lCID;
            this.m_useServerDefault = useServerDefault;
            this.m_isUniqueKey = isUniqueKey;
            this.m_columnSortOrder = columnSortOrder;
            this.m_sortOrdinal = sortOrdinal;
        }

        private void Construct(string name, System.Data.SqlDbType dbType, byte precision, byte scale, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.AssertNameIsValid(name);
            this.ValidateSortOrder(columnSortOrder, sortOrdinal);
            if (System.Data.SqlDbType.Decimal == dbType)
            {
                if ((precision > SqlDecimal.MaxPrecision) || (scale > precision))
                {
                    throw SQL.PrecisionValueOutOfRange(precision);
                }
                if (scale > SqlDecimal.MaxScale)
                {
                    throw SQL.ScaleValueOutOfRange(scale);
                }
            }
            else
            {
                if (((System.Data.SqlDbType.Time != dbType) && (System.Data.SqlDbType.DateTime2 != dbType)) && (System.Data.SqlDbType.DateTimeOffset != dbType))
                {
                    throw SQL.InvalidSqlDbTypeForConstructor(dbType);
                }
                if (scale > 7)
                {
                    throw SQL.TimeScaleValueOutOfRange(scale);
                }
            }
            this.SetDefaultsForType(dbType);
            this.m_strName = name;
            this.m_bPrecision = precision;
            this.m_bScale = scale;
            if (System.Data.SqlDbType.Decimal == dbType)
            {
                this.m_lMaxLength = __maxLenFromPrecision[precision - 1];
            }
            else
            {
                this.m_lMaxLength -= __maxVarTimeLenOffsetFromScale[scale];
            }
            this.m_useServerDefault = useServerDefault;
            this.m_isUniqueKey = isUniqueKey;
            this.m_columnSortOrder = columnSortOrder;
            this.m_sortOrdinal = sortOrdinal;
        }

        private void Construct(string name, System.Data.SqlDbType dbType, System.Type userDefinedType, string serverTypeName, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.AssertNameIsValid(name);
            this.ValidateSortOrder(columnSortOrder, sortOrdinal);
            if (System.Data.SqlDbType.Udt != dbType)
            {
                throw SQL.InvalidSqlDbTypeForConstructor(dbType);
            }
            if (null == userDefinedType)
            {
                throw ADP.ArgumentNull("userDefinedType");
            }
            this.SetDefaultsForType(System.Data.SqlDbType.Udt);
            this.m_strName = name;
            this.m_lMaxLength = SerializationHelperSql9.GetUdtMaxLength(userDefinedType);
            this.m_udttype = userDefinedType;
            this.m_serverTypeName = serverTypeName;
            this.m_useServerDefault = useServerDefault;
            this.m_isUniqueKey = isUniqueKey;
            this.m_columnSortOrder = columnSortOrder;
            this.m_sortOrdinal = sortOrdinal;
        }

        private void Construct(string name, System.Data.SqlDbType dbType, long maxLength, long locale, SqlCompareOptions compareOptions, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.AssertNameIsValid(name);
            this.ValidateSortOrder(columnSortOrder, sortOrdinal);
            if (System.Data.SqlDbType.Char == dbType)
            {
                if ((maxLength > 0x1f40L) || (maxLength < 0L))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            else if (System.Data.SqlDbType.VarChar == dbType)
            {
                if (((maxLength > 0x1f40L) || (maxLength < 0L)) && (maxLength != Max))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            else if (System.Data.SqlDbType.NChar == dbType)
            {
                if ((maxLength > 0xfa0L) || (maxLength < 0L))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            else if (System.Data.SqlDbType.NVarChar == dbType)
            {
                if (((maxLength > 0xfa0L) || (maxLength < 0L)) && (maxLength != Max))
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            else
            {
                if ((System.Data.SqlDbType.NText != dbType) && (System.Data.SqlDbType.Text != dbType))
                {
                    throw SQL.InvalidSqlDbTypeForConstructor(dbType);
                }
                if (Max != maxLength)
                {
                    throw ADP.Argument(Res.GetString("ADP_InvalidDataLength2", new object[] { maxLength.ToString(CultureInfo.InvariantCulture) }), "maxLength");
                }
            }
            if ((SqlCompareOptions.BinarySort != compareOptions) && ((~(SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreNonSpace | SqlCompareOptions.IgnoreCase) & compareOptions) != SqlCompareOptions.None))
            {
                throw ADP.InvalidEnumerationValue(typeof(SqlCompareOptions), (int) compareOptions);
            }
            this.SetDefaultsForType(dbType);
            this.m_strName = name;
            this.m_lMaxLength = maxLength;
            this.m_lLocale = locale;
            this.m_eCompareOptions = compareOptions;
            this.m_useServerDefault = useServerDefault;
            this.m_isUniqueKey = isUniqueKey;
            this.m_columnSortOrder = columnSortOrder;
            this.m_sortOrdinal = sortOrdinal;
        }

        private void Construct(string name, System.Data.SqlDbType dbType, string database, string owningSchema, string objectName, bool useServerDefault, bool isUniqueKey, System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            this.AssertNameIsValid(name);
            this.ValidateSortOrder(columnSortOrder, sortOrdinal);
            if (System.Data.SqlDbType.Xml != dbType)
            {
                throw SQL.InvalidSqlDbTypeForConstructor(dbType);
            }
            if (((database != null) || (owningSchema != null)) && (objectName == null))
            {
                throw ADP.ArgumentNull("objectName");
            }
            this.SetDefaultsForType(System.Data.SqlDbType.Xml);
            this.m_strName = name;
            this.m_XmlSchemaCollectionDatabase = database;
            this.m_XmlSchemaCollectionOwningSchema = owningSchema;
            this.m_XmlSchemaCollectionName = objectName;
            this.m_useServerDefault = useServerDefault;
            this.m_isUniqueKey = isUniqueKey;
            this.m_columnSortOrder = columnSortOrder;
            this.m_sortOrdinal = sortOrdinal;
        }

        internal static SqlMetaData GetNewSqlMetaDataFromOld(_SqlMetaData sqlMetaData, string name)
        {
            long length;
            byte scale;
            byte precision;
            SqlCompareOptions sqlCompareOptions;
            long lCID;
            if (0xff != sqlMetaData.precision)
            {
                precision = sqlMetaData.precision;
            }
            else if (0xff != sqlMetaData.metaType.Precision)
            {
                precision = sqlMetaData.metaType.Precision;
            }
            else
            {
                precision = 0;
            }
            if (0xff != sqlMetaData.scale)
            {
                scale = sqlMetaData.scale;
            }
            else if (0xff != sqlMetaData.metaType.Scale)
            {
                scale = sqlMetaData.metaType.Scale;
            }
            else
            {
                scale = 0;
            }
            if (sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.TinyInt)
            {
                sqlMetaData.precision = 3;
                sqlMetaData.scale = 0;
            }
            else if (sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.SmallInt)
            {
                sqlMetaData.precision = 5;
                sqlMetaData.scale = 0;
            }
            else if (sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.Int)
            {
                sqlMetaData.precision = 10;
                sqlMetaData.scale = 0;
            }
            else if (sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.BigInt)
            {
                sqlMetaData.precision = 0x13;
                sqlMetaData.scale = 0;
            }
            if (System.Data.SqlDbType.Variant == sqlMetaData.metaType.SqlDbType)
            {
                length = sqlMetaData.length;
            }
            else if (sqlMetaData.metaType.IsPlp)
            {
                length = -1L;
            }
            else if (sqlMetaData.metaType.IsFixed)
            {
                length = sqlMetaData.metaType.FixedLength;
            }
            else
            {
                length = sqlMetaData.length;
            }
            if ((length > 0L) && (((sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.NChar) || (sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.NVarChar)) || (sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.NText)))
            {
                length /= 2L;
            }
            if (sqlMetaData.collation != null)
            {
                lCID = sqlMetaData.collation.LCID;
                sqlCompareOptions = sqlMetaData.collation.SqlCompareOptions;
            }
            else
            {
                lCID = 0L;
                sqlCompareOptions = SqlCompareOptions.None;
            }
            if (sqlMetaData.metaType.SqlDbType == System.Data.SqlDbType.Udt)
            {
                SqlConnection.CheckGetExtendedUDTInfo(sqlMetaData, true);
                return new SqlMetaData(name, System.Data.SqlDbType.Udt, sqlMetaData.udtType);
            }
            return new SqlMetaData(name, sqlMetaData.metaType.SqlDbType, length, precision, scale, lCID, sqlCompareOptions, sqlMetaData.xmlSchemaCollectionDatabase, sqlMetaData.xmlSchemaCollectionOwningSchema, sqlMetaData.xmlSchemaCollectionName, sqlMetaData.metaType.IsPlp, null);
        }

        internal static SqlMetaData GetPartialLengthMetaData(SqlMetaData md)
        {
            if (md.IsPartialLength)
            {
                return md;
            }
            if (md.SqlDbType == System.Data.SqlDbType.Xml)
            {
                ThrowInvalidType();
            }
            if (((md.SqlDbType != System.Data.SqlDbType.NVarChar) && (md.SqlDbType != System.Data.SqlDbType.VarChar)) && (md.SqlDbType != System.Data.SqlDbType.VarBinary))
            {
                return md;
            }
            return new SqlMetaData(md.Name, md.SqlDbType, Max, 0, 0, md.LocaleId, md.CompareOptions, null, null, null, true, md.Type);
        }

        public static SqlMetaData InferFromValue(object value, string name)
        {
            if (value == null)
            {
                throw ADP.ArgumentNull("value");
            }
            System.Type type = value.GetType();
            switch (System.Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    throw ADP.InvalidDataType(TypeCode.Empty);

                case TypeCode.Object:
                {
                    if (!(type == typeof(byte[])))
                    {
                        if (type == typeof(char[]))
                        {
                            long maxLength = ((char[]) value).Length;
                            if (maxLength < 1L)
                            {
                                maxLength = 1L;
                            }
                            if (0xfa0L < maxLength)
                            {
                                maxLength = Max;
                            }
                            return new SqlMetaData(name, System.Data.SqlDbType.NVarChar, maxLength);
                        }
                        if (type == typeof(Guid))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.UniqueIdentifier);
                        }
                        if (type == typeof(object))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.Variant);
                        }
                        if (type == typeof(SqlBinary))
                        {
                            long max;
                            SqlBinary binary = (SqlBinary) value;
                            if (!binary.IsNull)
                            {
                                max = binary.Length;
                                if (max < 1L)
                                {
                                    max = 1L;
                                }
                                if (0x1f40L < max)
                                {
                                    max = Max;
                                }
                            }
                            else
                            {
                                max = sxm_rgDefaults[0x15].MaxLength;
                            }
                            return new SqlMetaData(name, System.Data.SqlDbType.VarBinary, max);
                        }
                        if (type == typeof(SqlBoolean))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.Bit);
                        }
                        if (type == typeof(SqlByte))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.TinyInt);
                        }
                        if (type == typeof(SqlDateTime))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.DateTime);
                        }
                        if (type == typeof(SqlDouble))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.Float);
                        }
                        if (type == typeof(SqlGuid))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.UniqueIdentifier);
                        }
                        if (type == typeof(SqlInt16))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.SmallInt);
                        }
                        if (type == typeof(SqlInt32))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.Int);
                        }
                        if (type == typeof(SqlInt64))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.BigInt);
                        }
                        if (type == typeof(SqlMoney))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.Money);
                        }
                        if (type == typeof(SqlDecimal))
                        {
                            byte scale;
                            byte precision;
                            SqlDecimal num10 = (SqlDecimal) value;
                            if (!num10.IsNull)
                            {
                                precision = num10.Precision;
                                scale = num10.Scale;
                            }
                            else
                            {
                                precision = sxm_rgDefaults[5].Precision;
                                scale = sxm_rgDefaults[5].Scale;
                            }
                            return new SqlMetaData(name, System.Data.SqlDbType.Decimal, precision, scale);
                        }
                        if (type == typeof(SqlSingle))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.Real);
                        }
                        if (type == typeof(SqlString))
                        {
                            SqlString str = (SqlString) value;
                            if (!str.IsNull)
                            {
                                long num4 = str.Value.Length;
                                if (num4 < 1L)
                                {
                                    num4 = 1L;
                                }
                                if (num4 > 0xfa0L)
                                {
                                    num4 = Max;
                                }
                                return new SqlMetaData(name, System.Data.SqlDbType.NVarChar, num4, (long) str.LCID, str.SqlCompareOptions);
                            }
                            return new SqlMetaData(name, System.Data.SqlDbType.NVarChar, sxm_rgDefaults[12].MaxLength);
                        }
                        if (type == typeof(SqlChars))
                        {
                            long num2;
                            SqlChars chars = (SqlChars) value;
                            if (!chars.IsNull)
                            {
                                num2 = chars.Length;
                                if (num2 < 1L)
                                {
                                    num2 = 1L;
                                }
                                if (num2 > 0xfa0L)
                                {
                                    num2 = Max;
                                }
                            }
                            else
                            {
                                num2 = sxm_rgDefaults[12].MaxLength;
                            }
                            return new SqlMetaData(name, System.Data.SqlDbType.NVarChar, num2);
                        }
                        if (type == typeof(SqlBytes))
                        {
                            long num;
                            SqlBytes bytes = (SqlBytes) value;
                            if (!bytes.IsNull)
                            {
                                num = bytes.Length;
                                if (num < 1L)
                                {
                                    num = 1L;
                                }
                                else if (0x1f40L < num)
                                {
                                    num = Max;
                                }
                            }
                            else
                            {
                                num = sxm_rgDefaults[0x15].MaxLength;
                            }
                            return new SqlMetaData(name, System.Data.SqlDbType.VarBinary, num);
                        }
                        if (type == typeof(SqlXml))
                        {
                            return new SqlMetaData(name, System.Data.SqlDbType.Xml);
                        }
                        if (type == typeof(TimeSpan))
                        {
                            TimeSpan span = (TimeSpan) value;
                            return new SqlMetaData(name, System.Data.SqlDbType.Time, 0, InferScaleFromTimeTicks(span.Ticks));
                        }
                        if (type != typeof(DateTimeOffset))
                        {
                            throw ADP.UnknownDataType(type);
                        }
                        DateTimeOffset offset = (DateTimeOffset) value;
                        return new SqlMetaData(name, System.Data.SqlDbType.DateTimeOffset, 0, InferScaleFromTimeTicks(offset.Ticks));
                    }
                    long length = ((byte[]) value).Length;
                    if (length < 1L)
                    {
                        length = 1L;
                    }
                    if (0x1f40L < length)
                    {
                        length = Max;
                    }
                    return new SqlMetaData(name, System.Data.SqlDbType.VarBinary, length);
                }
                case TypeCode.DBNull:
                    throw ADP.InvalidDataType(TypeCode.DBNull);

                case TypeCode.Boolean:
                    return new SqlMetaData(name, System.Data.SqlDbType.Bit);

                case TypeCode.Char:
                    return new SqlMetaData(name, System.Data.SqlDbType.NVarChar, 1L);

                case TypeCode.SByte:
                    throw ADP.InvalidDataType(TypeCode.SByte);

                case TypeCode.Byte:
                    return new SqlMetaData(name, System.Data.SqlDbType.TinyInt);

                case TypeCode.Int16:
                    return new SqlMetaData(name, System.Data.SqlDbType.SmallInt);

                case TypeCode.UInt16:
                    throw ADP.InvalidDataType(TypeCode.UInt16);

                case TypeCode.Int32:
                    return new SqlMetaData(name, System.Data.SqlDbType.Int);

                case TypeCode.UInt32:
                    throw ADP.InvalidDataType(TypeCode.UInt32);

                case TypeCode.Int64:
                    return new SqlMetaData(name, System.Data.SqlDbType.BigInt);

                case TypeCode.UInt64:
                    throw ADP.InvalidDataType(TypeCode.UInt64);

                case TypeCode.Single:
                    return new SqlMetaData(name, System.Data.SqlDbType.Real);

                case TypeCode.Double:
                    return new SqlMetaData(name, System.Data.SqlDbType.Float);

                case TypeCode.Decimal:
                {
                    SqlDecimal num11 = new SqlDecimal((decimal) value);
                    return new SqlMetaData(name, System.Data.SqlDbType.Decimal, num11.Precision, num11.Scale);
                }
                case TypeCode.DateTime:
                    return new SqlMetaData(name, System.Data.SqlDbType.DateTime);

                case TypeCode.String:
                {
                    long num7 = ((string) value).Length;
                    if (num7 < 1L)
                    {
                        num7 = 1L;
                    }
                    if (0xfa0L < num7)
                    {
                        num7 = Max;
                    }
                    return new SqlMetaData(name, System.Data.SqlDbType.NVarChar, num7);
                }
            }
            throw ADP.UnknownDataTypeCode(type, System.Type.GetTypeCode(type));
        }

        private static byte InferScaleFromTimeTicks(long ticks)
        {
            for (byte i = 0; i < 7; i = (byte) (i + 1))
            {
                if (((ticks / __unitTicksFromScale[i]) * __unitTicksFromScale[i]) == ticks)
                {
                    return i;
                }
            }
            return 7;
        }

        private SqlDecimal InternalAdjustSqlDecimal(SqlDecimal value)
        {
            if (value.IsNull || ((value.Precision == this.Precision) && (value.Scale == this.Scale)))
            {
                return value;
            }
            if (value.Scale != this.Scale)
            {
                value = SqlDecimal.AdjustScale(value, this.Scale - value.Scale, false);
            }
            return SqlDecimal.ConvertToPrecScale(value, this.Precision, this.Scale);
        }

        private long InternalAdjustTimeTicks(long ticks)
        {
            return ((ticks / __unitTicksFromScale[this.Scale]) * __unitTicksFromScale[this.Scale]);
        }

        private void SetDefaultsForType(System.Data.SqlDbType dbType)
        {
            if ((System.Data.SqlDbType.BigInt <= dbType) && (System.Data.SqlDbType.DateTimeOffset >= dbType))
            {
                SqlMetaData data = sxm_rgDefaults[(int) dbType];
                this.m_sqlDbType = dbType;
                this.m_lMaxLength = data.MaxLength;
                this.m_bPrecision = data.Precision;
                this.m_bScale = data.Scale;
                this.m_lLocale = data.LocaleId;
                this.m_eCompareOptions = data.CompareOptions;
            }
        }

        private static void ThrowInvalidType()
        {
            throw ADP.InvalidMetaDataValue();
        }

        private void ValidateSortOrder(System.Data.SqlClient.SortOrder columnSortOrder, int sortOrdinal)
        {
            if (((System.Data.SqlClient.SortOrder.Unspecified != columnSortOrder) && (columnSortOrder != System.Data.SqlClient.SortOrder.Ascending)) && (System.Data.SqlClient.SortOrder.Descending != columnSortOrder))
            {
                throw SQL.InvalidSortOrder(columnSortOrder);
            }
            if ((System.Data.SqlClient.SortOrder.Unspecified == columnSortOrder) != (-1 == sortOrdinal))
            {
                throw SQL.MustSpecifyBothSortOrderAndOrdinal(columnSortOrder, sortOrdinal);
            }
        }

        private void VerifyDateTimeRange(DateTime value)
        {
            if ((System.Data.SqlDbType.SmallDateTime == this.SqlDbType) && ((x_dtSmallMax < value) || (x_dtSmallMin > value)))
            {
                ThrowInvalidType();
            }
        }

        private void VerifyMoneyRange(SqlMoney value)
        {
            if (System.Data.SqlDbType.SmallMoney == this.SqlDbType)
            {
                SqlBoolean flag2 = x_smSmallMax < value;
                if (!flag2.Value)
                {
                    SqlBoolean flag = x_smSmallMin > value;
                    if (!flag.Value)
                    {
                        return;
                    }
                }
                ThrowInvalidType();
            }
        }

        private void VerifyTimeRange(TimeSpan value)
        {
            if ((System.Data.SqlDbType.Time == this.SqlDbType) && ((x_timeMin > value) || (value > x_timeMax)))
            {
                ThrowInvalidType();
            }
        }

        public SqlCompareOptions CompareOptions
        {
            get
            {
                return this.m_eCompareOptions;
            }
        }

        public System.Data.DbType DbType
        {
            get
            {
                return sxm_rgSqlDbTypeToDbType[(int) this.m_sqlDbType];
            }
        }

        internal bool IsPartialLength
        {
            get
            {
                return this.m_bPartialLength;
            }
        }

        public bool IsUniqueKey
        {
            get
            {
                return this.m_isUniqueKey;
            }
        }

        public long LocaleId
        {
            get
            {
                return this.m_lLocale;
            }
        }

        public static long Max
        {
            get
            {
                return -1L;
            }
        }

        public long MaxLength
        {
            get
            {
                return this.m_lMaxLength;
            }
        }

        public string Name
        {
            get
            {
                return this.m_strName;
            }
        }

        public byte Precision
        {
            get
            {
                return this.m_bPrecision;
            }
        }

        public byte Scale
        {
            get
            {
                return this.m_bScale;
            }
        }

        internal string ServerTypeName
        {
            get
            {
                return this.m_serverTypeName;
            }
        }

        public System.Data.SqlClient.SortOrder SortOrder
        {
            get
            {
                return this.m_columnSortOrder;
            }
        }

        public int SortOrdinal
        {
            get
            {
                return this.m_sortOrdinal;
            }
        }

        public System.Data.SqlDbType SqlDbType
        {
            get
            {
                return this.m_sqlDbType;
            }
        }

        public System.Type Type
        {
            get
            {
                return this.m_udttype;
            }
        }

        public string TypeName
        {
            get
            {
                if (this.m_serverTypeName != null)
                {
                    return this.m_serverTypeName;
                }
                if (this.SqlDbType == System.Data.SqlDbType.Udt)
                {
                    return this.UdtTypeName;
                }
                return sxm_rgDefaults[(int) this.SqlDbType].Name;
            }
        }

        internal string UdtTypeName
        {
            get
            {
                if (this.SqlDbType != System.Data.SqlDbType.Udt)
                {
                    return null;
                }
                if (this.m_udttype == null)
                {
                    return null;
                }
                return this.m_udttype.FullName;
            }
        }

        public bool UseServerDefault
        {
            get
            {
                return this.m_useServerDefault;
            }
        }

        public string XmlSchemaCollectionDatabase
        {
            get
            {
                return this.m_XmlSchemaCollectionDatabase;
            }
        }

        public string XmlSchemaCollectionName
        {
            get
            {
                return this.m_XmlSchemaCollectionName;
            }
        }

        public string XmlSchemaCollectionOwningSchema
        {
            get
            {
                return this.m_XmlSchemaCollectionOwningSchema;
            }
        }
    }
}

