namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class AcknowledgementTraceRecord : WsrmTraceRecord
    {
        private int bufferRemaining;
        private IList<SequenceRange> ranges;

        internal AcknowledgementTraceRecord(UniqueId id, IList<SequenceRange> ranges, int bufferRemaining) : base(id)
        {
            this.bufferRemaining = bufferRemaining;
            this.ranges = ranges;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("Ranges");
            for (int i = 0; i < this.ranges.Count; i++)
            {
                writer.WriteStartElement("Range");
                SequenceRange range = this.ranges[i];
                writer.WriteAttributeString("Lower", range.Lower.ToString(CultureInfo.InvariantCulture));
                SequenceRange range2 = this.ranges[i];
                writer.WriteAttributeString("Upper", range2.Upper.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            if (this.bufferRemaining != -1)
            {
                writer.WriteStartElement("BufferRemaining");
                writer.WriteString(this.bufferRemaining.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
        }
    }
}

