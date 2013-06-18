namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Transactions;

    internal class ParticipantEnlistment : TransactionEnlistment
    {
        private EndpointAddress coordinatorService;
        private long lastMessageTime;
        private TwoPhaseCommitParticipantProxy participantProxy;
        private Microsoft.Transactions.Wsat.Messaging.ControlProtocol protocol;
        private TimeSpan timeoutEstimate;

        public ParticipantEnlistment(ProtocolState state, Enlistment coordinatorEnlistment, TransactionContextManager contextManager) : base(state)
        {
            this.protocol = Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Volatile2PC;
            base.ourContextManager = contextManager;
            base.enlistment = new Enlistment();
            base.enlistment.LocalTransactionId = coordinatorEnlistment.LocalTransactionId;
            base.enlistment.RemoteTransactionId = coordinatorEnlistment.RemoteTransactionId;
            base.enlistment.NotificationMask = Notifications.Volatile | Notifications.TwoPhaseCommit;
            base.enlistment.ProtocolProviderContext = this;
            base.stateMachine = new SubordinateStateMachine(this);
            base.stateMachine.ChangeState(state.States.SubordinateInitializing);
        }

        public ParticipantEnlistment(ProtocolState state, Enlistment enlistment, Guid enlistmentId, EndpointAddress service) : base(state, enlistmentId)
        {
            base.enlistment = enlistment;
            base.enlistment.ProtocolProviderContext = this;
            this.protocol = Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Durable2PC;
            base.stateMachine = new DurableStateMachine(this);
            this.participantProxy = state.TryCreateTwoPhaseCommitParticipantProxy(service);
            if (this.participantProxy == null)
            {
                if (RecoveredParticipantInvalidMetadataRecord.ShouldTrace)
                {
                    RecoveredParticipantInvalidMetadataRecord.Trace(base.enlistmentId, enlistment.RemoteTransactionId, service, base.state.ProtocolVersion);
                }
                base.stateMachine.ChangeState(state.States.DurableFailedRecovery);
            }
            else
            {
                base.stateMachine.ChangeState(state.States.DurableRecovering);
            }
            base.AddToLookupTable();
        }

        public ParticipantEnlistment(ProtocolState state, WsatRegistrationHeader header, Microsoft.Transactions.Wsat.Messaging.ControlProtocol protocol, TwoPhaseCommitParticipantProxy proxy) : base(state)
        {
            this.protocol = protocol;
            proxy.AddRef();
            this.participantProxy = proxy;
            this.ConfigureEnlistment(header);
            this.CreateCoordinatorService();
            switch (protocol)
            {
                case Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Volatile2PC:
                    base.stateMachine = new VolatileStateMachine(this);
                    base.stateMachine.ChangeState(state.States.VolatileRegistering);
                    return;

                case Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Durable2PC:
                    base.stateMachine = new DurableStateMachine(this);
                    base.stateMachine.ChangeState(state.States.DurableRegistering);
                    return;
            }
            Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Invalid protocol");
        }

        private void ConfigureEnlistment(WsatRegistrationHeader header)
        {
            Enlistment enlistment = new Enlistment();
            string contextId = header.ContextId;
            if (contextId == null)
            {
                contextId = CoordinationContext.CreateNativeIdentifier(header.TransactionId);
            }
            enlistment.LocalTransactionId = header.TransactionId;
            enlistment.RemoteTransactionId = contextId;
            Notifications twoPhaseCommit = Notifications.TwoPhaseCommit;
            switch (this.protocol)
            {
                case Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Volatile2PC:
                    twoPhaseCommit |= Notifications.Volatile | Notifications.Phase0;
                    break;

                case Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Durable2PC:
                    break;

                default:
                    Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Invalid protocol");
                    break;
            }
            enlistment.NotificationMask = twoPhaseCommit;
            enlistment.ProtocolProviderContext = this;
            base.enlistment = enlistment;
        }

        public void CreateCoordinatorService()
        {
            if ((this.protocol != Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Durable2PC) && (this.protocol != Microsoft.Transactions.Wsat.Messaging.ControlProtocol.Volatile2PC))
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Need protocol for coordinator service");
            }
            EnlistmentHeader refParam = new EnlistmentHeader(base.enlistmentId, this.protocol);
            this.coordinatorService = base.state.TwoPhaseCommitCoordinatorListener.CreateEndpointReference(refParam);
            this.participantProxy.From = this.coordinatorService;
        }

        public void OnParticipantRegistered()
        {
            base.AddToLookupTable();
            base.VerifyAndTraceEnlistmentOptions();
            base.TraceTransferEvent();
            this.timeoutEstimate = base.state.ElapsedTime + base.enlistment.EnlistmentOptions.Expires;
        }

        public override void OnStateMachineComplete()
        {
            base.OnStateMachineComplete();
            if (this.participantProxy != null)
            {
                this.participantProxy.Release();
            }
        }

        public void OnSubordinateRegistered()
        {
            base.FindAndActivateTransactionContextManager();
            base.VerifyAndTraceEnlistmentOptions();
            base.TraceTransferEvent();
        }

        public Microsoft.Transactions.Wsat.Messaging.ControlProtocol ControlProtocol
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocol;
            }
        }

        public EndpointAddress CoordinatorService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.coordinatorService;
            }
        }

        public long LastMessageTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lastMessageTime;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.lastMessageTime = value;
            }
        }

        public TwoPhaseCommitParticipantProxy ParticipantProxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.participantProxy;
            }
        }

        public TimeSpan TimeoutEstimate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.timeoutEstimate;
            }
        }
    }
}

