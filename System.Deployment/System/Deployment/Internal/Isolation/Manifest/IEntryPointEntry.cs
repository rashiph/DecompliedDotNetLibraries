namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1583EFE9-832F-4d08-B041-CAC5ACEDB948")]
    internal interface IEntryPointEntry
    {
        System.Deployment.Internal.Isolation.Manifest.EntryPointEntry AllData { [SecurityCritical] get; }
        string Name { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string CommandLine_File { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string CommandLine_Parameters { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.IReferenceIdentity Identity { [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
    }
}

