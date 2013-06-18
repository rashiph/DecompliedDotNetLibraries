namespace System.Messaging.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000109-0000-0000-C000-000000000046")]
    internal interface IPersistStream
    {
        [SuppressUnmanagedCodeSecurity]
        void GetClassID(out Guid pClassID);
        [SuppressUnmanagedCodeSecurity]
        int IsDirty();
        [SuppressUnmanagedCodeSecurity]
        void Load([In, MarshalAs(UnmanagedType.Interface)] IStream pstm);
        [SuppressUnmanagedCodeSecurity]
        void Save([In, MarshalAs(UnmanagedType.Interface)] IStream pstm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
        [SuppressUnmanagedCodeSecurity]
        long GetSizeMax();
    }
}

