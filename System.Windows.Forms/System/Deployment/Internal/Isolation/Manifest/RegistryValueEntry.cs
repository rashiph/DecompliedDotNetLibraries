namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class RegistryValueEntry
    {
        public uint Flags;
        public uint OperationHint;
        public uint Type;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string BuildFilter;
    }
}

