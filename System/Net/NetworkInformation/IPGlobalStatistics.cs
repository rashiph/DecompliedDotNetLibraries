namespace System.Net.NetworkInformation
{
    using System;

    public abstract class IPGlobalStatistics
    {
        protected IPGlobalStatistics()
        {
        }

        public abstract int DefaultTtl { get; }

        public abstract bool ForwardingEnabled { get; }

        public abstract int NumberOfInterfaces { get; }

        public abstract int NumberOfIPAddresses { get; }

        public abstract int NumberOfRoutes { get; }

        public abstract long OutputPacketRequests { get; }

        public abstract long OutputPacketRoutingDiscards { get; }

        public abstract long OutputPacketsDiscarded { get; }

        public abstract long OutputPacketsWithNoRoute { get; }

        public abstract long PacketFragmentFailures { get; }

        public abstract long PacketReassembliesRequired { get; }

        public abstract long PacketReassemblyFailures { get; }

        public abstract long PacketReassemblyTimeout { get; }

        public abstract long PacketsFragmented { get; }

        public abstract long PacketsReassembled { get; }

        public abstract long ReceivedPackets { get; }

        public abstract long ReceivedPacketsDelivered { get; }

        public abstract long ReceivedPacketsDiscarded { get; }

        public abstract long ReceivedPacketsForwarded { get; }

        public abstract long ReceivedPacketsWithAddressErrors { get; }

        public abstract long ReceivedPacketsWithHeadersErrors { get; }

        public abstract long ReceivedPacketsWithUnknownProtocol { get; }
    }
}

