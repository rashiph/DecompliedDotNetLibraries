namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Obsolete("OracleClientFactory has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260", false)]
    public sealed class OracleClientFactory : DbProviderFactory
    {
        public static readonly OracleClientFactory Instance = new OracleClientFactory();

        private OracleClientFactory()
        {
        }

        public override DbCommand CreateCommand()
        {
            return new OracleCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new OracleCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new OracleConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new OracleConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new OracleDataAdapter();
        }

        public override DbParameter CreateParameter()
        {
            return new OracleParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return new OraclePermission(state);
        }
    }
}

