namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IPMulticastRequest
    {
        internal int MulticastAddress;
        internal int InterfaceAddress;
        internal static readonly int Size;
        static IPMulticastRequest()
        {
            Size = Marshal.SizeOf(typeof(IPMulticastRequest));
        }
    }
}

