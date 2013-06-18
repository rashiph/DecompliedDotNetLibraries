namespace System.Messaging.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPersistStreamInit
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
        [SuppressUnmanagedCodeSecurity]
        void InitNew();
    }
}

