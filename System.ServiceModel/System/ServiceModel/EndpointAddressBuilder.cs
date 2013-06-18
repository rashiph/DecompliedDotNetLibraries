namespace System.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    public class EndpointAddressBuilder
    {
        private EndpointAddress epr;
        private XmlBuffer extensionBuffer;
        private bool hasExtension;
        private bool hasMetadata;
        private Collection<AddressHeader> headers;
        private EndpointIdentity identity;
        private XmlBuffer metadataBuffer;
        private System.Uri uri;

        public EndpointAddressBuilder()
        {
            this.headers = new Collection<AddressHeader>();
        }

        public EndpointAddressBuilder(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            this.epr = address;
            this.uri = address.Uri;
            this.identity = address.Identity;
            this.headers = new Collection<AddressHeader>();
            for (int i = 0; i < address.Headers.Count; i++)
            {
                this.headers.Add(address.Headers[i]);
            }
        }

        public XmlDictionaryReader GetReaderAtExtensions()
        {
            if (!this.hasExtension)
            {
                if (this.epr != null)
                {
                    return this.epr.GetReaderAtExtensions();
                }
                return null;
            }
            if (this.extensionBuffer == null)
            {
                return null;
            }
            XmlDictionaryReader reader = this.extensionBuffer.GetReader(0);
            reader.MoveToContent();
            reader.Read();
            return reader;
        }

        public XmlDictionaryReader GetReaderAtMetadata()
        {
            if (!this.hasMetadata)
            {
                if (this.epr != null)
                {
                    return this.epr.GetReaderAtMetadata();
                }
                return null;
            }
            if (this.metadataBuffer == null)
            {
                return null;
            }
            XmlDictionaryReader reader = this.metadataBuffer.GetReader(0);
            reader.MoveToContent();
            reader.Read();
            return reader;
        }

        public void SetExtensionReader(XmlDictionaryReader reader)
        {
            EndpointIdentity identity;
            int num;
            this.hasExtension = true;
            this.extensionBuffer = EndpointAddress.ReadExtensions(reader, null, null, out identity, out num);
            if (this.extensionBuffer != null)
            {
                this.extensionBuffer.Close();
            }
            if (identity != null)
            {
                this.identity = identity;
            }
        }

        public void SetMetadataReader(XmlDictionaryReader reader)
        {
            this.hasMetadata = true;
            this.metadataBuffer = null;
            if (reader != null)
            {
                this.metadataBuffer = new XmlBuffer(0x7fff);
                XmlDictionaryWriter writer = this.metadataBuffer.OpenSection(reader.Quotas);
                writer.WriteStartElement("Dummy", "http://Dummy");
                EndpointAddress.Copy(writer, reader);
                this.metadataBuffer.CloseSection();
                this.metadataBuffer.Close();
            }
        }

        public EndpointAddress ToEndpointAddress()
        {
            return new EndpointAddress(this.uri, this.identity, new AddressHeaderCollection(this.headers), this.GetReaderAtMetadata(), this.GetReaderAtExtensions(), (this.epr == null) ? null : this.epr.GetReaderAtPsp());
        }

        public Collection<AddressHeader> Headers
        {
            get
            {
                return this.headers;
            }
        }

        public EndpointIdentity Identity
        {
            get
            {
                return this.identity;
            }
            set
            {
                this.identity = value;
            }
        }

        public System.Uri Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }
    }
}

