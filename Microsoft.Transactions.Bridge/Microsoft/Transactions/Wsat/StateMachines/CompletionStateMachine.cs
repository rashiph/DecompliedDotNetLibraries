namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CompletionStateMachine : StateMachine
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompletionStateMachine(CompletionEnlistment completion) : base(completion)
        {
        }

        public override Microsoft.Transactions.Wsat.StateMachines.State AbortedState
        {
            get
            {
                return base.state.States.CompletionAborted;
            }
        }
    }
}

