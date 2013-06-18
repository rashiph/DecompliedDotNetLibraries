namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    public sealed class OleDbFactory : DbProviderFactory
    {
        public static readonly OleDbFactory Instance = new OleDbFactory();

        private OleDbFactory()
        {
        }

        public override DbCommand CreateCommand()
        {
            return new OleDbCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new OleDbCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new OleDbConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new OleDbConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new OleDbDataAdapter();
        }

        public override DbParameter CreateParameter()
        {
            return new OleDbParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return new OleDbPermission(state);
        }
    }
}

