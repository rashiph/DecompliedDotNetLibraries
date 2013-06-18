namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal abstract class Faults
    {
        protected AtomicTransactionStrings atomicTransactionStrings;
        protected CoordinationStrings coordinationStrings;
        public static readonly FaultCode CoordinatorRegistrationFailedCode = new FaultCode("CoordinatorRegistrationFailed", "http://schemas.microsoft.com/ws/2006/02/transactions");
        protected Fault inconsistentInternalState;
        protected Fault invalidParameters;
        protected Fault invalidPolicy;
        protected Fault invalidProtocol;
        protected Fault invalidState;

        protected Faults(ProtocolVersion protocolVersion)
        {
            this.coordinationStrings = CoordinationStrings.Version(protocolVersion);
            this.atomicTransactionStrings = AtomicTransactionStrings.Version(protocolVersion);
            string reasonText = Microsoft.Transactions.SR.GetString("InvalidStateReason");
            FaultCode code = new FaultCode(this.coordinationStrings.InvalidState, this.coordinationStrings.Namespace);
            this.invalidState = new Fault(this.coordinationStrings.FaultAction, code, reasonText);
            string str2 = Microsoft.Transactions.SR.GetString("InvalidProtocolReason");
            FaultCode code2 = new FaultCode(this.coordinationStrings.InvalidProtocol, this.coordinationStrings.Namespace);
            this.invalidProtocol = new Fault(this.coordinationStrings.FaultAction, code2, str2);
            string str3 = Microsoft.Transactions.SR.GetString("InvalidParametersReason");
            FaultCode code3 = new FaultCode(this.coordinationStrings.InvalidParameters, this.coordinationStrings.Namespace);
            this.invalidParameters = new Fault(this.coordinationStrings.FaultAction, code3, str3);
            string str4 = Microsoft.Transactions.SR.GetString("InconsistentInternalStateReason");
            FaultCode code4 = new FaultCode(this.atomicTransactionStrings.InconsistentInternalState, this.atomicTransactionStrings.Namespace);
            this.inconsistentInternalState = new Fault(this.atomicTransactionStrings.FaultAction, code4, str4);
            string str5 = Microsoft.Transactions.SR.GetString("InvalidPolicyReason");
            FaultCode code5 = new FaultCode("InvalidPolicy", "http://schemas.microsoft.com/ws/2006/02/transactions");
            this.invalidPolicy = new Fault(this.atomicTransactionStrings.FaultAction, code5, str5);
        }

        public static Fault CreateAccessDeniedFault(MessageVersion version)
        {
            FaultException exception = (FaultException) AuthorizationBehavior.CreateAccessDeniedFaultException();
            return new Fault(version.Addressing.DefaultFaultAction, exception.Code, exception.Reason.ToString());
        }

        public abstract Fault ParticipantTMRegistrationFailed(Status status);
        public abstract Fault SubordinateTMRegistrationFailed(Status status);
        public abstract Fault TMEnlistFailed(Status status);
        public static Faults Version(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(Faults), "V");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return Faults10.Instance;

                case ProtocolVersion.Version11:
                    return Faults11.Instance;
            }
            return null;
        }

        public abstract Fault CannotCreateContext { get; }

        public abstract Fault CompletionAlreadyRegistered { get; }

        public abstract Fault CreateContextDispatchFailed { get; }

        public Fault InconsistentInternalState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.inconsistentInternalState;
            }
        }

        public Fault InvalidParameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.invalidParameters;
            }
        }

        public Fault InvalidPolicy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.invalidPolicy;
            }
        }

        public Fault InvalidProtocol
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.invalidProtocol;
            }
        }

        public Fault InvalidState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.invalidState;
            }
        }

        public abstract Fault ParticipantRegistrationLoopback { get; }

        public abstract Fault ParticipantRegistrationNetAccessDisabled { get; }

        public abstract Fault RegistrationDispatchFailed { get; }

        public abstract Fault RegistrationProxyFailed { get; }

        public abstract Fault SubordinateRegistrationNetAccessDisabled { get; }

        public abstract Fault UnknownCompletionEnlistment { get; }

        public abstract Fault UnknownTransaction { get; }
    }
}

