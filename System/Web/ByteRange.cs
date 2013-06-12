namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ByteRange
    {
        internal long Offset;
        internal long Length;
    }
}

