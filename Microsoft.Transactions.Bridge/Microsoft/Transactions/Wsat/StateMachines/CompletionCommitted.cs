namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CompletionCommitted : TerminalState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompletionCommitted(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            if (ParticipantStateMachineFinishedRecord.ShouldTrace)
            {
                ParticipantStateMachineFinishedRecord.Trace(stateMachine.Enlistment.EnlistmentId, stateMachine.Enlistment.Enlistment.RemoteTransactionId, TransactionOutcome.Committed);
            }
        }

        public override void OnEvent(MsgCompletionCommitEvent e)
        {
            base.state.CompletionCoordinator.SendCommitted(e.ReplyTo);
        }
    }
}

