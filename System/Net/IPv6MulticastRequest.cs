namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IPv6MulticastRequest
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal byte[] MulticastAddress;
        internal int InterfaceIndex;
        internal static readonly int Size;
        static IPv6MulticastRequest()
        {
            Size = Marshal.SizeOf(typeof(IPv6MulticastRequest));
        }
    }
}

