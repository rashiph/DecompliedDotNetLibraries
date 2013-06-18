namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class CategoriesDocument : IExtensibleSyndicationObject
    {
        private Uri baseUri;
        private ExtensibleSyndicationObject extensions = new ExtensibleSyndicationObject();
        private string language;

        internal CategoriesDocument()
        {
        }

        public static InlineCategoriesDocument Create(Collection<SyndicationCategory> categories)
        {
            return new InlineCategoriesDocument(categories);
        }

        public static ReferencedCategoriesDocument Create(Uri linkToCategoriesDocument)
        {
            return new ReferencedCategoriesDocument(linkToCategoriesDocument);
        }

        public static InlineCategoriesDocument Create(Collection<SyndicationCategory> categories, bool isFixed, string scheme)
        {
            return new InlineCategoriesDocument(categories, isFixed, scheme);
        }

        public CategoriesDocumentFormatter GetFormatter()
        {
            return new AtomPub10CategoriesDocumentFormatter(this);
        }

        public static CategoriesDocument Load(XmlReader reader)
        {
            AtomPub10CategoriesDocumentFormatter formatter = new AtomPub10CategoriesDocumentFormatter();
            formatter.ReadFrom(reader);
            return formatter.Document;
        }

        internal void LoadElementExtensions(XmlBuffer buffer)
        {
            this.extensions.LoadElementExtensions(buffer);
        }

        internal void LoadElementExtensions(XmlReader readerOverUnparsedExtensions, int maxExtensionSize)
        {
            this.extensions.LoadElementExtensions(readerOverUnparsedExtensions, maxExtensionSize);
        }

        public void Save(XmlWriter writer)
        {
            this.GetFormatter().WriteTo(writer);
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

        public SyndicationElementExtensionCollection ElementExtensions
        {
            get
            {
                return this.extensions.ElementExtensions;
            }
        }

        internal abstract bool IsInline { get; }

        public string Language
        {
            get
            {
                return this.language;
            }
            set
            {
                this.language = value;
            }
        }
    }
}

