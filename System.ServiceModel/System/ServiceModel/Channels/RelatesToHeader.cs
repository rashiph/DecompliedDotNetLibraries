namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    internal class RelatesToHeader : AddressingHeader
    {
        private System.Xml.UniqueId messageId;
        private const bool mustUnderstandValue = false;
        internal static readonly Uri ReplyRelationshipType = new Uri("http://www.w3.org/2005/08/addressing/reply");

        private RelatesToHeader(System.Xml.UniqueId messageId, AddressingVersion version) : base(version)
        {
            this.messageId = messageId;
        }

        public static RelatesToHeader Create(System.Xml.UniqueId messageId, AddressingVersion addressingVersion)
        {
            if (object.ReferenceEquals(messageId, null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageId"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            }
            return new RelatesToHeader(messageId, addressingVersion);
        }

        public static RelatesToHeader Create(System.Xml.UniqueId messageId, AddressingVersion addressingVersion, Uri relationshipType)
        {
            if (object.ReferenceEquals(messageId, null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageId"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            }
            if (relationshipType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("relationshipType"));
            }
            if (relationshipType == ReplyRelationshipType)
            {
                return new RelatesToHeader(messageId, addressingVersion);
            }
            return new FullRelatesToHeader(messageId, "", false, false, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteValue(this.messageId);
        }

        public static RelatesToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, string actor, bool mustUnderstand, bool relay)
        {
            System.Xml.UniqueId id;
            Uri uri;
            ReadHeaderValue(reader, version, out uri, out id);
            if (((actor.Length == 0) && !mustUnderstand) && (!relay && (uri == ReplyRelationshipType)))
            {
                return new RelatesToHeader(id, version);
            }
            return new FullRelatesToHeader(id, actor, mustUnderstand, relay, version);
        }

        public static void ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version, out Uri relationshipType, out System.Xml.UniqueId messageId)
        {
            AddressingDictionary addressingDictionary = XD.AddressingDictionary;
            relationshipType = ReplyRelationshipType;
            messageId = reader.ReadElementContentAsUniqueId();
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.AddressingDictionary.RelatesTo;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return false;
            }
        }

        public virtual Uri RelationshipType
        {
            get
            {
                return ReplyRelationshipType;
            }
        }

        public System.Xml.UniqueId UniqueId
        {
            get
            {
                return this.messageId;
            }
        }

        private class FullRelatesToHeader : RelatesToHeader
        {
            private string actor;
            private bool mustUnderstand;
            private bool relay;

            public FullRelatesToHeader(UniqueId messageId, string actor, bool mustUnderstand, bool relay, AddressingVersion version) : base(messageId, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteValue(base.messageId);
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

