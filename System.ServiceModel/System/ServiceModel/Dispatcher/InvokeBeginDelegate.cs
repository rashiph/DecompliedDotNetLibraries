namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate IAsyncResult InvokeBeginDelegate(object target, object[] inputs, AsyncCallback asyncCallback, object state);
}

