namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CompletionActive : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompletionActive(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            base.Enter(stateMachine);
            if (RegisterParticipantRecord.ShouldTrace)
            {
                CompletionEnlistment enlistment = (CompletionEnlistment) stateMachine.Enlistment;
                RegisterParticipantRecord.Trace(enlistment.EnlistmentId, enlistment.Enlistment.RemoteTransactionId, ControlProtocol.Completion, enlistment.ParticipantProxy.To, base.state.ProtocolVersion);
            }
        }

        public override void OnEvent(MsgCompletionCommitEvent e)
        {
            base.state.TransactionManagerSend.Commit(e.Completion);
            e.StateMachine.ChangeState(base.state.States.CompletionCommitting);
        }

        public override void OnEvent(MsgCompletionRollbackEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Completion);
            e.StateMachine.ChangeState(base.state.States.CompletionAborting);
        }

        public override void OnEvent(MsgRegisterCompletionEvent e)
        {
            if (RegisterParticipantFailureRecord.ShouldTrace)
            {
                RegisterParticipantFailureRecord.Trace(e.Completion.EnlistmentId, e.Completion.Enlistment.RemoteTransactionId, ControlProtocol.Completion, e.ParticipantService, Microsoft.Transactions.SR.GetString("RegisterCompletionFailureDuplicate"), base.state.ProtocolVersion);
            }
            base.state.RegistrationCoordinator.SendFault(e.Result, base.state.Faults.CompletionAlreadyRegistered);
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            CompletionEnlistment completion = (CompletionEnlistment) e.Enlistment;
            base.state.CompletionCoordinator.SendAborted(completion);
            completion.SetCallback(e.Callback, e.CallbackState);
            base.state.TransactionManagerSend.Aborted(completion);
            e.StateMachine.ChangeState(base.state.States.CompletionAborted);
        }
    }
}

