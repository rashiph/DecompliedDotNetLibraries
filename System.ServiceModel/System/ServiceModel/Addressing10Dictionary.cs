namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class Addressing10Dictionary
    {
        public XmlDictionaryString Anonymous;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString Metadata;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NoneAddress;
        public XmlDictionaryString ReplyRelationship;

        public Addressing10Dictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://www.w3.org/2005/08/addressing", 3);
            this.Anonymous = dictionary.CreateString("http://www.w3.org/2005/08/addressing/anonymous", 10);
            this.FaultAction = dictionary.CreateString("http://www.w3.org/2005/08/addressing/fault", 0x63);
            this.ReplyRelationship = dictionary.CreateString("http://www.w3.org/2005/08/addressing/reply", 0x66);
            this.NoneAddress = dictionary.CreateString("http://www.w3.org/2005/08/addressing/none", 0x67);
            this.Metadata = dictionary.CreateString("Metadata", 0x68);
        }
    }
}

