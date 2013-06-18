namespace System.ServiceModel
{
    using System;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class WSAddressing10ProblemHeaderQNameFault : MessageFault
    {
        private string actor;
        private FaultCode code;
        private string invalidHeaderName;
        private string node;
        private FaultReason reason;

        public WSAddressing10ProblemHeaderQNameFault(ActionMismatchAddressingException e)
        {
            this.invalidHeaderName = "Action";
            this.code = FaultCode.CreateSenderFaultCode(new FaultCode("ActionMismatch", AddressingVersion.WSAddressing10.Namespace));
            this.reason = new FaultReason(e.Message, CultureInfo.CurrentCulture);
            this.actor = "";
            this.node = "";
        }

        public WSAddressing10ProblemHeaderQNameFault(MessageHeaderException e)
        {
            this.invalidHeaderName = e.HeaderName;
            if (e.IsDuplicate)
            {
                this.code = FaultCode.CreateSenderFaultCode(new FaultCode("InvalidAddressingHeader", AddressingVersion.WSAddressing10.Namespace, new FaultCode("InvalidCardinality", AddressingVersion.WSAddressing10.Namespace)));
            }
            else
            {
                this.code = FaultCode.CreateSenderFaultCode(new FaultCode("MessageAddressingHeaderRequired", AddressingVersion.WSAddressing10.Namespace));
            }
            this.reason = new FaultReason(e.Message, CultureInfo.CurrentCulture);
            this.actor = "";
            this.node = "";
        }

        public void AddHeaders(MessageHeaders headers)
        {
            if (headers.MessageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                headers.Add(new WSAddressing10ProblemHeaderQNameHeader(this.invalidHeaderName));
            }
        }

        protected override void OnWriteDetail(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            if (version == EnvelopeVersion.Soap12)
            {
                this.OnWriteStartDetail(writer, version);
                this.OnWriteDetailContents(writer);
                writer.WriteEndElement();
            }
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("ProblemHeaderQName", AddressingVersion.WSAddressing10.Namespace);
            writer.WriteQualifiedName(this.invalidHeaderName, AddressingVersion.WSAddressing10.Namespace);
            writer.WriteEndElement();
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public override FaultCode Code
        {
            get
            {
                return this.code;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return true;
            }
        }

        public override string Node
        {
            get
            {
                return this.node;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return this.reason;
            }
        }

        private class WSAddressing10ProblemHeaderQNameHeader : MessageHeader
        {
            private string invalidHeaderName;

            public WSAddressing10ProblemHeaderQNameHeader(string invalidHeaderName)
            {
                this.invalidHeaderName = invalidHeaderName;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteStartElement("ProblemHeaderQName", this.Namespace);
                writer.WriteQualifiedName(this.invalidHeaderName, this.Namespace);
                writer.WriteEndElement();
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteStartElement(this.Name, this.Namespace);
            }

            public override string Name
            {
                get
                {
                    return "FaultDetail";
                }
            }

            public override string Namespace
            {
                get
                {
                    return AddressingVersion.WSAddressing10.Namespace;
                }
            }
        }
    }
}

