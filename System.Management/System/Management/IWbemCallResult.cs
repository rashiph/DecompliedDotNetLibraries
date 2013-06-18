namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("44ACA675-E8FC-11D0-A07C-00C04FB68820"), TypeLibType((short) 0x200)]
    internal interface IWbemCallResult
    {
        [PreserveSig]
        int GetResultObject_([In] int lTimeout, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(MarshalWbemObject), MarshalCookie="")] out IWbemClassObjectFreeThreaded ppResultObject);
        [PreserveSig]
        int GetResultString_([In] int lTimeout, [MarshalAs(UnmanagedType.BStr)] out string pstrResultString);
        [PreserveSig]
        int GetResultServices_([In] int lTimeout, [MarshalAs(UnmanagedType.Interface)] out IWbemServices ppServices);
        [PreserveSig]
        int GetCallStatus_([In] int lTimeout, out int plStatus);
    }
}

