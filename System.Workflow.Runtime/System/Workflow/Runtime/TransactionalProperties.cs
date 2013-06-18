namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;

    internal sealed class TransactionalProperties
    {
        internal List<SchedulableItem> ItemsToBeScheduledAtCompletion;
        internal WorkflowQueuingService LocalQueuingService;
        internal System.Transactions.Transaction Transaction;
        internal System.Transactions.TransactionScope TransactionScope;
        internal TransactionProcessState TransactionState;
    }
}

