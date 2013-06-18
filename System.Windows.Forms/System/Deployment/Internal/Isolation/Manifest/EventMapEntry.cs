namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class EventMapEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string MapName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
        public uint Value;
        public bool IsValueMap;
    }
}

