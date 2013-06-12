namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpStats
    {
        internal uint reTransmissionAlgorithm;
        internal uint minimumRetransmissionTimeOut;
        internal uint maximumRetransmissionTimeOut;
        internal uint maximumConnections;
        internal uint activeOpens;
        internal uint passiveOpens;
        internal uint failedConnectionAttempts;
        internal uint resetConnections;
        internal uint currentConnections;
        internal uint segmentsReceived;
        internal uint segmentsSent;
        internal uint segmentsResent;
        internal uint errorsReceived;
        internal uint segmentsSentWithReset;
        internal uint cumulativeConnections;
    }
}

