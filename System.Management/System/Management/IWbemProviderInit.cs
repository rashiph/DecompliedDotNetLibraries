namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("1BE41572-91DD-11D1-AEB2-00C04FB68820"), InterfaceType((short) 1)]
    internal interface IWbemProviderInit
    {
        [PreserveSig]
        int Initialize_([In, MarshalAs(UnmanagedType.LPWStr)] string wszUser, [In] int lFlags, [In, MarshalAs(UnmanagedType.LPWStr)] string wszNamespace, [In, MarshalAs(UnmanagedType.LPWStr)] string wszLocale, [In, MarshalAs(UnmanagedType.Interface)] IWbemServices pNamespace, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [In, MarshalAs(UnmanagedType.Interface)] IWbemProviderInitSink pInitSink);
    }
}

