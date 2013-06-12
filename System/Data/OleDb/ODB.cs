namespace System.Data.OleDb
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class ODB
    {
        internal const string _Add = "add";
        internal const string _Keyword = "keyword";
        internal const string _Name = "name";
        internal const string _Value = "value";
        internal const int ADODB_AlreadyClosedError = -2146824584;
        internal const int ADODB_NextResultError = -2146825037;
        internal const string Asynchronous_Processing = "asynchronous processing";
        internal const string AttachDBFileName = "attachdbfilename";
        internal const int CacheIncrement = 10;
        internal const string CHARACTER_MAXIMUM_LENGTH = "CHARACTER_MAXIMUM_LENGTH";
        internal const int CLSCTX_ALL = 0x17;
        internal static Guid CLSID_DataLinks = new Guid(0x2206cdb2, 0x19c1, 0x11d1, 0x89, 0xe0, 0, 0xc0, 0x4f, 0xd7, 0xa8, 0x29);
        internal static readonly Guid CLSID_MSDASQL = new Guid(0xc8b522cb, 0x5cf3, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal const string COLUMN_NAME = "COLUMN_NAME";
        internal const string Connect_Timeout = "connect timeout";
        internal const string Current_Catalog = "current catalog";
        internal const string Data_Source = "data source";
        internal const string DATA_TYPE = "DATA_TYPE";
        internal const string DataLinks_CLSID = @"CLSID\{2206CDB2-19C1-11D1-89E0-00C04FD7A829}\InprocServer32";
        internal const uint DB_ALL_EXCEPT_LIKE = 3;
        internal static readonly IntPtr DB_INVALID_HACCESSOR = ADP.PtrZero;
        internal const uint DB_LIKE_ONLY = 2;
        internal static readonly IntPtr DB_NULL_HCHAPTER = ADP.PtrZero;
        internal static readonly IntPtr DB_NULL_HROW = ADP.PtrZero;
        internal const uint DB_SEARCHABLE = 4;
        internal const uint DB_UNSEARCHABLE = 1;
        internal const int DBACCESSOR_PARAMETERDATA = 4;
        internal const int DBACCESSOR_ROWDATA = 2;
        internal static readonly object DBCOL_SPECIALCOL = new Guid(0xc8b52232, 0x5cf3, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal const string DBCOLUMN_BASECATALOGNAME = "DBCOLUMN_BASECATALOGNAME";
        internal const string DBCOLUMN_BASECOLUMNNAME = "DBCOLUMN_BASECOLUMNNAME";
        internal const string DBCOLUMN_BASESCHEMANAME = "DBCOLUMN_BASESCHEMANAME";
        internal const string DBCOLUMN_BASETABLENAME = "DBCOLUMN_BASETABLENAME";
        internal const string DBCOLUMN_COLUMNSIZE = "DBCOLUMN_COLUMNSIZE";
        internal const string DBCOLUMN_FLAGS = "DBCOLUMN_FLAGS";
        internal const string DBCOLUMN_GUID = "DBCOLUMN_GUID";
        internal const string DBCOLUMN_IDNAME = "DBCOLUMN_IDNAME";
        internal const string DBCOLUMN_ISAUTOINCREMENT = "DBCOLUMN_ISAUTOINCREMENT";
        internal const string DBCOLUMN_ISUNIQUE = "DBCOLUMN_ISUNIQUE";
        internal const string DBCOLUMN_KEYCOLUMN = "DBCOLUMN_KEYCOLUMN";
        internal const string DBCOLUMN_NAME = "DBCOLUMN_NAME";
        internal const string DBCOLUMN_NUMBER = "DBCOLUMN_NUMBER";
        internal const string DBCOLUMN_PRECISION = "DBCOLUMN_PRECISION";
        internal const string DBCOLUMN_PROPID = "DBCOLUMN_PROPID";
        internal const string DBCOLUMN_SCALE = "DBCOLUMN_SCALE";
        internal const string DBCOLUMN_TYPE = "DBCOLUMN_TYPE";
        internal const string DBCOLUMN_TYPEINFO = "DBCOLUMN_TYPEINFO";
        internal const int DBCOLUMNFLAGS_ISBOOKMARK = 1;
        internal const int DBCOLUMNFLAGS_ISFIXEDLENGTH = 0x10;
        internal const int DBCOLUMNFLAGS_ISLONG = 0x80;
        internal const int DBCOLUMNFLAGS_ISLONG_DBCOLUMNFLAGS_ISSTREAM = 0x80080;
        internal const int DBCOLUMNFLAGS_ISNULLABLE = 0x20;
        internal const int DBCOLUMNFLAGS_ISNULLABLE_DBCOLUMNFLAGS_MAYBENULL = 0x60;
        internal const int DBCOLUMNFLAGS_ISROW = 0x200000;
        internal const int DBCOLUMNFLAGS_ISROWID_DBCOLUMNFLAGS_ISROWVER = 0x300;
        internal const int DBCOLUMNFLAGS_ISROWSET = 0x100000;
        internal const int DBCOLUMNFLAGS_ISROWSET_DBCOLUMNFLAGS_ISROW = 0x300000;
        internal const int DBCOLUMNFLAGS_WRITE_DBCOLUMNFLAGS_WRITEUNKNOWN = 12;
        internal static Guid DBGUID_DEFAULT = new Guid(0xc8b521fb, 0x5cf3, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid DBGUID_ROW = new Guid(0xc8b522f7, 0x5cf3, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid DBGUID_ROWDEFAULTSTREAM = new Guid(0xc733ab7, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid DBGUID_ROWSET = new Guid(0xc8b522f6, 0x5cf3, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal const string DbInfoKeywords = "DbInfoKeywords";
        internal const int DBKIND_GUID = 6;
        internal const int DBKIND_GUID_NAME = 0;
        internal const int DBKIND_GUID_PROPID = 1;
        internal const int DBKIND_NAME = 2;
        internal const int DBKIND_PGUID_NAME = 3;
        internal const int DBKIND_PGUID_PROPID = 4;
        internal const int DBKIND_PROPID = 5;
        internal const int DBLITERAL_CATALOG_SEPARATOR = 3;
        internal const int DBLITERAL_QUOTE_PREFIX = 15;
        internal const int DBLITERAL_QUOTE_SUFFIX = 0x1c;
        internal const int DBLITERAL_SCHEMA_SEPARATOR = 0x1b;
        internal const int DBLITERAL_TABLE_NAME = 0x11;
        internal const string DBMS_Version = "dbms version";
        internal const int DBPARAMTYPE_INPUT = 1;
        internal const int DBPARAMTYPE_INPUTOUTPUT = 2;
        internal const int DBPARAMTYPE_OUTPUT = 3;
        internal const int DBPARAMTYPE_RETURNVALUE = 4;
        internal const int DBPROP_ACCESSORDER = 0xe7;
        internal const int DBPROP_AUTH_CACHE_AUTHINFO = 5;
        internal const int DBPROP_AUTH_ENCRYPT_PASSWORD = 6;
        internal const int DBPROP_AUTH_INTEGRATED = 7;
        internal const int DBPROP_AUTH_MASK_PASSWORD = 8;
        internal const int DBPROP_AUTH_PASSWORD = 9;
        internal const int DBPROP_AUTH_PERSIST_ENCRYPTED = 10;
        internal const int DBPROP_AUTH_PERSIST_SENSITIVE_AUTHINFO = 11;
        internal const int DBPROP_AUTH_USERID = 12;
        internal const int DBPROP_CATALOGLOCATION = 0x16;
        internal const int DBPROP_COMMANDTIMEOUT = 0x22;
        internal const int DBPROP_CONNECTIONSTATUS = 0xf4;
        internal const int DBPROP_CURRENTCATALOG = 0x25;
        internal const int DBPROP_DATASOURCENAME = 0x26;
        internal const int DBPROP_DBMSNAME = 40;
        internal const int DBPROP_DBMSVER = 0x29;
        internal const int DBPROP_GROUPBY = 0x2c;
        internal const int DBPROP_HIDDENCOLUMNS = 0x102;
        internal const int DBPROP_IColumnsRowset = 0x7b;
        internal const int DBPROP_IDENTIFIERCASE = 0x2e;
        internal const int DBPROP_INIT_ASYNCH = 200;
        internal const int DBPROP_INIT_BINDFLAGS = 270;
        internal const int DBPROP_INIT_CATALOG = 0xe9;
        internal const int DBPROP_INIT_DATASOURCE = 0x3b;
        internal const int DBPROP_INIT_GENERALTIMEOUT = 0x11c;
        internal const int DBPROP_INIT_HWND = 60;
        internal const int DBPROP_INIT_IMPERSONATION_LEVEL = 0x3d;
        internal const int DBPROP_INIT_LCID = 0xba;
        internal const int DBPROP_INIT_LOCATION = 0x3e;
        internal const int DBPROP_INIT_LOCKOWNER = 0x10f;
        internal const int DBPROP_INIT_MODE = 0x3f;
        internal const int DBPROP_INIT_OLEDBSERVICES = 0xf8;
        internal const int DBPROP_INIT_PROMPT = 0x40;
        internal const int DBPROP_INIT_PROTECTION_LEVEL = 0x41;
        internal const int DBPROP_INIT_PROVIDERSTRING = 160;
        internal const int DBPROP_INIT_TIMEOUT = 0x42;
        internal const int DBPROP_IRow = 0x107;
        internal const int DBPROP_MAXROWS = 0x49;
        internal const int DBPROP_MULTIPLERESULTS = 0xc4;
        internal const int DBPROP_ORDERBYCOLUNSINSELECT = 0x55;
        internal const int DBPROP_PROVIDERFILENAME = 0x60;
        internal const int DBPROP_QUOTEDIDENTIFIERCASE = 100;
        internal const int DBPROP_RESETDATASOURCE = 0xf7;
        internal const int DBPROP_SQLSUPPORT = 0x6d;
        internal const int DBPROP_UNIQUEROWS = 0xee;
        internal const int DBPROPFLAGS_SESSION = 0x1000;
        internal const int DBPROPFLAGS_WRITE = 0x400;
        internal const int DBPROPOPTIONS_OPTIONAL = 1;
        internal const int DBPROPOPTIONS_REQUIRED = 0;
        internal const int DBPROPSTATUS_BADCOLUMN = 4;
        internal const int DBPROPSTATUS_BADOPTION = 3;
        internal const int DBPROPSTATUS_BADVALUE = 2;
        internal const int DBPROPSTATUS_CONFLICTING = 8;
        internal const int DBPROPSTATUS_NOTALLSETTABLE = 5;
        internal const int DBPROPSTATUS_NOTAVAILABLE = 9;
        internal const int DBPROPSTATUS_NOTSET = 7;
        internal const int DBPROPSTATUS_NOTSETTABLE = 6;
        internal const int DBPROPSTATUS_NOTSUPPORTED = 1;
        internal const int DBPROPSTATUS_OK = 0;
        internal const int DBPROPVAL_AO_RANDOM = 2;
        internal const int DBPROPVAL_CL_END = 2;
        internal const int DBPROPVAL_CL_START = 1;
        internal const int DBPROPVAL_CS_COMMUNICATIONFAILURE = 2;
        internal const int DBPROPVAL_CS_INITIALIZED = 1;
        internal const int DBPROPVAL_CS_UNINITIALIZED = 0;
        internal const int DBPROPVAL_GB_COLLATE = 0x10;
        internal const int DBPROPVAL_GB_CONTAINS_SELECT = 4;
        internal const int DBPROPVAL_GB_EQUALS_SELECT = 2;
        internal const int DBPROPVAL_GB_NO_RELATION = 8;
        internal const int DBPROPVAL_GB_NOT_SUPPORTED = 1;
        internal const int DBPROPVAL_IC_LOWER = 2;
        internal const int DBPROPVAL_IC_MIXED = 8;
        internal const int DBPROPVAL_IC_SENSITIVE = 4;
        internal const int DBPROPVAL_IC_UPPER = 1;
        internal const int DBPROPVAL_IN_ALLOWNULL = 0;
        internal const int DBPROPVAL_MR_NOTSUPPORTED = 0;
        internal const int DBPROPVAL_OS_AGR_AFTERSESSION = 8;
        internal const int DBPROPVAL_OS_CLIENTCURSOR = 4;
        internal const int DBPROPVAL_OS_RESOURCEPOOLING = 1;
        internal const int DBPROPVAL_OS_TXNENLISTMENT = 2;
        internal const int DBPROPVAL_RD_RESETALL = -1;
        internal const int DBPROPVAL_SQL_ESCAPECLAUSES = 0x100;
        internal const int DBPROPVAL_SQL_ODBC_MINIMUM = 1;
        internal static readonly IntPtr DBRESULTFLAG_DEFAULT = IntPtr.Zero;
        internal const string DefaultDescription_MSDASQL = "microsoft ole db provider for odbc drivers";
        internal static readonly char[] ErrorTrimCharacters;
        internal const int ExecutedIMultipleResults = 0;
        internal const int ExecutedIRow = 2;
        internal const int ExecutedIRowset = 1;
        internal const string File_Name = "file name";
        internal static Guid IID_ICommandText = new Guid(0xc733a27, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_IDBCreateCommand = new Guid(0xc733a1d, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_IDBCreateSession = new Guid(0xc733a5d, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_IDBInitialize = new Guid(0xc733a8b, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_IMultipleResults = new Guid(0xc733a90, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_IRow = new Guid(0xc733ab4, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_IRowset = new Guid(0xc733a7c, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_ISQLErrorInfo = new Guid(0xc733a74, 0x2a1c, 0x11ce, 0xad, 0xe5, 0, 170, 0, 0x44, 0x77, 0x3d);
        internal static Guid IID_IUnknown = new Guid(0, 0, 0, 0xc0, 0, 0, 0, 0, 0, 0, 70);
        internal static Guid IID_NULL = Guid.Empty;
        internal const string INDEX_NAME = "INDEX_NAME";
        internal const string Initial_Catalog = "initial catalog";
        internal const int InternalStateClosed = 0;
        internal const int InternalStateConnecting = 2;
        internal const int InternalStateExecuting = 5;
        internal const int InternalStateExecutingNot = -5;
        internal const int InternalStateFetching = 9;
        internal const int InternalStateFetchingNot = -9;
        internal const int InternalStateOpen = 1;
        internal const string IS_NULLABLE = "IS_NULLABLE";
        internal const string Keyword = "Keyword";
        internal const int LargeDataSize = 0x2000;
        internal const int MaxProgIdLength = 0xff;
        internal const string MSDASQL = "msdasql";
        internal const string MSDASQLdot = "msdasql.";
        internal const string NULLS = "NULLS";
        internal const string NUMERIC_PRECISION = "NUMERIC_PRECISION";
        internal const string NUMERIC_SCALE = "NUMERIC_SCALE";
        internal static readonly int OffsetOf_tagDBBINDING_obValue = Marshal.OffsetOf(typeof(tagDBBINDING), "obValue").ToInt32();
        internal static readonly int OffsetOf_tagDBBINDING_wType = Marshal.OffsetOf(typeof(tagDBBINDING), "wType").ToInt32();
        internal static readonly int OffsetOf_tagDBLITERALINFO_it = Marshal.OffsetOf(typeof(tagDBLITERALINFO), "it").ToInt32();
        internal static readonly int OffsetOf_tagDBPROP_Status = Marshal.OffsetOf(typeof(tagDBPROP), "dwStatus").ToInt32();
        internal static readonly int OffsetOf_tagDBPROP_Value = Marshal.OffsetOf(typeof(tagDBPROP), "vValue").ToInt32();
        internal static readonly int OffsetOf_tagDBPROPIDSET_PropertySet = Marshal.OffsetOf(typeof(tagDBPROPIDSET), "guidPropertySet").ToInt32();
        internal static readonly int OffsetOf_tagDBPROPINFO_Value = Marshal.OffsetOf(typeof(tagDBPROPINFO), "vValue").ToInt32();
        internal static readonly int OffsetOf_tagDBPROPSET_Properties = Marshal.OffsetOf(typeof(tagDBPROPSET), "rgProperties").ToInt32();
        internal const string OLEDB_SERVICES = "OLEDB_SERVICES";
        internal const string ORDINAL_POSITION = "ORDINAL_POSITION";
        internal const string ORDINAL_POSITION_ASC = "ORDINAL_POSITION ASC";
        internal const string PARAMETER_NAME = "PARAMETER_NAME";
        internal const string PARAMETER_TYPE = "PARAMETER_TYPE";
        internal const int ParameterDirectionFlag = 3;
        internal const string Password = "password";
        internal const string Persist_Security_Info = "persist security info";
        internal const int PrepareICommandText = 3;
        internal const string PRIMARY_KEY = "PRIMARY_KEY";
        internal const string Properties = "Properties";
        internal const string Provider = "provider";
        internal const string Pwd = "pwd";
        internal const string RestrictionSupport = "RestrictionSupport";
        internal const string Schema = "Schema";
        internal const string SchemaGuids = "SchemaGuids";
        internal static readonly int SizeOf_Guid = Marshal.SizeOf(typeof(Guid));
        internal static readonly int SizeOf_tagDBBINDING = Marshal.SizeOf(typeof(tagDBBINDING));
        internal static readonly int SizeOf_tagDBCOLUMNINFO = Marshal.SizeOf(typeof(tagDBCOLUMNINFO));
        internal static readonly int SizeOf_tagDBLITERALINFO = Marshal.SizeOf(typeof(tagDBLITERALINFO));
        internal static readonly int SizeOf_tagDBPROP = Marshal.SizeOf(typeof(tagDBPROP));
        internal static readonly int SizeOf_tagDBPROPIDSET = Marshal.SizeOf(typeof(tagDBPROPIDSET));
        internal static readonly int SizeOf_tagDBPROPINFO = Marshal.SizeOf(typeof(tagDBPROPINFO));
        internal static readonly int SizeOf_tagDBPROPINFOSET = Marshal.SizeOf(typeof(tagDBPROPINFOSET));
        internal static readonly int SizeOf_tagDBPROPSET = Marshal.SizeOf(typeof(tagDBPROPSET));
        internal static readonly int SizeOf_Variant = (8 + (2 * ADP.PtrSize));
        internal const string TYPE_NAME = "TYPE_NAME";
        internal const string UNIQUE = "UNIQUE";
        internal const string User_ID = "user id";
        internal const short VARIANT_FALSE = 0;
        internal const short VARIANT_TRUE = -1;

        static ODB()
        {
            char[] chArray = new char[3];
            chArray[0] = '\r';
            chArray[1] = '\n';
            ErrorTrimCharacters = chArray;
        }

        internal static ArgumentException AsynchronousNotSupported()
        {
            return ADP.Argument(Res.GetString("OleDb_AsynchronousNotSupported"));
        }

        internal static InvalidOperationException BadAccessor()
        {
            return ADP.DataAdapter(Res.GetString("OleDb_BadAccessor"));
        }

        internal static Exception BadStatus_ParamAcc(int index, DBBindStatus status)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_BadStatus_ParamAcc", new object[] { index.ToString(CultureInfo.InvariantCulture), status.ToString() }));
        }

        internal static InvalidOperationException BadStatusRowAccessor(int i, DBBindStatus rowStatus)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_BadStatusRowAccessor", new object[] { i.ToString(CultureInfo.InvariantCulture), rowStatus.ToString() }));
        }

        internal static InvalidCastException CantConvertValue()
        {
            return ADP.InvalidCast(Res.GetString("OleDb_CantConvertValue"));
        }

        internal static InvalidOperationException CantCreate(Type type)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_CantCreate", new object[] { type.Name }));
        }

        internal static Exception CommandParameterStatus(string value, Exception inner)
        {
            if (ADP.IsEmpty(value))
            {
                return inner;
            }
            return ADP.InvalidOperation(value, inner);
        }

        internal static void CommandParameterStatus(StringBuilder builder, int index, DBStatus status)
        {
            switch (status)
            {
                case DBStatus.S_OK:
                case DBStatus.S_ISNULL:
                case DBStatus.S_IGNORE:
                    return;

                case DBStatus.E_BADACCESSOR:
                    builder.Append(Res.GetString("OleDb_CommandParameterBadAccessor", new object[] { index.ToString(CultureInfo.InvariantCulture), "" }));
                    builder.Append(Environment.NewLine);
                    return;

                case DBStatus.E_CANTCONVERTVALUE:
                    builder.Append(Res.GetString("OleDb_CommandParameterCantConvertValue", new object[] { index.ToString(CultureInfo.InvariantCulture), "" }));
                    builder.Append(Environment.NewLine);
                    return;

                case DBStatus.E_SIGNMISMATCH:
                    builder.Append(Res.GetString("OleDb_CommandParameterSignMismatch", new object[] { index.ToString(CultureInfo.InvariantCulture), "" }));
                    builder.Append(Environment.NewLine);
                    return;

                case DBStatus.E_DATAOVERFLOW:
                    builder.Append(Res.GetString("OleDb_CommandParameterDataOverflow", new object[] { index.ToString(CultureInfo.InvariantCulture), "" }));
                    builder.Append(Environment.NewLine);
                    return;

                case DBStatus.E_UNAVAILABLE:
                    builder.Append(Res.GetString("OleDb_CommandParameterUnavailable", new object[] { index.ToString(CultureInfo.InvariantCulture), "" }));
                    builder.Append(Environment.NewLine);
                    return;

                case DBStatus.S_DEFAULT:
                    builder.Append(Res.GetString("OleDb_CommandParameterDefault", new object[] { index.ToString(CultureInfo.InvariantCulture), "" }));
                    builder.Append(Environment.NewLine);
                    return;
            }
            builder.Append(Res.GetString("OleDb_CommandParameterError", new object[] { index.ToString(CultureInfo.InvariantCulture), status.ToString() }));
            builder.Append(Environment.NewLine);
        }

        internal static InvalidOperationException CommandTextNotSupported(string provider, Exception inner)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_CommandTextNotSupported", new object[] { provider }), inner);
        }

        internal static InvalidCastException ConversionRequired()
        {
            return ADP.InvalidCast();
        }

        internal static InvalidOperationException DataOverflow(Type type)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_DataOverflow", new object[] { type.Name }));
        }

        internal static InvalidOperationException DBBindingGetVector()
        {
            return ADP.InvalidOperation(Res.GetString("OleDb_DBBindingGetVector"));
        }

        internal static string ELookup(OleDbHResult hr)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(hr.ToString());
            if ((0 < builder.Length) && char.IsDigit(builder[0]))
            {
                builder.Length = 0;
            }
            builder.Append("(0x");
            builder.Append(((int) hr).ToString("X8", CultureInfo.InvariantCulture));
            builder.Append(")");
            return builder.ToString();
        }

        internal static string FailedGetDescription(OleDbHResult errorcode)
        {
            return Res.GetString("OleDb_FailedGetDescription", new object[] { ELookup(errorcode) });
        }

        internal static string FailedGetSource(OleDbHResult errorcode)
        {
            return Res.GetString("OleDb_FailedGetSource", new object[] { ELookup(errorcode) });
        }

        internal static ArgumentException Fill_EmptyRecord(string parameter, Exception innerException)
        {
            return ADP.Argument(Res.GetString("OleDb_Fill_EmptyRecord"), parameter, innerException);
        }

        internal static ArgumentException Fill_EmptyRecordSet(string parameter, Exception innerException)
        {
            return ADP.Argument(Res.GetString("OleDb_Fill_EmptyRecordSet", new object[] { "IRowset" }), parameter, innerException);
        }

        internal static ArgumentException Fill_NotADODB(string parameter)
        {
            return ADP.Argument(Res.GetString("OleDb_Fill_NotADODB"), parameter);
        }

        internal static OleDbHResult GetErrorDescription(UnsafeNativeMethods.IErrorInfo errorInfo, OleDbHResult hresult, out string message)
        {
            Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS>\n");
            OleDbHResult description = errorInfo.GetDescription(out message);
            Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS|RET> %08X{HRESULT}, Message='%ls'\n", description, message);
            if ((description < OleDbHResult.S_OK) && ADP.IsEmpty(message))
            {
                message = FailedGetDescription(description) + Environment.NewLine + ELookup(hresult);
            }
            if (ADP.IsEmpty(message))
            {
                message = ELookup(hresult);
            }
            return description;
        }

        internal static InvalidOperationException GVtUnknown(int wType)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_GVtUnknown", new object[] { wType.ToString("X4", CultureInfo.InvariantCulture), wType.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static InvalidOperationException IDBInfoNotSupported()
        {
            return ADP.InvalidOperation(Res.GetString("OleDb_IDBInfoNotSupported"));
        }

        internal static Exception InvalidOleDbType(OleDbType value)
        {
            return ADP.InvalidEnumerationValue(typeof(OleDbType), (int) value);
        }

        internal static ArgumentException InvalidProviderSpecified()
        {
            return ADP.Argument(Res.GetString("OleDb_InvalidProviderSpecified"));
        }

        internal static ArgumentException InvalidRestrictionsDbInfoKeywords(string parameter)
        {
            return ADP.Argument(Res.GetString("OleDb_InvalidRestrictionsDbInfoKeywords"), parameter);
        }

        internal static ArgumentException InvalidRestrictionsDbInfoLiteral(string parameter)
        {
            return ADP.Argument(Res.GetString("OleDb_InvalidRestrictionsDbInfoLiteral"), parameter);
        }

        internal static ArgumentException InvalidRestrictionsSchemaGuids(string parameter)
        {
            return ADP.Argument(Res.GetString("OleDb_InvalidRestrictionsSchemaGuids"), parameter);
        }

        internal static ArgumentException ISourcesRowsetNotSupported()
        {
            throw ADP.Argument("OleDb_ISourcesRowsetNotSupported");
        }

        internal static InvalidOperationException MDACNotAvailable(Exception inner)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_MDACNotAvailable"), inner);
        }

        internal static ArgumentException MSDASQLNotSupported()
        {
            return ADP.Argument(Res.GetString("OleDb_MSDASQLNotSupported"));
        }

        internal static OleDbException NoErrorInformation(string provider, OleDbHResult hr, Exception inner)
        {
            OleDbException exception;
            if (!ADP.IsEmpty(provider))
            {
                exception = new OleDbException(Res.GetString("OleDb_NoErrorInformation2", new object[] { provider, ELookup(hr) }), hr, inner);
            }
            else
            {
                exception = new OleDbException(Res.GetString("OleDb_NoErrorInformation", new object[] { ELookup(hr) }), hr, inner);
            }
            ADP.TraceExceptionAsReturnValue(exception);
            return exception;
        }

        internal static string NoErrorMessage(OleDbHResult errorcode)
        {
            return Res.GetString("OleDb_NoErrorMessage", new object[] { ELookup(errorcode) });
        }

        internal static ArgumentException NoProviderSpecified()
        {
            return ADP.Argument(Res.GetString("OleDb_NoProviderSpecified"));
        }

        internal static Exception NoProviderSupportForParameters(string provider, Exception inner)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_NoProviderSupportForParameters", new object[] { provider }), inner);
        }

        internal static Exception NoProviderSupportForSProcResetParameters(string provider)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_NoProviderSupportForSProcResetParameters", new object[] { provider }));
        }

        internal static ArgumentException NotSupportedSchemaTable(Guid schema, OleDbConnection connection)
        {
            return ADP.Argument(Res.GetString("OleDb_NotSupportedSchemaTable", new object[] { OleDbSchemaGuid.GetTextFromValue(schema), connection.Provider }));
        }

        internal static InvalidOperationException PossiblePromptNotUserInteractive()
        {
            return ADP.DataAdapter(Res.GetString("OleDb_PossiblePromptNotUserInteractive"));
        }

        internal static Exception PropsetSetFailure(string value, Exception inner)
        {
            if (ADP.IsEmpty(value))
            {
                return inner;
            }
            return ADP.InvalidOperation(value, inner);
        }

        internal static void PropsetSetFailure(StringBuilder builder, string description, OleDbPropertyStatus status)
        {
            switch (status)
            {
                case OleDbPropertyStatus.NotSupported:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyNotSupported", new object[] { description }));
                    return;

                case OleDbPropertyStatus.BadValue:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyBadValue", new object[] { description }));
                    return;

                case OleDbPropertyStatus.BadOption:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyBadOption", new object[] { description }));
                    return;

                case OleDbPropertyStatus.BadColumn:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyBadColumn", new object[] { description }));
                    return;

                case OleDbPropertyStatus.NotAllSettable:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyNotAllSettable", new object[] { description }));
                    return;

                case OleDbPropertyStatus.NotSettable:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyNotSettable", new object[] { description }));
                    return;

                case OleDbPropertyStatus.NotSet:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyNotSet", new object[] { description }));
                    return;

                case OleDbPropertyStatus.Conflicting:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyConflicting", new object[] { description }));
                    return;

                case OleDbPropertyStatus.NotAvailable:
                    if (0 < builder.Length)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(Res.GetString("OleDb_PropertyNotAvailable", new object[] { description }));
                    return;

                case OleDbPropertyStatus.Ok:
                    return;
            }
            if (0 < builder.Length)
            {
                builder.Append(Environment.NewLine);
            }
            object[] args = new object[] { ((int) status).ToString(CultureInfo.InvariantCulture) };
            builder.Append(Res.GetString("OleDb_PropertyStatusUnknown", args));
        }

        internal static InvalidOperationException ProviderUnavailable(string provider, Exception inner)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_ProviderUnavailable", new object[] { provider }), inner);
        }

        internal static ArgumentException SchemaRowsetsNotSupported(string provider)
        {
            return ADP.Argument(Res.GetString("OleDb_SchemaRowsetsNotSupported", new object[] { "IDBSchemaRowset", provider }));
        }

        internal static InvalidOperationException SignMismatch(Type type)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_SignMismatch", new object[] { type.Name }));
        }

        internal static InvalidOperationException SVtUnknown(int wType)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_SVtUnknown", new object[] { wType.ToString("X4", CultureInfo.InvariantCulture), wType.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static InvalidOperationException ThreadApartmentState(Exception innerException)
        {
            return ADP.InvalidOperation(Res.GetString("OleDb_ThreadApartmentState"), innerException);
        }

        internal static InvalidOperationException TransactionsNotSupported(string provider, Exception inner)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_TransactionsNotSupported", new object[] { provider }), inner);
        }

        internal static InvalidOperationException Unavailable(Type type)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_Unavailable", new object[] { type.Name }));
        }

        internal static InvalidOperationException UnexpectedStatusValue(DBStatus status)
        {
            return ADP.DataAdapter(Res.GetString("OleDb_UnexpectedStatusValue", new object[] { status.ToString() }));
        }

        internal static Exception UninitializedParameters(int index, OleDbType dbtype)
        {
            return ADP.InvalidOperation(Res.GetString("OleDb_UninitializedParameters", new object[] { index.ToString(CultureInfo.InvariantCulture), dbtype.ToString() }));
        }
    }
}

