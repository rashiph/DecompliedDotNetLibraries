namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal sealed class DetectRunnableInstancesAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[DetectRunnableInstances]", new object[] { "[System.Activities.DurableInstancing]" });

        public DetectRunnableInstancesAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@workflowHostType",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = base.Store.WorkflowHostType
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
                bool flag = !reader.IsDBNull(1);
                TimeSpan? taskIntervalOverride = null;
                bool flag2 = false;
                if (flag)
                {
                    DateTime dateTime = reader.GetDateTime(1);
                    DateTime time2 = reader.GetDateTime(2);
                    if (dateTime <= time2)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        taskIntervalOverride = new TimeSpan?(dateTime.Subtract(time2));
                    }
                }
                if (flag2)
                {
                    base.Store.UpdateEventStatus(true, InstancePersistenceEvent<HasRunnableWorkflowEvent>.Value);
                    return nextResultSet;
                }
                base.Store.UpdateEventStatus(false, InstancePersistenceEvent<HasRunnableWorkflowEvent>.Value);
                base.StoreLock.InstanceDetectionTask.ResetTimer(false, taskIntervalOverride);
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

