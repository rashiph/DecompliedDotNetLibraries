namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Blob
    {
        public int cbSize;
        public int pBlobData;
    }
}

