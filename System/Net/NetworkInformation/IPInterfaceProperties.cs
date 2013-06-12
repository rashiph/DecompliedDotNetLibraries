namespace System.Net.NetworkInformation
{
    using System;

    public abstract class IPInterfaceProperties
    {
        protected IPInterfaceProperties()
        {
        }

        public abstract IPv4InterfaceProperties GetIPv4Properties();
        public abstract IPv6InterfaceProperties GetIPv6Properties();

        public abstract IPAddressInformationCollection AnycastAddresses { get; }

        public abstract IPAddressCollection DhcpServerAddresses { get; }

        public abstract IPAddressCollection DnsAddresses { get; }

        public abstract string DnsSuffix { get; }

        public abstract GatewayIPAddressInformationCollection GatewayAddresses { get; }

        public abstract bool IsDnsEnabled { get; }

        public abstract bool IsDynamicDnsEnabled { get; }

        public abstract MulticastIPAddressInformationCollection MulticastAddresses { get; }

        public abstract UnicastIPAddressInformationCollection UnicastAddresses { get; }

        public abstract IPAddressCollection WinsServersAddresses { get; }
    }
}

