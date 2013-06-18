namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class WsdlWrapper : IXmlSerializable
    {
        private ServiceDescription wsdl;

        public WsdlWrapper(ServiceDescription wsdl)
        {
            this.wsdl = wsdl;
        }

        public XmlSchema GetSchema()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void ReadXml(XmlReader xmlReader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public void WriteXml(XmlWriter xmlWriter)
        {
            if (this.wsdl != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    this.wsdl.Write(stream);
                    XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas {
                        MaxDepth = 0x20,
                        MaxStringContentLength = 0x2000,
                        MaxArrayLength = 0x4000,
                        MaxBytesPerRead = 0x1000,
                        MaxNameTableCharCount = 0x4000
                    };
                    stream.Seek(0L, SeekOrigin.Begin);
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, null, quotas, null);
                    if ((reader.MoveToContent() == XmlNodeType.Element) && (reader.Name == "wsdl:definitions"))
                    {
                        xmlWriter.WriteNode(reader, false);
                    }
                    reader.Close();
                }
            }
        }
    }
}

