namespace System.IO.MemoryMappedFiles
{
    using System;

    [Flags]
    public enum MemoryMappedFileRights
    {
        AccessSystemSecurity = 0x1000000,
        ChangePermissions = 0x40000,
        CopyOnWrite = 1,
        Delete = 0x10000,
        Execute = 8,
        FullControl = 0xf000f,
        Read = 4,
        ReadExecute = 12,
        ReadPermissions = 0x20000,
        ReadWrite = 6,
        ReadWriteExecute = 14,
        TakeOwnership = 0x80000,
        Write = 2
    }
}

