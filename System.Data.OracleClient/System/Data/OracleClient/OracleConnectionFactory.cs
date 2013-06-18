namespace System.Data.OracleClient
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class OracleConnectionFactory : System.Data.ProviderBase.DbConnectionFactory
    {
        public const string _metaDataXml = "MetaDataXml";
        public static readonly OracleConnectionFactory SingletonInstance = new OracleConnectionFactory();

        private OracleConnectionFactory() : base(OraclePerformanceCounters.SingletonInstance)
        {
        }

        protected override System.Data.ProviderBase.DbConnectionInternal CreateConnection(System.Data.Common.DbConnectionOptions options, object poolGroupProviderInfo, System.Data.ProviderBase.DbConnectionPool pool, DbConnection owningObject)
        {
            return new OracleInternalConnection(options as OracleConnectionString);
        }

        protected override System.Data.Common.DbConnectionOptions CreateConnectionOptions(string connectionOptions, System.Data.Common.DbConnectionOptions previous)
        {
            return new OracleConnectionString(connectionOptions);
        }

        protected override System.Data.ProviderBase.DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(System.Data.Common.DbConnectionOptions connectionOptions)
        {
            OracleConnectionString str = (OracleConnectionString) connectionOptions;
            System.Data.ProviderBase.DbConnectionPoolGroupOptions options = null;
            if (str.Pooling)
            {
                options = new System.Data.ProviderBase.DbConnectionPoolGroupOptions(str.IntegratedSecurity, str.MinPoolSize, str.MaxPoolSize, 0x7530, str.LoadBalanceTimeout, str.Enlist, false);
            }
            return options;
        }

        protected override System.Data.ProviderBase.DbMetaDataFactory CreateMetaDataFactory(System.Data.ProviderBase.DbConnectionInternal internalConnection, out bool cacheMetaDataFactory)
        {
            cacheMetaDataFactory = false;
            NameValueCollection section = (NameValueCollection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.data.oracleclient");
            Stream xmlStream = null;
            if (section != null)
            {
                string[] values = section.GetValues("MetaDataXml");
                if (values != null)
                {
                    xmlStream = System.Data.Common.ADP.GetXmlStreamFromValues(values, "MetaDataXml");
                }
            }
            if (xmlStream == null)
            {
                xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("System.Data.OracleClient.OracleMetaData.xml");
                cacheMetaDataFactory = true;
            }
            return new System.Data.ProviderBase.DbMetaDataFactory(xmlStream, internalConnection.ServerVersion, internalConnection.ServerVersionNormalized);
        }

        internal override System.Data.ProviderBase.DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection)
        {
            OracleConnection connection2 = connection as OracleConnection;
            if (connection2 != null)
            {
                return connection2.PoolGroup;
            }
            return null;
        }

        internal override System.Data.ProviderBase.DbConnectionInternal GetInnerConnection(DbConnection connection)
        {
            OracleConnection connection2 = connection as OracleConnection;
            if (connection2 != null)
            {
                return connection2.InnerConnection;
            }
            return null;
        }

        protected override int GetObjectId(DbConnection connection)
        {
            OracleConnection connection2 = connection as OracleConnection;
            if (connection2 != null)
            {
                return connection2.ObjectID;
            }
            return 0;
        }

        internal override void PermissionDemand(DbConnection outerConnection)
        {
            OracleConnection connection = outerConnection as OracleConnection;
            if (connection != null)
            {
                connection.PermissionDemand();
            }
        }

        internal override void SetConnectionPoolGroup(DbConnection outerConnection, System.Data.ProviderBase.DbConnectionPoolGroup poolGroup)
        {
            OracleConnection connection = outerConnection as OracleConnection;
            if (connection != null)
            {
                connection.PoolGroup = poolGroup;
            }
        }

        internal override void SetInnerConnectionEvent(DbConnection owningObject, System.Data.ProviderBase.DbConnectionInternal to)
        {
            OracleConnection connection = owningObject as OracleConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionEvent(to);
            }
        }

        internal override bool SetInnerConnectionFrom(DbConnection owningObject, System.Data.ProviderBase.DbConnectionInternal to, System.Data.ProviderBase.DbConnectionInternal from)
        {
            OracleConnection connection = owningObject as OracleConnection;
            return ((connection != null) && connection.SetInnerConnectionFrom(to, from));
        }

        internal override void SetInnerConnectionTo(DbConnection owningObject, System.Data.ProviderBase.DbConnectionInternal to)
        {
            OracleConnection connection = owningObject as OracleConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionTo(to);
            }
        }

        public override DbProviderFactory ProviderFactory
        {
            get
            {
                return OracleClientFactory.Instance;
            }
        }
    }
}

