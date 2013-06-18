namespace System.Transactions.Diagnostics
{
    using System;
    using System.Xml;

    internal abstract class TraceRecord
    {
        protected internal const string EventIdBase = "http://schemas.microsoft.com/2004/03/Transactions/";
        protected internal const string NamespaceSuffix = "TraceRecord";

        protected TraceRecord()
        {
        }

        public override string ToString()
        {
            PlainXmlWriter xml = new PlainXmlWriter();
            this.WriteTo(xml);
            return xml.ToString();
        }

        internal abstract void WriteTo(XmlWriter xml);

        internal virtual string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/EmptyTraceRecord";
            }
        }
    }
}

