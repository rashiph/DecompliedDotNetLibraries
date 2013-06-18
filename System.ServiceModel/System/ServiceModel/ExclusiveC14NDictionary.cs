namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class ExclusiveC14NDictionary
    {
        public XmlDictionaryString InclusiveNamespaces;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString PrefixList;

        public ExclusiveC14NDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://www.w3.org/2001/10/xml-exc-c14n#", 0x6f);
            this.PrefixList = dictionary.CreateString("PrefixList", 0x70);
            this.InclusiveNamespaces = dictionary.CreateString("InclusiveNamespaces", 0x71);
            this.Prefix = dictionary.CreateString("ec", 0x72);
        }
    }
}

