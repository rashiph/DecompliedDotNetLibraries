namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class FaultToHeader : AddressingHeader
    {
        private EndpointAddress faultTo;
        private const bool mustUnderstandValue = false;

        private FaultToHeader(EndpointAddress faultTo, AddressingVersion version) : base(version)
        {
            this.faultTo = faultTo;
        }

        public static FaultToHeader Create(EndpointAddress faultTo, AddressingVersion addressingVersion)
        {
            if (faultTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("faultTo"));
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            return new FaultToHeader(faultTo, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.faultTo.WriteContentsTo(base.Version, writer);
        }

        public static FaultToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress faultTo = ReadHeaderValue(reader, version);
            if (((actor.Length == 0) && !mustUnderstand) && !relay)
            {
                return new FaultToHeader(faultTo, version);
            }
            return new FullFaultToHeader(faultTo, actor, mustUnderstand, relay, version);
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            return EndpointAddress.ReadFrom(version, reader);
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.AddressingDictionary.FaultTo;
            }
        }

        public EndpointAddress FaultTo
        {
            get
            {
                return this.faultTo;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return false;
            }
        }

        private class FullFaultToHeader : FaultToHeader
        {
            private string actor;
            private bool mustUnderstand;
            private bool relay;

            public FullFaultToHeader(EndpointAddress faultTo, string actor, bool mustUnderstand, bool relay, AddressingVersion version) : base(faultTo, version)
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

