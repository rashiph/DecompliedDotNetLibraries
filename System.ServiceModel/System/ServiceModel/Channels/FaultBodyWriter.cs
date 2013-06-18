namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class FaultBodyWriter : BodyWriter
    {
        private MessageFault fault;
        private EnvelopeVersion version;

        public FaultBodyWriter(MessageFault fault, EnvelopeVersion version) : base(true)
        {
            this.fault = fault;
            this.version = version;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.fault.WriteTo(writer, this.version);
        }

        internal override bool IsFault
        {
            get
            {
                return true;
            }
        }
    }
}

