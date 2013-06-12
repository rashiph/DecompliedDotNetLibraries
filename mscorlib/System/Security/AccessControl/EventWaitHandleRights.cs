namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum EventWaitHandleRights
    {
        ChangePermissions = 0x40000,
        Delete = 0x10000,
        FullControl = 0x1f0003,
        Modify = 2,
        ReadPermissions = 0x20000,
        Synchronize = 0x100000,
        TakeOwnership = 0x80000
    }
}

