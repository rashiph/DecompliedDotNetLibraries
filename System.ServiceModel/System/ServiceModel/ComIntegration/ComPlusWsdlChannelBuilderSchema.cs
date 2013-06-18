namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [DataContract(Name="ComPlusWsdlChannelBuilder")]
    internal class ComPlusWsdlChannelBuilderSchema : TraceRecord
    {
        [DataMember(Name="BindingQName")]
        private XmlQualifiedName bindingQname;
        [DataMember(Name="ContractQName")]
        private XmlQualifiedName contractQname;
        [DataMember(Name="ImportedBinding")]
        private string importedBinding;
        [DataMember(Name="ImportedContract")]
        private string importedContract;
        [DataMember(Name="XmlSchemaSet")]
        private XmlSchemaWrapper schema;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusWsdlChannelBuilderTraceRecord";
        [DataMember(Name="ServiceQName")]
        private XmlQualifiedName serviceQname;

        public ComPlusWsdlChannelBuilderSchema(XmlQualifiedName bindingQname, XmlQualifiedName contractQname, XmlQualifiedName serviceQname, string importedContract, string importedBinding, XmlSchema schema)
        {
            this.bindingQname = bindingQname;
            this.contractQname = contractQname;
            this.serviceQname = serviceQname;
            this.importedContract = importedContract;
            this.importedBinding = importedBinding;
            this.schema = new XmlSchemaWrapper(schema);
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusWsdlChannelBuilderTraceRecord";
            }
        }

        private class XmlSchemaWrapper : IXmlSerializable
        {
            private XmlSchema schema;

            public XmlSchemaWrapper(XmlSchema schema)
            {
                this.schema = schema;
            }

            public XmlSchema GetSchema()
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            public void ReadXml(XmlReader xmlReader)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            public void WriteXml(XmlWriter xmlWriter)
            {
                StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
                XmlTextWriter writer = new XmlTextWriter(w);
                this.schema.Write(writer);
                writer.Flush();
                byte[] bytes = new UTF8Encoding().GetBytes(w.ToString());
                XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas {
                    MaxDepth = 0x20,
                    MaxStringContentLength = 0x2000,
                    MaxArrayLength = 0x4000,
                    MaxBytesPerRead = 0x1000,
                    MaxNameTableCharCount = 0x4000
                };
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(bytes, 0, bytes.GetLength(0), null, quotas, null);
                if ((reader.MoveToContent() == XmlNodeType.Element) && (reader.Name == "xs:schema"))
                {
                    xmlWriter.WriteNode(reader, false);
                }
                reader.Close();
            }
        }
    }
}

