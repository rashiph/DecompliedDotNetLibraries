namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum FileSystemRights
    {
        AppendData = 4,
        ChangePermissions = 0x40000,
        CreateDirectories = 4,
        CreateFiles = 2,
        Delete = 0x10000,
        DeleteSubdirectoriesAndFiles = 0x40,
        ExecuteFile = 0x20,
        FullControl = 0x1f01ff,
        ListDirectory = 1,
        Modify = 0x301bf,
        Read = 0x20089,
        ReadAndExecute = 0x200a9,
        ReadAttributes = 0x80,
        ReadData = 1,
        ReadExtendedAttributes = 8,
        ReadPermissions = 0x20000,
        Synchronize = 0x100000,
        TakeOwnership = 0x80000,
        Traverse = 0x20,
        Write = 0x116,
        WriteAttributes = 0x100,
        WriteData = 2,
        WriteExtendedAttributes = 0x10
    }
}

