namespace System.EnterpriseServices
{
    using System;

    [Serializable]
    public enum TransactionOption
    {
        Disabled,
        NotSupported,
        Supported,
        Required,
        RequiresNew
    }
}

