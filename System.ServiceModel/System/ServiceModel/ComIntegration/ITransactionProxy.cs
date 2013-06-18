namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Transactions;

    [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("02558374-DF2E-4dae-BD6B-1D5C994F9BDC")]
    internal interface ITransactionProxy
    {
        void Commit(Guid guid);
        void Abort();
        [return: MarshalAs(UnmanagedType.Interface)]
        IDtcTransaction Promote();
        void CreateVoter([MarshalAs(UnmanagedType.Interface)] ITransactionVoterNotifyAsync2 voterNotification, IntPtr voterBallot);
        DtcIsolationLevel GetIsolationLevel();
        Guid GetIdentifier();
        bool IsReusable();
    }
}

