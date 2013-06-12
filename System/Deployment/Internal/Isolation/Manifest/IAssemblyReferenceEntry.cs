namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("FD47B733-AFBC-45e4-B7C2-BBEB1D9F766C")]
    internal interface IAssemblyReferenceEntry
    {
        System.Deployment.Internal.Isolation.Manifest.AssemblyReferenceEntry AllData { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity { [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.Manifest.IAssemblyReferenceDependentAssemblyEntry DependentAssembly { [SecurityCritical] get; }
    }
}

