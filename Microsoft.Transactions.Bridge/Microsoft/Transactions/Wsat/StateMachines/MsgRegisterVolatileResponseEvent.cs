namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class MsgRegisterVolatileResponseEvent : MsgRegisterResponseEvent
    {
        private VolatileCoordinatorEnlistment volatileCoordinator;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MsgRegisterVolatileResponseEvent(VolatileCoordinatorEnlistment volatileCoordinator, RegisterResponse response, TwoPhaseCommitCoordinatorProxy proxy) : base(volatileCoordinator, response, proxy)
        {
            this.volatileCoordinator = volatileCoordinator;
        }

        public override void Execute(StateMachine stateMachine)
        {
            try
            {
                if (DebugTrace.Info)
                {
                    base.state.DebugTraceSink.OnEvent(this);
                }
                stateMachine.State.OnEvent(this);
            }
            finally
            {
                base.Proxy.Release();
            }
        }

        public VolatileCoordinatorEnlistment VolatileCoordinator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileCoordinator;
            }
        }
    }
}

