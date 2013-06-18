namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_NAME_RESULT
    {
        public int cItems;
        public IntPtr rItems;
    }
}

