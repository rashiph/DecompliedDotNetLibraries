namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class DeploymentMetadataEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string DeploymentProviderCodebase;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string MinimumRequiredVersion;
        public ushort MaximumAge;
        public byte MaximumAge_Unit;
        public uint DeploymentFlags;
    }
}

