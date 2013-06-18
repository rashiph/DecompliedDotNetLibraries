namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorCommitting : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorCommitting(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgDurableCommitEvent e)
        {
        }

        public override void OnEvent(MsgDurablePrepareEvent e)
        {
        }

        public override void OnEvent(MsgDurableRollbackEvent e)
        {
            base.TraceInvalidEvent(e, false);
            base.state.TwoPhaseCommitParticipant.SendFault(e.FaultTo, e.MessageId, base.state.Faults.InconsistentInternalState);
        }

        public override void OnEvent(MsgVolatilePrepareEvent e)
        {
        }

        public override void OnEvent(TimerCoordinatorEvent e)
        {
        }

        public override void OnEvent(TmCommitResponseEvent e)
        {
            if (e.Status != Status.Committed)
            {
                DiagnosticUtility.FailFast("Transaction manager should respond Committed to Commit");
            }
            base.state.TwoPhaseCommitParticipant.SendCommitted(e.Coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorCommitted);
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
        }
    }
}

