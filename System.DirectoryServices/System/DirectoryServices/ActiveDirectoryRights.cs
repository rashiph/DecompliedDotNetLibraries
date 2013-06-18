namespace System.DirectoryServices
{
    using System;

    [Flags]
    public enum ActiveDirectoryRights
    {
        AccessSystemSecurity = 0x1000000,
        CreateChild = 1,
        Delete = 0x10000,
        DeleteChild = 2,
        DeleteTree = 0x40,
        ExtendedRight = 0x100,
        GenericAll = 0xf01ff,
        GenericExecute = 0x20004,
        GenericRead = 0x20094,
        GenericWrite = 0x20028,
        ListChildren = 4,
        ListObject = 0x80,
        ReadControl = 0x20000,
        ReadProperty = 0x10,
        Self = 8,
        Synchronize = 0x100000,
        WriteDacl = 0x40000,
        WriteOwner = 0x80000,
        WriteProperty = 0x20
    }
}

