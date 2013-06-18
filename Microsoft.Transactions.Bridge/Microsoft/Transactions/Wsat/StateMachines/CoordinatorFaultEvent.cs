namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal abstract class CoordinatorFaultEvent : CoordinatorEvent
    {
        protected MessageFault fault;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinatorFaultEvent(CoordinatorEnlistment coordinator, MessageFault fault) : base(coordinator)
        {
            this.fault = fault;
        }

        public MessageFault Fault
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fault;
            }
        }
    }
}

