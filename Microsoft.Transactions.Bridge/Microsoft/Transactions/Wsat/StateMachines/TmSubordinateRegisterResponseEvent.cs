namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TmSubordinateRegisterResponseEvent : ParticipantStatusEvent
    {
        private InternalEnlistSubordinateTransactionEvent source;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TmSubordinateRegisterResponseEvent(ParticipantEnlistment participant, Status status, InternalEnlistSubordinateTransactionEvent source) : base(participant, status)
        {
            this.source = source;
        }

        public override void Execute(StateMachine stateMachine)
        {
            if (DebugTrace.Info)
            {
                base.state.DebugTraceSink.OnEvent(this);
            }
            stateMachine.State.OnEvent(this);
        }

        public InternalEnlistSubordinateTransactionEvent SourceEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.source;
            }
        }
    }
}

