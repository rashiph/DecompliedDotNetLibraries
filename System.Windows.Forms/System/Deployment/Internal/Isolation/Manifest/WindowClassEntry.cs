namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class WindowClassEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ClassName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string HostDll;
        public bool fVersioned;
    }
}

