namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableStateMachine : ParticipantStateMachine
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableStateMachine(ParticipantEnlistment participant) : base(participant)
        {
        }

        public override Microsoft.Transactions.Wsat.StateMachines.State AbortedState
        {
            get
            {
                return base.state.States.DurableAborted;
            }
        }
    }
}

