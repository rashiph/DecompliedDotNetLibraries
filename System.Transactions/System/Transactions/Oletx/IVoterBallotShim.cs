namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("A5FAB903-21CB-49eb-93AE-EF72CD45169E")]
    internal interface IVoterBallotShim
    {
        void Vote([MarshalAs(UnmanagedType.Bool)] bool voteYes);
    }
}

