namespace System.Net.NetworkInformation
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNetInfoNativeMethods
    {
        private const string IPHLPAPI = "iphlpapi.dll";

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("iphlpapi.dll")]
        internal static extern uint CancelMibChangeNotify2(IntPtr notificationHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("iphlpapi.dll")]
        internal static extern void FreeMibTable(IntPtr handle);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetAdaptersAddresses(AddressFamily family, uint flags, IntPtr pReserved, SafeLocalFree adapterAddresses, ref uint outBufLen);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetAdaptersInfo(SafeLocalFree pAdapterInfo, ref uint pOutBufLen);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetBestInterface(int ipAddress, out int index);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetExtendedTcpTable(SafeLocalFree pTcpTable, ref uint dwOutBufLen, bool order, uint IPVersion, TcpTableClass tableClass, uint reserved);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetExtendedUdpTable(SafeLocalFree pUdpTable, ref uint dwOutBufLen, bool order, uint IPVersion, UdpTableClass tableClass, uint reserved);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetIcmpStatistics(out MibIcmpInfo statistics);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetIcmpStatisticsEx(out MibIcmpInfoEx statistics, AddressFamily family);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetIfEntry(ref MibIfRow pIfRow);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetIpStatistics(out MibIpStats statistics);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetIpStatisticsEx(out MibIpStats statistics, AddressFamily family);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetNetworkParams(SafeLocalFree pFixedInfo, ref uint pOutBufLen);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetPerAdapterInfo(uint IfIndex, SafeLocalFree pPerAdapterInfo, ref uint pOutBufLen);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetTcpStatistics(out MibTcpStats statistics);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetTcpStatisticsEx(out MibTcpStats statistics, AddressFamily family);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetTcpTable(SafeLocalFree pTcpTable, ref uint dwOutBufLen, bool order);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetUdpStatistics(out MibUdpStats statistics);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetUdpStatisticsEx(out MibUdpStats statistics, AddressFamily family);
        [DllImport("iphlpapi.dll")]
        internal static extern uint GetUdpTable(SafeLocalFree pUdpTable, ref uint dwOutBufLen, bool order);
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern SafeCloseIcmpHandle Icmp6CreateFile();
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern uint Icmp6ParseReplies(IntPtr replyBuffer, uint replySize);
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern uint Icmp6SendEcho2(SafeCloseIcmpHandle icmpHandle, SafeWaitHandle Event, IntPtr apcRoutine, IntPtr apcContext, byte[] sourceSocketAddress, byte[] destSocketAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern uint Icmp6SendEcho2(SafeCloseIcmpHandle icmpHandle, IntPtr Event, IntPtr apcRoutine, IntPtr apcContext, byte[] sourceSocketAddress, byte[] destSocketAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern bool IcmpCloseHandle(IntPtr handle);
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern SafeCloseIcmpHandle IcmpCreateFile();
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern uint IcmpParseReplies(IntPtr replyBuffer, uint replySize);
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern uint IcmpSendEcho2(SafeCloseIcmpHandle icmpHandle, SafeWaitHandle Event, IntPtr apcRoutine, IntPtr apcContext, uint ipAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);
        [DllImport("iphlpapi.dll", SetLastError=true)]
        internal static extern uint IcmpSendEcho2(SafeCloseIcmpHandle icmpHandle, IntPtr Event, IntPtr apcRoutine, IntPtr apcContext, uint ipAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);
        [DllImport("iphlpapi.dll")]
        internal static extern uint NotifyStableUnicastIpAddressTable([In] AddressFamily addressFamily, out SafeFreeMibTable table, [In, MarshalAs(UnmanagedType.FunctionPtr)] StableUnicastIpAddressTableDelegate callback, [In] IntPtr context, out SafeCancelMibChangeNotify notificationHandle);
    }
}

