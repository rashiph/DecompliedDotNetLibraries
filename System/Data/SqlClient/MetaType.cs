namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.Data.SqlTypes;
    using System.Xml;

    internal sealed class MetaType
    {
        internal readonly Type ClassType;
        internal readonly System.Data.DbType DbType;
        internal readonly int FixedLength;
        internal readonly bool Is100Supported;
        internal readonly bool Is70Supported;
        internal readonly bool Is80Supported;
        internal readonly bool Is90Supported;
        internal readonly bool IsAnsiType;
        internal readonly bool IsBinType;
        internal readonly bool IsCharType;
        internal readonly bool IsFixed;
        internal readonly bool IsLong;
        internal readonly bool IsNCharType;
        internal readonly bool IsPlp;
        internal readonly bool IsSizeInCharacters;
        private static readonly MetaType MetaBigInt = new MetaType(0x13, 0xff, 8, true, false, false, 0x7f, 0x26, "bigint", typeof(long), typeof(SqlInt64), System.Data.SqlDbType.BigInt, System.Data.DbType.Int64, 0);
        private static readonly MetaType MetaBinary = new MetaType(0xff, 0xff, -1, false, false, false, 0xad, 0xad, "binary", typeof(byte[]), typeof(SqlBinary), System.Data.SqlDbType.Binary, System.Data.DbType.Binary, 2);
        private static readonly MetaType MetaBit = new MetaType(0xff, 0xff, 1, true, false, false, 50, 0x68, "bit", typeof(bool), typeof(SqlBoolean), System.Data.SqlDbType.Bit, System.Data.DbType.Boolean, 0);
        private static readonly MetaType MetaChar = new MetaType(0xff, 0xff, -1, false, false, false, 0xaf, 0xaf, "char", typeof(string), typeof(SqlString), System.Data.SqlDbType.Char, System.Data.DbType.AnsiStringFixedLength, 7);
        private static readonly MetaType MetaDate = new MetaType(0xff, 0xff, 3, true, false, false, 40, 40, "date", typeof(DateTime), typeof(DateTime), System.Data.SqlDbType.Date, System.Data.DbType.Date, 0);
        private static readonly MetaType MetaDateTime = new MetaType(0x17, 3, 8, true, false, false, 0x3d, 0x6f, "datetime", typeof(DateTime), typeof(SqlDateTime), System.Data.SqlDbType.DateTime, System.Data.DbType.DateTime, 0);
        private static readonly MetaType MetaDateTime2 = new MetaType(0xff, 7, -1, false, false, false, 0x2a, 0x2a, "datetime2", typeof(DateTime), typeof(DateTime), System.Data.SqlDbType.DateTime2, System.Data.DbType.DateTime2, 1);
        internal static readonly MetaType MetaDateTimeOffset = new MetaType(0xff, 7, -1, false, false, false, 0x2b, 0x2b, "datetimeoffset", typeof(DateTimeOffset), typeof(DateTimeOffset), System.Data.SqlDbType.DateTimeOffset, System.Data.DbType.DateTimeOffset, 1);
        internal static readonly MetaType MetaDecimal = new MetaType(0x26, 4, 0x11, true, false, false, 0x6c, 0x6c, "decimal", typeof(decimal), typeof(SqlDecimal), System.Data.SqlDbType.Decimal, System.Data.DbType.Decimal, 2);
        private static readonly MetaType MetaFloat = new MetaType(15, 0xff, 8, true, false, false, 0x3e, 0x6d, "float", typeof(double), typeof(SqlDouble), System.Data.SqlDbType.Float, System.Data.DbType.Double, 0);
        internal static readonly MetaType MetaImage = new MetaType(0xff, 0xff, -1, false, true, false, 0x22, 0x22, "image", typeof(byte[]), typeof(SqlBinary), System.Data.SqlDbType.Image, System.Data.DbType.Binary, 0);
        private static readonly MetaType MetaInt = new MetaType(10, 0xff, 4, true, false, false, 0x38, 0x26, "int", typeof(int), typeof(SqlInt32), System.Data.SqlDbType.Int, System.Data.DbType.Int32, 0);
        internal static readonly MetaType MetaMaxNVarChar = new MetaType(0xff, 0xff, -1, false, true, true, 0xe7, 0xe7, "nvarchar", typeof(string), typeof(SqlString), System.Data.SqlDbType.NVarChar, System.Data.DbType.String, 7);
        private static readonly MetaType MetaMaxUdt = new MetaType(0xff, 0xff, -1, false, true, true, 240, 240, "udt", typeof(object), typeof(object), System.Data.SqlDbType.Udt, System.Data.DbType.Object, 0);
        internal static readonly MetaType MetaMaxVarBinary = new MetaType(0xff, 0xff, -1, false, true, true, 0xa5, 0xa5, "varbinary", typeof(byte[]), typeof(SqlBinary), System.Data.SqlDbType.VarBinary, System.Data.DbType.Binary, 2);
        internal static readonly MetaType MetaMaxVarChar = new MetaType(0xff, 0xff, -1, false, true, true, 0xa7, 0xa7, "varchar", typeof(string), typeof(SqlString), System.Data.SqlDbType.VarChar, System.Data.DbType.AnsiString, 7);
        private static readonly MetaType MetaMoney = new MetaType(0x13, 0xff, 8, true, false, false, 60, 110, "money", typeof(decimal), typeof(SqlMoney), System.Data.SqlDbType.Money, System.Data.DbType.Currency, 0);
        private static readonly MetaType MetaNChar = new MetaType(0xff, 0xff, -1, false, false, false, 0xef, 0xef, "nchar", typeof(string), typeof(SqlString), System.Data.SqlDbType.NChar, System.Data.DbType.StringFixedLength, 7);
        internal static readonly MetaType MetaNText = new MetaType(0xff, 0xff, -1, false, true, false, 0x63, 0x63, "ntext", typeof(string), typeof(SqlString), System.Data.SqlDbType.NText, System.Data.DbType.String, 7);
        internal static readonly MetaType MetaNVarChar = new MetaType(0xff, 0xff, -1, false, false, false, 0xe7, 0xe7, "nvarchar", typeof(string), typeof(SqlString), System.Data.SqlDbType.NVarChar, System.Data.DbType.String, 7);
        private static readonly MetaType MetaReal = new MetaType(7, 0xff, 4, true, false, false, 0x3b, 0x6d, "real", typeof(float), typeof(SqlSingle), System.Data.SqlDbType.Real, System.Data.DbType.Single, 0);
        private static readonly MetaType MetaSmallDateTime = new MetaType(0x10, 0, 4, true, false, false, 0x3a, 0x6f, "smalldatetime", typeof(DateTime), typeof(SqlDateTime), System.Data.SqlDbType.SmallDateTime, System.Data.DbType.DateTime, 0);
        private static readonly MetaType MetaSmallInt = new MetaType(5, 0xff, 2, true, false, false, 0x34, 0x26, "smallint", typeof(short), typeof(SqlInt16), System.Data.SqlDbType.SmallInt, System.Data.DbType.Int16, 0);
        private static readonly MetaType MetaSmallMoney = new MetaType(10, 0xff, 4, true, false, false, 0x7a, 110, "smallmoney", typeof(decimal), typeof(SqlMoney), System.Data.SqlDbType.SmallMoney, System.Data.DbType.Currency, 0);
        private static readonly MetaType MetaSmallVarBinary = new MetaType(0xff, 0xff, -1, false, false, false, 0x25, 0xad, ADP.StrEmpty, typeof(byte[]), typeof(SqlBinary), System.Data.SqlDbType.SmallInt | System.Data.SqlDbType.Int, System.Data.DbType.Binary, 2);
        private static readonly MetaType MetaSUDT = new MetaType(0xff, 0xff, -1, false, false, false, 0x1f, 0x1f, "", typeof(SqlDataRecord), typeof(SqlDataRecord), System.Data.SqlDbType.Structured, System.Data.DbType.Object, 0);
        private static readonly MetaType MetaTable = new MetaType(0xff, 0xff, -1, false, false, false, 0xf3, 0xf3, "table", typeof(IEnumerable<DbDataRecord>), typeof(IEnumerable<DbDataRecord>), System.Data.SqlDbType.Structured, System.Data.DbType.Object, 0);
        internal static readonly MetaType MetaText = new MetaType(0xff, 0xff, -1, false, true, false, 0x23, 0x23, "text", typeof(string), typeof(SqlString), System.Data.SqlDbType.Text, System.Data.DbType.AnsiString, 0);
        internal static readonly MetaType MetaTime = new MetaType(0xff, 7, -1, false, false, false, 0x29, 0x29, "time", typeof(TimeSpan), typeof(TimeSpan), System.Data.SqlDbType.Time, System.Data.DbType.Time, 1);
        private static readonly MetaType MetaTimestamp = new MetaType(0xff, 0xff, -1, false, false, false, 0xad, 0xad, "timestamp", typeof(byte[]), typeof(SqlBinary), System.Data.SqlDbType.Timestamp, System.Data.DbType.Binary, 2);
        private static readonly MetaType MetaTinyInt = new MetaType(3, 0xff, 1, true, false, false, 0x30, 0x26, "tinyint", typeof(byte), typeof(SqlByte), System.Data.SqlDbType.TinyInt, System.Data.DbType.Byte, 0);
        internal static readonly MetaType MetaUdt = new MetaType(0xff, 0xff, -1, false, false, true, 240, 240, "udt", typeof(object), typeof(object), System.Data.SqlDbType.Udt, System.Data.DbType.Object, 0);
        private static readonly MetaType MetaUniqueId = new MetaType(0xff, 0xff, 0x10, true, false, false, 0x24, 0x24, "uniqueidentifier", typeof(Guid), typeof(SqlGuid), System.Data.SqlDbType.UniqueIdentifier, System.Data.DbType.Guid, 0);
        internal static readonly MetaType MetaVarBinary = new MetaType(0xff, 0xff, -1, false, false, false, 0xa5, 0xa5, "varbinary", typeof(byte[]), typeof(SqlBinary), System.Data.SqlDbType.VarBinary, System.Data.DbType.Binary, 2);
        private static readonly MetaType MetaVarChar = new MetaType(0xff, 0xff, -1, false, false, false, 0xa7, 0xa7, "varchar", typeof(string), typeof(SqlString), System.Data.SqlDbType.VarChar, System.Data.DbType.AnsiString, 7);
        private static readonly MetaType MetaVariant = new MetaType(0xff, 0xff, -1, true, false, false, 0x62, 0x62, "sql_variant", typeof(object), typeof(object), System.Data.SqlDbType.Variant, System.Data.DbType.Object, 0);
        internal static readonly MetaType MetaXml = new MetaType(0xff, 0xff, -1, false, true, true, 0xf1, 0xf1, "xml", typeof(string), typeof(SqlXml), System.Data.SqlDbType.Xml, System.Data.DbType.Xml, 0);
        internal readonly byte NullableType;
        internal readonly byte Precision;
        internal readonly byte PropBytes;
        internal readonly byte Scale;
        internal readonly System.Data.SqlDbType SqlDbType;
        internal readonly Type SqlType;
        internal readonly byte TDSType;
        internal readonly string TypeName;

        public MetaType(byte precision, byte scale, int fixedLength, bool isFixed, bool isLong, bool isPlp, byte tdsType, byte nullableTdsType, string typeName, Type classType, Type sqlType, System.Data.SqlDbType sqldbType, System.Data.DbType dbType, byte propBytes)
        {
            this.Precision = precision;
            this.Scale = scale;
            this.FixedLength = fixedLength;
            this.IsFixed = isFixed;
            this.IsLong = isLong;
            this.IsPlp = isPlp;
            this.TDSType = tdsType;
            this.NullableType = nullableTdsType;
            this.TypeName = typeName;
            this.SqlDbType = sqldbType;
            this.DbType = dbType;
            this.ClassType = classType;
            this.SqlType = sqlType;
            this.PropBytes = propBytes;
            this.IsAnsiType = _IsAnsiType(sqldbType);
            this.IsBinType = _IsBinType(sqldbType);
            this.IsCharType = _IsCharType(sqldbType);
            this.IsNCharType = _IsNCharType(sqldbType);
            this.IsSizeInCharacters = _IsSizeInCharacters(this.SqlDbType);
            this.Is70Supported = _Is70Supported(this.SqlDbType);
            this.Is80Supported = _Is80Supported(this.SqlDbType);
            this.Is90Supported = _Is90Supported(this.SqlDbType);
            this.Is100Supported = _Is100Supported(this.SqlDbType);
        }

        private static bool _Is100Supported(System.Data.SqlDbType type)
        {
            if ((!_Is90Supported(type) && (System.Data.SqlDbType.Date != type)) && ((System.Data.SqlDbType.Time != type) && (System.Data.SqlDbType.DateTime2 != type)))
            {
                return (System.Data.SqlDbType.DateTimeOffset == type);
            }
            return true;
        }

        private static bool _Is70Supported(System.Data.SqlDbType type)
        {
            return (((type != System.Data.SqlDbType.BigInt) && (type > System.Data.SqlDbType.BigInt)) && (type <= System.Data.SqlDbType.VarChar));
        }

        private static bool _Is80Supported(System.Data.SqlDbType type)
        {
            return ((type >= System.Data.SqlDbType.BigInt) && (type <= System.Data.SqlDbType.Variant));
        }

        private static bool _Is90Supported(System.Data.SqlDbType type)
        {
            if (!_Is80Supported(type) && (System.Data.SqlDbType.Xml != type))
            {
                return (System.Data.SqlDbType.Udt == type);
            }
            return true;
        }

        private static bool _IsAnsiType(System.Data.SqlDbType type)
        {
            if ((type != System.Data.SqlDbType.Char) && (type != System.Data.SqlDbType.VarChar))
            {
                return (type == System.Data.SqlDbType.Text);
            }
            return true;
        }

        private static bool _IsBinType(System.Data.SqlDbType type)
        {
            if ((((type != System.Data.SqlDbType.Image) && (type != System.Data.SqlDbType.Binary)) && ((type != System.Data.SqlDbType.VarBinary) && (type != System.Data.SqlDbType.Timestamp))) && (type != System.Data.SqlDbType.Udt))
            {
                return (type == (System.Data.SqlDbType.SmallInt | System.Data.SqlDbType.Int));
            }
            return true;
        }

        private static bool _IsCharType(System.Data.SqlDbType type)
        {
            if ((((type != System.Data.SqlDbType.NChar) && (type != System.Data.SqlDbType.NVarChar)) && ((type != System.Data.SqlDbType.NText) && (type != System.Data.SqlDbType.Char))) && ((type != System.Data.SqlDbType.VarChar) && (type != System.Data.SqlDbType.Text)))
            {
                return (type == System.Data.SqlDbType.Xml);
            }
            return true;
        }

        private static bool _IsNCharType(System.Data.SqlDbType type)
        {
            if (((type != System.Data.SqlDbType.NChar) && (type != System.Data.SqlDbType.NVarChar)) && (type != System.Data.SqlDbType.NText))
            {
                return (type == System.Data.SqlDbType.Xml);
            }
            return true;
        }

        private static bool _IsNewKatmaiType(System.Data.SqlDbType type)
        {
            return (System.Data.SqlDbType.Structured == type);
        }

        private static bool _IsSizeInCharacters(System.Data.SqlDbType type)
        {
            if (((type != System.Data.SqlDbType.NChar) && (type != System.Data.SqlDbType.NVarChar)) && (type != System.Data.SqlDbType.Xml))
            {
                return (type == System.Data.SqlDbType.NText);
            }
            return true;
        }

        internal static bool _IsVarTime(System.Data.SqlDbType type)
        {
            if ((type != System.Data.SqlDbType.Time) && (type != System.Data.SqlDbType.DateTime2))
            {
                return (type == System.Data.SqlDbType.DateTimeOffset);
            }
            return true;
        }

        public static TdsDateTime FromDateTime(DateTime dateTime, byte cb)
        {
            SqlDateTime time2;
            TdsDateTime time = new TdsDateTime();
            if (cb == 8)
            {
                time2 = new SqlDateTime(dateTime);
                time.time = time2.TimeTicks;
            }
            else
            {
                time2 = new SqlDateTime(dateTime.AddSeconds(30.0));
                time.time = time2.TimeTicks / SqlDateTime.SQLTicksPerMinute;
            }
            time.days = time2.DayTicks;
            return time;
        }

        internal static object GetComValueFromSqlVariant(object sqlVal)
        {
            object obj2 = null;
            if (!ADP.IsNull(sqlVal))
            {
                if (sqlVal is SqlSingle)
                {
                    SqlSingle num7 = (SqlSingle) sqlVal;
                    return num7.Value;
                }
                if (sqlVal is SqlString)
                {
                    SqlString str = (SqlString) sqlVal;
                    return str.Value;
                }
                if (sqlVal is SqlDouble)
                {
                    SqlDouble num6 = (SqlDouble) sqlVal;
                    return num6.Value;
                }
                if (sqlVal is SqlBinary)
                {
                    SqlBinary binary = (SqlBinary) sqlVal;
                    return binary.Value;
                }
                if (sqlVal is SqlGuid)
                {
                    SqlGuid guid = (SqlGuid) sqlVal;
                    return guid.Value;
                }
                if (sqlVal is SqlBoolean)
                {
                    SqlBoolean flag = (SqlBoolean) sqlVal;
                    return flag.Value;
                }
                if (sqlVal is SqlByte)
                {
                    SqlByte num5 = (SqlByte) sqlVal;
                    return num5.Value;
                }
                if (sqlVal is SqlInt16)
                {
                    SqlInt16 num4 = (SqlInt16) sqlVal;
                    return num4.Value;
                }
                if (sqlVal is SqlInt32)
                {
                    SqlInt32 num3 = (SqlInt32) sqlVal;
                    return num3.Value;
                }
                if (sqlVal is SqlInt64)
                {
                    SqlInt64 num2 = (SqlInt64) sqlVal;
                    return num2.Value;
                }
                if (sqlVal is SqlDecimal)
                {
                    SqlDecimal num = (SqlDecimal) sqlVal;
                    return num.Value;
                }
                if (sqlVal is SqlDateTime)
                {
                    SqlDateTime time = (SqlDateTime) sqlVal;
                    return time.Value;
                }
                if (sqlVal is SqlMoney)
                {
                    SqlMoney money = (SqlMoney) sqlVal;
                    return money.Value;
                }
                if (sqlVal is SqlXml)
                {
                    obj2 = ((SqlXml) sqlVal).Value;
                }
            }
            return obj2;
        }

        internal static MetaType GetDefaultMetaType()
        {
            return MetaNVarChar;
        }

        internal static MetaType GetMaxMetaTypeFromMetaType(MetaType mt)
        {
            System.Data.SqlDbType sqlDbType = mt.SqlDbType;
            if (sqlDbType <= System.Data.SqlDbType.NVarChar)
            {
                switch (sqlDbType)
                {
                    case System.Data.SqlDbType.Binary:
                        goto Label_004F;

                    case System.Data.SqlDbType.Bit:
                        return mt;

                    case System.Data.SqlDbType.Char:
                        goto Label_0055;

                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.NVarChar:
                        return MetaMaxNVarChar;

                    case System.Data.SqlDbType.NText:
                        return mt;
                }
                return mt;
            }
            switch (sqlDbType)
            {
                case System.Data.SqlDbType.VarBinary:
                    break;

                case System.Data.SqlDbType.VarChar:
                    goto Label_0055;

                case System.Data.SqlDbType.Udt:
                    return MetaMaxUdt;

                default:
                    return mt;
            }
        Label_004F:
            return MetaMaxVarBinary;
        Label_0055:
            return MetaMaxVarChar;
        }

        internal static MetaType GetMetaTypeFromDbType(System.Data.DbType target)
        {
            switch (target)
            {
                case System.Data.DbType.AnsiString:
                    return MetaVarChar;

                case System.Data.DbType.Binary:
                    return MetaVarBinary;

                case System.Data.DbType.Byte:
                    return MetaTinyInt;

                case System.Data.DbType.Boolean:
                    return MetaBit;

                case System.Data.DbType.Currency:
                    return MetaMoney;

                case System.Data.DbType.Date:
                case System.Data.DbType.DateTime:
                    return MetaDateTime;

                case System.Data.DbType.Decimal:
                    return MetaDecimal;

                case System.Data.DbType.Double:
                    return MetaFloat;

                case System.Data.DbType.Guid:
                    return MetaUniqueId;

                case System.Data.DbType.Int16:
                    return MetaSmallInt;

                case System.Data.DbType.Int32:
                    return MetaInt;

                case System.Data.DbType.Int64:
                    return MetaBigInt;

                case System.Data.DbType.Object:
                    return MetaVariant;

                case System.Data.DbType.Single:
                    return MetaReal;

                case System.Data.DbType.String:
                    return MetaNVarChar;

                case System.Data.DbType.Time:
                    return MetaDateTime;

                case System.Data.DbType.AnsiStringFixedLength:
                    return MetaChar;

                case System.Data.DbType.StringFixedLength:
                    return MetaNChar;

                case System.Data.DbType.Xml:
                    return MetaXml;

                case System.Data.DbType.DateTime2:
                    return MetaDateTime2;

                case System.Data.DbType.DateTimeOffset:
                    return MetaDateTimeOffset;
            }
            throw ADP.DbTypeNotSupported(target, typeof(System.Data.SqlDbType));
        }

        internal static MetaType GetMetaTypeFromSqlDbType(System.Data.SqlDbType target, bool isMultiValued)
        {
            switch (target)
            {
                case System.Data.SqlDbType.BigInt:
                    return MetaBigInt;

                case System.Data.SqlDbType.Binary:
                    return MetaBinary;

                case System.Data.SqlDbType.Bit:
                    return MetaBit;

                case System.Data.SqlDbType.Char:
                    return MetaChar;

                case System.Data.SqlDbType.DateTime:
                    return MetaDateTime;

                case System.Data.SqlDbType.Decimal:
                    return MetaDecimal;

                case System.Data.SqlDbType.Float:
                    return MetaFloat;

                case System.Data.SqlDbType.Image:
                    return MetaImage;

                case System.Data.SqlDbType.Int:
                    return MetaInt;

                case System.Data.SqlDbType.Money:
                    return MetaMoney;

                case System.Data.SqlDbType.NChar:
                    return MetaNChar;

                case System.Data.SqlDbType.NText:
                    return MetaNText;

                case System.Data.SqlDbType.NVarChar:
                    return MetaNVarChar;

                case System.Data.SqlDbType.Real:
                    return MetaReal;

                case System.Data.SqlDbType.UniqueIdentifier:
                    return MetaUniqueId;

                case System.Data.SqlDbType.SmallDateTime:
                    return MetaSmallDateTime;

                case System.Data.SqlDbType.SmallInt:
                    return MetaSmallInt;

                case System.Data.SqlDbType.SmallMoney:
                    return MetaSmallMoney;

                case System.Data.SqlDbType.Text:
                    return MetaText;

                case System.Data.SqlDbType.Timestamp:
                    return MetaTimestamp;

                case System.Data.SqlDbType.TinyInt:
                    return MetaTinyInt;

                case System.Data.SqlDbType.VarBinary:
                    return MetaVarBinary;

                case System.Data.SqlDbType.VarChar:
                    return MetaVarChar;

                case System.Data.SqlDbType.Variant:
                    return MetaVariant;

                case (System.Data.SqlDbType.SmallInt | System.Data.SqlDbType.Int):
                    return MetaSmallVarBinary;

                case System.Data.SqlDbType.Xml:
                    return MetaXml;

                case System.Data.SqlDbType.Udt:
                    return MetaUdt;

                case System.Data.SqlDbType.Structured:
                    if (!isMultiValued)
                    {
                        return MetaSUDT;
                    }
                    return MetaTable;

                case System.Data.SqlDbType.Date:
                    return MetaDate;

                case System.Data.SqlDbType.Time:
                    return MetaTime;

                case System.Data.SqlDbType.DateTime2:
                    return MetaDateTime2;

                case System.Data.SqlDbType.DateTimeOffset:
                    return MetaDateTimeOffset;
            }
            throw SQL.InvalidSqlDbType(target);
        }

        internal static MetaType GetMetaTypeFromType(Type dataType)
        {
            return GetMetaTypeFromValue(dataType, null, false);
        }

        internal static MetaType GetMetaTypeFromValue(object value)
        {
            return GetMetaTypeFromValue(value.GetType(), value, true);
        }

        private static MetaType GetMetaTypeFromValue(Type dataType, object value, bool inferLen)
        {
            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.Empty:
                    throw ADP.InvalidDataType(TypeCode.Empty);

                case TypeCode.Object:
                    if (!(dataType == typeof(byte[])))
                    {
                        if (dataType == typeof(Guid))
                        {
                            return MetaUniqueId;
                        }
                        if (dataType == typeof(object))
                        {
                            return MetaVariant;
                        }
                        if (dataType == typeof(SqlBinary))
                        {
                            return MetaVarBinary;
                        }
                        if (dataType == typeof(SqlBoolean))
                        {
                            return MetaBit;
                        }
                        if (dataType == typeof(SqlByte))
                        {
                            return MetaTinyInt;
                        }
                        if (dataType == typeof(SqlBytes))
                        {
                            return MetaVarBinary;
                        }
                        if (dataType != typeof(SqlChars))
                        {
                            if (dataType == typeof(SqlDateTime))
                            {
                                return MetaDateTime;
                            }
                            if (dataType == typeof(SqlDouble))
                            {
                                return MetaFloat;
                            }
                            if (dataType == typeof(SqlGuid))
                            {
                                return MetaUniqueId;
                            }
                            if (dataType == typeof(SqlInt16))
                            {
                                return MetaSmallInt;
                            }
                            if (dataType == typeof(SqlInt32))
                            {
                                return MetaInt;
                            }
                            if (dataType == typeof(SqlInt64))
                            {
                                return MetaBigInt;
                            }
                            if (dataType == typeof(SqlMoney))
                            {
                                return MetaMoney;
                            }
                            if (dataType == typeof(SqlDecimal))
                            {
                                return MetaDecimal;
                            }
                            if (dataType == typeof(SqlSingle))
                            {
                                return MetaReal;
                            }
                            if (dataType == typeof(SqlXml))
                            {
                                return MetaXml;
                            }
                            if (dataType == typeof(XmlReader))
                            {
                                return MetaXml;
                            }
                            if (!(dataType == typeof(SqlString)))
                            {
                                if ((dataType == typeof(IEnumerable<DbDataRecord>)) || (dataType == typeof(DataTable)))
                                {
                                    return MetaTable;
                                }
                                if (dataType == typeof(TimeSpan))
                                {
                                    return MetaTime;
                                }
                                if (dataType == typeof(DateTimeOffset))
                                {
                                    return MetaDateTimeOffset;
                                }
                                if (SqlUdtInfo.TryGetFromType(dataType) == null)
                                {
                                    throw ADP.UnknownDataType(dataType);
                                }
                                return MetaUdt;
                            }
                            if (inferLen)
                            {
                                SqlString str2 = (SqlString) value;
                                if (!str2.IsNull)
                                {
                                    SqlString str = (SqlString) value;
                                    return PromoteStringType(str.Value);
                                }
                            }
                        }
                        return MetaNVarChar;
                    }
                    if (inferLen && (((byte[]) value).Length > 0x1f40))
                    {
                        return MetaImage;
                    }
                    return MetaVarBinary;

                case TypeCode.DBNull:
                    throw ADP.InvalidDataType(TypeCode.DBNull);

                case TypeCode.Boolean:
                    return MetaBit;

                case TypeCode.Char:
                    throw ADP.InvalidDataType(TypeCode.Char);

                case TypeCode.SByte:
                    throw ADP.InvalidDataType(TypeCode.SByte);

                case TypeCode.Byte:
                    return MetaTinyInt;

                case TypeCode.Int16:
                    return MetaSmallInt;

                case TypeCode.UInt16:
                    throw ADP.InvalidDataType(TypeCode.UInt16);

                case TypeCode.Int32:
                    return MetaInt;

                case TypeCode.UInt32:
                    throw ADP.InvalidDataType(TypeCode.UInt32);

                case TypeCode.Int64:
                    return MetaBigInt;

                case TypeCode.UInt64:
                    throw ADP.InvalidDataType(TypeCode.UInt64);

                case TypeCode.Single:
                    return MetaReal;

                case TypeCode.Double:
                    return MetaFloat;

                case TypeCode.Decimal:
                    return MetaDecimal;

                case TypeCode.DateTime:
                    return MetaDateTime;

                case TypeCode.String:
                    if (!inferLen)
                    {
                        return MetaNVarChar;
                    }
                    return PromoteStringType((string) value);
            }
            throw ADP.UnknownDataTypeCode(dataType, Type.GetTypeCode(dataType));
        }

        internal static object GetNullSqlValue(Type sqlType)
        {
            if (sqlType == typeof(SqlSingle))
            {
                return SqlSingle.Null;
            }
            if (sqlType == typeof(SqlString))
            {
                return SqlString.Null;
            }
            if (sqlType == typeof(SqlDouble))
            {
                return SqlDouble.Null;
            }
            if (sqlType == typeof(SqlBinary))
            {
                return SqlBinary.Null;
            }
            if (sqlType == typeof(SqlGuid))
            {
                return SqlGuid.Null;
            }
            if (sqlType == typeof(SqlBoolean))
            {
                return SqlBoolean.Null;
            }
            if (sqlType == typeof(SqlByte))
            {
                return SqlByte.Null;
            }
            if (sqlType == typeof(SqlInt16))
            {
                return SqlInt16.Null;
            }
            if (sqlType == typeof(SqlInt32))
            {
                return SqlInt32.Null;
            }
            if (sqlType == typeof(SqlInt64))
            {
                return SqlInt64.Null;
            }
            if (sqlType == typeof(SqlDecimal))
            {
                return SqlDecimal.Null;
            }
            if (sqlType == typeof(SqlDateTime))
            {
                return SqlDateTime.Null;
            }
            if (sqlType == typeof(SqlMoney))
            {
                return SqlMoney.Null;
            }
            if (sqlType == typeof(SqlXml))
            {
                return SqlXml.Null;
            }
            if (sqlType != typeof(object))
            {
                if (sqlType == typeof(IEnumerable<DbDataRecord>))
                {
                    return DBNull.Value;
                }
                if (sqlType == typeof(DataTable))
                {
                    return DBNull.Value;
                }
                if (sqlType == typeof(DateTime))
                {
                    return DBNull.Value;
                }
                if (sqlType == typeof(TimeSpan))
                {
                    return DBNull.Value;
                }
                if (sqlType == typeof(DateTimeOffset))
                {
                    return DBNull.Value;
                }
            }
            return DBNull.Value;
        }

        internal static MetaType GetSqlDataType(int tdsType, uint userType, int length)
        {
            switch (tdsType)
            {
                case 0x7a:
                    return MetaSmallMoney;

                case 0x7f:
                    return MetaBigInt;

                case 0x22:
                    return MetaImage;

                case 0x23:
                    return MetaText;

                case 0x24:
                    return MetaUniqueId;

                case 0x25:
                    return MetaSmallVarBinary;

                case 0x26:
                    if (4 <= length)
                    {
                        if (4 != length)
                        {
                            return MetaBigInt;
                        }
                        return MetaInt;
                    }
                    if (2 == length)
                    {
                        return MetaSmallInt;
                    }
                    return MetaTinyInt;

                case 0x27:
                case 0xa7:
                    return MetaVarChar;

                case 40:
                    return MetaDate;

                case 0x29:
                    return MetaTime;

                case 0x2a:
                    return MetaDateTime2;

                case 0x2b:
                    return MetaDateTimeOffset;

                case 0x2d:
                case 0xad:
                    if (80 != userType)
                    {
                        return MetaBinary;
                    }
                    return MetaTimestamp;

                case 0x2f:
                case 0xaf:
                    return MetaChar;

                case 0x30:
                    return MetaTinyInt;

                case 50:
                case 0x68:
                    return MetaBit;

                case 0x34:
                    return MetaSmallInt;

                case 0x38:
                    return MetaInt;

                case 0x3a:
                    return MetaSmallDateTime;

                case 0x3b:
                    return MetaReal;

                case 60:
                    return MetaMoney;

                case 0x3d:
                    return MetaDateTime;

                case 0x3e:
                    return MetaFloat;

                case 0x62:
                    return MetaVariant;

                case 0x63:
                    return MetaNText;

                case 0x6a:
                case 0x6c:
                    return MetaDecimal;

                case 0x6d:
                    if (4 == length)
                    {
                        return MetaReal;
                    }
                    return MetaFloat;

                case 110:
                    if (4 == length)
                    {
                        return MetaSmallMoney;
                    }
                    return MetaMoney;

                case 0x6f:
                    if (4 == length)
                    {
                        return MetaSmallDateTime;
                    }
                    return MetaDateTime;

                case 0xa5:
                    return MetaVarBinary;

                case 0xef:
                    return MetaNChar;

                case 240:
                    return MetaUdt;

                case 0xf1:
                    return MetaXml;

                case 0xf3:
                    return MetaTable;

                case 0xe7:
                    return MetaNVarChar;
            }
            throw SQL.InvalidSqlDbType((System.Data.SqlDbType) tdsType);
        }

        internal static System.Data.SqlDbType GetSqlDbTypeFromOleDbType(short dbType, string typeName)
        {
            System.Data.SqlDbType variant = System.Data.SqlDbType.Variant;
            OleDbType type2 = (OleDbType) dbType;
            if (type2 <= OleDbType.Filetime)
            {
                switch (type2)
                {
                    case OleDbType.SmallInt:
                    case OleDbType.UnsignedSmallInt:
                        return System.Data.SqlDbType.SmallInt;

                    case OleDbType.Integer:
                        return System.Data.SqlDbType.Int;

                    case OleDbType.Single:
                        return System.Data.SqlDbType.Real;

                    case OleDbType.Double:
                        return System.Data.SqlDbType.Float;

                    case OleDbType.Currency:
                        return ((typeName == "smallmoney") ? System.Data.SqlDbType.SmallMoney : System.Data.SqlDbType.Money);

                    case OleDbType.Date:
                    case OleDbType.Filetime:
                        goto Label_0133;

                    case OleDbType.BSTR:
                        goto Label_01B3;

                    case OleDbType.IDispatch:
                    case OleDbType.Error:
                    case OleDbType.IUnknown:
                    case ((OleDbType) 15):
                    case OleDbType.UnsignedInt:
                        return variant;

                    case OleDbType.Boolean:
                        return System.Data.SqlDbType.Bit;

                    case OleDbType.Variant:
                        return System.Data.SqlDbType.Variant;

                    case OleDbType.Decimal:
                        goto Label_016B;

                    case OleDbType.TinyInt:
                    case OleDbType.UnsignedTinyInt:
                        return System.Data.SqlDbType.TinyInt;

                    case OleDbType.BigInt:
                        return System.Data.SqlDbType.BigInt;
                }
                return variant;
            }
            switch (type2)
            {
                case OleDbType.Binary:
                case OleDbType.VarBinary:
                    return ((typeName == "binary") ? System.Data.SqlDbType.Binary : System.Data.SqlDbType.VarBinary);

                case OleDbType.Char:
                case OleDbType.VarChar:
                    return ((typeName == "char") ? System.Data.SqlDbType.Char : System.Data.SqlDbType.VarChar);

                case OleDbType.WChar:
                case OleDbType.VarWChar:
                    goto Label_01B3;

                case OleDbType.Numeric:
                    goto Label_016B;

                case (OleDbType.Binary | OleDbType.Single):
                    return System.Data.SqlDbType.Udt;

                case OleDbType.DBDate:
                    return System.Data.SqlDbType.Date;

                case OleDbType.DBTime:
                case (OleDbType.Binary | OleDbType.BSTR):
                case (OleDbType.Char | OleDbType.BSTR):
                case OleDbType.PropVariant:
                case OleDbType.VarNumeric:
                case (OleDbType.Binary | OleDbType.Variant):
                case (OleDbType.PropVariant | OleDbType.Single):
                case (OleDbType.VarNumeric | OleDbType.Single):
                case (OleDbType.Binary | OleDbType.TinyInt):
                    return variant;

                case OleDbType.DBTimeStamp:
                    break;

                case (OleDbType.DBDate | OleDbType.BSTR):
                    return System.Data.SqlDbType.Xml;

                case (OleDbType.Char | OleDbType.TinyInt):
                    return System.Data.SqlDbType.Time;

                case (OleDbType.WChar | OleDbType.TinyInt):
                    return System.Data.SqlDbType.DateTimeOffset;

                case OleDbType.Guid:
                    return System.Data.SqlDbType.UniqueIdentifier;

                case OleDbType.LongVarChar:
                    return System.Data.SqlDbType.Text;

                case OleDbType.LongVarWChar:
                    return System.Data.SqlDbType.NText;

                case OleDbType.LongVarBinary:
                    return System.Data.SqlDbType.Image;

                default:
                    return variant;
            }
        Label_0133:
            switch (typeName)
            {
                case "smalldatetime":
                    return System.Data.SqlDbType.SmallDateTime;

                case "datetime2":
                    return System.Data.SqlDbType.DateTime2;

                default:
                    return System.Data.SqlDbType.DateTime;
            }
        Label_016B:
            return System.Data.SqlDbType.Decimal;
        Label_01B3:
            return ((typeName == "nchar") ? System.Data.SqlDbType.NChar : System.Data.SqlDbType.NVarChar);
        }

        internal static object GetSqlValueFromComVariant(object comVal)
        {
            if ((comVal != null) && (DBNull.Value != comVal))
            {
                if (comVal is float)
                {
                    return new SqlSingle((float) comVal);
                }
                if (comVal is string)
                {
                    return new SqlString((string) comVal);
                }
                if (comVal is double)
                {
                    return new SqlDouble((double) comVal);
                }
                if (comVal is byte[])
                {
                    return new SqlBinary((byte[]) comVal);
                }
                if (comVal is char)
                {
                    char ch = (char) comVal;
                    return new SqlString(ch.ToString());
                }
                if (comVal is char[])
                {
                    return new SqlChars((char[]) comVal);
                }
                if (comVal is Guid)
                {
                    return new SqlGuid((Guid) comVal);
                }
                if (comVal is bool)
                {
                    return new SqlBoolean((bool) comVal);
                }
                if (comVal is byte)
                {
                    return new SqlByte((byte) comVal);
                }
                if (comVal is short)
                {
                    return new SqlInt16((short) comVal);
                }
                if (comVal is int)
                {
                    return new SqlInt32((int) comVal);
                }
                if (comVal is long)
                {
                    return new SqlInt64((long) comVal);
                }
                if (comVal is decimal)
                {
                    return new SqlDecimal((decimal) comVal);
                }
                if (comVal is DateTime)
                {
                    return new SqlDateTime((DateTime) comVal);
                }
                if (comVal is XmlReader)
                {
                    return new SqlXml((XmlReader) comVal);
                }
                if ((comVal is TimeSpan) || (comVal is DateTimeOffset))
                {
                    return comVal;
                }
            }
            return null;
        }

        internal static string GetStringFromXml(XmlReader xmlreader)
        {
            SqlXml xml = new SqlXml(xmlreader);
            return xml.Value;
        }

        internal static int GetTimeSizeFromScale(byte scale)
        {
            if (scale <= 2)
            {
                return 3;
            }
            if (scale <= 4)
            {
                return 4;
            }
            return 5;
        }

        internal static MetaType PromoteStringType(string s)
        {
            if ((s.Length << 1) > 0x1f40)
            {
                return MetaVarChar;
            }
            return MetaNVarChar;
        }

        public static DateTime ToDateTime(int sqlDays, int sqlTime, int length)
        {
            if (length == 4)
            {
                SqlDateTime time2 = new SqlDateTime(sqlDays, sqlTime * SqlDateTime.SQLTicksPerMinute);
                return time2.Value;
            }
            SqlDateTime time = new SqlDateTime(sqlDays, sqlTime);
            return time.Value;
        }

        internal bool IsNewKatmaiType
        {
            get
            {
                return _IsNewKatmaiType(this.SqlDbType);
            }
        }

        internal bool IsVarTime
        {
            get
            {
                return _IsVarTime(this.SqlDbType);
            }
        }

        public int TypeId
        {
            get
            {
                return 0;
            }
        }
    }
}

