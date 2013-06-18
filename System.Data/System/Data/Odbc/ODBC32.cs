namespace System.Data.Odbc
{
    using System;
    using System.Text;

    internal static class ODBC32
    {
        internal const int COLUMN_NAME = 4;
        internal const int COLUMN_SIZE = 8;
        internal const int COLUMN_TYPE = 5;
        internal const int DATA_TYPE = 6;
        internal const int DECIMAL_DIGITS = 10;
        internal const int MAX_CONNECTION_STRING_LENGTH = 0x400;
        internal const int NUM_PREC_RADIX = 11;
        private const int SIGNED_OFFSET = -20;
        internal const short SQL_ALL_TYPES = 0;
        internal static readonly IntPtr SQL_AUTOCOMMIT_OFF = ADP.PtrZero;
        internal static readonly IntPtr SQL_AUTOCOMMIT_ON = new IntPtr(1);
        internal const int SQL_CD_FALSE = 0;
        internal const int SQL_CD_TRUE = 1;
        internal const short SQL_COMMIT = 0;
        internal static readonly IntPtr SQL_CP_OFF = new IntPtr(0);
        internal static readonly IntPtr SQL_CP_ONE_PER_DRIVER = new IntPtr(1);
        internal static readonly IntPtr SQL_CP_ONE_PER_HENV = new IntPtr(2);
        internal const int SQL_DEFAULT_PARAM = -5;
        internal const short SQL_DIAG_SQLSTATE = 4;
        internal const int SQL_DTC_DONE = 0;
        internal static readonly IntPtr SQL_HANDLE_NULL = ADP.PtrZero;
        internal const int SQL_IS_POINTER = -4;
        internal const int SQL_IS_PTR = 1;
        internal const int SQL_NO_TOTAL = -4;
        internal const int SQL_NTS = -3;
        internal const int SQL_NULL_DATA = -1;
        internal static readonly IntPtr SQL_OV_ODBC3 = new IntPtr(3);
        internal const short SQL_RESULT_COL = 3;
        internal const short SQL_ROLLBACK = 1;
        private const int UNSIGNED_OFFSET = -22;

        internal static OdbcErrorCollection GetDiagErrors(string source, OdbcHandle hrHandle, RetCode retcode)
        {
            OdbcErrorCollection errors = new OdbcErrorCollection();
            GetDiagErrors(errors, source, hrHandle, retcode);
            return errors;
        }

        internal static void GetDiagErrors(OdbcErrorCollection errors, string source, OdbcHandle hrHandle, RetCode retcode)
        {
            if (retcode != RetCode.SUCCESS)
            {
                short record = 0;
                short cchActual = 0;
                StringBuilder message = new StringBuilder(0x400);
                bool flag = true;
                while (flag)
                {
                    int num3;
                    string str;
                    record = (short) (record + 1);
                    retcode = hrHandle.GetDiagnosticRecord(record, out str, message, out num3, out cchActual);
                    if ((RetCode.SUCCESS_WITH_INFO == retcode) && ((message.Capacity - 1) < cchActual))
                    {
                        message.Capacity = cchActual + 1;
                        retcode = hrHandle.GetDiagnosticRecord(record, out str, message, out num3, out cchActual);
                    }
                    if ((retcode == RetCode.SUCCESS) || (retcode == RetCode.SUCCESS_WITH_INFO))
                    {
                        errors.Add(new OdbcError(source, message.ToString(), str, num3));
                    }
                }
            }
        }

        internal static string RetcodeToString(RetCode retcode)
        {
            switch (retcode)
            {
                case RetCode.INVALID_HANDLE:
                    return "INVALID_HANDLE";

                case RetCode.SUCCESS:
                    return "SUCCESS";

                case RetCode.SUCCESS_WITH_INFO:
                    return "SUCCESS_WITH_INFO";

                case RetCode.NO_DATA:
                    return "NO_DATA";
            }
            return "ERROR";
        }

        internal enum HANDLER
        {
            IGNORE,
            THROW
        }

        internal enum RetCode : short
        {
            ERROR = -1,
            INVALID_HANDLE = -2,
            NO_DATA = 100,
            SUCCESS = 0,
            SUCCESS_WITH_INFO = 1
        }

        [Serializable]
        public enum RETCODE
        {
            ERROR = -1,
            INVALID_HANDLE = -2,
            NO_DATA = 100,
            SUCCESS = 0,
            SUCCESS_WITH_INFO = 1
        }

        internal enum SQL_API : ushort
        {
            SQLCOLUMNS = 40,
            SQLEXECDIRECT = 11,
            SQLGETTYPEINFO = 0x2f,
            SQLPROCEDURECOLUMNS = 0x42,
            SQLPROCEDURES = 0x43,
            SQLSTATISTICS = 0x35,
            SQLTABLES = 0x36
        }

        internal enum SQL_ATTR
        {
            APP_PARAM_DESC = 0x271b,
            APP_ROW_DESC = 0x271a,
            AUTOCOMMIT = 0x66,
            CONNECTION_DEAD = 0x4b9,
            CONNECTION_POOLING = 0xc9,
            CURRENT_CATALOG = 0x6d,
            IMP_PARAM_DESC = 0x271d,
            IMP_ROW_DESC = 0x271c,
            LOGIN_TIMEOUT = 0x67,
            METADATA_ID = 0x271e,
            ODBC_VERSION = 200,
            QUERY_TIMEOUT = 0,
            SQL_COPT_SS_BASE = 0x4b0,
            SQL_COPT_SS_ENLIST_IN_DTC = 0x4b7,
            SQL_COPT_SS_TXN_ISOLATION = 0x4cb,
            TXN_ISOLATION = 0x6c
        }

        internal enum SQL_C : short
        {
            ARD_TYPE = -99,
            BINARY = -2,
            BIT = -7,
            CHAR = 1,
            DEFAULT = 0x63,
            DOUBLE = 8,
            GUID = -11,
            NUMERIC = 2,
            REAL = 7,
            SBIGINT = -25,
            SLONG = -16,
            SSHORT = -15,
            TIMESTAMP = 11,
            TYPE_DATE = 0x5b,
            TYPE_TIME = 0x5c,
            TYPE_TIMESTAMP = 0x5d,
            UBIGINT = -27,
            UTINYINT = -28,
            WCHAR = -8
        }

        internal enum SQL_COLUMN
        {
            COUNT,
            NAME,
            TYPE,
            LENGTH,
            PRECISION,
            SCALE,
            DISPLAY_SIZE,
            NULLABLE,
            UNSIGNED,
            MONEY,
            UPDATABLE,
            AUTO_INCREMENT,
            CASE_SENSITIVE,
            SEARCHABLE,
            TYPE_NAME,
            TABLE_NAME,
            OWNER_NAME,
            QUALIFIER_NAME,
            LABEL
        }

        internal enum SQL_CONVERT : ushort
        {
            BIGINT = 0x35,
            BINARY = 0x36,
            BIT = 0x37,
            CHAR = 0x38,
            DATE = 0x39,
            DECIMAL = 0x3a,
            DOUBLE = 0x3b,
            FLOAT = 60,
            INTEGER = 0x3d,
            LONGVARBINARY = 0x47,
            LONGVARCHAR = 0x3e,
            NUMERIC = 0x3f,
            REAL = 0x40,
            SMALLINT = 0x41,
            TIME = 0x42,
            TIMESTAMP = 0x43,
            TINYINT = 0x44,
            VARBINARY = 0x45,
            VARCHAR = 70
        }

        [Flags]
        internal enum SQL_CVT
        {
            BIGINT = 0x4000,
            BINARY = 0x400,
            BIT = 0x1000,
            CHAR = 1,
            DATE = 0x8000,
            DECIMAL = 4,
            DOUBLE = 0x80,
            FLOAT = 0x20,
            GUID = 0x1000000,
            INTEGER = 8,
            INTERVAL_DAY_TIME = 0x100000,
            INTERVAL_YEAR_MONTH = 0x80000,
            LONGVARBINARY = 0x40000,
            LONGVARCHAR = 0x200,
            NUMERIC = 2,
            REAL = 0x40,
            SMALLINT = 0x10,
            TIME = 0x10000,
            TIMESTAMP = 0x20000,
            TINYINT = 0x2000,
            VARBINARY = 0x800,
            VARCHAR = 0x100,
            WCHAR = 0x200000,
            WLONGVARCHAR = 0x400000,
            WVARCHAR = 0x800000
        }

        internal enum SQL_DESC : short
        {
            ALLOC_TYPE = 0x44b,
            AUTO_UNIQUE_VALUE = 11,
            BASE_COLUMN_NAME = 0x16,
            BASE_TABLE_NAME = 0x17,
            CATALOG_NAME = 0x11,
            CONCISE_TYPE = 2,
            COUNT = 0x3e9,
            DATA_PTR = 0x3f2,
            DATETIME_INTERVAL_CODE = 0x3ef,
            DISPLAY_SIZE = 6,
            INDICATOR_PTR = 0x3f1,
            LENGTH = 0x3eb,
            NAME = 0x3f3,
            NULLABLE = 0x3f0,
            OCTET_LENGTH = 0x3f5,
            OCTET_LENGTH_PTR = 0x3ec,
            PRECISION = 0x3ed,
            SCALE = 0x3ee,
            SCHEMA_NAME = 0x10,
            TABLE_NAME = 15,
            TYPE = 0x3ea,
            TYPE_NAME = 14,
            UNNAMED = 0x3f4,
            UNSIGNED = 8,
            UPDATABLE = 10
        }

        internal enum SQL_HANDLE : short
        {
            DBC = 2,
            DESC = 4,
            ENV = 1,
            STMT = 3
        }

        internal enum SQL_INFO : ushort
        {
            CATALOG_NAME_SEPARATOR = 0x29,
            DATA_SOURCE_NAME = 2,
            DBMS_NAME = 0x11,
            DBMS_VER = 0x12,
            DRIVER_NAME = 6,
            DRIVER_ODBC_VER = 0x4d,
            DRIVER_VER = 7,
            GROUP_BY = 0x58,
            IDENTIFIER_CASE = 0x1c,
            IDENTIFIER_QUOTE_CHAR = 0x1d,
            KEYWORDS = 0x59,
            ODBC_VER = 10,
            ORDER_BY_COLUMNS_IN_SELECT = 90,
            QUOTED_IDENTIFIER_CASE = 0x5d,
            SEARCH_PATTERN_ESCAPE = 14,
            SERVER_NAME = 13,
            SQL_OJ_CAPABILITIES_20 = 0xfdeb,
            SQL_OJ_CAPABILITIES_30 = 0x73,
            SQL_SQL92_RELATIONAL_JOIN_OPERATORS = 0xa1
        }

        internal enum SQL_IS
        {
            INTEGER = -6,
            POINTER = -4,
            SMALLINT = -8,
            UINTEGER = -5
        }

        internal enum SQL_NULLABILITY : ushort
        {
            NO_NULLS = 0,
            NULLABLE = 1,
            UNKNOWN = 2
        }

        internal enum SQL_PARAM
        {
            INPUT = 1,
            INPUT_OUTPUT = 2,
            OUTPUT = 4,
            RETURN_VALUE = 5
        }

        internal enum SQL_SCOPE : ushort
        {
            CURROW = 0,
            SESSION = 2,
            TRANSACTION = 1
        }

        internal enum SQL_SPECIALCOLS : ushort
        {
            BEST_ROWID = 1,
            ROWVER = 2
        }

        internal enum SQL_TRANSACTION
        {
            READ_COMMITTED = 2,
            READ_UNCOMMITTED = 1,
            REPEATABLE_READ = 4,
            SERIALIZABLE = 8,
            SNAPSHOT = 0x20
        }

        internal enum SQL_TYPE : short
        {
            BIGINT = -5,
            BINARY = -2,
            BIT = -7,
            CHAR = 1,
            DECIMAL = 3,
            DOUBLE = 8,
            FLOAT = 6,
            GUID = -11,
            INTEGER = 4,
            LONGVARBINARY = -4,
            LONGVARCHAR = -1,
            NUMERIC = 2,
            REAL = 7,
            SMALLINT = 5,
            SS_TIME_EX = -154,
            SS_UDT = -151,
            SS_UTCDATETIME = -153,
            SS_VARIANT = -150,
            SS_XML = -152,
            TIMESTAMP = 11,
            TINYINT = -6,
            TYPE_DATE = 0x5b,
            TYPE_TIME = 0x5c,
            TYPE_TIMESTAMP = 0x5d,
            VARBINARY = -3,
            VARCHAR = 12,
            WCHAR = -8,
            WLONGVARCHAR = -10,
            WVARCHAR = -9
        }

        internal enum STMT : short
        {
            CLOSE = 0,
            DROP = 1,
            RESET_PARAMS = 3,
            UNBIND = 2
        }
    }
}

