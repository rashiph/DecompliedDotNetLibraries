namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct IpAdapterAddresses
    {
        internal const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        internal uint length;
        internal uint index;
        internal IntPtr next;
        [MarshalAs(UnmanagedType.LPStr)]
        internal string AdapterName;
        internal IntPtr FirstUnicastAddress;
        internal IntPtr FirstAnycastAddress;
        internal IntPtr FirstMulticastAddress;
        internal IntPtr FirstDnsServerAddress;
        internal string dnsSuffix;
        internal string description;
        internal string friendlyName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        internal byte[] address;
        internal uint addressLength;
        internal AdapterFlags flags;
        internal uint mtu;
        internal NetworkInterfaceType type;
        internal OperationalStatus operStatus;
        internal uint ipv6Index;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal uint[] zoneIndices;
        internal IntPtr firstPrefix;
    }
}

