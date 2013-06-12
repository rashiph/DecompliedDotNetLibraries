namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("11df5cad-c183-479b-9a44-3842b71639ce")]
    internal interface IMuiResourceTypeIdStringEntry
    {
        System.Deployment.Internal.Isolation.Manifest.MuiResourceTypeIdStringEntry AllData { [SecurityCritical] get; }
        object StringIds { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        object IntegerIds { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
    }
}

