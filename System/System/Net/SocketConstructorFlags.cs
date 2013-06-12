namespace System.Net
{
    using System;

    [Flags]
    internal enum SocketConstructorFlags
    {
        WSA_FLAG_MULTIPOINT_C_LEAF = 4,
        WSA_FLAG_MULTIPOINT_C_ROOT = 2,
        WSA_FLAG_MULTIPOINT_D_LEAF = 0x10,
        WSA_FLAG_MULTIPOINT_D_ROOT = 8,
        WSA_FLAG_OVERLAPPED = 1
    }
}

