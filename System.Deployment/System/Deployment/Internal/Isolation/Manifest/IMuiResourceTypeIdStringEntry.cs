namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("11df5cad-c183-479b-9a44-3842b71639ce"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMuiResourceTypeIdStringEntry
    {
        System.Deployment.Internal.Isolation.Manifest.MuiResourceTypeIdStringEntry AllData { [SecurityCritical] get; }
        object StringIds { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        object IntegerIds { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
    }
}

