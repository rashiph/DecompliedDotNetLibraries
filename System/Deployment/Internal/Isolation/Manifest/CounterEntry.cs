namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class CounterEntry
    {
        public Guid CounterSetGuid;
        public uint CounterId;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Description;
        public uint CounterType;
        public ulong Attributes;
        public uint BaseId;
        public uint DefaultScale;
    }
}

