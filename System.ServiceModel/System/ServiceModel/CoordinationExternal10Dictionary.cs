namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class CoordinationExternal10Dictionary
    {
        public XmlDictionaryString CreateCoordinationContextAction;
        public XmlDictionaryString CreateCoordinationContextResponseAction;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString RegisterAction;
        public XmlDictionaryString RegisterResponseAction;

        public CoordinationExternal10Dictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor", 0x164);
            this.CreateCoordinationContextAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContext", 0x171);
            this.CreateCoordinationContextResponseAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContextResponse", 370);
            this.RegisterAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/Register", 0x173);
            this.RegisterResponseAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/RegisterResponse", 0x174);
            this.FaultAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault", 0x175);
        }
    }
}

