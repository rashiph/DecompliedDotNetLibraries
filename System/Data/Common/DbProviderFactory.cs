namespace System.Data.Common
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    public abstract class DbProviderFactory
    {
        protected DbProviderFactory()
        {
        }

        public virtual DbCommand CreateCommand()
        {
            return null;
        }

        public virtual DbCommandBuilder CreateCommandBuilder()
        {
            return null;
        }

        public virtual DbConnection CreateConnection()
        {
            return null;
        }

        public virtual DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return null;
        }

        public virtual DbDataAdapter CreateDataAdapter()
        {
            return null;
        }

        public virtual DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return null;
        }

        public virtual DbParameter CreateParameter()
        {
            return null;
        }

        public virtual CodeAccessPermission CreatePermission(PermissionState state)
        {
            return null;
        }

        public virtual bool CanCreateDataSourceEnumerator
        {
            get
            {
                return false;
            }
        }
    }
}

