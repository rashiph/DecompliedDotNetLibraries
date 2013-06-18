namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate IAsyncResult ChainedBeginHandler(TimeSpan timeout, AsyncCallback asyncCallback, object state);
}

