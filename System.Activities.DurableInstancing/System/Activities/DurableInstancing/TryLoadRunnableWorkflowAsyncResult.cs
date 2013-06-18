namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal sealed class TryLoadRunnableWorkflowAsyncResult : LoadWorkflowAsyncResult
    {
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[TryLoadRunnableInstance]", new object[] { "[System.Activities.DurableInstancing]" });

        public TryLoadRunnableWorkflowAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
            if (base.Store.WorkflowHostType == Guid.Empty)
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(command.Name, System.Activities.DurableInstancing.SR.TryLoadRequiresWorkflowType, null));
            }
        }

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            double totalMilliseconds = base.TimeoutHelper.RemainingTime().TotalMilliseconds;
            SqlParameter parameter = new SqlParameter {
                ParameterName = "@surrogateLockOwnerId",
                SqlDbType = SqlDbType.BigInt,
                Value = base.StoreLock.SurrogateLockOwnerId
            };
            command.Parameters.Add(parameter);
            SqlParameter parameter2 = new SqlParameter {
                ParameterName = "@workflowHostType",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Value = base.Store.WorkflowHostType
            };
            command.Parameters.Add(parameter2);
            SqlParameter parameter3 = new SqlParameter {
                ParameterName = "@operationType",
                SqlDbType = SqlDbType.TinyInt,
                Value = LoadType.LoadByInstance
            };
            command.Parameters.Add(parameter3);
            SqlParameter parameter4 = new SqlParameter {
                ParameterName = "@handleInstanceVersion",
                SqlDbType = SqlDbType.BigInt,
                Value = base.InstancePersistenceContext.InstanceVersion
            };
            command.Parameters.Add(parameter4);
            SqlParameter parameter5 = new SqlParameter {
                ParameterName = "@handleIsBoundToLock",
                SqlDbType = SqlDbType.Bit,
                Value = base.InstancePersistenceContext.InstanceView.IsBoundToLock
            };
            command.Parameters.Add(parameter5);
            SqlParameter parameter6 = new SqlParameter {
                ParameterName = "@encodingOption",
                SqlDbType = SqlDbType.TinyInt,
                Value = base.Store.InstanceEncodingOption
            };
            command.Parameters.Add(parameter6);
            SqlParameter parameter7 = new SqlParameter {
                ParameterName = "@operationTimeout",
                SqlDbType = SqlDbType.Int,
                Value = (totalMilliseconds < 2147483647.0) ? Convert.ToInt32(totalMilliseconds) : 0x7fffffff
            };
            command.Parameters.Add(parameter7);
        }

        protected override string GetSqlCommandText()
        {
            return commandText;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception nextResultSet = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
            if ((nextResultSet == null) && (!reader.GetBoolean(1) || (base.ProcessSqlResult(reader) != null)))
            {
                base.Store.UpdateEventStatus(false, InstancePersistenceEvent<HasRunnableWorkflowEvent>.Value);
                base.StoreLock.InstanceDetectionTask.ResetTimer(false);
            }
            return nextResultSet;
        }
    }
}

