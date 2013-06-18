namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorRegisteringDurable : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorRegisteringDurable(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment enlistment = (CoordinatorEnlistment) stateMachine.Enlistment;
            if (enlistment.RegisterVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorRegisteringDurable requires RegisterVolatileCoordinator");
            }
        }

        public override void OnEvent(MsgRegisterDurableResponseEvent e)
        {
            base.SetDurableCoordinatorActive(e);
            e.StateMachine.ChangeState(base.state.States.CoordinatorVolatileActive);
        }

        public override void OnEvent(MsgVolatileRollbackEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.VolatileCoordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            base.ProcessTmAsyncRollback(e);
        }
    }
}

