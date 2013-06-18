namespace System.DirectoryServices.Protocols
{
    using System;

    [Flags]
    public enum LocatorFlags : long
    {
        AvoidSelf = 0x4000L,
        DirectoryServicesPreferred = 0x20L,
        DirectoryServicesRequired = 0x10L,
        ForceRediscovery = 1L,
        GCRequired = 0x40L,
        GoodTimeServerPreferred = 0x2000L,
        IPRequired = 0x200L,
        IsDnsName = 0x20000L,
        IsFlatName = 0x10000L,
        KdcRequired = 0x400L,
        None = 0L,
        OnlyLdapNeeded = 0x8000L,
        PdcRequired = 0x80L,
        ReturnDnsName = 0x40000000L,
        ReturnFlatName = 0x80000000L,
        TimeServerRequired = 0x800L,
        WriteableRequired = 0x1000L
    }
}

