namespace System.Net.NetworkInformation
{
    using System;

    [Flags]
    internal enum AdapterAddressFlags
    {
        DnsEligible = 1,
        Transient = 2
    }
}

