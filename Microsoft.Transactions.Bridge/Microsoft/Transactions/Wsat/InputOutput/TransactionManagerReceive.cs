namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.Recovery;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal class TransactionManagerReceive : IProtocolProviderCoordinatorService, IProtocolProviderPropagationService
    {
        private ProtocolState state;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionManagerReceive(ProtocolState state)
        {
            this.state = state;
        }

        public void Begin(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void Commit(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            ParticipantEnlistment protocolProviderContext = (ParticipantEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmCommitEvent(protocolProviderContext, callback, obj));
        }

        public void EnlistPrePrepare(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            CoordinatorEnlistment protocolProviderContext = (CoordinatorEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmEnlistPrePrepareEvent(protocolProviderContext, callback, obj));
        }

        public void Forget(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            ParticipantEnlistment protocolProviderContext = enlistment.ProtocolProviderContext as ParticipantEnlistment;
            if (protocolProviderContext != null)
            {
                protocolProviderContext.StateMachine.Enqueue(new TmParticipantForgetEvent(protocolProviderContext, callback, obj));
            }
            else
            {
                CoordinatorEnlistment coordinator = (CoordinatorEnlistment) enlistment.ProtocolProviderContext;
                coordinator.StateMachine.Enqueue(new TmCoordinatorForgetEvent(coordinator, callback, obj));
            }
        }

        public void MarshalTransaction(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void Prepare(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            ParticipantEnlistment protocolProviderContext = (ParticipantEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmPrepareEvent(protocolProviderContext, callback, obj));
        }

        public void PrePrepare(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            ParticipantEnlistment protocolProviderContext = (ParticipantEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmPrePrepareEvent(protocolProviderContext, callback, obj));
        }

        public void Pull(Enlistment enlistment, byte[] protocolInformation, ProtocolProviderCallback callback, object obj)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void Push(Enlistment enlistment, byte[] protocolInformation, ProtocolProviderCallback callback, object obj)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void RecoveryBeginning()
        {
            DebugTrace.TraceEnter(this, "RecoveryBeginning");
            try
            {
                this.state.RecoveryBeginning();
            }
            catch (Exception exception)
            {
                DebugTrace.Trace(TraceLevel.Error, "RecoveryBeginning failed: {0}", exception);
                ProtocolRecoveryBeginningFailureRecord.TraceAndLog(PluggableProtocol.Id(this.state.ProtocolVersion), PluggableProtocol.Name(this.state.ProtocolVersion), exception);
                throw;
            }
            finally
            {
                DebugTrace.TraceLeave(this, "RecoveryBeginning");
            }
        }

        public void RecoveryComplete()
        {
            DebugTrace.TraceEnter(this, "RecoveryComplete");
            try
            {
                this.state.RecoveryComplete();
                ProtocolRecoveryCompleteRecord.TraceAndLog(PluggableProtocol.Id(this.state.ProtocolVersion), PluggableProtocol.Name(this.state.ProtocolVersion));
            }
            catch (Exception exception)
            {
                DebugTrace.Trace(TraceLevel.Error, "RecoveryComplete failed: {0}", exception);
                ProtocolRecoveryCompleteFailureRecord.TraceAndLog(PluggableProtocol.Id(this.state.ProtocolVersion), PluggableProtocol.Name(this.state.ProtocolVersion), exception);
                throw;
            }
            finally
            {
                DebugTrace.TraceLeave(this, "RecoveryComplete");
            }
        }

        public void Rejoin(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            DebugTrace.TraceEnter(this, "Rejoin");
            ParticipantEnlistment participant = null;
            try
            {
                participant = this.state.LogEntrySerialization.DeserializeParticipant(enlistment);
            }
            catch (SerializationException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Critical);
                if (DebugTrace.Error)
                {
                    DebugTrace.Trace(TraceLevel.Error, "Failed to deserialize log entry for participant: {0}", exception);
                }
                ParticipantRecoveryLogEntryCorruptRecord.TraceAndLog(enlistment.LocalTransactionId, enlistment.RemoteTransactionId, enlistment.GetRecoveryData(), exception);
                DiagnosticUtility.FailFast("A participant recovery log entry could not be deserialized. This is a fatal condition.");
            }
            if (ParticipantRecoveredRecord.ShouldTrace)
            {
                ParticipantRecoveredRecord.Trace(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId, (participant.ParticipantProxy != null) ? participant.ParticipantProxy.To : null, this.state.ProtocolVersion);
            }
            participant.StateMachine.Enqueue(new TmRejoinEvent(participant, callback, obj));
            DebugTrace.TraceLeave(this, "Rejoin");
        }

        public void Replay(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            DebugTrace.TraceEnter(this, "Replay");
            CoordinatorEnlistment coordinator = null;
            try
            {
                coordinator = this.state.LogEntrySerialization.DeserializeCoordinator(enlistment);
            }
            catch (SerializationException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Critical);
                if (DebugTrace.Error)
                {
                    DebugTrace.Trace(TraceLevel.Error, "Failed to deserialize log entry for coordinator: {0}", exception);
                }
                CoordinatorRecoveryLogEntryCorruptRecord.TraceAndLog(enlistment.LocalTransactionId, enlistment.RemoteTransactionId, enlistment.GetRecoveryData(), exception);
                DiagnosticUtility.FailFast("A coordinator recovery log entry could not be deserialized. This is a fatal condition.");
            }
            if (CoordinatorRecoveredRecord.ShouldTrace)
            {
                CoordinatorRecoveredRecord.Trace(coordinator.EnlistmentId, coordinator.Enlistment.RemoteTransactionId, (coordinator.CoordinatorProxy != null) ? coordinator.CoordinatorProxy.To : null, this.state.ProtocolVersion);
            }
            coordinator.StateMachine.Enqueue(new TmReplayEvent(coordinator, callback, obj));
            DebugTrace.TraceLeave(this, "Replay");
        }

        public void Rollback(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            ParticipantEnlistment protocolProviderContext = enlistment.ProtocolProviderContext as ParticipantEnlistment;
            if (protocolProviderContext != null)
            {
                protocolProviderContext.StateMachine.Enqueue(new TmRollbackEvent(protocolProviderContext, callback, obj));
            }
            else
            {
                TransactionEnlistment enlistment3 = (TransactionEnlistment) enlistment.ProtocolProviderContext;
                enlistment3.StateMachine.Enqueue(new TmAsyncRollbackEvent(enlistment3, callback, obj));
            }
        }

        public void SinglePhaseCommit(Enlistment enlistment, ProtocolProviderCallback callback, object obj)
        {
            ParticipantEnlistment protocolProviderContext = (ParticipantEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmSinglePhaseCommitEvent(protocolProviderContext, callback, obj));
        }
    }
}

