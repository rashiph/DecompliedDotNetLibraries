namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0fb15084-af41-11ce-bd2b-204c4f4f5020"), SuppressUnmanagedCodeSecurity]
    internal interface ITransactionNativeInternal
    {
        void Commit(int retaining, [MarshalAs(UnmanagedType.I4)] OletxXacttc commitType, int reserved);
        void Abort(IntPtr reason, int retaining, int async);
        void GetTransactionInfo(out OletxXactTransInfo xactInfo);
    }
}

