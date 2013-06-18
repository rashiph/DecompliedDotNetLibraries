namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorEnlisting : InactiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorEnlisting(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(TmEnlistTransactionResponseEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            MsgEnlistTransactionEvent sourceEvent = e.SourceEvent;
            Status status = e.Status;
            if (status == Status.Success)
            {
                if (EnlistTransactionRecord.ShouldTrace)
                {
                    EnlistTransactionRecord.Trace(coordinator.EnlistmentId, sourceEvent.Body.CurrentContext);
                }
                coordinator.OnCoordinatorEnlisted();
                coordinator.StateMachine.ChangeState(base.state.States.CoordinatorEnlisted);
            }
            else if (status == Status.DuplicateTransaction)
            {
                base.ForwardEnlistmentEventToSubordinate(sourceEvent);
                coordinator.StateMachine.ChangeState(base.state.States.CoordinatorInitializationFailed);
            }
            else
            {
                if (EnlistTransactionFailureRecord.ShouldTrace)
                {
                    EnlistTransactionFailureRecord.Trace(coordinator.EnlistmentId, sourceEvent.Body.CurrentContext, Microsoft.Transactions.SR.GetString("PplCreateSuperiorEnlistmentFailed", new object[] { e.Status.ToString() }));
                }
                coordinator.ContextManager.Fault = base.state.Faults.TMEnlistFailed(e.Status);
                coordinator.StateMachine.ChangeState(base.state.States.CoordinatorInitializationFailed);
            }
        }
    }
}

