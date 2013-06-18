namespace System.ServiceModel.Syndication
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class SyndicationElementExtension
    {
        private XmlBuffer buffer;
        private int bufferElementIndex;
        private object extensionData;
        private ExtensionDataWriter extensionDataWriter;
        private string outerName;
        private string outerNamespace;

        public SyndicationElementExtension(object dataContractExtension) : this(dataContractExtension, (XmlObjectSerializer) null)
        {
        }

        public SyndicationElementExtension(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
            }
            SyndicationFeedFormatter.MoveToStartElement(xmlReader);
            this.outerName = xmlReader.LocalName;
            this.outerNamespace = xmlReader.NamespaceURI;
            this.buffer = new XmlBuffer(0x7fffffff);
            using (XmlDictionaryWriter writer = this.buffer.OpenSection(XmlDictionaryReaderQuotas.Max))
            {
                writer.WriteStartElement("extensionWrapper");
                writer.WriteNode(xmlReader, false);
                writer.WriteEndElement();
            }
            this.buffer.CloseSection();
            this.buffer.Close();
            this.bufferElementIndex = 0;
        }

        public SyndicationElementExtension(object dataContractExtension, XmlObjectSerializer dataContractSerializer) : this(null, null, dataContractExtension, dataContractSerializer)
        {
        }

        public SyndicationElementExtension(object xmlSerializerExtension, XmlSerializer serializer)
        {
            if (xmlSerializerExtension == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlSerializerExtension");
            }
            if (serializer == null)
            {
                serializer = new XmlSerializer(xmlSerializerExtension.GetType());
            }
            this.extensionData = xmlSerializerExtension;
            this.extensionDataWriter = new ExtensionDataWriter(this.extensionData, serializer);
        }

        public SyndicationElementExtension(string outerName, string outerNamespace, object dataContractExtension) : this(outerName, outerNamespace, dataContractExtension, null)
        {
        }

        internal SyndicationElementExtension(XmlBuffer buffer, int bufferElementIndex, string outerName, string outerNamespace)
        {
            this.buffer = buffer;
            this.bufferElementIndex = bufferElementIndex;
            this.outerName = outerName;
            this.outerNamespace = outerNamespace;
        }

        public SyndicationElementExtension(string outerName, string outerNamespace, object dataContractExtension, XmlObjectSerializer dataContractSerializer)
        {
            if (dataContractExtension == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dataContractExtension");
            }
            if (outerName == string.Empty)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("OuterNameOfElementExtensionEmpty"));
            }
            if (dataContractSerializer == null)
            {
                dataContractSerializer = new DataContractSerializer(dataContractExtension.GetType());
            }
            this.outerName = outerName;
            this.outerNamespace = outerNamespace;
            this.extensionData = dataContractExtension;
            this.extensionDataWriter = new ExtensionDataWriter(this.extensionData, dataContractSerializer, this.outerName, this.outerNamespace);
        }

        private void EnsureBuffer()
        {
            if (this.buffer == null)
            {
                this.buffer = new XmlBuffer(0x7fffffff);
                using (XmlDictionaryWriter writer = this.buffer.OpenSection(XmlDictionaryReaderQuotas.Max))
                {
                    writer.WriteStartElement("extensionWrapper");
                    this.WriteTo(writer);
                    writer.WriteEndElement();
                }
                this.buffer.CloseSection();
                this.buffer.Close();
                this.bufferElementIndex = 0;
            }
        }

        private void EnsureOuterNameAndNs()
        {
            this.extensionDataWriter.ComputeOuterNameAndNs(out this.outerName, out this.outerNamespace);
        }

        public TExtension GetObject<TExtension>()
        {
            return this.GetObject<TExtension>(new DataContractSerializer(typeof(TExtension)));
        }

        public TExtension GetObject<TExtension>(XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            if ((this.extensionData != null) && typeof(TExtension).IsAssignableFrom(this.extensionData.GetType()))
            {
                return (TExtension) this.extensionData;
            }
            using (XmlReader reader = this.GetReader())
            {
                return (TExtension) serializer.ReadObject(reader, false);
            }
        }

        public TExtension GetObject<TExtension>(XmlSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            if ((this.extensionData != null) && typeof(TExtension).IsAssignableFrom(this.extensionData.GetType()))
            {
                return (TExtension) this.extensionData;
            }
            using (XmlReader reader = this.GetReader())
            {
                return (TExtension) serializer.Deserialize(reader);
            }
        }

        public XmlReader GetReader()
        {
            this.EnsureBuffer();
            XmlReader reader = this.buffer.GetReader(0);
            int num = 0;
            reader.ReadStartElement("extensionWrapper");
            while (reader.IsStartElement())
            {
                if (num == this.bufferElementIndex)
                {
                    return reader;
                }
                num++;
                reader.Skip();
            }
            return reader;
        }

        public void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (this.extensionDataWriter != null)
            {
                this.extensionDataWriter.WriteTo(writer);
            }
            else
            {
                using (XmlReader reader = this.GetReader())
                {
                    writer.WriteNode(reader, false);
                }
            }
        }

        public string OuterName
        {
            get
            {
                if (this.outerName == null)
                {
                    this.EnsureOuterNameAndNs();
                }
                return this.outerName;
            }
        }

        public string OuterNamespace
        {
            get
            {
                if (this.outerName == null)
                {
                    this.EnsureOuterNameAndNs();
                }
                return this.outerNamespace;
            }
        }

        private class ExtensionDataWriter
        {
            private readonly XmlObjectSerializer dataContractSerializer;
            private readonly object extensionData;
            private readonly string outerName;
            private readonly string outerNamespace;
            private readonly XmlSerializer xmlSerializer;

            public ExtensionDataWriter(object extensionData, XmlSerializer serializer)
            {
                this.xmlSerializer = serializer;
                this.extensionData = extensionData;
            }

            public ExtensionDataWriter(object extensionData, XmlObjectSerializer dataContractSerializer, string outerName, string outerNamespace)
            {
                this.dataContractSerializer = dataContractSerializer;
                this.extensionData = extensionData;
                this.outerName = outerName;
                this.outerNamespace = outerNamespace;
            }

            internal void ComputeOuterNameAndNs(out string name, out string ns)
            {
                if (this.outerName != null)
                {
                    name = this.outerName;
                    ns = this.outerNamespace;
                }
                else if (this.dataContractSerializer != null)
                {
                    XmlQualifiedName rootElementName = new XsdDataContractExporter().GetRootElementName(this.extensionData.GetType());
                    if (rootElementName != null)
                    {
                        name = rootElementName.Name;
                        ns = rootElementName.Namespace;
                    }
                    else
                    {
                        this.ReadOuterNameAndNs(out name, out ns);
                    }
                }
                else
                {
                    XmlTypeMapping mapping = new XmlReflectionImporter().ImportTypeMapping(this.extensionData.GetType());
                    if ((mapping != null) && !string.IsNullOrEmpty(mapping.ElementName))
                    {
                        name = mapping.ElementName;
                        ns = mapping.Namespace;
                    }
                    else
                    {
                        this.ReadOuterNameAndNs(out name, out ns);
                    }
                }
            }

            internal void ReadOuterNameAndNs(out string name, out string ns)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (XmlWriter writer = XmlWriter.Create(stream))
                    {
                        this.WriteTo(writer);
                    }
                    stream.Seek(0L, SeekOrigin.Begin);
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        SyndicationFeedFormatter.MoveToStartElement(reader);
                        name = reader.LocalName;
                        ns = reader.NamespaceURI;
                    }
                }
            }

            public void WriteTo(XmlWriter writer)
            {
                if (this.xmlSerializer != null)
                {
                    this.xmlSerializer.Serialize(writer, this.extensionData);
                }
                else if (this.outerName != null)
                {
                    writer.WriteStartElement(this.outerName, this.outerNamespace);
                    this.dataContractSerializer.WriteObjectContent(writer, this.extensionData);
                    writer.WriteEndElement();
                }
                else
                {
                    this.dataContractSerializer.WriteObject(writer, this.extensionData);
                }
            }
        }
    }
}

