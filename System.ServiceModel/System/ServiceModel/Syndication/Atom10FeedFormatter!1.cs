namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="feed", Namespace="http://www.w3.org/2005/Atom"), TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class Atom10FeedFormatter<TSyndicationFeed> : Atom10FeedFormatter where TSyndicationFeed: SyndicationFeed, new()
    {
        public Atom10FeedFormatter() : base(typeof(TSyndicationFeed))
        {
        }

        public Atom10FeedFormatter(TSyndicationFeed feedToWrite) : base(feedToWrite)
        {
        }

        protected override SyndicationFeed CreateFeedInstance()
        {
            return Activator.CreateInstance<TSyndicationFeed>();
        }
    }
}

