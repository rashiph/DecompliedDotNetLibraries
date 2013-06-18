namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    [Flags]
    public enum LocatorOptions : long
    {
        AvoidSelf = 0x4000L,
        ForceRediscovery = 1L,
        KdcRequired = 0x400L,
        TimeServerRequired = 0x800L,
        WriteableRequired = 0x1000L
    }
}

