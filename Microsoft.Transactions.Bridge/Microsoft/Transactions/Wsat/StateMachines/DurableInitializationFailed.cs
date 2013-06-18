namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableInitializationFailed : TerminalState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableInitializationFailed(ProtocolState state) : base(state)
        {
        }
    }
}

