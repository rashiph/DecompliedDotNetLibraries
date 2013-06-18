namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal abstract class ParticipantFaultEvent : ParticipantEvent
    {
        protected MessageFault fault;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ParticipantFaultEvent(ParticipantEnlistment participant, MessageFault fault) : base(participant)
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

