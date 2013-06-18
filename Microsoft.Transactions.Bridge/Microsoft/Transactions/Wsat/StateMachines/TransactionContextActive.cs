namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TransactionContextActive : InactiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionContextActive(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            TransactionContextStateMachine machine = (TransactionContextStateMachine) stateMachine;
            TransactionContextManager contextManager = machine.ContextManager;
            foreach (TransactionContextEnlistTransactionEvent event2 in contextManager.Requests)
            {
                base.state.ActivationCoordinator.SendCreateCoordinationContextResponse(contextManager.TransactionContext, event2.Result);
            }
            contextManager.Requests.Clear();
        }

        public override void OnEvent(TransactionContextEnlistTransactionEvent e)
        {
            base.state.ActivationCoordinator.SendCreateCoordinationContextResponse(e.ContextManager.TransactionContext, e.Result);
        }

        public override void OnEvent(TransactionContextTransactionDoneEvent e)
        {
            e.StateMachine.ChangeState(base.state.States.TransactionContextFinished);
        }
    }
}

