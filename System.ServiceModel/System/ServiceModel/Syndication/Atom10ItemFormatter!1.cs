namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="entry", Namespace="http://www.w3.org/2005/Atom"), TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class Atom10ItemFormatter<TSyndicationItem> : Atom10ItemFormatter where TSyndicationItem: SyndicationItem, new()
    {
        public Atom10ItemFormatter() : base(typeof(TSyndicationItem))
        {
        }

        public Atom10ItemFormatter(TSyndicationItem itemToWrite) : base(itemToWrite)
        {
        }

        protected override SyndicationItem CreateItemInstance()
        {
            return Activator.CreateInstance<TSyndicationItem>();
        }
    }
}

