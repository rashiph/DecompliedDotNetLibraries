namespace System.ServiceModel.Syndication
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class XmlSyndicationContent : SyndicationContent
    {
        private XmlBuffer contentBuffer;
        private SyndicationElementExtension extension;
        private string type;

        protected XmlSyndicationContent(XmlSyndicationContent source) : base(source)
        {
            if (source == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.contentBuffer = source.contentBuffer;
            this.extension = source.extension;
            this.type = source.type;
        }

        public XmlSyndicationContent(XmlReader reader)
        {
            if (reader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            SyndicationFeedFormatter.MoveToStartElement(reader);
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    string localName = reader.LocalName;
                    string namespaceURI = reader.NamespaceURI;
                    string str3 = reader.Value;
                    if ((localName == "type") && (namespaceURI == string.Empty))
                    {
                        this.type = str3;
                    }
                    else if (!FeedUtils.IsXmlns(localName, namespaceURI))
                    {
                        base.AttributeExtensions.Add(new XmlQualifiedName(localName, namespaceURI), str3);
                    }
                }
                reader.MoveToElement();
            }
            this.type = string.IsNullOrEmpty(this.type) ? "text/xml" : this.type;
            this.contentBuffer = new XmlBuffer(0x7fffffff);
            using (XmlDictionaryWriter writer = this.contentBuffer.OpenSection(XmlDictionaryReaderQuotas.Max))
            {
                writer.WriteNode(reader, false);
            }
            this.contentBuffer.CloseSection();
            this.contentBuffer.Close();
        }

        public XmlSyndicationContent(string type, SyndicationElementExtension extension)
        {
            if (extension == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("extension");
            }
            this.type = string.IsNullOrEmpty(type) ? "text/xml" : type;
            this.extension = extension;
        }

        public XmlSyndicationContent(string type, object dataContractExtension, XmlObjectSerializer dataContractSerializer)
        {
            this.type = string.IsNullOrEmpty(type) ? "text/xml" : type;
            this.extension = new SyndicationElementExtension(dataContractExtension, dataContractSerializer);
        }

        public XmlSyndicationContent(string type, object xmlSerializerExtension, XmlSerializer serializer)
        {
            this.type = string.IsNullOrEmpty(type) ? "text/xml" : type;
            this.extension = new SyndicationElementExtension(xmlSerializerExtension, serializer);
        }

        public override SyndicationContent Clone()
        {
            return new XmlSyndicationContent(this);
        }

        private void EnsureContentBuffer()
        {
            if (this.contentBuffer == null)
            {
                XmlBuffer buffer = new XmlBuffer(0x7fffffff);
                using (XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max))
                {
                    base.WriteTo(writer, "content", "http://www.w3.org/2005/Atom");
                }
                buffer.CloseSection();
                buffer.Close();
                this.contentBuffer = buffer;
            }
        }

        public XmlDictionaryReader GetReaderAtContent()
        {
            this.EnsureContentBuffer();
            return this.contentBuffer.GetReader(0);
        }

        public TContent ReadContent<TContent>()
        {
            return this.ReadContent<TContent>((XmlObjectSerializer) null);
        }

        public TContent ReadContent<TContent>(XmlObjectSerializer dataContractSerializer)
        {
            if (dataContractSerializer == null)
            {
                dataContractSerializer = new DataContractSerializer(typeof(TContent));
            }
            if (this.extension != null)
            {
                return this.extension.GetObject<TContent>(dataContractSerializer);
            }
            using (XmlDictionaryReader reader = this.contentBuffer.GetReader(0))
            {
                reader.ReadStartElement();
                return (TContent) dataContractSerializer.ReadObject(reader, false);
            }
        }

        public TContent ReadContent<TContent>(XmlSerializer serializer)
        {
            if (serializer == null)
            {
                serializer = new XmlSerializer(typeof(TContent));
            }
            if (this.extension != null)
            {
                return this.extension.GetObject<TContent>(serializer);
            }
            using (XmlDictionaryReader reader = this.contentBuffer.GetReader(0))
            {
                reader.ReadStartElement();
                return (TContent) serializer.Deserialize(reader);
            }
        }

        protected override void WriteContentsTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (this.extension != null)
            {
                this.extension.WriteTo(writer);
            }
            else if (this.contentBuffer != null)
            {
                using (XmlDictionaryReader reader = this.contentBuffer.GetReader(0))
                {
                    reader.MoveToStartElement();
                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement();
                        while ((reader.Depth >= 1) && (reader.ReadState == System.Xml.ReadState.Interactive))
                        {
                            writer.WriteNode(reader, false);
                        }
                    }
                }
            }
        }

        public SyndicationElementExtension Extension
        {
            get
            {
                return this.extension;
            }
        }

        public override string Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

