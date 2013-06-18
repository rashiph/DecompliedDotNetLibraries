namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("27C73B91-99F5-46d5-A247-732A1A16529E"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IResourceManagerShim
    {
        void Enlist([MarshalAs(UnmanagedType.Interface)] ITransactionShim transactionShim, IntPtr managedIdentifier, [MarshalAs(UnmanagedType.Interface)] out IEnlistmentShim enlistmentShim);
        void Reenlist([MarshalAs(UnmanagedType.U4)] uint prepareInfoSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] prepareInfo, out OletxTransactionOutcome outcome);
        void ReenlistComplete();
    }
}

