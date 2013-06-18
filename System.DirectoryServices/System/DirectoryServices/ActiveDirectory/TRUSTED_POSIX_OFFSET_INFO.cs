namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class TRUSTED_POSIX_OFFSET_INFO
    {
        internal int Offset;
    }
}

