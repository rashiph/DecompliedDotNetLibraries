namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibUdpStats
    {
        internal uint datagramsReceived;
        internal uint incomingDatagramsDiscarded;
        internal uint incomingDatagramsWithErrors;
        internal uint datagramsSent;
        internal uint udpListeners;
    }
}

