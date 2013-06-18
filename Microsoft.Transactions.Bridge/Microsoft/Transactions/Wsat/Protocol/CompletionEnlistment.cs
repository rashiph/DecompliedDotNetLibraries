namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class CompletionEnlistment : TransactionEnlistment
    {
        private EndpointAddress coordinatorService;
        private CompletionParticipantProxy participantProxy;

        public CompletionEnlistment(ProtocolState state) : base(state, Guid.Empty)
        {
            this.ConfigureEnlistment();
            base.stateMachine = new CompletionStateMachine(this);
            base.stateMachine.ChangeState(state.States.CompletionInitializing);
        }

        private void ConfigureEnlistment()
        {
            Enlistment enlistment = new Enlistment {
                ProtocolProviderContext = this
            };
            base.enlistment = enlistment;
        }

        public void OnRootTransactionCreated()
        {
            base.enlistmentId = base.enlistment.LocalTransactionId;
            base.enlistment.RemoteTransactionId = CoordinationContext.CreateNativeIdentifier(base.enlistmentId);
            EnlistmentHeader refParam = new EnlistmentHeader(base.enlistmentId);
            this.coordinatorService = base.state.CompletionCoordinatorListener.CreateEndpointReference(refParam);
            base.ourContextManager = new TransactionContextManager(base.state, base.enlistment.RemoteTransactionId);
            base.state.Lookup.AddTransactionContextManager(base.ourContextManager);
            base.ActivateTransactionContextManager(base.ourContextManager);
            base.AddToLookupTable();
            base.VerifyAndTraceEnlistmentOptions();
            base.TraceTransferEvent();
        }

        public override void OnStateMachineComplete()
        {
            base.OnStateMachineComplete();
            if (this.participantProxy != null)
            {
                this.participantProxy.Release();
            }
        }

        public void SetCompletionProxy(CompletionParticipantProxy proxy)
        {
            proxy.AddRef();
            this.participantProxy = proxy;
        }

        public EndpointAddress CoordinatorService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorService;
            }
        }

        public CompletionParticipantProxy ParticipantProxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.participantProxy;
            }
        }
    }
}

