namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IssuerListInfoEx
    {
        public SafeHandle aIssuers;
        public uint cIssuers;
        public unsafe IssuerListInfoEx(SafeHandle handle, byte[] nativeBuffer)
        {
            this.aIssuers = handle;
            fixed (byte* numRef = nativeBuffer)
            {
                this.cIssuers = numRef[IntPtr.Size];
            }
        }
    }
}

