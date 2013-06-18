namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TimerCoordinatorEvent : CoordinatorEvent
    {
        private TimerProfile profile;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TimerCoordinatorEvent(CoordinatorEnlistment coordinator, TimerProfile profile) : base(coordinator)
        {
            this.profile = profile;
        }

        public override void Execute(StateMachine stateMachine)
        {
            if (DebugTrace.Info)
            {
                base.state.DebugTraceSink.OnEvent(this);
            }
            base.coordinator.StateMachine.State.OnEvent(this);
        }
    }
}

