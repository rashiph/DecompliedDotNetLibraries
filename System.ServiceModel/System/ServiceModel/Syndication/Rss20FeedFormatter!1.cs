namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), XmlRoot(ElementName="rss", Namespace="")]
    public class Rss20FeedFormatter<TSyndicationFeed> : Rss20FeedFormatter where TSyndicationFeed: SyndicationFeed, new()
    {
        public Rss20FeedFormatter() : base(typeof(TSyndicationFeed))
        {
        }

        public Rss20FeedFormatter(TSyndicationFeed feedToWrite) : base(feedToWrite)
        {
        }

        public Rss20FeedFormatter(TSyndicationFeed feedToWrite, bool serializeExtensionsAsAtom) : base(feedToWrite, serializeExtensionsAsAtom)
        {
        }

        protected override SyndicationFeed CreateFeedInstance()
        {
            return Activator.CreateInstance<TSyndicationFeed>();
        }
    }
}

