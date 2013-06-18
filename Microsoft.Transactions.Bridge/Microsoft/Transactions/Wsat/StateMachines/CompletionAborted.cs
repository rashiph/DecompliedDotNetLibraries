namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CompletionAborted : TerminalState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompletionAborted(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            if (ParticipantStateMachineFinishedRecord.ShouldTrace)
            {
                ParticipantStateMachineFinishedRecord.Trace(stateMachine.Enlistment.EnlistmentId, stateMachine.Enlistment.Enlistment.RemoteTransactionId, TransactionOutcome.Aborted);
            }
        }

        public override void OnEvent(MsgCompletionCommitEvent e)
        {
            base.state.CompletionCoordinator.SendAborted(e.ReplyTo);
        }

        public override void OnEvent(MsgCompletionRollbackEvent e)
        {
            base.state.CompletionCoordinator.SendAborted(e.ReplyTo);
        }

        public override void OnEvent(TmCompletionRollbackResponseEvent e)
        {
            if (e.Status != Status.Aborted)
            {
                DiagnosticUtility.FailFast("Transaction manager should respond Aborted to Rollback");
            }
        }
    }
}

