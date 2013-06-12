namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityPackageInfo
    {
        internal int Capabilities;
        internal short Version;
        internal short RPCID;
        internal int MaxToken;
        internal IntPtr Name;
        internal IntPtr Comment;
        internal static readonly int Size;
        internal static readonly int NameOffest;
        static SecurityPackageInfo()
        {
            Size = Marshal.SizeOf(typeof(SecurityPackageInfo));
            NameOffest = (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "Name");
        }
    }
}

