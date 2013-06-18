namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal abstract class DelegatingHeader : MessageHeader
    {
        private MessageHeader innerHeader;

        protected DelegatingHeader(MessageHeader innerHeader)
        {
            if (innerHeader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerHeader");
            }
            this.innerHeader = innerHeader;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.innerHeader.WriteHeaderContents(writer, messageVersion);
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.innerHeader.WriteStartHeader(writer, messageVersion);
        }

        public override string Actor
        {
            get
            {
                return this.innerHeader.Actor;
            }
        }

        protected MessageHeader InnerHeader
        {
            get
            {
                return this.innerHeader;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.innerHeader.MustUnderstand;
            }
        }

        public override string Name
        {
            get
            {
                return this.innerHeader.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.innerHeader.Namespace;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.innerHeader.Relay;
            }
        }
    }
}

