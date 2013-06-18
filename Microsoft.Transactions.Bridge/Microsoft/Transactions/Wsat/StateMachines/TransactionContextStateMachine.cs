namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TransactionContextStateMachine : StateMachine
    {
        private TransactionContextManager contextManager;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionContextStateMachine(TransactionContextManager contextManager) : base(contextManager)
        {
            this.contextManager = contextManager;
        }

        public override Microsoft.Transactions.Wsat.StateMachines.State AbortedState
        {
            get
            {
                return base.state.States.TransactionContextFinished;
            }
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

