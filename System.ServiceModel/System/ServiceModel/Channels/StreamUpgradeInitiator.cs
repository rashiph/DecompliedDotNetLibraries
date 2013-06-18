namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;

    public abstract class StreamUpgradeInitiator
    {
        protected StreamUpgradeInitiator()
        {
        }

        internal virtual IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        public abstract IAsyncResult BeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state);
        internal virtual IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        internal virtual void Close(TimeSpan timeout)
        {
        }

        internal virtual void EndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public abstract Stream EndInitiateUpgrade(IAsyncResult result);
        internal virtual void EndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public abstract string GetNextUpgrade();
        public abstract Stream InitiateUpgrade(Stream stream);
        internal virtual void Open(TimeSpan timeout)
        {
        }
    }
}

