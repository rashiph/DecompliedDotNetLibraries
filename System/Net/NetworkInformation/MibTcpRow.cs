namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpRow
    {
        internal TcpState state;
        internal uint localAddr;
        internal byte localPort1;
        internal byte localPort2;
        internal byte ignoreLocalPort3;
        internal byte ignoreLocalPort4;
        internal uint remoteAddr;
        internal byte remotePort1;
        internal byte remotePort2;
        internal byte ignoreRemotePort3;
        internal byte ignoreRemotePort4;
    }
}

