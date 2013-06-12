namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a5c6aaa3-03e4-478d-b9f5-2e45908d5e4f")]
    internal interface IEnumSTORE_ASSEMBLY_FILE
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.STORE_ASSEMBLY_FILE[] rgelt);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumSTORE_ASSEMBLY_FILE Clone();
    }
}

