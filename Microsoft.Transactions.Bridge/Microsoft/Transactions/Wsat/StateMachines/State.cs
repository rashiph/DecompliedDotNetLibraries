namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.InputOutput;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal abstract class State : IIncomingEventSink
    {
        protected ProtocolState state;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected State(ProtocolState state)
        {
            this.state = state;
        }

        protected void EnlistPrePrepare(TmEnlistPrePrepareEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            coordinator.OnEnlistPrePrepare(e);
            this.state.RegistrationParticipant.SendVolatileRegister(coordinator.RegisterVolatileCoordinator);
        }

        public virtual void Enter(StateMachine stateMachine)
        {
        }

        protected void ForwardEnlistmentEventToSubordinate(MsgEnlistTransactionEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            TransactionContextManager contextManager = coordinator.ContextManager;
            coordinator.ContextManager = null;
            ParticipantEnlistment participant = new ParticipantEnlistment(this.state, coordinator.Enlistment, contextManager);
            participant.StateMachine.Enqueue(new InternalEnlistSubordinateTransactionEvent(participant, e));
        }

        private void InvalidCompletionMessage(CompletionParticipantEvent e)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, e.Completion);
            this.state.CompletionCoordinator.SendFault(e.FaultTo, e.MessageId, this.state.Faults.InvalidState);
        }

        private void InvalidDurableCoordinatorMessage(DurableTwoPhaseCommitCoordinatorEvent e)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, e.Coordinator);
            this.TrySendFault(e, this.state.Faults.InvalidState);
        }

        private void InvalidEventFailfast(SynchronizationEvent e)
        {
            DiagnosticUtility.FailFast(string.Format(CultureInfo.InvariantCulture, "Failfasting due to unexpected event {0} for state {1}", new object[] { e, this }));
        }

        private void InvalidFaultEvent(SynchronizationEvent e, Microsoft.Transactions.Wsat.Protocol.TransactionEnlistment enlistment, MessageFault fault)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, enlistment);
        }

        private void InvalidInternalEvent(SynchronizationEvent e)
        {
            this.TraceInvalidEvent(e, true);
            this.InvalidEventFailfast(e);
        }

        private void InvalidParticipantFaultEvent(SynchronizationEvent e, ParticipantEnlistment participant)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, participant);
        }

        private void InvalidParticipantMessage(TwoPhaseCommitParticipantEvent e)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, e.Participant);
            this.state.TwoPhaseCommitCoordinator.SendFault(e.FaultTo, e.MessageId, this.state.Faults.InvalidState);
        }

        private void InvalidRegisterCompletionMessage(MsgRegisterCompletionEvent e)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, e.Completion);
            if (RegisterParticipantFailureRecord.ShouldTrace)
            {
                RegisterParticipantFailureRecord.Trace(e.Completion.EnlistmentId, e.Completion.Enlistment.RemoteTransactionId, ControlProtocol.Completion, e.ParticipantService, Microsoft.Transactions.SR.GetString("RegisterFailureInvalidState", new object[] { e.StateMachine.State.ToString() }), this.state.ProtocolVersion);
            }
            this.state.RegistrationCoordinator.SendFault(e.Result, this.state.Faults.InvalidState);
        }

        private void InvalidRegistrationCoordinatorMessage(MsgRegisterDurableResponseEvent e)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, e.Coordinator);
        }

        private void InvalidRegistrationCoordinatorMessage(MsgRegisterVolatileResponseEvent e)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, e.VolatileCoordinator);
        }

        private void InvalidSendMessageFailureEvent(SynchronizationEvent e, Microsoft.Transactions.Wsat.Protocol.TransactionEnlistment enlistment)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, enlistment);
        }

        private void InvalidTimerEvent(SynchronizationEvent e)
        {
            this.TraceInvalidEvent(e, true);
            this.InvalidEventFailfast(e);
        }

        private void InvalidTransactionManagerEvent(SynchronizationEvent e)
        {
            this.TraceInvalidEvent(e, true);
            this.InvalidEventFailfast(e);
        }

        private void InvalidVolatileCoordinatorMessage(VolatileTwoPhaseCommitCoordinatorEvent e)
        {
            this.TraceInvalidEvent(e, false);
            this.TryToAbortTransaction(e, e.VolatileCoordinator);
            this.TrySendFault(e, this.state.Faults.InvalidState);
        }

        public virtual void Leave(StateMachine stateMachine)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(InternalEnlistSubordinateTransactionEvent e)
        {
            this.InvalidInternalEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgAbortedEvent e)
        {
            this.InvalidParticipantMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgCommittedEvent e)
        {
            this.InvalidParticipantMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgCompletionCommitEvent e)
        {
            this.InvalidCompletionMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgCompletionRollbackEvent e)
        {
            this.InvalidCompletionMessage(e);
        }

        public virtual void OnEvent(MsgCreateTransactionEvent e)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgDurableCommitEvent e)
        {
            this.InvalidDurableCoordinatorMessage(e);
        }

        public virtual void OnEvent(MsgDurableCoordinatorFaultEvent e)
        {
            this.InvalidFaultEvent(e, e.Coordinator, e.Fault);
        }

        public virtual void OnEvent(MsgDurableCoordinatorSendFailureEvent e)
        {
            this.InvalidSendMessageFailureEvent(e, e.Coordinator);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgDurablePrepareEvent e)
        {
            this.InvalidDurableCoordinatorMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgDurableRollbackEvent e)
        {
            this.InvalidDurableCoordinatorMessage(e);
        }

        public virtual void OnEvent(MsgEnlistTransactionEvent e)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public virtual void OnEvent(MsgParticipantFaultEvent e)
        {
            this.InvalidParticipantFaultEvent(e, e.Participant);
        }

        public virtual void OnEvent(MsgParticipantSendFailureEvent e)
        {
            this.InvalidSendMessageFailureEvent(e, e.Participant);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgPreparedEvent e)
        {
            this.InvalidParticipantMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgReadOnlyEvent e)
        {
            this.InvalidParticipantMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgRegisterCompletionEvent e)
        {
            this.InvalidRegisterCompletionMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgRegisterDurableResponseEvent e)
        {
            this.InvalidRegistrationCoordinatorMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgRegisterVolatileResponseEvent e)
        {
            this.InvalidRegistrationCoordinatorMessage(e);
        }

        public virtual void OnEvent(MsgRegistrationCoordinatorFaultEvent e)
        {
            if ((this.state.ProtocolVersion == ProtocolVersion.Version10) && (e.Coordinator.ContextManager != null))
            {
                Fault fault = CoordinatorRegistrationFailedFault.CreateFault(e.Fault);
                e.Coordinator.ContextManager.Fault = fault;
            }
            this.InvalidFaultEvent(e, e.Coordinator, e.Fault);
        }

        public virtual void OnEvent(MsgRegistrationCoordinatorSendFailureEvent e)
        {
            if ((this.state.ProtocolVersion == ProtocolVersion.Version10) && (e.Coordinator.ContextManager != null))
            {
                Fault fault = CoordinatorRegistrationFailedFault.CreateFault(null);
                e.Coordinator.ContextManager.Fault = fault;
            }
            this.InvalidSendMessageFailureEvent(e, e.Coordinator);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgReplayEvent e)
        {
            this.InvalidParticipantMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgVolatileCommitEvent e)
        {
            this.InvalidVolatileCoordinatorMessage(e);
        }

        public void OnEvent(MsgVolatileCoordinatorFaultEvent e)
        {
            this.InvalidFaultEvent(e, e.VolatileCoordinator, e.Fault);
        }

        public virtual void OnEvent(MsgVolatileCoordinatorSendFailureEvent e)
        {
            this.InvalidSendMessageFailureEvent(e, e.VolatileCoordinator);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgVolatilePrepareEvent e)
        {
            this.InvalidVolatileCoordinatorMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(MsgVolatileRollbackEvent e)
        {
            this.InvalidVolatileCoordinatorMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TimerCoordinatorEvent e)
        {
            this.InvalidTimerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TimerParticipantEvent e)
        {
            this.InvalidTimerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmAsyncRollbackEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmCommitEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmCommitResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmCompletionCommitResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmCompletionRollbackResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmCoordinatorForgetEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmCreateTransactionResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmEnlistPrePrepareEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmEnlistTransactionResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmParticipantForgetEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmPrepareEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmPrepareResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmPrePrepareEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmPrePrepareResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmRegisterResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmRejoinEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmReplayEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmRollbackEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmRollbackResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmSinglePhaseCommitEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TmSubordinateRegisterResponseEvent e)
        {
            this.InvalidTransactionManagerEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TransactionContextCreatedEvent e)
        {
            this.InvalidInternalEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TransactionContextEnlistTransactionEvent e)
        {
            this.InvalidInternalEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void OnEvent(TransactionContextTransactionDoneEvent e)
        {
            this.InvalidInternalEvent(e);
        }

        protected void ProcessTmAsyncRollback(TmAsyncRollbackEvent e)
        {
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) e.Enlistment;
            coordinator.SetCallback(e.Callback, e.CallbackState);
            this.state.TransactionManagerSend.Aborted(coordinator);
            e.StateMachine.ChangeState(this.state.States.CoordinatorAborted);
        }

        protected void ProcessTmRegisterResponse(TmRegisterResponseEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            MsgRegisterEvent sourceEvent = e.SourceEvent;
            if (e.Status != Status.Success)
            {
                Fault fault = this.state.Faults.ParticipantTMRegistrationFailed(e.Status);
                this.state.RegistrationCoordinator.SendFault(sourceEvent.Result, fault);
                if (RegisterParticipantFailureRecord.ShouldTrace)
                {
                    RegisterParticipantFailureRecord.Trace(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId, participant.ControlProtocol, participant.ParticipantProxy.To, Microsoft.Transactions.SR.GetString("PplCreateSubordinateEnlistmentFailed", new object[] { e.Status.ToString() }), this.state.ProtocolVersion);
                }
            }
            else
            {
                participant.OnParticipantRegistered();
                this.state.RegistrationCoordinator.SendRegisterResponse(participant, sourceEvent.Result, sourceEvent.Protocol, participant.CoordinatorService);
                if (RegisterParticipantRecord.ShouldTrace)
                {
                    RegisterParticipantRecord.Trace(participant.EnlistmentId, participant.Enlistment.RemoteTransactionId, participant.ControlProtocol, participant.ParticipantProxy.To, this.state.ProtocolVersion);
                }
            }
        }

        public static long QueryStartTime()
        {
            return QueryTime(-1L);
        }

        public static long QueryStopTime()
        {
            return QueryTime(0L);
        }

        private static long QueryTime(long failureDefault)
        {
            long time = 0L;
            if (UnsafeNativeMethods.QueryPerformanceCounter(out time) == 0)
            {
                time = failureDefault;
            }
            return time;
        }

        protected void SetDurableCoordinatorActive(MsgRegisterDurableResponseEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            coordinator.SetCoordinatorProxy(e.Proxy);
            coordinator.OnDurableCoordinatorActive();
            if (RegisterCoordinatorRecord.ShouldTrace)
            {
                RegisterCoordinatorRecord.Trace(coordinator.EnlistmentId, coordinator.SuperiorContext, ControlProtocol.Durable2PC, e.Proxy.To, this.state.ProtocolVersion);
            }
        }

        public override string ToString()
        {
            return base.GetType().Name;
        }

        protected void TraceInvalidEvent(SynchronizationEvent e, bool fatal)
        {
            e.StateMachine.TraceInvalidEvent(e, fatal);
        }

        protected void TrySendAborted(CoordinatorEnlistment coordinator)
        {
            if (coordinator.CoordinatorProxy != null)
            {
                this.state.TwoPhaseCommitParticipant.SendDurableAborted(coordinator);
            }
        }

        protected void TrySendAborted(VolatileCoordinatorEnlistment coordinator)
        {
            if (coordinator.CoordinatorProxy != null)
            {
                this.state.TwoPhaseCommitParticipant.SendVolatileAborted(coordinator);
            }
        }

        protected void TrySendFault(DurableTwoPhaseCommitCoordinatorEvent e, Fault fault)
        {
            this.state.TwoPhaseCommitParticipant.SendFault(e.FaultTo, e.MessageId, fault);
        }

        protected void TrySendFault(VolatileTwoPhaseCommitCoordinatorEvent e, Fault fault)
        {
            this.state.TwoPhaseCommitParticipant.SendFault(e.FaultTo, e.MessageId, fault);
        }

        private void TryToAbortTransaction(SynchronizationEvent e, Microsoft.Transactions.Wsat.Protocol.TransactionEnlistment enlistment)
        {
            if (this is InactiveState)
            {
                enlistment.StateMachine.ChangeState(enlistment.StateMachine.AbortedState);
            }
            else if (this is ActiveState)
            {
                this.state.TransactionManagerSend.Rollback(enlistment);
                enlistment.StateMachine.ChangeState(enlistment.StateMachine.AbortedState);
            }
        }
    }
}

