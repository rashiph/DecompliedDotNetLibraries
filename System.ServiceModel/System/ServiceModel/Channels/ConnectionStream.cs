namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class ConnectionStream : Stream
    {
        private TimeSpan closeTimeout;
        private IConnection connection;
        private bool immediate;
        private int readTimeout;
        private int writeTimeout;

        public ConnectionStream(IConnection connection, IDefaultCommunicationTimeouts defaultTimeouts)
        {
            this.connection = connection;
            this.closeTimeout = defaultTimeouts.CloseTimeout;
            this.ReadTimeout = TimeoutHelper.ToMilliseconds(defaultTimeouts.ReceiveTimeout);
            this.WriteTimeout = TimeoutHelper.ToMilliseconds(defaultTimeouts.SendTimeout);
            this.immediate = true;
        }

        public void Abort()
        {
            this.connection.Abort();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new ReadAsyncResult(this.connection, buffer, offset, count, TimeoutHelper.FromMilliseconds(this.ReadTimeout), callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.connection.BeginWrite(buffer, offset, count, this.Immediate, TimeoutHelper.FromMilliseconds(this.WriteTimeout), callback, state);
        }

        public override void Close()
        {
            this.connection.Close(this.CloseTimeout, false);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ReadAsyncResult.End(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.connection.EndWrite(asyncResult);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Read(buffer, offset, count, TimeoutHelper.FromMilliseconds(this.ReadTimeout));
        }

        protected int Read(byte[] buffer, int offset, int count, TimeSpan timeout)
        {
            return this.connection.Read(buffer, offset, count, timeout);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
        }

        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
        }

        public void Shutdown(TimeSpan timeout)
        {
            this.connection.Shutdown(timeout);
        }

        public bool Validate(Uri uri)
        {
            return this.connection.Validate(uri);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.connection.Write(buffer, offset, count, this.Immediate, TimeoutHelper.FromMilliseconds(this.WriteTimeout));
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public TimeSpan CloseTimeout
        {
            get
            {
                return this.closeTimeout;
            }
            set
            {
                this.closeTimeout = value;
            }
        }

        public IConnection Connection
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

        public bool Immediate
        {
            get
            {
                return this.immediate;
            }
            set
            {
                this.immediate = value;
            }
        }

        public override long Length
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
            }
        }

        public override long Position
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.readTimeout;
            }
            set
            {
                if (value < -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { -1, 0x7fffffff })));
                }
                this.readTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.writeTimeout;
            }
            set
            {
                if (value < -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { -1, 0x7fffffff })));
                }
                this.writeTimeout = value;
            }
        }

        private class ReadAsyncResult : AsyncResult
        {
            private byte[] buffer;
            private int bytesRead;
            private IConnection connection;
            private int offset;
            private static WaitCallback onAsyncReadComplete;

            public ReadAsyncResult(IConnection connection, byte[] buffer, int offset, int count, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.buffer = buffer;
                this.offset = offset;
                this.connection = connection;
                if (onAsyncReadComplete == null)
                {
                    onAsyncReadComplete = new WaitCallback(ConnectionStream.ReadAsyncResult.OnAsyncReadComplete);
                }
                if (this.connection.BeginRead(0, Math.Min(count, this.connection.AsyncReadBufferSize), timeout, onAsyncReadComplete, this) == AsyncReadResult.Completed)
                {
                    this.HandleRead();
                    base.Complete(true);
                }
            }

            public static int End(IAsyncResult result)
            {
                return AsyncResult.End<ConnectionStream.ReadAsyncResult>(result).bytesRead;
            }

            private void HandleRead()
            {
                this.bytesRead = this.connection.EndRead();
                Buffer.BlockCopy(this.connection.AsyncReadBuffer, 0, this.buffer, this.offset, this.bytesRead);
            }

            private static void OnAsyncReadComplete(object state)
            {
                ConnectionStream.ReadAsyncResult result = (ConnectionStream.ReadAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.HandleRead();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.Complete(false, exception);
            }
        }
    }
}

