namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.DataAccess;
    using System.Web.Util;

    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class SqlWebEventProvider : BufferedWebEventProvider, IInternalWebEventProvider
    {
        private int _commandTimeout = -1;
        private int _connectionCount;
        private int _maxEventDetailsLength = -1;
        private DateTime _retryDate = DateTime.MinValue;
        private int _SchemaVersionCheck;
        private string _sqlConnectionString;
        private const int NO_LIMIT = -1;
        private const string SP_LOG_EVENT = "dbo.aspnet_WebEvent_LogEvent";
        private const int SQL_MAX_NTEXT_SIZE = 0x3fffffff;

        protected internal SqlWebEventProvider()
        {
        }

        private void CheckSchemaVersion(SqlConnection connection)
        {
            string[] features = new string[] { "Health Monitoring" };
            string version = "1";
            SecUtility.CheckSchemaVersion(this, connection, features, version, ref this._SchemaVersionCheck);
        }

        protected virtual void EventProcessingComplete(WebBaseEventCollection raisedEvents)
        {
        }

        private void FillParams(SqlCommand sqlCommand, WebBaseEvent eventRaised)
        {
            Exception errorException = null;
            WebRequestInformation requestInformation = null;
            string str = null;
            WebApplicationInformation applicationInformation = WebBaseEvent.ApplicationInformation;
            int num = 0;
            sqlCommand.Parameters[num++].Value = eventRaised.EventID.ToString("N", CultureInfo.InstalledUICulture);
            sqlCommand.Parameters[num++].Value = eventRaised.EventTimeUtc;
            sqlCommand.Parameters[num++].Value = eventRaised.EventTime;
            sqlCommand.Parameters[num++].Value = eventRaised.GetType().ToString();
            sqlCommand.Parameters[num++].Value = eventRaised.EventSequence;
            sqlCommand.Parameters[num++].Value = eventRaised.EventOccurrence;
            sqlCommand.Parameters[num++].Value = eventRaised.EventCode;
            sqlCommand.Parameters[num++].Value = eventRaised.EventDetailCode;
            sqlCommand.Parameters[num++].Value = eventRaised.Message;
            sqlCommand.Parameters[num++].Value = applicationInformation.ApplicationPath;
            sqlCommand.Parameters[num++].Value = applicationInformation.ApplicationVirtualPath;
            sqlCommand.Parameters[num++].Value = applicationInformation.MachineName;
            if (eventRaised is WebRequestEvent)
            {
                requestInformation = ((WebRequestEvent) eventRaised).RequestInformation;
            }
            else if (eventRaised is WebRequestErrorEvent)
            {
                requestInformation = ((WebRequestErrorEvent) eventRaised).RequestInformation;
            }
            else if (eventRaised is WebErrorEvent)
            {
                requestInformation = ((WebErrorEvent) eventRaised).RequestInformation;
            }
            else if (eventRaised is WebAuditEvent)
            {
                requestInformation = ((WebAuditEvent) eventRaised).RequestInformation;
            }
            sqlCommand.Parameters[num++].Value = (requestInformation != null) ? requestInformation.RequestUrl : Convert.DBNull;
            if (eventRaised is WebBaseErrorEvent)
            {
                errorException = ((WebBaseErrorEvent) eventRaised).ErrorException;
            }
            sqlCommand.Parameters[num++].Value = (errorException != null) ? errorException.GetType().ToString() : Convert.DBNull;
            str = eventRaised.ToString();
            if ((this._maxEventDetailsLength != -1) && (str.Length > this._maxEventDetailsLength))
            {
                str = str.Substring(0, this._maxEventDetailsLength);
            }
            sqlCommand.Parameters[num++].Value = str;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            this._SchemaVersionCheck = 0;
            string val = null;
            ProviderUtil.GetAndRemoveStringAttribute(config, "connectionStringName", name, ref val);
            ProviderUtil.GetAndRemoveStringAttribute(config, "connectionString", name, ref this._sqlConnectionString);
            if (!string.IsNullOrEmpty(val))
            {
                if (!string.IsNullOrEmpty(this._sqlConnectionString))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Only_one_connection_string_allowed"));
                }
                this._sqlConnectionString = SqlConnectionHelper.GetConnectionString(val, true, true);
                if ((this._sqlConnectionString == null) || (this._sqlConnectionString.Length < 1))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Connection_string_not_found", new object[] { val }));
                }
            }
            else
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(this._sqlConnectionString);
                if (builder.IntegratedSecurity)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Cannot_use_integrated_security"));
                }
            }
            if (string.IsNullOrEmpty(this._sqlConnectionString))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Must_specify_connection_string_or_name", new object[] { val }));
            }
            ProviderUtil.GetAndRemovePositiveOrInfiniteAttribute(config, "maxEventDetailsLength", name, ref this._maxEventDetailsLength);
            if (this._maxEventDetailsLength == 0x7fffffff)
            {
                this._maxEventDetailsLength = -1;
            }
            else if (this._maxEventDetailsLength > 0x3fffffff)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_max_event_details_length", new object[] { name, this._maxEventDetailsLength.ToString(CultureInfo.CurrentCulture) }));
            }
            ProviderUtil.GetAndRemovePositiveAttribute(config, "commandTimeout", name, ref this._commandTimeout);
            base.Initialize(name, config);
        }

        private void PrepareParams(SqlCommand sqlCommand)
        {
            sqlCommand.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Char, 0x20));
            sqlCommand.Parameters.Add(new SqlParameter("@EventTimeUtc", SqlDbType.DateTime));
            sqlCommand.Parameters.Add(new SqlParameter("@EventTime", SqlDbType.DateTime));
            sqlCommand.Parameters.Add(new SqlParameter("@EventType", SqlDbType.NVarChar, 0x100));
            sqlCommand.Parameters.Add(new SqlParameter("@EventSequence", SqlDbType.Decimal));
            sqlCommand.Parameters.Add(new SqlParameter("@EventOccurrence", SqlDbType.Decimal));
            sqlCommand.Parameters.Add(new SqlParameter("@EventCode", SqlDbType.Int));
            sqlCommand.Parameters.Add(new SqlParameter("@EventDetailCode", SqlDbType.Int));
            sqlCommand.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar, 0x400));
            sqlCommand.Parameters.Add(new SqlParameter("@ApplicationPath", SqlDbType.NVarChar, 0x100));
            sqlCommand.Parameters.Add(new SqlParameter("@ApplicationVirtualPath", SqlDbType.NVarChar, 0x100));
            sqlCommand.Parameters.Add(new SqlParameter("@MachineName", SqlDbType.NVarChar, 0x100));
            sqlCommand.Parameters.Add(new SqlParameter("@RequestUrl", SqlDbType.NVarChar, 0x400));
            sqlCommand.Parameters.Add(new SqlParameter("@ExceptionType", SqlDbType.NVarChar, 0x100));
            sqlCommand.Parameters.Add(new SqlParameter("@Details", SqlDbType.NText));
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            if (base.UseBuffering)
            {
                base.ProcessEvent(eventRaised);
            }
            else
            {
                this.WriteToSQL(new WebBaseEventCollection(eventRaised), 0, new DateTime(0L));
            }
        }

        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo)
        {
            this.WriteToSQL(flushInfo.Events, flushInfo.EventsDiscardedSinceLastNotification, flushInfo.LastNotificationUtc);
        }

        public override void Shutdown()
        {
            try
            {
                this.Flush();
            }
            finally
            {
                base.Shutdown();
            }
            if (this._connectionCount > 0)
            {
                int num = this._commandTimeout * 2;
                if (num <= 0)
                {
                    num = 60;
                }
                while ((this._connectionCount > 0) && (num > 0))
                {
                    num--;
                    Thread.Sleep(0x3e8);
                }
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.Assert, Unrestricted=true), SqlClientPermission(SecurityAction.Assert, Unrestricted=true)]
        private void WriteToSQL(WebBaseEventCollection events, int eventsDiscardedByBuffer, DateTime lastNotificationUtc)
        {
            if (this._retryDate <= DateTime.UtcNow)
            {
                try
                {
                    SqlConnectionHolder connection = SqlConnectionHelper.GetConnection(this._sqlConnectionString, true);
                    SqlCommand sqlCommand = new SqlCommand("dbo.aspnet_WebEvent_LogEvent");
                    this.CheckSchemaVersion(connection.Connection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Connection = connection.Connection;
                    if (this._commandTimeout > -1)
                    {
                        sqlCommand.CommandTimeout = this._commandTimeout;
                    }
                    this.PrepareParams(sqlCommand);
                    try
                    {
                        connection.Open(null, true);
                        Interlocked.Increment(ref this._connectionCount);
                        if (eventsDiscardedByBuffer != 0)
                        {
                            WebBaseEvent eventRaised = new WebBaseEvent(System.Web.SR.GetString("Sql_webevent_provider_events_dropped", new object[] { eventsDiscardedByBuffer.ToString(CultureInfo.InstalledUICulture), lastNotificationUtc.ToString("r", CultureInfo.InstalledUICulture) }), null, 0x1771, 0xc47d);
                            this.FillParams(sqlCommand, eventRaised);
                            sqlCommand.ExecuteNonQuery();
                        }
                        foreach (WebBaseEvent event3 in events)
                        {
                            this.FillParams(sqlCommand, event3);
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        connection.Close();
                        Interlocked.Decrement(ref this._connectionCount);
                    }
                    try
                    {
                        this.EventProcessingComplete(events);
                    }
                    catch
                    {
                    }
                }
                catch
                {
                    double num = 30.0;
                    if (this._commandTimeout > -1)
                    {
                        num = this._commandTimeout;
                    }
                    this._retryDate = DateTime.UtcNow.AddSeconds(num);
                    throw;
                }
            }
        }
    }
}

