namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibUdp6RowOwnerPid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal byte[] localAddr;
        internal uint localScopeId;
        internal byte localPort1;
        internal byte localPort2;
        internal byte ignoreLocalPort3;
        internal byte ignoreLocalPort4;
        internal uint owningPid;
    }
}

