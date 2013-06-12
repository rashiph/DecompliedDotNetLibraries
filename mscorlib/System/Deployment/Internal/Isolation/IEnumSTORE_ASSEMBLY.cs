namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("a5c637bf-6eaa-4e5f-b535-55299657e33e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumSTORE_ASSEMBLY
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] STORE_ASSEMBLY[] rgelt);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_ASSEMBLY Clone();
    }
}

