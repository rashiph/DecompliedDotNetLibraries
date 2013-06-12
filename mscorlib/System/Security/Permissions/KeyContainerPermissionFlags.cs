namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum KeyContainerPermissionFlags
    {
        AllFlags = 0x3337,
        ChangeAcl = 0x2000,
        Create = 1,
        Decrypt = 0x200,
        Delete = 4,
        Export = 0x20,
        Import = 0x10,
        NoFlags = 0,
        Open = 2,
        Sign = 0x100,
        ViewAcl = 0x1000
    }
}

