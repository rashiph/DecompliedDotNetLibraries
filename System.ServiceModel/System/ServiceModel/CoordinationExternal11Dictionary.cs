namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class CoordinationExternal11Dictionary
    {
        public XmlDictionaryString CannotCreateContext;
        public XmlDictionaryString CannotRegisterParticipant;
        public XmlDictionaryString CreateCoordinationContextAction;
        public XmlDictionaryString CreateCoordinationContextResponseAction;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString RegisterAction;
        public XmlDictionaryString RegisterResponseAction;

        public CoordinationExternal11Dictionary(XmlDictionary dictionary)
        {
            this.Namespace = dictionary.Add("http://docs.oasis-open.org/ws-tx/wscoor/2006/06");
            this.CreateCoordinationContextAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContext");
            this.CreateCoordinationContextResponseAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContextResponse");
            this.RegisterAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wscoor/2006/06/Register");
            this.RegisterResponseAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wscoor/2006/06/RegisterResponse");
            this.FaultAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wscoor/2006/06/fault");
            this.CannotCreateContext = dictionary.Add("CannotCreateContext");
            this.CannotRegisterParticipant = dictionary.Add("CannotRegisterParticipant");
        }
    }
}

