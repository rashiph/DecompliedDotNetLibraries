namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal class MetadataSectionEntry : IDisposable
    {
        public uint SchemaVersion;
        public uint ManifestFlags;
        public uint UsagePatterns;
        public System.Deployment.Internal.Isolation.IDefinitionIdentity CdfIdentity;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string LocalPath;
        public uint HashAlgorithm;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr ManifestHash;
        public uint ManifestHashSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ContentType;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string RuntimeImageVersion;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr MvidValue;
        public uint MvidValueSize;
        public System.Deployment.Internal.Isolation.Manifest.DescriptionMetadataEntry DescriptionData;
        public System.Deployment.Internal.Isolation.Manifest.DeploymentMetadataEntry DeploymentData;
        public System.Deployment.Internal.Isolation.Manifest.DependentOSMetadataEntry DependentOSData;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string defaultPermissionSetID;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string RequestedExecutionLevel;
        public bool RequestedExecutionLevelUIAccess;
        public System.Deployment.Internal.Isolation.IReferenceIdentity ResourceTypeResourcesDependency;
        public System.Deployment.Internal.Isolation.IReferenceIdentity ResourceTypeManifestResourcesDependency;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string KeyInfoElement;
        public System.Deployment.Internal.Isolation.Manifest.CompatibleFrameworksMetadataEntry CompatibleFrameworksData;
        ~MetadataSectionEntry()
        {
            this.Dispose(false);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        [SecuritySafeCritical]
        public void Dispose(bool fDisposing)
        {
            if (this.ManifestHash != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.ManifestHash);
                this.ManifestHash = IntPtr.Zero;
            }
            if (this.MvidValue != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.MvidValue);
                this.MvidValue = IntPtr.Zero;
            }
            if (fDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}

