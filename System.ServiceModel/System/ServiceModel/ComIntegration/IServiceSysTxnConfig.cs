namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("33CAF1A1-FCB8-472b-B45E-967448DED6D8")]
    internal interface IServiceSysTxnConfig
    {
        void ConfigureTransaction(TransactionConfig transactionConfig);
        void IsolationLevel(int option);
        void TransactionTimeout(uint ulTimeoutSec);
        void BringYourOwnTransaction([MarshalAs(UnmanagedType.LPWStr)] string szTipURL);
        void NewTransactionDescription([MarshalAs(UnmanagedType.LPWStr)] string szTxDesc);
        void ConfigureBYOT(IntPtr pITxByot);
        void ConfigureBYOTSysTxn(IntPtr pITxByot);
    }
}

