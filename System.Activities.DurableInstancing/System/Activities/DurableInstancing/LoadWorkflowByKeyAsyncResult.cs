namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    internal sealed class LoadWorkflowByKeyAsyncResult : LoadWorkflowAsyncResult
    {
        public LoadWorkflowByKeyAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, SqlWorkflowInstanceStore store, SqlWorkflowInstanceStoreLock storeLock, Transaction currentTransaction, TimeSpan timeout, AsyncCallback callback, object state) : base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            LoadWorkflowByInstanceKeyCommand instancePersistenceCommand = base.InstancePersistenceCommand as LoadWorkflowByInstanceKeyCommand;
            LoadType loadType = instancePersistenceCommand.AcceptUninitializedInstance ? LoadType.LoadOrCreateByKey : LoadType.LoadByKey;
            Guid lookupInstanceKey = instancePersistenceCommand.LookupInstanceKey;
            List<CorrelationKey> keysToAssociate = CorrelationKey.BuildKeyList(instancePersistenceCommand.InstanceKeysToAssociate, base.Store.InstanceEncodingOption);
            Guid associateInstanceKeyToInstanceId = instancePersistenceCommand.AssociateInstanceKeyToInstanceId;
            base.GenerateLoadSqlCommand(command, loadType, lookupInstanceKey, associateInstanceKeyToInstanceId, keysToAssociate);
        }
    }
}

