namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableAborted : TerminalState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableAborted(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            if (ParticipantStateMachineFinishedRecord.ShouldTrace)
            {
                ParticipantStateMachineFinishedRecord.Trace(stateMachine.Enlistment.EnlistmentId, stateMachine.Enlistment.Enlistment.RemoteTransactionId, TransactionOutcome.Aborted);
            }
        }

        public override void OnEvent(MsgAbortedEvent e)
        {
        }

        public override void OnEvent(MsgPreparedEvent e)
        {
            base.state.TwoPhaseCommitCoordinator.SendRollback(e.ReplyTo);
        }

        public override void OnEvent(MsgReadOnlyEvent e)
        {
        }

        public override void OnEvent(MsgReplayEvent e)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(base.state.ProtocolVersion, typeof(DurableAborted), "OnEvent(replay)");
            base.state.TwoPhaseCommitCoordinator.SendRollback(e.ReplyTo);
        }

        public override void OnEvent(TimerParticipantEvent e)
        {
        }

        public override void OnEvent(TmParticipantForgetEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.ForgetResponse(participant, Status.Success);
        }

        public override void OnEvent(TmPrepareEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Aborted(participant);
        }

        public override void OnEvent(TmRollbackEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Aborted(participant);
        }

        public override void OnEvent(TmRollbackResponseEvent e)
        {
            if (e.Status != Status.Aborted)
            {
                DiagnosticUtility.FailFast("Transaction manager should respond Aborted to Rollback");
            }
        }
    }
}

