namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("55FF6514-948A-4307-A692-73B84E2AF53E"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPhase0EnlistmentShim
    {
        void Unenlist();
        void Phase0Done([MarshalAs(UnmanagedType.Bool)] bool voteYes);
    }
}

