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

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), XmlRoot(ElementName="service", Namespace="http://www.w3.org/2007/app")]
    public class AtomPub10ServiceDocumentFormatter : ServiceDocumentFormatter, IXmlSerializable
    {
        private Type documentType;
        private int maxExtensionSize;
        private bool preserveAttributeExtensions;
        private bool preserveElementExtensions;

        public AtomPub10ServiceDocumentFormatter() : this(typeof(ServiceDocument))
        {
        }

        public AtomPub10ServiceDocumentFormatter(ServiceDocument documentToWrite) : base(documentToWrite)
        {
            this.maxExtensionSize = 0x7fffffff;
            this.preserveAttributeExtensions = true;
            this.preserveElementExtensions = true;
            this.documentType = documentToWrite.GetType();
        }

        public AtomPub10ServiceDocumentFormatter(Type documentTypeToCreate)
        {
            if (documentTypeToCreate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("documentTypeToCreate");
            }
            if (!typeof(ServiceDocument).IsAssignableFrom(documentTypeToCreate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("documentTypeToCreate", System.ServiceModel.SR.GetString("InvalidObjectTypePassed", new object[] { "documentTypeToCreate", "ServiceDocument" }));
            }
            this.maxExtensionSize = 0x7fffffff;
            this.preserveAttributeExtensions = true;
            this.preserveElementExtensions = true;
            this.documentType = documentTypeToCreate;
        }

        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return reader.IsStartElement("service", "http://www.w3.org/2007/app");
        }

        protected override ServiceDocument CreateDocumentInstance()
        {
            if (this.documentType == typeof(ServiceDocument))
            {
                return new ServiceDocument();
            }
            return (ServiceDocument) Activator.CreateInstance(this.documentType);
        }

        internal static CategoriesDocument ReadCategories(XmlReader reader, Uri baseUri, CreateInlineCategoriesDelegate inlineCategoriesFactory, CreateReferencedCategoriesDelegate referencedCategoriesFactory, string version, bool preserveElementExtensions, bool preserveAttributeExtensions, int maxExtensionSize)
        {
            string attribute = reader.GetAttribute("href", string.Empty);
            if (string.IsNullOrEmpty(attribute))
            {
                InlineCategoriesDocument inlineCategories = inlineCategoriesFactory();
                ReadInlineCategories(reader, inlineCategories, baseUri, version, preserveElementExtensions, preserveAttributeExtensions, maxExtensionSize);
                return inlineCategories;
            }
            ReferencedCategoriesDocument referencedCategories = referencedCategoriesFactory();
            ReadReferencedCategories(reader, referencedCategories, baseUri, new Uri(attribute, UriKind.RelativeOrAbsolute), version, preserveElementExtensions, preserveAttributeExtensions, maxExtensionSize);
            return referencedCategories;
        }

        private ResourceCollectionInfo ReadCollection(XmlReader reader, Workspace workspace)
        {
            CreateInlineCategoriesDelegate inlineCategoriesFactory = null;
            CreateReferencedCategoriesDelegate referencedCategoriesFactory = null;
            ResourceCollectionInfo result = ServiceDocumentFormatter.CreateCollection(workspace);
            result.BaseUri = workspace.BaseUri;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                    {
                        result.BaseUri = FeedUtils.CombineXmlBase(result.BaseUri, reader.Value);
                    }
                    else
                    {
                        if ((reader.LocalName == "href") && (reader.NamespaceURI == string.Empty))
                        {
                            result.Link = new Uri(reader.Value, UriKind.RelativeOrAbsolute);
                            continue;
                        }
                        string namespaceURI = reader.NamespaceURI;
                        string localName = reader.LocalName;
                        if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                        {
                            string str3 = reader.Value;
                            if (!ServiceDocumentFormatter.TryParseAttribute(localName, namespaceURI, str3, result, this.Version))
                            {
                                if (this.preserveAttributeExtensions)
                                {
                                    result.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
            }
            XmlBuffer buffer = null;
            XmlDictionaryWriter extWriter = null;
            reader.ReadStartElement();
            try
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement("title", "http://www.w3.org/2005/Atom"))
                    {
                        result.Title = Atom10FeedFormatter.ReadTextContentFrom(reader, "//app:service/app:workspace/app:collection/atom:title[@type]", this.preserveAttributeExtensions);
                    }
                    else
                    {
                        if (reader.IsStartElement("categories", "http://www.w3.org/2007/app"))
                        {
                            if (inlineCategoriesFactory == null)
                            {
                                inlineCategoriesFactory = () => ServiceDocumentFormatter.CreateInlineCategories(result);
                            }
                            if (referencedCategoriesFactory == null)
                            {
                                referencedCategoriesFactory = () => ServiceDocumentFormatter.CreateReferencedCategories(result);
                            }
                            result.Categories.Add(ReadCategories(reader, result.BaseUri, inlineCategoriesFactory, referencedCategoriesFactory, this.Version, this.preserveElementExtensions, this.preserveAttributeExtensions, this.maxExtensionSize));
                            continue;
                        }
                        if (reader.IsStartElement("accept", "http://www.w3.org/2007/app"))
                        {
                            result.Accepts.Add(reader.ReadElementString());
                        }
                        else if (!ServiceDocumentFormatter.TryParseElement(reader, result, this.Version))
                        {
                            if (this.preserveElementExtensions)
                            {
                                SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref extWriter, reader, this.maxExtensionSize);
                                continue;
                            }
                            SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            reader.Skip();
                        }
                    }
                }
                ServiceDocumentFormatter.LoadElementExtensions(buffer, extWriter, result);
            }
            finally
            {
                if (extWriter != null)
                {
                    extWriter.Close();
                }
            }
            reader.ReadEndElement();
            return result;
        }

        private void ReadDocument(XmlReader reader)
        {
            ServiceDocument document = this.CreateDocumentInstance();
            try
            {
                SyndicationFeedFormatter.MoveToStartElement(reader);
                bool isEmptyElement = reader.IsEmptyElement;
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if ((reader.LocalName == "lang") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                        {
                            document.Language = reader.Value;
                        }
                        else
                        {
                            if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                            {
                                document.BaseUri = new Uri(reader.Value, UriKind.RelativeOrAbsolute);
                                continue;
                            }
                            string namespaceURI = reader.NamespaceURI;
                            string localName = reader.LocalName;
                            if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                            {
                                string str3 = reader.Value;
                                if (!ServiceDocumentFormatter.TryParseAttribute(localName, namespaceURI, str3, document, this.Version))
                                {
                                    if (this.preserveAttributeExtensions)
                                    {
                                        document.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                        continue;
                                    }
                                    SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                }
                            }
                        }
                    }
                }
                XmlBuffer buffer = null;
                XmlDictionaryWriter extWriter = null;
                reader.ReadStartElement();
                if (!isEmptyElement)
                {
                    try
                    {
                        while (reader.IsStartElement())
                        {
                            if (reader.IsStartElement("workspace", "http://www.w3.org/2007/app"))
                            {
                                document.Workspaces.Add(this.ReadWorkspace(reader, document));
                            }
                            else if (!ServiceDocumentFormatter.TryParseElement(reader, document, this.Version))
                            {
                                if (this.preserveElementExtensions)
                                {
                                    SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref extWriter, reader, this.maxExtensionSize);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                reader.Skip();
                            }
                        }
                        ServiceDocumentFormatter.LoadElementExtensions(buffer, extWriter, document);
                    }
                    finally
                    {
                        if (extWriter != null)
                        {
                            extWriter.Close();
                        }
                    }
                }
                reader.ReadEndElement();
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDocument"), exception));
            }
            catch (ArgumentException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(FeedUtils.AddLineInfo(reader, "ErrorParsingDocument"), exception2));
            }
            this.SetDocument(document);
        }

        public override void ReadFrom(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            reader.MoveToContent();
            if (!this.CanRead(reader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnknownDocumentXml", new object[] { reader.LocalName, reader.NamespaceURI })));
            }
            TraceServiceDocumentReadBegin();
            this.ReadDocument(reader);
            TraceServiceDocumentReadEnd();
        }

        private static void ReadInlineCategories(XmlReader reader, InlineCategoriesDocument inlineCategories, Uri baseUri, string version, bool preserveElementExtensions, bool preserveAttributeExtensions, int maxExtensionSize)
        {
            inlineCategories.BaseUri = baseUri;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                    {
                        inlineCategories.BaseUri = FeedUtils.CombineXmlBase(inlineCategories.BaseUri, reader.Value);
                    }
                    else
                    {
                        if ((reader.LocalName == "lang") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                        {
                            inlineCategories.Language = reader.Value;
                            continue;
                        }
                        if ((reader.LocalName == "fixed") && (reader.NamespaceURI == string.Empty))
                        {
                            inlineCategories.IsFixed = reader.Value == "yes";
                            continue;
                        }
                        if ((reader.LocalName == "scheme") && (reader.NamespaceURI == string.Empty))
                        {
                            inlineCategories.Scheme = reader.Value;
                            continue;
                        }
                        string namespaceURI = reader.NamespaceURI;
                        string localName = reader.LocalName;
                        if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                        {
                            string str3 = reader.Value;
                            if (!ServiceDocumentFormatter.TryParseAttribute(localName, namespaceURI, str3, inlineCategories, version))
                            {
                                if (preserveAttributeExtensions)
                                {
                                    inlineCategories.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
            }
            SyndicationFeedFormatter.MoveToStartElement(reader);
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                XmlBuffer buffer = null;
                XmlDictionaryWriter extWriter = null;
                try
                {
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement("category", "http://www.w3.org/2005/Atom"))
                        {
                            SyndicationCategory category = ServiceDocumentFormatter.CreateCategory(inlineCategories);
                            Atom10FeedFormatter.ReadCategory(reader, category, version, preserveAttributeExtensions, preserveElementExtensions, maxExtensionSize);
                            if (category.Scheme == null)
                            {
                                category.Scheme = inlineCategories.Scheme;
                            }
                            inlineCategories.Categories.Add(category);
                        }
                        else if (!ServiceDocumentFormatter.TryParseElement(reader, inlineCategories, version))
                        {
                            if (preserveElementExtensions)
                            {
                                SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref extWriter, reader, maxExtensionSize);
                                continue;
                            }
                            SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            reader.Skip();
                        }
                    }
                    ServiceDocumentFormatter.LoadElementExtensions(buffer, extWriter, inlineCategories);
                }
                finally
                {
                    if (extWriter != null)
                    {
                        extWriter.Close();
                    }
                }
                reader.ReadEndElement();
            }
        }

        private static void ReadReferencedCategories(XmlReader reader, ReferencedCategoriesDocument referencedCategories, Uri baseUri, Uri link, string version, bool preserveElementExtensions, bool preserveAttributeExtensions, int maxExtensionSize)
        {
            referencedCategories.BaseUri = baseUri;
            referencedCategories.Link = link;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                    {
                        referencedCategories.BaseUri = FeedUtils.CombineXmlBase(referencedCategories.BaseUri, reader.Value);
                    }
                    else
                    {
                        if ((reader.LocalName == "lang") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                        {
                            referencedCategories.Language = reader.Value;
                            continue;
                        }
                        if ((reader.LocalName != "href") || (reader.NamespaceURI != string.Empty))
                        {
                            string namespaceURI = reader.NamespaceURI;
                            string localName = reader.LocalName;
                            if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                            {
                                string str3 = reader.Value;
                                if (!ServiceDocumentFormatter.TryParseAttribute(localName, namespaceURI, str3, referencedCategories, version))
                                {
                                    if (preserveAttributeExtensions)
                                    {
                                        referencedCategories.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                        continue;
                                    }
                                    SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                }
                            }
                        }
                    }
                }
            }
            reader.MoveToElement();
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                XmlBuffer buffer = null;
                XmlDictionaryWriter extWriter = null;
                try
                {
                    while (reader.IsStartElement())
                    {
                        if (!ServiceDocumentFormatter.TryParseElement(reader, referencedCategories, version))
                        {
                            if (preserveElementExtensions)
                            {
                                SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref extWriter, reader, maxExtensionSize);
                            }
                            else
                            {
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                                reader.Skip();
                            }
                        }
                    }
                    ServiceDocumentFormatter.LoadElementExtensions(buffer, extWriter, referencedCategories);
                }
                finally
                {
                    if (extWriter != null)
                    {
                        extWriter.Close();
                    }
                }
                reader.ReadEndElement();
            }
        }

        private Workspace ReadWorkspace(XmlReader reader, ServiceDocument document)
        {
            Workspace workspace = ServiceDocumentFormatter.CreateWorkspace(document);
            workspace.BaseUri = document.BaseUri;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.LocalName == "base") && (reader.NamespaceURI == "http://www.w3.org/XML/1998/namespace"))
                    {
                        workspace.BaseUri = FeedUtils.CombineXmlBase(workspace.BaseUri, reader.Value);
                    }
                    else
                    {
                        string namespaceURI = reader.NamespaceURI;
                        string localName = reader.LocalName;
                        if (!FeedUtils.IsXmlns(localName, namespaceURI) && !FeedUtils.IsXmlSchemaType(localName, namespaceURI))
                        {
                            string str3 = reader.Value;
                            if (!ServiceDocumentFormatter.TryParseAttribute(localName, namespaceURI, str3, workspace, this.Version))
                            {
                                if (this.preserveAttributeExtensions)
                                {
                                    workspace.AttributeExtensions.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.Value);
                                    continue;
                                }
                                SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            }
                        }
                    }
                }
            }
            XmlBuffer buffer = null;
            XmlDictionaryWriter extWriter = null;
            reader.ReadStartElement();
            try
            {
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement("title", "http://www.w3.org/2005/Atom"))
                    {
                        workspace.Title = Atom10FeedFormatter.ReadTextContentFrom(reader, "//app:service/app:workspace/atom:title[@type]", this.preserveAttributeExtensions);
                    }
                    else
                    {
                        if (reader.IsStartElement("collection", "http://www.w3.org/2007/app"))
                        {
                            workspace.Collections.Add(this.ReadCollection(reader, workspace));
                            continue;
                        }
                        if (!ServiceDocumentFormatter.TryParseElement(reader, workspace, this.Version))
                        {
                            if (this.preserveElementExtensions)
                            {
                                SyndicationFeedFormatter.CreateBufferIfRequiredAndWriteNode(ref buffer, ref extWriter, reader, this.maxExtensionSize);
                                continue;
                            }
                            SyndicationFeedFormatter.TraceSyndicationElementIgnoredOnRead(reader);
                            reader.Skip();
                        }
                    }
                }
                ServiceDocumentFormatter.LoadElementExtensions(buffer, extWriter, workspace);
            }
            finally
            {
                if (extWriter != null)
                {
                    extWriter.Close();
                }
            }
            reader.ReadEndElement();
            return workspace;
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
            TraceServiceDocumentReadBegin();
            this.ReadDocument(reader);
            TraceServiceDocumentReadEnd();
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
            TraceServiceDocumentWriteBegin();
            this.WriteDocument(writer);
            TraceServiceDocumentWriteEnd();
        }

        internal static void TraceServiceDocumentReadBegin()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0028, System.ServiceModel.SR.GetString("TraceCodeSyndicationReadServiceDocumentBegin"));
            }
        }

        internal static void TraceServiceDocumentReadEnd()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0029, System.ServiceModel.SR.GetString("TraceCodeSyndicationReadServiceDocumentEnd"));
            }
        }

        internal static void TraceServiceDocumentWriteBegin()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf002c, System.ServiceModel.SR.GetString("TraceCodeSyndicationWriteServiceDocumentBegin"));
            }
        }

        internal static void TraceServiceDocumentWriteEnd()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf002d, System.ServiceModel.SR.GetString("TraceCodeSyndicationWriteServiceDocumentEnd"));
            }
        }

        private static void WriteCategories(XmlWriter writer, CategoriesDocument categories, Uri baseUri, string version)
        {
            writer.WriteStartElement("app", "categories", "http://www.w3.org/2007/app");
            WriteCategoriesInnerXml(writer, categories, baseUri, version);
            writer.WriteEndElement();
        }

        internal static void WriteCategoriesInnerXml(XmlWriter writer, CategoriesDocument categories, Uri baseUri, string version)
        {
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, categories.BaseUri);
            if (baseUriToWrite != null)
            {
                WriteXmlBase(writer, baseUriToWrite);
            }
            if (!string.IsNullOrEmpty(categories.Language))
            {
                WriteXmlLang(writer, categories.Language);
            }
            if (categories.IsInline)
            {
                WriteInlineCategoriesContent(writer, (InlineCategoriesDocument) categories, version);
            }
            else
            {
                WriteReferencedCategoriesContent(writer, (ReferencedCategoriesDocument) categories, version);
            }
        }

        private void WriteCollection(XmlWriter writer, ResourceCollectionInfo collection, Uri baseUri)
        {
            writer.WriteStartElement("app", "collection", "http://www.w3.org/2007/app");
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, collection.BaseUri);
            if (baseUriToWrite != null)
            {
                baseUri = collection.BaseUri;
                WriteXmlBase(writer, baseUriToWrite);
            }
            if (collection.Link != null)
            {
                writer.WriteAttributeString("href", FeedUtils.GetUriString(collection.Link));
            }
            ServiceDocumentFormatter.WriteAttributeExtensions(writer, collection, this.Version);
            if (collection.Title != null)
            {
                collection.Title.WriteTo(writer, "title", "http://www.w3.org/2005/Atom");
            }
            for (int i = 0; i < collection.Accepts.Count; i++)
            {
                writer.WriteElementString("app", "accept", "http://www.w3.org/2007/app", collection.Accepts[i]);
            }
            for (int j = 0; j < collection.Categories.Count; j++)
            {
                WriteCategories(writer, collection.Categories[j], baseUri, this.Version);
            }
            ServiceDocumentFormatter.WriteElementExtensions(writer, collection, this.Version);
            writer.WriteEndElement();
        }

        private void WriteDocument(XmlWriter writer)
        {
            writer.WriteAttributeString("a10", "http://www.w3.org/2000/xmlns/", "http://www.w3.org/2005/Atom");
            if (!string.IsNullOrEmpty(base.Document.Language))
            {
                WriteXmlLang(writer, base.Document.Language);
            }
            Uri baseUri = base.Document.BaseUri;
            if (baseUri != null)
            {
                WriteXmlBase(writer, baseUri);
            }
            ServiceDocumentFormatter.WriteAttributeExtensions(writer, base.Document, this.Version);
            for (int i = 0; i < base.Document.Workspaces.Count; i++)
            {
                this.WriteWorkspace(writer, base.Document.Workspaces[i], baseUri);
            }
            ServiceDocumentFormatter.WriteElementExtensions(writer, base.Document, this.Version);
        }

        private static void WriteInlineCategoriesContent(XmlWriter writer, InlineCategoriesDocument categories, string version)
        {
            if (!string.IsNullOrEmpty(categories.Scheme))
            {
                writer.WriteAttributeString("scheme", categories.Scheme);
            }
            if (categories.IsFixed)
            {
                writer.WriteAttributeString("fixed", "yes");
            }
            ServiceDocumentFormatter.WriteAttributeExtensions(writer, categories, version);
            for (int i = 0; i < categories.Categories.Count; i++)
            {
                Atom10FeedFormatter.WriteCategory(writer, categories.Categories[i], version);
            }
            ServiceDocumentFormatter.WriteElementExtensions(writer, categories, version);
        }

        private static void WriteReferencedCategoriesContent(XmlWriter writer, ReferencedCategoriesDocument categories, string version)
        {
            if (categories.Link != null)
            {
                writer.WriteAttributeString("href", FeedUtils.GetUriString(categories.Link));
            }
            ServiceDocumentFormatter.WriteAttributeExtensions(writer, categories, version);
            ServiceDocumentFormatter.WriteElementExtensions(writer, categories, version);
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
            TraceServiceDocumentWriteBegin();
            writer.WriteStartElement("app", "service", "http://www.w3.org/2007/app");
            this.WriteDocument(writer);
            writer.WriteEndElement();
            TraceServiceDocumentWriteEnd();
        }

        private void WriteWorkspace(XmlWriter writer, Workspace workspace, Uri baseUri)
        {
            writer.WriteStartElement("app", "workspace", "http://www.w3.org/2007/app");
            Uri baseUriToWrite = FeedUtils.GetBaseUriToWrite(baseUri, workspace.BaseUri);
            if (baseUriToWrite != null)
            {
                baseUri = workspace.BaseUri;
                WriteXmlBase(writer, baseUriToWrite);
            }
            ServiceDocumentFormatter.WriteAttributeExtensions(writer, workspace, this.Version);
            if (workspace.Title != null)
            {
                workspace.Title.WriteTo(writer, "title", "http://www.w3.org/2005/Atom");
            }
            for (int i = 0; i < workspace.Collections.Count; i++)
            {
                this.WriteCollection(writer, workspace.Collections[i], baseUri);
            }
            ServiceDocumentFormatter.WriteElementExtensions(writer, workspace, this.Version);
            writer.WriteEndElement();
        }

        private static void WriteXmlBase(XmlWriter writer, Uri baseUri)
        {
            writer.WriteAttributeString("xml", "base", "http://www.w3.org/XML/1998/namespace", FeedUtils.GetUriString(baseUri));
        }

        private static void WriteXmlLang(XmlWriter writer, string lang)
        {
            writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", lang);
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

