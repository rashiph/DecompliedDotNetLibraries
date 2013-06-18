namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Xml;

    internal class SequenceFaultedTraceRecord : WsrmTraceRecord
    {
        private string reason;

        internal SequenceFaultedTraceRecord(UniqueId id, string reason) : base(id)
        {
            this.reason = reason;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("Reason");
            writer.WriteString(this.reason);
            writer.WriteEndElement();
        }
    }
}

