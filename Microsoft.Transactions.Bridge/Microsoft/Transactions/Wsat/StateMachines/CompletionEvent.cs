namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class CompletionEvent : SynchronizationEvent
    {
        protected CompletionEnlistment completion;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CompletionEvent(CompletionEnlistment completion) : base(completion)
        {
            this.completion = completion;
        }

        public CompletionEnlistment Completion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completion;
            }
        }
    }
}

