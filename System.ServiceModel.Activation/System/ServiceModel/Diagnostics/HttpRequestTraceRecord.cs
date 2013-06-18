namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Web;
    using System.Xml;

    internal class HttpRequestTraceRecord : TraceRecord
    {
        private HttpRequest request;

        internal HttpRequestTraceRecord(HttpRequest request)
        {
            this.request = request;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("Headers");
            foreach (string str in this.request.Headers.Keys)
            {
                writer.WriteElementString(str, this.request.Headers[str]);
            }
            writer.WriteEndElement();
            writer.WriteElementString("Path", this.request.Path);
            if ((this.request.QueryString != null) && (this.request.QueryString.Count > 0))
            {
                writer.WriteStartElement("QueryString");
                foreach (string str2 in this.request.QueryString.Keys)
                {
                    writer.WriteElementString(str2, this.request.Headers[str2]);
                }
                writer.WriteEndElement();
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("HttpRequest");
            }
        }
    }
}

