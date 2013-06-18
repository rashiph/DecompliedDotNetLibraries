namespace System.Data.SqlClient
{
    using System;
    using System.Data;

    internal static class TdsEnums
    {
        internal const ushort ABORT = 210;
        public const string ADSP = "adsp";
        public const int AOPANY = 0x53;
        public const int AOPAVG = 0x4f;
        public const int AOPCNT = 0x4b;
        public const int AOPCNTB = 9;
        public const int AOPMAX = 0x52;
        public const int AOPMIN = 0x51;
        public const int AOPNOOP = 0x56;
        public const int AOPSTDEV = 0x30;
        public const int AOPSTDEVP = 0x31;
        public const int AOPSUM = 0x4d;
        public const int AOPVAR = 50;
        public const int AOPVARP = 0x33;
        internal const ushort BEGINXACT = 0xd4;
        public const string BROWSE_OFF = " SET NO_BROWSETABLE OFF;";
        public const string BROWSE_ON = " SET NO_BROWSETABLE ON;";
        internal const ushort BULKINSERT = 240;
        public const string BV = "bv";
        public const short CHARSET_CODE_PAGE_OFFSET = 2;
        public const int CLIENT_PROG_VER = 0x6000000;
        public const byte ClrFixedLen = 1;
        public static readonly ushort[] CODE_PAGE_FROM_SORT_ID = new ushort[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1b5, 0x1b5, 
            0x1b5, 0x1b5, 0x1b5, 0, 0, 0, 0, 0, 850, 850, 850, 850, 850, 0, 0, 0, 
            0, 850, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 850, 850, 850, 850, 850, 850, 850, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0, 0, 0, 0, 
            0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 0x4e2, 
            0x4e2, 0x4e2, 0x4e2, 0, 0, 0, 0, 0, 0x4e3, 0x4e3, 0x4e3, 0x4e3, 0x4e3, 0, 0, 0, 
            0x4e5, 0x4e5, 0x4e5, 0, 0, 0, 0, 0, 0x4e5, 0x4e5, 0x4e5, 0, 0x4e5, 0, 0, 0, 
            0x4e6, 0x4e6, 0x4e6, 0, 0, 0, 0, 0, 0x4e7, 0x4e7, 0x4e7, 0, 0, 0, 0, 0, 
            0x4e8, 0x4e8, 0x4e8, 0, 0, 0, 0, 0, 0x4e9, 0x4e9, 0x4e9, 0x4e9, 0x4e9, 0x4e9, 0x4e9, 0x4e9, 
            0x4e9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0, 0, 0, 0, 0, 
            0x3a4, 0x3a4, 0x3b5, 0x3b5, 950, 950, 0x3a8, 0x3a8, 0x3a4, 0x3b5, 950, 0x3a8, 0x36a, 0x36a, 0x36a, 0, 
            0, 0, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0x4e4, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
         };
        public const int COLLATION_INFO_LEN = 4;
        public const string CONNECTION_GET_SVR_USER = "ConnectionGetSvrUser";
        public const string DEFAULT_ENGLISH_CODE_PAGE_STRING = "iso_1";
        public const short DEFAULT_ENGLISH_CODE_PAGE_VALUE = 0x4e4;
        public const int DEFAULT_LOGIN_PACKET_SIZE = 0x1000;
        public const int DEFAULT_MINOR = 0;
        public const int DEFAULT_NUMERIC_PRECISION = 0x1d;
        public const int DEFAULT_VARTIME_SCALE = 7;
        internal const ushort DELETE = 0xc4;
        public const int DONE_ATTN = 0x20;
        public const int DONE_COUNT = 0x10;
        public const int DONE_ERROR = 2;
        public const int DONE_FMTSENT = 0x8000;
        public const int DONE_INPROC = 0x40;
        public const int DONE_INXACT = 4;
        public const int DONE_MORE = 1;
        public const int DONE_PROC = 8;
        public const int DONE_RPCINBATCH = 0x80;
        public const int DONE_SRVERROR = 0x100;
        public const short ENCRYPTION_NOT_SUPPORTED = 20;
        internal const ushort ENDXACT = 0xd5;
        public const byte ENV_BEGINTRAN = 8;
        public const byte ENV_CHARSET = 3;
        public const byte ENV_COLLATION = 7;
        public const byte ENV_COMMITTRAN = 9;
        public const byte ENV_COMPFLAGS = 6;
        public const byte ENV_DATABASE = 1;
        public const byte ENV_DEFECTDTC = 12;
        public const byte ENV_ENLISTDTC = 11;
        public const byte ENV_LANG = 2;
        public const byte ENV_LOCALEID = 5;
        public const byte ENV_LOGSHIPNODE = 13;
        public const byte ENV_PACKETSIZE = 4;
        public const byte ENV_PROMOTETRANSACTION = 15;
        public const byte ENV_ROLLBACKTRAN = 10;
        public const byte ENV_ROUTING = 20;
        public const byte ENV_SPRESETCONNECTIONACK = 0x12;
        public const byte ENV_TRANSACTIONENDED = 0x11;
        public const byte ENV_TRANSACTIONMANAGERADDRESS = 0x10;
        public const byte ENV_USERINSTANCE = 0x13;
        public const int EXEC_THRESHOLD = 3;
        public const int FAIL = 0;
        public const byte FATAL_ERROR_CLASS = 20;
        public const byte FIXEDNULL = 0;
        public const string FMTONLY_OFF = " SET FMTONLY OFF;";
        public const string FMTONLY_ON = " SET FMTONLY ON;";
        public const string GEN_CLIENT_CONTEXT = "GenClientContext";
        public const byte HARDFLUSH = 1;
        public const int HEADER_LEN = 8;
        public const int HEADER_LEN_FIELD_OFFSET = 2;
        public const int HEADERTYPE_MARS = 2;
        public const byte Identity = 0x10;
        public const byte IGNORE = 2;
        public const int IMPERSONATION_FAILED = 0x542;
        public const int INIT_DB_FATAL = 1;
        public const int INIT_LANG_FATAL = 1;
        public const string INIT_SESSION = "InitSession";
        public const string INIT_SSPI_PACKAGE = "InitSSPIPackage";
        internal const ushort INSERT = 0xc3;
        public const bool Is68K = false;
        public const byte IsColumnSet = 4;
        public const int KATMAI_INCREMENT = 10;
        public const int KATMAI_MAJOR = 0x73;
        public const int KATMAI_MINOR = 3;
        public const int LOGON_FAILED = 0x4818;
        public const string LPC = "lpc";
        public const int MARS_ID_OFFSET = 8;
        public const byte MAX_LOG_NAME = 30;
        public const byte MAX_NIC_SIZE = 6;
        public const int MAX_NUMERIC_LEN = 0x11;
        public const int MAX_NUMERIC_PRECISION = 0x26;
        public const int MAX_PACKET_SIZE = 0x8000;
        public const int MAX_PARAMETER_NAME_LENGTH = 0x80;
        public const byte MAX_PK_LEN = 6;
        public const int MAX_PRELOGIN_PAYLOAD_LENGTH = 0x400;
        public const byte MAX_PROG_NAME = 10;
        public const int MAX_SERVER_USER_NAME = 0x100;
        internal const int MAX_SERVERNAME = 0xff;
        public const byte MAX_USER_CORRECTABLE_ERROR_CLASS = 0x10;
        internal const ushort MAXLEN_APPNAME = 0x80;
        internal const ushort MAXLEN_ATTACHDBFILE = 260;
        internal const ushort MAXLEN_CLIENTINTERFACE = 0x80;
        internal const ushort MAXLEN_DATABASE = 0x80;
        internal const ushort MAXLEN_HOSTNAME = 0x80;
        internal const ushort MAXLEN_LANGUAGE = 0x80;
        internal const ushort MAXLEN_NEWPASSWORD = 0x80;
        internal const ushort MAXLEN_PASSWORD = 0x80;
        internal const ushort MAXLEN_SERVERNAME = 0x80;
        internal const ushort MAXLEN_USERNAME = 0x80;
        public const int MAXSIZE = 0x1f40;
        internal const ushort MERGE = 0x117;
        public const byte MIN_ERROR_CLASS = 11;
        public const int MIN_PACKET_SIZE = 0x200;
        public const byte MT_ACK = 11;
        public const byte MT_ATTN = 6;
        public const byte MT_BINARY = 5;
        public const byte MT_BULK = 7;
        public const byte MT_CLOSE = 9;
        public const byte MT_ECHO = 12;
        public const byte MT_ERROR = 10;
        public const byte MT_LOGIN = 2;
        public const byte MT_LOGIN7 = 0x10;
        public const byte MT_LOGOUT = 13;
        public const byte MT_OLEDB = 15;
        public const byte MT_OPEN = 8;
        public const byte MT_PRELOGIN = 0x12;
        public const byte MT_RPC = 3;
        public const byte MT_SQL = 1;
        public const byte MT_SSPI = 0x11;
        public const byte MT_TOKENS = 4;
        public const byte MT_TRANS = 14;
        public const string NP = "np";
        public const byte Nullable = 1;
        public const int ODBC_ON = 1;
        internal const ushort OPENCURSOR = 0x20;
        public const int ORDER_68000 = 1;
        public const int P_TOKENTOOLONG = 0x67;
        public const string PARAM_OUTPUT = "output";
        public const int PASSWORD_EXPIRED = 0x4838;
        public const int READONLY_INTENT_ON = 1;
        public const int REPL_ON = 3;
        public const string RPC = "rpc";
        public const byte RPC_NOMETADATA = 2;
        public const byte RPC_PARAM_BYREF = 1;
        public const byte RPC_PARAM_DEFAULT = 2;
        public const byte RPC_PARAM_IS_LOB_COOKIE = 8;
        public const ushort RPC_PROCID_CURSOR = 1;
        public const ushort RPC_PROCID_CURSORCLOSE = 9;
        public const ushort RPC_PROCID_CURSOREXECUTE = 4;
        public const ushort RPC_PROCID_CURSORFETCH = 7;
        public const ushort RPC_PROCID_CURSOROPEN = 2;
        public const ushort RPC_PROCID_CURSOROPTION = 8;
        public const ushort RPC_PROCID_CURSORPREPARE = 3;
        public const ushort RPC_PROCID_CURSORPREPEXEC = 5;
        public const ushort RPC_PROCID_CURSORUNPREPARE = 6;
        public const ushort RPC_PROCID_EXECUTE = 12;
        public const ushort RPC_PROCID_EXECUTESQL = 10;
        public const ushort RPC_PROCID_PREPARE = 11;
        public const ushort RPC_PROCID_PREPEXEC = 13;
        public const ushort RPC_PROCID_PREPEXECRPC = 14;
        public const ushort RPC_PROCID_UNPREPARE = 15;
        public const byte RPC_RECOMPILE = 1;
        public const string SDCI_MAPFILENAME = "SqlClientSSDebug";
        public const byte SDCI_MAX_DATA = 0xff;
        public const byte SDCI_MAX_DLLNAME = 0x10;
        public const byte SDCI_MAX_MACHINENAME = 0x20;
        public const byte SEC_COMP_LEN = 8;
        internal const ushort SELECT = 0xc1;
        public const int SET_LANG_ON = 1;
        public const int SHILOH_INCREMENT = 1;
        public const byte SHILOH_RPCBATCHFLAG = 0x80;
        public const int SHILOHSP1_INCREMENT = 0;
        public const int SHILOHSP1_MAJOR = 0x71;
        public const int SHILOHSP1_MINOR = 1;
        public const SqlDbType SmallVarBinary = (SqlDbType.SmallInt | SqlDbType.Int);
        public const uint SNI_SSL_USE_SCHANNEL_CACHE = 2;
        public const uint SNI_SSL_VALIDATE_CERTIFICATE = 1;
        public const uint SNI_SUCCESS = 0;
        public const uint SNI_SUCCESS_IO_PENDING = 0x3e5;
        public const uint SNI_UNINITIALIZED = uint.MaxValue;
        public const uint SNI_WAIT_TIMEOUT = 0x102;
        public const short SNI_WSAECONNRESET = 0x2746;
        public const byte SOFTFLUSH = 0;
        public const string SP_EXECUTE = "sp_execute";
        public const string SP_EXECUTESQL = "sp_executesql";
        public const string SP_PARAMS = "sp_procedure_params_rowset";
        public const string SP_PARAMS_MANAGED = "sp_procedure_params_managed";
        public const string SP_PARAMS_MGD10 = "sp_procedure_params_100_managed";
        public const string SP_PREPARE = "sp_prepare";
        public const string SP_PREPEXEC = "sp_prepexec";
        public const string SP_SDIDEBUG = "sp_sdidebug";
        public const string SP_UNPREPARE = "sp_unprepare";
        public const int SPHINX_DEFAULT_NUMERIC_PRECISION = 0x1c;
        public const int SPHINX_INCREMENT = 0;
        public const int SPHINXORSHILOH_MAJOR = 7;
        public const string SPX = "spx";
        public const int SQL_PLP_CHUNK_TERMINATOR = 0;
        public const ulong SQL_PLP_NULL = ulong.MaxValue;
        public const ulong SQL_PLP_UNKNOWNLEN = 18446744073709551614L;
        public const string SQL_PROVIDER_NAME = ".Net SqlClient Data Provider";
        public const short SQL_SERVER_VERSION_SEVEN = 7;
        public static readonly decimal SQL_SMALL_MONEY_MAX = 214748.3647M;
        public static readonly decimal SQL_SMALL_MONEY_MIN = -214748.3648M;
        public const ushort SQL_USHORTVARMAXLEN = 0xffff;
        public const byte SQLALTCONTROL = 0xaf;
        public const byte SQLALTFMT = 0xa8;
        public const byte SQLALTMETADATA = 0x88;
        public const byte SQLALTNAME = 0xa7;
        public const byte SQLALTROW = 0xd3;
        public const int SQLBIGBINARY = 0xad;
        public const int SQLBIGCHAR = 0xaf;
        public const int SQLBIGVARBINARY = 0xa5;
        public const int SQLBIGVARCHAR = 0xa7;
        public const int SQLBINARY = 0x2d;
        public const int SQLBIT = 50;
        public const int SQLBITN = 0x68;
        public const int SQLCHAR = 0x2f;
        public const byte SQLCOLFMT = 0xa1;
        public const byte SQLCOLINFO = 0xa5;
        public const byte SQLCOLMETADATA = 0x81;
        public const byte SQLCOLNAME = 160;
        public const byte SQLCONTROL = 0xae;
        public const int SQLDATE = 40;
        public const int SQLDATETIM4 = 0x3a;
        public const int SQLDATETIME = 0x3d;
        public const int SQLDATETIME2 = 0x2a;
        public const int SQLDATETIMEOFFSET = 0x2b;
        public const int SQLDATETIMN = 0x6f;
        public const byte SQLDEBUG_CMD = 0x60;
        public const int SQLDEBUG_CONTEXT = 2;
        public static readonly string[] SQLDEBUG_MODE_NAMES = new string[] { "off", "on", "context" };
        public const int SQLDEBUG_OFF = 0;
        public const int SQLDEBUG_ON = 1;
        public const int SQLDECIMALN = 0x6a;
        public const byte SQLDifferentName = 0x20;
        public const byte SQLDONE = 0xfd;
        public const byte SQLDONEINPROC = 0xff;
        public const byte SQLDONEPROC = 0xfe;
        public const byte SQLENVCHANGE = 0xe3;
        public const byte SQLERROR = 170;
        public const byte SQLExpression = 4;
        public const byte SQLFixedLen = 0x30;
        public const int SQLFLT4 = 0x3b;
        public const int SQLFLT8 = 0x3e;
        public const int SQLFLTN = 0x6d;
        public const byte SQLHidden = 0x10;
        public const int SQLIMAGE = 0x22;
        public const byte SQLINFO = 0xab;
        public const int SQLINT1 = 0x30;
        public const int SQLINT2 = 0x34;
        public const int SQLINT4 = 0x38;
        public const int SQLINT8 = 0x7f;
        public const int SQLINTN = 0x26;
        public const byte SQLKey = 8;
        public const byte SQLLenMask = 0x30;
        public const byte SQLLOGINACK = 0xad;
        public const int SQLMONEY = 60;
        public const int SQLMONEY4 = 0x7a;
        public const int SQLMONEYN = 110;
        public const int SQLNCHAR = 0xef;
        public const int SQLNTEXT = 0x63;
        public const int SQLNUMERICN = 0x6c;
        public const int SQLNVARCHAR = 0xe7;
        public const byte SQLOFFSET = 120;
        public const byte SQLORDER = 0xa9;
        public const byte SQLPROCID = 0x7c;
        public const byte SQLRETURNSTATUS = 0x79;
        public const byte SQLRETURNTOK = 0xdb;
        public const byte SQLRETURNVALUE = 0xac;
        public const byte SQLROW = 0xd1;
        public const byte SQLROWCRC = 0x39;
        public const byte SQLSECLEVEL = 0xed;
        public const byte SQLSSPI = 0xed;
        public const int SQLTABLE = 0xf3;
        public const byte SQLTABNAME = 0xa4;
        public const int SQLTEXT = 0x23;
        public const int SQLTIME = 0x29;
        public const int SQLTIMESTAMP = 80;
        public const int SQLUDT = 240;
        public const int SQLUNIQUEID = 0x24;
        public const int SQLVARBINARY = 0x25;
        public const int SQLVARCHAR = 0x27;
        public const byte SQLVarCnt = 0;
        public const int SQLVARIANT = 0x62;
        public const byte SQLVARIANT_SIZE = 2;
        public const byte SQLVarLen = 0x20;
        public const int SQLVOID = 0x1f;
        public const int SQLXMLTYPE = 0xf1;
        public const byte SQLZeroLen = 0x10;
        public const int SSPI_ON = 1;
        public const byte ST_AACK = 2;
        public const byte ST_BATCH = 4;
        public const byte ST_EOM = 1;
        public const byte ST_IGNORE = 2;
        public const byte ST_RESET_CONNECTION = 8;
        public const byte ST_RESET_CONNECTION_PRESERVE_TRANSACTION = 0x10;
        public const int SUCCEED = 1;
        public const string TABLE = "Table";
        public const string TCP = "tcp";
        public const int TEXT_TIME_STAMP_LEN = 8;
        internal static readonly long[] TICKS_FROM_SCALE = new long[] { 0x989680L, 0xf4240L, 0x186a0L, 0x2710L, 0x3e8L, 100L, 10L, 1L };
        public const short TIMEOUT_EXPIRED = -2;
        public const bool TraceTDS = false;
        public const string TRANS_BEGIN = "BEGIN TRANSACTION";
        public const string TRANS_COMMIT = "COMMIT TRANSACTION";
        public const string TRANS_IF_ROLLBACK = "IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION";
        public const string TRANS_READ_COMMITTED = "SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
        public const string TRANS_READ_UNCOMMITTED = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
        public const string TRANS_REPEATABLE_READ = "SET TRANSACTION ISOLATION LEVEL REPEATABLE READ";
        public const string TRANS_ROLLBACK = "ROLLBACK TRANSACTION";
        public const string TRANS_SAVE = "SAVE TRANSACTION";
        public const string TRANS_SERIALIZABLE = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE";
        public const string TRANS_SNAPSHOT = "SET TRANSACTION ISOLATION LEVEL SNAPSHOT";
        public const int TVP_DEFAULT_COLUMN = 0x200;
        public const byte TVP_END_TOKEN = 0;
        public const ushort TVP_NOMETADATA_TOKEN = 0xffff;
        public const byte TVP_ORDER_UNIQUE_TOKEN = 0x10;
        public const byte TVP_ORDERASC_FLAG = 1;
        public const byte TVP_ORDERDESC_FLAG = 2;
        public const byte TVP_ROW_TOKEN = 1;
        public const byte TVP_ROWCOUNT_ESTIMATE = 0x12;
        public const byte TVP_UNIQUE_FLAG = 4;
        public const short TYPE_SIZE_LIMIT = 0x1f40;
        public const ulong UDTNULL = ulong.MaxValue;
        public const byte UNKNOWN_PRECISION_SCALE = 0xff;
        public const byte Updatability = 11;
        internal const ushort UPDATE = 0xc5;
        public const int USE_DB_ON = 1;
        public const uint VARLONGNULL = uint.MaxValue;
        public const int VARNULL = 0xffff;
        public const byte VERSION_SIZE = 4;
        public const string VIA = "via";
        internal const int WHIDBEY_DATE_LENGTH = 10;
        internal static readonly int[] WHIDBEY_DATETIME2_LENGTH = new int[] { 0x13, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b };
        internal static readonly int[] WHIDBEY_DATETIMEOFFSET_LENGTH = new int[] { 0x1a, 0x1c, 0x1d, 30, 0x1f, 0x20, 0x21, 0x22 };
        internal static readonly int[] WHIDBEY_TIME_LENGTH = new int[] { 8, 10, 11, 12, 13, 14, 15, 0x10 };
        public const int XMLUNICODEBOM = 0xfeff;
        public const int YUKON_HEADER_LEN = 12;
        public const int YUKON_INCREMENT = 9;
        public const int YUKON_LOG_REC_FIXED_LEN = 0x5e;
        public const int YUKON_MAJOR = 0x72;
        public const byte YUKON_RPCBATCHFLAG = 0xff;
        public const int YUKON_RTM_MINOR = 2;

        internal enum TransactionManagerIsolationLevel
        {
            Unspecified,
            ReadUncommitted,
            ReadCommitted,
            RepeatableRead,
            Serializable,
            Snapshot
        }

        internal enum TransactionManagerRequestType
        {
            Begin = 5,
            Commit = 7,
            GetDTCAddress = 0,
            Promote = 6,
            Propagate = 1,
            Rollback = 8,
            Save = 9
        }
    }
}

