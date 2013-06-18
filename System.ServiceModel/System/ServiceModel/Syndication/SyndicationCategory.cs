namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class SyndicationCategory : IExtensibleSyndicationObject
    {
        private ExtensibleSyndicationObject extensions;
        private string label;
        private string name;
        private string scheme;

        public SyndicationCategory() : this((string) null)
        {
        }

        protected SyndicationCategory(SyndicationCategory source)
        {
            this.extensions = new ExtensibleSyndicationObject();
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.label = source.label;
            this.name = source.name;
            this.scheme = source.scheme;
            this.extensions = source.extensions.Clone();
        }

        public SyndicationCategory(string name) : this(name, null, null)
        {
        }

        public SyndicationCategory(string name, string scheme, string label)
        {
            this.extensions = new ExtensibleSyndicationObject();
            this.name = name;
            this.scheme = scheme;
            this.label = label;
        }

        public virtual SyndicationCategory Clone()
        {
            return new SyndicationCategory(this);
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

        public SyndicationElementExtensionCollection ElementExtensions
        {
            get
            {
                return this.extensions.ElementExtensions;
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
            set
            {
                this.label = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Scheme
        {
            get
            {
                return this.scheme;
            }
            set
            {
                this.scheme = value;
            }
        }
    }
}

