namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x200), InterfaceType((short) 1), Guid("DC12A687-737F-11CF-884D-00AA004B2E24")]
    internal interface IWbemLocator
    {
        [PreserveSig]
        int ConnectServer_([In, MarshalAs(UnmanagedType.BStr)] string strNetworkResource, [In, MarshalAs(UnmanagedType.BStr)] string strUser, [In] IntPtr strPassword, [In, MarshalAs(UnmanagedType.BStr)] string strLocale, [In] int lSecurityFlags, [In, MarshalAs(UnmanagedType.BStr)] string strAuthority, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx, [MarshalAs(UnmanagedType.Interface)] out IWbemServices ppNamespace);
    }
}

