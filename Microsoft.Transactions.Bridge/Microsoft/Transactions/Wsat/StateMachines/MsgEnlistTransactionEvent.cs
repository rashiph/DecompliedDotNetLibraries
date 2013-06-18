namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class MsgEnlistTransactionEvent : CoordinatorEvent
    {
        private CreateCoordinationContext create;
        private RequestAsyncResult result;

        public MsgEnlistTransactionEvent(CoordinatorEnlistment coordinator, ref CreateCoordinationContext create, RequestAsyncResult result) : base(coordinator)
        {
            this.create = create;
            this.result = result;
        }

        public override void Execute(StateMachine stateMachine)
        {
            if (DebugTrace.Info)
            {
                base.state.DebugTraceSink.OnEvent(this);
            }
            stateMachine.State.OnEvent(this);
        }

        public CreateCoordinationContext Body
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.create;
            }
        }

        public RequestAsyncResult Result
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.result;
            }
        }
    }
}

