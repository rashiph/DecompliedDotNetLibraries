namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x200), Guid("580ACAF8-FA1C-11D0-AD72-00C04FD8FDFF"), InterfaceType((short) 1)]
    internal interface IWbemEventProviderQuerySink
    {
        [PreserveSig]
        int NewQuery_([In] uint dwId, [In, MarshalAs(UnmanagedType.LPWStr)] string wszQueryLanguage, [In, MarshalAs(UnmanagedType.LPWStr)] string wszQuery);
        [PreserveSig]
        int CancelQuery_([In] uint dwId);
    }
}

