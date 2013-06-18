namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class FromHeader : AddressingHeader
    {
        private EndpointAddress from;
        private const bool mustUnderstandValue = false;

        private FromHeader(EndpointAddress from, AddressingVersion version) : base(version)
        {
            this.from = from;
        }

        public static FromHeader Create(EndpointAddress from, AddressingVersion addressingVersion)
        {
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("from"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            return new FromHeader(from, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.from.WriteContentsTo(base.Version, writer);
        }

        public static FromHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress from = ReadHeaderValue(reader, version);
            if (((actor.Length == 0) && !mustUnderstand) && !relay)
            {
                return new FromHeader(from, version);
            }
            return new FullFromHeader(from, actor, mustUnderstand, relay, version);
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion addressingVersion)
        {
            return EndpointAddress.ReadFrom(addressingVersion, reader);
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.AddressingDictionary.From;
            }
        }

        public EndpointAddress From
        {
            get
            {
                return this.from;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return false;
            }
        }

        private class FullFromHeader : FromHeader
        {
            private string actor;
            private bool mustUnderstand;
            private bool relay;

            public FullFromHeader(EndpointAddress from, string actor, bool mustUnderstand, bool relay, AddressingVersion version) : base(from, version)
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

