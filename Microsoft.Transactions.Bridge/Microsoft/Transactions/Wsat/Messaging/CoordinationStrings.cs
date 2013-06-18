namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class CoordinationStrings
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinationStrings()
        {
        }

        public static CoordinationStrings Version(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationStrings), "V");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return CoordinationStrings10.Instance;

                case ProtocolVersion.Version11:
                    return CoordinationStrings11.Instance;
            }
            return null;
        }

        public string ActivationCoordinatorPortType
        {
            get
            {
                return "ActivationCoordinatorPortType";
            }
        }

        public string AlreadyRegistered
        {
            get
            {
                return "AlreadyRegistered";
            }
        }

        public string CannotCreateContext
        {
            get
            {
                return "CannotCreateContext";
            }
        }

        public string CannotRegisterParticipant
        {
            get
            {
                return "CannotRegisterParticipant";
            }
        }

        public string ContextRefused
        {
            get
            {
                return "ContextRefused";
            }
        }

        public string CoordinationContext
        {
            get
            {
                return "CoordinationContext";
            }
        }

        public string CoordinationType
        {
            get
            {
                return "CoordinationType";
            }
        }

        public string CoordinatorProtocolService
        {
            get
            {
                return "CoordinatorProtocolService";
            }
        }

        public string CreateCoordinationContext
        {
            get
            {
                return "CreateCoordinationContext";
            }
        }

        public abstract string CreateCoordinationContextAction { get; }

        public string CreateCoordinationContextResponse
        {
            get
            {
                return "CreateCoordinationContextResponse";
            }
        }

        public abstract string CreateCoordinationContextResponseAction { get; }

        public string CurrentContext
        {
            get
            {
                return "CurrentContext";
            }
        }

        public string Expires
        {
            get
            {
                return "Expires";
            }
        }

        public abstract string FaultAction { get; }

        public string Identifier
        {
            get
            {
                return "Identifier";
            }
        }

        public string InvalidParameters
        {
            get
            {
                return "InvalidParameters";
            }
        }

        public string InvalidProtocol
        {
            get
            {
                return "InvalidProtocol";
            }
        }

        public string InvalidState
        {
            get
            {
                return "InvalidState";
            }
        }

        public abstract string Namespace { get; }

        public string NoActivity
        {
            get
            {
                return "NoActivity";
            }
        }

        public string ParticipantProtocolService
        {
            get
            {
                return "ParticipantProtocolService";
            }
        }

        public string Prefix
        {
            get
            {
                return "wscoor";
            }
        }

        public string Protocol
        {
            get
            {
                return "ProtocolIdentifier";
            }
        }

        public string Register
        {
            get
            {
                return "Register";
            }
        }

        public abstract string RegisterAction { get; }

        public string RegisterResponse
        {
            get
            {
                return "RegisterResponse";
            }
        }

        public abstract string RegisterResponseAction { get; }

        public string RegistrationCoordinatorPortType
        {
            get
            {
                return "RegistrationCoordinatorPortType";
            }
        }

        public string RegistrationService
        {
            get
            {
                return "RegistrationService";
            }
        }
    }
}

