namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("5433376C-414D-11d3-B206-00C04FC2F3EF")]
    internal interface ITransactionVoterBallotAsync2
    {
        void VoteRequestDone(int hr, int reason);
    }
}

