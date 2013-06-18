namespace System.ServiceModel.Dispatcher
{
    using System;

    internal enum ExclusiveInstanceContextTransactionResult
    {
        Acquired,
        Wait,
        Fault
    }
}

