namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;

    internal class PnrpResolveExceptionTraceRecord : TraceRecord
    {
        private string cloudName;
        private Exception exception;
        private string peerName;

        public PnrpResolveExceptionTraceRecord(string peerName, string cloudName, Exception exception)
        {
            this.peerName = peerName;
            this.cloudName = cloudName;
            this.exception = exception;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("PeerName", this.peerName);
            writer.WriteElementString("CloudName", this.cloudName);
            writer.WriteElementString("Exception", this.exception.ToString());
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PnrpResolveExceptionTraceRecord";
            }
        }
    }
}

