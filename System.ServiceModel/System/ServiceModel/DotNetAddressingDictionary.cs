namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class DotNetAddressingDictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString RedirectTo;
        public XmlDictionaryString Via;

        public DotNetAddressingDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/06/addressingex", 0x6c);
            this.RedirectTo = dictionary.CreateString("RedirectTo", 0x6d);
            this.Via = dictionary.CreateString("Via", 110);
        }
    }
}

