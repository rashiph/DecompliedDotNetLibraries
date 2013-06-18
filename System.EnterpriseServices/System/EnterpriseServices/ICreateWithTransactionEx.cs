namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("455ACF57-5345-11D2-99CF-00C04F797BC9")]
    internal interface ICreateWithTransactionEx
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object CreateInstance(ITransaction pTransaction, [In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    }
}

