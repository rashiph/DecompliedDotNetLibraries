namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class CoordinatorCallbackEvent : CoordinatorEvent
    {
        protected ProtocolProviderCallback callback;
        protected object callbackState;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinatorCallbackEvent(CoordinatorEnlistment coordinator, ProtocolProviderCallback callback, object state) : base(coordinator)
        {
            this.callback = callback;
            this.callbackState = state;
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

