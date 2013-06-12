namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcp6RowOwnerPid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal byte[] localAddr;
        internal uint localScopeId;
        internal byte localPort1;
        internal byte localPort2;
        internal byte ignoreLocalPort3;
        internal byte ignoreLocalPort4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal byte[] remoteAddr;
        internal uint remoteScopeId;
        internal byte remotePort1;
        internal byte remotePort2;
        internal byte ignoreRemotePort3;
        internal byte ignoreRemotePort4;
        internal TcpState state;
        internal uint owningPid;
    }
}

