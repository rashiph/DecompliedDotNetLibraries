namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class Message11Dictionary
    {
        public XmlDictionaryString Actor;
        public XmlDictionaryString FaultActor;
        public XmlDictionaryString FaultCode;
        public XmlDictionaryString FaultDetail;
        public XmlDictionaryString FaultNamespace;
        public XmlDictionaryString FaultString;
        public XmlDictionaryString Namespace;

        public Message11Dictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/soap/envelope/", 0x1e1);
            this.Actor = dictionary.CreateString("actor", 0x1e2);
            this.FaultCode = dictionary.CreateString("faultcode", 0x1e3);
            this.FaultString = dictionary.CreateString("faultstring", 0x1e4);
            this.FaultActor = dictionary.CreateString("faultactor", 0x1e5);
            this.FaultDetail = dictionary.CreateString("detail", 0x1e6);
            this.FaultNamespace = dictionary.CreateString("", 0x51);
        }
    }
}

