namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class TransactionContextEvent : SynchronizationEvent
    {
        private TransactionContextManager contextManager;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TransactionContextEvent(TransactionContextManager contextManager) : base(contextManager)
        {
            this.contextManager = contextManager;
        }

        public TransactionContextManager ContextManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.contextManager;
            }
        }
    }
}

