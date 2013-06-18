namespace System.DirectoryServices
{
    using System;

    public enum ReferralChasingOption
    {
        All = 0x60,
        External = 0x40,
        None = 0,
        Subordinate = 0x20
    }
}

