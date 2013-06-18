namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LARGE_INTEGER
    {
        public int lowPart = 0;
        public int highPart = 0;
    }
}

