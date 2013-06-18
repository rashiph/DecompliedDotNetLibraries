namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorAwaitingEndOfRecovery : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorAwaitingEndOfRecovery(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgDurableCommitEvent e)
        {
            base.state.TransactionManagerSend.Commit(e.Coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorCommitting);
        }

        public override void OnEvent(MsgDurablePrepareEvent e)
        {
        }

        public override void OnEvent(MsgDurableRollbackEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
        }

        public override void OnEvent(TmCoordinatorForgetEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            coordinator.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.ForgetResponse(coordinator, Status.Success);
            e.StateMachine.ChangeState(base.state.States.CoordinatorForgotten);
        }

        public override void OnEvent(TmReplayEvent e)
        {
            if (base.state.Recovering)
            {
                DiagnosticUtility.FailFast("Replay events should only be re-delivered after recovery");
            }
            e.StateMachine.ChangeState(base.state.States.CoordinatorRecovered);
        }
    }
}

