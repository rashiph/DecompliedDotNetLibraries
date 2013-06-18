namespace System.Web.Services.Discovery
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    internal class DiscoveryDocumentSerializer : XmlSerializer
    {
        public override bool CanDeserialize(XmlReader xmlReader)
        {
            return xmlReader.IsStartElement("discovery", "http://schemas.xmlsoap.org/disco/");
        }

        protected override XmlSerializationReader CreateReader()
        {
            return new DiscoveryDocumentSerializationReader();
        }

        protected override XmlSerializationWriter CreateWriter()
        {
            return new DiscoveryDocumentSerializationWriter();
        }

        protected override object Deserialize(XmlSerializationReader reader)
        {
            return ((DiscoveryDocumentSerializationReader) reader).Read10_discovery();
        }

        protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
        {
            ((DiscoveryDocumentSerializationWriter) writer).Write10_discovery(objectToSerialize);
        }
    }
}

