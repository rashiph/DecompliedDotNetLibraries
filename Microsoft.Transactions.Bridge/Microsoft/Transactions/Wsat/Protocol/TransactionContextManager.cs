namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class TransactionContextManager : TransactionEnlistment
    {
        private Microsoft.Transactions.Wsat.Protocol.TransactionContext context;
        private Microsoft.Transactions.Wsat.Messaging.Fault fault;
        private string identifier;
        private Queue<TransactionContextEnlistTransactionEvent> requests;

        public TransactionContextManager(ProtocolState state, string identifier) : base(state)
        {
            this.identifier = identifier;
            this.requests = new Queue<TransactionContextEnlistTransactionEvent>();
            base.stateMachine = new TransactionContextStateMachine(this);
            base.stateMachine.ChangeState(state.States.TransactionContextInitializing);
        }

        public override void OnStateMachineComplete()
        {
            base.state.Lookup.RemoveTransactionContextManager(this);
        }

        public Microsoft.Transactions.Wsat.Messaging.Fault Fault
        {
            get
            {
                if (this.fault == null)
                {
                    return base.state.Faults.CannotCreateContext;
                }
                return this.fault;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.fault = value;
            }
        }

        public string Identifier
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.identifier;
            }
        }

        public Queue<TransactionContextEnlistTransactionEvent> Requests
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.requests;
            }
        }

        public Microsoft.Transactions.Wsat.Protocol.TransactionContext TransactionContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.context = value;
            }
        }
    }
}

