namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), TypeLibType((short) 0x200), Guid("44ACA674-E8FC-11D0-A07C-00C04FB68820")]
    internal interface IWbemContext
    {
        [PreserveSig]
        int Clone_([MarshalAs(UnmanagedType.Interface)] out IWbemContext ppNewCopy);
        [PreserveSig]
        int GetNames_([In] int lFlags, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);
        [PreserveSig]
        int BeginEnumeration_([In] int lFlags);
        [PreserveSig]
        int Next_([In] int lFlags, [MarshalAs(UnmanagedType.BStr)] out string pstrName, out object pValue);
        [PreserveSig]
        int EndEnumeration_();
        [PreserveSig]
        int SetValue_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In] ref object pValue);
        [PreserveSig]
        int GetValue_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, out object pValue);
        [PreserveSig]
        int DeleteValue_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags);
        [PreserveSig]
        int DeleteAll_();
    }
}

