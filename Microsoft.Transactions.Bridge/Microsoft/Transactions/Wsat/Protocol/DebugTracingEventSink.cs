namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.InputOutput;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal class DebugTracingEventSink : IIncomingEventSink
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(InternalEnlistSubordinateTransactionEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgAbortedEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgCommittedEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgCompletionCommitEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgCompletionRollbackEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        public void OnEvent(MsgCreateTransactionEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0}", e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgDurableCommitEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        public void OnEvent(MsgDurableCoordinatorFaultEvent e)
        {
            this.TraceFault(e.Fault, e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgDurableCoordinatorSendFailureEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgDurablePrepareEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgDurableRollbackEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        public void OnEvent(MsgEnlistTransactionEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0}{1}", e, (e.Body.IssuedToken != null) ? " with issued token" : string.Empty);
        }

        public void OnEvent(MsgParticipantFaultEvent e)
        {
            this.TraceFault(e.Fault, e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgParticipantSendFailureEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgPreparedEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgReadOnlyEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        public void OnEvent(MsgRegisterCompletionEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} for {1} at {2}", e, ControlProtocol.Completion, Ports.TryGetAddress(e.Proxy));
        }

        public void OnEvent(MsgRegisterDurableResponseEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} from {1}", e, Ports.TryGetAddress(e.Coordinator.RegistrationProxy));
        }

        public void OnEvent(MsgRegisterVolatileResponseEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} from {1}", e, Ports.TryGetAddress(e.VolatileCoordinator.Coordinator.RegistrationProxy));
        }

        public void OnEvent(MsgRegistrationCoordinatorFaultEvent e)
        {
            this.TraceFault(e.Fault, e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgRegistrationCoordinatorSendFailureEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgReplayEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgVolatileCommitEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        public void OnEvent(MsgVolatileCoordinatorFaultEvent e)
        {
            this.TraceFault(e.Fault, e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgVolatileCoordinatorSendFailureEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgVolatilePrepareEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(MsgVolatileRollbackEvent e)
        {
            this.TraceNotificationMessage(e);
        }

        public void OnEvent(TimerCoordinatorEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} for coordinator at {1}", e, Ports.TryGetAddress(e.Coordinator.CoordinatorProxy));
        }

        public void OnEvent(TimerParticipantEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} for {1} participant at {2}", e, e.Participant.ControlProtocol, Ports.TryGetAddress(e.Participant.ParticipantProxy));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmAsyncRollbackEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmCommitEvent e)
        {
            this.TraceTmEvent(e);
        }

        public void OnEvent(TmCommitResponseEvent e)
        {
            this.TraceTmResponse(e, e.Status);
        }

        public void OnEvent(TmCompletionCommitResponseEvent e)
        {
            this.TraceTmResponse(e, e.Status);
        }

        public void OnEvent(TmCompletionRollbackResponseEvent e)
        {
            this.TraceTmResponse(e, e.Status);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmCoordinatorForgetEvent e)
        {
            this.TraceTmEvent(e);
        }

        public void OnEvent(TmCreateTransactionResponseEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} {1}", e, e.Status);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmEnlistPrePrepareEvent e)
        {
            this.TraceGenericEvent(e);
        }

        public void OnEvent(TmEnlistTransactionResponseEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} {1}", e, e.Status);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmParticipantForgetEvent e)
        {
            this.TraceTmEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmPrepareEvent e)
        {
            this.TraceTmEvent(e);
        }

        public void OnEvent(TmPrepareResponseEvent e)
        {
            this.TraceTmResponse(e, e.Status);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmPrePrepareEvent e)
        {
            this.TraceTmEvent(e);
        }

        public void OnEvent(TmPrePrepareResponseEvent e)
        {
            this.TraceTmResponse(e, e.Status);
        }

        public void OnEvent(TmRegisterResponseEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} {1} for {2}", e, e.Status, e.Participant.ControlProtocol);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmRejoinEvent e)
        {
            this.TraceTmEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmReplayEvent e)
        {
            this.TraceTmEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmRollbackEvent e)
        {
            this.TraceTmEvent(e);
        }

        public void OnEvent(TmRollbackResponseEvent e)
        {
            this.TraceTmResponse(e, e.Status);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TmSinglePhaseCommitEvent e)
        {
            this.TraceTmEvent(e);
        }

        public void OnEvent(TmSubordinateRegisterResponseEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} {1} for {2}", e, e.Status, e.Participant.ControlProtocol);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TransactionContextCreatedEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TransactionContextEnlistTransactionEvent e)
        {
            this.TraceGenericEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void OnEvent(TransactionContextTransactionDoneEvent e)
        {
            this.TraceGenericEvent(e);
        }

        private void TraceFault(MessageFault fault, SynchronizationEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} {1}: {2}", e, Library.GetFaultCodeName(fault), Library.GetFaultCodeReason(fault));
        }

        private void TraceGenericEvent(SynchronizationEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0}", e);
        }

        private void TraceNotificationMessage(CompletionEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} from {1}", e, Ports.TryGetAddress(e.Completion.ParticipantProxy));
        }

        private void TraceNotificationMessage(CoordinatorEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} from {1}", e, Ports.TryGetAddress(e.Coordinator.CoordinatorProxy));
        }

        private void TraceNotificationMessage(ParticipantEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} from {1}", e, Ports.TryGetAddress(e.Participant.ParticipantProxy));
        }

        private void TraceNotificationMessage(VolatileCoordinatorEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} from {1}", e, Ports.TryGetAddress(e.VolatileCoordinator.CoordinatorProxy));
        }

        private void TraceTmEvent(CoordinatorCallbackEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} for {1}", e, Ports.TryGetAddress(e.Coordinator.CoordinatorProxy));
        }

        private void TraceTmEvent(ParticipantCallbackEvent e)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} for {1}", e, Ports.TryGetAddress(e.Participant.ParticipantProxy));
        }

        private void TraceTmResponse(SynchronizationEvent e, Status status)
        {
            DebugTrace.TxTrace(TraceLevel.Info, e.Enlistment.EnlistmentId, "{0} {1}", e, status);
        }
    }
}

