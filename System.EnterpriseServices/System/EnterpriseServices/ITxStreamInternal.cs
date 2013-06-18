namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("4E31107F-8E81-11d1-9DCE-00C04FC2FBA2")]
    internal interface ITxStreamInternal
    {
        void GetTransaction(out ITransaction ptx);
        [PreserveSig]
        Guid GetGuid();
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool TxIsDoomed();
    }
}

