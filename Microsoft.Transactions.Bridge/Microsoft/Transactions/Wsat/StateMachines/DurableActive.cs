namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableActive : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableActive(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgAbortedEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Participant);
            e.StateMachine.ChangeState(base.state.States.DurableAborted);
        }

        public override void OnEvent(MsgReadOnlyEvent e)
        {
            e.StateMachine.ChangeState(base.state.States.DurableUnregistered);
        }

        public override void OnEvent(MsgReplayEvent e)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(base.state.ProtocolVersion, typeof(DurableActive), "OnEvent(replay)");
            ParticipantEnlistment participant = e.Participant;
            base.state.TwoPhaseCommitCoordinator.SendRollback(participant);
            base.state.TransactionManagerSend.Rollback(participant);
            e.StateMachine.ChangeState(base.state.States.DurableAborted);
        }

        public override void OnEvent(TmPrepareEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TwoPhaseCommitCoordinator.SendPrepare(participant);
            e.StateMachine.ChangeState(base.state.States.DurablePreparing);
        }

        public override void OnEvent(TmRollbackEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            base.state.TwoPhaseCommitCoordinator.SendRollback(participant);
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Aborted(participant);
            e.StateMachine.ChangeState(base.state.States.DurableAborted);
        }
    }
}

