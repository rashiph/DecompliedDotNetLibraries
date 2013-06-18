namespace System.Security.Permissions
{
    using System;

    [Serializable, Flags]
    public enum DataProtectionPermissionFlags
    {
        AllFlags = 15,
        NoFlags = 0,
        ProtectData = 1,
        ProtectMemory = 4,
        UnprotectData = 2,
        UnprotectMemory = 8
    }
}

