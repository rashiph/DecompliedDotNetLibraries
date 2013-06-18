namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class PeerWireStringsDictionary
    {
        public XmlDictionaryString Demuxer;
        public XmlDictionaryString FloodAction;
        public XmlDictionaryString HopCount;
        public XmlDictionaryString HopCountNamespace;
        public XmlDictionaryString LinkUtilityAction;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString PeerTo;
        public XmlDictionaryString PeerVia;

        public PeerWireStringsDictionary(ServiceModelDictionary dictionary)
        {
            this.FloodAction = dictionary.CreateString("FloodMessage", 0x1ad);
            this.LinkUtilityAction = dictionary.CreateString("LinkUtility", 430);
            this.HopCount = dictionary.CreateString("Hops", 0x1af);
            this.HopCountNamespace = dictionary.CreateString("http://schemas.microsoft.com/net/2006/05/peer/HopCount", 0x1b0);
            this.PeerVia = dictionary.CreateString("PeerVia", 0x1b1);
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/net/2006/05/peer", 0x1b2);
            this.Demuxer = dictionary.CreateString("PeerFlooder", 0x1b3);
            this.PeerTo = dictionary.CreateString("PeerTo", 0x1b4);
        }
    }
}

