namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("C31FF59E-CD25-47b8-9EF3-CF4433EB97CC")]
    internal interface IAssemblyReferenceDependentAssemblyEntry
    {
        AssemblyReferenceDependentAssemblyEntry AllData { [SecurityCritical] get; }
        string Group { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Codebase { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        ulong Size { [SecurityCritical] get; }
        object HashValue { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        uint HashAlgorithm { [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
        string ResourceFallbackCulture { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Description { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SupportUrl { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        ISection HashElements { [SecurityCritical] get; }
    }
}

