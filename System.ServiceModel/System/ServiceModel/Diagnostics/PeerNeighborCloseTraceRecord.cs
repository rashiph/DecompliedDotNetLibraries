namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.Xml;

    internal class PeerNeighborCloseTraceRecord : PeerNeighborTraceRecord
    {
        private string closeInitiator;
        private string closeReason;

        public PeerNeighborCloseTraceRecord(ulong remoteNodeId, ulong localNodeId, PeerNodeAddress listenAddress, IPAddress connectIPAddress, int hashCode, bool initiator, string state, string previousState, string attemptedState, string closeInitiator, string closeReason) : base(remoteNodeId, localNodeId, listenAddress, connectIPAddress, hashCode, initiator, state, previousState, attemptedState, null)
        {
            this.closeInitiator = closeInitiator;
            this.closeReason = closeReason;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("CloseReason", this.closeReason);
            writer.WriteElementString("CloseInitiator", this.closeInitiator);
        }
    }
}

