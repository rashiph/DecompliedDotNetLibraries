namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class MessageTransmitTraceRecord : MessageTraceRecord
    {
        private Uri address;
        private string addressElementName;

        private MessageTransmitTraceRecord(Message message) : base(message)
        {
        }

        private MessageTransmitTraceRecord(Message message, string addressElementName) : this(message)
        {
            this.addressElementName = addressElementName;
        }

        private MessageTransmitTraceRecord(Message message, string addressElementName, EndpointAddress address) : this(message, addressElementName)
        {
            if (address != null)
            {
                this.address = address.Uri;
            }
        }

        private MessageTransmitTraceRecord(Message message, string addressElementName, Uri uri) : this(message, addressElementName)
        {
            this.address = uri;
        }

        internal static MessageTransmitTraceRecord CreateReceiveTraceRecord(Message message)
        {
            return new MessageTransmitTraceRecord(message);
        }

        internal static MessageTransmitTraceRecord CreateReceiveTraceRecord(Message message, EndpointAddress address)
        {
            return new MessageTransmitTraceRecord(message, "LocalAddress", address);
        }

        internal static MessageTransmitTraceRecord CreateReceiveTraceRecord(Message message, Uri uri)
        {
            return new MessageTransmitTraceRecord(message, "LocalAddress", uri);
        }

        internal static MessageTransmitTraceRecord CreateSendTraceRecord(Message message, EndpointAddress address)
        {
            return new MessageTransmitTraceRecord(message, "RemoteAddress", address);
        }

        internal override void WriteTo(XmlWriter xml)
        {
            base.WriteTo(xml);
            if (this.address != null)
            {
                xml.WriteElementString(this.addressElementName, this.address.ToString());
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("MessageTransmit");
            }
        }
    }
}

