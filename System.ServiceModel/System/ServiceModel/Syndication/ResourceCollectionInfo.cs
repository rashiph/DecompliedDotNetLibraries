namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ResourceCollectionInfo : IExtensibleSyndicationObject
    {
        private Collection<string> accepts;
        private Uri baseUri;
        private Collection<CategoriesDocument> categories;
        private ExtensibleSyndicationObject extensions;
        private Uri link;
        private static IEnumerable<string> singleEmptyAccept;
        private TextSyndicationContent title;

        public ResourceCollectionInfo()
        {
            this.extensions = new ExtensibleSyndicationObject();
        }

        public ResourceCollectionInfo(TextSyndicationContent title, Uri link) : this(title, link, null, (IEnumerable<string>) null)
        {
        }

        public ResourceCollectionInfo(string title, Uri link) : this((title == null) ? null : new TextSyndicationContent(title), link)
        {
        }

        public ResourceCollectionInfo(TextSyndicationContent title, Uri link, IEnumerable<CategoriesDocument> categories, bool allowsNewEntries) : this(title, link, categories, allowsNewEntries ? null : CreateSingleEmptyAccept())
        {
        }

        public ResourceCollectionInfo(TextSyndicationContent title, Uri link, IEnumerable<CategoriesDocument> categories, IEnumerable<string> accepts)
        {
            this.extensions = new ExtensibleSyndicationObject();
            if (title == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("title");
            }
            if (link == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("link");
            }
            this.title = title;
            this.link = link;
            if (categories != null)
            {
                this.categories = new NullNotAllowedCollection<CategoriesDocument>();
                foreach (CategoriesDocument document in categories)
                {
                    this.categories.Add(document);
                }
            }
            if (accepts != null)
            {
                this.accepts = new NullNotAllowedCollection<string>();
                foreach (string str in accepts)
                {
                    this.accepts.Add(str);
                }
            }
        }

        protected internal virtual InlineCategoriesDocument CreateInlineCategoriesDocument()
        {
            return new InlineCategoriesDocument();
        }

        protected internal virtual ReferencedCategoriesDocument CreateReferencedCategoriesDocument()
        {
            return new ReferencedCategoriesDocument();
        }

        private static IEnumerable<string> CreateSingleEmptyAccept()
        {
            if (singleEmptyAccept == null)
            {
                singleEmptyAccept = new List<string>(1) { string.Empty }.AsReadOnly();
            }
            return singleEmptyAccept;
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

        public Collection<string> Accepts
        {
            get
            {
                if (this.accepts == null)
                {
                    this.accepts = new NullNotAllowedCollection<string>();
                }
                return this.accepts;
            }
        }

        public Dictionary<XmlQualifiedName, string> AttributeExtensions
        {
            get
            {
                return this.extensions.AttributeExtensions;
            }
        }

        public Uri BaseUri
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

        public Collection<CategoriesDocument> Categories
        {
            get
            {
                if (this.categories == null)
                {
                    this.categories = new NullNotAllowedCollection<CategoriesDocument>();
                }
                return this.categories;
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions
        {
            get
            {
                return this.extensions.ElementExtensions;
            }
        }

        public Uri Link
        {
            get
            {
                return this.link;
            }
            set
            {
                this.link = value;
            }
        }

        public TextSyndicationContent Title
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
    }
}

