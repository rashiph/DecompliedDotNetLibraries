namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LSA_UNICODE_STRING
    {
        public short Length;
        public short MaximumLength;
        public IntPtr Buffer;
    }
}

