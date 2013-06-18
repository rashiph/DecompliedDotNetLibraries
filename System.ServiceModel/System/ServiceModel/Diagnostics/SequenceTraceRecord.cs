namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Xml;

    internal class SequenceTraceRecord : WsrmTraceRecord
    {
        private bool isLast;
        private long sequenceNumber;

        internal SequenceTraceRecord(UniqueId id, long sequenceNumber, bool isLast) : base(id)
        {
            this.sequenceNumber = sequenceNumber;
            this.isLast = isLast;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("MessageNumber");
            writer.WriteString(this.sequenceNumber.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.WriteStartElement("LastMessage");
            writer.WriteString(this.isLast.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }
    }
}

