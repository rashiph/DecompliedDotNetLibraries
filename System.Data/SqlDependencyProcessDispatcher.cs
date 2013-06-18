using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Xml;

internal class SqlDependencyProcessDispatcher : MarshalByRefObject
{
    private Dictionary<SqlConnectionContainerHashHelper, SqlConnectionContainer> _connectionContainers;
    private readonly int _objectID;
    private static int _objectTypeCount;
    private Dictionary<string, SqlDependencyPerAppDomainDispatcher> _sqlDependencyPerAppDomainDispatchers;
    private static SqlDependencyProcessDispatcher _staticInstance = new SqlDependencyProcessDispatcher(null);

    public SqlDependencyProcessDispatcher()
    {
        IntPtr ptr;
        this._objectID = Interlocked.Increment(ref _objectTypeCount);
        Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyProcessDispatcher|DEP> %d#", this.ObjectID);
        try
        {
        }
        finally
        {
            Bid.ScopeLeave(ref ptr);
        }
    }

    private SqlDependencyProcessDispatcher(object dummyVariable)
    {
        IntPtr ptr;
        this._objectID = Interlocked.Increment(ref _objectTypeCount);
        Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyProcessDispatcher|DEP> %d#", this.ObjectID);
        try
        {
            this._connectionContainers = new Dictionary<SqlConnectionContainerHashHelper, SqlConnectionContainer>();
            this._sqlDependencyPerAppDomainDispatchers = new Dictionary<string, SqlDependencyPerAppDomainDispatcher>();
        }
        finally
        {
            Bid.ScopeLeave(ref ptr);
        }
    }

    private void AppDomainUnloading(object state)
    {
        IntPtr ptr;
        Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyProcessDispatcher.AppDomainUnloading|DEP> %d#", this.ObjectID);
        try
        {
            string appDomainKey = (string) state;
            lock (this._connectionContainers)
            {
                List<SqlConnectionContainerHashHelper> list = new List<SqlConnectionContainerHashHelper>();
                foreach (KeyValuePair<SqlConnectionContainerHashHelper, SqlConnectionContainer> pair in this._connectionContainers)
                {
                    SqlConnectionContainer container = pair.Value;
                    if (container.AppDomainUnload(appDomainKey))
                    {
                        list.Add(container.HashHelper);
                    }
                }
                foreach (SqlConnectionContainerHashHelper helper in list)
                {
                    this._connectionContainers.Remove(helper);
                }
            }
            lock (this._sqlDependencyPerAppDomainDispatchers)
            {
                this._sqlDependencyPerAppDomainDispatchers.Remove(appDomainKey);
            }
        }
        finally
        {
            Bid.ScopeLeave(ref ptr);
        }
    }

    private static SqlConnectionContainerHashHelper GetHashHelper(string connectionString, out SqlConnectionStringBuilder connectionStringBuilder, out DbConnectionPoolIdentity identity, out string user, string queue)
    {
        SqlConnectionContainerHashHelper helper;
        IntPtr ptr;
        Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyProcessDispatcher.GetHashString|DEP> %d#, queue: %ls", _staticInstance.ObjectID, queue);
        try
        {
            connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            connectionStringBuilder.AsynchronousProcessing = true;
            connectionStringBuilder.Pooling = false;
            connectionStringBuilder.Enlist = false;
            if (queue != null)
            {
                connectionStringBuilder.ApplicationName = queue;
            }
            if (connectionStringBuilder.IntegratedSecurity)
            {
                identity = DbConnectionPoolIdentity.GetCurrent();
                user = null;
            }
            else
            {
                identity = null;
                user = connectionStringBuilder.UserID;
            }
            helper = new SqlConnectionContainerHashHelper(identity, connectionStringBuilder.ConnectionString, queue, connectionStringBuilder);
        }
        finally
        {
            Bid.ScopeLeave(ref ptr);
        }
        return helper;
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }

    private void Invalidate(string server, SqlNotification sqlNotification)
    {
        IntPtr ptr;
        Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyProcessDispatcher.Invalidate|DEP> %d#, server: %ls", this.ObjectID, server);
        try
        {
            lock (this._sqlDependencyPerAppDomainDispatchers)
            {
                foreach (KeyValuePair<string, SqlDependencyPerAppDomainDispatcher> pair in this._sqlDependencyPerAppDomainDispatchers)
                {
                    SqlDependencyPerAppDomainDispatcher dispatcher = pair.Value;
                    try
                    {
                        dispatcher.InvalidateServer(server, sqlNotification);
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        ADP.TraceExceptionWithoutRethrow(exception);
                    }
                }
            }
        }
        finally
        {
            Bid.ScopeLeave(ref ptr);
        }
    }

    internal void QueueAppDomainUnloading(string appDomainKey)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.AppDomainUnloading), appDomainKey);
    }

    internal bool Start(string connectionString, string queue, string appDomainKey, SqlDependencyPerAppDomainDispatcher dispatcher)
    {
        string server = null;
        bool errorOccurred = false;
        DbConnectionPoolIdentity identity = null;
        return this.Start(connectionString, out server, out identity, out server, out server, ref queue, appDomainKey, dispatcher, out errorOccurred, out errorOccurred, false);
    }

    private bool Start(string connectionString, out string server, out DbConnectionPoolIdentity identity, out string user, out string database, ref string queueService, string appDomainKey, SqlDependencyPerAppDomainDispatcher dispatcher, out bool errorOccurred, out bool appDomainStart, bool useDefaults)
    {
        bool flag2;
        IntPtr ptr;
        Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyProcessDispatcher.Start|DEP> %d#, queue: '%ls', appDomainKey: '%ls', perAppDomainDispatcher ID: '%d'", this.ObjectID, queueService, appDomainKey, dispatcher.ObjectID);
        try
        {
            server = null;
            identity = null;
            user = null;
            database = null;
            errorOccurred = false;
            appDomainStart = false;
            lock (this._sqlDependencyPerAppDomainDispatchers)
            {
                if (!this._sqlDependencyPerAppDomainDispatchers.ContainsKey(appDomainKey))
                {
                    this._sqlDependencyPerAppDomainDispatchers[appDomainKey] = dispatcher;
                }
            }
            SqlConnectionStringBuilder connectionStringBuilder = null;
            SqlConnectionContainerHashHelper key = GetHashHelper(connectionString, out connectionStringBuilder, out identity, out user, queueService);
            bool flag = false;
            SqlConnectionContainer container = null;
            lock (this._connectionContainers)
            {
                if (!this._connectionContainers.ContainsKey(key))
                {
                    Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Start|DEP> %d#, hashtable miss, creating new container.\n", this.ObjectID);
                    container = new SqlConnectionContainer(key, appDomainKey, useDefaults);
                    this._connectionContainers.Add(key, container);
                    flag = true;
                    appDomainStart = true;
                }
                else
                {
                    container = this._connectionContainers[key];
                    Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Start|DEP> %d#, hashtable hit, container: %d\n", this.ObjectID, container.ObjectID);
                    if (container.InErrorState)
                    {
                        Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Start|DEP> %d#, container: %d is in error state!\n", this.ObjectID, container.ObjectID);
                        errorOccurred = true;
                    }
                    else
                    {
                        container.IncrementStartCount(appDomainKey, out appDomainStart);
                    }
                }
            }
            if (useDefaults && !errorOccurred)
            {
                server = container.Server;
                database = container.Database;
                queueService = container.Queue;
                Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Start|DEP> %d#, default service: '%ls', server: '%ls', database: '%ls'\n", this.ObjectID, queueService, server, database);
            }
            Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Start|DEP> %d#, started: %d\n", this.ObjectID, flag);
            flag2 = flag;
        }
        finally
        {
            Bid.ScopeLeave(ref ptr);
        }
        return flag2;
    }

    internal bool StartWithDefault(string connectionString, out string server, out DbConnectionPoolIdentity identity, out string user, out string database, ref string service, string appDomainKey, SqlDependencyPerAppDomainDispatcher dispatcher, out bool errorOccurred, out bool appDomainStart)
    {
        return this.Start(connectionString, out server, out identity, out user, out database, ref service, appDomainKey, dispatcher, out errorOccurred, out appDomainStart, true);
    }

    internal bool Stop(string connectionString, out string server, out DbConnectionPoolIdentity identity, out string user, out string database, ref string queueService, string appDomainKey, out bool appDomainStop)
    {
        bool flag2;
        IntPtr ptr;
        Bid.NotificationsScopeEnter(out ptr, "<sc.SqlDependencyProcessDispatcher.Stop|DEP> %d#, queue: '%ls'", this.ObjectID, queueService);
        try
        {
            server = null;
            identity = null;
            user = null;
            database = null;
            appDomainStop = false;
            SqlConnectionStringBuilder connectionStringBuilder = null;
            SqlConnectionContainerHashHelper key = GetHashHelper(connectionString, out connectionStringBuilder, out identity, out user, queueService);
            bool flag = false;
            lock (this._connectionContainers)
            {
                if (this._connectionContainers.ContainsKey(key))
                {
                    SqlConnectionContainer container = this._connectionContainers[key];
                    Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Stop|DEP> %d#, hashtable hit, container: %d\n", this.ObjectID, container.ObjectID);
                    server = container.Server;
                    database = container.Database;
                    queueService = container.Queue;
                    if (container.Stop(appDomainKey, out appDomainStop))
                    {
                        flag = true;
                        this._connectionContainers.Remove(key);
                    }
                }
                else
                {
                    Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Stop|DEP> %d#, hashtable miss.\n", this.ObjectID);
                }
            }
            Bid.NotificationsTrace("<sc.SqlDependencyProcessDispatcher.Stop|DEP> %d#, stopped: %d\n", this.ObjectID, flag);
            flag2 = flag;
        }
        finally
        {
            Bid.ScopeLeave(ref ptr);
        }
        return flag2;
    }

    internal int ObjectID
    {
        get
        {
            return this._objectID;
        }
    }

    internal SqlDependencyProcessDispatcher SingletonProcessDispatcher
    {
        get
        {
            return _staticInstance;
        }
    }

    private class SqlConnectionContainer
    {
        private Dictionary<string, int> _appDomainKeyHash;
        private string _beginConversationQuery;
        private string _cachedDatabase;
        private string _cachedServer;
        private SqlCommand _com;
        private SqlConnection _con;
        private string _concatQuery;
        private SqlParameter _conversationGuidParam;
        private readonly int _defaultWaitforTimeout;
        private string _dialogHandle;
        private string _endConversationQuery;
        private volatile bool _errorState;
        private string _escapedQueueName;
        private SqlDependencyProcessDispatcher.SqlConnectionContainerHashHelper _hashHelper;
        private readonly int _objectID;
        private static int _objectTypeCount;
        private string _queue;
        private string _receiveQuery;
        private Timer _retryTimer;
        private volatile bool _serviceQueueCreated;
        private string _sprocName;
        private int _startCount;
        private volatile bool _stop;
        private volatile bool _stopped;
        private SqlParameter _timeoutParam;
        private WindowsIdentity _windowsIdentity;

        internal SqlConnectionContainer(SqlDependencyProcessDispatcher.SqlConnectionContainerHashHelper hashHelper, string appDomainKey, bool useDefaults)
        {
            IntPtr ptr;
            this._defaultWaitforTimeout = 0xea60;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer|DEP> %d#, queue: '%ls'", this.ObjectID, hashHelper.Queue);
            bool flag = false;
            try
            {
                this._hashHelper = hashHelper;
                string str = null;
                if (useDefaults)
                {
                    str = Guid.NewGuid().ToString();
                    this._queue = "SqlQueryNotificationService-" + str;
                    this._hashHelper.ConnectionStringBuilder.ApplicationName = this._queue;
                }
                else
                {
                    this._queue = this._hashHelper.Queue;
                }
                this._con = new SqlConnection(this._hashHelper.ConnectionStringBuilder.ConnectionString);
                ((SqlConnectionString) this._con.ConnectionOptions).CreatePermissionSet().Assert();
                this._con.Open();
                this._cachedServer = this._con.DataSource;
                if (!this._con.IsYukonOrNewer)
                {
                    throw SQL.NotificationsRequireYukon();
                }
                if (hashHelper.Identity != null)
                {
                    this._windowsIdentity = DbConnectionPoolIdentity.GetCurrentWindowsIdentity();
                }
                this._escapedQueueName = SqlConnection.FixupDatabaseTransactionName(this._queue);
                this._appDomainKeyHash = new Dictionary<string, int>();
                this._com = new SqlCommand();
                this._com.Connection = this._con;
                this._com.CommandText = "select is_broker_enabled from sys.databases where database_id=db_id()";
                if (!((bool) this._com.ExecuteScalar()))
                {
                    throw SQL.SqlDependencyDatabaseBrokerDisabled();
                }
                this._conversationGuidParam = new SqlParameter("@p1", SqlDbType.UniqueIdentifier);
                this._timeoutParam = new SqlParameter("@p2", SqlDbType.Int);
                this._timeoutParam.Value = 0;
                this._com.Parameters.Add(this._timeoutParam);
                flag = true;
                this._receiveQuery = "WAITFOR(RECEIVE TOP (1) message_type_name, conversation_handle, cast(message_body AS XML) as message_body from " + this._escapedQueueName + "), TIMEOUT @p2;";
                if (useDefaults)
                {
                    this._sprocName = SqlConnection.FixupDatabaseTransactionName("SqlQueryNotificationStoredProcedure-" + str);
                    this.CreateQueueAndService(false);
                }
                else
                {
                    this._com.CommandText = this._receiveQuery;
                    this._endConversationQuery = "END CONVERSATION @p1; ";
                    this._concatQuery = this._endConversationQuery + this._receiveQuery;
                }
                bool appDomainStart = false;
                this.IncrementStartCount(appDomainKey, out appDomainStart);
                this.SynchronouslyQueryServiceBrokerQueue();
                this._timeoutParam.Value = this._defaultWaitforTimeout;
                this.AsynchronouslyQueryServiceBrokerQueue();
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                ADP.TraceExceptionWithoutRethrow(exception);
                if (flag)
                {
                    this.TearDownAndDispose();
                }
                else
                {
                    if (this._com != null)
                    {
                        this._com.Dispose();
                        this._com = null;
                    }
                    if (this._con != null)
                    {
                        this._con.Dispose();
                        this._con = null;
                    }
                }
                throw;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal bool AppDomainUnload(string appDomainKey)
        {
            bool flag;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.AppDomainUnload|DEP> %d#, AppDomainKey: '%ls'", this.ObjectID, appDomainKey);
            try
            {
                lock (this._appDomainKeyHash)
                {
                    if (this._appDomainKeyHash.ContainsKey(appDomainKey))
                    {
                        Bid.NotificationsTrace("<sc.SqlConnectionContainer.AppDomainUnload|DEP> _appDomainKeyHash contained AppDomainKey: '%ls'.\n", appDomainKey);
                        int num = this._appDomainKeyHash[appDomainKey];
                        Bid.NotificationsTrace("<sc.SqlConnectionContainer.AppDomainUnload|DEP> _appDomainKeyHash for AppDomainKey: '%ls' count: '%d'.\n", appDomainKey, num);
                        bool appDomainStop = false;
                        while (num > 0)
                        {
                            this.Stop(appDomainKey, out appDomainStop);
                            num--;
                        }
                        if (this._appDomainKeyHash.ContainsKey(appDomainKey))
                        {
                            Bid.NotificationsTrace("<sc.SqlConnectionContainer.AppDomainUnload|DEP|ERR> ERROR - after the Stop() loop, _appDomainKeyHash for AppDomainKey: '%ls' entry not removed from hash.  Count: %d'\n", appDomainKey, this._appDomainKeyHash[appDomainKey]);
                        }
                    }
                    else
                    {
                        Bid.NotificationsTrace("<sc.SqlConnectionContainer.AppDomainUnload|DEP> _appDomainKeyHash did not contain AppDomainKey: '%ls'.\n", appDomainKey);
                    }
                }
                Bid.NotificationsTrace("<sc.SqlConnectionContainer.AppDomainUnload|DEP> Exiting, _stopped: '%d'.\n", this._stopped);
                flag = this._stopped;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        private void AsynchronouslyQueryServiceBrokerQueue()
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.AsynchronouslyQueryServiceBrokerQueue|DEP> %d#", this.ObjectID);
            try
            {
                AsyncCallback callback = new AsyncCallback(this.AsyncResultCallback);
                this._com.BeginExecuteReader(callback, null);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void AsyncResultCallback(IAsyncResult asyncResult)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.AsyncResultCallback|DEP> %d#", this.ObjectID);
            try
            {
                using (SqlDataReader reader = this._com.EndExecuteReader(asyncResult))
                {
                    this.ProcessNotificationResults(reader);
                }
                if (!this._stop)
                {
                    this.AsynchronouslyQueryServiceBrokerQueue();
                }
                else
                {
                    this.TearDownAndDispose();
                }
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                Bid.NotificationsTrace("<sc.SqlConnectionContainer.AsyncResultCallback|DEP> Exception occurred.\n");
                if (!this._stop)
                {
                    ADP.TraceExceptionWithoutRethrow(exception);
                }
                if (this._stop)
                {
                    this.TearDownAndDispose();
                }
                else
                {
                    this._errorState = true;
                    this.Restart(null);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void CreateQueueAndService(bool restart)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.CreateQueueAndService|DEP> %d#", this.ObjectID);
            try
            {
                SqlCommand command = new SqlCommand {
                    Connection = this._con
                };
                SqlTransaction transaction = null;
                try
                {
                    transaction = this._con.BeginTransaction();
                    command.Transaction = transaction;
                    string str = SqlServerEscapeHelper.MakeStringLiteral(this._queue);
                    command.CommandText = "CREATE PROCEDURE " + this._sprocName + " AS BEGIN BEGIN TRANSACTION; RECEIVE TOP(0) conversation_handle FROM " + this._escapedQueueName + "; IF (SELECT COUNT(*) FROM " + this._escapedQueueName + " WHERE message_type_name = 'http://schemas.microsoft.com/SQL/ServiceBroker/DialogTimer') > 0 BEGIN if ((SELECT COUNT(*) FROM sys.services WHERE name = " + str + ") > 0)   DROP SERVICE " + this._escapedQueueName + "; if (OBJECT_ID(" + str + ", 'SQ') IS NOT NULL)   DROP QUEUE " + this._escapedQueueName + "; DROP PROCEDURE " + this._sprocName + "; END COMMIT TRANSACTION; END";
                    if (!restart)
                    {
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception exception3)
                        {
                            if (!ADP.IsCatchableExceptionType(exception3))
                            {
                                throw;
                            }
                            ADP.TraceExceptionWithoutRethrow(exception3);
                            try
                            {
                                if (transaction != null)
                                {
                                    transaction.Rollback();
                                    transaction = null;
                                }
                            }
                            catch (Exception exception2)
                            {
                                if (!ADP.IsCatchableExceptionType(exception2))
                                {
                                    throw;
                                }
                                ADP.TraceExceptionWithoutRethrow(exception2);
                            }
                        }
                        if (transaction == null)
                        {
                            transaction = this._con.BeginTransaction();
                            command.Transaction = transaction;
                        }
                    }
                    command.CommandText = "IF OBJECT_ID(" + str + ", 'SQ') IS NULL BEGIN CREATE QUEUE " + this._escapedQueueName + " WITH ACTIVATION (PROCEDURE_NAME=" + this._sprocName + ", MAX_QUEUE_READERS=1, EXECUTE AS OWNER); END; IF (SELECT COUNT(*) FROM sys.services WHERE NAME=" + str + ") = 0 BEGIN CREATE SERVICE " + this._escapedQueueName + " ON QUEUE " + this._escapedQueueName + " ([http://schemas.microsoft.com/SQL/Notifications/PostQueryNotification]); IF (SELECT COUNT(*) FROM sys.database_principals WHERE name='sql_dependency_subscriber' AND type='R') <> 0 BEGIN GRANT SEND ON SERVICE::" + this._escapedQueueName + " TO sql_dependency_subscriber; END;  END; BEGIN DIALOG @dialog_handle FROM SERVICE " + this._escapedQueueName + " TO SERVICE " + str;
                    SqlParameter parameter = new SqlParameter {
                        ParameterName = "@dialog_handle",
                        DbType = DbType.Guid,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                    this._dialogHandle = ((Guid) parameter.Value).ToString();
                    this._beginConversationQuery = "BEGIN CONVERSATION TIMER ('" + this._dialogHandle + "') TIMEOUT = 120; " + this._receiveQuery;
                    this._com.CommandText = this._beginConversationQuery;
                    this._endConversationQuery = "END CONVERSATION @p1; ";
                    this._concatQuery = this._endConversationQuery + this._com.CommandText;
                    transaction.Commit();
                    transaction = null;
                    this._serviceQueueCreated = true;
                }
                finally
                {
                    if (transaction != null)
                    {
                        try
                        {
                            transaction.Rollback();
                            transaction = null;
                        }
                        catch (Exception exception)
                        {
                            if (!ADP.IsCatchableExceptionType(exception))
                            {
                                throw;
                            }
                            ADP.TraceExceptionWithoutRethrow(exception);
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void IncrementStartCount(string appDomainKey, out bool appDomainStart)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.IncrementStartCount|DEP> %d#", this.ObjectID);
            try
            {
                appDomainStart = false;
                int num = Interlocked.Increment(ref this._startCount);
                Bid.NotificationsTrace("<sc.SqlConnectionContainer.IncrementStartCount|DEP> %d#, incremented _startCount: %d\n", SqlDependencyProcessDispatcher._staticInstance.ObjectID, num);
                lock (this._appDomainKeyHash)
                {
                    if (this._appDomainKeyHash.ContainsKey(appDomainKey))
                    {
                        this._appDomainKeyHash[appDomainKey] += 1;
                        Bid.NotificationsTrace("<sc.SqlConnectionContainer.IncrementStartCount|DEP> _appDomainKeyHash contained AppDomainKey: '%ls', incremented count: '%d'.\n", appDomainKey, this._appDomainKeyHash[appDomainKey]);
                    }
                    else
                    {
                        this._appDomainKeyHash[appDomainKey] = 1;
                        appDomainStart = true;
                        Bid.NotificationsTrace("<sc.SqlConnectionContainer.IncrementStartCount|DEP> _appDomainKeyHash did not contain AppDomainKey: '%ls', added to hashtable and value set to 1.\n", appDomainKey);
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void ProcessNotificationResults(SqlDataReader reader)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.ProcessNotificationResults|DEP> %d#", this.ObjectID);
            try
            {
                Guid empty = Guid.Empty;
                try
                {
                    if (!this._stop)
                    {
                        while (reader.Read())
                        {
                            Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP> Row read.\n");
                            string str2 = reader.GetString(0);
                            Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP> msgType: '%ls'\n", str2);
                            empty = reader.GetGuid(1);
                            if (string.Compare(str2, "http://schemas.microsoft.com/SQL/Notifications/QueryNotification", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                SqlXml sqlXml = reader.GetSqlXml(2);
                                if (sqlXml != null)
                                {
                                    SqlNotification sqlNotification = SqlDependencyProcessDispatcher.SqlNotificationParser.ProcessMessage(sqlXml);
                                    if (sqlNotification != null)
                                    {
                                        string key = sqlNotification.Key;
                                        Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP> Key: '%ls'\n", key);
                                        int index = key.IndexOf(';');
                                        if (index >= 0)
                                        {
                                            SqlDependencyPerAppDomainDispatcher dispatcher;
                                            string str3 = key.Substring(0, index);
                                            lock (SqlDependencyProcessDispatcher._staticInstance._sqlDependencyPerAppDomainDispatchers)
                                            {
                                                dispatcher = SqlDependencyProcessDispatcher._staticInstance._sqlDependencyPerAppDomainDispatchers[str3];
                                            }
                                            if (dispatcher != null)
                                            {
                                                try
                                                {
                                                    dispatcher.InvalidateCommandID(sqlNotification);
                                                    continue;
                                                }
                                                catch (Exception exception)
                                                {
                                                    if (!ADP.IsCatchableExceptionType(exception))
                                                    {
                                                        throw;
                                                    }
                                                    ADP.TraceExceptionWithoutRethrow(exception);
                                                    continue;
                                                }
                                            }
                                            Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP|ERR> Received notification but do not have an associated PerAppDomainDispatcher!\n");
                                        }
                                        else
                                        {
                                            Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP|ERR> Unexpected ID format received!\n");
                                        }
                                    }
                                    else
                                    {
                                        Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP|ERR> Null notification returned from ProcessMessage!\n");
                                    }
                                }
                                else
                                {
                                    Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP|ERR> Null payload for QN notification type!\n");
                                }
                            }
                            else
                            {
                                empty = Guid.Empty;
                                Bid.NotificationsTrace("<sc.SqlConnectionContainer.ProcessNotificationResults|DEP> Unexpected message format received!\n");
                            }
                        }
                    }
                }
                finally
                {
                    if (empty == Guid.Empty)
                    {
                        this._com.CommandText = (this._beginConversationQuery != null) ? this._beginConversationQuery : this._receiveQuery;
                        if (this._com.Parameters.Count > 1)
                        {
                            this._com.Parameters.Remove(this._conversationGuidParam);
                        }
                    }
                    else
                    {
                        this._com.CommandText = this._concatQuery;
                        this._conversationGuidParam.Value = empty;
                        if (this._com.Parameters.Count == 1)
                        {
                            this._com.Parameters.Add(this._conversationGuidParam);
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void Restart(object unused)
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.Restart|DEP> %d#", this.ObjectID);
            try
            {
                SqlDependencyProcessDispatcher.SqlConnectionContainer container2;
                bool flag3;
                lock (this)
                {
                    if (!this._stop)
                    {
                        try
                        {
                            this._con.Close();
                        }
                        catch (Exception exception5)
                        {
                            if (!ADP.IsCatchableExceptionType(exception5))
                            {
                                throw;
                            }
                            ADP.TraceExceptionWithoutRethrow(exception5);
                        }
                    }
                }
                lock (this)
                {
                    if (!this._stop)
                    {
                        if (this._hashHelper.Identity != null)
                        {
                            WindowsImpersonationContext context = null;
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                                context = this._windowsIdentity.Impersonate();
                                this._con.Open();
                                goto Label_00C3;
                            }
                            finally
                            {
                                if (context != null)
                                {
                                    context.Undo();
                                }
                            }
                        }
                        this._con.Open();
                    }
                }
            Label_00C3:
                flag3 = false;
                try
                {
                    Monitor.Enter(container2 = this, ref flag3);
                    if (!this._stop && this._serviceQueueCreated)
                    {
                        bool flag = false;
                        try
                        {
                            this.CreateQueueAndService(true);
                        }
                        catch (Exception exception4)
                        {
                            if (!ADP.IsCatchableExceptionType(exception4))
                            {
                                throw;
                            }
                            ADP.TraceExceptionWithoutRethrow(exception4);
                            flag = true;
                        }
                        if (flag)
                        {
                            SqlDependencyProcessDispatcher._staticInstance.Invalidate(this.Server, new SqlNotification(SqlNotificationInfo.Error, SqlNotificationSource.Client, SqlNotificationType.Change, null));
                        }
                    }
                }
                finally
                {
                    if (flag3)
                    {
                        Monitor.Exit(container2);
                    }
                }
                lock (this)
                {
                    if (!this._stop)
                    {
                        this._timeoutParam.Value = 0;
                        this.SynchronouslyQueryServiceBrokerQueue();
                        this._timeoutParam.Value = this._defaultWaitforTimeout;
                        this.AsynchronouslyQueryServiceBrokerQueue();
                        this._errorState = false;
                        this._retryTimer = null;
                    }
                }
                if (this._stop)
                {
                    this.TearDownAndDispose();
                }
            }
            catch (Exception exception3)
            {
                if (!ADP.IsCatchableExceptionType(exception3))
                {
                    throw;
                }
                ADP.TraceExceptionWithoutRethrow(exception3);
                try
                {
                    SqlDependencyProcessDispatcher._staticInstance.Invalidate(this.Server, new SqlNotification(SqlNotificationInfo.Error, SqlNotificationSource.Client, SqlNotificationType.Change, null));
                }
                catch (Exception exception2)
                {
                    if (!ADP.IsCatchableExceptionType(exception2))
                    {
                        throw;
                    }
                    ADP.TraceExceptionWithoutRethrow(exception2);
                }
                try
                {
                    this._con.Close();
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ADP.TraceExceptionWithoutRethrow(exception);
                }
                if (!this._stop)
                {
                    this._retryTimer = new Timer(new TimerCallback(this.Restart), null, this._defaultWaitforTimeout, -1);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal bool Stop(string appDomainKey, out bool appDomainStop)
        {
            bool flag;
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.Stop|DEP> %d#", this.ObjectID);
            try
            {
                appDomainStop = false;
                if (appDomainKey != null)
                {
                    lock (this._appDomainKeyHash)
                    {
                        if (this._appDomainKeyHash.ContainsKey(appDomainKey))
                        {
                            int num = this._appDomainKeyHash[appDomainKey];
                            Bid.NotificationsTrace("<sc.SqlConnectionContainer.Stop|DEP> _appDomainKeyHash contained AppDomainKey: '%ls', pre-decrement Count: '%d'.\n", appDomainKey, num);
                            if (num > 0)
                            {
                                this._appDomainKeyHash[appDomainKey] = num - 1;
                            }
                            else
                            {
                                Bid.NotificationsTrace("<sc.SqlConnectionContainer.Stop|DEP}ERR> ERROR pre-decremented count <= 0!\n");
                            }
                            if (1 == num)
                            {
                                this._appDomainKeyHash.Remove(appDomainKey);
                                appDomainStop = true;
                            }
                        }
                        else
                        {
                            Bid.NotificationsTrace("<sc.SqlConnectionContainer.Stop|DEP|ERR> ERROR appDomainKey not null and not found in hash!\n");
                        }
                    }
                }
                if (Interlocked.Decrement(ref this._startCount) == 0)
                {
                    Bid.NotificationsTrace("<sc.SqlConnectionContainer.Stop|DEP> Reached 0 count, cancelling and waiting.\n");
                    lock (this)
                    {
                        try
                        {
                            this._com.Cancel();
                        }
                        catch (Exception exception)
                        {
                            if (!ADP.IsCatchableExceptionType(exception))
                            {
                                throw;
                            }
                            ADP.TraceExceptionWithoutRethrow(exception);
                        }
                        this._stop = true;
                    }
                    while (true)
                    {
                        lock (this)
                        {
                            if (this._stopped)
                            {
                                goto Label_016C;
                            }
                            if (this._errorState)
                            {
                                Timer timer = this._retryTimer;
                                this._retryTimer = null;
                                if (timer != null)
                                {
                                    timer.Dispose();
                                }
                                this.TearDownAndDispose();
                            }
                        }
                        Thread.Sleep(0);
                    }
                }
                Bid.NotificationsTrace("<sc.SqlConnectionContainer.Stop|DEP> _startCount not 0 after decrement.  _startCount: '%d'.\n", this._startCount);
            Label_016C:
                flag = this._stopped;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        private void SynchronouslyQueryServiceBrokerQueue()
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.SynchronouslyQueryServiceBrokerQueue|DEP> %d#", this.ObjectID);
            try
            {
                using (SqlDataReader reader = this._com.ExecuteReader())
                {
                    this.ProcessNotificationResults(reader);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void TearDownAndDispose()
        {
            IntPtr ptr;
            Bid.NotificationsScopeEnter(out ptr, "<sc.SqlConnectionContainer.TearDownAndDispose|DEP> %d#", this.ObjectID);
            try
            {
                lock (this)
                {
                    try
                    {
                        if ((this._con.State != ConnectionState.Closed) && (ConnectionState.Broken != this._con.State))
                        {
                            if (this._com.Parameters.Count > 1)
                            {
                                try
                                {
                                    this._com.CommandText = this._endConversationQuery;
                                    this._com.Parameters.Remove(this._timeoutParam);
                                    this._com.ExecuteNonQuery();
                                }
                                catch (Exception exception2)
                                {
                                    if (!ADP.IsCatchableExceptionType(exception2))
                                    {
                                        throw;
                                    }
                                    ADP.TraceExceptionWithoutRethrow(exception2);
                                }
                            }
                            if (this._serviceQueueCreated && !this._errorState)
                            {
                                this._com.CommandText = "BEGIN TRANSACTION; DROP SERVICE " + this._escapedQueueName + "; DROP QUEUE " + this._escapedQueueName + "; DROP PROCEDURE " + this._sprocName + "; COMMIT TRANSACTION;";
                                try
                                {
                                    this._com.ExecuteNonQuery();
                                }
                                catch (Exception exception)
                                {
                                    if (!ADP.IsCatchableExceptionType(exception))
                                    {
                                        throw;
                                    }
                                    ADP.TraceExceptionWithoutRethrow(exception);
                                }
                            }
                        }
                    }
                    finally
                    {
                        this._stopped = true;
                        this._con.Dispose();
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal string Database
        {
            get
            {
                if (this._cachedDatabase == null)
                {
                    this._cachedDatabase = this._con.Database;
                }
                return this._cachedDatabase;
            }
        }

        internal SqlDependencyProcessDispatcher.SqlConnectionContainerHashHelper HashHelper
        {
            get
            {
                return this._hashHelper;
            }
        }

        internal bool InErrorState
        {
            get
            {
                return this._errorState;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal string Queue
        {
            get
            {
                return this._queue;
            }
        }

        internal string Server
        {
            get
            {
                return this._cachedServer;
            }
        }
    }

    private class SqlConnectionContainerHashHelper
    {
        private string _connectionString;
        private SqlConnectionStringBuilder _connectionStringBuilder;
        private DbConnectionPoolIdentity _identity;
        private string _queue;

        internal SqlConnectionContainerHashHelper(DbConnectionPoolIdentity identity, string connectionString, string queue, SqlConnectionStringBuilder connectionStringBuilder)
        {
            this._identity = identity;
            this._connectionString = connectionString;
            this._queue = queue;
            this._connectionStringBuilder = connectionStringBuilder;
        }

        public override bool Equals(object value)
        {
            SqlDependencyProcessDispatcher.SqlConnectionContainerHashHelper helper = (SqlDependencyProcessDispatcher.SqlConnectionContainerHashHelper) value;
            if (helper == null)
            {
                return false;
            }
            if (this == helper)
            {
                return true;
            }
            if (((this._identity != null) && (helper._identity == null)) || ((this._identity == null) && (helper._identity != null)))
            {
                return false;
            }
            if ((this._identity == null) && (helper._identity == null))
            {
                return ((helper._connectionString == this._connectionString) && string.Equals(helper._queue, this._queue, StringComparison.OrdinalIgnoreCase));
            }
            return ((helper._identity.Equals(this._identity) && (helper._connectionString == this._connectionString)) && string.Equals(helper._queue, this._queue, StringComparison.OrdinalIgnoreCase));
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (this._identity != null)
            {
                hashCode = this._identity.GetHashCode();
            }
            if (this._queue != null)
            {
                return ((this._connectionString.GetHashCode() + this._queue.GetHashCode()) + hashCode);
            }
            return (this._connectionString.GetHashCode() + hashCode);
        }

        internal SqlConnectionStringBuilder ConnectionStringBuilder
        {
            get
            {
                return this._connectionStringBuilder;
            }
        }

        internal DbConnectionPoolIdentity Identity
        {
            get
            {
                return this._identity;
            }
        }

        internal string Queue
        {
            get
            {
                return this._queue;
            }
        }
    }

    private class SqlNotificationParser
    {
        private const string InfoAttribute = "info";
        private const string MessageNode = "Message";
        private const string RootNode = "QueryNotification";
        private const string SourceAttribute = "source";
        private const string TypeAttribute = "type";

        internal static SqlNotification ProcessMessage(SqlXml xmlMessage)
        {
            using (XmlReader reader = xmlMessage.CreateReader())
            {
                MessageAttributes none = MessageAttributes.None;
                SqlNotificationType unknown = SqlNotificationType.Unknown;
                SqlNotificationInfo options = SqlNotificationInfo.Unknown;
                SqlNotificationSource source2 = SqlNotificationSource.Unknown;
                string key = string.Empty;
                reader.Read();
                if (((XmlNodeType.Element == reader.NodeType) && ("QueryNotification" == reader.LocalName)) && (3 <= reader.AttributeCount))
                {
                    while ((MessageAttributes.All != none) && reader.MoveToNextAttribute())
                    {
                        try
                        {
                            string localName = reader.LocalName;
                            if (localName != null)
                            {
                                if (!(localName == "type"))
                                {
                                    if (localName == "source")
                                    {
                                        goto Label_00F4;
                                    }
                                    if (localName == "info")
                                    {
                                        goto Label_014F;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        SqlNotificationType type = (SqlNotificationType) Enum.Parse(typeof(SqlNotificationType), reader.Value, true);
                                        if (Enum.IsDefined(typeof(SqlNotificationType), type))
                                        {
                                            unknown = type;
                                        }
                                    }
                                    catch (Exception exception3)
                                    {
                                        if (!ADP.IsCatchableExceptionType(exception3))
                                        {
                                            throw;
                                        }
                                        ADP.TraceExceptionWithoutRethrow(exception3);
                                    }
                                    none |= MessageAttributes.Type;
                                }
                            }
                            continue;
                        Label_00F4:;
                            try
                            {
                                SqlNotificationSource source = (SqlNotificationSource) Enum.Parse(typeof(SqlNotificationSource), reader.Value, true);
                                if (Enum.IsDefined(typeof(SqlNotificationSource), source))
                                {
                                    source2 = source;
                                }
                            }
                            catch (Exception exception2)
                            {
                                if (!ADP.IsCatchableExceptionType(exception2))
                                {
                                    throw;
                                }
                                ADP.TraceExceptionWithoutRethrow(exception2);
                            }
                            none |= MessageAttributes.Source;
                            continue;
                        Label_014F:;
                            try
                            {
                                SqlNotificationInfo info2;
                                string str3 = reader.Value;
                                string str = str3;
                                if (str == null)
                                {
                                    goto Label_019D;
                                }
                                if (!(str == "set options"))
                                {
                                    if (str == "previous invalid")
                                    {
                                        goto Label_0191;
                                    }
                                    if (str == "query template limit")
                                    {
                                        goto Label_0197;
                                    }
                                    goto Label_019D;
                                }
                                options = SqlNotificationInfo.Options;
                                goto Label_01EA;
                            Label_0191:
                                options = SqlNotificationInfo.PreviousFire;
                                goto Label_01EA;
                            Label_0197:
                                options = SqlNotificationInfo.TemplateLimit;
                                goto Label_01EA;
                            Label_019D:
                                info2 = (SqlNotificationInfo) Enum.Parse(typeof(SqlNotificationInfo), str3, true);
                                if (Enum.IsDefined(typeof(SqlNotificationInfo), info2))
                                {
                                    options = info2;
                                }
                            }
                            catch (Exception exception)
                            {
                                if (!ADP.IsCatchableExceptionType(exception))
                                {
                                    throw;
                                }
                                ADP.TraceExceptionWithoutRethrow(exception);
                            }
                        Label_01EA:
                            none |= MessageAttributes.Info;
                            continue;
                        }
                        catch (ArgumentException exception4)
                        {
                            ADP.TraceExceptionWithoutRethrow(exception4);
                            Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> Exception thrown - Enum.Parse failed to parse the value '%ls' of the attribute '%ls'.\n", reader.Value, reader.LocalName);
                            return null;
                        }
                    }
                    if (MessageAttributes.All != none)
                    {
                        Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> Not all expected attributes in Message; messageAttributes = '%d'.\n", (int) none);
                        return null;
                    }
                    if (!reader.Read())
                    {
                        Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> unexpected Read failure on xml or unexpected structure of xml.\n");
                        return null;
                    }
                    if ((XmlNodeType.Element != reader.NodeType) || (string.Compare(reader.LocalName, "Message", StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> unexpected Read failure on xml or unexpected structure of xml.\n");
                        return null;
                    }
                    if (!reader.Read())
                    {
                        Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> unexpected Read failure on xml or unexpected structure of xml.\n");
                        return null;
                    }
                    if (reader.NodeType != XmlNodeType.Text)
                    {
                        Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> unexpected Read failure on xml or unexpected structure of xml.\n");
                        return null;
                    }
                    using (XmlTextReader reader2 = new XmlTextReader(reader.Value, XmlNodeType.Element, null))
                    {
                        if (!reader2.Read())
                        {
                            Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> unexpected Read failure on xml or unexpected structure of xml.\n");
                            return null;
                        }
                        if (reader2.NodeType == XmlNodeType.Text)
                        {
                            key = reader2.Value;
                            reader2.Close();
                        }
                        else
                        {
                            Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> unexpected Read failure on xml or unexpected structure of xml.\n");
                            return null;
                        }
                    }
                    return new SqlNotification(options, source2, unknown, key);
                }
                Bid.Trace("<sc.SqlDependencyProcessDispatcher.ProcessMessage|DEP|ERR> unexpected Read failure on xml or unexpected structure of xml.\n");
                return null;
            }
        }

        [Flags]
        private enum MessageAttributes
        {
            All = 7,
            Info = 4,
            None = 0,
            Source = 2,
            Type = 1
        }
    }
}

