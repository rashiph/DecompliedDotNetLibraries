namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorRecovered : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorRecovered(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) stateMachine.Enlistment;
            coordinator.CreateParticipantService();
            base.state.TwoPhaseCommitParticipant.SendRecoverMessage(coordinator);
            stateMachine.StartTimer(TimerProfile.Replaying);
        }

        public override void Leave(StateMachine stateMachine)
        {
            base.Leave(stateMachine);
            stateMachine.CancelTimer();
        }

        public override void OnEvent(MsgDurableCommitEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            base.state.TransactionManagerSend.Commit(coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorCommitting);
        }

        public override void OnEvent(MsgDurableRollbackEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
        }

        public override void OnEvent(TimerCoordinatorEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            if (ReplayMessageRetryRecord.ShouldTrace)
            {
                coordinator.Retries++;
                ReplayMessageRetryRecord.Trace(coordinator.EnlistmentId, coordinator.Enlistment.RemoteTransactionId, coordinator.Retries);
            }
            base.state.Perf.ReplayRetryCountPerInterval.Increment();
            base.state.TwoPhaseCommitParticipant.SendRecoverMessage(e.Coordinator);
        }

        public override void OnEvent(TmCoordinatorForgetEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            coordinator.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.ForgetResponse(coordinator, Status.Success);
            e.StateMachine.ChangeState(base.state.States.CoordinatorForgotten);
        }
    }
}

