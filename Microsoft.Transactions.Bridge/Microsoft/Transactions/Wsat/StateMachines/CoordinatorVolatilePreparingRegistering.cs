namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorVolatilePreparingRegistering : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorVolatilePreparingRegistering(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment enlistment = (CoordinatorEnlistment) stateMachine.Enlistment;
            if (enlistment.PreparingVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorVolatilePreparingRegistering requires PreparingVolatileCoordinator");
            }
            if (enlistment.RegisterVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorVolatilePreparingRegistering requires RegisterVolatileCoordinator");
            }
        }

        public override void OnEvent(MsgDurableRollbackEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
        }

        public override void OnEvent(MsgRegisterVolatileResponseEvent e)
        {
            e.VolatileCoordinator.SetCoordinatorProxy(e.Proxy);
            e.StateMachine.ChangeState(base.state.States.CoordinatorVolatilePreparingRegistered);
        }

        public override void OnEvent(MsgVolatilePrepareEvent e)
        {
        }

        public override void OnEvent(MsgVolatileRollbackEvent e)
        {
            VolatileCoordinatorEnlistment volatileCoordinator = e.VolatileCoordinator;
            CoordinatorEnlistment coordinator = volatileCoordinator.Coordinator;
            if (object.ReferenceEquals(volatileCoordinator, coordinator.PreparingVolatileCoordinator))
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
                    e.StateMachine.ChangeState(base.state.States.CoordinatorRegisteringVolatile);
                    return;
                }
            }
            DiagnosticUtility.FailFast("Invalid status code");
        }
    }
}

