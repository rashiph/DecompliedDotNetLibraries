namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class SqlConnectionFactory : DbConnectionFactory
    {
        private const string _metaDataXml = "MetaDataXml";
        public static readonly SqlConnectionFactory SingletonInstance = new SqlConnectionFactory();

        private SqlConnectionFactory() : base(SqlPerformanceCounters.SingletonInstance)
        {
        }

        protected override DbConnectionInternal CreateConnection(DbConnectionOptions options, object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningConnection)
        {
            string instanceName;
            SqlConnectionString str = (SqlConnectionString) options;
            if (str.ContextConnection)
            {
                return this.GetContextConnection(str, poolGroupProviderInfo, owningConnection);
            }
            bool redirectedUserInstance = false;
            DbConnectionPoolIdentity current = null;
            if (str.IntegratedSecurity)
            {
                if (pool != null)
                {
                    current = pool.Identity;
                }
                else
                {
                    current = DbConnectionPoolIdentity.GetCurrent();
                }
            }
            if (!str.UserInstance)
            {
                goto Label_00F1;
            }
            redirectedUserInstance = true;
            if ((pool == null) || ((pool != null) && (pool.Count <= 0)))
            {
                using (SqlInternalConnectionTds tds = null)
                {
                    SqlConnectionString connectionOptions = new SqlConnectionString(str, str.DataSource, true, false);
                    tds = new SqlInternalConnectionTds(current, connectionOptions, null, "", null, false);
                    instanceName = tds.InstanceName;
                    if (!instanceName.StartsWith(@"\\.\", StringComparison.Ordinal))
                    {
                        throw SQL.NonLocalSSEInstance();
                    }
                    if (pool != null)
                    {
                        SqlConnectionPoolProviderInfo info2 = (SqlConnectionPoolProviderInfo) pool.ProviderInfo;
                        info2.InstanceName = instanceName;
                    }
                    goto Label_00DB;
                }
            }
            SqlConnectionPoolProviderInfo providerInfo = (SqlConnectionPoolProviderInfo) pool.ProviderInfo;
            instanceName = providerInfo.InstanceName;
        Label_00DB:
            str = new SqlConnectionString(str, instanceName, false, null);
            poolGroupProviderInfo = null;
        Label_00F1:
            return new SqlInternalConnectionTds(current, str, poolGroupProviderInfo, "", (SqlConnection) owningConnection, redirectedUserInstance);
        }

        protected override DbConnectionOptions CreateConnectionOptions(string connectionString, DbConnectionOptions previous)
        {
            return new SqlConnectionString(connectionString);
        }

        protected override DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(DbConnectionOptions connectionOptions)
        {
            SqlConnectionString str = (SqlConnectionString) connectionOptions;
            DbConnectionPoolGroupOptions options = null;
            if (str.ContextConnection || !str.Pooling)
            {
                return options;
            }
            int connectTimeout = str.ConnectTimeout;
            if ((0 < connectTimeout) && (connectTimeout < 0x20c49b))
            {
                connectTimeout *= 0x3e8;
            }
            else if (connectTimeout >= 0x20c49b)
            {
                connectTimeout = 0x7fffffff;
            }
            return new DbConnectionPoolGroupOptions(str.IntegratedSecurity, str.MinPoolSize, str.MaxPoolSize, connectTimeout, str.LoadBalanceTimeout, str.Enlist, false);
        }

        internal override DbConnectionPoolGroupProviderInfo CreateConnectionPoolGroupProviderInfo(DbConnectionOptions connectionOptions)
        {
            return new SqlConnectionPoolGroupProviderInfo((SqlConnectionString) connectionOptions);
        }

        internal override DbConnectionPoolProviderInfo CreateConnectionPoolProviderInfo(DbConnectionOptions connectionOptions)
        {
            DbConnectionPoolProviderInfo info = null;
            if (((SqlConnectionString) connectionOptions).UserInstance)
            {
                info = new SqlConnectionPoolProviderInfo();
            }
            return info;
        }

        protected override DbMetaDataFactory CreateMetaDataFactory(DbConnectionInternal internalConnection, out bool cacheMetaDataFactory)
        {
            cacheMetaDataFactory = false;
            if (internalConnection is SqlInternalConnectionSmi)
            {
                throw SQL.NotAvailableOnContextConnection();
            }
            NameValueCollection section = (NameValueCollection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.data.sqlclient");
            Stream xMLStream = null;
            if (section != null)
            {
                string[] values = section.GetValues("MetaDataXml");
                if (values != null)
                {
                    xMLStream = ADP.GetXmlStreamFromValues(values, "MetaDataXml");
                }
            }
            if (xMLStream == null)
            {
                xMLStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("System.Data.SqlClient.SqlMetaData.xml");
                cacheMetaDataFactory = true;
            }
            return new SqlMetaDataFactory(xMLStream, internalConnection.ServerVersion, internalConnection.ServerVersion);
        }

        internal static SqlConnectionString FindSqlConnectionOptions(string connectionString)
        {
            SqlConnectionString str = (SqlConnectionString) SingletonInstance.FindConnectionOptions(connectionString);
            if (str == null)
            {
                str = new SqlConnectionString(connectionString);
            }
            if (str.IsEmpty)
            {
                throw ADP.NoConnectionString();
            }
            return str;
        }

        internal override DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection)
        {
            SqlConnection connection2 = connection as SqlConnection;
            if (connection2 != null)
            {
                return connection2.PoolGroup;
            }
            return null;
        }

        private SqlInternalConnectionSmi GetContextConnection(SqlConnectionString options, object providerInfo, DbConnection owningConnection)
        {
            SmiContext currentContext = SmiContextFactory.Instance.GetCurrentContext();
            SqlInternalConnectionSmi contextValue = (SqlInternalConnectionSmi) currentContext.GetContextValue(0);
            if ((contextValue == null) || contextValue.IsConnectionDoomed)
            {
                if (contextValue != null)
                {
                    contextValue.Dispose();
                }
                contextValue = new SqlInternalConnectionSmi(options, currentContext);
                currentContext.SetContextValue(0, contextValue);
            }
            contextValue.Activate();
            return contextValue;
        }

        internal override DbConnectionInternal GetInnerConnection(DbConnection connection)
        {
            SqlConnection connection2 = connection as SqlConnection;
            if (connection2 != null)
            {
                return connection2.InnerConnection;
            }
            return null;
        }

        protected override int GetObjectId(DbConnection connection)
        {
            SqlConnection connection2 = connection as SqlConnection;
            if (connection2 != null)
            {
                return connection2.ObjectID;
            }
            return 0;
        }

        internal override void PermissionDemand(DbConnection outerConnection)
        {
            SqlConnection connection = outerConnection as SqlConnection;
            if (connection != null)
            {
                connection.PermissionDemand();
            }
        }

        internal override void SetConnectionPoolGroup(DbConnection outerConnection, DbConnectionPoolGroup poolGroup)
        {
            SqlConnection connection = outerConnection as SqlConnection;
            if (connection != null)
            {
                connection.PoolGroup = poolGroup;
            }
        }

        internal override void SetInnerConnectionEvent(DbConnection owningObject, DbConnectionInternal to)
        {
            SqlConnection connection = owningObject as SqlConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionEvent(to);
            }
        }

        internal override bool SetInnerConnectionFrom(DbConnection owningObject, DbConnectionInternal to, DbConnectionInternal from)
        {
            SqlConnection connection = owningObject as SqlConnection;
            return ((connection != null) && connection.SetInnerConnectionFrom(to, from));
        }

        internal override void SetInnerConnectionTo(DbConnection owningObject, DbConnectionInternal to)
        {
            SqlConnection connection = owningObject as SqlConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionTo(to);
            }
        }

        public override DbProviderFactory ProviderFactory
        {
            get
            {
                return SqlClientFactory.Instance;
            }
        }
    }
}

