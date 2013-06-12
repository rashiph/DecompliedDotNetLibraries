namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class SubcategoryMembershipEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Subcategory;
        public ISection CategoryMembershipData;
    }
}

