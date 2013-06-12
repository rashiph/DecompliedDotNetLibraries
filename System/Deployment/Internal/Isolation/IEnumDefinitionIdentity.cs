namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("f3549d9c-fc73-4793-9c00-1cd204254c0c")]
    internal interface IEnumDefinitionIdentity
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.IDefinitionIdentity[] DefinitionIdentity);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumDefinitionIdentity Clone();
    }
}

