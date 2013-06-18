namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class VolatileCoordinatorEvent : SynchronizationEvent
    {
        protected VolatileCoordinatorEnlistment coordinator;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected VolatileCoordinatorEvent(VolatileCoordinatorEnlistment coordinator) : base(coordinator)
        {
            this.coordinator = coordinator;
        }

        public VolatileCoordinatorEnlistment VolatileCoordinator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinator;
            }
        }
    }
}

