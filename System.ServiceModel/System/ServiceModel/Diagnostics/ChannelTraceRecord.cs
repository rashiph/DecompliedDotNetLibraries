namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class ChannelTraceRecord : TraceRecord
    {
        private string channelType;

        internal ChannelTraceRecord(IChannel channel)
        {
            this.channelType = (channel == null) ? null : channel.ToString();
        }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.channelType != null)
            {
                xml.WriteElementString("ChannelType", this.channelType);
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("Channel");
            }
        }
    }
}

