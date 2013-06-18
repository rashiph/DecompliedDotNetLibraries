namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;

    [Serializable]
    public enum TransactionState
    {
        Active,
        Committed,
        Aborted,
        Indoubt
    }
}

