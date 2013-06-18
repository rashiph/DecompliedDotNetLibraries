namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("7C857801-7381-11CF-884D-00AA004B2E24"), InterfaceType((short) 1), SuppressUnmanagedCodeSecurity, TypeLibType((short) 0x200)]
    internal interface IWbemObjectSink
    {
        [PreserveSig, SuppressUnmanagedCodeSecurity]
        int Indicate_([In] int lObjectCount, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apObjArray);
        [PreserveSig]
        int SetStatus_([In] int lFlags, [In, MarshalAs(UnmanagedType.Error)] int hResult, [In, MarshalAs(UnmanagedType.BStr)] string strParam, [In] IntPtr pObjParam);
    }
}

