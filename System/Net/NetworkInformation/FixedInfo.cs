namespace System.Net.NetworkInformation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FixedInfo
    {
        internal FIXED_INFO info;
        internal IPAddressCollection dnsAddresses;
        internal FixedInfo(FIXED_INFO info)
        {
            this.info = info;
            this.dnsAddresses = info.DnsServerList.ToIPAddressCollection();
        }

        internal IPAddressCollection DnsAddresses
        {
            get
            {
                return this.dnsAddresses;
            }
        }
        internal string HostName
        {
            get
            {
                return this.info.hostName;
            }
        }
        internal string DomainName
        {
            get
            {
                return this.info.domainName;
            }
        }
        internal NetBiosNodeType NodeType
        {
            get
            {
                return this.info.nodeType;
            }
        }
        internal string ScopeId
        {
            get
            {
                return this.info.scopeId;
            }
        }
        internal bool EnableRouting
        {
            get
            {
                return this.info.enableRouting;
            }
        }
        internal bool EnableProxy
        {
            get
            {
                return this.info.enableProxy;
            }
        }
        internal bool EnableDns
        {
            get
            {
                return this.info.enableDns;
            }
        }
    }
}

