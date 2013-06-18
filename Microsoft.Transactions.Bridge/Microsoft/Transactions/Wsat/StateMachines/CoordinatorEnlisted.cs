namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorEnlisted : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorEnlisted(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            CoordinatorEnlistment coordinator = (CoordinatorEnlistment) stateMachine.Enlistment;
            base.state.RegistrationParticipant.SendDurableRegister(coordinator);
        }

        public override void OnEvent(MsgRegisterDurableResponseEvent e)
        {
            base.SetDurableCoordinatorActive(e);
            e.Coordinator.StateMachine.ChangeState(base.state.States.CoordinatorActive);
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            base.ProcessTmAsyncRollback(e);
        }

        public override void OnEvent(TmEnlistPrePrepareEvent e)
        {
            base.EnlistPrePrepare(e);
            e.StateMachine.ChangeState(base.state.States.CoordinatorRegisteringBoth);
        }
    }
}

