namespace System.Messaging.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("0FB15084-AF41-11CE-BD2B-204C4F4F5020"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransaction
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int Commit([In, MarshalAs(UnmanagedType.I4)] int fRetaining, [In, MarshalAs(UnmanagedType.U4)] int grfTC, [In, MarshalAs(UnmanagedType.U4)] int grfRM);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int Abort([In, MarshalAs(UnmanagedType.U4)] int pboidReason, [In, MarshalAs(UnmanagedType.I4)] int fRetaining, [In, MarshalAs(UnmanagedType.I4)] int fAsync);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int GetTransactionInfo([In, Out] IntPtr pinfo);
    }
}

