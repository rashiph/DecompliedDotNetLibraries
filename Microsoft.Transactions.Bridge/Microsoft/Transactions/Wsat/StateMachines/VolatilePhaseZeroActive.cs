namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class VolatilePhaseZeroActive : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public VolatilePhaseZeroActive(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgAbortedEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Participant);
            e.StateMachine.ChangeState(base.state.States.VolatileAborted);
        }

        public override void OnEvent(MsgReadOnlyEvent e)
        {
            e.StateMachine.ChangeState(base.state.States.VolatilePhaseZeroUnregistered);
        }

        public override void OnEvent(TmPrePrepareEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TwoPhaseCommitCoordinator.SendPrepare(participant);
            e.StateMachine.ChangeState(base.state.States.VolatilePrePreparing);
        }

        public override void OnEvent(TmRollbackEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            base.state.TwoPhaseCommitCoordinator.SendRollback(participant);
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Aborted(participant);
            e.StateMachine.ChangeState(base.state.States.VolatileAborting);
        }
    }
}

