namespace System.Transactions
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("0fb15084-af41-11ce-bd2b-204c4f4f5020"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDtcTransaction
    {
        void Commit(int retaining, [MarshalAs(UnmanagedType.I4)] int commitType, int reserved);
        void Abort(IntPtr reason, int retaining, int async);
        void GetTransactionInfo(IntPtr transactionInformation);
    }
}

