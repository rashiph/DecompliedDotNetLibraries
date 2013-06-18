namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    internal interface IManualConcurrencyOperationInvoker : IOperationInvoker
    {
        object Invoke(object instance, object[] inputs, IInvokeReceivedNotification notification, out object[] outputs);
        IAsyncResult InvokeBegin(object instance, object[] inputs, IInvokeReceivedNotification notification, AsyncCallback callback, object state);

        bool OwnsFormatter { get; }
    }
}

