namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class VolatileCommitting : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public VolatileCommitting(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            stateMachine.StartTimer(TimerProfile.VolatileOutcomeAssurance);
        }

        public override void Leave(StateMachine stateMachine)
        {
            base.Leave(stateMachine);
            stateMachine.CancelTimer();
        }

        public override void OnEvent(MsgCommittedEvent e)
        {
            e.StateMachine.ChangeState(base.state.States.VolatileCommitted);
        }

        public override void OnEvent(MsgParticipantFaultEvent e)
        {
            e.StateMachine.ChangeState(base.state.States.VolatileCommitted);
        }

        public override void OnEvent(MsgParticipantSendFailureEvent e)
        {
            e.StateMachine.ChangeState(base.state.States.VolatileCommitted);
        }

        public override void OnEvent(MsgPreparedEvent e)
        {
            base.state.TwoPhaseCommitCoordinator.SendCommit(e.Participant);
        }

        public override void OnEvent(TimerParticipantEvent e)
        {
            if (e.Profile == TimerProfile.VolatileOutcomeAssurance)
            {
                ParticipantEnlistment participant = e.Participant;
                if (VolatileOutcomeTimeoutRecord.ShouldTrace)
                {
                    VolatileOutcomeTimeoutRecord.Trace(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId, TransactionOutcome.Committed, base.state.Config.VolatileOutcomePolicy.InitialDelay);
                }
                base.state.TwoPhaseCommitCoordinator.SendCommit(participant);
                e.StateMachine.ChangeState(base.state.States.VolatileCommitted);
            }
        }
    }
}

