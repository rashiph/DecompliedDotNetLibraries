namespace System.Net.NetworkInformation
{
    using Microsoft.Win32;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Permissions;

    internal class SystemIPInterfaceProperties : IPInterfaceProperties
    {
        private AdapterFlags adapterFlags;
        private IPAddressInformationCollection anycastAddresses;
        private IPAddressCollection dnsAddresses;
        private bool dnsEnabled;
        private string dnsSuffix;
        private bool dynamicDnsEnabled;
        internal uint index;
        private SystemIPv4InterfaceProperties ipv4Properties;
        internal uint ipv6Index;
        private SystemIPv6InterfaceProperties ipv6Properties;
        private uint mtu;
        private MulticastIPAddressInformationCollection multicastAddresses;
        private string name;
        private UnicastIPAddressInformationCollection unicastAddresses;
        internal IPVersion versionSupported;

        private SystemIPInterfaceProperties()
        {
        }

        internal SystemIPInterfaceProperties(FixedInfo fixedInfo, IpAdapterAddresses ipAdapterAddresses)
        {
            this.dnsEnabled = fixedInfo.EnableDns;
            this.index = ipAdapterAddresses.index;
            this.name = ipAdapterAddresses.AdapterName;
            this.ipv6Index = ipAdapterAddresses.ipv6Index;
            if (this.index > 0)
            {
                this.versionSupported |= IPVersion.IPv4;
            }
            if (this.ipv6Index > 0)
            {
                this.versionSupported |= IPVersion.IPv6;
            }
            this.mtu = ipAdapterAddresses.mtu;
            this.adapterFlags = ipAdapterAddresses.flags;
            this.dnsSuffix = ipAdapterAddresses.dnsSuffix;
            this.dynamicDnsEnabled = (ipAdapterAddresses.flags & AdapterFlags.DnsEnabled) > 0;
            this.multicastAddresses = SystemMulticastIPAddressInformation.ToAddressInformationCollection(ipAdapterAddresses.FirstMulticastAddress);
            this.dnsAddresses = SystemIPAddressInformation.ToAddressCollection(ipAdapterAddresses.FirstDnsServerAddress, this.versionSupported);
            this.anycastAddresses = SystemIPAddressInformation.ToAddressInformationCollection(ipAdapterAddresses.FirstAnycastAddress, this.versionSupported);
            this.unicastAddresses = SystemUnicastIPAddressInformation.ToAddressInformationCollection(ipAdapterAddresses.FirstUnicastAddress);
            if (this.ipv6Index > 0)
            {
                this.ipv6Properties = new SystemIPv6InterfaceProperties(this.ipv6Index, this.mtu);
            }
        }

        internal SystemIPInterfaceProperties(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
        {
            this.dnsEnabled = fixedInfo.EnableDns;
            this.name = ipAdapterInfo.adapterName;
            this.index = ipAdapterInfo.index;
            this.multicastAddresses = new MulticastIPAddressInformationCollection();
            this.anycastAddresses = new IPAddressInformationCollection();
            if (this.index > 0)
            {
                this.versionSupported |= IPVersion.IPv4;
            }
            if (ComNetOS.IsWin2K)
            {
                this.ReadRegDnsSuffix();
            }
            this.unicastAddresses = new UnicastIPAddressInformationCollection();
            foreach (IPExtendedAddress address in ipAdapterInfo.ipAddressList.ToIPExtendedAddressArrayList())
            {
                this.unicastAddresses.InternalAdd(new SystemUnicastIPAddressInformation(ipAdapterInfo, address));
            }
            try
            {
                this.ipv4Properties = new SystemIPv4InterfaceProperties(fixedInfo, ipAdapterInfo);
                if ((this.dnsAddresses == null) || (this.dnsAddresses.Count == 0))
                {
                    this.dnsAddresses = this.ipv4Properties.DnsAddresses;
                }
            }
            catch (NetworkInformationException exception)
            {
                if (exception.ErrorCode != 0x57L)
                {
                    throw;
                }
            }
        }

        public override IPv4InterfaceProperties GetIPv4Properties()
        {
            if (this.index == 0)
            {
                throw new NetworkInformationException(SocketError.ProtocolNotSupported);
            }
            return this.ipv4Properties;
        }

        public override IPv6InterfaceProperties GetIPv6Properties()
        {
            if (this.ipv6Index == 0)
            {
                throw new NetworkInformationException(SocketError.ProtocolNotSupported);
            }
            return this.ipv6Properties;
        }

        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces")]
        private void ReadRegDnsSuffix()
        {
            RegistryKey key = null;
            try
            {
                string name = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\" + this.name;
                key = Registry.LocalMachine.OpenSubKey(name);
                if (key != null)
                {
                    this.dnsSuffix = (string) key.GetValue("DhcpDomain");
                    if (this.dnsSuffix == null)
                    {
                        this.dnsSuffix = (string) key.GetValue("Domain");
                        if (this.dnsSuffix == null)
                        {
                            this.dnsSuffix = string.Empty;
                        }
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
        }

        internal bool Update(FixedInfo fixedInfo, IpAdapterInfo ipAdapterInfo)
        {
            try
            {
                foreach (IPExtendedAddress address in ipAdapterInfo.ipAddressList.ToIPExtendedAddressArrayList())
                {
                    foreach (SystemUnicastIPAddressInformation information in this.unicastAddresses)
                    {
                        if (address.address.Equals(information.Address))
                        {
                            information.ipv4Mask = address.mask;
                        }
                    }
                }
                this.ipv4Properties = new SystemIPv4InterfaceProperties(fixedInfo, ipAdapterInfo);
                if ((this.dnsAddresses == null) || (this.dnsAddresses.Count == 0))
                {
                    this.dnsAddresses = this.ipv4Properties.DnsAddresses;
                }
            }
            catch (NetworkInformationException exception)
            {
                if (((exception.ErrorCode != 0x57L) && (exception.ErrorCode != 13L)) && (((exception.ErrorCode != 0xe8L) && (exception.ErrorCode != 1L)) && (exception.ErrorCode != 2L)))
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        public override IPAddressInformationCollection AnycastAddresses
        {
            get
            {
                return this.anycastAddresses;
            }
        }

        public override IPAddressCollection DhcpServerAddresses
        {
            get
            {
                if (this.ipv4Properties != null)
                {
                    return this.ipv4Properties.GetDhcpServerAddresses();
                }
                return new IPAddressCollection();
            }
        }

        public override IPAddressCollection DnsAddresses
        {
            get
            {
                return this.dnsAddresses;
            }
        }

        public override string DnsSuffix
        {
            get
            {
                if (!ComNetOS.IsWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
                }
                return this.dnsSuffix;
            }
        }

        public override GatewayIPAddressInformationCollection GatewayAddresses
        {
            get
            {
                if (this.ipv4Properties != null)
                {
                    return this.ipv4Properties.GetGatewayAddresses();
                }
                return new GatewayIPAddressInformationCollection();
            }
        }

        public override bool IsDnsEnabled
        {
            get
            {
                return this.dnsEnabled;
            }
        }

        public override bool IsDynamicDnsEnabled
        {
            get
            {
                return this.dynamicDnsEnabled;
            }
        }

        public override MulticastIPAddressInformationCollection MulticastAddresses
        {
            get
            {
                return this.multicastAddresses;
            }
        }

        public override UnicastIPAddressInformationCollection UnicastAddresses
        {
            get
            {
                return this.unicastAddresses;
            }
        }

        public override IPAddressCollection WinsServersAddresses
        {
            get
            {
                if (this.ipv4Properties != null)
                {
                    return this.ipv4Properties.GetWinsServersAddresses();
                }
                return new IPAddressCollection();
            }
        }
    }
}

