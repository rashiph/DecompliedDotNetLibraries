namespace System.Data.OracleClient
{
    using System;
    using System.Data;
    using System.Data.Common;

    internal sealed class MetaType
    {
        private readonly int _bindSize;
        private readonly Type _convertToType;
        private readonly string _dataTypeName;
        private readonly System.Data.DbType _dbType;
        private readonly bool _isCharacterType;
        private readonly bool _isLob;
        private readonly bool _isLong;
        private readonly int _maxBindSize;
        private readonly Type _noConvertType;
        private readonly OCI.DATATYPE _ociType;
        private readonly System.Data.OracleClient.OracleType _oracleType;
        private readonly bool _usesNationalCharacterSet;
        private static readonly MetaType[] dbTypeMetaType = new MetaType[] { 
            new MetaType(System.Data.DbType.AnsiString, System.Data.OracleClient.OracleType.VarChar, OCI.DATATYPE.VARCHAR2, "VARCHAR2", typeof(string), typeof(OracleString), 0, 0xfa0, false), new MetaType(System.Data.DbType.Binary, System.Data.OracleClient.OracleType.Raw, OCI.DATATYPE.RAW, "RAW", typeof(byte[]), typeof(OracleBinary), 0, 0x7d0, false), new MetaType(System.Data.DbType.Byte, System.Data.OracleClient.OracleType.Byte, OCI.DATATYPE.UNSIGNEDINT, "UNSIGNED INTEGER", typeof(byte), typeof(byte), 1, 1, false), new MetaType(System.Data.DbType.Boolean, System.Data.OracleClient.OracleType.Byte, OCI.DATATYPE.UNSIGNEDINT, "UNSIGNED INTEGER", typeof(byte), typeof(byte), 1, 1, false), new MetaType(System.Data.DbType.Currency, System.Data.OracleClient.OracleType.Number, OCI.DATATYPE.VARNUM, "NUMBER", typeof(decimal), typeof(OracleNumber), 0x16, 0x16, false), new MetaType(System.Data.DbType.Date, System.Data.OracleClient.OracleType.DateTime, OCI.DATATYPE.DATE, "DATE", typeof(DateTime), typeof(OracleDateTime), 7, 7, false), new MetaType(System.Data.DbType.DateTime, System.Data.OracleClient.OracleType.DateTime, OCI.DATATYPE.DATE, "DATE", typeof(DateTime), typeof(OracleDateTime), 7, 7, false), new MetaType(System.Data.DbType.Decimal, System.Data.OracleClient.OracleType.Number, OCI.DATATYPE.VARNUM, "NUMBER", typeof(decimal), typeof(OracleNumber), 0x16, 0x16, false), new MetaType(System.Data.DbType.Double, System.Data.OracleClient.OracleType.Double, OCI.DATATYPE.FLOAT, "FLOAT", typeof(double), typeof(double), 8, 8, false), new MetaType(System.Data.DbType.Guid, System.Data.OracleClient.OracleType.Raw, OCI.DATATYPE.RAW, "RAW", typeof(byte[]), typeof(OracleBinary), 0x10, 0x10, false), new MetaType(System.Data.DbType.Int16, System.Data.OracleClient.OracleType.Int16, OCI.DATATYPE.INTEGER, "INTEGER", typeof(short), typeof(short), 2, 2, false), new MetaType(System.Data.DbType.Int32, System.Data.OracleClient.OracleType.Int32, OCI.DATATYPE.INTEGER, "INTEGER", typeof(int), typeof(int), 4, 4, false), new MetaType(System.Data.DbType.Int64, System.Data.OracleClient.OracleType.Number, OCI.DATATYPE.VARNUM, "NUMBER", typeof(decimal), typeof(OracleNumber), 0x16, 0x16, false), new MetaType(System.Data.DbType.Object, System.Data.OracleClient.OracleType.Blob, OCI.DATATYPE.BLOB, "BLOB", typeof(object), typeof(OracleLob), IntPtr.Size, IntPtr.Size, false), new MetaType(System.Data.DbType.SByte, System.Data.OracleClient.OracleType.SByte, OCI.DATATYPE.INTEGER, "INTEGER", typeof(sbyte), typeof(sbyte), 1, 1, false), new MetaType(System.Data.DbType.Single, System.Data.OracleClient.OracleType.Float, OCI.DATATYPE.FLOAT, "FLOAT", typeof(float), typeof(float), 4, 4, false), 
            new MetaType(System.Data.DbType.String, System.Data.OracleClient.OracleType.NVarChar, OCI.DATATYPE.VARCHAR2, "NVARCHAR2", typeof(string), typeof(OracleString), 0, 0xfa0, true), new MetaType(System.Data.DbType.Time, System.Data.OracleClient.OracleType.DateTime, OCI.DATATYPE.DATE, "DATE", typeof(DateTime), typeof(OracleDateTime), 7, 7, false), new MetaType(System.Data.DbType.UInt16, System.Data.OracleClient.OracleType.UInt16, OCI.DATATYPE.UNSIGNEDINT, "UNSIGNED INTEGER", typeof(ushort), typeof(ushort), 2, 2, false), new MetaType(System.Data.DbType.UInt32, System.Data.OracleClient.OracleType.UInt32, OCI.DATATYPE.UNSIGNEDINT, "UNSIGNED INTEGER", typeof(uint), typeof(uint), 4, 4, false), new MetaType(System.Data.DbType.UInt64, System.Data.OracleClient.OracleType.Number, OCI.DATATYPE.VARNUM, "NUMBER", typeof(decimal), typeof(OracleNumber), 0x16, 0x16, false), new MetaType(System.Data.DbType.VarNumeric, System.Data.OracleClient.OracleType.Number, OCI.DATATYPE.VARNUM, "NUMBER", typeof(decimal), typeof(OracleNumber), 0x16, 0x16, false), new MetaType(System.Data.DbType.AnsiStringFixedLength, System.Data.OracleClient.OracleType.Char, OCI.DATATYPE.CHAR, "CHAR", typeof(string), typeof(OracleString), 0, 0x7d0, false), new MetaType(System.Data.DbType.StringFixedLength, System.Data.OracleClient.OracleType.NChar, OCI.DATATYPE.CHAR, "NCHAR", typeof(string), typeof(OracleString), 0, 0x7d0, true)
         };
        internal const int LongMax = 0x7fffffff;
        private const string N_BFILE = "BFILE";
        private const string N_BLOB = "BLOB";
        private const string N_CHAR = "CHAR";
        private const string N_CLOB = "CLOB";
        private const string N_DATE = "DATE";
        private const string N_FLOAT = "FLOAT";
        private const string N_INTEGER = "INTEGER";
        private const string N_INTERVALDS = "INTERVAL DAY TO SECOND";
        private const string N_INTERVALYM = "INTERVAL YEAR TO MONTH";
        private const string N_LONG = "LONG";
        private const string N_LONGRAW = "LONG RAW";
        private const string N_NCHAR = "NCHAR";
        private const string N_NCLOB = "NCLOB";
        private const string N_NUMBER = "NUMBER";
        private const string N_NVARCHAR2 = "NVARCHAR2";
        private const string N_RAW = "RAW";
        private const string N_REFCURSOR = "REF CURSOR";
        private const string N_ROWID = "ROWID";
        private const string N_TIMESTAMP = "TIMESTAMP";
        private const string N_TIMESTAMPLTZ = "TIMESTAMP WITH LOCAL TIME ZONE";
        private const string N_TIMESTAMPTZ = "TIMESTAMP WITH TIME ZONE";
        private const string N_UNSIGNEDINT = "UNSIGNED INTEGER";
        private const string N_VARCHAR2 = "VARCHAR2";
        private static readonly MetaType[] oracleTypeMetaType = new MetaType[0x1f];
        internal static readonly MetaType oracleTypeMetaType_LONGNVARCHAR;
        internal static readonly MetaType oracleTypeMetaType_LONGVARCHAR;
        internal static readonly MetaType oracleTypeMetaType_LONGVARRAW;

        static MetaType()
        {
            oracleTypeMetaType[1] = new MetaType(System.Data.DbType.Binary, System.Data.OracleClient.OracleType.BFile, OCI.DATATYPE.BFILE, "BFILE", typeof(byte[]), typeof(OracleBFile), IntPtr.Size, IntPtr.Size, false);
            oracleTypeMetaType[2] = new MetaType(System.Data.DbType.Binary, System.Data.OracleClient.OracleType.Blob, OCI.DATATYPE.BLOB, "BLOB", typeof(byte[]), typeof(OracleLob), IntPtr.Size, IntPtr.Size, false);
            oracleTypeMetaType[3] = dbTypeMetaType[0x16];
            oracleTypeMetaType[4] = new MetaType(System.Data.DbType.AnsiString, System.Data.OracleClient.OracleType.Clob, OCI.DATATYPE.CLOB, "CLOB", typeof(string), typeof(OracleLob), IntPtr.Size, IntPtr.Size, false);
            oracleTypeMetaType[5] = new MetaType(System.Data.DbType.Object, System.Data.OracleClient.OracleType.Cursor, OCI.DATATYPE.RSET, "REF CURSOR", typeof(object), typeof(object), IntPtr.Size, IntPtr.Size, false);
            oracleTypeMetaType[6] = dbTypeMetaType[6];
            oracleTypeMetaType[8] = new MetaType(System.Data.DbType.Int32, System.Data.OracleClient.OracleType.IntervalYearToMonth, OCI.DATATYPE.INT_INTERVAL_YM, "INTERVAL YEAR TO MONTH", typeof(int), typeof(OracleMonthSpan), 5, 5, false);
            oracleTypeMetaType[7] = new MetaType(System.Data.DbType.Object, System.Data.OracleClient.OracleType.IntervalDayToSecond, OCI.DATATYPE.INT_INTERVAL_DS, "INTERVAL DAY TO SECOND", typeof(TimeSpan), typeof(OracleTimeSpan), 11, 11, false);
            oracleTypeMetaType[9] = new MetaType(System.Data.DbType.Binary, System.Data.OracleClient.OracleType.LongRaw, OCI.DATATYPE.LONGRAW, "LONG RAW", typeof(byte[]), typeof(OracleBinary), 0x7fffffff, 0x7fbc, false);
            oracleTypeMetaType[10] = new MetaType(System.Data.DbType.AnsiString, System.Data.OracleClient.OracleType.LongVarChar, OCI.DATATYPE.LONG, "LONG", typeof(string), typeof(OracleString), 0x7fffffff, 0x7fbc, false);
            oracleTypeMetaType[11] = dbTypeMetaType[0x17];
            oracleTypeMetaType[12] = new MetaType(System.Data.DbType.String, System.Data.OracleClient.OracleType.NClob, OCI.DATATYPE.CLOB, "NCLOB", typeof(string), typeof(OracleLob), IntPtr.Size, IntPtr.Size, true);
            oracleTypeMetaType[13] = dbTypeMetaType[0x15];
            oracleTypeMetaType[14] = dbTypeMetaType[0x10];
            oracleTypeMetaType[15] = dbTypeMetaType[1];
            oracleTypeMetaType[0x10] = new MetaType(System.Data.DbType.AnsiString, System.Data.OracleClient.OracleType.RowId, OCI.DATATYPE.VARCHAR2, "ROWID", typeof(string), typeof(OracleString), 0xf6e, 0xf6e, false);
            oracleTypeMetaType[0x12] = new MetaType(System.Data.DbType.DateTime, System.Data.OracleClient.OracleType.Timestamp, OCI.DATATYPE.INT_TIMESTAMP, "TIMESTAMP", typeof(DateTime), typeof(OracleDateTime), 11, 11, false);
            oracleTypeMetaType[0x13] = new MetaType(System.Data.DbType.DateTime, System.Data.OracleClient.OracleType.TimestampLocal, OCI.DATATYPE.INT_TIMESTAMP_LTZ, "TIMESTAMP WITH LOCAL TIME ZONE", typeof(DateTime), typeof(OracleDateTime), 11, 11, false);
            oracleTypeMetaType[20] = new MetaType(System.Data.DbType.DateTime, System.Data.OracleClient.OracleType.TimestampWithTZ, OCI.DATATYPE.INT_TIMESTAMP_TZ, "TIMESTAMP WITH TIME ZONE", typeof(DateTime), typeof(OracleDateTime), 13, 13, false);
            oracleTypeMetaType[0x16] = dbTypeMetaType[0];
            oracleTypeMetaType[0x17] = dbTypeMetaType[2];
            oracleTypeMetaType[0x18] = dbTypeMetaType[0x12];
            oracleTypeMetaType[0x19] = dbTypeMetaType[0x13];
            oracleTypeMetaType[0x1a] = dbTypeMetaType[14];
            oracleTypeMetaType[0x1b] = dbTypeMetaType[10];
            oracleTypeMetaType[0x1c] = dbTypeMetaType[11];
            oracleTypeMetaType[0x1d] = dbTypeMetaType[15];
            oracleTypeMetaType[30] = dbTypeMetaType[8];
            oracleTypeMetaType_LONGVARCHAR = new MetaType(System.Data.DbType.AnsiString, System.Data.OracleClient.OracleType.VarChar, OCI.DATATYPE.LONGVARCHAR, "VARCHAR2", typeof(string), typeof(OracleString), 0, 0x7fffffff, false);
            oracleTypeMetaType_LONGVARRAW = new MetaType(System.Data.DbType.Binary, System.Data.OracleClient.OracleType.Raw, OCI.DATATYPE.LONGVARRAW, "RAW", typeof(byte[]), typeof(OracleBinary), 0, 0x7fffffff, false);
            oracleTypeMetaType_LONGNVARCHAR = new MetaType(System.Data.DbType.String, System.Data.OracleClient.OracleType.NVarChar, OCI.DATATYPE.LONGVARCHAR, "NVARCHAR2", typeof(string), typeof(OracleString), 0, 0x7fffffff, true);
        }

        public MetaType(System.Data.DbType dbType, System.Data.OracleClient.OracleType oracleType, OCI.DATATYPE ociType, string dataTypeName, Type convertToType, Type noConvertType, int bindSize, int maxBindSize, bool usesNationalCharacterSet)
        {
            this._dbType = dbType;
            this._oracleType = oracleType;
            this._ociType = ociType;
            this._convertToType = convertToType;
            this._noConvertType = noConvertType;
            this._bindSize = bindSize;
            this._maxBindSize = maxBindSize;
            this._dataTypeName = dataTypeName;
            this._usesNationalCharacterSet = usesNationalCharacterSet;
            switch (oracleType)
            {
                case System.Data.OracleClient.OracleType.Char:
                case System.Data.OracleClient.OracleType.Clob:
                case System.Data.OracleClient.OracleType.LongVarChar:
                case System.Data.OracleClient.OracleType.NChar:
                case System.Data.OracleClient.OracleType.NClob:
                case System.Data.OracleClient.OracleType.NVarChar:
                case System.Data.OracleClient.OracleType.VarChar:
                    this._isCharacterType = true;
                    break;
            }
            switch (oracleType)
            {
                case System.Data.OracleClient.OracleType.LongRaw:
                case System.Data.OracleClient.OracleType.LongVarChar:
                    this._isLong = true;
                    break;
            }
            switch (oracleType)
            {
                case System.Data.OracleClient.OracleType.BFile:
                case System.Data.OracleClient.OracleType.Blob:
                case System.Data.OracleClient.OracleType.Clob:
                case System.Data.OracleClient.OracleType.NClob:
                    this._isLob = true;
                    break;

                case System.Data.OracleClient.OracleType.Char:
                    break;

                default:
                    return;
            }
        }

        internal static MetaType GetDefaultMetaType()
        {
            return dbTypeMetaType[0];
        }

        internal static MetaType GetMetaTypeForObject(object value)
        {
            Type type;
            if (value is Type)
            {
                type = (Type) value;
            }
            else
            {
                type = value.GetType();
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    throw System.Data.Common.ADP.InvalidDataType(TypeCode.Empty);

                case TypeCode.Object:
                    if (!(type == typeof(byte[])))
                    {
                        if (type == typeof(Guid))
                        {
                            return dbTypeMetaType[9];
                        }
                        if (type == typeof(object))
                        {
                            throw System.Data.Common.ADP.InvalidDataTypeForValue(type, Type.GetTypeCode(type));
                        }
                        if (type == typeof(OracleBFile))
                        {
                            return oracleTypeMetaType[1];
                        }
                        if (type == typeof(OracleBinary))
                        {
                            return oracleTypeMetaType[15];
                        }
                        if (type == typeof(OracleDateTime))
                        {
                            return oracleTypeMetaType[6];
                        }
                        if (type == typeof(OracleNumber))
                        {
                            return oracleTypeMetaType[13];
                        }
                        if (type == typeof(OracleString))
                        {
                            return oracleTypeMetaType[0x16];
                        }
                        if (type == typeof(OracleLob))
                        {
                            OracleLob lob = (OracleLob) value;
                            switch (lob.LobType)
                            {
                                case System.Data.OracleClient.OracleType.Blob:
                                    return oracleTypeMetaType[2];

                                case System.Data.OracleClient.OracleType.Clob:
                                    return oracleTypeMetaType[4];

                                case System.Data.OracleClient.OracleType.NClob:
                                    return oracleTypeMetaType[12];
                            }
                        }
                        break;
                    }
                    return dbTypeMetaType[1];

                case TypeCode.DBNull:
                    throw System.Data.Common.ADP.InvalidDataType(TypeCode.DBNull);

                case TypeCode.Boolean:
                    return dbTypeMetaType[3];

                case TypeCode.Char:
                    return dbTypeMetaType[2];

                case TypeCode.SByte:
                    return dbTypeMetaType[14];

                case TypeCode.Byte:
                    return dbTypeMetaType[2];

                case TypeCode.Int16:
                    return dbTypeMetaType[10];

                case TypeCode.UInt16:
                    return dbTypeMetaType[0x12];

                case TypeCode.Int32:
                    return dbTypeMetaType[11];

                case TypeCode.UInt32:
                    return dbTypeMetaType[0x13];

                case TypeCode.Int64:
                    return dbTypeMetaType[12];

                case TypeCode.UInt64:
                    return dbTypeMetaType[20];

                case TypeCode.Single:
                    return dbTypeMetaType[15];

                case TypeCode.Double:
                    return dbTypeMetaType[8];

                case TypeCode.Decimal:
                    return dbTypeMetaType[7];

                case TypeCode.DateTime:
                    return dbTypeMetaType[6];

                case TypeCode.String:
                    return dbTypeMetaType[0];

                default:
                    throw System.Data.Common.ADP.UnknownDataTypeCode(type, Type.GetTypeCode(type));
            }
            throw System.Data.Common.ADP.UnknownDataTypeCode(type, Type.GetTypeCode(type));
        }

        internal static MetaType GetMetaTypeForType(System.Data.DbType dbType)
        {
            if ((dbType < System.Data.DbType.AnsiString) || (dbType > System.Data.DbType.StringFixedLength))
            {
                throw System.Data.Common.ADP.InvalidDbType(dbType);
            }
            return dbTypeMetaType[(int) dbType];
        }

        internal static MetaType GetMetaTypeForType(System.Data.OracleClient.OracleType oracleType)
        {
            if ((oracleType < System.Data.OracleClient.OracleType.BFile) || ((oracleType - 1) > System.Data.OracleClient.OracleType.Double))
            {
                throw System.Data.Common.ADP.InvalidOracleType(oracleType);
            }
            return oracleTypeMetaType[(int) oracleType];
        }

        internal Type BaseType
        {
            get
            {
                return this._convertToType;
            }
        }

        internal int BindSize
        {
            get
            {
                return this._bindSize;
            }
        }

        internal string DataTypeName
        {
            get
            {
                return this._dataTypeName;
            }
        }

        internal System.Data.DbType DbType
        {
            get
            {
                return this._dbType;
            }
        }

        internal bool IsCharacterType
        {
            get
            {
                return this._isCharacterType;
            }
        }

        internal bool IsLob
        {
            get
            {
                return this._isLob;
            }
        }

        internal bool IsLong
        {
            get
            {
                return this._isLong;
            }
        }

        internal bool IsVariableLength
        {
            get
            {
                if (this._bindSize != 0)
                {
                    return (0x7fffffff == this._bindSize);
                }
                return true;
            }
        }

        internal int MaxBindSize
        {
            get
            {
                return this._maxBindSize;
            }
        }

        internal Type NoConvertType
        {
            get
            {
                return this._noConvertType;
            }
        }

        internal OCI.DATATYPE OciType
        {
            get
            {
                return this._ociType;
            }
        }

        internal System.Data.OracleClient.OracleType OracleType
        {
            get
            {
                return this._oracleType;
            }
        }

        internal bool UsesNationalCharacterSet
        {
            get
            {
                return this._usesNationalCharacterSet;
            }
        }
    }
}

