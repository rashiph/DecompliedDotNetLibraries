namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreTransactionOperation
    {
        [MarshalAs(UnmanagedType.U4)]
        public StoreTransactionOperationType Operation;
        public StoreTransactionData Data;
    }
}

