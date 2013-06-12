namespace System.Net.NetworkInformation
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class SystemIPGlobalProperties : IPGlobalProperties
    {
        private static string domainName = null;
        private System.Net.NetworkInformation.FixedInfo fixedInfo;
        private bool fixedInfoInitialized;
        private static string hostName = null;
        private static object syncObject = new object();

        internal SystemIPGlobalProperties()
        {
        }

        public override IAsyncResult BeginGetUnicastAddresses(AsyncCallback callback, object state)
        {
            if (!ComNetOS.IsVista)
            {
                throw new PlatformNotSupportedException(SR.GetString("VistaRequired"));
            }
            ContextAwareResult result = new ContextAwareResult(false, false, this, state, callback);
            result.StartPostingAsyncOp(false);
            if (TeredoHelper.UnsafeNotifyStableUnicastIpAddressTable(new Action<object>(SystemIPGlobalProperties.StableUnicastAddressTableCallback), result))
            {
                result.InvokeCallback();
            }
            result.FinishPostingAsyncOp();
            return result;
        }

        public override UnicastIPAddressInformationCollection EndGetUnicastAddresses(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            ContextAwareResult result = asyncResult as ContextAwareResult;
            if (((result == null) || (result.AsyncObject == null)) || (result.AsyncObject.GetType() != typeof(SystemIPGlobalProperties)))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"));
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndGetStableUnicastAddresses" }));
            }
            result.InternalWaitForCompletion();
            result.EndCalled = true;
            return GetUnicastAddressTable();
        }

        public override TcpConnectionInformation[] GetActiveTcpConnections()
        {
            List<TcpConnectionInformation> list = new List<TcpConnectionInformation>();
            foreach (TcpConnectionInformation information in this.GetAllTcpConnections())
            {
                if (information.State != TcpState.Listen)
                {
                    list.Add(information);
                }
            }
            return list.ToArray();
        }

        public override IPEndPoint[] GetActiveTcpListeners()
        {
            List<IPEndPoint> list = new List<IPEndPoint>();
            foreach (TcpConnectionInformation information in this.GetAllTcpConnections())
            {
                if (information.State == TcpState.Listen)
                {
                    list.Add(information.LocalEndPoint);
                }
            }
            return list.ToArray();
        }

        public override IPEndPoint[] GetActiveUdpListeners()
        {
            uint dwOutBufLen = 0;
            uint num2 = 0;
            SafeLocalFree pUdpTable = null;
            List<IPEndPoint> list = new List<IPEndPoint>();
            if (Socket.OSSupportsIPv4)
            {
                num2 = UnsafeNetInfoNativeMethods.GetUdpTable(SafeLocalFree.Zero, ref dwOutBufLen, true);
                while (num2 == 0x7a)
                {
                    try
                    {
                        pUdpTable = SafeLocalFree.LocalAlloc((int) dwOutBufLen);
                        num2 = UnsafeNetInfoNativeMethods.GetUdpTable(pUdpTable, ref dwOutBufLen, true);
                        if (num2 == 0)
                        {
                            IntPtr handle = pUdpTable.DangerousGetHandle();
                            MibUdpTable table = (MibUdpTable) Marshal.PtrToStructure(handle, typeof(MibUdpTable));
                            if (table.numberOfEntries > 0)
                            {
                                handle = (IntPtr) (((long) handle) + Marshal.SizeOf(table.numberOfEntries));
                                for (int i = 0; i < table.numberOfEntries; i++)
                                {
                                    MibUdpRow structure = (MibUdpRow) Marshal.PtrToStructure(handle, typeof(MibUdpRow));
                                    int port = (structure.localPort1 << 8) | structure.localPort2;
                                    list.Add(new IPEndPoint((long) structure.localAddr, port));
                                    handle = (IntPtr) (((long) handle) + Marshal.SizeOf(structure));
                                }
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (pUdpTable != null)
                        {
                            pUdpTable.Close();
                        }
                    }
                }
                if ((num2 != 0) && (num2 != 0xe8))
                {
                    throw new NetworkInformationException((int) num2);
                }
            }
            if (Socket.OSSupportsIPv6)
            {
                dwOutBufLen = 0;
                num2 = UnsafeNetInfoNativeMethods.GetExtendedUdpTable(SafeLocalFree.Zero, ref dwOutBufLen, true, 0x17, UdpTableClass.UdpTableOwnerPid, 0);
                while (num2 == 0x7a)
                {
                    try
                    {
                        pUdpTable = SafeLocalFree.LocalAlloc((int) dwOutBufLen);
                        num2 = UnsafeNetInfoNativeMethods.GetExtendedUdpTable(pUdpTable, ref dwOutBufLen, true, 0x17, UdpTableClass.UdpTableOwnerPid, 0);
                        if (num2 == 0)
                        {
                            IntPtr ptr = pUdpTable.DangerousGetHandle();
                            MibUdp6TableOwnerPid pid = (MibUdp6TableOwnerPid) Marshal.PtrToStructure(ptr, typeof(MibUdp6TableOwnerPid));
                            if (pid.numberOfEntries > 0)
                            {
                                ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(pid.numberOfEntries));
                                for (int j = 0; j < pid.numberOfEntries; j++)
                                {
                                    MibUdp6RowOwnerPid pid2 = (MibUdp6RowOwnerPid) Marshal.PtrToStructure(ptr, typeof(MibUdp6RowOwnerPid));
                                    int num6 = (pid2.localPort1 << 8) | pid2.localPort2;
                                    list.Add(new IPEndPoint(new IPAddress(pid2.localAddr, (long) pid2.localScopeId), num6));
                                    ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(pid2));
                                }
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (pUdpTable != null)
                        {
                            pUdpTable.Close();
                        }
                    }
                }
                if ((num2 != 0) && (num2 != 0xe8))
                {
                    throw new NetworkInformationException((int) num2);
                }
            }
            return list.ToArray();
        }

        private List<SystemTcpConnectionInformation> GetAllTcpConnections()
        {
            uint dwOutBufLen = 0;
            uint num2 = 0;
            SafeLocalFree pTcpTable = null;
            List<SystemTcpConnectionInformation> list = new List<SystemTcpConnectionInformation>();
            if (Socket.OSSupportsIPv4)
            {
                num2 = UnsafeNetInfoNativeMethods.GetTcpTable(SafeLocalFree.Zero, ref dwOutBufLen, true);
                while (num2 == 0x7a)
                {
                    try
                    {
                        pTcpTable = SafeLocalFree.LocalAlloc((int) dwOutBufLen);
                        num2 = UnsafeNetInfoNativeMethods.GetTcpTable(pTcpTable, ref dwOutBufLen, true);
                        if (num2 == 0)
                        {
                            IntPtr handle = pTcpTable.DangerousGetHandle();
                            MibTcpTable table = (MibTcpTable) Marshal.PtrToStructure(handle, typeof(MibTcpTable));
                            if (table.numberOfEntries > 0)
                            {
                                handle = (IntPtr) (((long) handle) + Marshal.SizeOf(table.numberOfEntries));
                                for (int i = 0; i < table.numberOfEntries; i++)
                                {
                                    MibTcpRow row = (MibTcpRow) Marshal.PtrToStructure(handle, typeof(MibTcpRow));
                                    list.Add(new SystemTcpConnectionInformation(row));
                                    handle = (IntPtr) (((long) handle) + Marshal.SizeOf(row));
                                }
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (pTcpTable != null)
                        {
                            pTcpTable.Close();
                        }
                    }
                }
                if ((num2 != 0) && (num2 != 0xe8))
                {
                    throw new NetworkInformationException((int) num2);
                }
            }
            if (Socket.OSSupportsIPv6)
            {
                dwOutBufLen = 0;
                num2 = UnsafeNetInfoNativeMethods.GetExtendedTcpTable(SafeLocalFree.Zero, ref dwOutBufLen, true, 0x17, TcpTableClass.TcpTableOwnerPidAll, 0);
                while (num2 == 0x7a)
                {
                    try
                    {
                        pTcpTable = SafeLocalFree.LocalAlloc((int) dwOutBufLen);
                        num2 = UnsafeNetInfoNativeMethods.GetExtendedTcpTable(pTcpTable, ref dwOutBufLen, true, 0x17, TcpTableClass.TcpTableOwnerPidAll, 0);
                        if (num2 == 0)
                        {
                            IntPtr ptr = pTcpTable.DangerousGetHandle();
                            MibTcp6TableOwnerPid pid = (MibTcp6TableOwnerPid) Marshal.PtrToStructure(ptr, typeof(MibTcp6TableOwnerPid));
                            if (pid.numberOfEntries > 0)
                            {
                                ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(pid.numberOfEntries));
                                for (int j = 0; j < pid.numberOfEntries; j++)
                                {
                                    MibTcp6RowOwnerPid pid2 = (MibTcp6RowOwnerPid) Marshal.PtrToStructure(ptr, typeof(MibTcp6RowOwnerPid));
                                    list.Add(new SystemTcpConnectionInformation(pid2));
                                    ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(pid2));
                                }
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (pTcpTable != null)
                        {
                            pTcpTable.Close();
                        }
                    }
                }
                if ((num2 != 0) && (num2 != 0xe8))
                {
                    throw new NetworkInformationException((int) num2);
                }
            }
            return list;
        }

        internal static System.Net.NetworkInformation.FixedInfo GetFixedInfo()
        {
            uint pOutBufLen = 0;
            SafeLocalFree pFixedInfo = null;
            System.Net.NetworkInformation.FixedInfo info = new System.Net.NetworkInformation.FixedInfo();
            uint networkParams = UnsafeNetInfoNativeMethods.GetNetworkParams(SafeLocalFree.Zero, ref pOutBufLen);
            while (networkParams == 0x6f)
            {
                try
                {
                    pFixedInfo = SafeLocalFree.LocalAlloc((int) pOutBufLen);
                    networkParams = UnsafeNetInfoNativeMethods.GetNetworkParams(pFixedInfo, ref pOutBufLen);
                    if (networkParams == 0)
                    {
                        info = new System.Net.NetworkInformation.FixedInfo((FIXED_INFO) Marshal.PtrToStructure(pFixedInfo.DangerousGetHandle(), typeof(FIXED_INFO)));
                    }
                    continue;
                }
                finally
                {
                    if (pFixedInfo != null)
                    {
                        pFixedInfo.Close();
                    }
                }
            }
            if (networkParams != 0)
            {
                throw new NetworkInformationException((int) networkParams);
            }
            return info;
        }

        public override IcmpV4Statistics GetIcmpV4Statistics()
        {
            return new SystemIcmpV4Statistics();
        }

        public override IcmpV6Statistics GetIcmpV6Statistics()
        {
            return new SystemIcmpV6Statistics();
        }

        public override IPGlobalStatistics GetIPv4GlobalStatistics()
        {
            return new SystemIPGlobalStatistics(AddressFamily.InterNetwork);
        }

        public override IPGlobalStatistics GetIPv6GlobalStatistics()
        {
            return new SystemIPGlobalStatistics(AddressFamily.InterNetworkV6);
        }

        public override TcpStatistics GetTcpIPv4Statistics()
        {
            return new SystemTcpStatistics(AddressFamily.InterNetwork);
        }

        public override TcpStatistics GetTcpIPv6Statistics()
        {
            return new SystemTcpStatistics(AddressFamily.InterNetworkV6);
        }

        public override UdpStatistics GetUdpIPv4Statistics()
        {
            return new SystemUdpStatistics(AddressFamily.InterNetwork);
        }

        public override UdpStatistics GetUdpIPv6Statistics()
        {
            return new SystemUdpStatistics(AddressFamily.InterNetworkV6);
        }

        public override UnicastIPAddressInformationCollection GetUnicastAddresses()
        {
            if (!ComNetOS.IsVista)
            {
                throw new PlatformNotSupportedException(SR.GetString("VistaRequired"));
            }
            using (ManualResetEvent event2 = new ManualResetEvent(false))
            {
                if (!TeredoHelper.UnsafeNotifyStableUnicastIpAddressTable(new Action<object>(SystemIPGlobalProperties.StableUnicastAddressTableCallback), event2))
                {
                    event2.WaitOne();
                }
            }
            return GetUnicastAddressTable();
        }

        private static UnicastIPAddressInformationCollection GetUnicastAddressTable()
        {
            UnicastIPAddressInformationCollection informations = new UnicastIPAddressInformationCollection();
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < allNetworkInterfaces.Length; i++)
            {
                foreach (UnicastIPAddressInformation information in allNetworkInterfaces[i].GetIPProperties().UnicastAddresses)
                {
                    if (!informations.Contains(information))
                    {
                        informations.InternalAdd(information);
                    }
                }
            }
            return informations;
        }

        private static void StableUnicastAddressTableCallback(object param)
        {
            EventWaitHandle handle = param as EventWaitHandle;
            if (handle != null)
            {
                handle.Set();
            }
            else
            {
                ((LazyAsyncResult) param).InvokeCallback();
            }
        }

        public override string DhcpScopeName
        {
            get
            {
                return this.FixedInfo.ScopeId;
            }
        }

        public override string DomainName
        {
            get
            {
                if (domainName == null)
                {
                    lock (syncObject)
                    {
                        if (domainName == null)
                        {
                            hostName = this.FixedInfo.HostName;
                            domainName = this.FixedInfo.DomainName;
                        }
                    }
                }
                return domainName;
            }
        }

        internal System.Net.NetworkInformation.FixedInfo FixedInfo
        {
            get
            {
                if (!this.fixedInfoInitialized)
                {
                    lock (this)
                    {
                        if (!this.fixedInfoInitialized)
                        {
                            this.fixedInfo = GetFixedInfo();
                            this.fixedInfoInitialized = true;
                        }
                    }
                }
                return this.fixedInfo;
            }
        }

        public override string HostName
        {
            get
            {
                if (hostName == null)
                {
                    lock (syncObject)
                    {
                        if (hostName == null)
                        {
                            hostName = this.FixedInfo.HostName;
                            domainName = this.FixedInfo.DomainName;
                        }
                    }
                }
                return hostName;
            }
        }

        public override bool IsWinsProxy
        {
            get
            {
                return this.FixedInfo.EnableProxy;
            }
        }

        public override NetBiosNodeType NodeType
        {
            get
            {
                return this.FixedInfo.NodeType;
            }
        }
    }
}

