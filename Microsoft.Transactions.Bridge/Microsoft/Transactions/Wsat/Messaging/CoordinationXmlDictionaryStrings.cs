namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class CoordinationXmlDictionaryStrings
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CoordinationXmlDictionaryStrings()
        {
        }

        public static CoordinationXmlDictionaryStrings Version(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationXmlDictionaryStrings), "V");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return CoordinationXmlDictionaryStrings10.Instance;

                case ProtocolVersion.Version11:
                    return CoordinationXmlDictionaryStrings11.Instance;
            }
            return null;
        }

        public XmlDictionaryString ActivationCoordinatorPortType
        {
            get
            {
                return XD.CoordinationExternalDictionary.ActivationCoordinatorPortType;
            }
        }

        public XmlDictionaryString AlreadyRegistered
        {
            get
            {
                return XD.CoordinationExternalDictionary.AlreadyRegistered;
            }
        }

        public XmlDictionaryString CannotCreateContext
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CannotCreateContext;
            }
        }

        public XmlDictionaryString CannotRegisterParticipant
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CannotRegisterParticipant;
            }
        }

        public XmlDictionaryString ContextRefused
        {
            get
            {
                return XD.CoordinationExternalDictionary.ContextRefused;
            }
        }

        public XmlDictionaryString CoordinationContext
        {
            get
            {
                return XD.CoordinationExternalDictionary.CoordinationContext;
            }
        }

        public XmlDictionaryString CoordinationType
        {
            get
            {
                return XD.CoordinationExternalDictionary.CoordinationType;
            }
        }

        public XmlDictionaryString CoordinatorProtocolService
        {
            get
            {
                return XD.CoordinationExternalDictionary.CoordinatorProtocolService;
            }
        }

        public XmlDictionaryString CreateCoordinationContext
        {
            get
            {
                return XD.CoordinationExternalDictionary.CreateCoordinationContext;
            }
        }

        public abstract XmlDictionaryString CreateCoordinationContextAction { get; }

        public XmlDictionaryString CreateCoordinationContextResponse
        {
            get
            {
                return XD.CoordinationExternalDictionary.CreateCoordinationContextResponse;
            }
        }

        public abstract XmlDictionaryString CreateCoordinationContextResponseAction { get; }

        public XmlDictionaryString CurrentContext
        {
            get
            {
                return XD.CoordinationExternalDictionary.CurrentContext;
            }
        }

        public XmlDictionaryString Expires
        {
            get
            {
                return XD.CoordinationExternalDictionary.Expires;
            }
        }

        public abstract XmlDictionaryString FaultAction { get; }

        public XmlDictionaryString Identifier
        {
            get
            {
                return XD.CoordinationExternalDictionary.Identifier;
            }
        }

        public XmlDictionaryString InvalidParameters
        {
            get
            {
                return XD.CoordinationExternalDictionary.InvalidParameters;
            }
        }

        public XmlDictionaryString InvalidProtocol
        {
            get
            {
                return XD.CoordinationExternalDictionary.InvalidProtocol;
            }
        }

        public XmlDictionaryString InvalidState
        {
            get
            {
                return XD.CoordinationExternalDictionary.InvalidState;
            }
        }

        public abstract XmlDictionaryString Namespace { get; }

        public XmlDictionaryString NoActivity
        {
            get
            {
                return XD.CoordinationExternalDictionary.NoActivity;
            }
        }

        public XmlDictionaryString ParticipantProtocolService
        {
            get
            {
                return XD.CoordinationExternalDictionary.ParticipantProtocolService;
            }
        }

        public XmlDictionaryString Prefix
        {
            get
            {
                return XD.CoordinationExternalDictionary.Prefix;
            }
        }

        public XmlDictionaryString Protocol
        {
            get
            {
                return XD.CoordinationExternalDictionary.Protocol;
            }
        }

        public XmlDictionaryString Register
        {
            get
            {
                return XD.CoordinationExternalDictionary.Register;
            }
        }

        public abstract XmlDictionaryString RegisterAction { get; }

        public XmlDictionaryString RegisterResponse
        {
            get
            {
                return XD.CoordinationExternalDictionary.RegisterResponse;
            }
        }

        public abstract XmlDictionaryString RegisterResponseAction { get; }

        public XmlDictionaryString RegistrationCoordinatorPortType
        {
            get
            {
                return XD.CoordinationExternalDictionary.RegistrationCoordinatorPortType;
            }
        }

        public XmlDictionaryString RegistrationService
        {
            get
            {
                return XD.CoordinationExternalDictionary.RegistrationService;
            }
        }
    }
}

