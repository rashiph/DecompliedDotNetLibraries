namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IStore_BindingResult
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Flags;
        [MarshalAs(UnmanagedType.U4)]
        public uint Disposition;
        public System.Deployment.Internal.Isolation.IStore_BindingResult_BoundVersion Component;
        public Guid CacheCoherencyGuid;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr Reserved;
    }
}

