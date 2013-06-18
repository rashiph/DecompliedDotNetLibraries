namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class OCI
    {
        private static int _clientVersion;

        private OCI()
        {
        }

        internal static int DetermineClientVersion()
        {
            if (_clientVersion == 0)
            {
                int num = 0;
                MODE environmentMode = MODE.OCI_DATA_AT_EXEC | MODE.OCI_BATCH_MODE;
                try
                {
                    System.Data.Common.UnsafeNativeMethods.OCILobCopy2(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0L, 0L, 0L);
                    num = 0x65;
                }
                catch (EntryPointNotFoundException exception6)
                {
                    System.Data.Common.ADP.TraceException(exception6);
                    try
                    {
                        OciHandle handle = new OciNlsEnvironmentHandle(environmentMode);
                        if (!handle.IsInvalid)
                        {
                            num = 0x5c;
                            OciHandle.SafeDispose(ref handle);
                        }
                    }
                    catch (EntryPointNotFoundException exception5)
                    {
                        System.Data.Common.ADP.TraceException(exception5);
                        try
                        {
                            environmentMode |= MODE.OCI_UTF16;
                            OciHandle handle2 = new OciEnvironmentHandle(environmentMode, true);
                            num = 90;
                            OciHandle.SafeDispose(ref handle2);
                        }
                        catch (EntryPointNotFoundException exception4)
                        {
                            System.Data.Common.ADP.TraceException(exception4);
                            num = 80;
                        }
                        catch (Exception exception)
                        {
                            if (!System.Data.Common.ADP.IsCatchableExceptionType(exception))
                            {
                                throw;
                            }
                            System.Data.Common.ADP.TraceException(exception);
                            num = 0x51;
                        }
                    }
                }
                catch (DllNotFoundException exception3)
                {
                    System.Data.Common.ADP.TraceException(exception3);
                    num = 0x49;
                }
                catch (BadImageFormatException exception2)
                {
                    throw System.Data.Common.ADP.BadOracleClientImageFormat(exception2);
                }
                if (0x51 > num)
                {
                    throw System.Data.Common.ADP.BadOracleClientVersion();
                }
                _clientVersion = num;
            }
            return _clientVersion;
        }

        internal static bool ClientVersionAtLeastOracle9i
        {
            get
            {
                return (90 <= _clientVersion);
            }
        }

        internal enum ATTR
        {
            OCI_ATTR_AGENT_ADDRESS = 0x41,
            OCI_ATTR_AGENT_NAME = 0x40,
            OCI_ATTR_AGENT_PROTOCOL = 0x42,
            OCI_ATTR_ALLOC_DURATION = 0x25,
            OCI_ATTR_ATTEMPTS = 0x3b,
            OCI_ATTR_BUF_ADDR = 0x4c,
            OCI_ATTR_BUF_SIZE = 0x4d,
            OCI_ATTR_CACHE = 0x73,
            OCI_ATTR_CACHE_MAX_SIZE = 0x23,
            OCI_ATTR_CACHE_OPT_SIZE = 0x22,
            OCI_ATTR_CHAR_COUNT = 15,
            OCI_ATTR_CHAR_SIZE = 0x11e,
            OCI_ATTR_CHARSET = 20,
            OCI_ATTR_CHARSET_FORM = 0x20,
            OCI_ATTR_CHARSET_ID = 0x1f,
            OCI_ATTR_CLUSTERED = 0x69,
            OCI_ATTR_COL_COUNT = 0x52,
            OCI_ATTR_CONSUMER_NAME = 50,
            OCI_ATTR_CORRELATION = 0x3a,
            OCI_ATTR_DATA_SIZE = 1,
            OCI_ATTR_DATA_TYPE = 2,
            OCI_ATTR_DATEFORMAT = 0x4b,
            OCI_ATTR_DELAY = 0x38,
            OCI_ATTR_DEQ_MODE = 0x33,
            OCI_ATTR_DEQ_MSGID = 0x36,
            OCI_ATTR_DIRPATH_FILE = 0x8b,
            OCI_ATTR_DIRPATH_INDEX_MAINT_METHOD = 0x8a,
            OCI_ATTR_DIRPATH_MODE = 0x4e,
            OCI_ATTR_DIRPATH_NOLOG = 0x4f,
            OCI_ATTR_DIRPATH_PARALLEL = 80,
            OCI_ATTR_DIRPATH_SORTED_INDEX = 0x89,
            OCI_ATTR_DIRPATH_STORAGE_INITIAL = 140,
            OCI_ATTR_DIRPATH_STORAGE_NEXT = 0x8d,
            OCI_ATTR_DISP_SIZE = 3,
            OCI_ATTR_DML_ROW_OFFSET = 0x4a,
            OCI_ATTR_DURATION = 0x84,
            OCI_ATTR_ENQ_TIME = 0x3e,
            OCI_ATTR_ENV = 5,
            OCI_ATTR_ENV_CHARSET_ID = 0xcf,
            OCI_ATTR_ENV_NCHARSET_ID = 0xd0,
            OCI_ATTR_ENV_UTF16 = 0xd1,
            OCI_ATTR_EXCEPTION_QUEUE = 0x3d,
            OCI_ATTR_EXPIRATION = 0x39,
            OCI_ATTR_EXTERNAL_NAME = 0x1a,
            OCI_ATTR_FDO = 0x27,
            OCI_ATTR_FNCODE = 1,
            OCI_ATTR_FOCBK = 0x2b,
            OCI_ATTR_FSPRECISION = 0x10,
            OCI_ATTR_HEAPALLOC = 30,
            OCI_ATTR_HW_MARK = 0x75,
            OCI_ATTR_IN_V8_MODE = 0x2c,
            OCI_ATTR_INCR = 0x72,
            OCI_ATTR_INDEX_ONLY = 0x6b,
            OCI_ATTR_INITIAL_CLIENT_ROLES = 100,
            OCI_ATTR_INTERNAL_NAME = 0x19,
            OCI_ATTR_IS_INVOKER_RIGHTS = 0x85,
            OCI_ATTR_IS_NULL = 7,
            OCI_ATTR_IS_TEMPORARY = 130,
            OCI_ATTR_IS_TYPED = 0x83,
            OCI_ATTR_LFPRECISION = 0x11,
            OCI_ATTR_LINK = 0x6f,
            OCI_ATTR_LIST_ARGUMENTS = 0x6c,
            OCI_ATTR_LIST_COLUMNS = 0x67,
            OCI_ATTR_LIST_SUBPROGRAMS = 0x6d,
            OCI_ATTR_LOBEMPTY = 0x2d,
            OCI_ATTR_LTYPE = 0x80,
            OCI_ATTR_MAX = 0x71,
            OCI_ATTR_MAXCHAR_SIZE = 0xa3,
            OCI_ATTR_MAXDATA_SIZE = 0x21,
            OCI_ATTR_MEMPOOL_APPNAME = 90,
            OCI_ATTR_MEMPOOL_HOMENAME = 0x5b,
            OCI_ATTR_MEMPOOL_INSTNAME = 0x59,
            OCI_ATTR_MEMPOOL_MODEL = 0x5c,
            OCI_ATTR_MEMPOOL_SIZE = 0x58,
            OCI_ATTR_MIGSESSION = 0x56,
            OCI_ATTR_MIN = 0x70,
            OCI_ATTR_MODES = 0x5d,
            OCI_ATTR_MSG_PROP = 0x48,
            OCI_ATTR_MSG_STATE = 0x3f,
            OCI_ATTR_NAME = 4,
            OCI_ATTR_NAVIGATION = 0x34,
            OCI_ATTR_NCHAR = 0x15,
            OCI_ATTR_NESTED_PREFETCH_MEMORY = 14,
            OCI_ATTR_NESTED_PREFETCH_ROWS = 12,
            OCI_ATTR_NFY_MSGID = 0x47,
            OCI_ATTR_NO_CACHE = 0x91,
            OCI_ATTR_NOCACHE = 0x57,
            OCI_ATTR_NONBLOCKING_MODE = 3,
            OCI_ATTR_NUM_ATTRS = 120,
            OCI_ATTR_NUM_COLS = 0x66,
            OCI_ATTR_NUM_DML_ERRORS = 0x49,
            OCI_ATTR_NUM_PARAMS = 0x79,
            OCI_ATTR_NUM_ROWS = 0x51,
            OCI_ATTR_OBJ_ID = 0x88,
            OCI_ATTR_OBJ_NAME = 0x86,
            OCI_ATTR_OBJ_SCHEMA = 0x87,
            OCI_ATTR_OBJECT = 2,
            OCI_ATTR_OBJID = 0x7a,
            OCI_ATTR_ORDER = 0x74,
            OCI_ATTR_ORIGINAL_MSGID = 0x45,
            OCI_ATTR_OVERLOAD_ID = 0x7d,
            OCI_ATTR_PARAM = 0x7c,
            OCI_ATTR_PARAM_COUNT = 0x12,
            OCI_ATTR_PARSE_ERROR_OFFSET = 0x81,
            OCI_ATTR_PARTITIONED = 0x6a,
            OCI_ATTR_PASSWORD = 0x17,
            OCI_ATTR_PDPRC = 0x11,
            OCI_ATTR_PDSCL = 0x10,
            OCI_ATTR_PIN_DURATION = 0x26,
            OCI_ATTR_PINOPTION = 0x24,
            OCI_ATTR_POSTPROCESSING_CALLBACK = 40,
            OCI_ATTR_POSTPROCESSING_CONTEXT = 0x29,
            OCI_ATTR_PRECISION = 5,
            OCI_ATTR_PREFETCH_MEMORY = 13,
            OCI_ATTR_PREFETCH_ROWS = 11,
            OCI_ATTR_PRIORITY = 0x37,
            OCI_ATTR_PROXY_CREDENTIALS = 0x63,
            OCI_ATTR_PTYPE = 0x7b,
            OCI_ATTR_QUEUE_NAME = 70,
            OCI_ATTR_RDBA = 0x68,
            OCI_ATTR_RECIPIENT_LIST = 60,
            OCI_ATTR_REF_TDO = 110,
            OCI_ATTR_RELATIVE_MSGID = 0x30,
            OCI_ATTR_RESERVED_1 = 0x92,
            OCI_ATTR_ROW_COUNT = 9,
            OCI_ATTR_ROWID = 0x13,
            OCI_ATTR_ROWS_RETURNED = 0x2a,
            OCI_ATTR_SCALE = 6,
            OCI_ATTR_SENDER_ID = 0x44,
            OCI_ATTR_SEQUENCE_DEVIATION = 0x31,
            OCI_ATTR_SERVER = 6,
            OCI_ATTR_SERVER_BUSY = 0x93,
            OCI_ATTR_SERVER_GROUP = 0x55,
            OCI_ATTR_SERVER_STATUS = 0x8f,
            OCI_ATTR_SESSION = 7,
            OCI_ATTR_SESSLANG = 0x2e,
            OCI_ATTR_SHARED_HEAPALLOC = 0x54,
            OCI_ATTR_SQLCODE = 4,
            OCI_ATTR_SQLFNCODE = 10,
            OCI_ATTR_STATEMENT = 0x90,
            OCI_ATTR_STMT_TYPE = 0x18,
            OCI_ATTR_STREAM_OFFSET = 0x53,
            OCI_ATTR_SUBSCR_CALLBACK = 0x5f,
            OCI_ATTR_SUBSCR_CTX = 0x60,
            OCI_ATTR_SUBSCR_NAME = 0x5e,
            OCI_ATTR_SUBSCR_NAMESPACE = 0x62,
            OCI_ATTR_SUBSCR_PAYLOAD = 0x61,
            OCI_ATTR_TABLESPACE = 0x7e,
            OCI_ATTR_TDO = 0x7f,
            OCI_ATTR_TIMESTAMP = 0x77,
            OCI_ATTR_TRANS = 8,
            OCI_ATTR_TRANS_LOCK = 0x1c,
            OCI_ATTR_TRANS_NAME = 0x1d,
            OCI_ATTR_TRANS_TIMEOUT = 0x8e,
            OCI_ATTR_TYPE_SCHEMA = 0x76,
            OCI_ATTR_UNK = 0x65,
            OCI_ATTR_USERNAME = 0x16,
            OCI_ATTR_VISIBILITY = 0x2f,
            OCI_ATTR_WAIT = 0x35,
            OCI_ATTR_XID = 0x1b
        }

        internal static class Callback
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int OCICallbackDefine(IntPtr octxp, IntPtr defnp, uint iter, IntPtr bufpp, IntPtr alenp, IntPtr piecep, IntPtr indp, IntPtr rcodep);
        }

        internal enum CHARSETFORM : byte
        {
            SQLCS_EXPLICIT = 3,
            SQLCS_FLEXIBLE = 4,
            SQLCS_IMPLICIT = 1,
            SQLCS_LIT_NULL = 5,
            SQLCS_NCHAR = 2
        }

        internal enum CRED
        {
            OCI_CRED_EXT = 2,
            OCI_CRED_PROXY = 3,
            OCI_CRED_RDBMS = 1
        }

        internal enum DATATYPE : short
        {
            ANSIDATE = 0xb8,
            BFILE = 0x72,
            BLOB = 0x71,
            CHAR = 0x60,
            CHARZ = 0x61,
            CLOB = 0x70,
            CURSOR = 0x66,
            DATE = 12,
            FLOAT = 4,
            INT_INTERVAL_DS = 0xb7,
            INT_INTERVAL_YM = 0xb6,
            INT_TIMESTAMP = 180,
            INT_TIMESTAMP_LTZ = 0xe7,
            INT_TIMESTAMP_TZ = 0xb5,
            INTEGER = 3,
            INTERVAL_DS = 190,
            INTERVAL_YM = 0xbd,
            LONG = 8,
            LONGRAW = 0x18,
            LONGVARCHAR = 0x5e,
            LONGVARRAW = 0x5f,
            MLSLABEL = 0x69,
            NUMBER = 2,
            OCIDATE = 0x9c,
            PLSQLRECORD = 250,
            PLSQLTABLE = 0xfb,
            RAW = 0x17,
            REF = 110,
            ROWID = 11,
            ROWID_DESC = 0x68,
            RSET = 0x74,
            STRING = 5,
            TIME = 0xb9,
            TIME_TZ = 0xba,
            TIMESTAMP = 0xbb,
            TIMESTAMP_LTZ = 0xe8,
            TIMESTAMP_TZ = 0xbc,
            UNSIGNEDINT = 0x44,
            UROWID = 0xd0,
            USERDEFINED = 0x6c,
            VARCHAR2 = 1,
            VARNUM = 6,
            VARRAW = 15
        }

        internal enum DURATION : short
        {
            OCI_DURATION_BEGIN = 10,
            OCI_DURATION_CALL = 12,
            OCI_DURATION_CALLOUT = 14,
            OCI_DURATION_DEFAULT = 8,
            OCI_DURATION_LAST = 14,
            OCI_DURATION_NEXT = 7,
            OCI_DURATION_NULL = 9,
            OCI_DURATION_SESSION = 10,
            OCI_DURATION_STATEMENT = 13,
            OCI_DURATION_TRANS = 11
        }

        internal enum FETCH : short
        {
            OCI_FETCH_ABSOLUTE = 0x20,
            OCI_FETCH_FIRST = 4,
            OCI_FETCH_LAST = 8,
            OCI_FETCH_NEXT = 2,
            OCI_FETCH_PRIOR = 0x10,
            OCI_FETCH_RELATIVE = 0x40
        }

        internal enum HTYPE
        {
            OCI_DTYPE_AQAGENT = 60,
            OCI_DTYPE_AQDEQ_OPTIONS = 0x3a,
            OCI_DTYPE_AQENQ_OPTIONS = 0x39,
            OCI_DTYPE_AQMSG_PROPERTIES = 0x3b,
            OCI_DTYPE_AQNFY_DESCRIPTOR = 0x40,
            OCI_DTYPE_COMPLEXOBJECTCOMP = 0x37,
            OCI_DTYPE_DATE = 0x41,
            OCI_DTYPE_FILE = 0x38,
            OCI_DTYPE_FIRST = 50,
            OCI_DTYPE_INTERVAL_DS = 0x3f,
            OCI_DTYPE_INTERVAL_YM = 0x3e,
            OCI_DTYPE_LAST = 0x47,
            OCI_DTYPE_LOB = 50,
            OCI_DTYPE_LOCATOR = 0x3d,
            OCI_DTYPE_PARAM = 0x35,
            OCI_DTYPE_ROWID = 0x36,
            OCI_DTYPE_RSET = 0x34,
            OCI_DTYPE_SNAP = 0x33,
            OCI_DTYPE_TIME = 0x42,
            OCI_DTYPE_TIME_TZ = 0x43,
            OCI_DTYPE_TIMESTAMP = 0x44,
            OCI_DTYPE_TIMESTAMP_LTZ = 70,
            OCI_DTYPE_TIMESTAMP_TZ = 0x45,
            OCI_DTYPE_UCB = 0x47,
            OCI_HTYPE_BIND = 5,
            OCI_HTYPE_COMPLEXOBJECT = 11,
            OCI_HTYPE_DEFINE = 6,
            OCI_HTYPE_DESCRIBE = 7,
            OCI_HTYPE_DIRPATH_COLUMN_ARRAY = 15,
            OCI_HTYPE_DIRPATH_CTX = 14,
            OCI_HTYPE_DIRPATH_STREAM = 0x10,
            OCI_HTYPE_ENV = 1,
            OCI_HTYPE_ERROR = 2,
            OCI_HTYPE_PROC = 0x11,
            OCI_HTYPE_SECURITY = 12,
            OCI_HTYPE_SERVER = 8,
            OCI_HTYPE_SESSION = 9,
            OCI_HTYPE_STMT = 4,
            OCI_HTYPE_SUBSCRIPTION = 13,
            OCI_HTYPE_SVCCTX = 3,
            OCI_HTYPE_TRANS = 10
        }

        internal enum INDICATOR
        {
            ISNULL = -1,
            OK = 0,
            TOOBIG = -2
        }

        internal enum LOB_TYPE : byte
        {
            OCI_TEMP_BLOB = 1,
            OCI_TEMP_CLOB = 2
        }

        [Flags]
        internal enum MODE
        {
            OCI_BATCH_ERRORS = 0x80,
            OCI_BATCH_MODE = 1,
            OCI_CACHE = 0x200,
            OCI_COMMIT_ON_SUCCESS = 0x20,
            OCI_DATA_AT_EXEC = 2,
            OCI_DEFAULT = 0,
            OCI_DESCRIBE_ONLY = 0x10,
            OCI_DYNAMIC_FETCH = 2,
            OCI_EVENTS = 4,
            OCI_EXACT_FETCH = 2,
            OCI_KEEP_FETCH_STATE = 4,
            OCI_MIGRATE = 1,
            OCI_NO_CACHE = 0x400,
            OCI_NO_MUTEX = 0x80,
            OCI_NO_UCB = 0x40,
            OCI_NON_BLOCKING = 0x40,
            OCI_OBJECT = 2,
            OCI_PARSE_ONLY = 0x100,
            OCI_PIECEWISE = 4,
            OCI_PRELIM_AUTH = 8,
            OCI_SB2_IND_PTR = 1,
            OCI_SCROLLABLE_CURSOR = 8,
            OCI_SHARED = 0x10,
            OCI_SHARED_EXT = 0x100,
            OCI_SHOW_DML_WARNINGS = 0x400,
            OCI_SYSDBA = 2,
            OCI_SYSOPER = 4,
            OCI_THREADED = 1,
            OCI_UTF16 = 0x4000,
            OCIP_ICACHE = 0x10
        }

        internal enum PATTR
        {
            OCI_ATTR_DATA_SIZE = 1,
            OCI_ATTR_DATA_TYPE = 2,
            OCI_ATTR_DISP_SIZE = 3,
            OCI_ATTR_IS_NULL = 7,
            OCI_ATTR_NAME = 4,
            OCI_ATTR_PRECISION = 5,
            OCI_ATTR_SCALE = 6
        }

        internal enum RETURNCODE
        {
            OCI_CONTINUE = -24200,
            OCI_ERROR = -1,
            OCI_INVALID_HANDLE = -2,
            OCI_NEED_DATA = 0x63,
            OCI_NO_DATA = 100,
            OCI_RESERVED_FOR_INT_USE = 200,
            OCI_STILL_EXECUTING = -3123,
            OCI_SUCCESS = 0,
            OCI_SUCCESS_WITH_INFO = 1
        }

        internal enum SIGN
        {
            OCI_NUMBER_SIGNED = 2,
            OCI_NUMBER_UNSIGNED = 0
        }

        internal enum STMT
        {
            OCI_STMT_ALTER = 7,
            OCI_STMT_BEGIN = 8,
            OCI_STMT_CREATE = 5,
            OCI_STMT_DECLARE = 9,
            OCI_STMT_DELETE = 3,
            OCI_STMT_DROP = 6,
            OCI_STMT_INSERT = 4,
            OCI_STMT_SELECT = 1,
            OCI_STMT_UPDATE = 2
        }

        internal enum SYNTAX
        {
            OCI_NTV_SYNTAX = 1,
            OCI_V7_SYNTAX = 2,
            OCI_V8_SYNTAX = 3
        }
    }
}

