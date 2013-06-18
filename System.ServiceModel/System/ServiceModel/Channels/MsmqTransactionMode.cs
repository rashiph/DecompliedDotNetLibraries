namespace System.ServiceModel.Channels
{
    using System;

    internal enum MsmqTransactionMode
    {
        None,
        Single,
        CurrentOrSingle,
        CurrentOrNone,
        CurrentOrThrow
    }
}

