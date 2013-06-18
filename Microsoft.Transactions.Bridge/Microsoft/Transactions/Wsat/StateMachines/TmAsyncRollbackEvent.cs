namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class TmAsyncRollbackEvent : SynchronizationEvent
    {
        private ProtocolProviderCallback callback;
        private object callbackState;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TmAsyncRollbackEvent(TransactionEnlistment enlistment, ProtocolProviderCallback callback, object state) : base(enlistment)
        {
            this.callback = callback;
            this.callbackState = state;
        }

        public override void Execute(StateMachine stateMachine)
        {
            if (DebugTrace.Info)
            {
                base.state.DebugTraceSink.OnEvent(this);
            }
            stateMachine.State.OnEvent(this);
        }

        public ProtocolProviderCallback Callback
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.callback;
            }
        }

        public object CallbackState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.callbackState;
            }
        }
    }
}

