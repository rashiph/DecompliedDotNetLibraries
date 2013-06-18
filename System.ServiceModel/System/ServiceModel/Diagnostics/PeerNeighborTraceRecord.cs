namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class PeerNeighborTraceRecord : TraceRecord
    {
        private string action;
        private string attemptedState;
        private IPAddress connectIPAddress;
        private int hashCode;
        private bool initiator;
        private PeerNodeAddress listenAddress;
        private ulong localNodeId;
        private string previousState;
        private ulong remoteNodeId;
        private string state;

        public PeerNeighborTraceRecord(ulong remoteNodeId, ulong localNodeId, PeerNodeAddress listenAddress, IPAddress connectIPAddress, int hashCode, bool initiator, string state, string previousState, string attemptedState, string action)
        {
            this.localNodeId = localNodeId;
            this.remoteNodeId = remoteNodeId;
            this.listenAddress = listenAddress;
            this.connectIPAddress = connectIPAddress;
            this.hashCode = hashCode;
            this.initiator = initiator;
            this.state = state;
            this.previousState = previousState;
            this.attemptedState = attemptedState;
            this.action = action;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("HashCode");
            writer.WriteValue(this.hashCode);
            writer.WriteEndElement();
            if (this.remoteNodeId != 0L)
            {
                writer.WriteElementString("RemoteNodeId", this.remoteNodeId.ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteElementString("LocalNodeId", this.localNodeId.ToString(CultureInfo.InvariantCulture));
            if (this.listenAddress != null)
            {
                this.listenAddress.EndpointAddress.WriteTo(AddressingVersion.WSAddressing10, writer, "ListenAddress", "");
                foreach (IPAddress address in this.listenAddress.IPAddresses)
                {
                    writer.WriteElementString("IPAddress", address.ToString());
                }
            }
            if (this.connectIPAddress != null)
            {
                writer.WriteElementString("ConnectIPAddress", this.connectIPAddress.ToString());
            }
            writer.WriteElementString("State", this.state);
            if (this.previousState != null)
            {
                writer.WriteElementString("PreviousState", this.previousState);
            }
            if (this.attemptedState != null)
            {
                writer.WriteElementString("AttemptedState", this.attemptedState);
            }
            writer.WriteStartElement("Initiator");
            writer.WriteValue(this.initiator);
            writer.WriteEndElement();
            if (this.action != null)
            {
                writer.WriteElementString("Action", this.action);
            }
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PeerNeighborTraceRecord";
            }
        }
    }
}

