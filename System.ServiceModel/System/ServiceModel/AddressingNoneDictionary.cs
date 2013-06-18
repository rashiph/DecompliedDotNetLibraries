namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class AddressingNoneDictionary
    {
        public XmlDictionaryString Namespace;

        public AddressingNoneDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2005/05/addressing/none", 0x1b7);
        }
    }
}

