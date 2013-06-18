namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class ReceivedMessage : Message
    {
        private bool isEmpty;
        private bool isFault;

        protected ReceivedMessage()
        {
        }

        protected static bool HasHeaderElement(XmlDictionaryReader reader, EnvelopeVersion envelopeVersion)
        {
            return reader.IsStartElement(XD.MessageDictionary.Header, envelopeVersion.DictionaryNamespace);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (!this.isEmpty)
            {
                using (XmlDictionaryReader reader = this.OnGetReaderAtBodyContents())
                {
                    if ((reader.ReadState != System.Xml.ReadState.Error) && (reader.ReadState != System.Xml.ReadState.Closed))
                    {
                        goto Label_005E;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageBodyReaderInvalidReadState", new object[] { reader.ReadState.ToString() })));
                Label_0056:
                    writer.WriteNode(reader, false);
                Label_005E:
                    if ((reader.NodeType != XmlNodeType.EndElement) && !reader.EOF)
                    {
                        goto Label_0056;
                    }
                    base.ReadFromBodyContentsToEnd(reader);
                }
            }
        }

        protected bool ReadStartBody(XmlDictionaryReader reader)
        {
            return Message.ReadStartBody(reader, this.Version.Envelope, out this.isFault, out this.isEmpty);
        }

        protected static EnvelopeVersion ReadStartEnvelope(XmlDictionaryReader reader)
        {
            EnvelopeVersion version;
            if (reader.IsStartElement(XD.MessageDictionary.Envelope, XD.Message12Dictionary.Namespace))
            {
                version = EnvelopeVersion.Soap12;
            }
            else
            {
                if (!reader.IsStartElement(XD.MessageDictionary.Envelope, XD.Message11Dictionary.Namespace))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MessageVersionUnknown")));
                }
                version = EnvelopeVersion.Soap11;
            }
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MessageBodyMissing")));
            }
            reader.Read();
            return version;
        }

        protected static void VerifyStartBody(XmlDictionaryReader reader, EnvelopeVersion version)
        {
            if (!reader.IsStartElement(XD.MessageDictionary.Body, version.DictionaryNamespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MessageBodyMissing")));
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return this.isEmpty;
            }
        }

        public override bool IsFault
        {
            get
            {
                return this.isFault;
            }
        }
    }
}

