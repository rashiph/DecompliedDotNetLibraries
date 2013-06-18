namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class StreamConnection : IConnection
    {
        private byte[] asyncReadBuffer;
        private int bytesRead;
        private ConnectionStream innerStream;
        private AsyncCallback onRead;
        private WaitCallback readCallback;
        private IAsyncResult readResult;
        private System.IO.Stream stream;

        public StreamConnection(System.IO.Stream stream, ConnectionStream innerStream)
        {
            this.stream = stream;
            this.innerStream = innerStream;
            this.onRead = Fx.ThunkCallback(new AsyncCallback(this.OnRead));
        }

        public void Abort()
        {
            this.innerStream.Abort();
        }

        public AsyncReadResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state)
        {
            ConnectionUtilities.ValidateBufferBounds(this.AsyncReadBufferSize, offset, size);
            this.readCallback = callback;
            try
            {
                this.SetReadTimeout(timeout);
                IAsyncResult asyncResult = this.stream.BeginRead(this.AsyncReadBuffer, offset, size, this.onRead, state);
                if (!asyncResult.CompletedSynchronously)
                {
                    return AsyncReadResult.Queued;
                }
                this.bytesRead = this.stream.EndRead(asyncResult);
            }
            catch (IOException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertIOException(exception));
            }
            return AsyncReadResult.Completed;
        }

        public IAsyncResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            try
            {
                this.innerStream.Immediate = immediate;
                this.SetWriteTimeout(timeout);
                result = this.stream.BeginWrite(buffer, offset, size, callback, state);
            }
            catch (IOException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertIOException(exception));
            }
            return result;
        }

        public void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            this.innerStream.CloseTimeout = timeout;
            try
            {
                this.stream.Close();
            }
            catch (IOException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertIOException(exception));
            }
        }

        private Exception ConvertIOException(IOException ioException)
        {
            if (ioException.InnerException is TimeoutException)
            {
                return new TimeoutException(ioException.InnerException.Message, ioException);
            }
            if (ioException.InnerException is CommunicationObjectAbortedException)
            {
                return new CommunicationObjectAbortedException(ioException.InnerException.Message, ioException);
            }
            if (ioException.InnerException is CommunicationException)
            {
                return new CommunicationException(ioException.InnerException.Message, ioException);
            }
            return new CommunicationException(System.ServiceModel.SR.GetString("StreamError"), ioException);
        }

        public object DuplicateAndClose(int targetProcessId)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public int EndRead()
        {
            if (this.readResult != null)
            {
                IAsyncResult readResult = this.readResult;
                this.readResult = null;
                try
                {
                    this.bytesRead = this.stream.EndRead(readResult);
                }
                catch (IOException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertIOException(exception));
                }
            }
            return this.bytesRead;
        }

        public void EndWrite(IAsyncResult result)
        {
            try
            {
                this.stream.EndWrite(result);
            }
            catch (IOException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertIOException(exception));
            }
        }

        public virtual object GetCoreTransport()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        private void OnRead(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                if (this.readResult != null)
                {
                    throw Fx.AssertAndThrow("StreamConnection: OnRead called twice.");
                }
                this.readResult = result;
                this.readCallback(result.AsyncState);
            }
        }

        public int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            int num;
            try
            {
                this.SetReadTimeout(timeout);
                num = this.stream.Read(buffer, offset, size);
            }
            catch (IOException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertIOException(exception));
            }
            return num;
        }

        private void SetReadTimeout(TimeSpan timeout)
        {
            int num = TimeoutHelper.ToMilliseconds(timeout);
            if (this.stream.CanTimeout)
            {
                this.stream.ReadTimeout = num;
            }
            this.innerStream.ReadTimeout = num;
        }

        private void SetWriteTimeout(TimeSpan timeout)
        {
            int num = TimeoutHelper.ToMilliseconds(timeout);
            if (this.stream.CanTimeout)
            {
                this.stream.WriteTimeout = num;
            }
            this.innerStream.WriteTimeout = num;
        }

        public void Shutdown(TimeSpan timeout)
        {
            this.innerStream.Shutdown(timeout);
        }

        public bool Validate(Uri uri)
        {
            return this.innerStream.Validate(uri);
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            try
            {
                this.innerStream.Immediate = immediate;
                this.SetWriteTimeout(timeout);
                this.stream.Write(buffer, offset, size);
            }
            catch (IOException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertIOException(exception));
            }
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            this.Write(buffer, offset, size, immediate, timeout);
            bufferManager.ReturnBuffer(buffer);
        }

        public byte[] AsyncReadBuffer
        {
            get
            {
                if (this.asyncReadBuffer == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.asyncReadBuffer == null)
                        {
                            this.asyncReadBuffer = DiagnosticUtility.Utility.AllocateByteArray(this.innerStream.Connection.AsyncReadBufferSize);
                        }
                    }
                }
                return this.asyncReadBuffer;
            }
        }

        public int AsyncReadBufferSize
        {
            get
            {
                return this.innerStream.Connection.AsyncReadBufferSize;
            }
        }

        public TraceEventType ExceptionEventType
        {
            get
            {
                return this.innerStream.ExceptionEventType;
            }
            set
            {
                this.innerStream.ExceptionEventType = value;
            }
        }

        public IPEndPoint RemoteIPEndPoint
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }

        public System.IO.Stream Stream
        {
            get
            {
                return this.stream;
            }
        }

        public object ThisLock
        {
            get
            {
                return this;
            }
        }
    }
}

