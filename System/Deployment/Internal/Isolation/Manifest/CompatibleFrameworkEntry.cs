namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class CompatibleFrameworkEntry
    {
        public uint index;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetVersion;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Profile;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string SupportedRuntime;
    }
}

