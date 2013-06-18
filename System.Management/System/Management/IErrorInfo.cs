namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("1CF2B120-547D-101B-8E65-08002B2BD119"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IErrorInfo
    {
        Guid GetGUID();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetSource();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetDescription();
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetHelpFile();
        uint GetHelpContext();
    }
}

