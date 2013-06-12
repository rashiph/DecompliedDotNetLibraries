namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("19be1967-b2fc-4dc1-9627-f3cb6305d2a7")]
    internal interface IEnumSTORE_CATEGORY_SUBCATEGORY
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.STORE_CATEGORY_SUBCATEGORY[] rgElements);
        [SecurityCritical]
        void Skip([In] uint ulElements);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumSTORE_CATEGORY_SUBCATEGORY Clone();
    }
}

