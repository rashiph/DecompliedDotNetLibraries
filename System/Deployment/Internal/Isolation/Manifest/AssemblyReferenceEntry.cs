namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class AssemblyReferenceEntry
    {
        public System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity;
        public uint Flags;
        public System.Deployment.Internal.Isolation.Manifest.AssemblyReferenceDependentAssemblyEntry DependentAssembly;
    }
}

