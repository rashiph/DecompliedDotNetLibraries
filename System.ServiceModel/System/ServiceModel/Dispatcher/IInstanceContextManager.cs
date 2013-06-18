namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;

    internal interface IInstanceContextManager
    {
        void Abort();
        void Add(InstanceContext instanceContext);
        IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state);
        void Close(TimeSpan timeout);
        void CloseInput(TimeSpan timeout);
        void EndClose(IAsyncResult result);
        void EndCloseInput(IAsyncResult result);
        bool Remove(InstanceContext instanceContext);
        InstanceContext[] ToArray();
    }
}

