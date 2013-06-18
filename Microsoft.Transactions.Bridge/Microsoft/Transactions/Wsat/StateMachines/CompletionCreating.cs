namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CompletionCreating : InactiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompletionCreating(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(TmCreateTransactionResponseEvent e)
        {
            MsgCreateTransactionEvent sourceEvent = e.SourceEvent;
            CompletionEnlistment completion = e.Completion;
            if (e.Status != Status.Success)
            {
                Fault cannotCreateContext = base.state.Faults.CannotCreateContext;
                base.state.ActivationCoordinator.SendFault(sourceEvent.Result, cannotCreateContext);
                if (CreateTransactionFailureRecord.ShouldTrace)
                {
                    CreateTransactionFailureRecord.Trace(completion.EnlistmentId, Microsoft.Transactions.SR.GetString("PplCreateTransactionFailed", new object[] { e.Status.ToString() }));
                }
                completion.StateMachine.ChangeState(base.state.States.CompletionInitializationFailed);
            }
            else
            {
                completion.OnRootTransactionCreated();
                TransactionContext transactionContext = completion.ContextManager.TransactionContext;
                base.state.ActivationCoordinator.SendCreateCoordinationContextResponse(transactionContext, sourceEvent.Result);
                completion.StateMachine.ChangeState(base.state.States.CompletionCreated);
            }
        }
    }
}

