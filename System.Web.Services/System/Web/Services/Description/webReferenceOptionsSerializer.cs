namespace System.Web.Services.Description
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class webReferenceOptionsSerializer : XmlSerializer
    {
        public override bool CanDeserialize(XmlReader xmlReader)
        {
            return true;
        }

        protected override XmlSerializationReader CreateReader()
        {
            return new WebReferenceOptionsSerializationReader();
        }

        protected override XmlSerializationWriter CreateWriter()
        {
            return new WebReferenceOptionsSerializationWriter();
        }

        protected override object Deserialize(XmlSerializationReader reader)
        {
            return ((WebReferenceOptionsSerializationReader) reader).Read5_webReferenceOptions();
        }

        protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
        {
            ((WebReferenceOptionsSerializationWriter) writer).Write5_webReferenceOptions(objectToSerialize);
        }
    }
}

