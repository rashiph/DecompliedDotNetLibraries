namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class DotNetSecurityDictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;

        public DotNetSecurityDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2006/05/security", 0xa2);
            this.Prefix = dictionary.CreateString("dnse", 0xa3);
        }
    }
}

