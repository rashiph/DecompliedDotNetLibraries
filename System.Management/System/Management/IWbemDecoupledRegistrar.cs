namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("1005CBCF-E64F-4646-BCD3-3A089D8A84B4")]
    internal interface IWbemDecoupledRegistrar
    {
        [PreserveSig]
        int Register_([In] int flags, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext context, [In, MarshalAs(UnmanagedType.LPWStr)] string user, [In, MarshalAs(UnmanagedType.LPWStr)] string locale, [In, MarshalAs(UnmanagedType.LPWStr)] string scope, [In, MarshalAs(UnmanagedType.LPWStr)] string registration, [In, MarshalAs(UnmanagedType.IUnknown)] object unknown);
        [PreserveSig]
        int UnRegister_();
    }
}

