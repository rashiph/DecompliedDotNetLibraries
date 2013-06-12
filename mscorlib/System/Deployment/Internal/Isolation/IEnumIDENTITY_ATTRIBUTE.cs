namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("9cdaae75-246e-4b00-a26d-b9aec137a3eb"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumIDENTITY_ATTRIBUTE
    {
        [SecurityCritical]
        uint Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray)] IDENTITY_ATTRIBUTE[] rgAttributes);
        [SecurityCritical]
        IntPtr CurrentIntoBuffer([In] IntPtr Available, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] Data);
        [SecurityCritical]
        void Skip([In] uint celt);
        [SecurityCritical]
        void Reset();
        [SecurityCritical]
        IEnumIDENTITY_ATTRIBUTE Clone();
    }
}

