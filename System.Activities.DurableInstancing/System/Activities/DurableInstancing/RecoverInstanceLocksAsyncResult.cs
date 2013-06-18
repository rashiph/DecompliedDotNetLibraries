namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal sealed class RecoverInstanceLocksAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[RecoverInstanceLocks]", new object[] { "[System.Activities.DurableInstancing]" });

        public RecoverInstanceLocksAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
        }

        protected override string GetSqlCommandText()
        {
            return commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            return StoreUtilities.CheckRemainingResultSetForErrors(base.InstancePersistenceCommand.Name, reader);
        }

        protected override string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(base.Store.CachedConnectionString) {
                    ApplicationName = "System.Activities.DurableInstancing.SqlWorkflowInstanceStore"
                };
                return builder.ToString();
            }
        }
    }
}

