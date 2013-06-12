namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class OdbcHandle : SafeHandle
    {
        private ODBC32.SQL_HANDLE _handleType;
        private OdbcHandle _parentHandle;

        protected OdbcHandle(ODBC32.SQL_HANDLE handleType, OdbcHandle parentHandle) : base(IntPtr.Zero, true)
        {
            this._handleType = handleType;
            bool success = false;
            ODBC32.RetCode sUCCESS = ODBC32.RetCode.SUCCESS;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                switch (handleType)
                {
                    case ODBC32.SQL_HANDLE.ENV:
                        sUCCESS = UnsafeNativeMethods.SQLAllocHandle(handleType, IntPtr.Zero, out this.handle);
                        goto Label_0099;

                    case ODBC32.SQL_HANDLE.DBC:
                    case ODBC32.SQL_HANDLE.STMT:
                        parentHandle.DangerousAddRef(ref success);
                        sUCCESS = UnsafeNativeMethods.SQLAllocHandle(handleType, parentHandle, out this.handle);
                        goto Label_0099;
                }
            }
            finally
            {
                if (success)
                {
                    switch (handleType)
                    {
                        case ODBC32.SQL_HANDLE.DBC:
                        case ODBC32.SQL_HANDLE.STMT:
                            if (!(IntPtr.Zero != base.handle))
                            {
                                goto Label_0092;
                            }
                            this._parentHandle = parentHandle;
                            break;
                    }
                }
                goto Label_0098;
            Label_0092:
                parentHandle.DangerousRelease();
            Label_0098:;
            }
        Label_0099:
            Bid.TraceSqlReturn("<odbc.SQLAllocHandle|API|ODBC|RET> %08X{SQLRETURN}\n", sUCCESS);
            if ((ADP.PtrZero == base.handle) || (sUCCESS != ODBC32.RetCode.SUCCESS))
            {
                throw ODBC.CantAllocateEnvironmentHandle(sUCCESS);
            }
        }

        internal OdbcHandle(OdbcStatementHandle parentHandle, ODBC32.SQL_ATTR attribute) : base(IntPtr.Zero, true)
        {
            ODBC32.RetCode code;
            this._handleType = ODBC32.SQL_HANDLE.DESC;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                int num;
                parentHandle.DangerousAddRef(ref success);
                code = parentHandle.GetStatementAttribute(attribute, out this.handle, out num);
            }
            finally
            {
                if (success)
                {
                    if (IntPtr.Zero != base.handle)
                    {
                        this._parentHandle = parentHandle;
                    }
                    else
                    {
                        parentHandle.DangerousRelease();
                    }
                }
            }
            if (ADP.PtrZero == base.handle)
            {
                throw ODBC.FailedToGetDescriptorHandle(code);
            }
        }

        internal ODBC32.RetCode GetDiagnosticField(out string sqlState)
        {
            short num;
            StringBuilder rchState = new StringBuilder(6);
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLGetDiagFieldW(this.HandleType, this, 1, 4, rchState, (short) (2 * rchState.Capacity), out num);
            ODBC.TraceODBC(3, "SQLGetDiagFieldW", retcode);
            switch (retcode)
            {
                case ODBC32.RetCode.SUCCESS:
                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    sqlState = rchState.ToString();
                    return retcode;
            }
            sqlState = ADP.StrEmpty;
            return retcode;
        }

        internal ODBC32.RetCode GetDiagnosticRecord(short record, out string sqlState, StringBuilder message, out int nativeError, out short cchActual)
        {
            StringBuilder rchState = new StringBuilder(5);
            ODBC32.RetCode retcode = UnsafeNativeMethods.SQLGetDiagRecW(this.HandleType, this, record, rchState, out nativeError, message, (short) message.Capacity, out cchActual);
            ODBC.TraceODBC(3, "SQLGetDiagRecW", retcode);
            switch (retcode)
            {
                case ODBC32.RetCode.SUCCESS:
                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    sqlState = rchState.ToString();
                    return retcode;
            }
            sqlState = ADP.StrEmpty;
            return retcode;
        }

        protected override bool ReleaseHandle()
        {
            IntPtr statementHandle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != statementHandle)
            {
                ODBC32.SQL_HANDLE handleType = this.HandleType;
                switch (handleType)
                {
                    case ODBC32.SQL_HANDLE.ENV:
                    case ODBC32.SQL_HANDLE.DBC:
                    case ODBC32.SQL_HANDLE.STMT:
                    {
                        ODBC32.RetCode code = UnsafeNativeMethods.SQLFreeHandle(handleType, statementHandle);
                        Bid.TraceSqlReturn("<odbc.SQLFreeHandle|API|ODBC|RET> %08X{SQLRETURN}\n", code);
                        break;
                    }
                }
            }
            OdbcHandle handle = this._parentHandle;
            this._parentHandle = null;
            if (handle != null)
            {
                handle.DangerousRelease();
                handle = null;
            }
            return true;
        }

        internal ODBC32.SQL_HANDLE HandleType
        {
            get
            {
                return this._handleType;
            }
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }
    }
}

