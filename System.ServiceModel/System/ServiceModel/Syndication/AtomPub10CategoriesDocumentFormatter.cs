namespace System.ServiceModel.Syndication
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), XmlRoot(ElementName="categories", Namespace="http://www.w3.org/2007/app")]
    public class AtomPub10CategoriesDocumentFormatter : CategoriesDocumentFormatter, IXmlSerializable
    {
        private Type inlineDocumentType;
        private int maxExtensionSize;
        private bool preserveAttributeExtensions;
        private bool preserveElementExtensions;
        private Type referencedDocumentType;

        public AtomPub10CategoriesDocumentFormatter() : this(typeof(InlineCategoriesDocument), typeof(ReferencedCategoriesDocument))
        {
        }

        public AtomPub10CategoriesDocumentFormatter(CategoriesDocument documentToWrite) : base(documentToWrite)
        {
            this.maxExtensionSize = 0x7fffffff;
            this.preserveAttributeExtensions = true;
            this.preserveElementExtensions = true;
            if (documentToWrite.IsInline)
            {
                this.inlineDocumentType = documentToWrite.GetType();
                this.referencedDocumentType = typeof(ReferencedCategoriesDocument);
            }
            else
            {
                this.referencedDocumentType = documentToWrite.GetType();
                this.inlineDocumentType = typeof(InlineCategoriesDocument);
            }
        }

        public AtomPub10CategoriesDocumentFormatter(Type inlineDocumentType, Type referencedDocumentType)
        {
            if (inlineDocumentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inlineDocumentType");
            }
            if (!typeof(InlineCategoriesDocument).IsAssignableFrom(inlineDocumentType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("inlineDocumentType", System.ServiceModel.SR.GetString("InvalidObjectTypePassed", new object[] { "inlineDocumentType", "InlineCategoriesDocument" }));
            }
            if (referencedDocumentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("referencedDocumentType");
            }
            if (!typeof(ReferencedCategoriesDocument).IsAssignableFrom(referencedDocumentType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("referencedDocumentType", System.ServiceModel.SR.GetString("InvalidObjectTypePassed", new object[] { "referencedDocumentType", "ReferencedCategoriesDocument" }));
            }
            this.maxExtensionSize = 0x7fffffff;
            this.preserveAttributeExtensions = true;
            this.preserveElementExtensions = true;
            this.inlineDocumentType = inlineDocumentType;
            this.referencedDocumentType = referencedDocumentType;
        }

        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return reader.IsStartElement("categories", "http://www.w3.org/2007/app");
        }

        protected override InlineCategoriesDocument CreateInlineCategoriesDocument()
        {
            if (this.inlineDocumentType == typeof(InlineCategoriesDocument))
            {
                return new InlineCategoriesDocument();
            }
            return (InlineCategoriesDocument) Activator.CreateInstance(this.inlineDocumentType);
        }

        protected override ReferencedCategoriesDocument CreateReferencedCategoriesDocument()
        {
            if (this.referencedDocumentType == typeof(ReferencedCategoriesDocument))
            {
                return new ReferencedCategoriesDocument();
            }
            return (ReferencedCategoriesDocument) Activator.CreateInstance(this.referencedDocumentType);
        }

        private void ReadDocument(XmlReader reader)
        {
            CreateInlineCategoriesDelegate inlineCategoriesFactory = null;
            CreateReferencedCategoriesDelegate referencedCategoriesFactory = null;
            try
            {
                SyndicationFeedFormatter.MoveToStartElement(reader);
                if (inlineCategoriesFactory == null)
                {
                    inlineCategoriesFactory = () => this.CreateInlineCategoriesDocument();
                }
                if (referencedCategoriesFactory == null)
                {
                    referencedCategoriesFactory = () => this.CreateReferencedCategoriesDocument();
                }
                this.SetDocument(AtomPub10ServiceDocumentFormatter.ReadCategories(reader, null, inlineCategoriesFactory, referencedCategoriesFactory, this.Version, this.preserveElementExtensions, this.preserveAttributeExtensions, this.maxExtensionSize));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDocument"), exception));
            }
            catch (ArgumentException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDocument"), exception2));
            }
        }

        public override void ReadFrom(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (!this.CanRead(reader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnknownDocumentXml", new object[] { reader.LocalName, reader.NamespaceURI })));
            }
            TraceCategoriesDocumentReadBegin();
            this.ReadDocument(reader);
            TraceCategoriesDocumentReadEnd();
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            TraceCategoriesDocumentReadBegin();
            this.ReadDocument(reader);
            TraceCategoriesDocumentReadEnd();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (base.Document == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("DocumentFormatterDoesNotHaveDocument")));
            }
            TraceCategoriesDocumentWriteBegin();
            this.WriteDocument(writer);
            TraceCategoriesDocumentWriteEnd();
        }

        internal static void TraceCategoriesDocumentReadBegin()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf002a, System.ServiceModel.SR.GetString("TraceCodeSyndicationReadCategoriesDocumentBegin"));
            }
        }

        internal static void TraceCategoriesDocumentReadEnd()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf002b, System.ServiceModel.SR.GetString("TraceCodeSyndicationReadCategoriesDocumentEnd"));
            }
        }

        internal static void TraceCategoriesDocumentWriteBegin()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf002e, System.ServiceModel.SR.GetString("TraceCodeSyndicationWriteCategoriesDocumentBegin"));
            }
        }

        internal static void TraceCategoriesDocumentWriteEnd()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf002f, System.ServiceModel.SR.GetString("TraceCodeSyndicationWriteCategoriesDocumentEnd"));
            }
        }

        private void WriteDocument(XmlWriter writer)
        {
            writer.WriteAttributeString("a10", "http://www.w3.org/2000/xmlns/", "http://www.w3.org/2005/Atom");
            AtomPub10ServiceDocumentFormatter.WriteCategoriesInnerXml(writer, base.Document, null, this.Version);
        }

        public override void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (base.Document == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("DocumentFormatterDoesNotHaveDocument")));
            }
            TraceCategoriesDocumentWriteBegin();
            writer.WriteStartElement("app", "categories", "http://www.w3.org/2007/app");
            this.WriteDocument(writer);
            writer.WriteEndElement();
            TraceCategoriesDocumentWriteEnd();
        }

        public override string Version
        {
            get
            {
                return "http://www.w3.org/2007/app";
            }
        }
    }
}

