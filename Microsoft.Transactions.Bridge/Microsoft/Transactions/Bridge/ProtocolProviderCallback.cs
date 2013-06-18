namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void ProtocolProviderCallback(Enlistment enlistment, Status status, object state);
}

