namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class DotNetOneWayDictionary
    {
        public XmlDictionaryString HeaderName;
        public XmlDictionaryString Namespace;

        public DotNetOneWayDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2005/05/routing", 0x1b5);
            this.HeaderName = dictionary.CreateString("PacketRoutable", 0x1b6);
        }
    }
}

