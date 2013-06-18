namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x200), InterfaceType((short) 1), Guid("DC12A680-737F-11CF-884D-00AA004B2E24")]
    internal interface IWbemQualifierSet_DoNotMarshal
    {
        [PreserveSig]
        int Get_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] int lFlags, [In, Out] ref object pVal, [In, Out] ref int plFlavor);
        [PreserveSig]
        int Put_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] ref object pVal, [In] int lFlavor);
        [PreserveSig]
        int Delete_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName);
        [PreserveSig]
        int GetNames_([In] int lFlags, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_BSTR)] out string[] pNames);
        [PreserveSig]
        int BeginEnumeration_([In] int lFlags);
        [PreserveSig]
        int Next_([In] int lFlags, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string pstrName, [In, Out] ref object pVal, [In, Out] ref int plFlavor);
        [PreserveSig]
        int EndEnumeration_();
    }
}

