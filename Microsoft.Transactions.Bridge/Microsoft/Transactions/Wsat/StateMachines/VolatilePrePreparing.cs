namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class VolatilePrePreparing : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public VolatilePrePreparing(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            ((ParticipantEnlistment) stateMachine.Enlistment).LastMessageTime = Microsoft.Transactions.Wsat.StateMachines.State.QueryStartTime();
            stateMachine.StartTimer(TimerProfile.Preparing);
        }

        public override void Leave(StateMachine stateMachine)
        {
            base.Leave(stateMachine);
            ParticipantEnlistment enlistment = (ParticipantEnlistment) stateMachine.Enlistment;
            long num = Microsoft.Transactions.Wsat.StateMachines.State.QueryStopTime() - enlistment.LastMessageTime;
            base.state.Perf.AverageParticipantPrepareResponseTimeBase.Increment();
            base.state.Perf.AverageParticipantPrepareResponseTime.IncrementBy(num);
            enlistment.Retries = 0;
            stateMachine.CancelTimer();
        }

        public override void OnEvent(MsgAbortedEvent e)
        {
            base.state.TransactionManagerSend.PrePrepareAborted(e.Participant);
            e.StateMachine.ChangeState(base.state.States.VolatileAborted);
        }

        public override void OnEvent(MsgPreparedEvent e)
        {
            base.state.TransactionManagerSend.PrePrepared(e.Participant);
            e.StateMachine.ChangeState(base.state.States.VolatilePrePrepared);
        }

        public override void OnEvent(MsgReadOnlyEvent e)
        {
            base.state.TransactionManagerSend.PrePrepared(e.Participant);
            e.StateMachine.ChangeState(base.state.States.VolatilePhaseOneUnregistered);
        }

        public override void OnEvent(TimerParticipantEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            participant.Retries++;
            if (PrepareMessageRetryRecord.ShouldTrace)
            {
                PrepareMessageRetryRecord.Trace(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId, participant.Retries);
            }
            base.state.Perf.PrepareRetryCountPerInterval.Increment();
            base.state.TwoPhaseCommitCoordinator.SendPrepare(participant);
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

