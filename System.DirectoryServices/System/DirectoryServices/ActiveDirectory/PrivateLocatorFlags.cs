namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    [Flags]
    internal enum PrivateLocatorFlags : long
    {
        BackgroundOnly = 0x100L,
        DirectoryServicesPreferred = 0x20L,
        DirectoryServicesRequired = 0x10L,
        DSWriteableRequired = 0x1000L,
        GCRequired = 0x40L,
        GoodTimeServerPreferred = 0x2000L,
        IPRequired = 0x200L,
        IsDNSName = 0x20000L,
        IsFlatName = 0x10000L,
        OnlyLDAPNeeded = 0x8000L,
        PdcRequired = 0x80L,
        ReturnDNSName = 0x40000000L,
        ReturnFlatName = 0x80000000L
    }
}

