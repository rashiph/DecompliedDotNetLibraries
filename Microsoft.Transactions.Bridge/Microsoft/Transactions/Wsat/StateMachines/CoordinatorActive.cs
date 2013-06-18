namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.Recovery;
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal class CoordinatorActive : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorActive(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment enlistment = (CoordinatorEnlistment) stateMachine.Enlistment;
            if (enlistment.RegisterVolatileCoordinator != null)
            {
                DiagnosticUtility.FailFast("CoordinatorActive requires null RegisterVolatileCoordinator");
            }
        }

        public override void OnEvent(MsgDurablePrepareEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            Exception exception = null;
            try
            {
                byte[] data = base.state.LogEntrySerialization.Serialize(coordinator);
                coordinator.Enlistment.SetRecoveryData(data);
                base.state.TransactionManagerSend.Prepare(coordinator);
                e.StateMachine.ChangeState(base.state.States.CoordinatorPreparing);
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
                    DebugTrace.Trace(TraceLevel.Error, "Failed to serialize log entry for coordinator: {0}", exception);
                }
                CoordinatorRecoveryLogEntryCreationFailureRecord.TraceAndLog(coordinator.EnlistmentId, coordinator.Enlistment.RemoteTransactionId, exception.Message, exception);
                base.state.TwoPhaseCommitParticipant.SendDurableAborted(coordinator);
                base.state.TransactionManagerSend.Rollback(coordinator);
                e.StateMachine.ChangeState(base.state.States.DurableAborted);
            }
        }

        public override void OnEvent(MsgDurableRollbackEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
        }

        public override void OnEvent(MsgVolatilePrepareEvent e)
        {
            VolatileCoordinatorEnlistment volatileCoordinator = e.VolatileCoordinator;
            CoordinatorEnlistment coordinator = volatileCoordinator.Coordinator;
            if (object.ReferenceEquals(volatileCoordinator, coordinator.LastCompletedVolatileCoordinator))
            {
                base.state.TwoPhaseCommitParticipant.SendVolatileReadOnly(volatileCoordinator);
            }
        }

        public override void OnEvent(MsgVolatileRollbackEvent e)
        {
            VolatileCoordinatorEnlistment volatileCoordinator = e.VolatileCoordinator;
            CoordinatorEnlistment coordinator = volatileCoordinator.Coordinator;
            if (object.ReferenceEquals(volatileCoordinator, coordinator.LastCompletedVolatileCoordinator))
            {
                base.state.TwoPhaseCommitParticipant.SendVolatileAborted(volatileCoordinator);
                base.state.TransactionManagerSend.Rollback(volatileCoordinator);
                e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
            }
            else
            {
                base.OnEvent(e);
            }
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            base.ProcessTmAsyncRollback(e);
        }

        public override void OnEvent(TmEnlistPrePrepareEvent e)
        {
            base.EnlistPrePrepare(e);
            e.StateMachine.ChangeState(base.state.States.CoordinatorRegisteringVolatile);
        }
    }
}

