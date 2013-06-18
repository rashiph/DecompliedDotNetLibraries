namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class AddressingDictionary
    {
        public XmlDictionaryString Action;
        public XmlDictionaryString Address;
        public XmlDictionaryString Dns;
        public XmlDictionaryString Empty;
        public XmlDictionaryString EndpointReference;
        public XmlDictionaryString FaultTo;
        public XmlDictionaryString From;
        public XmlDictionaryString Identity;
        public XmlDictionaryString IdentityExtensionNamespace;
        public XmlDictionaryString IsReferenceParameter;
        public XmlDictionaryString MessageId;
        public XmlDictionaryString PortName;
        public XmlDictionaryString PortType;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString ReferenceParameters;
        public XmlDictionaryString ReferenceProperties;
        public XmlDictionaryString RelatesTo;
        public XmlDictionaryString RelationshipType;
        public XmlDictionaryString Reply;
        public XmlDictionaryString ReplyTo;
        public XmlDictionaryString Rsa;
        public XmlDictionaryString ServiceName;
        public XmlDictionaryString Spn;
        public XmlDictionaryString To;
        public XmlDictionaryString Upn;
        public XmlDictionaryString X509v3Certificate;

        public AddressingDictionary(ServiceModelDictionary dictionary)
        {
            this.Action = dictionary.CreateString("Action", 5);
            this.To = dictionary.CreateString("To", 6);
            this.RelatesTo = dictionary.CreateString("RelatesTo", 9);
            this.MessageId = dictionary.CreateString("MessageID", 13);
            this.Address = dictionary.CreateString("Address", 0x15);
            this.ReplyTo = dictionary.CreateString("ReplyTo", 0x16);
            this.Empty = dictionary.CreateString("", 0x51);
            this.From = dictionary.CreateString("From", 0x52);
            this.FaultTo = dictionary.CreateString("FaultTo", 0x53);
            this.EndpointReference = dictionary.CreateString("EndpointReference", 0x54);
            this.PortType = dictionary.CreateString("PortType", 0x55);
            this.ServiceName = dictionary.CreateString("ServiceName", 0x56);
            this.PortName = dictionary.CreateString("PortName", 0x57);
            this.ReferenceProperties = dictionary.CreateString("ReferenceProperties", 0x58);
            this.RelationshipType = dictionary.CreateString("RelationshipType", 0x59);
            this.Reply = dictionary.CreateString("Reply", 90);
            this.Prefix = dictionary.CreateString("a", 0x5b);
            this.IdentityExtensionNamespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2006/02/addressingidentity", 0x5c);
            this.Identity = dictionary.CreateString("Identity", 0x5d);
            this.Spn = dictionary.CreateString("Spn", 0x5e);
            this.Upn = dictionary.CreateString("Upn", 0x5f);
            this.Rsa = dictionary.CreateString("Rsa", 0x60);
            this.Dns = dictionary.CreateString("Dns", 0x61);
            this.X509v3Certificate = dictionary.CreateString("X509v3Certificate", 0x62);
            this.ReferenceParameters = dictionary.CreateString("ReferenceParameters", 100);
            this.IsReferenceParameter = dictionary.CreateString("IsReferenceParameter", 0x65);
        }
    }
}

