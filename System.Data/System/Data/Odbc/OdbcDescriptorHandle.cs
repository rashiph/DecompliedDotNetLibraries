namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;

    internal sealed class OdbcDescriptorHandle : OdbcHandle
    {
        internal OdbcDescriptorHandle(OdbcStatementHandle statementHandle, ODBC32.SQL_ATTR attribute) : base(statementHandle, attribute)
        {
        }

        internal ODBC32.RetCode GetDescriptionField(int i, ODBC32.SQL_DESC attribute, CNativeBuffer buffer, out int numericAttribute)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLGetDescFieldW(this, (short) i, attribute, buffer, buffer.ShortLength, out numericAttribute);
            ODBC.TraceODBC(3, "SQLGetDescFieldW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode SetDescriptionField1(short ordinal, ODBC32.SQL_DESC type, IntPtr value)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLSetDescFieldW(this, ordinal, type, value, 0);
            ODBC.TraceODBC(3, "SQLSetDescFieldW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode SetDescriptionField2(short ordinal, ODBC32.SQL_DESC type, HandleRef value)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLSetDescFieldW(this, ordinal, type, value, 0);
            ODBC.TraceODBC(3, "SQLSetDescFieldW", retcode);
            return retcode;
        }
    }
}

