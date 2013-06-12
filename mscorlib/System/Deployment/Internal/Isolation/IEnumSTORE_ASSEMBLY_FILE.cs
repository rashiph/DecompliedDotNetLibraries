namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("a5c6aaa3-03e4-478d-b9f5-2e45908d5e4f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumSTORE_ASSEMBLY_FILE
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] STORE_ASSEMBLY_FILE[] rgelt);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_ASSEMBLY_FILE Clone();
    }
}

