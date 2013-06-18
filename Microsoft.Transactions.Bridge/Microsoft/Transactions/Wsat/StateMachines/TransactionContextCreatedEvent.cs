namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TransactionContextCreatedEvent : TransactionContextEvent
    {
        private Microsoft.Transactions.Wsat.Protocol.TransactionContext context;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionContextCreatedEvent(TransactionContextManager contextManager, Microsoft.Transactions.Wsat.Protocol.TransactionContext context) : base(contextManager)
        {
            this.context = context;
        }

        public override void Execute(StateMachine stateMachine)
        {
            if (DebugTrace.Info)
            {
                base.state.DebugTraceSink.OnEvent(this);
            }
            stateMachine.State.OnEvent(this);
        }

        public Microsoft.Transactions.Wsat.Protocol.TransactionContext TransactionContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
        }
    }
}

