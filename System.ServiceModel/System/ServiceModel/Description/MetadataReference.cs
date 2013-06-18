namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="MetadataReference", Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex")]
    public class MetadataReference : IXmlSerializable
    {
        private EndpointAddress address;
        private AddressingVersion addressVersion;
        private Collection<System.Xml.XmlAttribute> attributes;
        private static XmlDocument Document = new XmlDocument();

        public MetadataReference()
        {
            this.attributes = new Collection<System.Xml.XmlAttribute>();
        }

        public MetadataReference(EndpointAddress address, AddressingVersion addressVersion)
        {
            this.attributes = new Collection<System.Xml.XmlAttribute>();
            this.address = address;
            this.addressVersion = addressVersion;
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.address = EndpointAddress.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader), out this.addressVersion);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.address != null)
            {
                this.address.WriteContentsTo(this.addressVersion, writer);
            }
        }

        public EndpointAddress Address
        {
            get
            {
                return this.address;
            }
            set
            {
                this.address = value;
            }
        }

        public AddressingVersion AddressVersion
        {
            get
            {
                return this.addressVersion;
            }
            set
            {
                this.addressVersion = value;
            }
        }
    }
}

