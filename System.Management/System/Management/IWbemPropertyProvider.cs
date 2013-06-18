namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), TypeLibType((short) 0x200), Guid("CE61E841-65BC-11D0-B6BD-00AA003240C7")]
    internal interface IWbemPropertyProvider
    {
        [PreserveSig]
        int GetProperty_([In] int lFlags, [In, MarshalAs(UnmanagedType.BStr)] string strLocale, [In, MarshalAs(UnmanagedType.BStr)] string strClassMapping, [In, MarshalAs(UnmanagedType.BStr)] string strInstMapping, [In, MarshalAs(UnmanagedType.BStr)] string strPropMapping, out object pvValue);
        [PreserveSig]
        int PutProperty_([In] int lFlags, [In, MarshalAs(UnmanagedType.BStr)] string strLocale, [In, MarshalAs(UnmanagedType.BStr)] string strClassMapping, [In, MarshalAs(UnmanagedType.BStr)] string strInstMapping, [In, MarshalAs(UnmanagedType.BStr)] string strPropMapping, [In] ref object pvValue);
    }
}

