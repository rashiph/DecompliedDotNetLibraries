namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.DataAccess;

    internal static class SqlCacheDependencyManager
    {
        private const string CacheKeySeparator = ":";
        private const char CacheKeySeparatorChar = ':';
        private const string CacheKeySeparatorEscaped = @"\:";
        internal const bool ENABLED_DEFAULT = true;
        internal static TimeSpan OneSec = new TimeSpan(0, 0, 1);
        internal const int POLLTIME_DEFAULT = 0xea60;
        private static int s_activePolling = 0;
        internal static Hashtable s_DatabaseNotifStates = new Hashtable();
        private static bool s_shutdown = false;
        private static TimerCallback s_timerCallback = new TimerCallback(SqlCacheDependencyManager.PollCallback);
        internal const string SQL_CUSTOM_ERROR_TABLE_NOT_FOUND = "00000001";
        internal const int SQL_EXCEPTION_ADHOC = 0xc350;
        internal const int SQL_EXCEPTION_NO_GRANT_PERMISSION = 0x1205;
        internal const int SQL_EXCEPTION_PERMISSION_DENIED_ON_DATABASE = 0x106;
        internal const int SQL_EXCEPTION_PERMISSION_DENIED_ON_OBJECT = 0xe5;
        internal const int SQL_EXCEPTION_PERMISSION_DENIED_ON_USER = 0xac8;
        internal const int SQL_EXCEPTION_SP_NOT_FOUND = 0xafc;
        internal const string SQL_NOTIF_TABLE = "AspNet_SqlCacheTablesForChangeNotification";
        internal const string SQL_POLLING_SP = "AspNet_SqlCachePollingStoredProcedure";
        internal const string SQL_POLLING_SP_DBO = "dbo.AspNet_SqlCachePollingStoredProcedure";
        internal const int TABLE_NAME_LENGTH = 0x80;

        internal static DatabaseNotifState AddRef(string database)
        {
            DatabaseNotifState state = (DatabaseNotifState) s_DatabaseNotifStates[database];
            Interlocked.Increment(ref state._refCount);
            return state;
        }

        internal static void Dispose(int waitTimeoutMs)
        {
            try
            {
                DateTime time = DateTime.UtcNow.AddMilliseconds((double) waitTimeoutMs);
                s_shutdown = true;
                if ((s_DatabaseNotifStates == null) || (s_DatabaseNotifStates.Count <= 0))
                {
                    return;
                }
                lock (s_DatabaseNotifStates)
                {
                    foreach (DictionaryEntry entry in s_DatabaseNotifStates)
                    {
                        object obj2 = entry.Value;
                        if (obj2 != null)
                        {
                            ((DatabaseNotifState) obj2).Dispose();
                        }
                    }
                }
            Label_00A0:
                if (s_activePolling != 0)
                {
                    Thread.Sleep(250);
                    if (Debugger.IsAttached || (DateTime.UtcNow <= time))
                    {
                        goto Label_00A0;
                    }
                }
            }
            catch
            {
            }
        }

        internal static void EnsureTableIsRegisteredAndPolled(string database, string table)
        {
            DateTime time2;
            bool flag2;
            Exception exception;
            int num2;
            bool flag = false;
            if (HttpRuntime.CacheInternal[GetMoniterKey(database, table)] != null)
            {
                return;
            }
            InitPolling(database);
            DatabaseNotifState state = (DatabaseNotifState) s_DatabaseNotifStates[database];
            if (!state._init)
            {
                int num;
                HttpContext current = HttpContext.Current;
                if (current == null)
                {
                    num = 30;
                }
                else
                {
                    num = Math.Max(current.Timeout.Seconds / 3, 30);
                }
                DateTime time = DateTime.UtcNow.Add(new TimeSpan(0, 0, num));
                do
                {
                    if (state._init)
                    {
                        goto Label_00BD;
                    }
                    Thread.Sleep(250);
                }
                while (Debugger.IsAttached || (DateTime.UtcNow <= time));
                throw new HttpException(System.Web.SR.GetString("Cant_connect_sql_cache_dep_database_polling", new object[] { database }));
            }
        Label_00BD:
            num2 = 0;
            lock (state)
            {
                exception = state._pollExpt;
                if (exception != null)
                {
                    num2 = state._pollSqlError;
                }
                time2 = state._utcTablesUpdated;
                flag2 = state._notifEnabled;
            }
            if (((exception == null) && flag2) && state._tables.ContainsKey(table))
            {
                return;
            }
            if (flag || ((DateTime.UtcNow - time2) < OneSec))
            {
                string str;
                if (num2 == 0xafc)
                {
                    exception = null;
                }
                if (exception == null)
                {
                    if (!flag2)
                    {
                        throw new DatabaseNotEnabledForNotificationException(System.Web.SR.GetString("Database_not_enabled_for_notification", new object[] { database }));
                    }
                    throw new TableNotEnabledForNotificationException(System.Web.SR.GetString("Table_not_enabled_for_notification", new object[] { table, database }));
                }
                switch (num2)
                {
                    case 0xe5:
                    case 0x106:
                        str = "Permission_denied_database_polling";
                        break;

                    default:
                        str = "Cant_connect_sql_cache_dep_database_polling";
                        break;
                }
                HttpException e = new HttpException(System.Web.SR.GetString(str, new object[] { database }), exception);
                e.SetFormatter(new UseLastUnhandledErrorFormatter(e));
                throw e;
            }
            UpdateDatabaseNotifState(database);
            flag = true;
            goto Label_00BD;
        }

        internal static SqlCacheDependencyDatabase GetDatabaseConfig(string database)
        {
            object obj2 = RuntimeConfig.GetAppConfig().SqlCacheDependency.Databases[database];
            if (obj2 == null)
            {
                throw new HttpException(System.Web.SR.GetString("Database_not_found", new object[] { database }));
            }
            return (SqlCacheDependencyDatabase) obj2;
        }

        internal static string GetMoniterKey(string database, string table)
        {
            if (database.IndexOf(':') != -1)
            {
                database = database.Replace(":", @"\:");
            }
            if (table.IndexOf(':') != -1)
            {
                table = table.Replace(":", @"\:");
            }
            return ("b" + database + ":" + table);
        }

        internal static void InitPolling(string database)
        {
            SqlCacheDependencySection sqlCacheDependency = RuntimeConfig.GetAppConfig().SqlCacheDependency;
            if (!sqlCacheDependency.Enabled)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Polling_not_enabled_for_sql_cache"), sqlCacheDependency.ElementInformation.Properties["enabled"].Source, sqlCacheDependency.ElementInformation.Properties["enabled"].LineNumber);
            }
            SqlCacheDependencyDatabase databaseConfig = GetDatabaseConfig(database);
            if (databaseConfig.PollTime == 0)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Polltime_zero_for_database_sql_cache", new object[] { database }), databaseConfig.ElementInformation.Properties["pollTime"].Source, databaseConfig.ElementInformation.Properties["pollTime"].LineNumber);
            }
            if (!s_DatabaseNotifStates.ContainsKey(database))
            {
                string connection = SqlConnectionHelper.GetConnectionString(databaseConfig.ConnectionStringName, true, true);
                if ((connection == null) || (connection.Length < 1))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Connection_string_not_found", new object[] { databaseConfig.ConnectionStringName }), databaseConfig.ElementInformation.Properties["connectionStringName"].Source, databaseConfig.ElementInformation.Properties["connectionStringName"].LineNumber);
                }
                lock (s_DatabaseNotifStates)
                {
                    if (!s_DatabaseNotifStates.ContainsKey(database))
                    {
                        DatabaseNotifState state;
                        state = new DatabaseNotifState(database, connection, databaseConfig.PollTime) {
                            _timer = new Timer(s_timerCallback, state, 0, databaseConfig.PollTime)
                        };
                        s_DatabaseNotifStates.Add(database, state);
                    }
                }
            }
        }

        private static void PollCallback(object state)
        {
            using (new ApplicationImpersonationContext())
            {
                PollDatabaseForChanges((DatabaseNotifState) state, true);
            }
        }

        internal static void PollDatabaseForChanges(DatabaseNotifState dbState, bool fromTimer)
        {
            SqlDataReader reader = null;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            bool flag = false;
            Exception exception = null;
            SqlException exception2 = null;
            if (s_shutdown)
            {
                return;
            }
            if (((dbState._refCount == 0) && fromTimer) && dbState._init)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref dbState._rqInCallback, 1, 0) != 0)
            {
                int num2;
                if (fromTimer)
                {
                    return;
                }
                HttpContext current = HttpContext.Current;
                if (current == null)
                {
                    num2 = 30;
                }
                else
                {
                    num2 = Math.Max(current.Timeout.Seconds / 3, 30);
                }
                DateTime time = DateTime.UtcNow.Add(new TimeSpan(0, 0, num2));
                do
                {
                    if (Interlocked.CompareExchange(ref dbState._rqInCallback, 1, 0) == 0)
                    {
                        goto Label_00EA;
                    }
                    Thread.Sleep(250);
                    if (s_shutdown)
                    {
                        return;
                    }
                }
                while (Debugger.IsAttached || (DateTime.UtcNow <= time));
                throw new HttpException(System.Web.SR.GetString("Cant_connect_sql_cache_dep_database_polling", new object[] { dbState._database }));
            }
        Label_00EA:
            try
            {
                try
                {
                    Interlocked.Increment(ref s_activePolling);
                    dbState.GetConnection(out sqlConn, out sqlCmd);
                    reader = sqlCmd.ExecuteReader();
                    if (!s_shutdown)
                    {
                        flag = true;
                        Hashtable hashtable = (Hashtable) dbState._tables.Clone();
                        while (reader.Read())
                        {
                            string table = reader.GetString(0);
                            int num = reader.GetInt32(1);
                            string moniterKey = GetMoniterKey(dbState._database, table);
                            object obj2 = cacheInternal[moniterKey];
                            if (obj2 == null)
                            {
                                cacheInternal.UtcAdd(moniterKey, num, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                                dbState._tables.Add(table, null);
                            }
                            else if (num != ((int) obj2))
                            {
                                cacheInternal.UtcInsert(moniterKey, num, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                            }
                            hashtable.Remove(table);
                        }
                        foreach (object obj3 in hashtable.Keys)
                        {
                            dbState._tables.Remove((string) obj3);
                            cacheInternal.Remove(GetMoniterKey(dbState._database, (string) obj3));
                        }
                        if (dbState._pollSqlError != 0)
                        {
                            dbState._pollSqlError = 0;
                        }
                    }
                }
                catch (Exception exception3)
                {
                    exception = exception3;
                    exception2 = exception3 as SqlException;
                    if (exception2 != null)
                    {
                        dbState._pollSqlError = exception2.Number;
                    }
                    else
                    {
                        dbState._pollSqlError = 0;
                    }
                }
                finally
                {
                    try
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                        dbState.ReleaseConnection(ref sqlConn, ref sqlCmd, exception != null);
                    }
                    catch
                    {
                    }
                    lock (dbState)
                    {
                        dbState._pollExpt = exception;
                        if ((dbState._notifEnabled && !flag) && ((exception != null) && (dbState._pollSqlError == 0xafc)))
                        {
                            foreach (object obj4 in dbState._tables.Keys)
                            {
                                try
                                {
                                    cacheInternal.Remove(GetMoniterKey(dbState._database, (string) obj4));
                                }
                                catch
                                {
                                }
                            }
                            dbState._tables.Clear();
                        }
                        dbState._notifEnabled = flag;
                        dbState._utcTablesUpdated = DateTime.UtcNow;
                    }
                    if (!dbState._init)
                    {
                        dbState._init = true;
                    }
                    Interlocked.Decrement(ref s_activePolling);
                    Interlocked.Exchange(ref dbState._rqInCallback, 0);
                }
            }
            catch
            {
                throw;
            }
        }

        internal static void Release(DatabaseNotifState dbState)
        {
            Interlocked.Decrement(ref dbState._refCount);
        }

        internal static void UpdateAllDatabaseNotifState()
        {
            lock (s_DatabaseNotifStates)
            {
                foreach (DictionaryEntry entry in s_DatabaseNotifStates)
                {
                    DatabaseNotifState state = (DatabaseNotifState) entry.Value;
                    if (state._init)
                    {
                        UpdateDatabaseNotifState((string) entry.Key);
                    }
                }
            }
        }

        internal static void UpdateDatabaseNotifState(string database)
        {
            using (new ApplicationImpersonationContext())
            {
                InitPolling(database);
                PollDatabaseForChanges((DatabaseNotifState) s_DatabaseNotifStates[database], false);
            }
        }
    }
}

