namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class POLICY_DNS_DOMAIN_INFO
    {
        public LSA_UNICODE_STRING Name;
        public LSA_UNICODE_STRING DnsDomainName;
        public LSA_UNICODE_STRING DnsForestName;
        public Guid DomainGuid;
        public IntPtr Sid;
    }
}

