namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableCommitting : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableCommitting(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            ((ParticipantEnlistment) stateMachine.Enlistment).LastMessageTime = Microsoft.Transactions.Wsat.StateMachines.State.QueryStartTime();
            stateMachine.StartTimer(TimerProfile.Committing);
        }

        public override void Leave(StateMachine stateMachine)
        {
            base.Leave(stateMachine);
            long num = Microsoft.Transactions.Wsat.StateMachines.State.QueryStopTime() - ((ParticipantEnlistment) stateMachine.Enlistment).LastMessageTime;
            base.state.Perf.AverageParticipantCommitResponseTimeBase.Increment();
            base.state.Perf.AverageParticipantCommitResponseTime.IncrementBy(num);
            stateMachine.CancelTimer();
        }

        public override void OnEvent(MsgCommittedEvent e)
        {
            base.state.TransactionManagerSend.Committed(e.Participant);
            e.StateMachine.ChangeState(base.state.States.DurableCommitted);
        }

        public override void OnEvent(MsgPreparedEvent e)
        {
            base.state.TwoPhaseCommitCoordinator.SendCommit(e.Participant);
        }

        public override void OnEvent(MsgReplayEvent e)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(base.state.ProtocolVersion, typeof(DurableCommitting), "OnEvent(replay)");
            base.state.TwoPhaseCommitCoordinator.SendCommit(e.Participant);
        }

        public override void OnEvent(TimerParticipantEvent e)
        {
            if (e.Profile == TimerProfile.Committing)
            {
                ParticipantEnlistment participant = e.Participant;
                participant.Retries++;
                if (CommitMessageRetryRecord.ShouldTrace)
                {
                    CommitMessageRetryRecord.Trace(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId, participant.Retries);
                }
                base.state.Perf.CommitRetryCountPerInterval.Increment();
                base.state.TwoPhaseCommitCoordinator.SendCommit(participant);
            }
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

