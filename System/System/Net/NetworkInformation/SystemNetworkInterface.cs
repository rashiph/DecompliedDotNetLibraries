namespace System.Net.NetworkInformation
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal class SystemNetworkInterface : NetworkInterface
    {
        private AdapterFlags adapterFlags;
        private uint addressLength;
        private string description;
        private string id;
        internal uint index;
        private SystemIPInterfaceProperties interfaceProperties;
        internal uint ipv6Index;
        private string name;
        private System.Net.NetworkInformation.OperationalStatus operStatus;
        private byte[] physicalAddress;
        private long speed;
        private System.Net.NetworkInformation.NetworkInterfaceType type;

        private SystemNetworkInterface()
        {
        }

        internal SystemNetworkInterface(FixedInfo fixedInfo, IpAdapterAddresses ipAdapterAddresses)
        {
            this.id = ipAdapterAddresses.AdapterName;
            this.name = ipAdapterAddresses.friendlyName;
            this.description = ipAdapterAddresses.description;
            this.index = ipAdapterAddresses.index;
            this.physicalAddress = ipAdapterAddresses.address;
            this.addressLength = ipAdapterAddresses.addressLength;
            this.type = ipAdapterAddresses.type;
            this.operStatus = ipAdapterAddresses.operStatus;
            this.ipv6Index = ipAdapterAddresses.ipv6Index;
            this.adapterFlags = ipAdapterAddresses.flags;
            this.interfaceProperties = new SystemIPInterfaceProperties(fixedInfo, ipAdapterAddresses);
        }

        internal SystemNetworkInterface(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
        {
            this.id = ipAdapterInfo.adapterName;
            this.name = string.Empty;
            this.description = ipAdapterInfo.description;
            this.index = ipAdapterInfo.index;
            this.physicalAddress = ipAdapterInfo.address;
            this.addressLength = ipAdapterInfo.addressLength;
            if (ComNetOS.IsWin2K && !ComNetOS.IsPostWin2K)
            {
                this.name = this.ReadAdapterName(this.id);
            }
            if (this.name.Length == 0)
            {
                this.name = this.description;
            }
            SystemIPv4InterfaceStatistics statistics = new SystemIPv4InterfaceStatistics((long) this.index);
            this.operStatus = statistics.OperationalStatus;
            switch (ipAdapterInfo.type)
            {
                case OldInterfaceType.Ppp:
                    this.type = System.Net.NetworkInformation.NetworkInterfaceType.Ppp;
                    break;

                case OldInterfaceType.Loopback:
                    this.type = System.Net.NetworkInformation.NetworkInterfaceType.Loopback;
                    break;

                case OldInterfaceType.Slip:
                    this.type = System.Net.NetworkInformation.NetworkInterfaceType.Slip;
                    break;

                case OldInterfaceType.Fddi:
                    this.type = System.Net.NetworkInformation.NetworkInterfaceType.Fddi;
                    break;

                case OldInterfaceType.Ethernet:
                    this.type = System.Net.NetworkInformation.NetworkInterfaceType.Ethernet;
                    break;

                case OldInterfaceType.TokenRing:
                    this.type = System.Net.NetworkInformation.NetworkInterfaceType.TokenRing;
                    break;

                default:
                    this.type = System.Net.NetworkInformation.NetworkInterfaceType.Unknown;
                    break;
            }
            this.interfaceProperties = new SystemIPInterfaceProperties(fixedInfo, ipAdapterInfo);
        }

        private static SystemNetworkInterface[] GetAdaptersAddresses(AddressFamily family, FixedInfo fixedInfo)
        {
            uint outBufLen = 0;
            SafeLocalFree adapterAddresses = null;
            ArrayList list = new ArrayList();
            SystemNetworkInterface[] interfaceArray = null;
            uint num2 = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(family, 0, IntPtr.Zero, SafeLocalFree.Zero, ref outBufLen);
            while (num2 == 0x6f)
            {
                try
                {
                    adapterAddresses = SafeLocalFree.LocalAlloc((int) outBufLen);
                    num2 = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(family, 0, IntPtr.Zero, adapterAddresses, ref outBufLen);
                    if (num2 == 0)
                    {
                        IpAdapterAddresses ipAdapterAddresses = (IpAdapterAddresses) Marshal.PtrToStructure(adapterAddresses.DangerousGetHandle(), typeof(IpAdapterAddresses));
                        list.Add(new SystemNetworkInterface(fixedInfo, ipAdapterAddresses));
                        while (ipAdapterAddresses.next != IntPtr.Zero)
                        {
                            ipAdapterAddresses = (IpAdapterAddresses) Marshal.PtrToStructure(ipAdapterAddresses.next, typeof(IpAdapterAddresses));
                            list.Add(new SystemNetworkInterface(fixedInfo, ipAdapterAddresses));
                        }
                    }
                    continue;
                }
                finally
                {
                    if (adapterAddresses != null)
                    {
                        adapterAddresses.Close();
                    }
                    adapterAddresses = null;
                }
            }
            switch (num2)
            {
                case 0xe8:
                case 0x57:
                    return new SystemNetworkInterface[0];
            }
            if (num2 != 0)
            {
                throw new NetworkInformationException((int) num2);
            }
            interfaceArray = new SystemNetworkInterface[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                interfaceArray[i] = (SystemNetworkInterface) list[i];
            }
            return interfaceArray;
        }

        public override IPInterfaceProperties GetIPProperties()
        {
            return this.interfaceProperties;
        }

        public override IPv4InterfaceStatistics GetIPv4Statistics()
        {
            return new SystemIPv4InterfaceStatistics((long) this.index);
        }

        internal static NetworkInterface[] GetNetworkInterfaces()
        {
            return GetNetworkInterfaces(AddressFamily.Unspecified);
        }

        private static NetworkInterface[] GetNetworkInterfaces(AddressFamily family)
        {
            IpHelperErrors.CheckFamilyUnspecified(family);
            if (ComNetOS.IsPostWin2K)
            {
                return PostWin2KGetNetworkInterfaces(family);
            }
            FixedInfo fixedInfo = SystemIPGlobalProperties.GetFixedInfo();
            if ((family != AddressFamily.Unspecified) && (family != AddressFamily.InterNetwork))
            {
                throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
            }
            SafeLocalFree pAdapterInfo = null;
            uint pOutBufLen = 0;
            ArrayList list = new ArrayList();
            uint adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(SafeLocalFree.Zero, ref pOutBufLen);
            while (adaptersInfo == 0x6f)
            {
                try
                {
                    pAdapterInfo = SafeLocalFree.LocalAlloc((int) pOutBufLen);
                    adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(pAdapterInfo, ref pOutBufLen);
                    if (adaptersInfo == 0)
                    {
                        IpAdapterInfo ipAdapterInfo = (IpAdapterInfo) Marshal.PtrToStructure(pAdapterInfo.DangerousGetHandle(), typeof(IpAdapterInfo));
                        list.Add(new SystemNetworkInterface(fixedInfo, ipAdapterInfo));
                        while (ipAdapterInfo.Next != IntPtr.Zero)
                        {
                            ipAdapterInfo = (IpAdapterInfo) Marshal.PtrToStructure(ipAdapterInfo.Next, typeof(IpAdapterInfo));
                            list.Add(new SystemNetworkInterface(fixedInfo, ipAdapterInfo));
                        }
                    }
                    continue;
                }
                finally
                {
                    if (pAdapterInfo != null)
                    {
                        pAdapterInfo.Close();
                    }
                }
            }
            if (adaptersInfo == 0xe8)
            {
                return new SystemNetworkInterface[0];
            }
            if (adaptersInfo != 0)
            {
                throw new NetworkInformationException((int) adaptersInfo);
            }
            SystemNetworkInterface[] interfaceArray = new SystemNetworkInterface[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                interfaceArray[i] = (SystemNetworkInterface) list[i];
            }
            return interfaceArray;
        }

        public override PhysicalAddress GetPhysicalAddress()
        {
            byte[] destinationArray = new byte[this.addressLength];
            Array.Copy(this.physicalAddress, destinationArray, (long) this.addressLength);
            return new PhysicalAddress(destinationArray);
        }

        internal static bool InternalGetIsNetworkAvailable()
        {
            if (ComNetOS.IsWinNt)
            {
                foreach (NetworkInterface interface2 in GetNetworkInterfaces())
                {
                    if (((interface2.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up) && (interface2.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Tunnel)) && (interface2.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback))
                    {
                        return true;
                    }
                }
                return false;
            }
            uint flags = 0;
            return UnsafeWinINetNativeMethods.InternetGetConnectedState(ref flags, 0);
        }

        private static SystemNetworkInterface[] PostWin2KGetNetworkInterfaces(AddressFamily family)
        {
            FixedInfo fixedInfo = SystemIPGlobalProperties.GetFixedInfo();
            SystemNetworkInterface[] adaptersAddresses = null;
        Label_0008:
            try
            {
                adaptersAddresses = GetAdaptersAddresses(family, fixedInfo);
            }
            catch (NetworkInformationException exception)
            {
                if (exception.ErrorCode != 1L)
                {
                    throw;
                }
                goto Label_0008;
            }
            if (Socket.OSSupportsIPv4)
            {
                uint pOutBufLen = 0;
                uint adaptersInfo = 0;
                SafeLocalFree pAdapterInfo = null;
                if ((family == AddressFamily.Unspecified) || (family == AddressFamily.InterNetwork))
                {
                    adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(SafeLocalFree.Zero, ref pOutBufLen);
                    int num3 = 0;
                    while (adaptersInfo == 0x6f)
                    {
                        try
                        {
                            pAdapterInfo = SafeLocalFree.LocalAlloc((int) pOutBufLen);
                            adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(pAdapterInfo, ref pOutBufLen);
                            if (adaptersInfo == 0)
                            {
                                IntPtr handle = pAdapterInfo.DangerousGetHandle();
                                while (handle != IntPtr.Zero)
                                {
                                    IpAdapterInfo ipAdapterInfo = (IpAdapterInfo) Marshal.PtrToStructure(handle, typeof(IpAdapterInfo));
                                    for (int i = 0; i < adaptersAddresses.Length; i++)
                                    {
                                        if ((adaptersAddresses[i] != null) && (ipAdapterInfo.index == adaptersAddresses[i].index))
                                        {
                                            if (!adaptersAddresses[i].interfaceProperties.Update(fixedInfo, ipAdapterInfo))
                                            {
                                                adaptersAddresses[i] = null;
                                                num3++;
                                            }
                                            break;
                                        }
                                    }
                                    handle = ipAdapterInfo.Next;
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            if (pAdapterInfo != null)
                            {
                                pAdapterInfo.Close();
                            }
                        }
                    }
                    if (num3 != 0)
                    {
                        SystemNetworkInterface[] interfaceArray2 = new SystemNetworkInterface[adaptersAddresses.Length - num3];
                        int num5 = 0;
                        for (int j = 0; j < adaptersAddresses.Length; j++)
                        {
                            if (adaptersAddresses[j] != null)
                            {
                                interfaceArray2[num5++] = adaptersAddresses[j];
                            }
                        }
                        adaptersAddresses = interfaceArray2;
                    }
                }
                if ((adaptersInfo != 0) && (adaptersInfo != 0xe8))
                {
                    throw new NetworkInformationException((int) adaptersInfo);
                }
            }
            return adaptersAddresses;
        }

        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Network\{4D36E972-E325-11CE-BFC1-08002BE10318}")]
        private string ReadAdapterName(string id)
        {
            RegistryKey key = null;
            string str = string.Empty;
            try
            {
                string name = @"SYSTEM\CurrentControlSet\Control\Network\{4D36E972-E325-11CE-BFC1-08002BE10318}\" + id + @"\Connection";
                key = Registry.LocalMachine.OpenSubKey(name);
                if (key != null)
                {
                    str = (string) key.GetValue("Name");
                    if (str == null)
                    {
                        str = string.Empty;
                    }
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }
            return str;
        }

        public override bool Supports(NetworkInterfaceComponent networkInterfaceComponent)
        {
            return (((networkInterfaceComponent == NetworkInterfaceComponent.IPv6) && (this.ipv6Index > 0)) || ((networkInterfaceComponent == NetworkInterfaceComponent.IPv4) && (this.index > 0)));
        }

        public override string Description
        {
            get
            {
                return this.description;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        internal static int InternalLoopbackInterfaceIndex
        {
            get
            {
                int num;
                int bestInterface = (int) UnsafeNetInfoNativeMethods.GetBestInterface(0x100007f, out num);
                if (bestInterface != 0)
                {
                    throw new NetworkInformationException(bestInterface);
                }
                return num;
            }
        }

        public override bool IsReceiveOnly
        {
            get
            {
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                return ((this.adapterFlags & AdapterFlags.ReceiveOnly) > 0);
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override System.Net.NetworkInformation.NetworkInterfaceType NetworkInterfaceType
        {
            get
            {
                return this.type;
            }
        }

        public override System.Net.NetworkInformation.OperationalStatus OperationalStatus
        {
            get
            {
                return this.operStatus;
            }
        }

        public override long Speed
        {
            get
            {
                if (this.speed == 0L)
                {
                    SystemIPv4InterfaceStatistics statistics = new SystemIPv4InterfaceStatistics((long) this.index);
                    this.speed = statistics.Speed;
                }
                return this.speed;
            }
        }

        public override bool SupportsMulticast
        {
            get
            {
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                return ((this.adapterFlags & AdapterFlags.NoMulticast) == 0);
            }
        }
    }
}

