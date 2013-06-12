namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class TransmitFileBuffers
    {
        internal IntPtr preBuffer;
        internal int preBufferLength;
        internal IntPtr postBuffer;
        internal int postBufferLength;
    }
}

