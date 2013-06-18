namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorAborted : TerminalState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorAborted(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            if (CoordinatorStateMachineFinishedRecord.ShouldTrace)
            {
                CoordinatorStateMachineFinishedRecord.Trace(stateMachine.Enlistment.EnlistmentId, stateMachine.Enlistment.Enlistment.RemoteTransactionId, TransactionOutcome.Aborted);
            }
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) stateMachine.Enlistment;
            base.TrySendAborted(coordinator);
            if (coordinator.RegisterVolatileCoordinator != null)
            {
                base.TrySendAborted(coordinator.RegisterVolatileCoordinator);
            }
            if (coordinator.PreparingVolatileCoordinator != null)
            {
                base.TrySendAborted(coordinator.PreparingVolatileCoordinator);
            }
            base.Enter(stateMachine);
        }

        public override void OnEvent(MsgDurableCommitEvent e)
        {
            base.TraceInvalidEvent(e, false);
            base.state.TwoPhaseCommitParticipant.SendFault(e.FaultTo, e.MessageId, base.state.Faults.InconsistentInternalState);
        }

        public override void OnEvent(MsgDurablePrepareEvent e)
        {
            base.state.TwoPhaseCommitParticipant.SendAborted(e.ReplyTo);
        }

        public override void OnEvent(MsgDurableRollbackEvent e)
        {
            base.state.TwoPhaseCommitParticipant.SendAborted(e.ReplyTo);
        }

        public override void OnEvent(MsgRegisterDurableResponseEvent e)
        {
        }

        public override void OnEvent(MsgRegisterVolatileResponseEvent e)
        {
        }

        public override void OnEvent(MsgVolatilePrepareEvent e)
        {
            base.state.TwoPhaseCommitParticipant.SendAborted(e.ReplyTo);
        }

        public override void OnEvent(MsgVolatileRollbackEvent e)
        {
            base.state.TwoPhaseCommitParticipant.SendAborted(e.ReplyTo);
        }

        public override void OnEvent(TimerCoordinatorEvent e)
        {
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) e.Enlistment;
            coordinator.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Aborted(coordinator);
        }

        public override void OnEvent(TmCoordinatorForgetEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            coordinator.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.ForgetResponse(coordinator, Status.Success);
        }

        public override void OnEvent(TmEnlistPrePrepareEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            coordinator.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.EnlistPrePrepareResponse(coordinator, Status.Aborted);
        }

        public override void OnEvent(TmPrepareResponseEvent e)
        {
        }

        public override void OnEvent(TmPrePrepareResponseEvent e)
        {
        }

        public override void OnEvent(TmReplayEvent e)
        {
        }

        public override void OnEvent(TmRollbackResponseEvent e)
        {
        }
    }
}

