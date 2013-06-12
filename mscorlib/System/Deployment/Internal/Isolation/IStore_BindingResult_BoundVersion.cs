namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IStore_BindingResult_BoundVersion
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort Revision;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Build;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Minor;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Major;
    }
}

