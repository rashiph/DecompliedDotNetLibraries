namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;

    internal static class ODBC
    {
        internal const string Pwd = "pwd";

        internal static Exception CantAllocateEnvironmentHandle(ODBC32.RetCode retcode)
        {
            return ADP.DataAdapter(Res.GetString("Odbc_CantAllocateEnvironmentHandle", new object[] { ODBC32.RetcodeToString(retcode) }));
        }

        internal static Exception CantEnableConnectionpooling(ODBC32.RetCode retcode)
        {
            return ADP.DataAdapter(Res.GetString("Odbc_CantEnableConnectionpooling", new object[] { ODBC32.RetcodeToString(retcode) }));
        }

        internal static Exception CantSetPropertyOnOpenConnection()
        {
            return ADP.InvalidOperation(Res.GetString("Odbc_CantSetPropertyOnOpenConnection"));
        }

        internal static Exception ConnectionStringTooLong()
        {
            return ADP.Argument(Res.GetString("OdbcConnection_ConnectionStringTooLong", new object[] { 0x400 }));
        }

        internal static Exception FailedToGetDescriptorHandle(ODBC32.RetCode retcode)
        {
            return ADP.DataAdapter(Res.GetString("Odbc_FailedToGetDescriptorHandle", new object[] { ODBC32.RetcodeToString(retcode) }));
        }

        internal static ArgumentException GetSchemaRestrictionRequired()
        {
            return ADP.Argument(Res.GetString("ODBC_GetSchemaRestrictionRequired"));
        }

        internal static Exception NegativeArgument()
        {
            return ADP.Argument(Res.GetString("Odbc_NegativeArgument"));
        }

        internal static InvalidOperationException NoMappingForSqlTransactionLevel(int value)
        {
            return ADP.DataAdapter(Res.GetString("Odbc_NoMappingForSqlTransactionLevel", new object[] { value.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception NotInTransaction()
        {
            return ADP.InvalidOperation(Res.GetString("Odbc_NotInTransaction"));
        }

        internal static ArgumentOutOfRangeException NotSupportedCommandType(CommandType value)
        {
            return NotSupportedEnumerationValue(typeof(CommandType), (int) value);
        }

        internal static ArgumentOutOfRangeException NotSupportedEnumerationValue(Type type, int value)
        {
            return ADP.ArgumentOutOfRange(Res.GetString("ODBC_NotSupportedEnumerationValue", new object[] { type.Name, value.ToString(CultureInfo.InvariantCulture) }), type.Name);
        }

        internal static ArgumentOutOfRangeException NotSupportedIsolationLevel(IsolationLevel value)
        {
            return NotSupportedEnumerationValue(typeof(IsolationLevel), (int) value);
        }

        internal static short ShortStringLength(string inputString)
        {
            return (short) ADP.StringLength(inputString);
        }

        internal static void TraceODBC(int level, string method, ODBC32.RetCode retcode)
        {
            Bid.TraceSqlReturn("<odbc|API|ODBC|RET> %08X{SQLRETURN}, method=%ls\n", retcode, method);
        }

        internal static Exception UnknownOdbcType(OdbcType odbctype)
        {
            return ADP.InvalidEnumerationValue(typeof(OdbcType), (int) odbctype);
        }

        internal static Exception UnknownSQLType(ODBC32.SQL_TYPE sqltype)
        {
            return ADP.Argument(Res.GetString("Odbc_UnknownSQLType", new object[] { sqltype.ToString() }));
        }
    }
}

