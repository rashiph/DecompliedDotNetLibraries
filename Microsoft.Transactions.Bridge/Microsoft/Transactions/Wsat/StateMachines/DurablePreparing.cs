namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.Recovery;
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal class DurablePreparing : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurablePreparing(ProtocolState state) : base(state)
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
            base.state.TransactionManagerSend.Aborted(e.Participant);
            e.StateMachine.ChangeState(base.state.States.DurableAborted);
        }

        public override void OnEvent(MsgPreparedEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            Exception exception = null;
            try
            {
                byte[] data = base.state.LogEntrySerialization.Serialize(participant);
                participant.Enlistment.SetRecoveryData(data);
                base.state.TransactionManagerSend.Prepared(participant);
                e.StateMachine.ChangeState(base.state.States.DurablePrepared);
            }
            catch (SerializationException exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                exception = exception2;
            }
            if (exception != null)
            {
                if (DebugTrace.Error)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Failed to serialize log entry for participant: {0}", exception);
                }
                ParticipantRecoveryLogEntryCreationFailureRecord.TraceAndLog(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId, exception.Message, exception);
                base.state.TwoPhaseCommitCoordinator.SendRollback(participant);
                base.state.TransactionManagerSend.Aborted(participant);
                e.StateMachine.ChangeState(base.state.States.DurableAborted);
            }
        }

        public override void OnEvent(MsgReadOnlyEvent e)
        {
            base.state.TransactionManagerSend.ReadOnly(e.Participant);
            e.StateMachine.ChangeState(base.state.States.DurableInDoubt);
        }

        public override void OnEvent(MsgReplayEvent e)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(base.state.ProtocolVersion, typeof(DurablePreparing), "OnEvent(replay)");
            ParticipantEnlistment participant = e.Participant;
            if (DurableParticipantReplayWhilePreparingRecord.ShouldTrace)
            {
                DurableParticipantReplayWhilePreparingRecord.Trace(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId);
            }
            base.state.TwoPhaseCommitCoordinator.SendRollback(participant);
            base.state.TransactionManagerSend.Aborted(participant);
            e.StateMachine.ChangeState(base.state.States.DurableAborted);
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
            e.StateMachine.ChangeState(base.state.States.DurableAborted);
        }
    }
}

