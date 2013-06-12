namespace System.Data.Common
{
    using System;
    using System.ComponentModel;
    using System.Data;

    public abstract class DbCommand : Component, IDbCommand, IDisposable
    {
        protected DbCommand()
        {
        }

        public abstract void Cancel();
        protected abstract DbParameter CreateDbParameter();
        public DbParameter CreateParameter()
        {
            return this.CreateDbParameter();
        }

        protected abstract DbDataReader ExecuteDbDataReader(CommandBehavior behavior);
        public abstract int ExecuteNonQuery();
        public DbDataReader ExecuteReader()
        {
            return this.ExecuteDbDataReader(CommandBehavior.Default);
        }

        public DbDataReader ExecuteReader(CommandBehavior behavior)
        {
            return this.ExecuteDbDataReader(behavior);
        }

        public abstract object ExecuteScalar();
        public abstract void Prepare();
        IDbDataParameter IDbCommand.CreateParameter()
        {
            return this.CreateDbParameter();
        }

        IDataReader IDbCommand.ExecuteReader()
        {
            return this.ExecuteDbDataReader(CommandBehavior.Default);
        }

        IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        {
            return this.ExecuteDbDataReader(behavior);
        }

        [ResDescription("DbCommand_CommandText"), ResCategory("DataCategory_Data"), DefaultValue(""), RefreshProperties(RefreshProperties.All)]
        public abstract string CommandText { get; set; }

        [ResDescription("DbCommand_CommandTimeout"), ResCategory("DataCategory_Data")]
        public abstract int CommandTimeout { get; set; }

        [DefaultValue(1), ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), ResDescription("DbCommand_CommandType")]
        public abstract System.Data.CommandType CommandType { get; set; }

        [ResDescription("DbCommand_Connection"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResCategory("DataCategory_Data"), Browsable(false)]
        public System.Data.Common.DbConnection Connection
        {
            get
            {
                return this.DbConnection;
            }
            set
            {
                this.DbConnection = value;
            }
        }

        protected abstract System.Data.Common.DbConnection DbConnection { get; set; }

        protected abstract System.Data.Common.DbParameterCollection DbParameterCollection { get; }

        protected abstract System.Data.Common.DbTransaction DbTransaction { get; set; }

        [DesignOnly(true), EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DefaultValue(true)]
        public abstract bool DesignTimeVisible { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResCategory("DataCategory_Data"), ResDescription("DbCommand_Parameters"), Browsable(false)]
        public System.Data.Common.DbParameterCollection Parameters
        {
            get
            {
                return this.DbParameterCollection;
            }
        }

        IDbConnection IDbCommand.Connection
        {
            get
            {
                return this.DbConnection;
            }
            set
            {
                this.DbConnection = (System.Data.Common.DbConnection) value;
            }
        }

        IDataParameterCollection IDbCommand.Parameters
        {
            get
            {
                return this.DbParameterCollection;
            }
        }

        IDbTransaction IDbCommand.Transaction
        {
            get
            {
                return this.DbTransaction;
            }
            set
            {
                this.DbTransaction = (System.Data.Common.DbTransaction) value;
            }
        }

        [DefaultValue((string) null), Browsable(false), ResDescription("DbCommand_Transaction"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Data.Common.DbTransaction Transaction
        {
            get
            {
                return this.DbTransaction;
            }
            set
            {
                this.DbTransaction = value;
            }
        }

        [DefaultValue(3), ResCategory("DataCategory_Update"), ResDescription("DbCommand_UpdatedRowSource")]
        public abstract UpdateRowSource UpdatedRowSource { get; set; }
    }
}

