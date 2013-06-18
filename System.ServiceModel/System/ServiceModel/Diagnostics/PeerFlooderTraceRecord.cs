namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.Xml;

    internal class PeerFlooderTraceRecord : TraceRecord
    {
        private Exception exception;
        private Uri from;
        private string meshId;

        public PeerFlooderTraceRecord(string meshId, PeerNodeAddress fromAddress, Exception e)
        {
            this.from = (fromAddress != null) ? fromAddress.EndpointAddress.Uri : new Uri("net.p2p://");
            this.meshId = meshId;
            this.exception = e;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", this.meshId.ToString());
            writer.WriteElementString("MessageSource", this.from.ToString());
            writer.WriteElementString("Exception", this.exception.Message);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PeerFlooderQuotaExceededTraceRecord";
            }
        }
    }
}

