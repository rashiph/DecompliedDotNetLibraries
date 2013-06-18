namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Data.Sql;
    using System.Security;
    using System.Security.Permissions;

    public sealed class SqlClientFactory : DbProviderFactory, IServiceProvider
    {
        public static readonly SqlClientFactory Instance = new SqlClientFactory();

        private SqlClientFactory()
        {
        }

        public override DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new SqlCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new SqlConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new SqlConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return SqlDataSourceEnumerator.Instance;
        }

        public override DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return new SqlClientPermission(state);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            object obj2 = null;
            if (serviceType == GreenMethods.SystemDataCommonDbProviderServices_Type)
            {
                obj2 = GreenMethods.SystemDataSqlClientSqlProviderServices_Instance();
            }
            return obj2;
        }

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                return true;
            }
        }
    }
}

