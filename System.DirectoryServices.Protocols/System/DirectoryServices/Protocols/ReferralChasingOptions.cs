namespace System.DirectoryServices.Protocols
{
    using System;

    [Flags]
    public enum ReferralChasingOptions
    {
        All = 0x60,
        External = 0x40,
        None = 0,
        Subordinate = 0x20
    }
}

