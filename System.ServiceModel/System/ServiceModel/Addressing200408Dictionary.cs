namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class Addressing200408Dictionary
    {
        public XmlDictionaryString Anonymous;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString Namespace;

        public Addressing200408Dictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/08/addressing", 0x69);
            this.Anonymous = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous", 0x6a);
            this.FaultAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", 0x6b);
        }
    }
}

