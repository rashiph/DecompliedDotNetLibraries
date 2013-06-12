namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class SystemIPv4InterfaceProperties : IPv4InterfaceProperties
    {
        private bool autoConfigActive;
        private bool autoConfigEnabled;
        private IPAddressCollection dhcpAddresses;
        private bool dhcpEnabled;
        internal IPAddressCollection dnsAddresses;
        private GatewayIPAddressInformationCollection gatewayAddresses;
        private bool haveWins;
        private uint index;
        private uint mtu;
        private bool routingEnabled;
        private IPAddressCollection winsServerAddresses;

        internal SystemIPv4InterfaceProperties(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
        {
            this.index = ipAdapterInfo.index;
            this.routingEnabled = fixedInfo.EnableRouting;
            this.dhcpEnabled = ipAdapterInfo.dhcpEnabled;
            this.haveWins = ipAdapterInfo.haveWins;
            this.gatewayAddresses = ipAdapterInfo.gatewayList.ToIPGatewayAddressCollection();
            this.dhcpAddresses = ipAdapterInfo.dhcpServer.ToIPAddressCollection();
            IPAddressCollection addresss = ipAdapterInfo.primaryWinsServer.ToIPAddressCollection();
            IPAddressCollection addresss2 = ipAdapterInfo.secondaryWinsServer.ToIPAddressCollection();
            this.winsServerAddresses = new IPAddressCollection();
            foreach (IPAddress address in addresss)
            {
                this.winsServerAddresses.InternalAdd(address);
            }
            foreach (IPAddress address2 in addresss2)
            {
                this.winsServerAddresses.InternalAdd(address2);
            }
            SystemIPv4InterfaceStatistics statistics = new SystemIPv4InterfaceStatistics((long) this.index);
            this.mtu = (uint) statistics.Mtu;
            if (ComNetOS.IsWin2K)
            {
                this.GetPerAdapterInfo(ipAdapterInfo.index);
            }
            else
            {
                this.dnsAddresses = fixedInfo.DnsAddresses;
            }
        }

        internal IPAddressCollection GetDhcpServerAddresses()
        {
            return this.dhcpAddresses;
        }

        internal GatewayIPAddressInformationCollection GetGatewayAddresses()
        {
            return this.gatewayAddresses;
        }

        private void GetPerAdapterInfo(uint index)
        {
            if (index != 0)
            {
                uint pOutBufLen = 0;
                SafeLocalFree pPerAdapterInfo = null;
                uint num2 = UnsafeNetInfoNativeMethods.GetPerAdapterInfo(index, SafeLocalFree.Zero, ref pOutBufLen);
                while (num2 == 0x6f)
                {
                    try
                    {
                        pPerAdapterInfo = SafeLocalFree.LocalAlloc((int) pOutBufLen);
                        num2 = UnsafeNetInfoNativeMethods.GetPerAdapterInfo(index, pPerAdapterInfo, ref pOutBufLen);
                        if (num2 == 0)
                        {
                            IpPerAdapterInfo info = (IpPerAdapterInfo) Marshal.PtrToStructure(pPerAdapterInfo.DangerousGetHandle(), typeof(IpPerAdapterInfo));
                            this.autoConfigEnabled = info.autoconfigEnabled;
                            this.autoConfigActive = info.autoconfigActive;
                            this.dnsAddresses = info.dnsServerList.ToIPAddressCollection();
                        }
                        continue;
                    }
                    finally
                    {
                        if (this.dnsAddresses == null)
                        {
                            this.dnsAddresses = new IPAddressCollection();
                        }
                        if (pPerAdapterInfo != null)
                        {
                            pPerAdapterInfo.Close();
                        }
                    }
                }
                if (this.dnsAddresses == null)
                {
                    this.dnsAddresses = new IPAddressCollection();
                }
                if (num2 != 0)
                {
                    throw new NetworkInformationException((int) num2);
                }
            }
        }

        internal IPAddressCollection GetWinsServersAddresses()
        {
            return this.winsServerAddresses;
        }

        internal IPAddressCollection DnsAddresses
        {
            get
            {
                return this.dnsAddresses;
            }
        }

        public override int Index
        {
            get
            {
                return (int) this.index;
            }
        }

        public override bool IsAutomaticPrivateAddressingActive
        {
            get
            {
                return this.autoConfigActive;
            }
        }

        public override bool IsAutomaticPrivateAddressingEnabled
        {
            get
            {
                return this.autoConfigEnabled;
            }
        }

        public override bool IsDhcpEnabled
        {
            get
            {
                return this.dhcpEnabled;
            }
        }

        public override bool IsForwardingEnabled
        {
            get
            {
                return this.routingEnabled;
            }
        }

        public override int Mtu
        {
            get
            {
                return (int) this.mtu;
            }
        }

        public override bool UsesWins
        {
            get
            {
                return this.haveWins;
            }
        }
    }
}

