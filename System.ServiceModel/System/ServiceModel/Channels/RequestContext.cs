namespace System.ServiceModel.Channels
{
    using System;

    public abstract class RequestContext : IDisposable
    {
        protected RequestContext()
        {
        }

        public abstract void Abort();
        public abstract IAsyncResult BeginReply(Message message, AsyncCallback callback, object state);
        public abstract IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        public abstract void Close();
        public abstract void Close(TimeSpan timeout);
        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract void EndReply(IAsyncResult result);
        public abstract void Reply(Message message);
        public abstract void Reply(Message message, TimeSpan timeout);
        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        public abstract Message RequestMessage { get; }
    }
}

