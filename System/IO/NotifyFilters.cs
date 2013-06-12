namespace System.IO
{
    using System;

    [Flags]
    public enum NotifyFilters
    {
        Attributes = 4,
        CreationTime = 0x40,
        DirectoryName = 2,
        FileName = 1,
        LastAccess = 0x20,
        LastWrite = 0x10,
        Security = 0x100,
        Size = 8
    }
}

