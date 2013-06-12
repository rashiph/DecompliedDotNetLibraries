namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class AssemblyReferenceEntry
    {
        public IReferenceIdentity ReferenceIdentity;
        public uint Flags;
        public AssemblyReferenceDependentAssemblyEntry DependentAssembly;
    }
}

