namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class ProgIdRedirectionEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ProgId;
        public Guid RedirectedGuid;
    }
}

