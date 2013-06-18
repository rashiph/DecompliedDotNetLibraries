namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3C05E54B-A42A-11D2-AFC4-00C04F8EE1C4")]
    internal interface IContextState
    {
        void SetDeactivateOnReturn([In, MarshalAs(UnmanagedType.Bool)] bool bDeactivate);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetDeactivateOnReturn();
        void SetMyTransactionVote([In, MarshalAs(UnmanagedType.I4)] TransactionVote txVote);
        [return: MarshalAs(UnmanagedType.I4)]
        TransactionVote GetMyTransactionVote();
    }
}

