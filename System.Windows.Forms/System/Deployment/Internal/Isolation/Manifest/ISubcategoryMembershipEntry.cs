namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("5A7A54D7-5AD5-418e-AB7A-CF823A8D48D0")]
    internal interface ISubcategoryMembershipEntry
    {
        System.Deployment.Internal.Isolation.Manifest.SubcategoryMembershipEntry AllData { [SecurityCritical] get; }
        string Subcategory { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection CategoryMembershipData { [SecurityCritical] get; }
    }
}

