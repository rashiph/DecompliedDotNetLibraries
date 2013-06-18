namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class CompletionStatusEvent : CompletionEvent
    {
        protected Microsoft.Transactions.Bridge.Status status;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CompletionStatusEvent(CompletionEnlistment completion, Microsoft.Transactions.Bridge.Status status) : base(completion)
        {
            this.status = status;
        }

        public Microsoft.Transactions.Bridge.Status Status
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.status;
            }
        }
    }
}

