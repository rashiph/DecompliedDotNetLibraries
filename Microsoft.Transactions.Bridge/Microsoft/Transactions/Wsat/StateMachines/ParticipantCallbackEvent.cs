namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class ParticipantCallbackEvent : ParticipantEvent
    {
        protected ProtocolProviderCallback callback;
        protected object callbackState;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ParticipantCallbackEvent(ParticipantEnlistment participant, ProtocolProviderCallback callback, object state) : base(participant)
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

