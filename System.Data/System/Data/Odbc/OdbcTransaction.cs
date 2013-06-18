namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;

    public sealed class OdbcTransaction : DbTransaction
    {
        private OdbcConnection _connection;
        private OdbcConnectionHandle _handle;
        private System.Data.IsolationLevel _isolevel = System.Data.IsolationLevel.Unspecified;

        internal OdbcTransaction(OdbcConnection connection, System.Data.IsolationLevel isolevel, OdbcConnectionHandle handle)
        {
            this._connection = connection;
            this._isolevel = isolevel;
            this._handle = handle;
        }

        public override void Commit()
        {
            OdbcConnection.ExecutePermission.Demand();
            OdbcConnection connection = this._connection;
            if (connection == null)
            {
                throw ADP.TransactionZombied(this);
            }
            connection.CheckState("CommitTransaction");
            if (this._handle == null)
            {
                throw ODBC.NotInTransaction();
            }
            ODBC32.RetCode retcode = this._handle.CompleteTransaction(0);
            if (retcode == ODBC32.RetCode.ERROR)
            {
                connection.HandleError(this._handle, retcode);
            }
            connection.LocalTransaction = null;
            this._connection = null;
            this._handle = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OdbcConnectionHandle hrHandle = this._handle;
                this._handle = null;
                if (hrHandle != null)
                {
                    try
                    {
                        ODBC32.RetCode retcode = hrHandle.CompleteTransaction(1);
                        if ((retcode == ODBC32.RetCode.ERROR) && (this._connection != null))
                        {
                            ADP.TraceExceptionWithoutRethrow(this._connection.HandleErrorNoThrow(hrHandle, retcode));
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                    }
                }
                if ((this._connection != null) && this._connection.IsOpen)
                {
                    this._connection.LocalTransaction = null;
                }
                this._connection = null;
                this._isolevel = System.Data.IsolationLevel.Unspecified;
            }
            base.Dispose(disposing);
        }

        public override void Rollback()
        {
            OdbcConnection connection = this._connection;
            if (connection == null)
            {
                throw ADP.TransactionZombied(this);
            }
            connection.CheckState("RollbackTransaction");
            if (this._handle == null)
            {
                throw ODBC.NotInTransaction();
            }
            ODBC32.RetCode retcode = this._handle.CompleteTransaction(1);
            if (retcode == ODBC32.RetCode.ERROR)
            {
                connection.HandleError(this._handle, retcode);
            }
            connection.LocalTransaction = null;
            this._connection = null;
            this._handle = null;
        }

        public OdbcConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
        }

        public override System.Data.IsolationLevel IsolationLevel
        {
            get
            {
                OdbcConnection connection = this._connection;
                if (connection == null)
                {
                    throw ADP.TransactionZombied(this);
                }
                if (System.Data.IsolationLevel.Unspecified == this._isolevel)
                {
                    int connectAttr = connection.GetConnectAttr(ODBC32.SQL_ATTR.TXN_ISOLATION, ODBC32.HANDLER.THROW);
                    switch (((ODBC32.SQL_TRANSACTION) connectAttr))
                    {
                        case ODBC32.SQL_TRANSACTION.READ_UNCOMMITTED:
                            this._isolevel = System.Data.IsolationLevel.ReadUncommitted;
                            goto Label_0094;

                        case ODBC32.SQL_TRANSACTION.READ_COMMITTED:
                            this._isolevel = System.Data.IsolationLevel.ReadCommitted;
                            goto Label_0094;

                        case ODBC32.SQL_TRANSACTION.REPEATABLE_READ:
                            this._isolevel = System.Data.IsolationLevel.RepeatableRead;
                            goto Label_0094;

                        case ODBC32.SQL_TRANSACTION.SERIALIZABLE:
                            this._isolevel = System.Data.IsolationLevel.Serializable;
                            goto Label_0094;

                        case ODBC32.SQL_TRANSACTION.SNAPSHOT:
                            this._isolevel = System.Data.IsolationLevel.Snapshot;
                            goto Label_0094;
                    }
                    throw ODBC.NoMappingForSqlTransactionLevel(connectAttr);
                }
            Label_0094:
                return this._isolevel;
            }
        }
    }
}

