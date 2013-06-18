namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class PolicyDictionary
    {
        public XmlDictionaryString Namespace;

        public PolicyDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2002/12/policy", 0x1ac);
        }
    }
}

