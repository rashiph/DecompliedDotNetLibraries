namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class SubordinateStateMachine : StateMachine
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SubordinateStateMachine(ParticipantEnlistment participant) : base(participant)
        {
        }

        public override Microsoft.Transactions.Wsat.StateMachines.State AbortedState
        {
            get
            {
                return base.state.States.SubordinateFinished;
            }
        }
    }
}

