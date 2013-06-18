namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), XmlRoot(ElementName="item", Namespace="")]
    public class Rss20ItemFormatter<TSyndicationItem> : Rss20ItemFormatter, IXmlSerializable where TSyndicationItem: SyndicationItem, new()
    {
        public Rss20ItemFormatter() : base(typeof(TSyndicationItem))
        {
        }

        public Rss20ItemFormatter(TSyndicationItem itemToWrite) : base(itemToWrite)
        {
        }

        public Rss20ItemFormatter(TSyndicationItem itemToWrite, bool serializeExtensionsAsAtom) : base(itemToWrite, serializeExtensionsAsAtom)
        {
        }

        protected override SyndicationItem CreateItemInstance()
        {
            return Activator.CreateInstance<TSyndicationItem>();
        }
    }
}

