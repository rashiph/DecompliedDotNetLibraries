namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9cdaae75-246e-4b00-a26d-b9aec137a3eb")]
    internal interface IEnumIDENTITY_ATTRIBUTE
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[] rgAttributes);
        [SecurityCritical]
        IntPtr CurrentIntoBuffer([In] IntPtr Available, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] Data);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumIDENTITY_ATTRIBUTE Clone();
    }
}

