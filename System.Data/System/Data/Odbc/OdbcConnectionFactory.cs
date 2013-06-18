namespace System.Data.Odbc
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class OdbcConnectionFactory : DbConnectionFactory
    {
        private const string _defaultMetaDataXml = "defaultMetaDataXml";
        private const string _MetaData = ":MetaDataXml";
        public static readonly OdbcConnectionFactory SingletonInstance = new OdbcConnectionFactory();

        private OdbcConnectionFactory()
        {
        }

        protected override DbConnectionInternal CreateConnection(DbConnectionOptions options, object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningObject)
        {
            return new OdbcConnectionOpen(owningObject as OdbcConnection, options as OdbcConnectionString);
        }

        protected override DbConnectionOptions CreateConnectionOptions(string connectionString, DbConnectionOptions previous)
        {
            return new OdbcConnectionString(connectionString, null != previous);
        }

        protected override DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(DbConnectionOptions connectionOptions)
        {
            return null;
        }

        internal override DbConnectionPoolGroupProviderInfo CreateConnectionPoolGroupProviderInfo(DbConnectionOptions connectionOptions)
        {
            return new OdbcConnectionPoolGroupProviderInfo();
        }

        protected override DbMetaDataFactory CreateMetaDataFactory(DbConnectionInternal internalConnection, out bool cacheMetaDataFactory)
        {
            cacheMetaDataFactory = false;
            OdbcConnection outerConnection = ((OdbcConnectionOpen) internalConnection).OuterConnection;
            NameValueCollection section = (NameValueCollection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.data.odbc");
            Stream xMLStream = null;
            object obj2 = null;
            string infoStringUnhandled = outerConnection.GetInfoStringUnhandled(ODBC32.SQL_INFO.DRIVER_NAME);
            if (infoStringUnhandled != null)
            {
                obj2 = infoStringUnhandled;
            }
            if (section != null)
            {
                string[] values = null;
                string name = null;
                if (obj2 != null)
                {
                    name = ((string) obj2) + ":MetaDataXml";
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
                xMLStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("System.Data.Odbc.OdbcMetaData.xml");
                cacheMetaDataFactory = true;
            }
            string serverVersion = outerConnection.GetInfoStringUnhandled(ODBC32.SQL_INFO.DBMS_VER);
            return new OdbcMetaDataFactory(xMLStream, serverVersion, serverVersion, outerConnection);
        }

        internal override DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection)
        {
            OdbcConnection connection2 = connection as OdbcConnection;
            if (connection2 != null)
            {
                return connection2.PoolGroup;
            }
            return null;
        }

        internal override DbConnectionInternal GetInnerConnection(DbConnection connection)
        {
            OdbcConnection connection2 = connection as OdbcConnection;
            if (connection2 != null)
            {
                return connection2.InnerConnection;
            }
            return null;
        }

        protected override int GetObjectId(DbConnection connection)
        {
            OdbcConnection connection2 = connection as OdbcConnection;
            if (connection2 != null)
            {
                return connection2.ObjectID;
            }
            return 0;
        }

        internal override void PermissionDemand(DbConnection outerConnection)
        {
            OdbcConnection connection = outerConnection as OdbcConnection;
            if (connection != null)
            {
                connection.PermissionDemand();
            }
        }

        internal override void SetConnectionPoolGroup(DbConnection outerConnection, DbConnectionPoolGroup poolGroup)
        {
            OdbcConnection connection = outerConnection as OdbcConnection;
            if (connection != null)
            {
                connection.PoolGroup = poolGroup;
            }
        }

        internal override void SetInnerConnectionEvent(DbConnection owningObject, DbConnectionInternal to)
        {
            OdbcConnection connection = owningObject as OdbcConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionEvent(to);
            }
        }

        internal override bool SetInnerConnectionFrom(DbConnection owningObject, DbConnectionInternal to, DbConnectionInternal from)
        {
            OdbcConnection connection = owningObject as OdbcConnection;
            return ((connection != null) && connection.SetInnerConnectionFrom(to, from));
        }

        internal override void SetInnerConnectionTo(DbConnection owningObject, DbConnectionInternal to)
        {
            OdbcConnection connection = owningObject as OdbcConnection;
            if (connection != null)
            {
                connection.SetInnerConnectionTo(to);
            }
        }

        public override DbProviderFactory ProviderFactory
        {
            get
            {
                return OdbcFactory.Instance;
            }
        }
    }
}

