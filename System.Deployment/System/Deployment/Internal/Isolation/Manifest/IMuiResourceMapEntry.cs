namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("397927f5-10f2-4ecb-bfe1-3c264212a193")]
    internal interface IMuiResourceMapEntry
    {
        System.Deployment.Internal.Isolation.Manifest.MuiResourceMapEntry AllData { [SecurityCritical] get; }
        object ResourceTypeIdInt { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        object ResourceTypeIdString { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
    }
}

