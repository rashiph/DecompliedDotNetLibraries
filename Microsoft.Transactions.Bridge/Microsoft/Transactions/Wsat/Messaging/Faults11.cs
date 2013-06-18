namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class Faults11 : Faults
    {
        private Fault cannotCreateContext;
        private Fault cannotRegisterParticipant;
        private static Faults11 instance = new Faults11();
        private Fault unknownTransaction;

        private Faults11() : base(ProtocolVersion.Version11)
        {
            string reasonText = Microsoft.Transactions.SR.GetString("CannotCreateContextReason");
            FaultCode code = new FaultCode(base.coordinationStrings.CannotCreateContext, base.coordinationStrings.Namespace);
            this.cannotCreateContext = new Fault(base.coordinationStrings.FaultAction, code, reasonText);
            string str2 = Microsoft.Transactions.SR.GetString("CannotRegisterParticipant");
            FaultCode code2 = new FaultCode(base.coordinationStrings.CannotRegisterParticipant, base.coordinationStrings.Namespace);
            this.cannotRegisterParticipant = new Fault(base.coordinationStrings.FaultAction, code2, str2);
            string str3 = Microsoft.Transactions.SR.GetString("UnknownTransactionReason");
            FaultCode code3 = new FaultCode(base.atomicTransactionStrings.UnknownTransaction, base.atomicTransactionStrings.Namespace);
            this.unknownTransaction = new Fault(base.atomicTransactionStrings.FaultAction, code3, str3);
        }

        public override Fault ParticipantTMRegistrationFailed(Status status)
        {
            return this.cannotRegisterParticipant;
        }

        public override Fault SubordinateTMRegistrationFailed(Status status)
        {
            return this.cannotCreateContext;
        }

        public override Fault TMEnlistFailed(Status status)
        {
            return this.cannotCreateContext;
        }

        public override Fault CannotCreateContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotCreateContext;
            }
        }

        public override Fault CompletionAlreadyRegistered
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotRegisterParticipant;
            }
        }

        public override Fault CreateContextDispatchFailed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotCreateContext;
            }
        }

        public static Faults Instance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return instance;
            }
        }

        public override Fault ParticipantRegistrationLoopback
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotRegisterParticipant;
            }
        }

        public override Fault ParticipantRegistrationNetAccessDisabled
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotRegisterParticipant;
            }
        }

        public override Fault RegistrationDispatchFailed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotRegisterParticipant;
            }
        }

        public override Fault RegistrationProxyFailed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotCreateContext;
            }
        }

        public override Fault SubordinateRegistrationNetAccessDisabled
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotCreateContext;
            }
        }

        public override Fault UnknownCompletionEnlistment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cannotRegisterParticipant;
            }
        }

        public override Fault UnknownTransaction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.unknownTransaction;
            }
        }
    }
}

