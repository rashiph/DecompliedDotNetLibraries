namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("a75b74e9-2c00-4ebb-b3f9-62a670aaa07e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISecurityDescriptorReferenceEntry
    {
        System.Deployment.Internal.Isolation.Manifest.SecurityDescriptorReferenceEntry AllData { [SecurityCritical] get; }
        string Name { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string BuildFilter { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

