namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Transactions;

    internal sealed class OdbcConnectionHandle : OdbcHandle
    {
        private HandleState _handleState;

        internal OdbcConnectionHandle(OdbcConnection connection, OdbcConnectionString constr, OdbcEnvironmentHandle environmentHandle) : base(ODBC32.SQL_HANDLE.DBC, environmentHandle)
        {
            if (connection == null)
            {
                throw ADP.ArgumentNull("connection");
            }
            if (constr == null)
            {
                throw ADP.ArgumentNull("constr");
            }
            int connectionTimeout = connection.ConnectionTimeout;
            ODBC32.RetCode retcode = this.SetConnectionAttribute2(ODBC32.SQL_ATTR.LOGIN_TIMEOUT, (IntPtr) connectionTimeout, -5);
            string connectionString = constr.UsersConnectionString(false);
            retcode = this.Connect(connectionString);
            connection.HandleError(this, retcode);
        }

        private ODBC32.RetCode AutoCommitOff()
        {
            ODBC32.RetCode code;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                code = UnsafeNativeMethods.SQLSetConnectAttrW(this, ODBC32.SQL_ATTR.AUTOCOMMIT, ODBC32.SQL_AUTOCOMMIT_OFF, -5);
                switch (code)
                {
                    case ODBC32.RetCode.SUCCESS:
                    case ODBC32.RetCode.SUCCESS_WITH_INFO:
                        this._handleState = HandleState.Transacted;
                        break;
                }
            }
            ODBC.TraceODBC(3, "SQLSetConnectAttrW", code);
            return code;
        }

        internal ODBC32.RetCode BeginTransaction(ref System.Data.IsolationLevel isolevel)
        {
            ODBC32.RetCode sUCCESS = ODBC32.RetCode.SUCCESS;
            if (System.Data.IsolationLevel.Unspecified != isolevel)
            {
                ODBC32.SQL_ATTR sql_attr;
                ODBC32.SQL_TRANSACTION sERIALIZABLE;
                switch (isolevel)
                {
                    case System.Data.IsolationLevel.RepeatableRead:
                        sERIALIZABLE = ODBC32.SQL_TRANSACTION.REPEATABLE_READ;
                        sql_attr = ODBC32.SQL_ATTR.TXN_ISOLATION;
                        break;

                    case System.Data.IsolationLevel.Serializable:
                        sERIALIZABLE = ODBC32.SQL_TRANSACTION.SERIALIZABLE;
                        sql_attr = ODBC32.SQL_ATTR.TXN_ISOLATION;
                        break;

                    case System.Data.IsolationLevel.Snapshot:
                        sERIALIZABLE = ODBC32.SQL_TRANSACTION.SNAPSHOT;
                        sql_attr = ODBC32.SQL_ATTR.SQL_COPT_SS_TXN_ISOLATION;
                        break;

                    case System.Data.IsolationLevel.Chaos:
                        throw ODBC.NotSupportedIsolationLevel(isolevel);

                    case System.Data.IsolationLevel.ReadUncommitted:
                        sERIALIZABLE = ODBC32.SQL_TRANSACTION.READ_UNCOMMITTED;
                        sql_attr = ODBC32.SQL_ATTR.TXN_ISOLATION;
                        break;

                    case System.Data.IsolationLevel.ReadCommitted:
                        sERIALIZABLE = ODBC32.SQL_TRANSACTION.READ_COMMITTED;
                        sql_attr = ODBC32.SQL_ATTR.TXN_ISOLATION;
                        break;

                    default:
                        throw ADP.InvalidIsolationLevel(isolevel);
                }
                sUCCESS = this.SetConnectionAttribute2(sql_attr, (IntPtr) ((long) sERIALIZABLE), -6);
                if (ODBC32.RetCode.SUCCESS_WITH_INFO == sUCCESS)
                {
                    isolevel = System.Data.IsolationLevel.Unspecified;
                }
            }
            switch (sUCCESS)
            {
                case ODBC32.RetCode.SUCCESS:
                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    sUCCESS = this.AutoCommitOff();
                    this._handleState = HandleState.TransactionInProgress;
                    return sUCCESS;
            }
            return sUCCESS;
        }

        internal ODBC32.RetCode CompleteTransaction(short transactionOperation)
        {
            ODBC32.RetCode code;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                code = this.CompleteTransaction(transactionOperation, base.handle);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return code;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private ODBC32.RetCode CompleteTransaction(short transactionOperation, IntPtr handle)
        {
            ODBC32.RetCode sUCCESS = ODBC32.RetCode.SUCCESS;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (HandleState.TransactionInProgress == this._handleState)
                {
                    sUCCESS = UnsafeNativeMethods.SQLEndTran(base.HandleType, handle, transactionOperation);
                    if ((sUCCESS == ODBC32.RetCode.SUCCESS) || (ODBC32.RetCode.SUCCESS_WITH_INFO == sUCCESS))
                    {
                        this._handleState = HandleState.Transacted;
                    }
                    Bid.TraceSqlReturn("<odbc.SQLEndTran|API|ODBC|RET> %08X{SQLRETURN}\n", sUCCESS);
                }
                if (HandleState.Transacted == this._handleState)
                {
                    sUCCESS = UnsafeNativeMethods.SQLSetConnectAttrW(handle, ODBC32.SQL_ATTR.AUTOCOMMIT, ODBC32.SQL_AUTOCOMMIT_ON, -5);
                    this._handleState = HandleState.Connected;
                    Bid.TraceSqlReturn("<odbc.SQLSetConnectAttr|API|ODBC|RET> %08X{SQLRETURN}\n", sUCCESS);
                }
            }
            return sUCCESS;
        }

        private ODBC32.RetCode Connect(string connectionString)
        {
            ODBC32.RetCode code;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                short num;
                code = UnsafeNativeMethods.SQLDriverConnectW(this, ADP.PtrZero, connectionString, -3, ADP.PtrZero, 0, out num, 0);
                switch (code)
                {
                    case ODBC32.RetCode.SUCCESS:
                    case ODBC32.RetCode.SUCCESS_WITH_INFO:
                        this._handleState = HandleState.Connected;
                        break;
                }
            }
            ODBC.TraceODBC(3, "SQLDriverConnectW", code);
            return code;
        }

        internal ODBC32.RetCode GetConnectionAttribute(ODBC32.SQL_ATTR attribute, byte[] buffer, out int cbActual)
        {
            ODBC32.RetCode code = UnsafeNativeMethods.SQLGetConnectAttrW(this, attribute, buffer, buffer.Length, out cbActual);
            Bid.Trace("<odbc.SQLGetConnectAttr|ODBC> SQLRETURN=%d, Attribute=%d, BufferLength=%d, StringLength=%d\n", (int) code, (int) attribute, buffer.Length, cbActual);
            return code;
        }

        internal ODBC32.RetCode GetFunctions(ODBC32.SQL_API fFunction, out short fExists)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLGetFunctions(this, fFunction, out fExists);
            ODBC.TraceODBC(3, "SQLGetFunctions", retcode);
            return retcode;
        }

        internal ODBC32.RetCode GetInfo1(ODBC32.SQL_INFO info, byte[] buffer)
        {
            ODBC32.RetCode code = UnsafeNativeMethods.SQLGetInfoW(this, info, buffer, (short) buffer.Length, ADP.PtrZero);
            Bid.Trace("<odbc.SQLGetInfo|ODBC> SQLRETURN=%d, InfoType=%d, BufferLength=%d\n", (int) code, (int) info, buffer.Length);
            return code;
        }

        internal ODBC32.RetCode GetInfo2(ODBC32.SQL_INFO info, byte[] buffer, out short cbActual)
        {
            ODBC32.RetCode code = UnsafeNativeMethods.SQLGetInfoW(this, info, buffer, (short) buffer.Length, out cbActual);
            Bid.Trace("<odbc.SQLGetInfo|ODBC> SQLRETURN=%d, InfoType=%d, BufferLength=%d, StringLength=%d\n", (int) code, (int) info, buffer.Length, cbActual);
            return code;
        }

        protected override bool ReleaseHandle()
        {
            ODBC32.RetCode code = this.CompleteTransaction(1, base.handle);
            if ((HandleState.Connected == this._handleState) || (HandleState.TransactionInProgress == this._handleState))
            {
                code = UnsafeNativeMethods.SQLDisconnect(base.handle);
                this._handleState = HandleState.Allocated;
                Bid.TraceSqlReturn("<odbc.SQLDisconnect|API|ODBC|RET> %08X{SQLRETURN}\n", code);
            }
            return base.ReleaseHandle();
        }

        internal ODBC32.RetCode SetConnectionAttribute2(ODBC32.SQL_ATTR attribute, IntPtr value, int length)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLSetConnectAttrW(this, attribute, value, length);
            ODBC.TraceODBC(3, "SQLSetConnectAttrW", retcode);
            return retcode;
        }

        internal ODBC32.RetCode SetConnectionAttribute3(ODBC32.SQL_ATTR attribute, string buffer, int length)
        {
            ODBC32.RetCode code = UnsafeNativeMethods.SQLSetConnectAttrW(this, attribute, buffer, length);
            Bid.Trace("<odbc.SQLSetConnectAttr|ODBC> SQLRETURN=%d, Attribute=%d, BufferLength=%d\n", (int) code, (int) attribute, buffer.Length);
            return code;
        }

        internal ODBC32.RetCode SetConnectionAttribute4(ODBC32.SQL_ATTR attribute, IDtcTransaction transaction, int length)
        {
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLSetConnectAttrW(this, attribute, transaction, length);
            ODBC.TraceODBC(3, "SQLSetConnectAttrW", retcode);
            return retcode;
        }

        private enum HandleState
        {
            Allocated,
            Connected,
            Transacted,
            TransactionInProgress
        }
    }
}

