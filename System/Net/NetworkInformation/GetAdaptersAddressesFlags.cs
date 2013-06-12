namespace System.Net.NetworkInformation
{
    using System;

    [Flags]
    internal enum GetAdaptersAddressesFlags
    {
        IncludePrefix = 0x10,
        SkipAnycast = 2,
        SkipDnsServer = 8,
        SkipFriendlyName = 0x20,
        SkipMulticast = 4,
        SkipUnicast = 1
    }
}

