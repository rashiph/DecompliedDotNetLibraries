namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorStateMachine : StateMachine
    {
        private CoordinatorEnlistment coordinator;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorStateMachine(CoordinatorEnlistment coordinator) : base(coordinator)
        {
            this.coordinator = coordinator;
        }

        protected override void OnTimer(TimerProfile profile)
        {
            base.OnTimer(profile);
            base.synchronization.Enqueue(new TimerCoordinatorEvent(this.coordinator, profile));
        }

        public override Microsoft.Transactions.Wsat.StateMachines.State AbortedState
        {
            get
            {
                return base.state.States.CoordinatorAborted;
            }
        }
    }
}

