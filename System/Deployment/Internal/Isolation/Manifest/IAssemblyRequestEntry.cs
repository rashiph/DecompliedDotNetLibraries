namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("2474ECB4-8EFD-4410-9F31-B3E7C4A07731"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAssemblyRequestEntry
    {
        System.Deployment.Internal.Isolation.Manifest.AssemblyRequestEntry AllData { [SecurityCritical] get; }
        string Name { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string permissionSetID { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

