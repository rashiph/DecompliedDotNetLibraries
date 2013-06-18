namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class SynchronizationEvent
    {
        protected TransactionEnlistment enlistment;
        protected ProtocolState state;

        protected SynchronizationEvent(TransactionEnlistment enlistment)
        {
            this.enlistment = enlistment;
            this.state = enlistment.State;
        }

        public abstract void Execute(Microsoft.Transactions.Wsat.StateMachines.StateMachine stateMachine);
        public override string ToString()
        {
            return base.GetType().Name;
        }

        public TransactionEnlistment Enlistment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enlistment;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.StateMachine StateMachine
        {
            get
            {
                return this.enlistment.StateMachine;
            }
        }
    }
}

