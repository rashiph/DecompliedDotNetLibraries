namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DsNameResult
    {
        public int itemCount;
        public IntPtr items;
    }
}

