namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class ParticipantEvent : SynchronizationEvent
    {
        protected ParticipantEnlistment participant;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ParticipantEvent(ParticipantEnlistment participant) : base(participant)
        {
            this.participant = participant;
        }

        public ParticipantEnlistment Participant
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.participant;
            }
        }
    }
}

