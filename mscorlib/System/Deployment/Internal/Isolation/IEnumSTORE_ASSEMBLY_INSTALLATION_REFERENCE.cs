namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("d8b1aacb-5142-4abb-bcc1-e9dc9052a89e")]
    internal interface IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] StoreApplicationReference[] rgelt);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE Clone();
    }
}

