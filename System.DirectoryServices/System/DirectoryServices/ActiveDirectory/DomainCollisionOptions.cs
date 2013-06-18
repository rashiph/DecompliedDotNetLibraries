namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    [Flags]
    public enum DomainCollisionOptions
    {
        NetBiosNameDisabledByAdmin = 4,
        NetBiosNameDisabledByConflict = 8,
        None = 0,
        SidDisabledByAdmin = 1,
        SidDisabledByConflict = 2
    }
}

