namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorRegisteringVolatile : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorRegisteringVolatile(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment enlistment = (CoordinatorEnlistment) stateMachine.Enlistment;
            if (enlistment.RegisterVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorRegisteringVolatile requires RegisterVolatileCoordinator");
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
            e.StateMachine.ChangeState(base.state.States.CoordinatorVolatileActive);
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
    }
}

