namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime;

    internal class VolatileCoordinatorEnlistment : CoordinatorEnlistmentBase
    {
        private CoordinatorEnlistment coordinator;

        public VolatileCoordinatorEnlistment(ProtocolState state, CoordinatorEnlistment coordinator) : base(state)
        {
            this.coordinator = coordinator;
            base.enlistment = this.coordinator.Enlistment;
            base.stateMachine = this.coordinator.StateMachine;
            base.CreateParticipantService();
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

