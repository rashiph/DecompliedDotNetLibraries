namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TimerParticipantEvent : ParticipantEvent
    {
        private TimerProfile profile;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TimerParticipantEvent(ParticipantEnlistment participant, TimerProfile profile) : base(participant)
        {
            this.profile = profile;
        }

        public override void Execute(StateMachine stateMachine)
        {
            if (DebugTrace.Info)
            {
                base.state.DebugTraceSink.OnEvent(this);
            }
            base.participant.StateMachine.State.OnEvent(this);
        }

        public TimerProfile Profile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.profile;
            }
        }
    }
}

