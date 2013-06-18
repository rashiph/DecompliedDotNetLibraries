namespace Microsoft.InfoCards.Diagnostics
{
    using Microsoft.InfoCards;
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;

    internal class InfoCardTraceRecord : TraceRecord
    {
        private const string InfoCardEventIdBase = "http://schemas.microsoft.com/2004/11/InfoCard/";
        private string m_eventID;
        private string m_message;

        public InfoCardTraceRecord(string eventID, string message)
        {
            InfoCardTrace.Assert(!string.IsNullOrEmpty(eventID), "null eventid", new object[0]);
            InfoCardTrace.Assert(!string.IsNullOrEmpty(message), "null message", new object[0]);
            this.m_eventID = eventID;
            this.m_message = message;
        }

        public override string ToString()
        {
            return Microsoft.InfoCards.SR.GetString("EventLogMessage", new object[] { this.m_eventID, this.m_message });
        }

        internal override void WriteTo(XmlWriter writer)
        {
            InfoCardTrace.Assert(null != writer, "null writer", new object[0]);
            writer.WriteElementString("message", this.m_message);
        }

        internal override string EventId
        {
            get
            {
                return ("http://schemas.microsoft.com/2004/11/InfoCard/" + this.m_eventID + "TraceRecord");
            }
        }
    }
}

