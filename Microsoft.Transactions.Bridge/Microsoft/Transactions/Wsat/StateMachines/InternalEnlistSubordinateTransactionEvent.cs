namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class InternalEnlistSubordinateTransactionEvent : ParticipantEvent
    {
        private MsgEnlistTransactionEvent source;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InternalEnlistSubordinateTransactionEvent(ParticipantEnlistment participant, MsgEnlistTransactionEvent source) : base(participant)
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

        public MsgEnlistTransactionEvent SourceEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.source;
            }
        }
    }
}

