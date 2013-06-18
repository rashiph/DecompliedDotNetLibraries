namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;

    internal enum ControlProtocol
    {
        None,
        Completion,
        Volatile2PC,
        Durable2PC
    }
}

