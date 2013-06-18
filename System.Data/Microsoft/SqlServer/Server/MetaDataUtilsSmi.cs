namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Globalization;

    internal class MetaDataUtilsSmi
    {
        private static readonly SqlDbType[] __extendedTypeCodeToSqlDbTypeMap;
        private static readonly Hashtable __typeToExtendedTypeCodeMap;
        internal const long InvalidMaxLength = -2L;
        internal const SqlDbType InvalidSqlDbType = ~SqlDbType.BigInt;

        static MetaDataUtilsSmi()
        {
            SqlDbType[] typeArray = new SqlDbType[0x2b];
            typeArray[0] = ~SqlDbType.BigInt;
            typeArray[1] = SqlDbType.Bit;
            typeArray[2] = SqlDbType.TinyInt;
            typeArray[3] = SqlDbType.NVarChar;
            typeArray[4] = SqlDbType.DateTime;
            typeArray[5] = ~SqlDbType.BigInt;
            typeArray[6] = SqlDbType.Decimal;
            typeArray[7] = SqlDbType.Float;
            typeArray[8] = ~SqlDbType.BigInt;
            typeArray[9] = SqlDbType.SmallInt;
            typeArray[10] = SqlDbType.Int;
            typeArray[12] = ~SqlDbType.BigInt;
            typeArray[13] = SqlDbType.Real;
            typeArray[14] = SqlDbType.NVarChar;
            typeArray[15] = ~SqlDbType.BigInt;
            typeArray[0x10] = ~SqlDbType.BigInt;
            typeArray[0x11] = ~SqlDbType.BigInt;
            typeArray[0x12] = ~SqlDbType.BigInt;
            typeArray[0x13] = SqlDbType.VarBinary;
            typeArray[20] = SqlDbType.NVarChar;
            typeArray[0x15] = SqlDbType.UniqueIdentifier;
            typeArray[0x16] = SqlDbType.VarBinary;
            typeArray[0x17] = SqlDbType.Bit;
            typeArray[0x18] = SqlDbType.TinyInt;
            typeArray[0x19] = SqlDbType.DateTime;
            typeArray[0x1a] = SqlDbType.Float;
            typeArray[0x1b] = SqlDbType.UniqueIdentifier;
            typeArray[0x1c] = SqlDbType.SmallInt;
            typeArray[0x1d] = SqlDbType.Int;
            typeArray[0x1f] = SqlDbType.Money;
            typeArray[0x20] = SqlDbType.Decimal;
            typeArray[0x21] = SqlDbType.Real;
            typeArray[0x22] = SqlDbType.NVarChar;
            typeArray[0x23] = SqlDbType.NVarChar;
            typeArray[0x24] = SqlDbType.VarBinary;
            typeArray[0x25] = SqlDbType.Xml;
            typeArray[0x26] = SqlDbType.Structured;
            typeArray[0x27] = SqlDbType.Structured;
            typeArray[40] = SqlDbType.Structured;
            typeArray[0x29] = SqlDbType.Time;
            typeArray[0x2a] = SqlDbType.DateTimeOffset;
            __extendedTypeCodeToSqlDbTypeMap = typeArray;
            Hashtable hashtable = new Hashtable(0x2a);
            hashtable.Add(typeof(bool), ExtendedClrTypeCode.Boolean);
            hashtable.Add(typeof(byte), ExtendedClrTypeCode.Byte);
            hashtable.Add(typeof(char), ExtendedClrTypeCode.Char);
            hashtable.Add(typeof(DateTime), ExtendedClrTypeCode.DateTime);
            hashtable.Add(typeof(DBNull), ExtendedClrTypeCode.DBNull);
            hashtable.Add(typeof(decimal), ExtendedClrTypeCode.Decimal);
            hashtable.Add(typeof(double), ExtendedClrTypeCode.Double);
            hashtable.Add(typeof(short), ExtendedClrTypeCode.Int16);
            hashtable.Add(typeof(int), ExtendedClrTypeCode.Int32);
            hashtable.Add(typeof(long), ExtendedClrTypeCode.Int64);
            hashtable.Add(typeof(sbyte), ExtendedClrTypeCode.SByte);
            hashtable.Add(typeof(float), ExtendedClrTypeCode.Single);
            hashtable.Add(typeof(string), ExtendedClrTypeCode.String);
            hashtable.Add(typeof(ushort), ExtendedClrTypeCode.UInt16);
            hashtable.Add(typeof(uint), ExtendedClrTypeCode.UInt32);
            hashtable.Add(typeof(ulong), ExtendedClrTypeCode.UInt64);
            hashtable.Add(typeof(object), ExtendedClrTypeCode.Object);
            hashtable.Add(typeof(byte[]), ExtendedClrTypeCode.ByteArray);
            hashtable.Add(typeof(char[]), ExtendedClrTypeCode.CharArray);
            hashtable.Add(typeof(Guid), ExtendedClrTypeCode.Guid);
            hashtable.Add(typeof(SqlBinary), ExtendedClrTypeCode.SqlBinary);
            hashtable.Add(typeof(SqlBoolean), ExtendedClrTypeCode.SqlBoolean);
            hashtable.Add(typeof(SqlByte), ExtendedClrTypeCode.SqlByte);
            hashtable.Add(typeof(SqlDateTime), ExtendedClrTypeCode.SqlDateTime);
            hashtable.Add(typeof(SqlDouble), ExtendedClrTypeCode.SqlDouble);
            hashtable.Add(typeof(SqlGuid), ExtendedClrTypeCode.SqlGuid);
            hashtable.Add(typeof(SqlInt16), ExtendedClrTypeCode.SqlInt16);
            hashtable.Add(typeof(SqlInt32), ExtendedClrTypeCode.SqlInt32);
            hashtable.Add(typeof(SqlInt64), ExtendedClrTypeCode.SqlInt64);
            hashtable.Add(typeof(SqlMoney), ExtendedClrTypeCode.SqlMoney);
            hashtable.Add(typeof(SqlDecimal), ExtendedClrTypeCode.SqlDecimal);
            hashtable.Add(typeof(SqlSingle), ExtendedClrTypeCode.SqlSingle);
            hashtable.Add(typeof(SqlString), ExtendedClrTypeCode.SqlString);
            hashtable.Add(typeof(SqlChars), ExtendedClrTypeCode.SqlChars);
            hashtable.Add(typeof(SqlBytes), ExtendedClrTypeCode.SqlBytes);
            hashtable.Add(typeof(SqlXml), ExtendedClrTypeCode.SqlXml);
            hashtable.Add(typeof(DataTable), ExtendedClrTypeCode.DataTable);
            hashtable.Add(typeof(DbDataReader), ExtendedClrTypeCode.DbDataReader);
            hashtable.Add(typeof(IEnumerable<SqlDataRecord>), ExtendedClrTypeCode.IEnumerableOfSqlDataRecord);
            hashtable.Add(typeof(TimeSpan), ExtendedClrTypeCode.TimeSpan);
            hashtable.Add(typeof(DateTimeOffset), ExtendedClrTypeCode.DateTimeOffset);
            __typeToExtendedTypeCodeMap = hashtable;
        }

        internal static long AdjustMaxLength(SqlDbType dbType, long maxLength)
        {
            if (-1L != maxLength)
            {
                if (maxLength < 0L)
                {
                    maxLength = -2L;
                }
                switch (dbType)
                {
                    case SqlDbType.Binary:
                        if (maxLength > 0x1f40L)
                        {
                            maxLength = -2L;
                        }
                        return maxLength;

                    case SqlDbType.Bit:
                        return maxLength;

                    case SqlDbType.Char:
                        if (maxLength > 0x1f40L)
                        {
                            maxLength = -2L;
                        }
                        return maxLength;

                    case SqlDbType.NChar:
                        if (maxLength > 0xfa0L)
                        {
                            maxLength = -2L;
                        }
                        return maxLength;

                    case SqlDbType.NText:
                        return maxLength;

                    case SqlDbType.NVarChar:
                        if (0xfa0L < maxLength)
                        {
                            maxLength = -1L;
                        }
                        return maxLength;

                    case SqlDbType.VarBinary:
                        if (0x1f40L < maxLength)
                        {
                            maxLength = -1L;
                        }
                        return maxLength;

                    case SqlDbType.VarChar:
                        if (0x1f40L < maxLength)
                        {
                            maxLength = -1L;
                        }
                        return maxLength;
                }
            }
            return maxLength;
        }

        internal static ExtendedClrTypeCode DetermineExtendedTypeCode(object value)
        {
            if (value == null)
            {
                return ExtendedClrTypeCode.Empty;
            }
            return DetermineExtendedTypeCodeFromType(value.GetType());
        }

        internal static ExtendedClrTypeCode DetermineExtendedTypeCodeForUseWithSqlDbType(SqlDbType dbType, bool isMultiValued, object value, Type udtType, ulong smiVersion)
        {
            ExtendedClrTypeCode invalid = ExtendedClrTypeCode.Invalid;
            if (value == null)
            {
                return ExtendedClrTypeCode.Empty;
            }
            if (DBNull.Value == value)
            {
                return ExtendedClrTypeCode.DBNull;
            }
            switch (dbType)
            {
                case SqlDbType.BigInt:
                    if (!(value.GetType() == typeof(long)))
                    {
                        if (value.GetType() == typeof(SqlInt64))
                        {
                            return ExtendedClrTypeCode.SqlInt64;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Int64)
                        {
                            invalid = ExtendedClrTypeCode.Int64;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.Int64;

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    if (!(value.GetType() == typeof(byte[])))
                    {
                        if (value.GetType() == typeof(SqlBinary))
                        {
                            return ExtendedClrTypeCode.SqlBinary;
                        }
                        if (value.GetType() == typeof(SqlBytes))
                        {
                            invalid = ExtendedClrTypeCode.SqlBytes;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.ByteArray;

                case SqlDbType.Bit:
                    if (!(value.GetType() == typeof(bool)))
                    {
                        if (value.GetType() == typeof(SqlBoolean))
                        {
                            return ExtendedClrTypeCode.SqlBoolean;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Boolean)
                        {
                            invalid = ExtendedClrTypeCode.Boolean;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.Boolean;

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                    if (!(value.GetType() == typeof(string)))
                    {
                        if (value.GetType() == typeof(SqlString))
                        {
                            return ExtendedClrTypeCode.SqlString;
                        }
                        if (value.GetType() == typeof(char[]))
                        {
                            return ExtendedClrTypeCode.CharArray;
                        }
                        if (value.GetType() == typeof(SqlChars))
                        {
                            return ExtendedClrTypeCode.SqlChars;
                        }
                        if (value.GetType() == typeof(char))
                        {
                            return ExtendedClrTypeCode.Char;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Char)
                        {
                            return ExtendedClrTypeCode.Char;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.String)
                        {
                            invalid = ExtendedClrTypeCode.String;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.String;

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    break;

                case SqlDbType.Decimal:
                    if (!(value.GetType() == typeof(decimal)))
                    {
                        if (value.GetType() == typeof(SqlDecimal))
                        {
                            return ExtendedClrTypeCode.SqlDecimal;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Decimal)
                        {
                            invalid = ExtendedClrTypeCode.Decimal;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.Decimal;

                case SqlDbType.Float:
                    if (!(value.GetType() == typeof(SqlDouble)))
                    {
                        if (value.GetType() == typeof(double))
                        {
                            return ExtendedClrTypeCode.Double;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Double)
                        {
                            invalid = ExtendedClrTypeCode.Double;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.SqlDouble;

                case SqlDbType.Int:
                    if (!(value.GetType() == typeof(int)))
                    {
                        if (value.GetType() == typeof(SqlInt32))
                        {
                            return ExtendedClrTypeCode.SqlInt32;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Int32)
                        {
                            invalid = ExtendedClrTypeCode.Int32;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.Int32;

                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    if (!(value.GetType() == typeof(SqlMoney)))
                    {
                        if (value.GetType() == typeof(decimal))
                        {
                            return ExtendedClrTypeCode.Decimal;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Decimal)
                        {
                            invalid = ExtendedClrTypeCode.Decimal;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.SqlMoney;

                case SqlDbType.Real:
                    if (!(value.GetType() == typeof(float)))
                    {
                        if (value.GetType() == typeof(SqlSingle))
                        {
                            return ExtendedClrTypeCode.SqlSingle;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Single)
                        {
                            invalid = ExtendedClrTypeCode.Single;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.Single;

                case SqlDbType.UniqueIdentifier:
                    if (!(value.GetType() == typeof(SqlGuid)))
                    {
                        if (value.GetType() == typeof(Guid))
                        {
                            invalid = ExtendedClrTypeCode.Guid;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.SqlGuid;

                case SqlDbType.SmallInt:
                    if (!(value.GetType() == typeof(short)))
                    {
                        if (value.GetType() == typeof(SqlInt16))
                        {
                            return ExtendedClrTypeCode.SqlInt16;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Int16)
                        {
                            invalid = ExtendedClrTypeCode.Int16;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.Int16;

                case SqlDbType.TinyInt:
                    if (!(value.GetType() == typeof(byte)))
                    {
                        if (value.GetType() == typeof(SqlByte))
                        {
                            return ExtendedClrTypeCode.SqlByte;
                        }
                        if (Type.GetTypeCode(value.GetType()) == TypeCode.Byte)
                        {
                            invalid = ExtendedClrTypeCode.Byte;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.Byte;

                case SqlDbType.Variant:
                    invalid = DetermineExtendedTypeCode(value);
                    if (ExtendedClrTypeCode.SqlXml == invalid)
                    {
                        invalid = ExtendedClrTypeCode.Invalid;
                    }
                    return invalid;

                case (SqlDbType.SmallInt | SqlDbType.Int):
                case (SqlDbType.Text | SqlDbType.Int):
                case (SqlDbType.Xml | SqlDbType.Bit):
                case (SqlDbType.TinyInt | SqlDbType.Int):
                    return invalid;

                case SqlDbType.Xml:
                    if (!(value.GetType() == typeof(SqlXml)))
                    {
                        if (value.GetType() == typeof(string))
                        {
                            invalid = ExtendedClrTypeCode.String;
                        }
                        return invalid;
                    }
                    return ExtendedClrTypeCode.SqlXml;

                case SqlDbType.Udt:
                    if ((null != udtType) && !(value.GetType() == udtType))
                    {
                        return ExtendedClrTypeCode.Invalid;
                    }
                    return ExtendedClrTypeCode.Object;

                case SqlDbType.Structured:
                    if (isMultiValued)
                    {
                        if (!(value is DataTable))
                        {
                            if (value is IEnumerable<SqlDataRecord>)
                            {
                                return ExtendedClrTypeCode.IEnumerableOfSqlDataRecord;
                            }
                            if (value is DbDataReader)
                            {
                                invalid = ExtendedClrTypeCode.DbDataReader;
                            }
                            return invalid;
                        }
                        invalid = ExtendedClrTypeCode.DataTable;
                    }
                    return invalid;

                case SqlDbType.Date:
                case SqlDbType.DateTime2:
                    if (smiVersion < 210L)
                    {
                        return invalid;
                    }
                    break;

                case SqlDbType.Time:
                    if ((value.GetType() == typeof(TimeSpan)) && (smiVersion >= 210L))
                    {
                        invalid = ExtendedClrTypeCode.TimeSpan;
                    }
                    return invalid;

                case SqlDbType.DateTimeOffset:
                    if ((value.GetType() == typeof(DateTimeOffset)) && (smiVersion >= 210L))
                    {
                        invalid = ExtendedClrTypeCode.DateTimeOffset;
                    }
                    return invalid;

                default:
                    return invalid;
            }
            if (value.GetType() == typeof(DateTime))
            {
                return ExtendedClrTypeCode.DateTime;
            }
            if (value.GetType() == typeof(SqlDateTime))
            {
                return ExtendedClrTypeCode.SqlDateTime;
            }
            if (Type.GetTypeCode(value.GetType()) == TypeCode.DateTime)
            {
                invalid = ExtendedClrTypeCode.DateTime;
            }
            return invalid;
        }

        internal static ExtendedClrTypeCode DetermineExtendedTypeCodeFromType(Type clrType)
        {
            object obj2 = __typeToExtendedTypeCodeMap[clrType];
            if (obj2 == null)
            {
                return ExtendedClrTypeCode.Invalid;
            }
            return (ExtendedClrTypeCode) obj2;
        }

        internal static SqlDbType InferSqlDbTypeFromType(Type type)
        {
            ExtendedClrTypeCode typeCode = DetermineExtendedTypeCodeFromType(type);
            if (ExtendedClrTypeCode.Invalid == typeCode)
            {
                return ~SqlDbType.BigInt;
            }
            return InferSqlDbTypeFromTypeCode(typeCode);
        }

        internal static SqlDbType InferSqlDbTypeFromType_Katmai(Type type)
        {
            SqlDbType type2 = InferSqlDbTypeFromType(type);
            if (SqlDbType.DateTime == type2)
            {
                type2 = SqlDbType.DateTime2;
            }
            return type2;
        }

        internal static SqlDbType InferSqlDbTypeFromTypeCode(ExtendedClrTypeCode typeCode)
        {
            return __extendedTypeCodeToSqlDbTypeMap[((int) typeCode) + 1];
        }

        internal static bool IsAnsiType(SqlDbType type)
        {
            if ((type != SqlDbType.Char) && (type != SqlDbType.VarChar))
            {
                return (type == SqlDbType.Text);
            }
            return true;
        }

        internal static bool IsBinaryType(SqlDbType type)
        {
            if ((type != SqlDbType.Binary) && (type != SqlDbType.VarBinary))
            {
                return (type == SqlDbType.Image);
            }
            return true;
        }

        internal static bool IsCharOrXmlType(SqlDbType type)
        {
            if (!IsUnicodeType(type) && !IsAnsiType(type))
            {
                return (type == SqlDbType.Xml);
            }
            return true;
        }

        internal static bool IsCompatible(SmiMetaData firstMd, SqlMetaData secondMd)
        {
            return (((((firstMd.SqlDbType == secondMd.SqlDbType) && (firstMd.MaxLength == secondMd.MaxLength)) && ((firstMd.Precision == secondMd.Precision) && (firstMd.Scale == secondMd.Scale))) && (((firstMd.CompareOptions == secondMd.CompareOptions) && (firstMd.LocaleId == secondMd.LocaleId)) && ((firstMd.Type == secondMd.Type) && (firstMd.SqlDbType != SqlDbType.Structured)))) && !firstMd.IsMultiValued);
        }

        internal static bool IsPlpFormat(SmiMetaData metaData)
        {
            if (((metaData.MaxLength != -1L) && (metaData.SqlDbType != SqlDbType.Image)) && ((metaData.SqlDbType != SqlDbType.NText) && (metaData.SqlDbType != SqlDbType.Text)))
            {
                return (metaData.SqlDbType == SqlDbType.Udt);
            }
            return true;
        }

        internal static bool IsUnicodeType(SqlDbType type)
        {
            if ((type != SqlDbType.NChar) && (type != SqlDbType.NVarChar))
            {
                return (type == SqlDbType.NText);
            }
            return true;
        }

        internal static bool IsValidForSmiVersion(SmiExtendedMetaData md, ulong smiVersion)
        {
            return ((210L == smiVersion) || ((((md.SqlDbType != SqlDbType.Structured) && (md.SqlDbType != SqlDbType.Date)) && ((md.SqlDbType != SqlDbType.DateTime2) && (md.SqlDbType != SqlDbType.DateTimeOffset))) && (md.SqlDbType != SqlDbType.Time)));
        }

        internal static SqlMetaData SmiExtendedMetaDataToSqlMetaData(SmiExtendedMetaData source)
        {
            if (SqlDbType.Xml == source.SqlDbType)
            {
                return new SqlMetaData(source.Name, source.SqlDbType, source.MaxLength, source.Precision, source.Scale, source.LocaleId, source.CompareOptions, source.TypeSpecificNamePart1, source.TypeSpecificNamePart2, source.TypeSpecificNamePart3, true, source.Type);
            }
            return new SqlMetaData(source.Name, source.SqlDbType, source.MaxLength, source.Precision, source.Scale, source.LocaleId, source.CompareOptions, source.Type);
        }

        internal static SmiExtendedMetaData SmiMetaDataFromDataColumn(DataColumn column, DataTable parent)
        {
            byte num;
            byte scale;
            SqlDbType dbType = InferSqlDbTypeFromType_Katmai(column.DataType);
            if (~SqlDbType.BigInt == dbType)
            {
                throw SQL.UnsupportedColumnTypeForSqlProvider(column.ColumnName, column.DataType.Name);
            }
            long maxLength = AdjustMaxLength(dbType, (long) column.MaxLength);
            if (-2L == maxLength)
            {
                throw SQL.InvalidColumnMaxLength(column.ColumnName, maxLength);
            }
            if (column.DataType == typeof(SqlDecimal))
            {
                scale = 0;
                byte num4 = 0;
                foreach (DataRow row2 in parent.Rows)
                {
                    object obj3 = row2[column];
                    if (!(obj3 is DBNull))
                    {
                        SqlDecimal num9 = (SqlDecimal) obj3;
                        if (!num9.IsNull)
                        {
                            byte num7 = (byte) (num9.Precision - num9.Scale);
                            if (num7 > num4)
                            {
                                num4 = num7;
                            }
                            if (num9.Scale > scale)
                            {
                                scale = num9.Scale;
                            }
                        }
                    }
                }
                num = (byte) (num4 + scale);
                if (SqlDecimal.MaxPrecision < num)
                {
                    throw SQL.InvalidTableDerivedPrecisionForTvp(column.ColumnName, num);
                }
                if (num == 0)
                {
                    num = 1;
                }
            }
            else
            {
                switch (dbType)
                {
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                    case SqlDbType.Time:
                        num = 0;
                        scale = SmiMetaData.DefaultTime.Scale;
                        goto Label_01FD;

                    case SqlDbType.Decimal:
                    {
                        scale = 0;
                        byte num3 = 0;
                        foreach (DataRow row in parent.Rows)
                        {
                            object obj2 = row[column];
                            if (!(obj2 is DBNull))
                            {
                                SqlDecimal num8 = (decimal) obj2;
                                byte num6 = (byte) (num8.Precision - num8.Scale);
                                if (num6 > num3)
                                {
                                    num3 = num6;
                                }
                                if (num8.Scale > scale)
                                {
                                    scale = num8.Scale;
                                }
                            }
                        }
                        num = (byte) (num3 + scale);
                        if (SqlDecimal.MaxPrecision < num)
                        {
                            throw SQL.InvalidTableDerivedPrecisionForTvp(column.ColumnName, num);
                        }
                        if (num == 0)
                        {
                            num = 1;
                        }
                        goto Label_01FD;
                    }
                }
                num = 0;
                scale = 0;
            }
        Label_01FD:
            return new SmiExtendedMetaData(dbType, maxLength, num, scale, (long) column.Locale.LCID, SmiMetaData.DefaultNVarChar.CompareOptions, column.DataType, false, null, null, column.ColumnName, null, null, null);
        }

        internal static SmiExtendedMetaData SmiMetaDataFromSchemaTableRow(DataRow schemaRow)
        {
            string columnName = "";
            object obj2 = schemaRow[SchemaTableColumn.ColumnName];
            if (DBNull.Value != obj2)
            {
                columnName = (string) obj2;
            }
            obj2 = schemaRow[SchemaTableColumn.DataType];
            if (DBNull.Value == obj2)
            {
                throw SQL.NullSchemaTableDataTypeNotSupported(columnName);
            }
            Type type2 = (Type) obj2;
            SqlDbType dbType = InferSqlDbTypeFromType_Katmai(type2);
            if (~SqlDbType.BigInt == dbType)
            {
                if (typeof(object) != type2)
                {
                    throw SQL.UnsupportedColumnTypeForSqlProvider(columnName, type2.ToString());
                }
                dbType = SqlDbType.VarBinary;
            }
            long maxLength = 0L;
            byte precision = 0;
            byte scale = 0;
            switch (dbType)
            {
                case SqlDbType.BigInt:
                case SqlDbType.Bit:
                case SqlDbType.DateTime:
                case SqlDbType.Float:
                case SqlDbType.Image:
                case SqlDbType.Int:
                case SqlDbType.Money:
                case SqlDbType.NText:
                case SqlDbType.Real:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.SmallDateTime:
                case SqlDbType.SmallInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Text:
                case SqlDbType.Timestamp:
                case SqlDbType.TinyInt:
                case SqlDbType.Variant:
                case SqlDbType.Xml:
                case SqlDbType.Date:
                    goto Label_0302;

                case SqlDbType.Binary:
                case SqlDbType.VarBinary:
                    obj2 = schemaRow[SchemaTableColumn.ColumnSize];
                    if (DBNull.Value != obj2)
                    {
                        maxLength = Convert.ToInt64(obj2, null);
                        if (maxLength > 0x1f40L)
                        {
                            maxLength = -1L;
                        }
                        if ((maxLength < 0L) && ((maxLength != -1L) || (SqlDbType.Binary == dbType)))
                        {
                            throw SQL.InvalidColumnMaxLength(columnName, maxLength);
                        }
                    }
                    else if (SqlDbType.Binary != dbType)
                    {
                        maxLength = -1L;
                    }
                    else
                    {
                        maxLength = 0x1f40L;
                    }
                    goto Label_0302;

                case SqlDbType.Char:
                case SqlDbType.VarChar:
                    obj2 = schemaRow[SchemaTableColumn.ColumnSize];
                    if (DBNull.Value != obj2)
                    {
                        maxLength = Convert.ToInt64(obj2, null);
                        if (maxLength > 0x1f40L)
                        {
                            maxLength = -1L;
                        }
                        if ((maxLength < 0L) && ((maxLength != -1L) || (SqlDbType.Char == dbType)))
                        {
                            throw SQL.InvalidColumnMaxLength(columnName, maxLength);
                        }
                    }
                    else if (SqlDbType.Char != dbType)
                    {
                        maxLength = -1L;
                    }
                    else
                    {
                        maxLength = 0x1f40L;
                    }
                    goto Label_0302;

                case SqlDbType.Decimal:
                    obj2 = schemaRow[SchemaTableColumn.NumericPrecision];
                    if (DBNull.Value != obj2)
                    {
                        precision = Convert.ToByte(obj2, null);
                        break;
                    }
                    precision = SmiMetaData.DefaultDecimal.Precision;
                    break;

                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                    obj2 = schemaRow[SchemaTableColumn.ColumnSize];
                    if (DBNull.Value != obj2)
                    {
                        maxLength = Convert.ToInt64(obj2, null);
                        if (maxLength > 0xfa0L)
                        {
                            maxLength = -1L;
                        }
                        if ((maxLength < 0L) && ((maxLength != -1L) || (SqlDbType.NChar == dbType)))
                        {
                            throw SQL.InvalidColumnMaxLength(columnName, maxLength);
                        }
                    }
                    else if (SqlDbType.NChar != dbType)
                    {
                        maxLength = -1L;
                    }
                    else
                    {
                        maxLength = 0xfa0L;
                    }
                    goto Label_0302;

                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    obj2 = schemaRow[SchemaTableColumn.NumericScale];
                    if (DBNull.Value != obj2)
                    {
                        scale = Convert.ToByte(obj2, null);
                    }
                    else
                    {
                        scale = SmiMetaData.DefaultTime.Scale;
                    }
                    if (scale > 7)
                    {
                        throw SQL.InvalidColumnPrecScale();
                    }
                    if (scale < 0)
                    {
                        scale = SmiMetaData.DefaultTime.Scale;
                    }
                    goto Label_0302;

                default:
                    throw SQL.UnsupportedColumnTypeForSqlProvider(columnName, type2.ToString());
            }
            obj2 = schemaRow[SchemaTableColumn.NumericScale];
            if (DBNull.Value == obj2)
            {
                scale = SmiMetaData.DefaultDecimal.Scale;
            }
            else
            {
                scale = Convert.ToByte(obj2, null);
            }
            if (((precision < 1) || (precision > SqlDecimal.MaxPrecision)) || (((scale < 0) || (scale > SqlDecimal.MaxScale)) || (scale > precision)))
            {
                throw SQL.InvalidColumnPrecScale();
            }
        Label_0302:
            return new SmiExtendedMetaData(dbType, maxLength, precision, scale, (long) CultureInfo.CurrentCulture.LCID, SmiMetaData.GetDefaultForType(dbType).CompareOptions, null, false, null, null, columnName, null, null, null);
        }

        internal static SmiExtendedMetaData SqlMetaDataToSmiExtendedMetaData(SqlMetaData source)
        {
            string xmlSchemaCollectionName = null;
            string xmlSchemaCollectionOwningSchema = null;
            string xmlSchemaCollectionDatabase = null;
            if (SqlDbType.Xml == source.SqlDbType)
            {
                xmlSchemaCollectionDatabase = source.XmlSchemaCollectionDatabase;
                xmlSchemaCollectionOwningSchema = source.XmlSchemaCollectionOwningSchema;
                xmlSchemaCollectionName = source.XmlSchemaCollectionName;
            }
            else if (SqlDbType.Udt == source.SqlDbType)
            {
                string serverTypeName = source.ServerTypeName;
                if (serverTypeName != null)
                {
                    string[] strArray = SqlParameter.ParseTypeName(serverTypeName, true);
                    if (1 == strArray.Length)
                    {
                        xmlSchemaCollectionName = strArray[0];
                    }
                    else if (2 == strArray.Length)
                    {
                        xmlSchemaCollectionOwningSchema = strArray[0];
                        xmlSchemaCollectionName = strArray[1];
                    }
                    else
                    {
                        if (3 != strArray.Length)
                        {
                            throw ADP.ArgumentOutOfRange("typeName");
                        }
                        xmlSchemaCollectionDatabase = strArray[0];
                        xmlSchemaCollectionOwningSchema = strArray[1];
                        xmlSchemaCollectionName = strArray[2];
                    }
                    if (((!ADP.IsEmpty(xmlSchemaCollectionDatabase) && (0xff < xmlSchemaCollectionDatabase.Length)) || (!ADP.IsEmpty(xmlSchemaCollectionOwningSchema) && (0xff < xmlSchemaCollectionOwningSchema.Length))) || (!ADP.IsEmpty(xmlSchemaCollectionName) && (0xff < xmlSchemaCollectionName.Length)))
                    {
                        throw ADP.ArgumentOutOfRange("typeName");
                    }
                }
            }
            return new SmiExtendedMetaData(source.SqlDbType, source.MaxLength, source.Precision, source.Scale, source.LocaleId, source.CompareOptions, source.Type, source.Name, xmlSchemaCollectionDatabase, xmlSchemaCollectionOwningSchema, xmlSchemaCollectionName);
        }
    }
}

