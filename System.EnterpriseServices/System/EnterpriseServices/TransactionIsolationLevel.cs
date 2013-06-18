namespace System.EnterpriseServices
{
    using System;

    [Serializable]
    public enum TransactionIsolationLevel
    {
        Any,
        ReadUncommitted,
        ReadCommitted,
        RepeatableRead,
        Serializable
    }
}

