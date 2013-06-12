namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIpStats
    {
        internal bool forwardingEnabled;
        internal uint defaultTtl;
        internal uint packetsReceived;
        internal uint receivedPacketsWithHeaderErrors;
        internal uint receivedPacketsWithAddressErrors;
        internal uint packetsForwarded;
        internal uint receivedPacketsWithUnknownProtocols;
        internal uint receivedPacketsDiscarded;
        internal uint receivedPacketsDelivered;
        internal uint packetOutputRequests;
        internal uint outputPacketRoutingDiscards;
        internal uint outputPacketsDiscarded;
        internal uint outputPacketsWithNoRoute;
        internal uint packetReassemblyTimeout;
        internal uint packetsReassemblyRequired;
        internal uint packetsReassembled;
        internal uint packetsReassemblyFailed;
        internal uint packetsFragmented;
        internal uint packetsFragmentFailed;
        internal uint packetsFragmentCreated;
        internal uint interfaces;
        internal uint ipAddresses;
        internal uint routes;
    }
}

