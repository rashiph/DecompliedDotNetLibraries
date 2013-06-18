namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate IAsyncResult OperationWithTimeoutBeginCallback(TimeSpan timeout, AsyncCallback asyncCallback, object asyncState);
}

