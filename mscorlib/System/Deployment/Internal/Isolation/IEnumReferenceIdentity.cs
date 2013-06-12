namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("b30352cf-23da-4577-9b3f-b4e6573be53b")]
    internal interface IEnumReferenceIdentity
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] IReferenceIdentity[] ReferenceIdentity);
        [SecurityCritical]
        void Skip(uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumReferenceIdentity Clone();
    }
}

