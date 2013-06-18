namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Web.Services.Configuration;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot("discovery", Namespace="http://schemas.xmlsoap.org/disco/")]
    public sealed class DiscoveryDocument
    {
        public const string Namespace = "http://schemas.xmlsoap.org/disco/";
        private ArrayList references = new ArrayList();

        public static bool CanRead(XmlReader xmlReader)
        {
            return WebServicesSection.Current.DiscoveryDocumentSerializer.CanDeserialize(xmlReader);
        }

        public static DiscoveryDocument Read(Stream stream)
        {
            XmlTextReader xmlReader = new XmlTextReader(stream) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return Read(xmlReader);
        }

        public static DiscoveryDocument Read(TextReader reader)
        {
            XmlTextReader xmlReader = new XmlTextReader(reader) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return Read(xmlReader);
        }

        public static DiscoveryDocument Read(XmlReader xmlReader)
        {
            return (DiscoveryDocument) WebServicesSection.Current.DiscoveryDocumentSerializer.Deserialize(xmlReader);
        }

        public void Write(Stream stream)
        {
            TextWriter writer = new StreamWriter(stream, new UTF8Encoding(false));
            this.Write(writer);
        }

        public void Write(TextWriter writer)
        {
            XmlTextWriter writer2 = new XmlTextWriter(writer) {
                Formatting = Formatting.Indented,
                Indentation = 2
            };
            this.Write(writer2);
        }

        public void Write(XmlWriter writer)
        {
            XmlSerializer discoveryDocumentSerializer = WebServicesSection.Current.DiscoveryDocumentSerializer;
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            discoveryDocumentSerializer.Serialize(writer, this, namespaces);
        }

        [XmlIgnore]
        public IList References
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.references;
            }
        }
    }
}

