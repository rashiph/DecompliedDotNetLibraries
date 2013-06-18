namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    [DefaultEvent("InfoMessage")]
    public sealed class SqlConnection : DbConnection, ICloneable
    {
        private bool _AsycCommandInProgress;
        private int _closeCount;
        private bool _collectstats;
        private static readonly DbConnectionFactory _connectionFactory = SqlConnectionFactory.SingletonInstance;
        private bool _fireInfoMessageEventOnUserErrors;
        private DbConnectionInternal _innerConnection;
        private static int _objectTypeCount;
        private DbConnectionPoolGroup _poolGroup;
        private SqlDebugContext _sdc;
        internal SqlStatistics _statistics;
        private DbConnectionOptions _userConnectionOptions;
        private static readonly object EventInfoMessage = new object();
        internal static readonly CodeAccessPermission ExecutePermission = CreateExecutePermission();
        internal readonly int ObjectID;

        [ResDescription("DbConnection_InfoMessage"), ResCategory("DataCategory_InfoMessage")]
        public event SqlInfoMessageEventHandler InfoMessage
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

        public SqlConnection()
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            this._innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
        }

        private SqlConnection(SqlConnection connection)
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
            this.CopyFrom(connection);
        }

        public SqlConnection(string connectionString) : this()
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

        internal void AddPreparedCommand(SqlCommand cmd)
        {
            this.GetOpenConnection().AddPreparedCommand(cmd);
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

        public SqlTransaction BeginTransaction()
        {
            return this.BeginTransaction(System.Data.IsolationLevel.Unspecified, null);
        }

        public SqlTransaction BeginTransaction(System.Data.IsolationLevel iso)
        {
            return this.BeginTransaction(iso, null);
        }

        public SqlTransaction BeginTransaction(string transactionName)
        {
            return this.BeginTransaction(System.Data.IsolationLevel.Unspecified, transactionName);
        }

        public SqlTransaction BeginTransaction(System.Data.IsolationLevel iso, string transactionName)
        {
            SqlStatistics statistics = null;
            SqlTransaction transaction;
            IntPtr ptr;
            string str = ADP.IsEmpty(transactionName) ? "None" : transactionName;
            Bid.ScopeEnter(out ptr, "<sc.SqlConnection.BeginTransaction|API> %d#, iso=%d{ds.IsolationLevel}, transactionName='%ls'\n", this.ObjectID, (int) iso, str);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                SqlTransaction transaction2 = this.GetOpenConnection().BeginSqlTransaction(iso, transactionName);
                GC.KeepAlive(this);
                transaction = transaction2;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
                SqlStatistics.StopTimer(statistics);
            }
            return transaction;
        }

        public override void ChangeDatabase(string database)
        {
            SNIHandle target = null;
            SqlStatistics statistics = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this);
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this.InnerConnection.ChangeDatabase(database);
            }
            catch (OutOfMemoryException exception3)
            {
                this.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
        }

        public static void ChangePassword(string connectionString, string newPassword)
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlConnection.ChangePassword|API>");
            try
            {
                if (ADP.IsEmpty(connectionString))
                {
                    throw SQL.ChangePasswordArgumentMissing("connectionString");
                }
                if (ADP.IsEmpty(newPassword))
                {
                    throw SQL.ChangePasswordArgumentMissing("newPassword");
                }
                if (0x80 < newPassword.Length)
                {
                    throw ADP.InvalidArgumentLength("newPassword", 0x80);
                }
                SqlConnectionString connectionOptions = SqlConnectionFactory.FindSqlConnectionOptions(connectionString);
                if (connectionOptions.IntegratedSecurity)
                {
                    throw SQL.ChangePasswordConflictsWithSSPI();
                }
                if (!ADP.IsEmpty(connectionOptions.AttachDBFilename))
                {
                    throw SQL.ChangePasswordUseOfUnallowedKey("attachdbfilename");
                }
                if (connectionOptions.ContextConnection)
                {
                    throw SQL.ChangePasswordUseOfUnallowedKey("context connection");
                }
                connectionOptions.CreatePermissionSet().Demand();
                using (SqlInternalConnectionTds tds = new SqlInternalConnectionTds(null, connectionOptions, null, newPassword, null, false))
                {
                    if (!tds.IsYukonOrNewer)
                    {
                        throw SQL.ChangePasswordRequiresYukon();
                    }
                }
                SqlConnectionFactory.SingletonInstance.ClearPool(connectionString);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal static void CheckGetExtendedUDTInfo(SqlMetaDataPriv metaData, bool fThrow)
        {
            if (metaData.udtType == null)
            {
                metaData.udtType = Type.GetType(metaData.udtAssemblyQualifiedName, fThrow);
                if (fThrow && (metaData.udtType == null))
                {
                    throw SQL.UDTUnexpectedResult(metaData.udtAssemblyQualifiedName);
                }
            }
        }

        internal void CheckSQLDebug()
        {
            if (this._sdc != null)
            {
                this.CheckSQLDebug(this._sdc);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private void CheckSQLDebug(SqlDebugContext sdc)
        {
            uint currentThreadId = (uint) AppDomain.GetCurrentThreadId();
            RefreshMemoryMappedData(sdc);
            if (!sdc.active && sdc.fOption)
            {
                sdc.active = true;
                sdc.tid = currentThreadId;
                try
                {
                    this.IssueSQLDebug(1, sdc.machineName, sdc.pid, sdc.dbgpid, sdc.sdiDllName, sdc.data);
                    sdc.tid = 0;
                }
                catch
                {
                    sdc.active = false;
                    throw;
                }
            }
            if (sdc.active)
            {
                if (!sdc.fOption)
                {
                    sdc.Dispose();
                    this.IssueSQLDebug(0, null, 0, 0, null, null);
                }
                else if (sdc.tid != currentThreadId)
                {
                    sdc.tid = currentThreadId;
                    try
                    {
                        this.IssueSQLDebug(2, null, sdc.pid, sdc.tid, null, null);
                    }
                    catch
                    {
                        sdc.tid = 0;
                        throw;
                    }
                }
            }
        }

        private void CheckSQLDebugOnConnect()
        {
            string str;
            uint currentProcessId = (uint) SafeNativeMethods.GetCurrentProcessId();
            if (ADP.IsPlatformNT5)
            {
                str = @"Global\SqlClientSSDebug";
            }
            else
            {
                str = "SqlClientSSDebug";
            }
            str = str + currentProcessId.ToString(CultureInfo.InvariantCulture);
            IntPtr hFileMappingObject = System.Data.Common.NativeMethods.OpenFileMappingA(4, false, str);
            if (ADP.PtrZero != hFileMappingObject)
            {
                IntPtr ptr2 = System.Data.Common.NativeMethods.MapViewOfFile(hFileMappingObject, 4, 0, 0, IntPtr.Zero);
                if (ADP.PtrZero != ptr2)
                {
                    SqlDebugContext sdc = new SqlDebugContext {
                        hMemMap = hFileMappingObject,
                        pMemMap = ptr2,
                        pid = currentProcessId
                    };
                    this.CheckSQLDebug(sdc);
                    this._sdc = sdc;
                }
            }
        }

        public static void ClearAllPools()
        {
            new SqlClientPermission(PermissionState.Unrestricted).Demand();
            SqlConnectionFactory.SingletonInstance.ClearAllPools();
        }

        public static void ClearPool(SqlConnection connection)
        {
            ADP.CheckArgumentNull(connection, "connection");
            DbConnectionOptions userConnectionOptions = connection.UserConnectionOptions;
            if (userConnectionOptions != null)
            {
                userConnectionOptions.DemandPermission();
                if (connection.IsContextConnection)
                {
                    throw SQL.NotAvailableOnContextConnection();
                }
                SqlConnectionFactory.SingletonInstance.ClearPool(connection);
            }
        }

        public override void Close()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlConnection.Close|API> %d#", this.ObjectID);
            try
            {
                SqlStatistics statistics = null;
                SNIHandle target = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    target = SqlInternalConnection.GetBestEffortCleanupTarget(this);
                    statistics = SqlStatistics.StartTimer(this.Statistics);
                    lock (this.InnerConnection)
                    {
                        this.InnerConnection.CloseConnection(this, this.ConnectionFactory);
                    }
                    if (this.Statistics != null)
                    {
                        ADP.TimerCurrent(out this._statistics._closeTimestamp);
                    }
                }
                catch (OutOfMemoryException exception3)
                {
                    this.Abort(exception3);
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this.Abort(exception2);
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this.Abort(exception);
                    SqlInternalConnection.BestEffortCleanup(target);
                    throw;
                }
                finally
                {
                    SqlStatistics.StopTimer(statistics);
                }
            }
            finally
            {
                SqlDebugContext context = this._sdc;
                this._sdc = null;
                Bid.ScopeLeave(ref ptr);
                if (context != null)
                {
                    context.Dispose();
                }
            }
        }

        private void CompleteOpen()
        {
            if (!this.GetOpenConnection().IsYukonOrNewer && Debugger.IsAttached)
            {
                bool flag = false;
                try
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                    flag = true;
                }
                catch (SecurityException exception)
                {
                    ADP.TraceExceptionWithoutRethrow(exception);
                }
                if (flag)
                {
                    this.CheckSQLDebugOnConnect();
                }
            }
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

        private void CopyFrom(SqlConnection connection)
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

        public SqlCommand CreateCommand()
        {
            return new SqlCommand(null, this);
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
            DBDataPermission permission = (DBDataPermission) SqlConnectionFactory.SingletonInstance.ProviderFactory.CreatePermission(PermissionState.None);
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
        }

        public void EnlistDistributedTransaction(ITransaction transaction)
        {
            if (this.IsContextConnection)
            {
                throw SQL.NotAvailableOnContextConnection();
            }
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

        internal static string FixupDatabaseTransactionName(string name)
        {
            if (!ADP.IsEmpty(name))
            {
                return SqlServerEscapeHelper.EscapeIdentifier(name);
            }
            return name;
        }

        internal byte[] GetBytes(object o)
        {
            Format native = Format.Native;
            int maxSize = 0;
            return this.GetBytes(o, out native, out maxSize);
        }

        internal byte[] GetBytes(object o, out Format format, out int maxSize)
        {
            SqlUdtInfo infoFromType = AssemblyCache.GetInfoFromType(o.GetType());
            maxSize = infoFromType.MaxByteSize;
            format = infoFromType.SerializationFormat;
            if ((maxSize < -1) || (maxSize >= 0xffff))
            {
                throw new InvalidOperationException(o.GetType() + ": invalid Size");
            }
            using (MemoryStream stream = new MemoryStream((maxSize < 0) ? 0 : maxSize))
            {
                SerializationHelperSql9.Serialize(stream, o);
                return stream.ToArray();
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

        internal SqlInternalConnection GetOpenConnection()
        {
            SqlInternalConnection innerConnection = this.InnerConnection as SqlInternalConnection;
            if (innerConnection == null)
            {
                throw ADP.ClosedConnectionError();
            }
            return innerConnection;
        }

        internal SqlInternalConnection GetOpenConnection(string method)
        {
            DbConnectionInternal innerConnection = this.InnerConnection;
            SqlInternalConnection connection = innerConnection as SqlInternalConnection;
            if (connection == null)
            {
                throw ADP.OpenConnectionRequired(method, innerConnection.State);
            }
            return connection;
        }

        internal SqlInternalConnectionTds GetOpenTdsConnection()
        {
            SqlInternalConnectionTds innerConnection = this.InnerConnection as SqlInternalConnectionTds;
            if (innerConnection == null)
            {
                throw ADP.ClosedConnectionError();
            }
            return innerConnection;
        }

        internal SqlInternalConnectionTds GetOpenTdsConnection(string method)
        {
            SqlInternalConnectionTds innerConnection = this.InnerConnection as SqlInternalConnectionTds;
            if (innerConnection == null)
            {
                throw ADP.OpenConnectionRequired(method, innerConnection.State);
            }
            return innerConnection;
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

        internal object GetUdtValue(object value, SqlMetaDataPriv metaData, bool returnDBNull)
        {
            if (returnDBNull && ADP.IsNull(value))
            {
                return DBNull.Value;
            }
            if (ADP.IsNull(value))
            {
                return metaData.udtType.InvokeMember("Null", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static, null, null, new object[0], CultureInfo.InvariantCulture);
            }
            MemoryStream s = new MemoryStream((byte[]) value);
            return SerializationHelperSql9.Deserialize(s, metaData.udtType);
        }

        private void IssueSQLDebug(uint option, string machineName, uint pid, uint id, string sdiDllName, byte[] data)
        {
            if (!this.GetOpenConnection().IsYukonOrNewer)
            {
                SqlCommand command = new SqlCommand("sp_sdidebug", this) {
                    CommandType = CommandType.StoredProcedure
                };
                SqlParameter parameter = new SqlParameter(null, SqlDbType.VarChar, TdsEnums.SQLDEBUG_MODE_NAMES[option].Length) {
                    Value = TdsEnums.SQLDEBUG_MODE_NAMES[option]
                };
                command.Parameters.Add(parameter);
                if (option == 1)
                {
                    parameter = new SqlParameter(null, SqlDbType.VarChar, sdiDllName.Length) {
                        Value = sdiDllName
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter(null, SqlDbType.VarChar, machineName.Length) {
                        Value = machineName
                    };
                    command.Parameters.Add(parameter);
                }
                if (option != 0)
                {
                    parameter = new SqlParameter(null, SqlDbType.Int) {
                        Value = pid
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter(null, SqlDbType.Int) {
                        Value = id
                    };
                    command.Parameters.Add(parameter);
                }
                if (option == 1)
                {
                    parameter = new SqlParameter(null, SqlDbType.VarBinary, (data != null) ? data.Length : 0) {
                        Value = data
                    };
                    command.Parameters.Add(parameter);
                }
                command.ExecuteNonQuery();
            }
        }

        internal void NotifyWeakReference(int message)
        {
            this.InnerConnection.NotifyWeakReference(message);
        }

        internal void OnError(SqlException exception, bool breakConnection)
        {
            if (breakConnection && (ConnectionState.Open == this.State))
            {
                Bid.Trace("<sc.SqlConnection.OnError|INFO> %d#, Connection broken.\n", this.ObjectID);
                this.Close();
            }
            if (exception.Class >= 11)
            {
                throw exception;
            }
            this.OnInfoMessage(new SqlInfoMessageEventArgs(exception));
        }

        internal void OnInfoMessage(SqlInfoMessageEventArgs imevent)
        {
            if (Bid.TraceOn)
            {
                Bid.Trace("<sc.SqlConnection.OnInfoMessage|API|INFO> %d#, Message='%ls'\n", this.ObjectID, (imevent != null) ? imevent.Message : "");
            }
            SqlInfoMessageEventHandler handler = (SqlInfoMessageEventHandler) base.Events[EventInfoMessage];
            if (handler != null)
            {
                try
                {
                    handler(this, imevent);
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
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlConnection.Open|API> %d#", this.ObjectID);
            try
            {
                if (this.StatisticsEnabled)
                {
                    if (this._statistics == null)
                    {
                        this._statistics = new SqlStatistics();
                    }
                    else
                    {
                        this._statistics.ContinueOnNewConnection();
                    }
                }
                SNIHandle target = null;
                SqlStatistics statistics = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    statistics = SqlStatistics.StartTimer(this.Statistics);
                    this.InnerConnection.OpenConnection(this, this.ConnectionFactory);
                    target = SqlInternalConnection.GetBestEffortCleanupTarget(this);
                    SqlInternalConnectionSmi innerConnection = this.InnerConnection as SqlInternalConnectionSmi;
                    if (innerConnection != null)
                    {
                        innerConnection.AutomaticEnlistment();
                    }
                    else
                    {
                        if (this.StatisticsEnabled)
                        {
                            ADP.TimerCurrent(out this._statistics._openTimestamp);
                            this.Parser.Statistics = this._statistics;
                        }
                        else
                        {
                            this.Parser.Statistics = null;
                            this._statistics = null;
                        }
                        this.CompleteOpen();
                    }
                }
                catch (OutOfMemoryException exception3)
                {
                    this.Abort(exception3);
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this.Abort(exception2);
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this.Abort(exception);
                    SqlInternalConnection.BestEffortCleanup(target);
                    throw;
                }
                finally
                {
                    SqlStatistics.StopTimer(statistics);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
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

        private static void RefreshMemoryMappedData(SqlDebugContext sdc)
        {
            MEMMAP memmap = (MEMMAP) Marshal.PtrToStructure(sdc.pMemMap, typeof(MEMMAP));
            sdc.dbgpid = memmap.dbgpid;
            sdc.fOption = memmap.fOption == 1;
            Encoding encoding = Encoding.GetEncoding(0x4e4);
            sdc.machineName = encoding.GetString(memmap.rgbMachineName, 0, memmap.rgbMachineName.Length);
            sdc.sdiDllName = encoding.GetString(memmap.rgbDllName, 0, memmap.rgbDllName.Length);
            sdc.data = memmap.rgbData;
        }

        internal void RemovePreparedCommand(SqlCommand cmd)
        {
            this.GetOpenConnection().RemovePreparedCommand(cmd);
        }

        internal void RemoveWeakReference(object value)
        {
            this.InnerConnection.RemoveWeakReference(value);
        }

        public void ResetStatistics()
        {
            if (this.IsContextConnection)
            {
                throw SQL.NotAvailableOnContextConnection();
            }
            if (this.Statistics != null)
            {
                this.Statistics.Reset();
                if (ConnectionState.Open == this.State)
                {
                    ADP.TimerCurrent(out this._statistics._openTimestamp);
                }
            }
        }

        public IDictionary RetrieveStatistics()
        {
            if (this.IsContextConnection)
            {
                throw SQL.NotAvailableOnContextConnection();
            }
            if (this.Statistics != null)
            {
                this.UpdateStatistics();
                return this.Statistics.GetHashtable();
            }
            return new SqlStatistics().GetHashtable();
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

        object ICloneable.Clone()
        {
            SqlConnection connection = new SqlConnection(this);
            Bid.Trace("<sc.SqlConnection.Clone|API> %d#, clone=%d#\n", this.ObjectID, connection.ObjectID);
            return connection;
        }

        private void UpdateStatistics()
        {
            if (ConnectionState.Open == this.State)
            {
                ADP.TimerCurrent(out this._statistics._closeTimestamp);
            }
            this.Statistics.UpdateStatistics();
        }

        internal void ValidateConnectionForExecute(string method, SqlCommand command)
        {
            this.GetOpenConnection(method).ValidateConnectionForExecute(command);
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

        internal bool AsycCommandInProgress
        {
            get
            {
                return this._AsycCommandInProgress;
            }
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            set
            {
                this._AsycCommandInProgress = value;
            }
        }

        internal bool Asynchronous
        {
            get
            {
                SqlConnectionString connectionOptions = (SqlConnectionString) this.ConnectionOptions;
                if (connectionOptions == null)
                {
                    return false;
                }
                return connectionOptions.Asynchronous;
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

        [DefaultValue(""), RecommendedAsConfigurable(true), SettingsBindable(true), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Data"), ResDescription("SqlConnection_ConnectionString"), Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlConnectionStringEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
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

        [ResDescription("SqlConnection_ConnectionTimeout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int ConnectionTimeout
        {
            get
            {
                SqlConnectionString connectionOptions = (SqlConnectionString) this.ConnectionOptions;
                if (connectionOptions == null)
                {
                    return 15;
                }
                return connectionOptions.ConnectTimeout;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("SqlConnection_Database")]
        public override string Database
        {
            get
            {
                SqlInternalConnection innerConnection = this.InnerConnection as SqlInternalConnection;
                if (innerConnection != null)
                {
                    return innerConnection.CurrentDatabase;
                }
                SqlConnectionString connectionOptions = (SqlConnectionString) this.ConnectionOptions;
                return ((connectionOptions != null) ? connectionOptions.InitialCatalog : "");
            }
        }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("SqlConnection_DataSource")]
        public override string DataSource
        {
            get
            {
                SqlInternalConnection innerConnection = this.InnerConnection as SqlInternalConnection;
                if (innerConnection != null)
                {
                    return innerConnection.CurrentDataSource;
                }
                SqlConnectionString connectionOptions = (SqlConnectionString) this.ConnectionOptions;
                return ((connectionOptions != null) ? connectionOptions.DataSource : "");
            }
        }

        protected override System.Data.Common.DbProviderFactory DbProviderFactory
        {
            get
            {
                return SqlClientFactory.Instance;
            }
        }

        public bool FireInfoMessageEventOnUserErrors
        {
            get
            {
                return this._fireInfoMessageEventOnUserErrors;
            }
            set
            {
                this._fireInfoMessageEventOnUserErrors = value;
            }
        }

        internal bool HasLocalTransaction
        {
            get
            {
                return this.GetOpenConnection().HasLocalTransaction;
            }
        }

        internal bool HasLocalTransactionFromAPI
        {
            get
            {
                return this.GetOpenConnection().HasLocalTransactionFromAPI;
            }
        }

        internal DbConnectionInternal InnerConnection
        {
            get
            {
                return this._innerConnection;
            }
        }

        internal bool IsContextConnection
        {
            get
            {
                SqlConnectionString connectionOptions = (SqlConnectionString) this.ConnectionOptions;
                bool contextConnection = false;
                if (connectionOptions != null)
                {
                    contextConnection = connectionOptions.ContextConnection;
                }
                return contextConnection;
            }
        }

        internal bool IsKatmaiOrNewer
        {
            get
            {
                return this.GetOpenConnection().IsKatmaiOrNewer;
            }
        }

        internal bool IsShiloh
        {
            get
            {
                return this.GetOpenConnection().IsShiloh;
            }
        }

        internal bool IsYukonOrNewer
        {
            get
            {
                return this.GetOpenConnection().IsYukonOrNewer;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResCategory("DataCategory_Data"), ResDescription("SqlConnection_PacketSize")]
        public int PacketSize
        {
            get
            {
                if (this.IsContextConnection)
                {
                    throw SQL.NotAvailableOnContextConnection();
                }
                SqlInternalConnectionTds innerConnection = this.InnerConnection as SqlInternalConnectionTds;
                if (innerConnection != null)
                {
                    return innerConnection.PacketSize;
                }
                SqlConnectionString connectionOptions = (SqlConnectionString) this.ConnectionOptions;
                return ((connectionOptions != null) ? connectionOptions.PacketSize : 0x1f40);
            }
        }

        internal TdsParser Parser
        {
            get
            {
                SqlInternalConnectionTds openConnection = this.GetOpenConnection() as SqlInternalConnectionTds;
                if (openConnection == null)
                {
                    throw SQL.NotAvailableOnContextConnection();
                }
                return openConnection.Parser;
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), ResDescription("SqlConnection_ServerVersion")]
        public override string ServerVersion
        {
            get
            {
                return this.GetOpenConnection().ServerVersion;
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

        internal SqlStatistics Statistics
        {
            get
            {
                return this._statistics;
            }
        }

        [ResDescription("SqlConnection_StatisticsEnabled"), DefaultValue(false), ResCategory("DataCategory_Data")]
        public bool StatisticsEnabled
        {
            get
            {
                return this._collectstats;
            }
            set
            {
                if (this.IsContextConnection)
                {
                    if (value)
                    {
                        throw SQL.NotAvailableOnContextConnection();
                    }
                }
                else
                {
                    if (value)
                    {
                        if (ConnectionState.Open == this.State)
                        {
                            if (this._statistics == null)
                            {
                                this._statistics = new SqlStatistics();
                                ADP.TimerCurrent(out this._statistics._openTimestamp);
                            }
                            this.Parser.Statistics = this._statistics;
                        }
                    }
                    else if ((this._statistics != null) && (ConnectionState.Open == this.State))
                    {
                        this.Parser.Statistics = null;
                        ADP.TimerCurrent(out this._statistics._closeTimestamp);
                    }
                    this._collectstats = value;
                }
            }
        }

        internal SqlConnectionString.TransactionBindingEnum TransactionBinding
        {
            get
            {
                return ((SqlConnectionString) this.ConnectionOptions).TransactionBinding;
            }
        }

        internal System.Data.SqlClient.SqlConnectionString.TypeSystem TypeSystem
        {
            get
            {
                return ((SqlConnectionString) this.ConnectionOptions).TypeSystemVersion;
            }
        }

        internal DbConnectionOptions UserConnectionOptions
        {
            get
            {
                return this._userConnectionOptions;
            }
        }

        [ResCategory("DataCategory_Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("SqlConnection_WorkstationId")]
        public string WorkstationId
        {
            get
            {
                if (this.IsContextConnection)
                {
                    throw SQL.NotAvailableOnContextConnection();
                }
                SqlConnectionString connectionOptions = (SqlConnectionString) this.ConnectionOptions;
                string machineName = (connectionOptions != null) ? connectionOptions.WorkstationId : null;
                if (machineName == null)
                {
                    machineName = Environment.MachineName;
                }
                return machineName;
            }
        }
    }
}

