namespace System.ServiceModel
{
    using System;

    public interface ICommunicationObject
    {
        event EventHandler Closed;

        event EventHandler Closing;

        event EventHandler Faulted;

        event EventHandler Opened;

        event EventHandler Opening;

        void Abort();
        IAsyncResult BeginClose(AsyncCallback callback, object state);
        IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginOpen(AsyncCallback callback, object state);
        IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        void Close();
        void Close(TimeSpan timeout);
        void EndClose(IAsyncResult result);
        void EndOpen(IAsyncResult result);
        void Open();
        void Open(TimeSpan timeout);

        CommunicationState State { get; }
    }
}

