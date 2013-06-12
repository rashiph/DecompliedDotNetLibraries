namespace System.Data.Odbc
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    [DefaultEvent("InfoMessage")]
    public sealed class OdbcConnection : DbConnection, ICloneable
    {
        private int _closeCount;
        private static readonly DbConnectionFactory _connectionFactory = OdbcConnectionFactory.SingletonInstance;
        private OdbcConnectionHandle _connectionHandle;
        private ConnectionState _extraState;
        private DbConnectionInternal _innerConnection;
        private static int _objectTypeCount;
        private DbConnectionPoolGroup _poolGroup;
        private DbConnectionOptions _userConnectionOptions;
        private int connectionTimeout;
        internal static readonly CodeAccessPermission ExecutePermission = CreateExecutePermission();
        internal readonly int ObjectID;
        private WeakReference weakTransaction;

        [ResCategory("DataCategory_InfoMessage"), ResDescription("DbConnection_InfoMessage")]
        public event OdbcInfoMessageEventHandler InfoMessage;

        public OdbcConnection()
        {
            this.connectionTimeout = 15;
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            this._innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
        }

        private OdbcConnection(OdbcConnection connection) : this()
        {
            this.CopyFrom(connection);
            this.connectionTimeout = connection.connectionTimeout;
        }

        public OdbcConnection(string connectionString) : this()
        {
            this.ConnectionString = connectionString;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Abort(Exception e)
        {
            DbConnectionInternal comparand = this._innerConnection;
            if (ConnectionState.Open == comparand.State)
            {
                Interlocked.CompareExchange<DbConnectionInternal>(ref this._innerConnection, DbConnectionClosedPreviouslyOpened.SingletonInstance, comparand);
                comparand.DoomThisConnection();
            }
            if (e is OutOfMemoryException)
            {
                Bid.Trace("<prov.DbConnectionHelper.Abort|RES|INFO|CPOOL> %d#, Aborting operation due to asynchronous exception: %ls\n", this.ObjectID, "OutOfMemory");
            }
            else
            {
                Bid.Trace("<prov.DbConnectionHelper.Abort|RES|INFO|CPOOL> %d#, Aborting operation due to asynchronous exception: %ls\n", this.ObjectID, e.ToString());
            }
        }

        internal void AddWeakReference(object value, int tag)
        {
            this.InnerConnection.AddWeakReference(value, tag);
        }

        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            DbTransaction transaction;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<prov.DbConnectionHelper.BeginDbTransaction|API> %d#, isolationLevel=%d{ds.IsolationLevel}", this.ObjectID, (int) isolationLevel);
            try
            {
                DbTransaction transaction2 = this.InnerConnection.BeginTransaction(isolationLevel);
                GC.KeepAlive(this);
                transaction = transaction2;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return transaction;
        }

        public OdbcTransaction BeginTransaction()
        {
            return this.BeginTransaction(System.Data.IsolationLevel.Unspecified);
        }

        public OdbcTransaction BeginTransaction(System.Data.IsolationLevel isolevel)
        {
            return (OdbcTransaction) this.InnerConnection.BeginTransaction(isolevel);
        }

        public override void ChangeDatabase(string value)
        {
            this.InnerConnection.ChangeDatabase(value);
        }

        internal void CheckState(string method)
        {
            ConnectionState internalState = this.InternalState;
            if (ConnectionState.Open != internalState)
            {
                throw ADP.OpenConnectionRequired(method, internalState);
            }
        }

        public override void Close()
        {
            this.InnerConnection.CloseConnection(this, this.ConnectionFactory);
            OdbcConnectionHandle handle = this._connectionHandle;
            if (handle != null)
            {
                this._connectionHandle = null;
                WeakReference weakTransaction = this.weakTransaction;
                if (weakTransaction != null)
                {
                    this.weakTransaction = null;
                    IDisposable target = weakTransaction.Target as OdbcTransaction;
                    if ((target != null) && weakTransaction.IsAlive)
                    {
                        target.Dispose();
                    }
                }
                handle.Dispose();
            }
        }

        internal bool ConnectionIsAlive(Exception innerException)
        {
            if (!this.IsOpen)
            {
                return false;
            }
            if (!this.ProviderInfo.NoConnectionDead)
            {
                int connectAttr = this.GetConnectAttr(ODBC32.SQL_ATTR.CONNECTION_DEAD, ODBC32.HANDLER.IGNORE);
                if (1 == connectAttr)
                {
                    this.Close();
                    throw ADP.ConnectionIsDisabled(innerException);
                }
            }
            return true;
        }

        private string ConnectionString_Get()
        {
            Bid.Trace("<prov.DbConnectionHelper.ConnectionString_Get|API> %d#\n", this.ObjectID);
            bool shouldHidePassword = this.InnerConnection.ShouldHidePassword;
            DbConnectionOptions userConnectionOptions = this.UserConnectionOptions;
            if (userConnectionOptions == null)
            {
                return "";
            }
            return userConnectionOptions.UsersConnectionString(shouldHidePassword);
        }

        private void ConnectionString_Set(string value)
        {
            DbConnectionOptions userConnectionOptions = null;
            DbConnectionPoolGroup group = this.ConnectionFactory.GetConnectionPoolGroup(value, null, ref userConnectionOptions);
            DbConnectionInternal innerConnection = this.InnerConnection;
            bool allowSetConnectionString = innerConnection.AllowSetConnectionString;
            if (allowSetConnectionString)
            {
                allowSetConnectionString = this.SetInnerConnectionFrom(DbConnectionClosedBusy.SingletonInstance, innerConnection);
                if (allowSetConnectionString)
                {
                    this._userConnectionOptions = userConnectionOptions;
                    this._poolGroup = group;
                    this._innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
                }
            }
            if (!allowSetConnectionString)
            {
                throw ADP.OpenConnectionPropertySet("ConnectionString", innerConnection.State);
            }
            if (Bid.TraceOn)
            {
                string str = (userConnectionOptions != null) ? userConnectionOptions.UsersConnectionStringForTrace() : "";
                Bid.Trace("<prov.DbConnectionHelper.ConnectionString_Set|API> %d#, '%ls'\n", this.ObjectID, str);
            }
        }

        private void CopyFrom(OdbcConnection connection)
        {
            ADP.CheckArgumentNull(connection, "connection");
            this._userConnectionOptions = connection.UserConnectionOptions;
            this._poolGroup = connection.PoolGroup;
            if (DbConnectionClosedNeverOpened.SingletonInstance == connection._innerConnection)
            {
                this._innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
            }
            else
            {
                this._innerConnection = DbConnectionClosedPreviouslyOpened.SingletonInstance;
            }
        }

        public OdbcCommand CreateCommand()
        {
            return new OdbcCommand(string.Empty, this);
        }

        protected override DbCommand CreateDbCommand()
        {
            DbCommand command = null;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<prov.DbConnectionHelper.CreateDbCommand|API> %d#\n", this.ObjectID);
            try
            {
                command = this.ConnectionFactory.ProviderFactory.CreateCommand();
                command.Connection = this;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return command;
        }

        private static CodeAccessPermission CreateExecutePermission()
        {
            DBDataPermission permission = (DBDataPermission) OdbcConnectionFactory.SingletonInstance.ProviderFactory.CreatePermission(PermissionState.None);
            permission.Add(string.Empty, string.Empty, KeyRestrictionBehavior.AllowOnly);
            return permission;
        }

        internal OdbcStatementHandle CreateStatementHandle()
        {
            return new OdbcStatementHandle(this.ConnectionHandle);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._userConnectionOptions = null;
                this._poolGroup = null;
                this.Close();
            }
            this.DisposeMe(disposing);
            base.Dispose(disposing);
        }

        private void DisposeMe(bool disposing)
        {
        }

        public void EnlistDistributedTransaction(ITransaction transaction)
        {
            this.EnlistDistributedTransactionHelper(transaction);
        }

        private void EnlistDistributedTransactionHelper(ITransaction transaction)
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(ExecutePermission);
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.Demand();
            Bid.Trace("<prov.DbConnectionHelper.EnlistDistributedTransactionHelper|RES|TRAN> %d#, Connection enlisting in a transaction.\n", this.ObjectID);
            Transaction transactionFromDtcTransaction = null;
            if (transaction != null)
            {
                transactionFromDtcTransaction = TransactionInterop.GetTransactionFromDtcTransaction((IDtcTransaction) transaction);
            }
            this.InnerConnection.EnlistTransaction(transactionFromDtcTransaction);
            GC.KeepAlive(this);
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            ExecutePermission.Demand();
            Bid.Trace("<prov.DbConnectionHelper.EnlistTransaction|RES|TRAN> %d#, Connection enlisting in a transaction.\n", this.ObjectID);
            DbConnectionInternal innerConnection = this.InnerConnection;
            Transaction enlistedTransaction = innerConnection.EnlistedTransaction;
            if (enlistedTransaction != null)
            {
                if (enlistedTransaction.Equals(transaction))
                {
                    return;
                }
                if (enlistedTransaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Active)
                {
                    throw ADP.TransactionPresent();
                }
            }
            innerConnection.EnlistTransaction(transaction);
            GC.KeepAlive(this);
        }

        internal char EscapeChar(string method)
        {
            this.CheckState(method);
            if (!this.ProviderInfo.HasEscapeChar)
            {
                string infoStringUnhandled = this.GetInfoStringUnhandled(ODBC32.SQL_INFO.SEARCH_PATTERN_ESCAPE);
                this.ProviderInfo.EscapeChar = (infoStringUnhandled.Length == 1) ? infoStringUnhandled[0] : this.QuoteChar(method)[0];
            }
            return this.ProviderInfo.EscapeChar;
        }

        internal void FlagRestrictedSqlBindType(ODBC32.SQL_TYPE sqltype)
        {
            ODBC32.SQL_CVT nUMERIC;
            switch (sqltype)
            {
                case ODBC32.SQL_TYPE.NUMERIC:
                    nUMERIC = ODBC32.SQL_CVT.NUMERIC;
                    break;

                case ODBC32.SQL_TYPE.DECIMAL:
                    nUMERIC = ODBC32.SQL_CVT.DECIMAL;
                    break;

                default:
                    return;
            }
            OdbcConnectionPoolGroupProviderInfo providerInfo = this.ProviderInfo;
            providerInfo.RestrictedSQLBindTypes |= nUMERIC;
        }

        internal void FlagUnsupportedColAttr(ODBC32.SQL_DESC v3FieldId, ODBC32.SQL_COLUMN v2FieldId)
        {
            if (this.IsV3Driver)
            {
                if (v3FieldId == ((ODBC32.SQL_DESC) 0x4bc))
                {
                    this.ProviderInfo.NoSqlCASSColumnKey = true;
                }
            }
        }

        internal void FlagUnsupportedConnectAttr(ODBC32.SQL_ATTR Attribute)
        {
            ODBC32.SQL_ATTR sql_attr = Attribute;
            if (sql_attr != ODBC32.SQL_ATTR.CURRENT_CATALOG)
            {
                if (sql_attr != ODBC32.SQL_ATTR.CONNECTION_DEAD)
                {
                    return;
                }
            }
            else
            {
                this.ProviderInfo.NoCurrentCatalog = true;
                return;
            }
            this.ProviderInfo.NoConnectionDead = true;
        }

        internal void FlagUnsupportedStmtAttr(ODBC32.SQL_ATTR Attribute)
        {
            switch (Attribute)
            {
                case ODBC32.SQL_ATTR.SQL_COPT_SS_TXN_ISOLATION:
                    this.ProviderInfo.NoSqlSoptSSHiddenColumns = true;
                    return;

                case ((ODBC32.SQL_ATTR) 0x4cc):
                    this.ProviderInfo.NoSqlSoptSSNoBrowseTable = true;
                    return;

                case ODBC32.SQL_ATTR.QUERY_TIMEOUT:
                    this.ProviderInfo.NoQueryTimeout = true;
                    return;
            }
        }

        internal int GetConnectAttr(ODBC32.SQL_ATTR attribute, ODBC32.HANDLER handler)
        {
            int cbActual = 0;
            byte[] buffer = new byte[4];
            OdbcConnectionHandle connectionHandle = this.ConnectionHandle;
            if (connectionHandle != null)
            {
                ODBC32.RetCode retcode = connectionHandle.GetConnectionAttribute(attribute, buffer, out cbActual);
                if ((retcode == ODBC32.RetCode.SUCCESS) || (ODBC32.RetCode.SUCCESS_WITH_INFO == retcode))
                {
                    return BitConverter.ToInt32(buffer, 0);
                }
                if (retcode == ODBC32.RetCode.ERROR)
                {
                    string diagSqlState = this.GetDiagSqlState();
                    if ((("HYC00" == diagSqlState) || ("HY092" == diagSqlState)) || ("IM001" == diagSqlState))
                    {
                        this.FlagUnsupportedConnectAttr(attribute);
                    }
                }
                if (handler == ODBC32.HANDLER.THROW)
                {
                    this.HandleError(connectionHandle, retcode);
                }
            }
            return -1;
        }

        internal string GetConnectAttrString(ODBC32.SQL_ATTR attribute)
        {
            string str2 = "";
            int cbActual = 0;
            byte[] buffer = new byte[100];
            OdbcConnectionHandle connectionHandle = this.ConnectionHandle;
            if (connectionHandle != null)
            {
                ODBC32.RetCode code = connectionHandle.GetConnectionAttribute(attribute, buffer, out cbActual);
                if ((buffer.Length + 2) <= cbActual)
                {
                    buffer = new byte[cbActual + 2];
                    code = connectionHandle.GetConnectionAttribute(attribute, buffer, out cbActual);
                }
                if ((code == ODBC32.RetCode.SUCCESS) || (ODBC32.RetCode.SUCCESS_WITH_INFO == code))
                {
                    return Encoding.Unicode.GetString(buffer, 0, Math.Min(cbActual, buffer.Length));
                }
                if (code != ODBC32.RetCode.ERROR)
                {
                    return str2;
                }
                string diagSqlState = this.GetDiagSqlState();
                if ((!("HYC00" == diagSqlState) && !("HY092" == diagSqlState)) && !("IM001" == diagSqlState))
                {
                    return str2;
                }
                this.FlagUnsupportedConnectAttr(attribute);
            }
            return str2;
        }

        private string GetDiagSqlState()
        {
            string str;
            this.ConnectionHandle.GetDiagnosticField(out str);
            return str;
        }

        internal ODBC32.RetCode GetInfoInt16Unhandled(ODBC32.SQL_INFO info, out short resultValue)
        {
            byte[] buffer = new byte[2];
            ODBC32.RetCode code = this.ConnectionHandle.GetInfo1(info, buffer);
            resultValue = BitConverter.ToInt16(buffer, 0);
            return code;
        }

        private int GetInfoInt32Unhandled(ODBC32.SQL_INFO infotype)
        {
            byte[] buffer = new byte[4];
            this.ConnectionHandle.GetInfo1(infotype, buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        internal ODBC32.RetCode GetInfoInt32Unhandled(ODBC32.SQL_INFO info, out int resultValue)
        {
            byte[] buffer = new byte[4];
            ODBC32.RetCode code = this.ConnectionHandle.GetInfo1(info, buffer);
            resultValue = BitConverter.ToInt32(buffer, 0);
            return code;
        }

        internal string GetInfoStringUnhandled(ODBC32.SQL_INFO info)
        {
            return this.GetInfoStringUnhandled(info, false);
        }

        private string GetInfoStringUnhandled(ODBC32.SQL_INFO info, bool handleError)
        {
            string str = null;
            short cbActual = 0;
            byte[] buffer = new byte[100];
            OdbcConnectionHandle connectionHandle = this.ConnectionHandle;
            if (connectionHandle != null)
            {
                ODBC32.RetCode retcode = connectionHandle.GetInfo2(info, buffer, out cbActual);
                if (buffer.Length < (cbActual - 2))
                {
                    buffer = new byte[cbActual + 2];
                    retcode = connectionHandle.GetInfo2(info, buffer, out cbActual);
                }
                if ((retcode == ODBC32.RetCode.SUCCESS) || (retcode == ODBC32.RetCode.SUCCESS_WITH_INFO))
                {
                    return Encoding.Unicode.GetString(buffer, 0, Math.Min(cbActual, buffer.Length));
                }
                if (handleError)
                {
                    this.HandleError(this.ConnectionHandle, retcode);
                }
                return str;
            }
            if (handleError)
            {
                str = "";
            }
            return str;
        }

        private DbMetaDataFactory GetMetaDataFactory(DbConnectionInternal internalConnection)
        {
            return this.ConnectionFactory.GetMetaDataFactory(this._poolGroup, internalConnection);
        }

        internal DbMetaDataFactory GetMetaDataFactoryInternal(DbConnectionInternal internalConnection)
        {
            return this.GetMetaDataFactory(internalConnection);
        }

        public override DataTable GetSchema()
        {
            return this.GetSchema(DbMetaDataCollectionNames.MetaDataCollections, null);
        }

        public override DataTable GetSchema(string collectionName)
        {
            return this.GetSchema(collectionName, null);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            ExecutePermission.Demand();
            return this.InnerConnection.GetSchema(this.ConnectionFactory, this.PoolGroup, this, collectionName, restrictionValues);
        }

        internal void HandleError(OdbcHandle hrHandle, ODBC32.RetCode retcode)
        {
            Exception exception = this.HandleErrorNoThrow(hrHandle, retcode);
            switch (retcode)
            {
                case ODBC32.RetCode.SUCCESS:
                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    return;
            }
            throw exception;
        }

        internal Exception HandleErrorNoThrow(OdbcHandle hrHandle, ODBC32.RetCode retcode)
        {
            switch (retcode)
            {
                case ODBC32.RetCode.SUCCESS:
                    break;

                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    if (this.infoMessageEventHandler != null)
                    {
                        OdbcErrorCollection errors = ODBC32.GetDiagErrors(null, hrHandle, retcode);
                        errors.SetSource(this.Driver);
                        this.OnInfoMessage(new OdbcInfoMessageEventArgs(errors));
                    }
                    break;

                default:
                {
                    OdbcException innerException = OdbcException.CreateException(ODBC32.GetDiagErrors(null, hrHandle, retcode), retcode);
                    if (innerException != null)
                    {
                        innerException.Errors.SetSource(this.Driver);
                    }
                    this.ConnectionIsAlive(innerException);
                    return innerException;
                }
            }
            return null;
        }

        internal void NotifyWeakReference(int message)
        {
            this.InnerConnection.NotifyWeakReference(message);
        }

        private void OnInfoMessage(OdbcInfoMessageEventArgs args)
        {
            if (this.infoMessageEventHandler != null)
            {
                try
                {
                    this.infoMessageEventHandler(this, args);
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableOrSecurityExceptionType(exception))
                    {
                        throw;
                    }
                    ADP.TraceExceptionWithoutRethrow(exception);
                }
            }
        }

        public override void Open()
        {
            this.InnerConnection.OpenConnection(this, this.ConnectionFactory);
            if (ADP.NeedManualEnlistment())
            {
                this.EnlistTransaction(Transaction.Current);
            }
        }

        internal OdbcTransaction Open_BeginTransaction(System.Data.IsolationLevel isolevel)
        {
            ExecutePermission.Demand();
            this.CheckState("BeginTransaction");
            this.RollbackDeadTransaction();
            if ((this.weakTransaction != null) && this.weakTransaction.IsAlive)
            {
                throw ADP.ParallelTransactionsNotSupported(this);
            }
            switch (isolevel)
            {
                case System.Data.IsolationLevel.Unspecified:
                case System.Data.IsolationLevel.ReadUncommitted:
                case System.Data.IsolationLevel.ReadCommitted:
                case System.Data.IsolationLevel.RepeatableRead:
                case System.Data.IsolationLevel.Serializable:
                case System.Data.IsolationLevel.Snapshot:
                {
                    OdbcConnectionHandle connectionHandle = this.ConnectionHandle;
                    ODBC32.RetCode retcode = connectionHandle.BeginTransaction(ref isolevel);
                    if (retcode == ODBC32.RetCode.ERROR)
                    {
                        this.HandleError(connectionHandle, retcode);
                    }
                    OdbcTransaction target = new OdbcTransaction(this, isolevel, connectionHandle);
                    this.weakTransaction = new WeakReference(target);
                    return target;
                }
                case System.Data.IsolationLevel.Chaos:
                    throw ODBC.NotSupportedIsolationLevel(isolevel);
            }
            throw ADP.InvalidIsolationLevel(isolevel);
        }

        internal void Open_ChangeDatabase(string value)
        {
            ExecutePermission.Demand();
            this.CheckState("ChangeDatabase");
            if ((value == null) || (value.Trim().Length == 0))
            {
                throw ADP.EmptyDatabaseName();
            }
            if (0x400 < ((value.Length * 2) + 2))
            {
                throw ADP.DatabaseNameTooLong();
            }
            this.RollbackDeadTransaction();
            OdbcConnectionHandle connectionHandle = this.ConnectionHandle;
            ODBC32.RetCode retcode = connectionHandle.SetConnectionAttribute3(ODBC32.SQL_ATTR.CURRENT_CATALOG, value, value.Length * 2);
            if (retcode != ODBC32.RetCode.SUCCESS)
            {
                this.HandleError(connectionHandle, retcode);
            }
        }

        internal void Open_EnlistTransaction(Transaction transaction)
        {
            ODBC32.RetCode code;
            if ((this.weakTransaction != null) && this.weakTransaction.IsAlive)
            {
                throw ADP.LocalTransactionPresent();
            }
            IDtcTransaction oletxTransaction = ADP.GetOletxTransaction(transaction);
            OdbcConnectionHandle connectionHandle = this.ConnectionHandle;
            if (oletxTransaction == null)
            {
                code = connectionHandle.SetConnectionAttribute2(ODBC32.SQL_ATTR.SQL_COPT_SS_ENLIST_IN_DTC, IntPtr.Zero, 1);
            }
            else
            {
                code = connectionHandle.SetConnectionAttribute4(ODBC32.SQL_ATTR.SQL_COPT_SS_ENLIST_IN_DTC, oletxTransaction, 1);
            }
            if (code != ODBC32.RetCode.SUCCESS)
            {
                this.HandleError(connectionHandle, code);
            }
            ((OdbcConnectionOpen) this.InnerConnection).EnlistedTransaction = transaction;
        }

        internal string Open_GetServerVersion()
        {
            return this.GetInfoStringUnhandled(ODBC32.SQL_INFO.DBMS_VER, true);
        }

        internal void PermissionDemand()
        {
            DbConnectionPoolGroup poolGroup = this.PoolGroup;
            DbConnectionOptions options = (poolGroup != null) ? poolGroup.ConnectionOptions : null;
            if ((options == null) || options.IsEmpty)
            {
                throw ADP.NoConnectionString();
            }
            this.UserConnectionOptions.DemandPermission();
        }

        internal string QuoteChar(string method)
        {
            this.CheckState(method);
            if (!this.ProviderInfo.HasQuoteChar)
            {
                string infoStringUnhandled = this.GetInfoStringUnhandled(ODBC32.SQL_INFO.IDENTIFIER_QUOTE_CHAR);
                this.ProviderInfo.QuoteChar = (1 == infoStringUnhandled.Length) ? infoStringUnhandled : "\0";
            }
            return this.ProviderInfo.QuoteChar;
        }

        public static void ReleaseObjectPool()
        {
            new OdbcPermission(PermissionState.Unrestricted).Demand();
            OdbcEnvironment.ReleaseObjectPool();
        }

        internal void RemoveWeakReference(object value)
        {
            this.InnerConnection.RemoveWeakReference(value);
        }

        private void RollbackDeadTransaction()
        {
            WeakReference weakTransaction = this.weakTransaction;
            if ((weakTransaction != null) && !weakTransaction.IsAlive)
            {
                this.weakTransaction = null;
                this.ConnectionHandle.CompleteTransaction(1);
            }
        }

        internal void SetInnerConnectionEvent(DbConnectionInternal to)
        {
            ConnectionState originalState = this._innerConnection.State & ConnectionState.Open;
            ConnectionState currentState = to.State & ConnectionState.Open;
            if ((originalState != currentState) && (currentState == ConnectionState.Closed))
            {
                this._closeCount++;
            }
            this._innerConnection = to;
            if ((originalState == ConnectionState.Closed) && (ConnectionState.Open == currentState))
            {
                this.OnStateChange(DbConnectionInternal.StateChangeOpen);
            }
            else if ((ConnectionState.Open == originalState) && (currentState == ConnectionState.Closed))
            {
                this.OnStateChange(DbConnectionInternal.StateChangeClosed);
            }
            else if (originalState != currentState)
            {
                this.OnStateChange(new StateChangeEventArgs(originalState, currentState));
            }
        }

        internal bool SetInnerConnectionFrom(DbConnectionInternal to, DbConnectionInternal from)
        {
            return (from == Interlocked.CompareExchange<DbConnectionInternal>(ref this._innerConnection, to, from));
        }

        internal void SetInnerConnectionTo(DbConnectionInternal to)
        {
            this._innerConnection = to;
        }

        internal OdbcTransaction SetStateExecuting(string method, OdbcTransaction transaction)
        {
            if (this.weakTransaction != null)
            {
                OdbcTransaction target = this.weakTransaction.Target as OdbcTransaction;
                if (transaction != target)
                {
                    if (transaction == null)
                    {
                        throw ADP.TransactionRequired(method);
                    }
                    if (this != transaction.Connection)
                    {
                        throw ADP.TransactionConnectionMismatch();
                    }
                    transaction = null;
                }
            }
            else if (transaction != null)
            {
                if (transaction.Connection != null)
                {
                    throw ADP.TransactionConnectionMismatch();
                }
                transaction = null;
            }
            ConnectionState internalState = this.InternalState;
            if (ConnectionState.Open == internalState)
            {
                return transaction;
            }
            this.NotifyWeakReference(1);
            internalState = this.InternalState;
            if (ConnectionState.Open == internalState)
            {
                return transaction;
            }
            if ((ConnectionState.Fetching & internalState) != ConnectionState.Closed)
            {
                throw ADP.OpenReaderExists();
            }
            throw ADP.OpenConnectionRequired(method, internalState);
        }

        internal void SetSupportedType(ODBC32.SQL_TYPE sqltype)
        {
            ODBC32.SQL_CVT wLONGVARCHAR;
            switch (sqltype)
            {
                case ODBC32.SQL_TYPE.WLONGVARCHAR:
                    wLONGVARCHAR = ODBC32.SQL_CVT.WLONGVARCHAR;
                    break;

                case ODBC32.SQL_TYPE.WVARCHAR:
                    wLONGVARCHAR = ODBC32.SQL_CVT.WVARCHAR;
                    break;

                case ODBC32.SQL_TYPE.WCHAR:
                    wLONGVARCHAR = ODBC32.SQL_CVT.WCHAR;
                    break;

                case ODBC32.SQL_TYPE.NUMERIC:
                    wLONGVARCHAR = ODBC32.SQL_CVT.NUMERIC;
                    break;

                default:
                    return;
            }
            OdbcConnectionPoolGroupProviderInfo providerInfo = this.ProviderInfo;
            providerInfo.TestedSQLTypes |= wLONGVARCHAR;
            OdbcConnectionPoolGroupProviderInfo info2 = this.ProviderInfo;
            info2.SupportedSQLTypes |= wLONGVARCHAR;
        }

        internal bool SQLGetFunctions(ODBC32.SQL_API odbcFunction)
        {
            short num;
            OdbcConnectionHandle connectionHandle = this.ConnectionHandle;
            if (connectionHandle == null)
            {
                throw ADP.InvalidOperation("what is the right exception to throw here?");
            }
            ODBC32.RetCode functions = connectionHandle.GetFunctions(odbcFunction, out num);
            if (functions != ODBC32.RetCode.SUCCESS)
            {
                this.HandleError(connectionHandle, functions);
            }
            if (num == 0)
            {
                return false;
            }
            return true;
        }

        object ICloneable.Clone()
        {
            OdbcConnection connection = new OdbcConnection(this);
            Bid.Trace("<odbc.OdbcConnection.Clone|API> %d#, clone=%d#\n", this.ObjectID, connection.ObjectID);
            return connection;
        }

        internal bool TestRestrictedSqlBindType(ODBC32.SQL_TYPE sqltype)
        {
            ODBC32.SQL_CVT nUMERIC;
            switch (sqltype)
            {
                case ODBC32.SQL_TYPE.NUMERIC:
                    nUMERIC = ODBC32.SQL_CVT.NUMERIC;
                    break;

                case ODBC32.SQL_TYPE.DECIMAL:
                    nUMERIC = ODBC32.SQL_CVT.DECIMAL;
                    break;

                default:
                    return false;
            }
            return (0 != (this.ProviderInfo.RestrictedSQLBindTypes & nUMERIC));
        }

        internal bool TestTypeSupport(ODBC32.SQL_TYPE sqltype)
        {
            ODBC32.SQL_CVT wLONGVARCHAR;
            ODBC32.SQL_CONVERT lONGVARCHAR;
            switch (sqltype)
            {
                case ODBC32.SQL_TYPE.WLONGVARCHAR:
                    lONGVARCHAR = ODBC32.SQL_CONVERT.LONGVARCHAR;
                    wLONGVARCHAR = ODBC32.SQL_CVT.WLONGVARCHAR;
                    break;

                case ODBC32.SQL_TYPE.WVARCHAR:
                    lONGVARCHAR = ODBC32.SQL_CONVERT.VARCHAR;
                    wLONGVARCHAR = ODBC32.SQL_CVT.WVARCHAR;
                    break;

                case ODBC32.SQL_TYPE.WCHAR:
                    lONGVARCHAR = ODBC32.SQL_CONVERT.CHAR;
                    wLONGVARCHAR = ODBC32.SQL_CVT.WCHAR;
                    break;

                case ODBC32.SQL_TYPE.NUMERIC:
                    lONGVARCHAR = ODBC32.SQL_CONVERT.NUMERIC;
                    wLONGVARCHAR = ODBC32.SQL_CVT.NUMERIC;
                    break;

                default:
                    return false;
            }
            if ((this.ProviderInfo.TestedSQLTypes & wLONGVARCHAR) == 0)
            {
                int num = this.GetInfoInt32Unhandled((ODBC32.SQL_INFO) lONGVARCHAR) & wLONGVARCHAR;
                OdbcConnectionPoolGroupProviderInfo providerInfo = this.ProviderInfo;
                providerInfo.TestedSQLTypes |= wLONGVARCHAR;
                OdbcConnectionPoolGroupProviderInfo info2 = this.ProviderInfo;
                info2.SupportedSQLTypes |= num;
            }
            return (0 != (this.ProviderInfo.SupportedSQLTypes & wLONGVARCHAR));
        }

        [Conditional("DEBUG")]
        internal static void VerifyExecutePermission()
        {
            try
            {
                ExecutePermission.Demand();
            }
            catch (SecurityException)
            {
                throw;
            }
        }

        internal int CloseCount
        {
            get
            {
                return this._closeCount;
            }
        }

        internal DbConnectionFactory ConnectionFactory
        {
            get
            {
                return _connectionFactory;
            }
        }

        internal OdbcConnectionHandle ConnectionHandle
        {
            get
            {
                return this._connectionHandle;
            }
            set
            {
                this._connectionHandle = value;
            }
        }

        internal DbConnectionOptions ConnectionOptions
        {
            get
            {
                DbConnectionPoolGroup poolGroup = this.PoolGroup;
                if (poolGroup == null)
                {
                    return null;
                }
                return poolGroup.ConnectionOptions;
            }
        }

        [SettingsBindable(true), ResCategory("DataCategory_Data"), DefaultValue(""), ResDescription("OdbcConnection_ConnectionString"), RecommendedAsConfigurable(true), RefreshProperties(RefreshProperties.All), Editor("Microsoft.VSDesigner.Data.Odbc.Design.OdbcConnectionStringEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public override string ConnectionString
        {
            get
            {
                return this.ConnectionString_Get();
            }
            set
            {
                this.ConnectionString_Set(value);
            }
        }

        [ResDescription("OdbcConnection_ConnectionTimeout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(15), ResCategory("DataCategory_Data")]
        public int ConnectionTimeout
        {
            get
            {
                return this.connectionTimeout;
            }
            set
            {
                if (value < 0)
                {
                    throw ODBC.NegativeArgument();
                }
                if (this.IsOpen)
                {
                    throw ODBC.CantSetPropertyOnOpenConnection();
                }
                this.connectionTimeout = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("OdbcConnection_Database")]
        public override string Database
        {
            get
            {
                if (this.IsOpen && !this.ProviderInfo.NoCurrentCatalog)
                {
                    return this.GetConnectAttrString(ODBC32.SQL_ATTR.CURRENT_CATALOG);
                }
                return string.Empty;
            }
        }

        [ResDescription("OdbcConnection_DataSource"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override string DataSource
        {
            get
            {
                if (this.IsOpen)
                {
                    return this.GetInfoStringUnhandled(ODBC32.SQL_INFO.SERVER_NAME, true);
                }
                return string.Empty;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("OdbcConnection_Driver")]
        public string Driver
        {
            get
            {
                if (!this.IsOpen)
                {
                    return ADP.StrEmpty;
                }
                if (this.ProviderInfo.DriverName == null)
                {
                    this.ProviderInfo.DriverName = this.GetInfoStringUnhandled(ODBC32.SQL_INFO.DRIVER_NAME);
                }
                return this.ProviderInfo.DriverName;
            }
        }

        internal DbConnectionInternal InnerConnection
        {
            get
            {
                return this._innerConnection;
            }
        }

        internal ConnectionState InternalState
        {
            get
            {
                return (this.State | this._extraState);
            }
        }

        internal bool IsOpen
        {
            get
            {
                return (this.InnerConnection is OdbcConnectionOpen);
            }
        }

        internal bool IsV3Driver
        {
            get
            {
                if (this.ProviderInfo.DriverVersion == null)
                {
                    this.ProviderInfo.DriverVersion = this.GetInfoStringUnhandled(ODBC32.SQL_INFO.DRIVER_ODBC_VER);
                    if ((this.ProviderInfo.DriverVersion != null) && (this.ProviderInfo.DriverVersion.Length >= 2))
                    {
                        try
                        {
                            this.ProviderInfo.IsV3Driver = int.Parse(this.ProviderInfo.DriverVersion.Substring(0, 2), CultureInfo.InvariantCulture) >= 3;
                        }
                        catch (FormatException exception)
                        {
                            this.ProviderInfo.IsV3Driver = false;
                            ADP.TraceExceptionWithoutRethrow(exception);
                        }
                    }
                    else
                    {
                        this.ProviderInfo.DriverVersion = "";
                    }
                }
                return this.ProviderInfo.IsV3Driver;
            }
        }

        internal OdbcTransaction LocalTransaction
        {
            get
            {
                OdbcTransaction target = null;
                if (this.weakTransaction != null)
                {
                    target = (OdbcTransaction) this.weakTransaction.Target;
                }
                return target;
            }
            set
            {
                this.weakTransaction = null;
                if (value != null)
                {
                    this.weakTransaction = new WeakReference(value);
                }
            }
        }

        internal DbConnectionPoolGroup PoolGroup
        {
            get
            {
                return this._poolGroup;
            }
            set
            {
                this._poolGroup = value;
            }
        }

        internal OdbcConnectionPoolGroupProviderInfo ProviderInfo
        {
            get
            {
                return (OdbcConnectionPoolGroupProviderInfo) this.PoolGroup.ProviderInfo;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), ResDescription("OdbcConnection_ServerVersion")]
        public override string ServerVersion
        {
            get
            {
                return this.InnerConnection.ServerVersion;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("DbConnection_State"), Browsable(false)]
        public override ConnectionState State
        {
            get
            {
                return this.InnerConnection.State;
            }
        }

        internal DbConnectionOptions UserConnectionOptions
        {
            get
            {
                return this._userConnectionOptions;
            }
        }
    }
}

