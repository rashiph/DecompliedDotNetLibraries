namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class EventEntry
    {
        public uint EventID;
        public uint Level;
        public uint Version;
        public System.Guid Guid;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string SubTypeName;
        public uint SubTypeValue;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string DisplayName;
        public uint EventNameMicrodomIndex;
    }
}

