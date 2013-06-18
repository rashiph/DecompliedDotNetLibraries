namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Transactions;

    [ComImport, Guid("02558374-DF2E-4dae-BD6B-1D5C994F9BDC"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransactionProxy
    {
        void Commit(Guid guid);
        void Abort();
        [return: MarshalAs(UnmanagedType.Interface)]
        IDtcTransaction Promote();
        void CreateVoter([MarshalAs(UnmanagedType.Interface)] ITransactionVoterNotifyAsync2 voterNotification, [MarshalAs(UnmanagedType.Interface)] out ITransactionVoterBallotAsync2 voterBallot);
        DtcIsolationLevel GetIsolationLevel();
        Guid GetIdentifier();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsReusable();
    }
}

