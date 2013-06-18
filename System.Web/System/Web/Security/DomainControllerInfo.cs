namespace System.Web.Security
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class DomainControllerInfo
    {
        public string DomainControllerName;
        public string DomainControllerAddress;
        public int DomainControllerAddressType;
        public Guid DomainGuid;
        public string DomainName;
        public string DnsForestName;
        public int Flags;
        public string DcSiteName;
        public string ClientSiteName;
    }
}

