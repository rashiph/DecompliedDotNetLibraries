namespace System.Security.Util
{
    using System;

    [Serializable, Flags]
    internal enum QuickCacheEntryType
    {
        FullTrustAll = 0x20000000,
        FullTrustZoneInternet = 0x4000000,
        FullTrustZoneIntranet = 0x2000000,
        FullTrustZoneMyComputer = 0x1000000,
        FullTrustZoneTrusted = 0x8000000,
        FullTrustZoneUntrusted = 0x10000000
    }
}

