namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class PeerNodeTraceRecord : TraceRecord
    {
        private PeerNodeAddress address;
        private ulong id;
        private string meshId;

        public PeerNodeTraceRecord(ulong id)
        {
            this.id = id;
        }

        public PeerNodeTraceRecord(ulong id, string meshId)
        {
            this.id = id;
            this.meshId = meshId;
        }

        public PeerNodeTraceRecord(ulong id, string meshId, PeerNodeAddress address)
        {
            this.id = id;
            this.meshId = meshId;
            this.address = address;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("NodeId", this.id.ToString(CultureInfo.InvariantCulture));
            if (this.meshId != null)
            {
                writer.WriteElementString("MeshId", this.meshId);
            }
            if (this.address != null)
            {
                this.address.EndpointAddress.WriteTo(AddressingVersion.WSAddressing10, writer, "LocalAddress", "");
                foreach (IPAddress address in this.address.IPAddresses)
                {
                    writer.WriteElementString("IPAddress", address.ToString());
                }
            }
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PeerNodeTraceRecord";
            }
        }
    }
}

