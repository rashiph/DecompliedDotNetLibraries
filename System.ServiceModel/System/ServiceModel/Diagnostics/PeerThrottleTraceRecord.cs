namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;

    internal class PeerThrottleTraceRecord : TraceRecord
    {
        private string meshId;
        private string message;

        public PeerThrottleTraceRecord(string meshId, string message)
        {
            this.meshId = meshId;
            this.message = message;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", this.meshId.ToString());
            writer.WriteElementString("Activity", this.message);
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

