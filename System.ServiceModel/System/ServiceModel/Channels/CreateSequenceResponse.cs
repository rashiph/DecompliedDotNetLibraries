namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class CreateSequenceResponse : BodyWriter
    {
        private EndpointAddress acceptAcksTo;
        private AddressingVersion addressingVersion;
        private TimeSpan? expires;
        private UniqueId identifier;
        private bool ordered;
        private ReliableMessagingVersion reliableMessagingVersion;

        private CreateSequenceResponse() : base(true)
        {
        }

        public CreateSequenceResponse(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion) : base(true)
        {
            this.addressingVersion = addressingVersion;
            this.reliableMessagingVersion = reliableMessagingVersion;
        }

        public static CreateSequenceResponseInfo Create(AddressingVersion addressingVersion, ReliableMessagingVersion reliableMessagingVersion, XmlDictionaryReader reader)
        {
            CreateSequenceResponseInfo info = new CreateSequenceResponseInfo();
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
            reader.ReadStartElement(dictionary.CreateSequenceResponse, namespaceUri);
            reader.ReadStartElement(dictionary.Identifier, namespaceUri);
            info.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            if (reader.IsStartElement(dictionary.Expires, namespaceUri))
            {
                reader.ReadElementContentAsTimeSpan();
            }
            if ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && reader.IsStartElement(DXD.Wsrm11Dictionary.IncompleteSequenceBehavior, namespaceUri))
            {
                string str2 = reader.ReadElementContentAsString();
                if (((str2 != "DiscardEntireSequence") && (str2 != "DiscardFollowingFirstGap")) && (str2 != "NoDiscard"))
                {
                    string message = System.ServiceModel.SR.GetString("CSResponseWithInvalidIncompleteSequenceBehavior");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(message));
                }
            }
            if (reader.IsStartElement(dictionary.Accept, namespaceUri))
            {
                reader.ReadStartElement();
                info.AcceptAcksTo = EndpointAddress.ReadFrom(addressingVersion, reader, dictionary.AcksTo, namespaceUri);
                while (reader.IsStartElement())
                {
                    reader.Skip();
                }
                reader.ReadEndElement();
            }
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();
            return info;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(this.reliableMessagingVersion);
            writer.WriteStartElement(dictionary.CreateSequenceResponse, namespaceUri);
            writer.WriteStartElement(dictionary.Identifier, namespaceUri);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();
            if (this.expires.HasValue)
            {
                writer.WriteStartElement(dictionary.Expires, namespaceUri);
                writer.WriteValue(this.expires.Value);
                writer.WriteEndElement();
            }
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                Wsrm11Dictionary dictionary2 = DXD.Wsrm11Dictionary;
                writer.WriteStartElement(dictionary2.IncompleteSequenceBehavior, namespaceUri);
                writer.WriteValue(this.ordered ? dictionary2.DiscardFollowingFirstGap : dictionary2.NoDiscard);
                writer.WriteEndElement();
            }
            if (this.acceptAcksTo != null)
            {
                writer.WriteStartElement(dictionary.Accept, namespaceUri);
                this.acceptAcksTo.WriteTo(this.addressingVersion, writer, dictionary.AcksTo, namespaceUri);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public EndpointAddress AcceptAcksTo
        {
            get
            {
                return this.acceptAcksTo;
            }
            set
            {
                this.acceptAcksTo = value;
            }
        }

        public TimeSpan? Expires
        {
            get
            {
                return this.expires;
            }
            set
            {
                this.expires = value;
            }
        }

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public bool Ordered
        {
            get
            {
                return this.ordered;
            }
            set
            {
                this.ordered = value;
            }
        }
    }
}

