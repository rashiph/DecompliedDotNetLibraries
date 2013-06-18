namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal class MessageLoggingFilterTraceRecord : TraceRecord
    {
        private XPathMessageFilter filter;

        internal MessageLoggingFilterTraceRecord(XPathMessageFilter filter)
        {
            this.filter = filter;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            this.filter.WriteXPathTo(writer, "", "Filter", "", false);
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("MessageLoggingFilter");
            }
        }
    }
}

