namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class EventTagEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TagData;
        public uint EventID;
    }
}

