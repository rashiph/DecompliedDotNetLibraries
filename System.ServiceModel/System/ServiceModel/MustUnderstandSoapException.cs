namespace System.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.Xml;

    [Serializable]
    internal class MustUnderstandSoapException : CommunicationException
    {
        private System.ServiceModel.EnvelopeVersion envelopeVersion;
        private Collection<MessageHeaderInfo> notUnderstoodHeaders;

        public MustUnderstandSoapException()
        {
        }

        public MustUnderstandSoapException(Collection<MessageHeaderInfo> notUnderstoodHeaders, System.ServiceModel.EnvelopeVersion envelopeVersion)
        {
            this.notUnderstoodHeaders = notUnderstoodHeaders;
            this.envelopeVersion = envelopeVersion;
        }

        protected MustUnderstandSoapException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private void AddNotUnderstoodHeaders(MessageHeaders headers)
        {
            for (int i = 0; i < this.notUnderstoodHeaders.Count; i++)
            {
                headers.Add(new NotUnderstoodHeader(this.notUnderstoodHeaders[i].Name, this.notUnderstoodHeaders[i].Namespace));
            }
        }

        internal Message ProvideFault(MessageVersion messageVersion)
        {
            string name = this.notUnderstoodHeaders[0].Name;
            string str2 = this.notUnderstoodHeaders[0].Namespace;
            FaultCode code = new FaultCode("MustUnderstand", this.envelopeVersion.Namespace);
            FaultReason reason = new FaultReason(System.ServiceModel.SR.GetString("SFxHeaderNotUnderstood", new object[] { name, str2 }), CultureInfo.CurrentCulture);
            MessageFault fault = MessageFault.CreateFault(code, reason);
            string defaultFaultAction = messageVersion.Addressing.DefaultFaultAction;
            Message message = Message.CreateMessage(messageVersion, fault, defaultFaultAction);
            if (this.envelopeVersion == System.ServiceModel.EnvelopeVersion.Soap12)
            {
                this.AddNotUnderstoodHeaders(message.Headers);
            }
            return message;
        }

        public System.ServiceModel.EnvelopeVersion EnvelopeVersion
        {
            get
            {
                return this.envelopeVersion;
            }
        }

        public Collection<MessageHeaderInfo> NotUnderstoodHeaders
        {
            get
            {
                return this.notUnderstoodHeaders;
            }
        }

        private class NotUnderstoodHeader : MessageHeader
        {
            private string notUnderstoodName;
            private string notUnderstoodNs;

            public NotUnderstoodHeader(string name, string ns)
            {
                this.notUnderstoodName = name;
                this.notUnderstoodNs = ns;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
            }

            protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteStartElement(this.Name, this.Namespace);
                writer.WriteXmlnsAttribute(null, this.notUnderstoodNs);
                writer.WriteStartAttribute("qname");
                writer.WriteQualifiedName(this.notUnderstoodName, this.notUnderstoodNs);
                writer.WriteEndAttribute();
            }

            public override string Name
            {
                get
                {
                    return "NotUnderstood";
                }
            }

            public override string Namespace
            {
                get
                {
                    return "http://www.w3.org/2003/05/soap-envelope";
                }
            }
        }
    }
}

