namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("EBE5A1ED-FEBC-42c4-A9E1-E087C6E36635"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPermissionSetEntry
    {
        PermissionSetEntry AllData { [SecurityCritical] get; }
        string Id { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string XmlSegment { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

