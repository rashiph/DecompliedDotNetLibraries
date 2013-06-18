namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7D40FCC8-F81E-462e-BBA1-8A99EBDC826C")]
    internal interface IContextTransactionInfo
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object FetchTransaction();
        void RegisterTransactionProxy([In, MarshalAs(UnmanagedType.Interface)] ITransactionProxy proxy, out Guid guid);
        void GetTxIsolationLevelAndTimeout(out DtcIsolationLevel isoLevel, out int timeout);
    }
}

