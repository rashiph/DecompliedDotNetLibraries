namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class CompatibleFrameworksMetadataEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string SupportUrl;
    }
}

