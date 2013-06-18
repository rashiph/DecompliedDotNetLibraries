namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.InputOutput;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class CoordinatorEnlistment : CoordinatorEnlistmentBase
    {
        private TmEnlistPrePrepareEvent enlistPrePrepareEvent;
        private VolatileCoordinatorEnlistment lastCompletedVolatileCoordinator;
        private VolatileCoordinatorEnlistment preparingVolatileCoordinator;
        private EndpointAddress recoveredCoordinatorService;
        private VolatileCoordinatorEnlistment registerVolatileCoordinator;
        private Microsoft.Transactions.Wsat.Messaging.RegistrationProxy registrationProxy;
        private CoordinationContext superiorContext;
        private RequestSecurityTokenResponse superiorRstr;
        private List<VolatileCoordinatorEnlistment> volatileCoordinators;

        public CoordinatorEnlistment(ProtocolState state, Enlistment enlistment, Guid enlistmentId, EndpointAddress service) : base(state, enlistmentId)
        {
            base.enlistment = enlistment;
            base.enlistment.ProtocolProviderContext = this;
            this.recoveredCoordinatorService = service;
            base.stateMachine = new CoordinatorStateMachine(this);
            base.coordinatorProxy = state.TryCreateTwoPhaseCommitCoordinatorProxy(service);
            if (base.coordinatorProxy == null)
            {
                if (RecoveredCoordinatorInvalidMetadataRecord.ShouldTrace)
                {
                    RecoveredCoordinatorInvalidMetadataRecord.Trace(base.enlistmentId, enlistment.RemoteTransactionId, service, base.state.ProtocolVersion);
                }
                base.stateMachine.ChangeState(state.States.CoordinatorFailedRecovery);
            }
            else
            {
                base.stateMachine.ChangeState(state.States.CoordinatorRecovering);
            }
            base.AddToLookupTable();
        }

        public CoordinatorEnlistment(ProtocolState state, TransactionContextManager contextManager, CoordinationContext context, RequestSecurityTokenResponse rstr) : base(state)
        {
            base.ourContextManager = contextManager;
            this.superiorContext = context;
            this.superiorRstr = rstr;
            this.ConfigureEnlistment(context);
            base.stateMachine = new CoordinatorStateMachine(this);
            base.stateMachine.ChangeState(state.States.CoordinatorInitializing);
        }

        private void ConfigureEnlistment(CoordinationContext context)
        {
            Enlistment enlistment = new Enlistment();
            string identifier = context.Identifier;
            if (identifier == null)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Need transactionId to create enlistment");
            }
            enlistment.RemoteTransactionId = identifier;
            enlistment.LocalTransactionId = Ports.GetGuidFromTransactionId(identifier);
            enlistment.ProtocolProviderContext = this;
            base.enlistment = enlistment;
        }

        public void OnCoordinatorEnlisted()
        {
            base.CreateParticipantService();
            base.AddToLookupTable();
            base.VerifyAndTraceEnlistmentOptions();
            base.TraceTransferEvent();
        }

        public void OnDurableCoordinatorActive()
        {
            base.FindAndActivateTransactionContextManager();
        }

        public void OnEnlistPrePrepare(TmEnlistPrePrepareEvent e)
        {
            this.enlistPrePrepareEvent = e;
            if (this.volatileCoordinators == null)
            {
                this.volatileCoordinators = new List<VolatileCoordinatorEnlistment>();
            }
            VolatileCoordinatorEnlistment item = new VolatileCoordinatorEnlistment(base.state, this);
            this.volatileCoordinators.Add(item);
            base.state.Lookup.AddEnlistment(item);
            if (this.registerVolatileCoordinator != null)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Duplicate EnlistPrePrepare from TM");
            }
            this.registerVolatileCoordinator = item;
        }

        public override void OnStateMachineComplete()
        {
            base.OnStateMachineComplete();
            if (this.registrationProxy != null)
            {
                this.registrationProxy.Release();
            }
            if (this.volatileCoordinators != null)
            {
                foreach (VolatileCoordinatorEnlistment enlistment in this.volatileCoordinators)
                {
                    base.state.Lookup.RemoveEnlistment(enlistment);
                }
            }
        }

        public void SetRegistrationProxy(Microsoft.Transactions.Wsat.Messaging.RegistrationProxy proxy)
        {
            proxy.AddRef();
            this.registrationProxy = proxy;
        }

        public TmEnlistPrePrepareEvent EnlistPrePrepareEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enlistPrePrepareEvent;
            }
            set
            {
                if ((this.enlistPrePrepareEvent != null) && (value != null))
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Cannot clobber EnlistPrePrepareEvent");
                }
                this.enlistPrePrepareEvent = value;
            }
        }

        public VolatileCoordinatorEnlistment LastCompletedVolatileCoordinator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lastCompletedVolatileCoordinator;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.lastCompletedVolatileCoordinator = value;
            }
        }

        public VolatileCoordinatorEnlistment PreparingVolatileCoordinator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.preparingVolatileCoordinator;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.preparingVolatileCoordinator = value;
            }
        }

        public VolatileCoordinatorEnlistment RegisterVolatileCoordinator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.registerVolatileCoordinator;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.registerVolatileCoordinator = value;
            }
        }

        public Microsoft.Transactions.Wsat.Messaging.RegistrationProxy RegistrationProxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.registrationProxy;
            }
        }

        public CoordinationContext SuperiorContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.superiorContext;
            }
        }

        public RequestSecurityTokenResponse SuperiorIssuedToken
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.superiorRstr;
            }
        }
    }
}

