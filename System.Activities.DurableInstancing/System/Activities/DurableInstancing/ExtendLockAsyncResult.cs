namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal sealed class ExtendLockAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[ExtendLock]", new object[] { "[System.Activities.DurableInstancing]" });

        public ExtendLockAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            long surrogateLockOwnerId = base.StoreLock.SurrogateLockOwnerId;
            double totalSeconds = base.Store.BufferedHostLockRenewalPeriod.TotalSeconds;
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@surrogateLockOwnerId",
                SqlDbType = SqlDbType.BigInt,
                Value = surrogateLockOwnerId
            };
            sqlCommand.Parameters.Add(parameter);
            SqlParameter parameter2 = new SqlParameter {
                ParameterName = "@lockTimeout",
                SqlDbType = SqlDbType.Int,
                Value = totalSeconds
            };
            sqlCommand.Parameters.Add(parameter2);
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

