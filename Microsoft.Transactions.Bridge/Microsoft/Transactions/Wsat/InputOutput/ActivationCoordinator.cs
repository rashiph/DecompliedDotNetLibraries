namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Channels;

    internal class ActivationCoordinator : IActivationCoordinator
    {
        private ProtocolState state;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivationCoordinator(ProtocolState state)
        {
            this.state = state;
        }

        public void CreateCoordinationContext(Message message, Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result)
        {
            Microsoft.Transactions.Wsat.Messaging.CreateCoordinationContext create = new Microsoft.Transactions.Wsat.Messaging.CreateCoordinationContext(message, this.state.ProtocolVersion);
            CoordinationContext currentContext = create.CurrentContext;
            if (currentContext == null)
            {
                CompletionEnlistment completion = new CompletionEnlistment(this.state);
                completion.StateMachine.Enqueue(new MsgCreateTransactionEvent(completion, ref create, result));
            }
            else
            {
                TransactionContextManager contextManager = this.state.Lookup.FindTransactionContextManager(currentContext.Identifier);
                if (contextManager == null)
                {
                    bool flag;
                    contextManager = new TransactionContextManager(this.state, currentContext.Identifier);
                    contextManager = this.state.Lookup.FindOrAddTransactionContextManager(contextManager, out flag);
                }
                contextManager.StateMachine.Enqueue(new TransactionContextEnlistTransactionEvent(contextManager, ref create, result));
            }
        }

        public void SendCreateCoordinationContextResponse(TransactionContext txContext, Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result)
        {
            CreateCoordinationContextResponse response = new CreateCoordinationContextResponse(this.state.ProtocolVersion) {
                CoordinationContext = txContext.CoordinationContext,
                IssuedToken = txContext.IssuedToken
            };
            if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "Sending CreateCoordinationContextResponse");
            }
            ActivationProxy.SendCreateCoordinationContextResponse(result, ref response);
        }

        public void SendFault(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Fault fault)
        {
            this.state.Perf.FaultsSentCountPerInterval.Increment();
            if (DebugTrace.Warning)
            {
                DebugTrace.Trace(TraceLevel.Warning, "Sending {0} fault to activation participant", fault.Code.Name);
            }
            ActivationProxy.SendFaultResponse(result, fault);
        }
    }
}

