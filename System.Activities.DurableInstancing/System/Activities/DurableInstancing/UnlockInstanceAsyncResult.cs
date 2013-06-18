namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal sealed class UnlockInstanceAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        private static string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[UnlockInstance]", new object[] { "[System.Activities.DurableInstancing]" });

        public UnlockInstanceAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            UnlockInstanceCommand instancePersistenceCommand = (UnlockInstanceCommand) base.InstancePersistenceCommand;
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@instanceId",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = instancePersistenceCommand.InstanceId
            };
            sqlCommand.Parameters.Add(parameter);
            SqlParameter parameter2 = new SqlParameter {
                ParameterName = "@surrogateLockOwnerId",
                SqlDbType = SqlDbType.BigInt,
                Value = instancePersistenceCommand.SurrogateOwnerId
            };
            sqlCommand.Parameters.Add(parameter2);
            SqlParameter parameter3 = new SqlParameter {
                ParameterName = "@handleInstanceVersion",
                SqlDbType = SqlDbType.BigInt,
                Value = instancePersistenceCommand.InstanceVersion
            };
            sqlCommand.Parameters.Add(parameter3);
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
    }
}

