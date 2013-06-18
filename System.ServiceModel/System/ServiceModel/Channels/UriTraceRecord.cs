namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;

    internal class UriTraceRecord : TraceRecord
    {
        private Uri uri;

        public UriTraceRecord(Uri uri)
        {
            this.uri = uri;
        }

        internal override void WriteTo(XmlWriter xml)
        {
            xml.WriteElementString("Uri", this.uri.AbsoluteUri);
        }
    }
}

