namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("5433376B-414D-11d3-B206-00C04FC2F3EF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    internal interface ITransactionVoterNotifyAsync2
    {
        void Committed([MarshalAs(UnmanagedType.Bool)] bool retaining, int newUow, int hr);
        void Aborted(int reason, [MarshalAs(UnmanagedType.Bool)] bool retaining, int newUow, int hr);
        void HeuristicDecision(int decision, int reason, int hr);
        void InDoubt();
        void VoteRequest();
    }
}

