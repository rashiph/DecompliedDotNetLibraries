namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class ReliableChannelTraceRecord : ChannelTraceRecord
    {
        private UniqueId id;

        internal ReliableChannelTraceRecord(IChannel channel, UniqueId id) : base(channel)
        {
            this.id = id;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("Identifier");
            writer.WriteString(this.id.ToString());
            writer.WriteEndElement();
        }
    }
}

