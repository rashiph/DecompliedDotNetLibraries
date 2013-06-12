namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("b30352cf-23da-4577-9b3f-b4e6573be53b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumReferenceIdentity
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.IReferenceIdentity[] ReferenceIdentity);
        [SecurityCritical]
        void Skip(uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumReferenceIdentity Clone();
    }
}

