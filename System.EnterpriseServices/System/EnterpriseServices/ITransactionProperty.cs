namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("788ea814-87b1-11d1-bba6-00c04fc2fa5f")]
    internal interface ITransactionProperty
    {
        [PreserveSig]
        void SetConsistent(bool fConsistent);
        void GetTransaction(out ITransaction ptx);
        [PreserveSig]
        void GetTxStream(out ITxStreamInternal ptsi);
        [PreserveSig]
        Guid GetTxStreamGuid();
        [PreserveSig]
        int GetTxStreamMarshalSize();
        [PreserveSig]
        int GetTxStreamMarshalBuffer();
        [PreserveSig]
        short GetUnmarshalVariant();
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool NeedEnvoy();
        [PreserveSig]
        short GetRootDtcCapabilities();
        [PreserveSig]
        int GetTransactionResourcePool(out ITransactionResourcePool pool);
        void GetTransactionId(ref Guid guid);
        object GetClassInfo();
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool IsRoot();
    }
}

