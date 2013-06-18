namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorRegisteringBoth : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorRegisteringBoth(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment enlistment = (CoordinatorEnlistment) stateMachine.Enlistment;
            if (enlistment.RegisterVolatileCoordinator == null)
            {
                DiagnosticUtility.FailFast("CoordinatorRegisteringBoth requires RegisterVolatileCoordinator");
            }
        }

        public override void OnEvent(MsgRegisterDurableResponseEvent e)
        {
            base.SetDurableCoordinatorActive(e);
            e.StateMachine.ChangeState(base.state.States.CoordinatorRegisteringVolatile);
        }

        public override void OnEvent(MsgRegisterVolatileResponseEvent e)
        {
            e.VolatileCoordinator.SetCoordinatorProxy(e.Proxy);
            e.StateMachine.ChangeState(base.state.States.CoordinatorRegisteringDurable);
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            base.ProcessTmAsyncRollback(e);
        }
    }
}

