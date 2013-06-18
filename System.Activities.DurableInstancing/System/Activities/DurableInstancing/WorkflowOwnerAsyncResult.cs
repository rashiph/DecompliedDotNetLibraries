namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal abstract class WorkflowOwnerAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        public WorkflowOwnerAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            base.StoreLock.TakeModificationLock();
        }

        protected override void OnCommandCompletion()
        {
            base.StoreLock.ReturnModificationLock();
        }
    }
}

