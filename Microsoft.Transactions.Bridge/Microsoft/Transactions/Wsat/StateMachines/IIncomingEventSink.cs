namespace Microsoft.Transactions.Wsat.StateMachines
{
    using System;

    internal interface IIncomingEventSink
    {
        void OnEvent(InternalEnlistSubordinateTransactionEvent e);
        void OnEvent(MsgAbortedEvent e);
        void OnEvent(MsgCommittedEvent e);
        void OnEvent(MsgCompletionCommitEvent e);
        void OnEvent(MsgCompletionRollbackEvent e);
        void OnEvent(MsgCreateTransactionEvent e);
        void OnEvent(MsgDurableCommitEvent e);
        void OnEvent(MsgDurableCoordinatorFaultEvent e);
        void OnEvent(MsgDurableCoordinatorSendFailureEvent e);
        void OnEvent(MsgDurablePrepareEvent e);
        void OnEvent(MsgDurableRollbackEvent e);
        void OnEvent(MsgEnlistTransactionEvent e);
        void OnEvent(MsgParticipantFaultEvent e);
        void OnEvent(MsgParticipantSendFailureEvent e);
        void OnEvent(MsgPreparedEvent e);
        void OnEvent(MsgReadOnlyEvent e);
        void OnEvent(MsgRegisterCompletionEvent e);
        void OnEvent(MsgRegisterDurableResponseEvent e);
        void OnEvent(MsgRegisterVolatileResponseEvent e);
        void OnEvent(MsgRegistrationCoordinatorFaultEvent e);
        void OnEvent(MsgRegistrationCoordinatorSendFailureEvent e);
        void OnEvent(MsgReplayEvent e);
        void OnEvent(MsgVolatileCommitEvent e);
        void OnEvent(MsgVolatileCoordinatorFaultEvent e);
        void OnEvent(MsgVolatileCoordinatorSendFailureEvent e);
        void OnEvent(MsgVolatilePrepareEvent e);
        void OnEvent(MsgVolatileRollbackEvent e);
        void OnEvent(TimerCoordinatorEvent e);
        void OnEvent(TimerParticipantEvent e);
        void OnEvent(TmAsyncRollbackEvent e);
        void OnEvent(TmCommitEvent e);
        void OnEvent(TmCommitResponseEvent e);
        void OnEvent(TmCompletionCommitResponseEvent e);
        void OnEvent(TmCompletionRollbackResponseEvent e);
        void OnEvent(TmCoordinatorForgetEvent e);
        void OnEvent(TmCreateTransactionResponseEvent e);
        void OnEvent(TmEnlistPrePrepareEvent e);
        void OnEvent(TmEnlistTransactionResponseEvent e);
        void OnEvent(TmParticipantForgetEvent e);
        void OnEvent(TmPrepareEvent e);
        void OnEvent(TmPrepareResponseEvent e);
        void OnEvent(TmPrePrepareEvent e);
        void OnEvent(TmPrePrepareResponseEvent e);
        void OnEvent(TmRegisterResponseEvent e);
        void OnEvent(TmRejoinEvent e);
        void OnEvent(TmReplayEvent e);
        void OnEvent(TmRollbackEvent e);
        void OnEvent(TmRollbackResponseEvent e);
        void OnEvent(TmSinglePhaseCommitEvent e);
        void OnEvent(TmSubordinateRegisterResponseEvent e);
        void OnEvent(TransactionContextCreatedEvent e);
        void OnEvent(TransactionContextEnlistTransactionEvent e);
        void OnEvent(TransactionContextTransactionDoneEvent e);
    }
}

