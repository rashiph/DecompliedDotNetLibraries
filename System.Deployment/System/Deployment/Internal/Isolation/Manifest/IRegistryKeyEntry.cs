namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("186685d1-6673-48c3-bc83-95859bb591df")]
    internal interface IRegistryKeyEntry
    {
        System.Deployment.Internal.Isolation.Manifest.RegistryKeyEntry AllData { [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
        uint Protection { [SecurityCritical] get; }
        string BuildFilter { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        object SecurityDescriptor { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        object Values { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        object Keys { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
    }
}

