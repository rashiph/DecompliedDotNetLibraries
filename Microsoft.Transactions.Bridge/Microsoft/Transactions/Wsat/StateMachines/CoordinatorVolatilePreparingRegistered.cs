namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorVolatilePreparingRegistered : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorVolatilePreparingRegistered(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment enlistment = (CoordinatorEnlistment) stateMachine.Enlistment;
            if (enlistment.PreparingVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorVolatilePreparingRegistered requires PreparingVolatileCoordinator");
            }
            if (enlistment.RegisterVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorVolatilePreparingRegistered requires RegisterVolatileCoordinator");
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
            if (object.ReferenceEquals(volatileCoordinator, coordinator.RegisterVolatileCoordinator))
            {
                base.OnEvent(e);
            }
        }

        public override void OnEvent(MsgVolatileRollbackEvent e)
        {
            VolatileCoordinatorEnlistment volatileCoordinator = e.VolatileCoordinator;
            CoordinatorEnlistment coordinator = volatileCoordinator.Coordinator;
            if (object.ReferenceEquals(volatileCoordinator, coordinator.RegisterVolatileCoordinator) || object.ReferenceEquals(volatileCoordinator, coordinator.PreparingVolatileCoordinator))
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

        public override void OnEvent(TmPrePrepareResponseEvent e)
        {
            switch (e.Status)
            {
                case Status.Aborted:
                    e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
                    return;

                case Status.PrePrepared:
                {
                    base.state.TwoPhaseCommitParticipant.SendVolatileReadOnly(e.VolatileCoordinator);
                    CoordinatorEnlistment coordinator = e.VolatileCoordinator.Coordinator;
                    coordinator.LastCompletedVolatileCoordinator = coordinator.PreparingVolatileCoordinator;
                    coordinator.PreparingVolatileCoordinator = null;
                    e.StateMachine.ChangeState(base.state.States.CoordinatorVolatileActive);
                    return;
                }
            }
            DiagnosticUtility.FailFast("Invalid status code");
        }
    }
}

