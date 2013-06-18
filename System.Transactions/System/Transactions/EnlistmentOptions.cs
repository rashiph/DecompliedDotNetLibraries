namespace System.Transactions
{
    using System;

    [Flags]
    public enum EnlistmentOptions
    {
        None,
        EnlistDuringPrepareRequired
    }
}

