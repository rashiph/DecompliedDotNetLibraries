namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class CoordinationExternalDictionary
    {
        public XmlDictionaryString ActivationCoordinatorPortType;
        public XmlDictionaryString AlreadyRegistered;
        public XmlDictionaryString ContextRefused;
        public XmlDictionaryString CoordinationContext;
        public XmlDictionaryString CoordinationType;
        public XmlDictionaryString CoordinatorProtocolService;
        public XmlDictionaryString CreateCoordinationContext;
        public XmlDictionaryString CreateCoordinationContextResponse;
        public XmlDictionaryString CurrentContext;
        public XmlDictionaryString Expires;
        public XmlDictionaryString Identifier;
        public XmlDictionaryString InvalidParameters;
        public XmlDictionaryString InvalidProtocol;
        public XmlDictionaryString InvalidState;
        public XmlDictionaryString NoActivity;
        public XmlDictionaryString ParticipantProtocolService;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Protocol;
        public XmlDictionaryString Register;
        public XmlDictionaryString RegisterResponse;
        public XmlDictionaryString RegistrationCoordinatorPortType;
        public XmlDictionaryString RegistrationService;

        public CoordinationExternalDictionary(ServiceModelDictionary dictionary)
        {
            this.Prefix = dictionary.CreateString("wscoor", 0x165);
            this.CreateCoordinationContext = dictionary.CreateString("CreateCoordinationContext", 0x166);
            this.CreateCoordinationContextResponse = dictionary.CreateString("CreateCoordinationContextResponse", 0x167);
            this.CoordinationContext = dictionary.CreateString("CoordinationContext", 360);
            this.CurrentContext = dictionary.CreateString("CurrentContext", 0x169);
            this.CoordinationType = dictionary.CreateString("CoordinationType", 0x16a);
            this.RegistrationService = dictionary.CreateString("RegistrationService", 0x16b);
            this.Register = dictionary.CreateString("Register", 0x16c);
            this.RegisterResponse = dictionary.CreateString("RegisterResponse", 0x16d);
            this.Protocol = dictionary.CreateString("ProtocolIdentifier", 0x16e);
            this.CoordinatorProtocolService = dictionary.CreateString("CoordinatorProtocolService", 0x16f);
            this.ParticipantProtocolService = dictionary.CreateString("ParticipantProtocolService", 0x170);
            this.Expires = dictionary.CreateString("Expires", 0x37);
            this.Identifier = dictionary.CreateString("Identifier", 15);
            this.ActivationCoordinatorPortType = dictionary.CreateString("ActivationCoordinatorPortType", 0x176);
            this.RegistrationCoordinatorPortType = dictionary.CreateString("RegistrationCoordinatorPortType", 0x177);
            this.InvalidState = dictionary.CreateString("InvalidState", 0x178);
            this.InvalidProtocol = dictionary.CreateString("InvalidProtocol", 0x179);
            this.InvalidParameters = dictionary.CreateString("InvalidParameters", 0x17a);
            this.NoActivity = dictionary.CreateString("NoActivity", 0x17b);
            this.ContextRefused = dictionary.CreateString("ContextRefused", 380);
            this.AlreadyRegistered = dictionary.CreateString("AlreadyRegistered", 0x17d);
        }
    }
}

