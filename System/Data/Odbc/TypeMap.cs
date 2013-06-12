namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal sealed class TypeMap
    {
        private static readonly TypeMap _BigInt = new TypeMap(OdbcType.BigInt, DbType.Int64, typeof(long), ODBC32.SQL_TYPE.BIGINT, ODBC32.SQL_C.SBIGINT, ODBC32.SQL_C.SBIGINT, 8, 20, true);
        private static readonly TypeMap _Binary = new TypeMap(OdbcType.Binary, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.BINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, false);
        private static readonly TypeMap _Bit = new TypeMap(OdbcType.Bit, DbType.Boolean, typeof(bool), ODBC32.SQL_TYPE.BIT, ODBC32.SQL_C.BIT, ODBC32.SQL_C.BIT, 1, 1, false);
        internal readonly int _bufferSize;
        internal static readonly TypeMap _Char = new TypeMap(OdbcType.Char, DbType.AnsiStringFixedLength, typeof(string), ODBC32.SQL_TYPE.CHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, false);
        internal readonly int _columnSize;
        private static readonly TypeMap _Date = new TypeMap(OdbcType.Date, DbType.Date, typeof(DateTime), ODBC32.SQL_TYPE.TYPE_DATE, ODBC32.SQL_C.TYPE_DATE, ODBC32.SQL_C.TYPE_DATE, 6, 10, false);
        private static readonly TypeMap _DateTime = new TypeMap(OdbcType.DateTime, DbType.DateTime, typeof(DateTime), ODBC32.SQL_TYPE.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, 0x10, 0x17, false);
        internal readonly DbType _dbType;
        private static readonly TypeMap _Decimal = new TypeMap(OdbcType.Decimal, DbType.Decimal, typeof(decimal), ODBC32.SQL_TYPE.DECIMAL, ODBC32.SQL_C.NUMERIC, ODBC32.SQL_C.NUMERIC, 0x13, 0x1c, false);
        private static readonly TypeMap _Double = new TypeMap(OdbcType.Double, DbType.Double, typeof(double), ODBC32.SQL_TYPE.DOUBLE, ODBC32.SQL_C.DOUBLE, ODBC32.SQL_C.DOUBLE, 8, 15, false);
        internal static readonly TypeMap _Image = new TypeMap(OdbcType.Image, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.LONGVARBINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, false);
        private static readonly TypeMap _Int = new TypeMap(OdbcType.Int, DbType.Int32, typeof(int), ODBC32.SQL_TYPE.INTEGER, ODBC32.SQL_C.SLONG, ODBC32.SQL_C.SLONG, 4, 10, true);
        private static readonly TypeMap _NChar = new TypeMap(OdbcType.NChar, DbType.StringFixedLength, typeof(string), ODBC32.SQL_TYPE.WCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.WCHAR, -1, -1, false);
        internal static readonly TypeMap _NText = new TypeMap(OdbcType.NText, DbType.String, typeof(string), ODBC32.SQL_TYPE.WLONGVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.WCHAR, -1, -1, false);
        private static readonly TypeMap _Numeric = new TypeMap(OdbcType.Numeric, DbType.Decimal, typeof(decimal), ODBC32.SQL_TYPE.NUMERIC, ODBC32.SQL_C.NUMERIC, ODBC32.SQL_C.NUMERIC, 0x13, 0x1c, false);
        internal static readonly TypeMap _NVarChar = new TypeMap(OdbcType.NVarChar, DbType.String, typeof(string), ODBC32.SQL_TYPE.WVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.WCHAR, -1, -1, false);
        internal readonly OdbcType _odbcType;
        internal readonly ODBC32.SQL_C _param_sql_c;
        private static readonly TypeMap _Real = new TypeMap(OdbcType.Real, DbType.Single, typeof(float), ODBC32.SQL_TYPE.REAL, ODBC32.SQL_C.REAL, ODBC32.SQL_C.REAL, 4, 7, false);
        internal readonly bool _signType;
        private static readonly TypeMap _SmallDT = new TypeMap(OdbcType.SmallDateTime, DbType.DateTime, typeof(DateTime), ODBC32.SQL_TYPE.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, ODBC32.SQL_C.TYPE_TIMESTAMP, 0x10, 0x17, false);
        private static readonly TypeMap _SmallInt = new TypeMap(OdbcType.SmallInt, DbType.Int16, typeof(short), ODBC32.SQL_TYPE.SMALLINT, ODBC32.SQL_C.SSHORT, ODBC32.SQL_C.SSHORT, 2, 5, true);
        internal readonly ODBC32.SQL_C _sql_c;
        internal readonly ODBC32.SQL_TYPE _sql_type;
        internal static readonly TypeMap _Text = new TypeMap(OdbcType.Text, DbType.AnsiString, typeof(string), ODBC32.SQL_TYPE.LONGVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, false);
        private static readonly TypeMap _Time = new TypeMap(OdbcType.Time, DbType.Time, typeof(TimeSpan), ODBC32.SQL_TYPE.TYPE_TIME, ODBC32.SQL_C.TYPE_TIME, ODBC32.SQL_C.TYPE_TIME, 6, 12, false);
        private static readonly TypeMap _Timestamp = new TypeMap(OdbcType.Timestamp, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.BINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, false);
        private static readonly TypeMap _TinyInt = new TypeMap(OdbcType.TinyInt, DbType.Byte, typeof(byte), ODBC32.SQL_TYPE.TINYINT, ODBC32.SQL_C.UTINYINT, ODBC32.SQL_C.UTINYINT, 1, 3, true);
        internal readonly Type _type;
        private static readonly TypeMap _UDT = new TypeMap(OdbcType.Binary, DbType.Binary, typeof(object), ODBC32.SQL_TYPE.SS_UDT, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, false);
        private static readonly TypeMap _UniqueId = new TypeMap(OdbcType.UniqueIdentifier, DbType.Guid, typeof(Guid), ODBC32.SQL_TYPE.GUID, ODBC32.SQL_C.GUID, ODBC32.SQL_C.GUID, 0x10, 0x24, false);
        private static readonly TypeMap _VarBinary = new TypeMap(OdbcType.VarBinary, DbType.Binary, typeof(byte[]), ODBC32.SQL_TYPE.VARBINARY, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, false);
        internal static readonly TypeMap _VarChar = new TypeMap(OdbcType.VarChar, DbType.AnsiString, typeof(string), ODBC32.SQL_TYPE.VARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, false);
        private static readonly TypeMap _Variant = new TypeMap(OdbcType.Binary, DbType.Binary, typeof(object), ODBC32.SQL_TYPE.SS_VARIANT, ODBC32.SQL_C.BINARY, ODBC32.SQL_C.BINARY, -1, -1, false);
        private static readonly TypeMap _XML = new TypeMap(OdbcType.Text, DbType.AnsiString, typeof(string), ODBC32.SQL_TYPE.LONGVARCHAR, ODBC32.SQL_C.WCHAR, ODBC32.SQL_C.CHAR, -1, -1, false);

        private TypeMap(OdbcType odbcType, DbType dbType, Type type, ODBC32.SQL_TYPE sql_type, ODBC32.SQL_C sql_c, ODBC32.SQL_C param_sql_c, int bsize, int csize, bool signType)
        {
            this._odbcType = odbcType;
            this._dbType = dbType;
            this._type = type;
            this._sql_type = sql_type;
            this._sql_c = sql_c;
            this._param_sql_c = param_sql_c;
            this._bufferSize = bsize;
            this._columnSize = csize;
            this._signType = signType;
        }

        internal static TypeMap FromDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return _VarChar;

                case DbType.Binary:
                    return _VarBinary;

                case DbType.Byte:
                    return _TinyInt;

                case DbType.Boolean:
                    return _Bit;

                case DbType.Currency:
                    return _Decimal;

                case DbType.Date:
                    return _Date;

                case DbType.DateTime:
                    return _DateTime;

                case DbType.Decimal:
                    return _Decimal;

                case DbType.Double:
                    return _Double;

                case DbType.Guid:
                    return _UniqueId;

                case DbType.Int16:
                    return _SmallInt;

                case DbType.Int32:
                    return _Int;

                case DbType.Int64:
                    return _BigInt;

                case DbType.Single:
                    return _Real;

                case DbType.String:
                    return _NVarChar;

                case DbType.Time:
                    return _Time;

                case DbType.AnsiStringFixedLength:
                    return _Char;

                case DbType.StringFixedLength:
                    return _NChar;
            }
            throw ADP.DbTypeNotSupported(dbType, typeof(OdbcType));
        }

        internal static TypeMap FromOdbcType(OdbcType odbcType)
        {
            switch (odbcType)
            {
                case OdbcType.BigInt:
                    return _BigInt;

                case OdbcType.Binary:
                    return _Binary;

                case OdbcType.Bit:
                    return _Bit;

                case OdbcType.Char:
                    return _Char;

                case OdbcType.DateTime:
                    return _DateTime;

                case OdbcType.Decimal:
                    return _Decimal;

                case OdbcType.Numeric:
                    return _Numeric;

                case OdbcType.Double:
                    return _Double;

                case OdbcType.Image:
                    return _Image;

                case OdbcType.Int:
                    return _Int;

                case OdbcType.NChar:
                    return _NChar;

                case OdbcType.NText:
                    return _NText;

                case OdbcType.NVarChar:
                    return _NVarChar;

                case OdbcType.Real:
                    return _Real;

                case OdbcType.UniqueIdentifier:
                    return _UniqueId;

                case OdbcType.SmallDateTime:
                    return _SmallDT;

                case OdbcType.SmallInt:
                    return _SmallInt;

                case OdbcType.Text:
                    return _Text;

                case OdbcType.Timestamp:
                    return _Timestamp;

                case OdbcType.TinyInt:
                    return _TinyInt;

                case OdbcType.VarBinary:
                    return _VarBinary;

                case OdbcType.VarChar:
                    return _VarChar;

                case OdbcType.Date:
                    return _Date;

                case OdbcType.Time:
                    return _Time;
            }
            throw ODBC.UnknownOdbcType(odbcType);
        }

        internal static TypeMap FromSqlType(ODBC32.SQL_TYPE sqltype)
        {
            switch (sqltype)
            {
                case ODBC32.SQL_TYPE.SS_TIME_EX:
                case ODBC32.SQL_TYPE.SS_UTCDATETIME:
                    throw ODBC.UnknownSQLType(sqltype);

                case ODBC32.SQL_TYPE.SS_XML:
                    return _XML;

                case ODBC32.SQL_TYPE.SS_UDT:
                    return _UDT;

                case ODBC32.SQL_TYPE.SS_VARIANT:
                    return _Variant;

                case ODBC32.SQL_TYPE.GUID:
                    return _UniqueId;

                case ODBC32.SQL_TYPE.WLONGVARCHAR:
                    return _NText;

                case ODBC32.SQL_TYPE.WVARCHAR:
                    return _NVarChar;

                case ODBC32.SQL_TYPE.WCHAR:
                    return _NChar;

                case ODBC32.SQL_TYPE.BIT:
                    return _Bit;

                case ODBC32.SQL_TYPE.TINYINT:
                    return _TinyInt;

                case ODBC32.SQL_TYPE.BIGINT:
                    return _BigInt;

                case ODBC32.SQL_TYPE.LONGVARBINARY:
                    return _Image;

                case ODBC32.SQL_TYPE.VARBINARY:
                    return _VarBinary;

                case ODBC32.SQL_TYPE.BINARY:
                    return _Binary;

                case ODBC32.SQL_TYPE.LONGVARCHAR:
                    return _Text;

                case ODBC32.SQL_TYPE.CHAR:
                    return _Char;

                case ODBC32.SQL_TYPE.NUMERIC:
                    return _Numeric;

                case ODBC32.SQL_TYPE.DECIMAL:
                    return _Decimal;

                case ODBC32.SQL_TYPE.INTEGER:
                    return _Int;

                case ODBC32.SQL_TYPE.SMALLINT:
                    return _SmallInt;

                case ODBC32.SQL_TYPE.FLOAT:
                    return _Double;

                case ODBC32.SQL_TYPE.REAL:
                    return _Real;

                case ODBC32.SQL_TYPE.DOUBLE:
                    return _Double;

                case ODBC32.SQL_TYPE.TIMESTAMP:
                case ODBC32.SQL_TYPE.TYPE_TIMESTAMP:
                    return _DateTime;

                case ODBC32.SQL_TYPE.VARCHAR:
                    return _VarChar;

                case ODBC32.SQL_TYPE.TYPE_DATE:
                    return _Date;

                case ODBC32.SQL_TYPE.TYPE_TIME:
                    return _Time;
            }
            throw ODBC.UnknownSQLType(sqltype);
        }

        internal static TypeMap FromSystemType(Type dataType)
        {
            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.Empty:
                    throw ADP.InvalidDataType(TypeCode.Empty);

                case TypeCode.Object:
                    if (dataType == typeof(byte[]))
                    {
                        return _VarBinary;
                    }
                    if (dataType == typeof(Guid))
                    {
                        return _UniqueId;
                    }
                    if (dataType == typeof(TimeSpan))
                    {
                        return _Time;
                    }
                    if (dataType != typeof(char[]))
                    {
                        throw ADP.UnknownDataType(dataType);
                    }
                    return _NVarChar;

                case TypeCode.DBNull:
                    throw ADP.InvalidDataType(TypeCode.DBNull);

                case TypeCode.Boolean:
                    return _Bit;

                case TypeCode.Char:
                case TypeCode.String:
                    return _NVarChar;

                case TypeCode.SByte:
                    return _SmallInt;

                case TypeCode.Byte:
                    return _TinyInt;

                case TypeCode.Int16:
                    return _SmallInt;

                case TypeCode.UInt16:
                    return _Int;

                case TypeCode.Int32:
                    return _Int;

                case TypeCode.UInt32:
                    return _BigInt;

                case TypeCode.Int64:
                    return _BigInt;

                case TypeCode.UInt64:
                    return _Numeric;

                case TypeCode.Single:
                    return _Real;

                case TypeCode.Double:
                    return _Double;

                case TypeCode.Decimal:
                    return _Numeric;

                case TypeCode.DateTime:
                    return _DateTime;
            }
            throw ADP.UnknownDataTypeCode(dataType, Type.GetTypeCode(dataType));
        }

        internal static TypeMap UpgradeSignedType(TypeMap typeMap, bool unsigned)
        {
            if (unsigned)
            {
                switch (typeMap._dbType)
                {
                    case DbType.Int16:
                        return _Int;

                    case DbType.Int32:
                        return _BigInt;

                    case DbType.Int64:
                        return _Decimal;
                }
                return typeMap;
            }
            if (typeMap._dbType == DbType.Byte)
            {
                return _SmallInt;
            }
            return typeMap;
        }
    }
}

