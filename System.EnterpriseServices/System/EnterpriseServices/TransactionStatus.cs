namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(false)]
    public enum TransactionStatus
    {
        Commited,
        LocallyOk,
        NoTransaction,
        Aborting,
        Aborted
    }
}

