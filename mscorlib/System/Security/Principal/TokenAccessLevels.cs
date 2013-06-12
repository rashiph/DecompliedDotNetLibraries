namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum TokenAccessLevels
    {
        AdjustDefault = 0x80,
        AdjustGroups = 0x40,
        AdjustPrivileges = 0x20,
        AdjustSessionId = 0x100,
        AllAccess = 0xf01ff,
        AssignPrimary = 1,
        Duplicate = 2,
        Impersonate = 4,
        MaximumAllowed = 0x2000000,
        Query = 8,
        QuerySource = 0x10,
        Read = 0x20008,
        Write = 0x200e0
    }
}

