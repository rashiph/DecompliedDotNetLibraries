namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class ReplyToHeader : AddressingHeader
    {
        private static ReplyToHeader anonymousReplyToHeader10;
        private static ReplyToHeader anonymousReplyToHeader200408;
        private const bool mustUnderstandValue = false;
        private EndpointAddress replyTo;

        private ReplyToHeader(EndpointAddress replyTo, AddressingVersion version) : base(version)
        {
            this.replyTo = replyTo;
        }

        public static ReplyToHeader Create(EndpointAddress replyTo, AddressingVersion addressingVersion)
        {
            if (replyTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("replyTo"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            }
            return new ReplyToHeader(replyTo, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.replyTo.WriteContentsTo(base.Version, writer);
        }

        public static ReplyToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress replyTo = ReadHeaderValue(reader, version);
            if (((actor.Length != 0) || mustUnderstand) || relay)
            {
                return new FullReplyToHeader(replyTo, actor, mustUnderstand, relay, version);
            }
            if (replyTo != EndpointAddress.AnonymousAddress)
            {
                return new ReplyToHeader(replyTo, version);
            }
            if (version == AddressingVersion.WSAddressing10)
            {
                return AnonymousReplyTo10;
            }
            return AnonymousReplyTo200408;
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            return EndpointAddress.ReadFrom(version, reader);
        }

        public static ReplyToHeader AnonymousReplyTo10
        {
            get
            {
                if (anonymousReplyToHeader10 == null)
                {
                    anonymousReplyToHeader10 = new ReplyToHeader(EndpointAddress.AnonymousAddress, AddressingVersion.WSAddressing10);
                }
                return anonymousReplyToHeader10;
            }
        }

        public static ReplyToHeader AnonymousReplyTo200408
        {
            get
            {
                if (anonymousReplyToHeader200408 == null)
                {
                    anonymousReplyToHeader200408 = new ReplyToHeader(EndpointAddress.AnonymousAddress, AddressingVersion.WSAddressingAugust2004);
                }
                return anonymousReplyToHeader200408;
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.AddressingDictionary.ReplyTo;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return false;
            }
        }

        public EndpointAddress ReplyTo
        {
            get
            {
                return this.replyTo;
            }
        }

        private class FullReplyToHeader : ReplyToHeader
        {
            private string actor;
            private bool mustUnderstand;
            private bool relay;

            public FullReplyToHeader(EndpointAddress replyTo, string actor, bool mustUnderstand, bool relay, AddressingVersion version) : base(replyTo, version)
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

