namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("f9fd4090-93db-45c0-af87-624940f19cff")]
    internal interface IEnumSTORE_DEPLOYMENT_METADATA
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] IDefinitionAppId[] AppIds);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumSTORE_DEPLOYMENT_METADATA Clone();
    }
}

