namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9f27c750-7dfb-46a1-a673-52e53e2337a9")]
    internal interface IDirectoryEntry
    {
        System.Deployment.Internal.Isolation.Manifest.DirectoryEntry AllData { [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
        uint Protection { [SecurityCritical] get; }
        string BuildFilter { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        object SecurityDescriptor { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
    }
}

