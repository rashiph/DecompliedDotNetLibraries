namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("51372AEC-CAE7-11CF-BE81-00AA00A2FA25")]
    internal interface IObjectControl
    {
        void Activate();
        void Deactivate();
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool CanBePooled();
    }
}

