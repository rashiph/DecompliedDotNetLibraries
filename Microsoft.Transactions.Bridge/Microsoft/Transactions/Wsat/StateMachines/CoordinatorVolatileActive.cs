namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorVolatileActive : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorVolatileActive(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) stateMachine.Enlistment;
            if (coordinator.RegisterVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorVolatileActive requires RegisterVolatileCoordinator");
            }
            if (RegisterCoordinatorRecord.ShouldTrace)
            {
                RegisterCoordinatorRecord.Trace(coordinator.EnlistmentId, coordinator.SuperiorContext, ControlProtocol.Volatile2PC, coordinator.RegisterVolatileCoordinator.CoordinatorProxy.To, base.state.ProtocolVersion);
            }
            TmEnlistPrePrepareEvent enlistPrePrepareEvent = coordinator.EnlistPrePrepareEvent;
            coordinator.EnlistPrePrepareEvent = null;
            coordinator.SetCallback(enlistPrePrepareEvent.Callback, enlistPrePrepareEvent.CallbackState);
            base.state.TransactionManagerSend.EnlistPrePrepareResponse(coordinator, Status.Success);
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
            else if (object.ReferenceEquals(volatileCoordinator, coordinator.RegisterVolatileCoordinator))
            {
                coordinator.PreparingVolatileCoordinator = volatileCoordinator;
                coordinator.RegisterVolatileCoordinator = null;
                base.state.TransactionManagerSend.PrePrepare(volatileCoordinator);
                e.StateMachine.ChangeState(base.state.States.CoordinatorVolatilePreparing);
            }
        }

        public override void OnEvent(MsgVolatileRollbackEvent e)
        {
            VolatileCoordinatorEnlistment volatileCoordinator = e.VolatileCoordinator;
            CoordinatorEnlistment coordinator = volatileCoordinator.Coordinator;
            if (object.ReferenceEquals(volatileCoordinator, coordinator.RegisterVolatileCoordinator) || object.ReferenceEquals(volatileCoordinator, coordinator.LastCompletedVolatileCoordinator))
            {
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
    }
}

