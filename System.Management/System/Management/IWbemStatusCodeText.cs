namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("EB87E1BC-3233-11D2-AEC9-00C04FB68820")]
    internal interface IWbemStatusCodeText
    {
        [PreserveSig]
        int GetErrorCodeText_([In, MarshalAs(UnmanagedType.Error)] int hRes, [In] uint LocaleId, [In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string MessageText);
        [PreserveSig]
        int GetFacilityCodeText_([In, MarshalAs(UnmanagedType.Error)] int hRes, [In] uint LocaleId, [In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string MessageText);
    }
}

