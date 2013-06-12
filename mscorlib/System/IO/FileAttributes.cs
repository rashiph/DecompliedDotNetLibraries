namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum FileAttributes
    {
        Archive = 0x20,
        Compressed = 0x800,
        Device = 0x40,
        Directory = 0x10,
        Encrypted = 0x4000,
        Hidden = 2,
        Normal = 0x80,
        NotContentIndexed = 0x2000,
        Offline = 0x1000,
        ReadOnly = 1,
        ReparsePoint = 0x400,
        SparseFile = 0x200,
        System = 4,
        Temporary = 0x100
    }
}

