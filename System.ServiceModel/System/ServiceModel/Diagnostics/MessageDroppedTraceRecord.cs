namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal sealed class MessageDroppedTraceRecord : MessageTraceRecord
    {
        private EndpointAddress endpointAddress;

        internal MessageDroppedTraceRecord(Message message, EndpointAddress endpointAddress) : base(message)
        {
            this.endpointAddress = endpointAddress;
        }

        internal override void WriteTo(XmlWriter xml)
        {
            base.WriteTo(xml);
            if (this.endpointAddress != null)
            {
                xml.WriteStartElement("EndpointAddress");
                this.endpointAddress.WriteTo(AddressingVersion.WSAddressing10, xml);
                xml.WriteEndElement();
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("MessageDropped");
            }
        }
    }
}

