namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class Message12Dictionary
    {
        public XmlDictionaryString FaultCode;
        public XmlDictionaryString FaultDetail;
        public XmlDictionaryString FaultNode;
        public XmlDictionaryString FaultReason;
        public XmlDictionaryString FaultRole;
        public XmlDictionaryString FaultSubcode;
        public XmlDictionaryString FaultText;
        public XmlDictionaryString FaultValue;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NotUnderstood;
        public XmlDictionaryString QName;
        public XmlDictionaryString Relay;
        public XmlDictionaryString Role;

        public Message12Dictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://www.w3.org/2003/05/soap-envelope", 2);
            this.Role = dictionary.CreateString("role", 0x45);
            this.Relay = dictionary.CreateString("relay", 70);
            this.FaultCode = dictionary.CreateString("Code", 0x47);
            this.FaultReason = dictionary.CreateString("Reason", 0x48);
            this.FaultText = dictionary.CreateString("Text", 0x49);
            this.FaultNode = dictionary.CreateString("Node", 0x4a);
            this.FaultRole = dictionary.CreateString("Role", 0x4b);
            this.FaultDetail = dictionary.CreateString("Detail", 0x4c);
            this.FaultValue = dictionary.CreateString("Value", 0x4d);
            this.FaultSubcode = dictionary.CreateString("Subcode", 0x4e);
            this.NotUnderstood = dictionary.CreateString("NotUnderstood", 0x4f);
            this.QName = dictionary.CreateString("qname", 80);
        }
    }
}

