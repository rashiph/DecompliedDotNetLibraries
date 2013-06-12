namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct MibIfRow
    {
        internal const int MAX_INTERFACE_NAME_LEN = 0x100;
        internal const int MAXLEN_IFDESCR = 0x100;
        internal const int MAXLEN_PHYSADDR = 8;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x100)]
        internal string wszName;
        internal uint dwIndex;
        internal uint dwType;
        internal uint dwMtu;
        internal uint dwSpeed;
        internal uint dwPhysAddrLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        internal byte[] bPhysAddr;
        internal uint dwAdminStatus;
        internal OldOperationalStatus operStatus;
        internal uint dwLastChange;
        internal uint dwInOctets;
        internal uint dwInUcastPkts;
        internal uint dwInNUcastPkts;
        internal uint dwInDiscards;
        internal uint dwInErrors;
        internal uint dwInUnknownProtos;
        internal uint dwOutOctets;
        internal uint dwOutUcastPkts;
        internal uint dwOutNUcastPkts;
        internal uint dwOutDiscards;
        internal uint dwOutErrors;
        internal uint dwOutQLen;
        internal uint dwDescrLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x100)]
        internal byte[] bDescr;
    }
}

