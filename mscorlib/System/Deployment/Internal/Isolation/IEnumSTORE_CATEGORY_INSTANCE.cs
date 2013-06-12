namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("5ba7cb30-8508-4114-8c77-262fcda4fadb")]
    internal interface IEnumSTORE_CATEGORY_INSTANCE
    {
        [SecurityCritical]
        uint Next([In] uint ulElements, [Out, MarshalAs(UnmanagedType.LPArray)] STORE_CATEGORY_INSTANCE[] rgInstances);
        [SecurityCritical]
        void Skip([In] uint ulElements);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_CATEGORY_INSTANCE Clone();
    }
}

