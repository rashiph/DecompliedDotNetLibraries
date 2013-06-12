namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcp6TableOwnerPid
    {
        internal uint numberOfEntries;
    }
}

