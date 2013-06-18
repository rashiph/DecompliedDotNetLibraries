namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;

    internal sealed class CMDWrapper
    {
        private bool _canceling;
        private readonly OdbcConnection _connection;
        internal CNativeBuffer _dataReaderBuf;
        internal bool _hasBoundColumns;
        internal OdbcDescriptorHandle _hdesc;
        private OdbcStatementHandle _keyinfostmt;
        internal CNativeBuffer _nativeParameterBuffer;
        internal bool _ssKeyInfoModeOff;
        internal bool _ssKeyInfoModeOn;
        private OdbcStatementHandle _stmt;

        internal CMDWrapper(OdbcConnection connection)
        {
            this._connection = connection;
        }

        internal void CreateKeyInfoStatementHandle()
        {
            this.DisposeKeyInfoStatementHandle();
            this._keyinfostmt = this._connection.CreateStatementHandle();
        }

        internal void CreateStatementHandle()
        {
            this.DisposeStatementHandle();
            this._stmt = this._connection.CreateStatementHandle();
        }

        internal void Dispose()
        {
            if (this._dataReaderBuf != null)
            {
                this._dataReaderBuf.Dispose();
                this._dataReaderBuf = null;
            }
            this.DisposeStatementHandle();
            CNativeBuffer buffer = this._nativeParameterBuffer;
            this._nativeParameterBuffer = null;
            if (buffer != null)
            {
                buffer.Dispose();
            }
            this._ssKeyInfoModeOn = false;
            this._ssKeyInfoModeOff = false;
        }

        private void DisposeDescriptorHandle()
        {
            OdbcDescriptorHandle handle = this._hdesc;
            if (handle != null)
            {
                this._hdesc = null;
                handle.Dispose();
            }
        }

        internal void DisposeKeyInfoStatementHandle()
        {
            OdbcStatementHandle handle = this._keyinfostmt;
            if (handle != null)
            {
                this._keyinfostmt = null;
                handle.Dispose();
            }
        }

        internal void DisposeStatementHandle()
        {
            this.DisposeKeyInfoStatementHandle();
            this.DisposeDescriptorHandle();
            OdbcStatementHandle handle = this._stmt;
            if (handle != null)
            {
                this._stmt = null;
                handle.Dispose();
            }
        }

        internal void FreeKeyInfoStatementHandle(ODBC32.STMT stmt)
        {
            OdbcStatementHandle handle = this._keyinfostmt;
            if (handle != null)
            {
                try
                {
                    handle.FreeStatement(stmt);
                }
                catch (Exception exception)
                {
                    if (ADP.IsCatchableExceptionType(exception))
                    {
                        this._keyinfostmt = null;
                        handle.Dispose();
                    }
                    throw;
                }
            }
        }

        internal void FreeStatementHandle(ODBC32.STMT stmt)
        {
            this.DisposeDescriptorHandle();
            OdbcStatementHandle handle = this._stmt;
            if (handle != null)
            {
                try
                {
                    ODBC32.RetCode retcode = handle.FreeStatement(stmt);
                    this.StatementErrorHandler(retcode);
                }
                catch (Exception exception)
                {
                    if (ADP.IsCatchableExceptionType(exception))
                    {
                        this._stmt = null;
                        handle.Dispose();
                    }
                    throw;
                }
            }
        }

        internal OdbcDescriptorHandle GetDescriptorHandle(ODBC32.SQL_ATTR attribute)
        {
            OdbcDescriptorHandle handle = this._hdesc;
            if (this._hdesc == null)
            {
                this._hdesc = handle = new OdbcDescriptorHandle(this._stmt, attribute);
            }
            return handle;
        }

        internal string GetDiagSqlState()
        {
            string str;
            this._stmt.GetDiagnosticField(out str);
            return str;
        }

        internal void StatementErrorHandler(ODBC32.RetCode retcode)
        {
            switch (retcode)
            {
                case ODBC32.RetCode.SUCCESS:
                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    this._connection.HandleErrorNoThrow(this._stmt, retcode);
                    return;
            }
            throw this._connection.HandleErrorNoThrow(this._stmt, retcode);
        }

        internal void UnbindStmtColumns()
        {
            if (this._hasBoundColumns)
            {
                this.FreeStatementHandle(ODBC32.STMT.UNBIND);
                this._hasBoundColumns = false;
            }
        }

        internal bool Canceling
        {
            get
            {
                return this._canceling;
            }
            set
            {
                this._canceling = value;
            }
        }

        internal OdbcConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        internal bool HasBoundColumns
        {
            set
            {
                this._hasBoundColumns = value;
            }
        }

        internal OdbcStatementHandle KeyInfoStatement
        {
            get
            {
                return this._keyinfostmt;
            }
        }

        internal OdbcStatementHandle StatementHandle
        {
            get
            {
                return this._stmt;
            }
        }
    }
}

