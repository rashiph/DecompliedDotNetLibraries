namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class ActiveState : Microsoft.Transactions.Wsat.StateMachines.State
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ActiveState(ProtocolState state) : base(state)
        {
        }
    }
}

