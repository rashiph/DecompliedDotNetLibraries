namespace System.ServiceModel.Description
{
    using System;
    using System.Xml.Serialization;

    internal abstract class XmlSerializer1 : XmlSerializer
    {
        protected XmlSerializer1()
        {
        }

        protected override XmlSerializationReader CreateReader()
        {
            return new XmlSerializationReaderMetadataSet();
        }

        protected override XmlSerializationWriter CreateWriter()
        {
            return new XmlSerializationWriterMetadataSet();
        }
    }
}

