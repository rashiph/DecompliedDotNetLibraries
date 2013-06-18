namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("5433376C-414D-11d3-B206-00C04FC2F3EF"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransactionVoterBallotAsync2
    {
        void VoteRequestDone(int hr, int reason);
    }
}

