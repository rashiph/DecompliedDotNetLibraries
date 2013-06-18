namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(false)]
    internal enum TransactionConfig
    {
        NoTransaction,
        IfContainerIsTransactional,
        CreateTransactionIfNecessary,
        NewTransaction
    }
}

