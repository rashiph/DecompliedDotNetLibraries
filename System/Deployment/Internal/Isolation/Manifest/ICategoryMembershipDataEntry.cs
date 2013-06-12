namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("DA0C3B27-6B6B-4b80-A8F8-6CE14F4BC0A4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICategoryMembershipDataEntry
    {
        System.Deployment.Internal.Isolation.Manifest.CategoryMembershipDataEntry AllData { [SecurityCritical] get; }
        uint index { [SecurityCritical] get; }
        string Xml { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Description { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

