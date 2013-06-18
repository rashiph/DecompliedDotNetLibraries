namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class XmlReaderBodyWriter : BodyWriter
    {
        private bool isFault;
        private XmlDictionaryReader reader;

        public XmlReaderBodyWriter(XmlDictionaryReader reader, EnvelopeVersion version) : base(false)
        {
            this.reader = reader;
            if (reader.MoveToContent() != XmlNodeType.Element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidReaderPositionOnCreateMessage"), "reader"));
            }
            this.isFault = Message.IsFaultStartElement(reader, version);
        }

        protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
        {
            return base.OnCreateBufferedCopy(maxBufferSize, this.reader.Quotas);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            using (this.reader)
            {
                for (XmlNodeType type = this.reader.MoveToContent(); !this.reader.EOF && (type != XmlNodeType.EndElement); type = this.reader.MoveToContent())
                {
                    if (type != XmlNodeType.Element)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidReaderPositionOnCreateMessage"), "reader"));
                    }
                    writer.WriteNode(this.reader, false);
                }
            }
        }

        internal override bool IsFault
        {
            get
            {
                return this.isFault;
            }
        }
    }
}

