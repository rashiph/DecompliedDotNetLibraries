namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class MessageDictionary
    {
        public XmlDictionaryString Body;
        public XmlDictionaryString Envelope;
        public XmlDictionaryString Fault;
        public XmlDictionaryString Header;
        public XmlDictionaryString MustUnderstand;
        public XmlDictionaryString MustUnderstandFault;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;

        public MessageDictionary(ServiceModelDictionary dictionary)
        {
            this.MustUnderstand = dictionary.CreateString("mustUnderstand", 0);
            this.Envelope = dictionary.CreateString("Envelope", 1);
            this.Header = dictionary.CreateString("Header", 4);
            this.Body = dictionary.CreateString("Body", 7);
            this.Prefix = dictionary.CreateString("s", 0x42);
            this.Fault = dictionary.CreateString("Fault", 0x43);
            this.MustUnderstandFault = dictionary.CreateString("MustUnderstand", 0x44);
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2005/05/envelope/none", 440);
        }
    }
}

