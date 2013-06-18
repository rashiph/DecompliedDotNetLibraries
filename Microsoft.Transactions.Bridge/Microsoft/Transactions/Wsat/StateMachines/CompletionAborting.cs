namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CompletionAborting : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompletionAborting(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgCompletionCommitEvent e)
        {
            base.state.CompletionCoordinator.SendAborted(e.Completion);
        }

        public override void OnEvent(MsgCompletionRollbackEvent e)
        {
            base.state.CompletionCoordinator.SendAborted(e.Completion);
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            CompletionEnlistment completion = (CompletionEnlistment) e.Enlistment;
            base.state.CompletionCoordinator.SendAborted(completion);
            completion.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Aborted(completion);
            e.StateMachine.ChangeState(base.state.States.CompletionAborted);
        }

        public override void OnEvent(TmCompletionRollbackResponseEvent e)
        {
            if (e.Status != Status.Aborted)
            {
                DiagnosticUtility.FailFast("Transaction manager should respond Aborted to Rollback");
            }
            base.state.CompletionCoordinator.SendAborted(e.Completion);
            e.StateMachine.ChangeState(base.state.States.CompletionAborted);
        }
    }
}

