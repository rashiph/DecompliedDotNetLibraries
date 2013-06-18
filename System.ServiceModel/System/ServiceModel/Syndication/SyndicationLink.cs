namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class SyndicationLink : IExtensibleSyndicationObject
    {
        private System.Uri baseUri;
        private ExtensibleSyndicationObject extensions;
        private long length;
        private string mediaType;
        private string relationshipType;
        private string title;
        private System.Uri uri;

        public SyndicationLink() : this(null, null, null, null, 0L)
        {
        }

        protected SyndicationLink(SyndicationLink source)
        {
            this.extensions = new ExtensibleSyndicationObject();
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.length = source.length;
            this.mediaType = source.mediaType;
            this.relationshipType = source.relationshipType;
            this.title = source.title;
            this.baseUri = source.baseUri;
            this.uri = source.uri;
            this.extensions = source.extensions.Clone();
        }

        public SyndicationLink(System.Uri uri) : this(uri, null, null, null, 0L)
        {
        }

        public SyndicationLink(System.Uri uri, string relationshipType, string title, string mediaType, long length)
        {
            this.extensions = new ExtensibleSyndicationObject();
            if (length < 0L)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("length"));
            }
            this.baseUri = null;
            this.uri = uri;
            this.title = title;
            this.relationshipType = relationshipType;
            this.mediaType = mediaType;
            this.length = length;
        }

        public virtual SyndicationLink Clone()
        {
            return new SyndicationLink(this);
        }

        public static SyndicationLink CreateAlternateLink(System.Uri uri)
        {
            return CreateAlternateLink(uri, null);
        }

        public static SyndicationLink CreateAlternateLink(System.Uri uri, string mediaType)
        {
            return new SyndicationLink(uri, "alternate", null, mediaType, 0L);
        }

        public static SyndicationLink CreateMediaEnclosureLink(System.Uri uri, string mediaType, long length)
        {
            return new SyndicationLink(uri, "enclosure", null, mediaType, length);
        }

        public static SyndicationLink CreateSelfLink(System.Uri uri)
        {
            return CreateSelfLink(uri, null);
        }

        public static SyndicationLink CreateSelfLink(System.Uri uri, string mediaType)
        {
            return new SyndicationLink(uri, "self", null, mediaType, 0L);
        }

        public System.Uri GetAbsoluteUri()
        {
            if (this.uri != null)
            {
                if (this.uri.IsAbsoluteUri)
                {
                    return this.uri;
                }
                if (this.baseUri != null)
                {
                    return new System.Uri(this.baseUri, this.uri);
                }
            }
            return null;
        }

        internal void LoadElementExtensions(XmlBuffer buffer)
        {
            this.extensions.LoadElementExtensions(buffer);
        }

        internal void LoadElementExtensions(XmlReader readerOverUnparsedExtensions, int maxExtensionSize)
        {
            this.extensions.LoadElementExtensions(readerOverUnparsedExtensions, maxExtensionSize);
        }

        protected internal virtual bool TryParseAttribute(string name, string ns, string value, string version)
        {
            return false;
        }

        protected internal virtual bool TryParseElement(XmlReader reader, string version)
        {
            return false;
        }

        protected internal virtual void WriteAttributeExtensions(XmlWriter writer, string version)
        {
            this.extensions.WriteAttributeExtensions(writer);
        }

        protected internal virtual void WriteElementExtensions(XmlWriter writer, string version)
        {
            this.extensions.WriteElementExtensions(writer);
        }

        public Dictionary<XmlQualifiedName, string> AttributeExtensions
        {
            get
            {
                return this.extensions.AttributeExtensions;
            }
        }

        public System.Uri BaseUri
        {
            get
            {
                return this.baseUri;
            }
            set
            {
                this.baseUri = value;
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions
        {
            get
            {
                return this.extensions.ElementExtensions;
            }
        }

        public long Length
        {
            get
            {
                return this.length;
            }
            set
            {
                if (value < 0L)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.length = value;
            }
        }

        public string MediaType
        {
            get
            {
                return this.mediaType;
            }
            set
            {
                this.mediaType = value;
            }
        }

        public string RelationshipType
        {
            get
            {
                return this.relationshipType;
            }
            set
            {
                this.relationshipType = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public System.Uri Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }
    }
}

