namespace System.ServiceModel.Description
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class MetadataSetSerializer : XmlSerializer1
    {
        private bool processOuterElement = true;

        public override bool CanDeserialize(XmlReader xmlReader)
        {
            return xmlReader.IsStartElement("Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex");
        }

        protected override object Deserialize(XmlSerializationReader reader)
        {
            ((XmlSerializationReaderMetadataSet) reader).ProcessOuterElement = this.processOuterElement;
            return ((XmlSerializationReaderMetadataSet) reader).Read68_Metadata();
        }

        protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
        {
            ((XmlSerializationWriterMetadataSet) writer).ProcessOuterElement = this.processOuterElement;
            ((XmlSerializationWriterMetadataSet) writer).Write68_Metadata(objectToSerialize);
        }

        public bool ProcessOuterElement
        {
            get
            {
                return this.processOuterElement;
            }
            set
            {
                this.processOuterElement = value;
            }
        }
    }
}

