namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("C5FEB7C1-346A-11D1-B1CC-00AA00BA3258"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransactionResourcePool
    {
        [PreserveSig]
        int PutResource(IntPtr pPool, [MarshalAs(UnmanagedType.Interface)] object pUnk);
        [PreserveSig]
        int GetResource(IntPtr pPool, [MarshalAs(UnmanagedType.Interface)] out object obj);
    }
}

