namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class UtilityDictionary
    {
        public XmlDictionaryString CreatedElement;
        public XmlDictionaryString ExpiresElement;
        public XmlDictionaryString IdAttribute;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Timestamp;
        public XmlDictionaryString UniqueEndpointHeaderName;
        public XmlDictionaryString UniqueEndpointHeaderNamespace;

        public UtilityDictionary(ServiceModelDictionary dictionary)
        {
            this.IdAttribute = dictionary.CreateString("Id", 14);
            this.Namespace = dictionary.CreateString("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", 0x33);
            this.Timestamp = dictionary.CreateString("Timestamp", 0x35);
            this.CreatedElement = dictionary.CreateString("Created", 0x36);
            this.ExpiresElement = dictionary.CreateString("Expires", 0x37);
            this.Prefix = dictionary.CreateString("u", 0x131);
            this.UniqueEndpointHeaderName = dictionary.CreateString("ChannelInstance", 0x132);
            this.UniqueEndpointHeaderNamespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2005/02/duplex", 0x133);
        }
    }
}

