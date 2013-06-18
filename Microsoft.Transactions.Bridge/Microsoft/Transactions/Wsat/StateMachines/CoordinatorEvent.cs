namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class CoordinatorEvent : SynchronizationEvent
    {
        protected CoordinatorEnlistment coordinator;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinatorEvent(CoordinatorEnlistment coordinator) : base(coordinator)
        {
            this.coordinator = coordinator;
        }

        public CoordinatorEnlistment Coordinator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinator;
            }
        }
    }
}

