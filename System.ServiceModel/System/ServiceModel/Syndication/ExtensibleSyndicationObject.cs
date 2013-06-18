namespace System.ServiceModel.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExtensibleSyndicationObject : IExtensibleSyndicationObject
    {
        private Dictionary<XmlQualifiedName, string> attributeExtensions;
        private SyndicationElementExtensionCollection elementExtensions;
        private ExtensibleSyndicationObject(ExtensibleSyndicationObject source)
        {
            if (source.attributeExtensions != null)
            {
                this.attributeExtensions = new Dictionary<XmlQualifiedName, string>();
                foreach (XmlQualifiedName name in source.attributeExtensions.Keys)
                {
                    this.attributeExtensions.Add(name, source.attributeExtensions[name]);
                }
            }
            else
            {
                this.attributeExtensions = null;
            }
            if (source.elementExtensions != null)
            {
                this.elementExtensions = new SyndicationElementExtensionCollection(source.elementExtensions);
            }
            else
            {
                this.elementExtensions = null;
            }
        }

        public Dictionary<XmlQualifiedName, string> AttributeExtensions
        {
            get
            {
                if (this.attributeExtensions == null)
                {
                    this.attributeExtensions = new Dictionary<XmlQualifiedName, string>();
                }
                return this.attributeExtensions;
            }
        }
        public SyndicationElementExtensionCollection ElementExtensions
        {
            get
            {
                if (this.elementExtensions == null)
                {
                    this.elementExtensions = new SyndicationElementExtensionCollection();
                }
                return this.elementExtensions;
            }
        }
        private static XmlBuffer CreateXmlBuffer(XmlDictionaryReader unparsedExtensionsReader, int maxExtensionSize)
        {
            XmlBuffer buffer = new XmlBuffer(maxExtensionSize);
            using (XmlDictionaryWriter writer = buffer.OpenSection(unparsedExtensionsReader.Quotas))
            {
                writer.WriteStartElement("extensionWrapper");
                while (unparsedExtensionsReader.IsStartElement())
                {
                    writer.WriteNode(unparsedExtensionsReader, false);
                }
                writer.WriteEndElement();
            }
            buffer.CloseSection();
            buffer.Close();
            return buffer;
        }

        internal void LoadElementExtensions(XmlReader readerOverUnparsedExtensions, int maxExtensionSize)
        {
            if (readerOverUnparsedExtensions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("readerOverUnparsedExtensions");
            }
            if (maxExtensionSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxExtensionSize"));
            }
            XmlDictionaryReader unparsedExtensionsReader = XmlDictionaryReader.CreateDictionaryReader(readerOverUnparsedExtensions);
            this.elementExtensions = new SyndicationElementExtensionCollection(CreateXmlBuffer(unparsedExtensionsReader, maxExtensionSize));
        }

        internal void LoadElementExtensions(XmlBuffer buffer)
        {
            this.elementExtensions = new SyndicationElementExtensionCollection(buffer);
        }

        internal void WriteAttributeExtensions(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (this.attributeExtensions != null)
            {
                foreach (XmlQualifiedName name in this.attributeExtensions.Keys)
                {
                    string str = this.attributeExtensions[name];
                    writer.WriteAttributeString(name.Name, name.Namespace, str);
                }
            }
        }

        internal void WriteElementExtensions(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (this.elementExtensions != null)
            {
                this.elementExtensions.WriteTo(writer);
            }
        }

        public ExtensibleSyndicationObject Clone()
        {
            return new ExtensibleSyndicationObject(this);
        }
    }
}

