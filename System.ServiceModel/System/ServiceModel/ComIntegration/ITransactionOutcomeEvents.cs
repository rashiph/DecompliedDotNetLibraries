namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("3A6AD9E2-23B9-11cf-AD60-00AA00A74CCD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    internal interface ITransactionOutcomeEvents
    {
        void Committed([MarshalAs(UnmanagedType.Bool)] bool retaining, int newUow, int hr);
        void Aborted(int reason, [MarshalAs(UnmanagedType.Bool)] bool retaining, int newUow, int hr);
        void HeuristicDecision(int decision, int reason, int hr);
        void InDoubt();
    }
}

