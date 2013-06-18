namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class SubordinateInitializing : InactiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SubordinateInitializing(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(InternalEnlistSubordinateTransactionEvent e)
        {
            ParticipantEnlistment participant = e.Participant;
            base.state.TransactionManagerSend.Register(participant, e);
            participant.StateMachine.ChangeState(base.state.States.SubordinateRegistering);
        }
    }
}

