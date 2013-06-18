namespace System.Messaging
{
    using System;
    using System.Messaging.Interop;

    public sealed class Cursor : IDisposable
    {
        private bool disposed;
        private CursorHandle handle;

        internal Cursor(MessageQueue queue)
        {
            CursorHandle handle;
            int num = SafeNativeMethods.MQCreateCursor(queue.MQInfo.ReadHandle, out handle);
            if (MessageQueue.IsFatalError(num))
            {
                throw new MessageQueueException(num);
            }
            this.handle = handle;
        }

        public void Close()
        {
            this.handle.Close();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.Close();
            this.disposed = true;
        }

        ~Cursor()
        {
            this.Dispose(false);
        }

        internal CursorHandle Handle
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.handle;
            }
        }
    }
}

