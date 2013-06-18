namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal sealed class ConnectionModeReader : InitialServerConnectionReader
    {
        private byte[] buffer;
        private ConnectionModeCallback callback;
        private ServerModeDecoder decoder;
        private int offset;
        private static WaitCallback readCallback;
        private Exception readException;
        private TimeoutHelper receiveTimeoutHelper;
        private int size;

        public ConnectionModeReader(IConnection connection, ConnectionModeCallback callback, ConnectionClosedCallback closedCallback) : base(connection, closedCallback)
        {
            this.callback = callback;
        }

        private void Complete()
        {
            this.callback(this);
        }

        private void Complete(Exception e)
        {
            this.readException = e;
            this.Complete();
        }

        private bool ContinueReading()
        {
            while (true)
            {
                if (this.size == 0)
                {
                    if (readCallback == null)
                    {
                        readCallback = new WaitCallback(ConnectionModeReader.ReadCallback);
                    }
                    if (base.Connection.BeginRead(0, base.Connection.AsyncReadBufferSize, this.GetRemainingTimeout(), readCallback, this) == AsyncReadResult.Queued)
                    {
                        return false;
                    }
                    if (!this.GetReadResult())
                    {
                        return false;
                    }
                }
                do
                {
                    int num;
                    try
                    {
                        num = this.decoder.Decode(this.buffer, this.offset, this.size);
                    }
                    catch (CommunicationException exception)
                    {
                        string str;
                        if (FramingEncodingString.TryGetFaultString(exception, out str))
                        {
                            byte[] drainBuffer = new byte[0x80];
                            InitialServerConnectionReader.SendFault(base.Connection, str, drainBuffer, this.GetRemainingTimeout(), base.MaxViaSize + base.MaxContentTypeSize);
                            base.Close(this.GetRemainingTimeout());
                        }
                        throw;
                    }
                    if (num > 0)
                    {
                        this.offset += num;
                        this.size -= num;
                    }
                    if (this.decoder.CurrentState == ServerModeDecoder.State.Done)
                    {
                        return true;
                    }
                }
                while (this.size != 0);
            }
        }

        public FramingMode GetConnectionMode()
        {
            if (this.readException != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.readException, base.Connection.ExceptionEventType);
            }
            return this.decoder.Mode;
        }

        private bool GetReadResult()
        {
            this.offset = 0;
            this.size = base.Connection.EndRead();
            if (this.size == 0)
            {
                if (this.decoder.StreamPosition != 0L)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
                }
                base.Close(this.GetRemainingTimeout());
                return false;
            }
            base.Connection.ExceptionEventType = TraceEventType.Error;
            if (this.buffer == null)
            {
                this.buffer = base.Connection.AsyncReadBuffer;
            }
            return true;
        }

        public TimeSpan GetRemainingTimeout()
        {
            return this.receiveTimeoutHelper.RemainingTime();
        }

        private static void ReadCallback(object state)
        {
            ConnectionModeReader reader = (ConnectionModeReader) state;
            bool flag = false;
            Exception e = null;
            try
            {
                if (reader.GetReadResult())
                {
                    flag = reader.ContinueReading();
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                flag = true;
                e = exception2;
            }
            if (flag)
            {
                reader.Complete(e);
            }
        }

        public void StartReading(TimeSpan receiveTimeout, Action connectionDequeuedCallback)
        {
            this.decoder = new ServerModeDecoder();
            this.receiveTimeoutHelper = new TimeoutHelper(receiveTimeout);
            base.ConnectionDequeuedCallback = connectionDequeuedCallback;
            bool flag = false;
            Exception e = null;
            try
            {
                flag = this.ContinueReading();
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                flag = true;
                e = exception2;
            }
            if (flag)
            {
                this.Complete(e);
            }
        }

        public int BufferOffset
        {
            get
            {
                return this.offset;
            }
        }

        public int BufferSize
        {
            get
            {
                return this.size;
            }
        }

        public long StreamPosition
        {
            get
            {
                return this.decoder.StreamPosition;
            }
        }
    }
}

