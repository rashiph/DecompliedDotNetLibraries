namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TmEnlistTransactionResponseEvent : CoordinatorStatusEvent
    {
        private MsgEnlistTransactionEvent e;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TmEnlistTransactionResponseEvent(CoordinatorEnlistment coordinator, Status status, MsgEnlistTransactionEvent e) : base(coordinator, status)
        {
            this.e = e;
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
                return this.e;
            }
        }
    }
}

