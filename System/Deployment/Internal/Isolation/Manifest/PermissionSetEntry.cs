namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class PermissionSetEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Id;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string XmlSegment;
    }
}

