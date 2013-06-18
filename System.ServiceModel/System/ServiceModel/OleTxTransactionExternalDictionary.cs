namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class OleTxTransactionExternalDictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString OleTxTransaction;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString PropagationToken;

        public OleTxTransactionExternalDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2006/02/tx/oletx", 0x160);
            this.Prefix = dictionary.CreateString("oletx", 0x161);
            this.OleTxTransaction = dictionary.CreateString("OleTxTransaction", 0x162);
            this.PropagationToken = dictionary.CreateString("PropagationToken", 0x163);
        }
    }
}

