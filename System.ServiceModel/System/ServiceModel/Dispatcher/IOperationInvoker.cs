namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    public interface IOperationInvoker
    {
        object[] AllocateInputs();
        object Invoke(object instance, object[] inputs, out object[] outputs);
        IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state);
        object InvokeEnd(object instance, out object[] outputs, IAsyncResult result);

        bool IsSynchronous { get; }
    }
}

