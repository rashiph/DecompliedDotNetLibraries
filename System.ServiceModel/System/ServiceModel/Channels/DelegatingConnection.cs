namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    internal abstract class DelegatingConnection : IConnection
    {
        private IConnection connection;

        protected DelegatingConnection(IConnection connection)
        {
            this.connection = connection;
        }

        public virtual void Abort()
        {
            this.connection.Abort();
        }

        public virtual AsyncReadResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state)
        {
            return this.connection.BeginRead(offset, size, timeout, callback, state);
        }

        public virtual IAsyncResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.connection.BeginWrite(buffer, offset, size, immediate, timeout, callback, state);
        }

        public virtual void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            this.connection.Close(timeout, asyncAndLinger);
        }

        public virtual object DuplicateAndClose(int targetProcessId)
        {
            return this.connection.DuplicateAndClose(targetProcessId);
        }

        public virtual int EndRead()
        {
            return this.connection.EndRead();
        }

        public virtual void EndWrite(IAsyncResult result)
        {
            this.connection.EndWrite(result);
        }

        public virtual object GetCoreTransport()
        {
            return this.connection.GetCoreTransport();
        }

        public virtual int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            return this.connection.Read(buffer, offset, size, timeout);
        }

        public virtual void Shutdown(TimeSpan timeout)
        {
            this.connection.Shutdown(timeout);
        }

        public virtual bool Validate(Uri uri)
        {
            return this.connection.Validate(uri);
        }

        public virtual void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            this.connection.Write(buffer, offset, size, immediate, timeout);
        }

        public virtual void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            this.connection.Write(buffer, offset, size, immediate, timeout, bufferManager);
        }

        public virtual byte[] AsyncReadBuffer
        {
            get
            {
                return this.connection.AsyncReadBuffer;
            }
        }

        public virtual int AsyncReadBufferSize
        {
            get
            {
                return this.connection.AsyncReadBufferSize;
            }
        }

        protected IConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public TraceEventType ExceptionEventType
        {
            get
            {
                return this.connection.ExceptionEventType;
            }
            set
            {
                this.connection.ExceptionEventType = value;
            }
        }

        public IPEndPoint RemoteIPEndPoint
        {
            get
            {
                return this.connection.RemoteIPEndPoint;
            }
        }
    }
}

