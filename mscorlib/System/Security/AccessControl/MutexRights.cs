namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum MutexRights
    {
        ChangePermissions = 0x40000,
        Delete = 0x10000,
        FullControl = 0x1f0001,
        Modify = 1,
        ReadPermissions = 0x20000,
        Synchronize = 0x100000,
        TakeOwnership = 0x80000
    }
}

