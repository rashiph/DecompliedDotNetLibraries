namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("70A4ECEE-B195-4c59-85BF-44B6ACA83F07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IResourceTableMappingEntry
    {
        System.Deployment.Internal.Isolation.Manifest.ResourceTableMappingEntry AllData { [SecurityCritical] get; }
        string id { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string FinalStringMapped { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

