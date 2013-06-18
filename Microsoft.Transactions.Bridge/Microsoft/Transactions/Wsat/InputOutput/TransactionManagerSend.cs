namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;

    internal class TransactionManagerSend
    {
        private TransactionManagerCallback commitResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.CommitResponse);
        private TransactionManagerCallback completionCommitResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.CompletionCommitResponse);
        private TransactionManagerCallback completionRollbackResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.CompletionRollbackResponse);
        private TransactionManagerCoordinatorService coordination;
        private TransactionManagerCallback createTransactionResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.CreateTransactionResponse);
        private TransactionManagerCallback enlistTransactionResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.EnlistTransactionResponse);
        private TransactionManagerCallback prepareResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.PrepareResponse);
        private TransactionManagerCallback prePrepareResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.PrePrepareResponse);
        private TransactionManagerPropagationService propagation;
        private TransactionManagerCallback registerResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.RegisterResponse);
        private TransactionManagerCallback rollbackResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.RollbackResponse);
        private ProtocolState state;
        private TransactionManagerCallback subordinateRegisterResponseCallback = new TransactionManagerCallback(TransactionManagerResponse.SubordinateRegisterResponse);

        public TransactionManagerSend(ProtocolState state)
        {
            this.state = state;
            this.propagation = state.TransactionManager.PropagationService;
            this.coordination = state.TransactionManager.CoordinatorService;
        }

        public void Aborted(CompletionEnlistment completion)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, completion.EnlistmentId, "Replying Aborted to transaction manager");
            }
            completion.DeliverCallback(Status.Aborted);
        }

        public void Aborted(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Replying Aborted to transaction manager");
            }
            coordinator.DeliverCallback(Status.Aborted);
        }

        public void Aborted(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Replying Aborted to transaction manager");
            }
            participant.DeliverCallback(Status.Aborted);
        }

        public void Commit(CompletionEnlistment completion)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, completion.EnlistmentId, "Sending completion Commit to transaction manager");
            }
            this.coordination.Commit(completion.Enlistment, this.completionCommitResponseCallback, completion);
        }

        public void Commit(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Commit to transaction manager");
            }
            this.coordination.Commit(coordinator.Enlistment, this.commitResponseCallback, coordinator);
        }

        public void Committed(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Replying Committed to transaction manager");
            }
            participant.DeliverCallback(Status.Committed);
        }

        public void CreateTransaction(CompletionEnlistment completion, EnlistmentOptions options, MsgCreateTransactionEvent e)
        {
            DebugTrace.Trace(TraceLevel.Info, "Sending CreateTransaction to transaction manager");
            this.propagation.CreateTransaction(completion.Enlistment, options, this.createTransactionResponseCallback, e);
        }

        public void EnlistPrePrepareResponse(CoordinatorEnlistment coordinator, Status status)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Replying {0} to transaction manager's EnlistPrePrepare", status);
            }
            coordinator.DeliverCallback(status);
        }

        public void EnlistTransaction(CoordinatorEnlistment coordinator, EnlistmentOptions options, MsgEnlistTransactionEvent e)
        {
            DebugTrace.Trace(TraceLevel.Info, "Sending CreateSuperiorEnlistment to transaction manager");
            this.propagation.CreateSuperiorEnlistment(coordinator.Enlistment, options, this.enlistTransactionResponseCallback, e);
        }

        public void ForgetResponse(TransactionEnlistment enlistment, Status status)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, enlistment.EnlistmentId, "Replying {0} to transaction manager's Forget", status);
            }
            enlistment.DeliverCallback(status);
        }

        public void Prepare(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Prepare to transaction manager");
            }
            this.coordination.Prepare(coordinator.Enlistment, this.prepareResponseCallback, coordinator);
        }

        public void Prepared(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Replying Prepared to transaction manager");
            }
            participant.DeliverCallback(Status.Prepared);
        }

        public void PrePrepare(VolatileCoordinatorEnlistment volatileCoordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, volatileCoordinator.EnlistmentId, "Sending PrePrepare to transaction manager");
            }
            this.coordination.PrePrepare(volatileCoordinator.Enlistment, this.prePrepareResponseCallback, volatileCoordinator);
        }

        public void PrePrepareAborted(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Replying Aborted to transaction manager");
            }
            participant.DeliverCallback(Status.Aborted);
        }

        public void PrePrepared(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Replying PrePrepared to transaction manager");
            }
            participant.DeliverCallback(Status.PrePrepared);
        }

        public void ReadOnly(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Replying Readonly to transaction manager");
            }
            participant.DeliverCallback(Status.Readonly);
        }

        public void Register(ParticipantEnlistment participant, InternalEnlistSubordinateTransactionEvent e)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Sending subordinate Register for {0} to transaction manager", participant.Enlistment.NotificationMask);
            }
            this.propagation.CreateSubordinateEnlistment(participant.Enlistment, this.subordinateRegisterResponseCallback, e);
        }

        public void Register(ParticipantEnlistment participant, MsgRegisterEvent e)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Sending Register for {0} to transaction manager", participant.Enlistment.NotificationMask);
            }
            this.propagation.CreateSubordinateEnlistment(participant.Enlistment, this.registerResponseCallback, e);
        }

        public void Rejoined(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Replying Success to transaction manager's Rejoin");
            }
            participant.DeliverCallback(Status.Success);
        }

        public void Replayed(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Replying Success to transaction manager's Replay");
            }
            coordinator.DeliverCallback(Status.Success);
        }

        public void Rollback(CompletionEnlistment completion)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, completion.EnlistmentId, "Sending completion Rollback to transaction manager");
            }
            this.coordination.Rollback(completion.Enlistment, this.completionRollbackResponseCallback, completion);
        }

        public void Rollback(TransactionEnlistment enlistment)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, enlistment.EnlistmentId, "Sending Rollback to transaction manager");
            }
            this.coordination.Rollback(enlistment.Enlistment, this.rollbackResponseCallback, enlistment);
        }
    }
}

