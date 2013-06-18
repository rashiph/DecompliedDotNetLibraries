namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal class DetectActivatableWorkflowsAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[GetActivatableWorkflowsActivationParameters]", new object[] { "[System.Activities.DurableInstancing]" });

        public DetectActivatableWorkflowsAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@machineName",
                SqlDbType = SqlDbType.NVarChar,
                Value = SqlWorkflowInstanceStoreConstants.MachineName
            };
            sqlCommand.Parameters.Add(parameter);
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
            Exception nextResultSet = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
            if (nextResultSet == null)
            {
                reader.NextResult();
                if (reader.Read())
                {
                    base.Store.UpdateEventStatus(true, InstancePersistenceEvent<HasActivatableWorkflowEvent>.Value);
                    return nextResultSet;
                }
                base.Store.UpdateEventStatus(false, InstancePersistenceEvent<HasActivatableWorkflowEvent>.Value);
                base.StoreLock.InstanceDetectionTask.ResetTimer(false);
            }
            return nextResultSet;
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

