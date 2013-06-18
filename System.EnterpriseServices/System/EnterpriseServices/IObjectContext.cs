namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("51372AE0-CAE7-11CF-BE81-00AA00A2FA25"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IObjectContext
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object CreateInstance([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        void SetComplete();
        void SetAbort();
        void EnableCommit();
        void DisableCommit();
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool IsInTransaction();
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool IsSecurityEnabled();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsCallerInRole([In, MarshalAs(UnmanagedType.BStr)] string role);
    }
}

