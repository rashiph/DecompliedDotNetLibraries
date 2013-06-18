namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class MessageIDHeader : AddressingHeader
    {
        private UniqueId messageId;
        private const bool mustUnderstandValue = false;

        private MessageIDHeader(UniqueId messageId, AddressingVersion version) : base(version)
        {
            this.messageId = messageId;
        }

        public static MessageIDHeader Create(UniqueId messageId, AddressingVersion addressingVersion)
        {
            if (object.ReferenceEquals(messageId, null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageId"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            }
            return new MessageIDHeader(messageId, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteValue(this.messageId);
        }

        public static MessageIDHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, string actor, bool mustUnderstand, bool relay)
        {
            UniqueId messageId = ReadHeaderValue(reader, version);
            if (((actor.Length == 0) && !mustUnderstand) && !relay)
            {
                return new MessageIDHeader(messageId, version);
            }
            return new FullMessageIDHeader(messageId, actor, mustUnderstand, relay, version);
        }

        public static UniqueId ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            return reader.ReadElementContentAsUniqueId();
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.AddressingDictionary.MessageId;
            }
        }

        public UniqueId MessageId
        {
            get
            {
                return this.messageId;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return false;
            }
        }

        private class FullMessageIDHeader : MessageIDHeader
        {
            private string actor;
            private bool mustUnderstand;
            private bool relay;

            public FullMessageIDHeader(UniqueId messageId, string actor, bool mustUnderstand, bool relay, AddressingVersion version) : base(messageId, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get
                {
                    return this.actor;
                }
            }

            public override bool MustUnderstand
            {
                get
                {
                    return this.mustUnderstand;
                }
            }

            public override bool Relay
            {
                get
                {
                    return this.relay;
                }
            }
        }
    }
}

