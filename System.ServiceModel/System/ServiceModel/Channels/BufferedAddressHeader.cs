namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class BufferedAddressHeader : AddressHeader
    {
        private XmlBuffer buffer;
        private bool isReferenceProperty;
        private string name;
        private string ns;

        public BufferedAddressHeader(XmlDictionaryReader reader)
        {
            this.buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = this.buffer.OpenSection(reader.Quotas);
            this.name = reader.LocalName;
            this.ns = reader.NamespaceURI;
            writer.WriteNode(reader, false);
            this.buffer.CloseSection();
            this.buffer.Close();
            this.isReferenceProperty = false;
        }

        public BufferedAddressHeader(XmlDictionaryReader reader, bool isReferenceProperty) : this(reader)
        {
            this.isReferenceProperty = isReferenceProperty;
        }

        public override XmlDictionaryReader GetAddressHeaderReader()
        {
            return this.buffer.GetReader(0);
        }

        protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            XmlDictionaryReader addressHeaderReader = this.GetAddressHeaderReader();
            addressHeaderReader.ReadStartElement();
            while (addressHeaderReader.NodeType != XmlNodeType.EndElement)
            {
                writer.WriteNode(addressHeaderReader, false);
            }
            addressHeaderReader.ReadEndElement();
            addressHeaderReader.Close();
        }

        protected override void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            XmlDictionaryReader addressHeaderReader = this.GetAddressHeaderReader();
            writer.WriteStartElement(addressHeaderReader.Prefix, addressHeaderReader.LocalName, addressHeaderReader.NamespaceURI);
            writer.WriteAttributes(addressHeaderReader, false);
            addressHeaderReader.Close();
        }

        public bool IsReferencePropertyHeader
        {
            get
            {
                return this.isReferenceProperty;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.ns;
            }
        }
    }
}

