namespace System.ServiceModel.Channels
{
    using System;

    internal class BufferedConnectionListener : IConnectionListener, IDisposable
    {
        private IConnectionListener connectionListener;
        private TimeSpan flushTimeout;
        private int writeBufferSize;

        public BufferedConnectionListener(IConnectionListener connectionListener, TimeSpan flushTimeout, int writeBufferSize)
        {
            this.connectionListener = connectionListener;
            this.flushTimeout = flushTimeout;
            this.writeBufferSize = writeBufferSize;
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return this.connectionListener.BeginAccept(callback, state);
        }

        public void Dispose()
        {
            this.connectionListener.Dispose();
        }

        public IConnection EndAccept(IAsyncResult result)
        {
            IConnection connection = this.connectionListener.EndAccept(result);
            if (connection == null)
            {
                return connection;
            }
            return new BufferedConnection(connection, this.flushTimeout, this.writeBufferSize);
        }

        public void Listen()
        {
            this.connectionListener.Listen();
        }
    }
}

