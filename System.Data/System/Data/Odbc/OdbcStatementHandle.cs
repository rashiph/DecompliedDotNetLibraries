namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;

    internal sealed class OdbcStatementHandle : OdbcHandle
    {
        internal OdbcStatementHandle(OdbcConnectionHandle connectionHandle) : base(ODBC32.SQL_HANDLE.STMT, connectionHandle)
        {
        }

        internal ODBC32.RetCode BindColumn2(int columnNumber, ODBC32.SQL_C targetType, HandleRef buffer, IntPtr length, IntPtr srLen_or_Ind)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLBindCol(this, (ushort) columnNumber, targetType, buffer, length, srLen_or_Ind);
            ODBC.TraceODBC(3, "SQLBindCol", retcode);
            return retcode;
        }

        internal ODBC32.RetCode BindColumn3(int columnNumber, ODBC32.SQL_C targetType, IntPtr srLen_or_Ind)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLBindCol(this, (ushort) columnNumber, targetType, ADP.PtrZero, ADP.PtrZero, srLen_or_Ind);
            ODBC.TraceODBC(3, "SQLBindCol", retcode);
            return retcode;
        }

        internal ODBC32.RetCode BindParameter(short ordinal, short parameterDirection, ODBC32.SQL_C sqlctype, ODBC32.SQL_TYPE sqltype, IntPtr cchSize, IntPtr scale, HandleRef buffer, IntPtr bufferLength, HandleRef intbuffer)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLBindParameter(this, (ushort) ordinal, parameterDirection, sqlctype, (short) sqltype, cchSize, scale, buffer, bufferLength, intbuffer);
            ODBC.TraceODBC(3, "SQLBindParameter", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Cancel()
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLCancel(this);
            ODBC.TraceODBC(3, "SQLCancel", retcode);
            return retcode;
        }

        internal ODBC32.RetCode CloseCursor()
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLCloseCursor(this);
            ODBC.TraceODBC(3, "SQLCloseCursor", retcode);
            return retcode;
        }

        internal ODBC32.RetCode ColumnAttribute(int columnNumber, short fieldIdentifier, CNativeBuffer characterAttribute, out short stringLength, out SQLLEN numericAttribute)
        {
            IntPtr ptr;
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLColAttributeW(this, (short) columnNumber, fieldIdentifier, characterAttribute, characterAttribute.ShortLength, out stringLength, out ptr);
            numericAttribute = new SQLLEN(ptr);
            ODBC.TraceODBC(3, "SQLColAttributeW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Columns(string tableCatalog, string tableSchema, string tableName, string columnName)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLColumnsW(this, tableCatalog, ODBC.ShortStringLength(tableCatalog), tableSchema, ODBC.ShortStringLength(tableSchema), tableName, ODBC.ShortStringLength(tableName), columnName, ODBC.ShortStringLength(columnName));
            ODBC.TraceODBC(3, "SQLColumnsW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Execute()
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLExecute(this);
            ODBC.TraceODBC(3, "SQLExecute", retcode);
            return retcode;
        }

        internal ODBC32.RetCode ExecuteDirect(string commandText)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLExecDirectW(this, commandText, -3);
            ODBC.TraceODBC(3, "SQLExecDirectW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Fetch()
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLFetch(this);
            ODBC.TraceODBC(3, "SQLFetch", retcode);
            return retcode;
        }

        internal ODBC32.RetCode FreeStatement(ODBC32.STMT stmt)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLFreeStmt(this, stmt);
            ODBC.TraceODBC(3, "SQLFreeStmt", retcode);
            return retcode;
        }

        internal ODBC32.RetCode GetData(int index, ODBC32.SQL_C sqlctype, CNativeBuffer buffer, int cb, out IntPtr cbActual)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLGetData(this, (ushort) index, sqlctype, buffer, new IntPtr(cb), out cbActual);
            ODBC.TraceODBC(3, "SQLGetData", retcode);
            return retcode;
        }

        internal ODBC32.RetCode GetStatementAttribute(ODBC32.SQL_ATTR attribute, out IntPtr value, out int stringLength)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLGetStmtAttrW(this, attribute, out value, ADP.PtrSize, out stringLength);
            ODBC.TraceODBC(3, "SQLGetStmtAttrW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode GetTypeInfo(short fSqlType)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLGetTypeInfo(this, fSqlType);
            ODBC.TraceODBC(3, "SQLGetTypeInfo", retcode);
            return retcode;
        }

        internal ODBC32.RetCode MoreResults()
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLMoreResults(this);
            ODBC.TraceODBC(3, "SQLMoreResults", retcode);
            return retcode;
        }

        internal ODBC32.RetCode NumberOfResultColumns(out short columnsAffected)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLNumResultCols(this, out columnsAffected);
            ODBC.TraceODBC(3, "SQLNumResultCols", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Prepare(string commandText)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLPrepareW(this, commandText, -3);
            ODBC.TraceODBC(3, "SQLPrepareW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode PrimaryKeys(string catalogName, string schemaName, string tableName)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLPrimaryKeysW(this, catalogName, ODBC.ShortStringLength(catalogName), schemaName, ODBC.ShortStringLength(schemaName), tableName, ODBC.ShortStringLength(tableName));
            ODBC.TraceODBC(3, "SQLPrimaryKeysW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode ProcedureColumns(string procedureCatalog, string procedureSchema, string procedureName, string columnName)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLProcedureColumnsW(this, procedureCatalog, ODBC.ShortStringLength(procedureCatalog), procedureSchema, ODBC.ShortStringLength(procedureSchema), procedureName, ODBC.ShortStringLength(procedureName), columnName, ODBC.ShortStringLength(columnName));
            ODBC.TraceODBC(3, "SQLProcedureColumnsW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Procedures(string procedureCatalog, string procedureSchema, string procedureName)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLProceduresW(this, procedureCatalog, ODBC.ShortStringLength(procedureCatalog), procedureSchema, ODBC.ShortStringLength(procedureSchema), procedureName, ODBC.ShortStringLength(procedureName));
            ODBC.TraceODBC(3, "SQLProceduresW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode RowCount(out SQLLEN rowCount)
        {
            IntPtr ptr;
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLRowCount(this, out ptr);
            rowCount = new SQLLEN(ptr);
            ODBC.TraceODBC(3, "SQLRowCount", retcode);
            return retcode;
        }

        internal ODBC32.RetCode SetStatementAttribute(ODBC32.SQL_ATTR attribute, IntPtr value, ODBC32.SQL_IS stringLength)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLSetStmtAttrW(this, (int) attribute, value, (int) stringLength);
            ODBC.TraceODBC(3, "SQLSetStmtAttrW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode SpecialColumns(string quotedTable)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLSpecialColumnsW(this, ODBC32.SQL_SPECIALCOLS.ROWVER, null, 0, null, 0, quotedTable, ODBC.ShortStringLength(quotedTable), ODBC32.SQL_SCOPE.SESSION, ODBC32.SQL_NULLABILITY.NO_NULLS);
            ODBC.TraceODBC(3, "SQLSpecialColumnsW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Statistics(string tableName)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLStatisticsW(this, null, 0, null, 0, tableName, ODBC.ShortStringLength(tableName), 0, 1);
            ODBC.TraceODBC(3, "SQLStatisticsW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Statistics(string tableCatalog, string tableSchema, string tableName, short unique, short accuracy)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLStatisticsW(this, tableCatalog, ODBC.ShortStringLength(tableCatalog), tableSchema, ODBC.ShortStringLength(tableSchema), tableName, ODBC.ShortStringLength(tableName), unique, accuracy);
            ODBC.TraceODBC(3, "SQLStatisticsW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode Tables(string tableCatalog, string tableSchema, string tableName, string tableType)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLTablesW(this, tableCatalog, ODBC.ShortStringLength(tableCatalog), tableSchema, ODBC.ShortStringLength(tableSchema), tableName, ODBC.ShortStringLength(tableName), tableType, ODBC.ShortStringLength(tableType));
            ODBC.TraceODBC(3, "SQLTablesW", retcode);
            return retcode;
        }
    }
}

