namespace System.Web.SessionState
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal class SqlSessionStateStore : SessionStateStoreProviderBase
    {
        private SqlPartitionInfo _partitionInfo;
        private IPartitionResolver _partitionResolver;
        private HttpContext _rqContext;
        private int _rqOrigStreamLen;
        private const int APP_SUFFIX_LENGTH = 8;
        private const int FIRST_RETRY_SLEEP_TIME = 0x1388;
        private static int ID_LENGTH = (SessionIDManager.SessionIDMaxLength + 8);
        private const int ITEM_SHORT_LENGTH = 0x1b58;
        private const int RETRY_SLEEP_TIME = 0x3e8;
        private static int s_commandTimeout;
        private static bool s_configAllowCustomSqlDatabase;
        private static bool s_configCompressionEnabled;
        private static string s_configPartitionResolverType;
        private static string s_configSqlConnectionFileName;
        private static int s_configSqlConnectionLineNumber;
        private static int s_isClearPoolInProgress;
        private static ReadWriteSpinLock s_lock;
        private static EventHandler s_onAppDomainUnload;
        private static bool s_oneTimeInited;
        private static PartitionManager s_partitionManager;
        private static TimeSpan s_retryInterval;
        private static SqlPartitionInfo s_singlePartitionInfo;
        private static bool s_usePartition;
        private const int SQL_CANNOT_OPEN_DATABASE_FOR_LOGIN = 0xfdc;
        internal const int SQL_COMMAND_TIMEOUT_DEFAULT = 30;
        private const int SQL_ERROR_PRIMARY_KEY_VIOLATION = 0xa43;
        private const int SQL_LOGIN_FAILED = 0x4818;
        private const int SQL_LOGIN_FAILED_2 = 0x4814;
        private const int SQL_LOGIN_FAILED_3 = 0x4812;
        private const int SQL_TIMEOUT_EXPIRED = -2;

        internal SqlSessionStateStore()
        {
        }

        private static bool CanRetry(SqlException ex, SqlConnection conn, ref bool isFirstAttempt, ref DateTime endRetryTime)
        {
            if (s_retryInterval.Seconds <= 0)
            {
                return false;
            }
            if (!IsFatalSqlException(ex))
            {
                if (!isFirstAttempt)
                {
                    ClearFlagForClearPoolInProgress();
                }
                return false;
            }
            if (isFirstAttempt)
            {
                if (Interlocked.CompareExchange(ref s_isClearPoolInProgress, 1, 0) == 0)
                {
                    SqlConnection.ClearPool(conn);
                }
                Thread.Sleep(0x1388);
                endRetryTime = DateTime.UtcNow.Add(s_retryInterval);
                isFirstAttempt = false;
                return true;
            }
            if (DateTime.UtcNow > endRetryTime)
            {
                if (!isFirstAttempt)
                {
                    ClearFlagForClearPoolInProgress();
                }
                return false;
            }
            Thread.Sleep(0x3e8);
            return true;
        }

        private bool CanUsePooling()
        {
            if (this.KnowForSureNotUsingIntegratedSecurity)
            {
                return true;
            }
            if (this._rqContext == null)
            {
                return false;
            }
            if (!this._rqContext.IsClientImpersonationConfigured)
            {
                return true;
            }
            if (HttpRuntime.IsOnUNCShareInternal)
            {
                return false;
            }
            return string.IsNullOrEmpty(this._rqContext.WorkerRequest.GetServerVariable("LOGON_USER"));
        }

        private static void ClearFlagForClearPoolInProgress()
        {
            Interlocked.CompareExchange(ref s_isClearPoolInProgress, 0, 1);
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return SessionStateUtility.CreateLegitStoreData(context, null, null, timeout);
        }

        internal IPartitionInfo CreatePartitionInfo(string sqlConnectionString)
        {
            SqlConnection connection;
            string path = null;
            try
            {
                connection = new SqlConnection(sqlConnectionString);
            }
            catch (Exception exception)
            {
                if (s_usePartition)
                {
                    HttpException e = new HttpException(System.Web.SR.GetString("Error_parsing_sql_partition_resolver_string", new object[] { s_configPartitionResolverType, exception.Message }), exception);
                    e.SetFormatter(new UseLastUnhandledErrorFormatter(e));
                    throw e;
                }
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Error_parsing_session_sqlConnectionString", new object[] { exception.Message }), exception, s_configSqlConnectionFileName, s_configSqlConnectionLineNumber);
            }
            string database = connection.Database;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(sqlConnectionString);
            if (string.IsNullOrEmpty(database))
            {
                database = builder.AttachDBFilename;
                path = database;
            }
            if (!string.IsNullOrEmpty(database))
            {
                if (!s_configAllowCustomSqlDatabase)
                {
                    if (s_usePartition)
                    {
                        throw new HttpException(System.Web.SR.GetString("No_database_allowed_in_sql_partition_resolver_string", new object[] { s_configPartitionResolverType, connection.DataSource, database }));
                    }
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("No_database_allowed_in_sqlConnectionString"), s_configSqlConnectionFileName, s_configSqlConnectionLineNumber);
                }
                if (path != null)
                {
                    HttpRuntime.CheckFilePermission(path, true);
                }
            }
            else
            {
                sqlConnectionString = sqlConnectionString + ";Initial Catalog=ASPState";
            }
            return new SqlPartitionInfo(new ResourcePool(new TimeSpan(0, 0, 5), 0x7fffffff), builder.IntegratedSecurity, sqlConnectionString);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            bool usePooling = true;
            SqlStateConnection conn = null;
            try
            {
                byte[] buffer;
                int num;
                SessionIDManager.CheckIdLength(id, true);
                SessionStateUtility.SerializeStoreData(this.CreateNewStoreData(context, timeout), 0x1b58, out buffer, out num, s_configCompressionEnabled);
                conn = this.GetConnection(id, ref usePooling);
                SqlCommand tempInsertUninitializedItem = conn.TempInsertUninitializedItem;
                tempInsertUninitializedItem.Parameters[0].Value = id + this._partitionInfo.AppSuffix;
                tempInsertUninitializedItem.Parameters[1].Size = num;
                tempInsertUninitializedItem.Parameters[1].Value = buffer;
                tempInsertUninitializedItem.Parameters[2].Value = timeout;
                SqlExecuteNonQueryWithRetry(tempInsertUninitializedItem, true, id);
            }
            finally
            {
                this.DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override void Dispose()
        {
        }

        private void DisposeOrReuseConnection(ref SqlStateConnection conn, bool usePooling)
        {
            try
            {
                if ((conn != null) && usePooling)
                {
                    conn.ClearAllParameters();
                    this._partitionInfo.StoreResource(conn);
                    conn = null;
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }

        private SessionStateStoreData DoGet(HttpContext context, string id, bool getExclusive, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            MemoryStream stream = null;
            SessionStateStoreData data2;
            bool flag = false;
            SqlStateConnection conn = null;
            SqlCommand cmd = null;
            bool usePooling = true;
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = SessionStateActions.None;
            byte[] buffer = null;
            SqlDataReader reader = null;
            conn = this.GetConnection(id, ref usePooling);
            if ((this._partitionInfo.SupportFlags & SupportFlags.GetLockAge) != SupportFlags.None)
            {
                flag = true;
            }
            try
            {
                SessionStateStoreData data;
                if (getExclusive)
                {
                    cmd = conn.TempGetExclusive;
                }
                else
                {
                    cmd = conn.TempGet;
                }
                cmd.Parameters[0].Value = id + this._partitionInfo.AppSuffix;
                cmd.Parameters[1].Value = Convert.DBNull;
                cmd.Parameters[2].Value = Convert.DBNull;
                cmd.Parameters[3].Value = Convert.DBNull;
                cmd.Parameters[4].Value = Convert.DBNull;
                cmd.Parameters[5].Value = Convert.DBNull;
                using (reader = SqlExecuteReaderWithRetry(cmd, CommandBehavior.Default))
                {
                    if (reader != null)
                    {
                        try
                        {
                            if (reader.Read())
                            {
                                buffer = (byte[]) reader[0];
                            }
                        }
                        catch (Exception exception)
                        {
                            ThrowSqlConnectionException(cmd.Connection, exception);
                        }
                    }
                }
                if (Convert.IsDBNull(cmd.Parameters[2].Value))
                {
                    return null;
                }
                locked = (bool) cmd.Parameters[2].Value;
                lockId = (int) cmd.Parameters[4].Value;
                if (locked)
                {
                    if (flag)
                    {
                        lockAge = new TimeSpan(0, 0, (int) cmd.Parameters[3].Value);
                    }
                    else
                    {
                        DateTime time = (DateTime) cmd.Parameters[3].Value;
                        lockAge = (TimeSpan) (DateTime.Now - time);
                    }
                    if (lockAge > new TimeSpan(0, 0, 0x1e13380))
                    {
                        lockAge = TimeSpan.Zero;
                    }
                    return null;
                }
                actionFlags = (SessionStateActions) cmd.Parameters[5].Value;
                if (buffer == null)
                {
                    buffer = (byte[]) cmd.Parameters[1].Value;
                }
                this.DisposeOrReuseConnection(ref conn, usePooling);
                using (stream = new MemoryStream(buffer))
                {
                    data = SessionStateUtility.DeserializeStoreData(context, stream, s_configCompressionEnabled);
                    this._rqOrigStreamLen = (int) stream.Position;
                }
                data2 = data;
            }
            finally
            {
                this.DisposeOrReuseConnection(ref conn, usePooling);
            }
            return data2;
        }

        public override void EndRequest(HttpContext context)
        {
            this._rqContext = null;
        }

        private SqlStateConnection GetConnection(string id, ref bool usePooling)
        {
            SqlStateConnection connection = null;
            if (this._partitionInfo == null)
            {
                this._partitionInfo = (SqlPartitionInfo) s_partitionManager.GetPartition(this._partitionResolver, id);
            }
            usePooling = this.CanUsePooling();
            if (usePooling)
            {
                connection = (SqlStateConnection) this._partitionInfo.RetrieveResource();
                if ((connection != null) && ((connection.Connection.State & ConnectionState.Open) == ConnectionState.Closed))
                {
                    connection.Dispose();
                    connection = null;
                }
            }
            if (connection == null)
            {
                connection = new SqlStateConnection(this._partitionInfo, s_retryInterval);
            }
            return connection;
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            SessionIDManager.CheckIdLength(id, true);
            return this.DoGet(context, id, false, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            SessionIDManager.CheckIdLength(id, true);
            return this.DoGet(context, id, true, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "SQL Server Session State Provider";
            }
            base.Initialize(name, config);
            if (!s_oneTimeInited)
            {
                s_lock.AcquireWriterLock();
                try
                {
                    if (!s_oneTimeInited)
                    {
                        this.OneTimeInit();
                    }
                }
                finally
                {
                    s_lock.ReleaseWriterLock();
                }
            }
            if (!s_usePartition)
            {
                this._partitionInfo = s_singlePartitionInfo;
            }
        }

        internal override void Initialize(string name, NameValueCollection config, IPartitionResolver partitionResolver)
        {
            this._partitionResolver = partitionResolver;
            this.Initialize(name, config);
        }

        public override void InitializeRequest(HttpContext context)
        {
            this._rqContext = context;
            this._rqOrigStreamLen = 0;
            if (s_usePartition)
            {
                this._partitionInfo = null;
            }
        }

        private static bool IsFatalSqlException(SqlException ex)
        {
            if ((ex == null) || (((ex.Class < 20) && (ex.Number != 0xfdc)) && (ex.Number != -2)))
            {
                return false;
            }
            return true;
        }

        private static bool IsInsertPKException(SqlException ex, bool ignoreInsertPKException, string id)
        {
            return (((ex != null) && (ex.Number == 0xa43)) && ignoreInsertPKException);
        }

        private void OnAppDomainUnload(object unusedObject, EventArgs unusedEventArgs)
        {
            Thread.GetDomain().DomainUnload -= s_onAppDomainUnload;
            if (this._partitionResolver == null)
            {
                if (s_singlePartitionInfo != null)
                {
                    s_singlePartitionInfo.Dispose();
                }
            }
            else if (s_partitionManager != null)
            {
                s_partitionManager.Dispose();
            }
        }

        private void OneTimeInit()
        {
            SessionStateSection sessionState = RuntimeConfig.GetAppConfig().SessionState;
            s_configPartitionResolverType = sessionState.PartitionResolverType;
            s_configSqlConnectionFileName = sessionState.ElementInformation.Properties["sqlConnectionString"].Source;
            s_configSqlConnectionLineNumber = sessionState.ElementInformation.Properties["sqlConnectionString"].LineNumber;
            s_configAllowCustomSqlDatabase = sessionState.AllowCustomSqlDatabase;
            s_configCompressionEnabled = sessionState.CompressionEnabled;
            if (this._partitionResolver == null)
            {
                string sqlConnectionString = sessionState.SqlConnectionString;
                SessionStateModule.ReadConnectionString(sessionState, ref sqlConnectionString, "sqlConnectionString");
                s_singlePartitionInfo = (SqlPartitionInfo) this.CreatePartitionInfo(sqlConnectionString);
            }
            else
            {
                s_usePartition = true;
                s_partitionManager = new PartitionManager(new System.Web.CreatePartitionInfo(this.CreatePartitionInfo));
            }
            s_commandTimeout = (int) sessionState.SqlCommandTimeout.TotalSeconds;
            s_retryInterval = sessionState.SqlConnectionRetryInterval;
            s_isClearPoolInProgress = 0;
            s_onAppDomainUnload = new EventHandler(this.OnAppDomainUnload);
            Thread.GetDomain().DomainUnload += s_onAppDomainUnload;
            s_oneTimeInited = true;
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            bool usePooling = true;
            SqlStateConnection conn = null;
            int num = (int) lockId;
            try
            {
                SessionIDManager.CheckIdLength(id, true);
                conn = this.GetConnection(id, ref usePooling);
                SqlCommand tempReleaseExclusive = conn.TempReleaseExclusive;
                tempReleaseExclusive.Parameters[0].Value = id + this._partitionInfo.AppSuffix;
                tempReleaseExclusive.Parameters[1].Value = num;
                SqlExecuteNonQueryWithRetry(tempReleaseExclusive, false, null);
            }
            finally
            {
                this.DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            bool usePooling = true;
            SqlStateConnection conn = null;
            int num = (int) lockId;
            try
            {
                SessionIDManager.CheckIdLength(id, true);
                conn = this.GetConnection(id, ref usePooling);
                SqlCommand tempRemove = conn.TempRemove;
                tempRemove.Parameters[0].Value = id + this._partitionInfo.AppSuffix;
                tempRemove.Parameters[1].Value = num;
                SqlExecuteNonQueryWithRetry(tempRemove, false, null);
            }
            finally
            {
                this.DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            bool usePooling = true;
            SqlStateConnection conn = null;
            try
            {
                SessionIDManager.CheckIdLength(id, true);
                conn = this.GetConnection(id, ref usePooling);
                SqlCommand tempResetTimeout = conn.TempResetTimeout;
                tempResetTimeout.Parameters[0].Value = id + this._partitionInfo.AppSuffix;
                SqlExecuteNonQueryWithRetry(tempResetTimeout, false, null);
            }
            finally
            {
                this.DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            bool usePooling = true;
            SqlStateConnection conn = null;
            try
            {
                byte[] buffer;
                int num;
                SqlCommand tempUpdateShort;
                int num2;
                SessionIDManager.CheckIdLength(id, true);
                try
                {
                    SessionStateUtility.SerializeStoreData(item, 0x1b58, out buffer, out num, s_configCompressionEnabled);
                }
                catch
                {
                    if (!newItem)
                    {
                        this.ReleaseItemExclusive(context, id, lockId);
                    }
                    throw;
                }
                if (lockId == null)
                {
                    num2 = 0;
                }
                else
                {
                    num2 = (int) lockId;
                }
                conn = this.GetConnection(id, ref usePooling);
                if (!newItem)
                {
                    if (num <= 0x1b58)
                    {
                        if (this._rqOrigStreamLen <= 0x1b58)
                        {
                            tempUpdateShort = conn.TempUpdateShort;
                        }
                        else
                        {
                            tempUpdateShort = conn.TempUpdateShortNullLong;
                        }
                    }
                    else if (this._rqOrigStreamLen <= 0x1b58)
                    {
                        tempUpdateShort = conn.TempUpdateLongNullShort;
                    }
                    else
                    {
                        tempUpdateShort = conn.TempUpdateLong;
                    }
                }
                else if (num <= 0x1b58)
                {
                    tempUpdateShort = conn.TempInsertShort;
                }
                else
                {
                    tempUpdateShort = conn.TempInsertLong;
                }
                tempUpdateShort.Parameters[0].Value = id + this._partitionInfo.AppSuffix;
                tempUpdateShort.Parameters[1].Size = num;
                tempUpdateShort.Parameters[1].Value = buffer;
                tempUpdateShort.Parameters[2].Value = item.Timeout;
                if (!newItem)
                {
                    tempUpdateShort.Parameters[3].Value = num2;
                }
                SqlExecuteNonQueryWithRetry(tempUpdateShort, newItem, id);
            }
            finally
            {
                this.DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        private static int SqlExecuteNonQueryWithRetry(SqlCommand cmd, bool ignoreInsertPKException, string id)
        {
            int num2;
            bool isFirstAttempt = true;
            DateTime utcNow = DateTime.UtcNow;
        Label_0008:
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                int num = cmd.ExecuteNonQuery();
                if (!isFirstAttempt)
                {
                    ClearFlagForClearPoolInProgress();
                }
                num2 = num;
            }
            catch (SqlException exception)
            {
                if (IsInsertPKException(exception, ignoreInsertPKException, id))
                {
                    num2 = -1;
                }
                else
                {
                    if (!CanRetry(exception, cmd.Connection, ref isFirstAttempt, ref utcNow))
                    {
                        ThrowSqlConnectionException(cmd.Connection, exception);
                    }
                    goto Label_0008;
                }
            }
            catch (Exception exception2)
            {
                ThrowSqlConnectionException(cmd.Connection, exception2);
                goto Label_0008;
            }
            return num2;
        }

        private static SqlDataReader SqlExecuteReaderWithRetry(SqlCommand cmd, CommandBehavior cmdBehavior)
        {
            SqlDataReader reader2;
            bool isFirstAttempt = true;
            DateTime utcNow = DateTime.UtcNow;
        Label_0008:
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                SqlDataReader reader = cmd.ExecuteReader(cmdBehavior);
                if (!isFirstAttempt)
                {
                    ClearFlagForClearPoolInProgress();
                }
                reader2 = reader;
            }
            catch (SqlException exception)
            {
                if (!CanRetry(exception, cmd.Connection, ref isFirstAttempt, ref utcNow))
                {
                    ThrowSqlConnectionException(cmd.Connection, exception);
                }
                goto Label_0008;
            }
            catch (Exception exception2)
            {
                ThrowSqlConnectionException(cmd.Connection, exception2);
                goto Label_0008;
            }
            return reader2;
        }

        internal static void ThrowSqlConnectionException(SqlConnection conn, Exception e)
        {
            if (s_usePartition)
            {
                throw new HttpException(System.Web.SR.GetString("Cant_connect_sql_session_database_partition_resolver", new object[] { s_configPartitionResolverType, conn.DataSource, conn.Database }));
            }
            throw new HttpException(System.Web.SR.GetString("Cant_connect_sql_session_database"), e);
        }

        public bool KnowForSureNotUsingIntegratedSecurity
        {
            get
            {
                if (this._partitionInfo == null)
                {
                    return false;
                }
                return !this._partitionInfo.UseIntegratedSecurity;
            }
        }

        internal class SqlPartitionInfo : PartitionInfo
        {
            private string _appSuffix;
            private object _lock;
            private string _sqlConnectionString;
            private bool _sqlInfoInited;
            private System.Web.SessionState.SqlSessionStateStore.SupportFlags _support;
            private string _tracingPartitionString;
            private bool _useIntegratedSecurity;
            private const string APP_SUFFIX_FORMAT = "x8";
            private const int APPID_MAX = 280;
            private const int SQL_2000_MAJ_VER = 8;

            internal SqlPartitionInfo(ResourcePool rpool, bool useIntegratedSecurity, string sqlConnectionString) : base(rpool)
            {
                this._support = ~System.Web.SessionState.SqlSessionStateStore.SupportFlags.None;
                this._lock = new object();
                this._useIntegratedSecurity = useIntegratedSecurity;
                this._sqlConnectionString = sqlConnectionString;
            }

            private void GetServerSupportOptions(SqlConnection sqlConnection)
            {
                SqlDataReader reader = null;
                System.Web.SessionState.SqlSessionStateStore.SupportFlags none = System.Web.SessionState.SqlSessionStateStore.SupportFlags.None;
                bool flag = false;
                SqlCommand cmd = new SqlCommand("Select name from sysobjects where type = 'P' and name = 'TempGetVersion'", sqlConnection) {
                    CommandType = CommandType.Text
                };
                using (reader = SqlSessionStateStore.SqlExecuteReaderWithRetry(cmd, CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    if (SqlSessionStateStore.s_usePartition)
                    {
                        throw new HttpException(System.Web.SR.GetString("Need_v2_SQL_Server_partition_resolver", new object[] { SqlSessionStateStore.s_configPartitionResolverType, sqlConnection.DataSource, sqlConnection.Database }));
                    }
                    throw new HttpException(System.Web.SR.GetString("Need_v2_SQL_Server"));
                }
                cmd = new SqlCommand("dbo.GetMajorVersion", sqlConnection) {
                    CommandType = CommandType.StoredProcedure
                };
                SqlParameter parameter = cmd.Parameters.Add(new SqlParameter("@@ver", SqlDbType.Int));
                parameter.Direction = ParameterDirection.Output;
                SqlSessionStateStore.SqlExecuteNonQueryWithRetry(cmd, false, null);
                try
                {
                    if (((int) parameter.Value) >= 8)
                    {
                        none |= System.Web.SessionState.SqlSessionStateStore.SupportFlags.GetLockAge;
                    }
                    this.SupportFlags = none;
                }
                catch (Exception exception)
                {
                    SqlSessionStateStore.ThrowSqlConnectionException(sqlConnection, exception);
                }
            }

            internal void InitSqlInfo(SqlConnection sqlConnection)
            {
                if (!this._sqlInfoInited)
                {
                    lock (this._lock)
                    {
                        if (!this._sqlInfoInited)
                        {
                            this.GetServerSupportOptions(sqlConnection);
                            SqlCommand command = new SqlCommand("dbo.TempGetAppID", sqlConnection) {
                                CommandType = CommandType.StoredProcedure,
                                CommandTimeout = SqlSessionStateStore.s_commandTimeout
                            };
                            command.Parameters.Add(new SqlParameter("@appName", SqlDbType.VarChar, 280)).Value = HttpRuntime.AppDomainAppIdInternal;
                            SqlParameter parameter = command.Parameters.Add(new SqlParameter("@appId", SqlDbType.Int));
                            parameter.Direction = ParameterDirection.Output;
                            parameter.Value = Convert.DBNull;
                            command.ExecuteNonQuery();
                            this._appSuffix = ((int) parameter.Value).ToString("x8", CultureInfo.InvariantCulture);
                            this._sqlInfoInited = true;
                        }
                    }
                }
            }

            internal string AppSuffix
            {
                get
                {
                    return this._appSuffix;
                }
            }

            internal string SqlConnectionString
            {
                get
                {
                    return this._sqlConnectionString;
                }
            }

            internal System.Web.SessionState.SqlSessionStateStore.SupportFlags SupportFlags
            {
                get
                {
                    return this._support;
                }
                set
                {
                    this._support = value;
                }
            }

            protected override string TracingPartitionString
            {
                get
                {
                    if (this._tracingPartitionString == null)
                    {
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(this._sqlConnectionString) {
                            Password = string.Empty,
                            UserID = string.Empty
                        };
                        this._tracingPartitionString = builder.ConnectionString;
                    }
                    return this._tracingPartitionString;
                }
            }

            internal bool UseIntegratedSecurity
            {
                get
                {
                    return this._useIntegratedSecurity;
                }
            }
        }

        private class SqlStateConnection : IDisposable
        {
            private SqlCommand _cmdTempGet;
            private SqlCommand _cmdTempGetExclusive;
            private SqlCommand _cmdTempInsertLong;
            private SqlCommand _cmdTempInsertShort;
            private SqlCommand _cmdTempInsertUninitializedItem;
            private SqlCommand _cmdTempReleaseExclusive;
            private SqlCommand _cmdTempRemove;
            private SqlCommand _cmdTempResetTimeout;
            private SqlCommand _cmdTempUpdateLong;
            private SqlCommand _cmdTempUpdateLongNullShort;
            private SqlCommand _cmdTempUpdateShort;
            private SqlCommand _cmdTempUpdateShortNullLong;
            private SqlSessionStateStore.SqlPartitionInfo _partitionInfo;
            private SqlConnection _sqlConnection;

            internal SqlStateConnection(SqlSessionStateStore.SqlPartitionInfo sqlPartitionInfo, TimeSpan retryInterval)
            {
                this._partitionInfo = sqlPartitionInfo;
                this._sqlConnection = new SqlConnection(sqlPartitionInfo.SqlConnectionString);
                bool isFirstAttempt = true;
                DateTime utcNow = DateTime.UtcNow;
            Label_0026:
                try
                {
                    this._sqlConnection.Open();
                    if (!isFirstAttempt)
                    {
                        SqlSessionStateStore.ClearFlagForClearPoolInProgress();
                    }
                }
                catch (SqlException exception)
                {
                    if ((exception != null) && (((exception.Number == 0x4818) || (exception.Number == 0x4814)) || (exception.Number == 0x4812)))
                    {
                        string name;
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(sqlPartitionInfo.SqlConnectionString);
                        if (builder.IntegratedSecurity)
                        {
                            name = WindowsIdentity.GetCurrent().Name;
                        }
                        else
                        {
                            name = builder.UserID;
                        }
                        HttpException e = new HttpException(System.Web.SR.GetString("Login_failed_sql_session_database", new object[] { name }), exception);
                        e.SetFormatter(new UseLastUnhandledErrorFormatter(e));
                        this.ClearConnectionAndThrow(e);
                    }
                    if (!SqlSessionStateStore.CanRetry(exception, this._sqlConnection, ref isFirstAttempt, ref utcNow))
                    {
                        this.ClearConnectionAndThrow(exception);
                    }
                    goto Label_0026;
                }
                catch (Exception exception3)
                {
                    this.ClearConnectionAndThrow(exception3);
                    goto Label_0026;
                }
                try
                {
                    this._partitionInfo.InitSqlInfo(this._sqlConnection);
                    PerfCounters.IncrementCounter(AppPerfCounter.SESSION_SQL_SERVER_CONNECTIONS);
                }
                catch
                {
                    this.Dispose();
                    throw;
                }
            }

            internal void ClearAllParameters()
            {
                this.ClearAllParameters(this._cmdTempGet);
                this.ClearAllParameters(this._cmdTempGetExclusive);
                this.ClearAllParameters(this._cmdTempReleaseExclusive);
                this.ClearAllParameters(this._cmdTempInsertShort);
                this.ClearAllParameters(this._cmdTempInsertLong);
                this.ClearAllParameters(this._cmdTempUpdateShort);
                this.ClearAllParameters(this._cmdTempUpdateShortNullLong);
                this.ClearAllParameters(this._cmdTempUpdateLong);
                this.ClearAllParameters(this._cmdTempUpdateLongNullShort);
                this.ClearAllParameters(this._cmdTempRemove);
                this.ClearAllParameters(this._cmdTempResetTimeout);
                this.ClearAllParameters(this._cmdTempInsertUninitializedItem);
            }

            internal void ClearAllParameters(SqlCommand cmd)
            {
                if (cmd != null)
                {
                    foreach (SqlParameter parameter in cmd.Parameters)
                    {
                        parameter.Value = Convert.DBNull;
                    }
                }
            }

            private void ClearConnectionAndThrow(Exception e)
            {
                SqlConnection conn = this._sqlConnection;
                this._sqlConnection = null;
                SqlSessionStateStore.ThrowSqlConnectionException(conn, e);
            }

            public void Dispose()
            {
                if (this._sqlConnection != null)
                {
                    this._sqlConnection.Close();
                    this._sqlConnection = null;
                    PerfCounters.DecrementCounter(AppPerfCounter.SESSION_SQL_SERVER_CONNECTIONS);
                }
            }

            internal SqlConnection Connection
            {
                get
                {
                    return this._sqlConnection;
                }
            }

            internal SqlCommand TempGet
            {
                get
                {
                    if (this._cmdTempGet == null)
                    {
                        this._cmdTempGet = new SqlCommand("dbo.TempGetStateItem3", this._sqlConnection);
                        this._cmdTempGet.CommandType = CommandType.StoredProcedure;
                        this._cmdTempGet.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        if ((this._partitionInfo.SupportFlags & SqlSessionStateStore.SupportFlags.GetLockAge) != SqlSessionStateStore.SupportFlags.None)
                        {
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@lockAge", SqlDbType.Int)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int)).Direction = ParameterDirection.Output;
                        }
                        else
                        {
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@lockDate", SqlDbType.DateTime)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int)).Direction = ParameterDirection.Output;
                            this._cmdTempGet.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int)).Direction = ParameterDirection.Output;
                        }
                    }
                    return this._cmdTempGet;
                }
            }

            internal SqlCommand TempGetExclusive
            {
                get
                {
                    if (this._cmdTempGetExclusive == null)
                    {
                        this._cmdTempGetExclusive = new SqlCommand("dbo.TempGetStateItemExclusive3", this._sqlConnection);
                        this._cmdTempGetExclusive.CommandType = CommandType.StoredProcedure;
                        this._cmdTempGetExclusive.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        if ((this._partitionInfo.SupportFlags & SqlSessionStateStore.SupportFlags.GetLockAge) != SqlSessionStateStore.SupportFlags.None)
                        {
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockAge", SqlDbType.Int)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int)).Direction = ParameterDirection.Output;
                        }
                        else
                        {
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockDate", SqlDbType.DateTime)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int)).Direction = ParameterDirection.Output;
                            this._cmdTempGetExclusive.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int)).Direction = ParameterDirection.Output;
                        }
                    }
                    return this._cmdTempGetExclusive;
                }
            }

            internal SqlCommand TempInsertLong
            {
                get
                {
                    if (this._cmdTempInsertLong == null)
                    {
                        this._cmdTempInsertLong = new SqlCommand("dbo.TempInsertStateItemLong", this._sqlConnection);
                        this._cmdTempInsertLong.CommandType = CommandType.StoredProcedure;
                        this._cmdTempInsertLong.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempInsertLong.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempInsertLong.Parameters.Add(new SqlParameter("@itemLong", SqlDbType.Image, 0x1f40));
                        this._cmdTempInsertLong.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                    }
                    return this._cmdTempInsertLong;
                }
            }

            internal SqlCommand TempInsertShort
            {
                get
                {
                    if (this._cmdTempInsertShort == null)
                    {
                        this._cmdTempInsertShort = new SqlCommand("dbo.TempInsertStateItemShort", this._sqlConnection);
                        this._cmdTempInsertShort.CommandType = CommandType.StoredProcedure;
                        this._cmdTempInsertShort.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempInsertShort.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempInsertShort.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58));
                        this._cmdTempInsertShort.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                    }
                    return this._cmdTempInsertShort;
                }
            }

            internal SqlCommand TempInsertUninitializedItem
            {
                get
                {
                    if (this._cmdTempInsertUninitializedItem == null)
                    {
                        this._cmdTempInsertUninitializedItem = new SqlCommand("dbo.TempInsertUninitializedItem", this._sqlConnection);
                        this._cmdTempInsertUninitializedItem.CommandType = CommandType.StoredProcedure;
                        this._cmdTempInsertUninitializedItem.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempInsertUninitializedItem.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempInsertUninitializedItem.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58));
                        this._cmdTempInsertUninitializedItem.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                    }
                    return this._cmdTempInsertUninitializedItem;
                }
            }

            internal SqlCommand TempReleaseExclusive
            {
                get
                {
                    if (this._cmdTempReleaseExclusive == null)
                    {
                        this._cmdTempReleaseExclusive = new SqlCommand("dbo.TempReleaseStateItemExclusive", this._sqlConnection);
                        this._cmdTempReleaseExclusive.CommandType = CommandType.StoredProcedure;
                        this._cmdTempReleaseExclusive.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempReleaseExclusive.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempReleaseExclusive.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }
                    return this._cmdTempReleaseExclusive;
                }
            }

            internal SqlCommand TempRemove
            {
                get
                {
                    if (this._cmdTempRemove == null)
                    {
                        this._cmdTempRemove = new SqlCommand("dbo.TempRemoveStateItem", this._sqlConnection);
                        this._cmdTempRemove.CommandType = CommandType.StoredProcedure;
                        this._cmdTempRemove.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempRemove.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempRemove.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }
                    return this._cmdTempRemove;
                }
            }

            internal SqlCommand TempResetTimeout
            {
                get
                {
                    if (this._cmdTempResetTimeout == null)
                    {
                        this._cmdTempResetTimeout = new SqlCommand("dbo.TempResetTimeout", this._sqlConnection);
                        this._cmdTempResetTimeout.CommandType = CommandType.StoredProcedure;
                        this._cmdTempResetTimeout.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempResetTimeout.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                    }
                    return this._cmdTempResetTimeout;
                }
            }

            internal SqlCommand TempUpdateLong
            {
                get
                {
                    if (this._cmdTempUpdateLong == null)
                    {
                        this._cmdTempUpdateLong = new SqlCommand("dbo.TempUpdateStateItemLong", this._sqlConnection);
                        this._cmdTempUpdateLong.CommandType = CommandType.StoredProcedure;
                        this._cmdTempUpdateLong.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempUpdateLong.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempUpdateLong.Parameters.Add(new SqlParameter("@itemLong", SqlDbType.Image, 0x1f40));
                        this._cmdTempUpdateLong.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        this._cmdTempUpdateLong.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }
                    return this._cmdTempUpdateLong;
                }
            }

            internal SqlCommand TempUpdateLongNullShort
            {
                get
                {
                    if (this._cmdTempUpdateLongNullShort == null)
                    {
                        this._cmdTempUpdateLongNullShort = new SqlCommand("dbo.TempUpdateStateItemLongNullShort", this._sqlConnection);
                        this._cmdTempUpdateLongNullShort.CommandType = CommandType.StoredProcedure;
                        this._cmdTempUpdateLongNullShort.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@itemLong", SqlDbType.Image, 0x1f40));
                        this._cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        this._cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }
                    return this._cmdTempUpdateLongNullShort;
                }
            }

            internal SqlCommand TempUpdateShort
            {
                get
                {
                    if (this._cmdTempUpdateShort == null)
                    {
                        this._cmdTempUpdateShort = new SqlCommand("dbo.TempUpdateStateItemShort", this._sqlConnection);
                        this._cmdTempUpdateShort.CommandType = CommandType.StoredProcedure;
                        this._cmdTempUpdateShort.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempUpdateShort.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempUpdateShort.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58));
                        this._cmdTempUpdateShort.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        this._cmdTempUpdateShort.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }
                    return this._cmdTempUpdateShort;
                }
            }

            internal SqlCommand TempUpdateShortNullLong
            {
                get
                {
                    if (this._cmdTempUpdateShortNullLong == null)
                    {
                        this._cmdTempUpdateShortNullLong = new SqlCommand("dbo.TempUpdateStateItemShortNullLong", this._sqlConnection);
                        this._cmdTempUpdateShortNullLong.CommandType = CommandType.StoredProcedure;
                        this._cmdTempUpdateShortNullLong.CommandTimeout = SqlSessionStateStore.s_commandTimeout;
                        this._cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, SqlSessionStateStore.ID_LENGTH));
                        this._cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, 0x1b58));
                        this._cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        this._cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }
                    return this._cmdTempUpdateShortNullLong;
                }
            }
        }

        internal enum SupportFlags : uint
        {
            GetLockAge = 1,
            None = 0,
            Uninitialized = 0xffffffff
        }
    }
}

