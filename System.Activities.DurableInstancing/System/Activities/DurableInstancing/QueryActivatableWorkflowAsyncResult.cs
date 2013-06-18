namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal sealed class QueryActivatableWorkflowAsyncResult : DetectActivatableWorkflowsAsyncResult
    {
        private static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[GetActivatableWorkflowsActivationParameters]", new object[] { "[System.Activities.DurableInstancing]" });

        public QueryActivatableWorkflowAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception nextResultSet = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
            if (nextResultSet == null)
            {
                reader.NextResult();
                List<IDictionary<XName, object>> parameters = new List<IDictionary<XName, object>>();
                if (reader.Read())
                {
                    do
                    {
                        IDictionary<XName, object> item = new Dictionary<XName, object>();
                        item.Add(WorkflowServiceNamespace.SiteName, reader.GetString(0));
                        item.Add(WorkflowServiceNamespace.RelativeApplicationPath, reader.GetString(1));
                        item.Add(WorkflowServiceNamespace.RelativeServicePath, reader.GetString(2));
                        parameters.Add(item);
                    }
                    while (reader.Read());
                }
                else
                {
                    base.Store.UpdateEventStatus(false, InstancePersistenceEvent<HasActivatableWorkflowEvent>.Value);
                    base.StoreLock.InstanceDetectionTask.ResetTimer(false);
                }
                base.InstancePersistenceContext.QueriedInstanceStore(new ActivatableWorkflowsQueryResult(parameters));
            }
            return nextResultSet;
        }
    }
}

