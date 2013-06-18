namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("49e1fe8d-ebb8-4593-8c4e-3e14c845b142"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IRegistryValueEntry
    {
        System.Deployment.Internal.Isolation.Manifest.RegistryValueEntry AllData { [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
        uint OperationHint { [SecurityCritical] get; }
        uint Type { [SecurityCritical] get; }
        string Value { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string BuildFilter { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

