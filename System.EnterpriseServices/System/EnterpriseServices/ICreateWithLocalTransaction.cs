namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("227AC7A8-8423-42ce-B7CF-03061EC9AAA3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICreateWithLocalTransaction
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object CreateInstanceWithSysTx(ITransactionProxy pTransaction, [In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    }
}

