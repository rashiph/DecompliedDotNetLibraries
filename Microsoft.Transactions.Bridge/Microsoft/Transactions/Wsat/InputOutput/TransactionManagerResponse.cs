namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;

    internal static class TransactionManagerResponse
    {
        public static void CommitResponse(Enlistment enlistment, Status status, object obj)
        {
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) obj;
            coordinator.StateMachine.Enqueue(new TmCommitResponseEvent(coordinator, status));
        }

        public static void CompletionCommitResponse(Enlistment enlistment, Status status, object obj)
        {
            CompletionEnlistment completion = (CompletionEnlistment) obj;
            completion.StateMachine.Enqueue(new TmCompletionCommitResponseEvent(completion, status));
        }

        public static void CompletionRollbackResponse(Enlistment enlistment, Status status, object obj)
        {
            CompletionEnlistment completion = (CompletionEnlistment) obj;
            completion.StateMachine.Enqueue(new TmCompletionRollbackResponseEvent(completion, status));
        }

        public static void CreateTransactionResponse(Enlistment enlistment, Status status, object obj)
        {
            MsgCreateTransactionEvent e = (MsgCreateTransactionEvent) obj;
            CompletionEnlistment protocolProviderContext = (CompletionEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmCreateTransactionResponseEvent(protocolProviderContext, status, e));
        }

        public static void EnlistTransactionResponse(Enlistment enlistment, Status status, object obj)
        {
            MsgEnlistTransactionEvent e = (MsgEnlistTransactionEvent) obj;
            CoordinatorEnlistment protocolProviderContext = (CoordinatorEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmEnlistTransactionResponseEvent(protocolProviderContext, status, e));
        }

        public static void PrepareResponse(Enlistment enlistment, Status status, object obj)
        {
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) obj;
            coordinator.StateMachine.Enqueue(new TmPrepareResponseEvent(coordinator, status));
        }

        public static void PrePrepareResponse(Enlistment enlistment, Status status, object obj)
        {
            VolatileCoordinatorEnlistment coordinator = (VolatileCoordinatorEnlistment) obj;
            coordinator.StateMachine.Enqueue(new TmPrePrepareResponseEvent(coordinator, status));
        }

        public static void RegisterResponse(Enlistment enlistment, Status status, object obj)
        {
            MsgRegisterEvent source = (MsgRegisterEvent) obj;
            ParticipantEnlistment protocolProviderContext = (ParticipantEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmRegisterResponseEvent(protocolProviderContext, status, source));
        }

        public static void RollbackResponse(Enlistment enlistment, Status status, object obj)
        {
            TransactionEnlistment enlistment2 = (TransactionEnlistment) obj;
            enlistment2.StateMachine.Enqueue(new TmRollbackResponseEvent(enlistment2, status));
        }

        public static void SubordinateRegisterResponse(Enlistment enlistment, Status status, object obj)
        {
            InternalEnlistSubordinateTransactionEvent source = (InternalEnlistSubordinateTransactionEvent) obj;
            ParticipantEnlistment protocolProviderContext = (ParticipantEnlistment) enlistment.ProtocolProviderContext;
            protocolProviderContext.StateMachine.Enqueue(new TmSubordinateRegisterResponseEvent(protocolProviderContext, status, source));
        }
    }
}

