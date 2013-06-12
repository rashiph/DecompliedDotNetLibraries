namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum FileOptions
    {
        Asynchronous = 0x40000000,
        DeleteOnClose = 0x4000000,
        Encrypted = 0x4000,
        None = 0,
        RandomAccess = 0x10000000,
        SequentialScan = 0x8000000,
        WriteThrough = -2147483648
    }
}

