namespace System.ServiceModel.Channels
{
    using System;

    public interface IDuplexSession : IInputSession, IOutputSession, ISession
    {
        IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state);
        IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state);
        void CloseOutputSession();
        void CloseOutputSession(TimeSpan timeout);
        void EndCloseOutputSession(IAsyncResult result);
    }
}

