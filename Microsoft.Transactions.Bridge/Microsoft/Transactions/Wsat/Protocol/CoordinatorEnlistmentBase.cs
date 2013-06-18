namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class CoordinatorEnlistmentBase : TransactionEnlistment
    {
        protected TwoPhaseCommitCoordinatorProxy coordinatorProxy;
        protected EndpointAddress participantService;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinatorEnlistmentBase(ProtocolState state) : base(state)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinatorEnlistmentBase(ProtocolState state, Guid enlistmentId) : base(state, enlistmentId)
        {
        }

        public void CreateParticipantService()
        {
            EnlistmentHeader refParam = new EnlistmentHeader(base.enlistmentId);
            this.participantService = base.state.TwoPhaseCommitParticipantListener.CreateEndpointReference(refParam);
            if (this.coordinatorProxy != null)
            {
                this.coordinatorProxy.From = this.participantService;
            }
        }

        public override void OnStateMachineComplete()
        {
            base.OnStateMachineComplete();
            if (this.coordinatorProxy != null)
            {
                this.coordinatorProxy.Release();
            }
        }

        public void SetCoordinatorProxy(TwoPhaseCommitCoordinatorProxy proxy)
        {
            if (this.participantService == null)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("participantService needed for coordinatorProxy");
            }
            proxy.AddRef();
            this.coordinatorProxy = proxy;
            this.coordinatorProxy.From = this.participantService;
        }

        public TwoPhaseCommitCoordinatorProxy CoordinatorProxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorProxy;
            }
        }

        public EndpointAddress ParticipantService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.participantService;
            }
        }
    }
}

