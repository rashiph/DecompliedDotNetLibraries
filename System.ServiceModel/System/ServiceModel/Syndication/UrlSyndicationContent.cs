namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UrlSyndicationContent : SyndicationContent
    {
        private string mediaType;
        private Uri url;

        protected UrlSyndicationContent(UrlSyndicationContent source) : base(source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.url = source.url;
            this.mediaType = source.mediaType;
        }

        public UrlSyndicationContent(Uri url, string mediaType)
        {
            if (url == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("url");
            }
            this.url = url;
            this.mediaType = mediaType;
        }

        public override SyndicationContent Clone()
        {
            return new UrlSyndicationContent(this);
        }

        protected override void WriteContentsTo(XmlWriter writer)
        {
            writer.WriteAttributeString("src", string.Empty, FeedUtils.GetUriString(this.url));
        }

        public override string Type
        {
            get
            {
                return this.mediaType;
            }
        }

        public Uri Url
        {
            get
            {
                return this.url;
            }
        }
    }
}

