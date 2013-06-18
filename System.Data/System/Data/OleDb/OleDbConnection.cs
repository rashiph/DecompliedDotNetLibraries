namespace System.Data.OleDb
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
    public sealed class OleDbConnection : DbConnection, ICloneable, IDbConnection, IDisposable
    {
        private int _closeCount;
        private static readonly DbConnectionFactory _connectionFactory = OleDbConnectionFactory.SingletonInstance;
        private DbConnectionInternal _innerConnection;
        private static int _objectTypeCount;
        private DbConnectionPoolGroup _poolGroup;
        private DbConnectionOptions _userConnectionOptions;
        private static readonly object EventInfoMessage = new object();
        internal static readonly CodeAccessPermission ExecutePermission = CreateExecutePermission();
        internal readonly int ObjectID;

        [ResDescription("DbConnection_InfoMessage"), ResCategory("DataCategory_InfoMessage")]
        public event OleDbInfoMessageEventHandler InfoMessage
        {
            add
            {
                base.Events.AddHandler(EventInfoMessage, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInfoMessage, value);
            }
        }

        public OleDbConnection()
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            this._innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
        }

        private OleDbConnection(OleDbConnection connection) : this()
        {
            this.CopyFrom(connection);
        }

        public OleDbConnection(string connectionString) : this()
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

        public OleDbTransaction BeginTransaction()
        {
            return this.BeginTransaction(System.Data.IsolationLevel.Unspecified);
        }

        public OleDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return (OleDbTransaction) this.InnerConnection.BeginTransaction(isolationLevel);
        }

        public override void ChangeDatabase(string value)
        {
            IntPtr ptr;
            ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbConnection.ChangeDatabase|API> %d#, value='%ls'\n", this.ObjectID, value);
            try
            {
                this.CheckStateOpen("ChangeDatabase");
                if ((value == null) || (value.Trim().Length == 0))
                {
                    throw ADP.EmptyDatabaseName();
                }
                this.SetDataSourcePropertyValue(OleDbPropertySetGuid.DataSource, 0x25, "current catalog", true, value);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void CheckStateOpen(string method)
        {
            ConnectionState state = this.State;
            if (ConnectionState.Open != state)
            {
                throw ADP.OpenConnectionRequired(method, state);
            }
        }

        public override void Close()
        {
            this.InnerConnection.CloseConnection(this, this.ConnectionFactory);
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

        private void CopyFrom(OleDbConnection connection)
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

        public OleDbCommand CreateCommand()
        {
            return new OleDbCommand("", this);
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
            DBDataPermission permission = (DBDataPermission) OleDbConnectionFactory.SingletonInstance.ProviderFactory.CreatePermission(PermissionState.None);
            permission.Add(string.Empty, string.Empty, KeyRestrictionBehavior.AllowOnly);
            return permission;
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
            if (disposing && base.DesignMode)
            {
                ReleaseObjectPool();
            }
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

        internal object GetDataSourcePropertyValue(Guid propertySet, int propertyID)
        {
            return this.GetOpenConnection().GetDataSourcePropertyValue(propertySet, propertyID);
        }

        internal object GetDataSourceValue(Guid propertySet, int propertyID)
        {
            object dataSourcePropertyValue = this.GetDataSourcePropertyValue(propertySet, propertyID);
            if (!(dataSourcePropertyValue is OleDbPropertyStatus) && !Convert.IsDBNull(dataSourcePropertyValue))
            {
                return dataSourcePropertyValue;
            }
            return null;
        }

        internal void GetLiteralQuotes(string method, out string quotePrefix, out string quoteSuffix)
        {
            this.CheckStateOpen(method);
            OleDbConnectionPoolGroupProviderInfo providerInfo = this.ProviderInfo;
            if (providerInfo.HasQuoteFix)
            {
                quotePrefix = providerInfo.QuotePrefix;
                quoteSuffix = providerInfo.QuoteSuffix;
            }
            else
            {
                OleDbConnectionInternal openConnection = this.GetOpenConnection();
                quotePrefix = openConnection.GetLiteralInfo(15);
                quoteSuffix = openConnection.GetLiteralInfo(0x1c);
                if (quotePrefix == null)
                {
                    quotePrefix = "";
                }
                if (quoteSuffix == null)
                {
                    quoteSuffix = quotePrefix;
                }
                providerInfo.SetQuoteFix(quotePrefix, quoteSuffix);
            }
        }

        private DbMetaDataFactory GetMetaDataFactory(DbConnectionInternal internalConnection)
        {
            return this.ConnectionFactory.GetMetaDataFactory(this._poolGroup, internalConnection);
        }

        internal DbMetaDataFactory GetMetaDataFactoryInternal(DbConnectionInternal internalConnection)
        {
            return this.GetMetaDataFactory(internalConnection);
        }

        public DataTable GetOleDbSchemaTable(Guid schema, object[] restrictions)
        {
            DataTable table;
            IntPtr ptr;
            ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbConnection.GetOleDbSchemaTable|API> %d#, schema=%ls, restrictions\n", this.ObjectID, schema);
            try
            {
                this.CheckStateOpen("GetOleDbSchemaTable");
                OleDbConnectionInternal openConnection = this.GetOpenConnection();
                if (OleDbSchemaGuid.DbInfoLiterals == schema)
                {
                    if ((restrictions != null) && (restrictions.Length != 0))
                    {
                        throw ODB.InvalidRestrictionsDbInfoLiteral("restrictions");
                    }
                    return openConnection.BuildInfoLiterals();
                }
                if (OleDbSchemaGuid.SchemaGuids == schema)
                {
                    if ((restrictions != null) && (restrictions.Length != 0))
                    {
                        throw ODB.InvalidRestrictionsSchemaGuids("restrictions");
                    }
                    return openConnection.BuildSchemaGuids();
                }
                if (OleDbSchemaGuid.DbInfoKeywords == schema)
                {
                    if ((restrictions != null) && (restrictions.Length != 0))
                    {
                        throw ODB.InvalidRestrictionsDbInfoKeywords("restrictions");
                    }
                    return openConnection.BuildInfoKeywords();
                }
                if (openConnection.SupportSchemaRowset(schema))
                {
                    return openConnection.GetSchemaRowset(schema, restrictions);
                }
                using (IDBSchemaRowsetWrapper wrapper = openConnection.IDBSchemaRowset())
                {
                    if (wrapper.Value == null)
                    {
                        throw ODB.SchemaRowsetsNotSupported(this.Provider);
                    }
                }
                throw ODB.NotSupportedSchemaTable(schema, this);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return table;
        }

        private OleDbConnectionInternal GetOpenConnection()
        {
            return (this.InnerConnection as OleDbConnectionInternal);
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

        internal DataTable GetSchemaRowset(Guid schema, object[] restrictions)
        {
            return this.GetOpenConnection().GetSchemaRowset(schema, restrictions);
        }

        internal bool HasLiveReader(OleDbCommand cmd)
        {
            bool flag = false;
            OleDbConnectionInternal openConnection = this.GetOpenConnection();
            if (openConnection != null)
            {
                flag = openConnection.HasLiveReader(cmd);
            }
            return flag;
        }

        internal System.Data.Common.UnsafeNativeMethods.ICommandText ICommandText()
        {
            return this.GetOpenConnection().ICommandText();
        }

        private IDBPropertiesWrapper IDBProperties()
        {
            return this.GetOpenConnection().IDBProperties();
        }

        internal IOpenRowsetWrapper IOpenRowset()
        {
            return this.GetOpenConnection().IOpenRowset();
        }

        internal void NotifyWeakReference(int message)
        {
            this.InnerConnection.NotifyWeakReference(message);
        }

        internal void OnInfoMessage(System.Data.Common.UnsafeNativeMethods.IErrorInfo errorInfo, OleDbHResult errorCode)
        {
            OleDbInfoMessageEventHandler handler = (OleDbInfoMessageEventHandler) base.Events[EventInfoMessage];
            if (handler != null)
            {
                try
                {
                    OleDbInfoMessageEventArgs e = new OleDbInfoMessageEventArgs(OleDbException.CreateException(errorInfo, errorCode, null));
                    if (Bid.TraceOn)
                    {
                        Bid.Trace("<oledb.OledbConnection.OnInfoMessage|API|INFO> %d#, Message='%ls'\n", this.ObjectID, e.Message);
                    }
                    handler(this, e);
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
            if (((2 & ((OleDbConnectionString) this.ConnectionOptions).OleDbServices) != 0) && ADP.NeedManualEnlistment())
            {
                this.GetOpenConnection().EnlistTransactionInternal(Transaction.Current);
            }
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

        internal static Exception ProcessResults(OleDbHResult hresult, OleDbConnection connection, object src)
        {
            if ((OleDbHResult.S_OK <= hresult) && ((connection == null) || (connection.Events[EventInfoMessage] == null)))
            {
                SafeNativeMethods.Wrapper.ClearErrorInfo();
                return null;
            }
            Exception e = null;
            System.Data.Common.UnsafeNativeMethods.IErrorInfo ppIErrorInfo = null;
            if ((System.Data.Common.UnsafeNativeMethods.GetErrorInfo(0, out ppIErrorInfo) == OleDbHResult.S_OK) && (ppIErrorInfo != null))
            {
                if (hresult < OleDbHResult.S_OK)
                {
                    e = OleDbException.CreateException(ppIErrorInfo, hresult, null);
                    if (OleDbHResult.DB_E_OBJECTOPEN == hresult)
                    {
                        e = ADP.OpenReaderExists(e);
                    }
                    ResetState(connection);
                }
                else if (connection != null)
                {
                    connection.OnInfoMessage(ppIErrorInfo, hresult);
                }
                else
                {
                    Bid.Trace("<oledb.OledbConnection|WARN|INFO> ErrorInfo available, but not connection %08X{HRESULT}\n", hresult);
                }
                Marshal.ReleaseComObject(ppIErrorInfo);
            }
            else if (OleDbHResult.S_OK < hresult)
            {
                Bid.Trace("<oledb.OledbConnection|ERR|INFO> ErrorInfo not available %08X{HRESULT}\n", hresult);
            }
            else if (hresult < OleDbHResult.S_OK)
            {
                e = ODB.NoErrorInformation((connection != null) ? connection.Provider : null, hresult, null);
                ResetState(connection);
            }
            if (e != null)
            {
                ADP.TraceExceptionAsReturnValue(e);
            }
            return e;
        }

        internal int QuotedIdentifierCase()
        {
            object dataSourcePropertyValue = this.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 100);
            if (dataSourcePropertyValue is int)
            {
                return (int) dataSourcePropertyValue;
            }
            return -1;
        }

        public static void ReleaseObjectPool()
        {
            IntPtr ptr;
            new OleDbPermission(PermissionState.Unrestricted).Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbConnection.ReleaseObjectPool|API>\n");
            try
            {
                OleDbConnectionString.ReleaseObjectPool();
                OleDbConnectionInternal.ReleaseObjectPool();
                OleDbConnectionFactory.SingletonInstance.ClearAllPools();
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void RemoveWeakReference(object value)
        {
            this.InnerConnection.RemoveWeakReference(value);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void ResetState()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<oledb.OleDbCommand.ResetState|API> %d#\n", this.ObjectID);
            try
            {
                if (this.IsOpen)
                {
                    object dataSourcePropertyValue = this.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 0xf4);
                    if (dataSourcePropertyValue is int)
                    {
                        switch (((int) dataSourcePropertyValue))
                        {
                            case 0:
                            case 2:
                                this.GetOpenConnection().DoomThisConnection();
                                this.NotifyWeakReference(-1);
                                this.Close();
                                return;

                            case 1:
                                return;
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private static void ResetState(OleDbConnection connection)
        {
            if (connection != null)
            {
                connection.ResetState();
            }
        }

        internal void SetDataSourcePropertyValue(Guid propertySet, int propertyID, string description, bool required, object value)
        {
            this.CheckStateOpen("SetProperties");
            using (IDBPropertiesWrapper wrapper = this.IDBProperties())
            {
                using (DBPropSet set = DBPropSet.CreateProperty(propertySet, propertyID, required, value))
                {
                    Bid.Trace("<oledb.IDBProperties.SetProperties|API|OLEDB> %d#\n", this.ObjectID);
                    OleDbHResult result = wrapper.Value.SetProperties(set.PropertySetCount, set);
                    Bid.Trace("<oledb.IDBProperties.SetProperties|API|OLEDB|RET> %08X{HRESULT}\n", result);
                    if (result < OleDbHResult.S_OK)
                    {
                        Exception inner = ProcessResults(result, null, this);
                        if (OleDbHResult.DB_E_ERRORSOCCURRED == result)
                        {
                            StringBuilder builder = new StringBuilder();
                            tagDBPROP[] gdbpropArray = set.GetPropertySet(0, out propertySet);
                            ODB.PropsetSetFailure(builder, description, gdbpropArray[0].dwStatus);
                            inner = ODB.PropsetSetFailure(builder.ToString(), inner);
                        }
                        if (inner != null)
                        {
                            throw inner;
                        }
                    }
                    else
                    {
                        SafeNativeMethods.Wrapper.ClearErrorInfo();
                    }
                }
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

        internal int SqlSupport()
        {
            return this.OleDbConnectionStringValue.GetSqlSupport(this);
        }

        internal bool SupportIRow(OleDbCommand cmd)
        {
            return this.OleDbConnectionStringValue.GetSupportIRow(this, cmd);
        }

        internal bool SupportMultipleResults()
        {
            return this.OleDbConnectionStringValue.GetSupportMultipleResults(this);
        }

        internal bool SupportSchemaRowset(Guid schema)
        {
            return this.GetOpenConnection().SupportSchemaRowset(schema);
        }

        object ICloneable.Clone()
        {
            OleDbConnection connection = new OleDbConnection(this);
            Bid.Trace("<oledb.OleDbConnection.Clone|API> %d#, clone=%d#\n", this.ObjectID, connection.ObjectID);
            return connection;
        }

        internal OleDbTransaction ValidateTransaction(OleDbTransaction transaction, string method)
        {
            return this.GetOpenConnection().ValidateTransaction(transaction, method);
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

        [RecommendedAsConfigurable(true), ResDescription("OleDbConnection_ConnectionString"), SettingsBindable(true), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Data"), Editor("Microsoft.VSDesigner.Data.ADO.Design.OleDbConnectionStringEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue("")]
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

        [ResDescription("OleDbConnection_ConnectionTimeout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int ConnectionTimeout
        {
            get
            {
                int num;
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<oledb.OleDbConnection.get_ConnectionTimeout|API> %d#\n", this.ObjectID);
                try
                {
                    object dataSourceValue = null;
                    if (this.IsOpen)
                    {
                        dataSourceValue = this.GetDataSourceValue(OleDbPropertySetGuid.DBInit, 0x42);
                    }
                    else
                    {
                        OleDbConnectionString oleDbConnectionStringValue = this.OleDbConnectionStringValue;
                        dataSourceValue = (oleDbConnectionStringValue != null) ? oleDbConnectionStringValue.ConnectTimeout : 15;
                    }
                    if (dataSourceValue != null)
                    {
                        return Convert.ToInt32(dataSourceValue, CultureInfo.InvariantCulture);
                    }
                    num = 15;
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
                return num;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("OleDbConnection_Database")]
        public override string Database
        {
            get
            {
                string str2;
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<oledb.OleDbConnection.get_Database|API> %d#\n", this.ObjectID);
                try
                {
                    OleDbConnectionString userConnectionOptions = (OleDbConnectionString) this.UserConnectionOptions;
                    object dataSourceValue = (userConnectionOptions != null) ? userConnectionOptions.InitialCatalog : ADP.StrEmpty;
                    if ((dataSourceValue != null) && !((string) dataSourceValue).StartsWith("|datadirectory|", StringComparison.OrdinalIgnoreCase))
                    {
                        OleDbConnectionInternal openConnection = this.GetOpenConnection();
                        if (openConnection != null)
                        {
                            if (openConnection.HasSession)
                            {
                                dataSourceValue = this.GetDataSourceValue(OleDbPropertySetGuid.DataSource, 0x25);
                            }
                            else
                            {
                                dataSourceValue = this.GetDataSourceValue(OleDbPropertySetGuid.DBInit, 0xe9);
                            }
                        }
                        else
                        {
                            userConnectionOptions = this.OleDbConnectionStringValue;
                            dataSourceValue = (userConnectionOptions != null) ? userConnectionOptions.InitialCatalog : ADP.StrEmpty;
                        }
                    }
                    str2 = Convert.ToString(dataSourceValue, CultureInfo.InvariantCulture);
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
                return str2;
            }
        }

        [Browsable(true), ResDescription("OleDbConnection_DataSource")]
        public override string DataSource
        {
            get
            {
                string str2;
                IntPtr ptr;
                Bid.ScopeEnter(out ptr, "<oledb.OleDbConnection.get_DataSource|API> %d#\n", this.ObjectID);
                try
                {
                    OleDbConnectionString userConnectionOptions = (OleDbConnectionString) this.UserConnectionOptions;
                    object dataSourceValue = (userConnectionOptions != null) ? userConnectionOptions.DataSource : ADP.StrEmpty;
                    if ((dataSourceValue != null) && !((string) dataSourceValue).StartsWith("|datadirectory|", StringComparison.OrdinalIgnoreCase))
                    {
                        if (this.IsOpen)
                        {
                            dataSourceValue = this.GetDataSourceValue(OleDbPropertySetGuid.DBInit, 0x3b);
                            if ((dataSourceValue == null) || ((dataSourceValue is string) && ((dataSourceValue as string).Length == 0)))
                            {
                                dataSourceValue = this.GetDataSourceValue(OleDbPropertySetGuid.DataSourceInfo, 0x26);
                            }
                        }
                        else
                        {
                            userConnectionOptions = this.OleDbConnectionStringValue;
                            dataSourceValue = (userConnectionOptions != null) ? userConnectionOptions.DataSource : ADP.StrEmpty;
                        }
                    }
                    str2 = Convert.ToString(dataSourceValue, CultureInfo.InvariantCulture);
                }
                finally
                {
                    Bid.ScopeLeave(ref ptr);
                }
                return str2;
            }
        }

        internal DbConnectionInternal InnerConnection
        {
            get
            {
                return this._innerConnection;
            }
        }

        internal bool IsOpen
        {
            get
            {
                return (null != this.GetOpenConnection());
            }
        }

        internal OleDbTransaction LocalTransaction
        {
            set
            {
                OleDbConnectionInternal openConnection = this.GetOpenConnection();
                if (openConnection != null)
                {
                    openConnection.LocalTransaction = value;
                }
            }
        }

        private OleDbConnectionString OleDbConnectionStringValue
        {
            get
            {
                return (OleDbConnectionString) this.ConnectionOptions;
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(true), ResDescription("OleDbConnection_Provider"), ResCategory("DataCategory_Data")]
        public string Provider
        {
            get
            {
                Bid.Trace("<oledb.OleDbConnection.get_Provider|API> %d#\n", this.ObjectID);
                OleDbConnectionString oleDbConnectionStringValue = this.OleDbConnectionStringValue;
                string str = (oleDbConnectionStringValue != null) ? oleDbConnectionStringValue.ConvertValueToString("provider", null) : null;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
        }

        internal OleDbConnectionPoolGroupProviderInfo ProviderInfo
        {
            get
            {
                return (OleDbConnectionPoolGroupProviderInfo) this.PoolGroup.ProviderInfo;
            }
        }

        [ResDescription("OleDbConnection_ServerVersion")]
        public override string ServerVersion
        {
            get
            {
                return this.InnerConnection.ServerVersion;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("DbConnection_State")]
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

