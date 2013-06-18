namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class InactiveState : Microsoft.Transactions.Wsat.StateMachines.State
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InactiveState(ProtocolState state) : base(state)
        {
        }
    }
}

