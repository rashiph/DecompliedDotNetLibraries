namespace System.Data.OleDb
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class OleDbConnectionFactory : DbConnectionFactory
    {
        private const string _defaultMetaDataXml = "defaultMetaDataXml";
        private const string _metaDataXml = ":MetaDataXml";
        public static readonly OleDbConnectionFactory SingletonInstance = new OleDbConnectionFactory();

        private OleDbConnectionFactory()
        {
        }

        protected override DbConnectionInternal CreateConnection(DbConnectionOptions options, object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningObject)
        {
            return new OleDbConnectionInternal((OleDbConnectionString) options, (OleDbConnection) owningObject);
        }

        protected override DbConnectionOptions CreateConnectionOptions(string connectionString, DbConnectionOptions previous)
        {
            return new OleDbConnectionString(connectionString, null != previous);
        }

        protected override DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(DbConnectionOptions connectionOptions)
        {
            return null;
        }

        internal override DbConnectionPoolGroupProviderInfo CreateConnectionPoolGroupProviderInfo(DbConnectionOptions connectionOptions)
        {
            return new OleDbConnectionPoolGroupProviderInfo();
        }

        protected override DbMetaDataFactory CreateMetaDataFactory(DbConnectionInternal internalConnection, out bool cacheMetaDataFactory)
        {
            cacheMetaDataFactory = false;
            OleDbConnectionInternal internal2 = (OleDbConnectionInternal) internalConnection;
            OleDbConnection connection = internal2.Connection;
            NameValueCollection section = (NameValueCollection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.data.oledb");
            Stream xMLStream = null;
            string dataSourcePropertyValue = connection.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 0x60) as string;
            if (section != null)
            {
                string[] values = null;
                string name = null;
                if (dataSourcePropertyValue != null)
                {
                    name = dataSourcePropertyValue + ":MetaDataXml";
                    values = section.GetValues(name);
                }
                if (values == null)
                {
                    name = "defaultMetaDataXml";
                    values = section.GetValues(name);
                }
                if (values != null)
                {
                    xMLStream = ADP.GetXmlStreamFromValues(values, name);
                }
            }
            if (xMLStream == null)
            {
                xMLStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("System.Data.OleDb.OleDbMetaData.xml");
                cacheMetaDataFactory = true;
            }
            return new OleDbMetaDataFactory(xMLStream, internal2.ServerVersion, internal2.ServerVersion, internal2.GetSchemaRowsetInformation());
        }

        internal override DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection)
        {
            OleDbConnection connection2 = connection as OleDbConnection;
            if (connection2 != null)
            {
                return connection2.PoolGroup;
            }
            return null;
        }

        internal override DbConnectionInternal GetInnerConnection(DbConnection connection)
        {
            OleDbConnection connection2 = connection as OleDbConnection;
            if (connection2 != null)
            {
                return connection2.InnerConnection;
            }
            return null;
        }

        protected override int GetObjectId(DbConnection connection)
        {
            OleDbConnection connection2 = connection as OleDbConnection;
            if (connection2 != null)
            {
                return connection2.ObjectID;
            }
            return 0;
        }

        internal override void PermissionDemand(DbConnection outerConnection)
        {
            OleDbConnection connection = outerConnection as OleDbConnection;
            if (connection != null)
            {
                connection.PermissionDemand();
            }
        }

        internal override void SetConnectionPoolGroup(DbConnection outerConnection, DbConnectionPoolGroup poolGroup)
        {
            OleDbConnection connection = outerConnection as OleDbConnection;
            if (connection != null)
            {
                connection.PoolGroup = poolGroup;
            }
        }

        internal override void SetInnerConnectionEvent(DbConnection owningObject, DbConnectionInternal to)
        {
            OleDbConnection connection = owningObject as OleDbConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionEvent(to);
            }
        }

        internal override bool SetInnerConnectionFrom(DbConnection owningObject, DbConnectionInternal to, DbConnectionInternal from)
        {
            OleDbConnection connection = owningObject as OleDbConnection;
            return ((connection != null) && connection.SetInnerConnectionFrom(to, from));
        }

        internal override void SetInnerConnectionTo(DbConnection owningObject, DbConnectionInternal to)
        {
            OleDbConnection connection = owningObject as OleDbConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionTo(to);
            }
        }

        public override DbProviderFactory ProviderFactory
        {
            get
            {
                return OleDbFactory.Instance;
            }
        }
    }
}

