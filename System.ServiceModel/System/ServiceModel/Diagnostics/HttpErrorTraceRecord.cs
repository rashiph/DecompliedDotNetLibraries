namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;

    internal class HttpErrorTraceRecord : TraceRecord
    {
        private string html;

        internal HttpErrorTraceRecord(string html)
        {
            this.html = base.XmlEncode(html);
        }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteElementString("HtmlErrorMessage", this.html);
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("HttpError");
            }
        }
    }
}

