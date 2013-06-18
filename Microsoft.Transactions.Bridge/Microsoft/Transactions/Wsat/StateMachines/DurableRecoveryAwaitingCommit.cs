namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableRecoveryAwaitingCommit : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableRecoveryAwaitingCommit(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgCommittedEvent e)
        {
            e.StateMachine.ChangeState(base.state.States.DurableCommitted);
        }

        public override void OnEvent(MsgPreparedEvent e)
        {
        }

        public override void OnEvent(MsgReplayEvent e)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(base.state.ProtocolVersion, typeof(DurableRecoveryAwaitingCommit), "OnEvent(replay)");
        }

        public override void OnEvent(TmCommitEvent e)
        {
            if (base.state.Recovering)
            {
                DiagnosticUtility.FailFast("Rejoin events should only be re-delivered after recovery");
            }
            e.Participant.SetCallback(e.Callback, e.CallbackState);
            e.StateMachine.ChangeState(base.state.States.DurableRecoveryReceivedCommit);
        }

        public override void OnEvent(TmParticipantForgetEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.ForgetResponse(participant, Status.Success);
            e.StateMachine.ChangeState(base.state.States.DurableInDoubt);
        }
    }
}

