namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class PnrpPeerResolverTraceRecord : TraceRecord
    {
        private List<PeerNodeAddress> addresses;
        private string meshId;

        public PnrpPeerResolverTraceRecord(string meshId, List<PeerNodeAddress> addresses)
        {
            this.meshId = meshId;
            this.addresses = addresses;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", this.meshId);
            if (this.addresses != null)
            {
                foreach (PeerNodeAddress address in this.addresses)
                {
                    address.EndpointAddress.WriteTo(AddressingVersion.WSAddressing10, writer, "Address", "");
                    foreach (IPAddress address2 in address.IPAddresses)
                    {
                        writer.WriteElementString("IPAddress", address2.ToString());
                    }
                }
            }
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PnrpPeerResolverTraceRecord";
            }
        }
    }
}

