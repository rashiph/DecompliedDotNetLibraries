namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableRecovering : InactiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableRecovering(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(TmRejoinEvent e)
        {
            if (!base.state.Recovering)
            {
                DiagnosticUtility.FailFast("Rejoin events should only be delivered during recovery");
            }
            ParticipantEnlistment participant = e.Participant;
            participant.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Rejoined(participant);
            e.StateMachine.ChangeState(base.state.States.DurableRejoined);
        }
    }
}

