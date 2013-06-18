namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Threading;

    internal abstract class SessionConnectionReader : IMessageSource
    {
        private byte[] buffer;
        private IConnection connection;
        private byte[] envelopeBuffer;
        private int envelopeOffset;
        private int envelopeSize;
        private bool isAtEOF;
        private int offset;
        private WaitCallback onAsyncReadComplete;
        private WaitCallback pendingCallback;
        private object pendingCallbackState;
        private Exception pendingException;
        private Message pendingMessage;
        private IConnection rawConnection;
        private bool readIntoEnvelopeBuffer;
        private TimeoutHelper readTimeoutHelper;
        private SecurityMessageProperty security;
        private int size;
        private bool usingAsyncReadBuffer;

        protected SessionConnectionReader(IConnection connection, IConnection rawConnection, int offset, int size, SecurityMessageProperty security)
        {
            this.offset = offset;
            this.size = size;
            if (size > 0)
            {
                this.buffer = connection.AsyncReadBuffer;
            }
            this.connection = connection;
            this.rawConnection = rawConnection;
            this.onAsyncReadComplete = new WaitCallback(this.OnAsyncReadComplete);
            this.security = security;
        }

        public AsyncReceiveResult BeginReceive(TimeSpan timeout, WaitCallback callback, object state)
        {
            if ((this.pendingMessage == null) && (this.pendingException == null))
            {
                this.readTimeoutHelper = new TimeoutHelper(timeout);
                while (!this.isAtEOF)
                {
                    AsyncReadResult result;
                    if (this.size > 0)
                    {
                        this.pendingMessage = this.DecodeMessage(this.readTimeoutHelper.RemainingTime());
                        if (this.pendingMessage != null)
                        {
                            this.PrepareMessage(this.pendingMessage);
                            return AsyncReceiveResult.Completed;
                        }
                        if (this.isAtEOF)
                        {
                            return AsyncReceiveResult.Completed;
                        }
                    }
                    if (this.size != 0)
                    {
                        throw Fx.AssertAndThrow("BeginReceive: DecodeMessage() should consume the outstanding buffer or return a message.");
                    }
                    if (!this.usingAsyncReadBuffer)
                    {
                        this.buffer = this.connection.AsyncReadBuffer;
                        this.usingAsyncReadBuffer = true;
                    }
                    this.pendingCallback = callback;
                    this.pendingCallbackState = state;
                    bool flag = true;
                    try
                    {
                        result = this.connection.BeginRead(0, this.buffer.Length, this.readTimeoutHelper.RemainingTime(), this.onAsyncReadComplete, null);
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.pendingCallback = null;
                            this.pendingCallbackState = null;
                        }
                    }
                    if (result == AsyncReadResult.Queued)
                    {
                        return AsyncReceiveResult.Pending;
                    }
                    this.pendingCallback = null;
                    this.pendingCallbackState = null;
                    int bytesRead = this.connection.EndRead();
                    this.HandleReadComplete(bytesRead, false);
                }
            }
            return AsyncReceiveResult.Completed;
        }

        public AsyncReceiveResult BeginWaitForMessage(TimeSpan timeout, WaitCallback callback, object state)
        {
            try
            {
                return this.BeginReceive(timeout, callback, state);
            }
            catch (TimeoutException exception)
            {
                this.pendingException = exception;
                return AsyncReceiveResult.Completed;
            }
        }

        private Message DecodeMessage(TimeSpan timeout)
        {
            if ((DiagnosticUtility.ShouldUseActivity && (ServiceModelActivity.Current != null)) && (ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction))
            {
                ServiceModelActivity.Current.Resume();
            }
            if (!this.readIntoEnvelopeBuffer)
            {
                return this.DecodeMessage(this.buffer, ref this.offset, ref this.size, ref this.isAtEOF, timeout);
            }
            int envelopeOffset = this.envelopeOffset;
            return this.DecodeMessage(this.envelopeBuffer, ref envelopeOffset, ref this.size, ref this.isAtEOF, timeout);
        }

        protected abstract Message DecodeMessage(byte[] buffer, ref int offset, ref int size, ref bool isAtEof, TimeSpan timeout);
        public Message EndReceive()
        {
            return this.GetPendingMessage();
        }

        public bool EndWaitForMessage()
        {
            try
            {
                Message message = this.EndReceive();
                this.pendingMessage = message;
                return true;
            }
            catch (TimeoutException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                return false;
            }
        }

        protected abstract void EnsureDecoderAtEof();
        private Message GetPendingMessage()
        {
            if (this.pendingException != null)
            {
                Exception pendingException = this.pendingException;
                this.pendingException = null;
                throw TraceUtility.ThrowHelperError(pendingException, this.pendingMessage);
            }
            if (this.pendingMessage != null)
            {
                Message pendingMessage = this.pendingMessage;
                this.pendingMessage = null;
                return pendingMessage;
            }
            return null;
        }

        public IConnection GetRawConnection()
        {
            IConnection innerConnection = null;
            if (this.rawConnection == null)
            {
                return innerConnection;
            }
            innerConnection = this.rawConnection;
            this.rawConnection = null;
            if (this.size <= 0)
            {
                return innerConnection;
            }
            PreReadConnection connection2 = innerConnection as PreReadConnection;
            if (connection2 != null)
            {
                connection2.AddPreReadData(this.buffer, this.offset, this.size);
                return innerConnection;
            }
            return new PreReadConnection(innerConnection, this.buffer, this.offset, this.size);
        }

        private void HandleReadComplete(int bytesRead, bool readIntoEnvelopeBuffer)
        {
            this.readIntoEnvelopeBuffer = readIntoEnvelopeBuffer;
            if (bytesRead == 0)
            {
                this.EnsureDecoderAtEof();
                this.isAtEOF = true;
            }
            else
            {
                this.offset = 0;
                this.size = bytesRead;
            }
        }

        private void OnAsyncReadComplete(object state)
        {
            WaitCallback callback;
            try
            {
                do
                {
                    int bytesRead = this.connection.EndRead();
                    this.HandleReadComplete(bytesRead, false);
                    if (this.isAtEOF)
                    {
                        goto Label_00A0;
                    }
                    Message message = this.DecodeMessage(this.readTimeoutHelper.RemainingTime());
                    if (message != null)
                    {
                        this.PrepareMessage(message);
                        this.pendingMessage = message;
                        goto Label_00A0;
                    }
                    if (this.isAtEOF)
                    {
                        goto Label_00A0;
                    }
                    if (this.size != 0)
                    {
                        throw Fx.AssertAndThrow("OnAsyncReadComplete: DecodeMessage() should consume the outstanding buffer or return a message.");
                    }
                }
                while (this.connection.BeginRead(0, this.buffer.Length, this.readTimeoutHelper.RemainingTime(), this.onAsyncReadComplete, null) != AsyncReadResult.Queued);
                return;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.pendingException = exception;
            }
        Label_00A0:
            callback = this.pendingCallback;
            object pendingCallbackState = this.pendingCallbackState;
            this.pendingCallback = null;
            this.pendingCallbackState = null;
            callback(pendingCallbackState);
        }

        protected virtual void PrepareMessage(Message message)
        {
            if (this.security != null)
            {
                message.Properties.Security = (SecurityMessageProperty) this.security.CreateCopy();
            }
        }

        public Message Receive(TimeSpan timeout)
        {
            Message pendingMessage = this.GetPendingMessage();
            if (pendingMessage != null)
            {
                return pendingMessage;
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            while (!this.isAtEOF)
            {
                int num;
                if (this.size > 0)
                {
                    pendingMessage = this.DecodeMessage(helper.RemainingTime());
                    if (pendingMessage != null)
                    {
                        this.PrepareMessage(pendingMessage);
                        return pendingMessage;
                    }
                    if (this.isAtEOF)
                    {
                        return null;
                    }
                }
                if (this.size != 0)
                {
                    throw Fx.AssertAndThrow("Receive: DecodeMessage() should consume the outstanding buffer or return a message.");
                }
                if (this.buffer == null)
                {
                    this.buffer = DiagnosticUtility.Utility.AllocateByteArray(this.connection.AsyncReadBufferSize);
                }
                if ((this.EnvelopeBuffer != null) && ((this.EnvelopeSize - this.EnvelopeOffset) >= this.buffer.Length))
                {
                    num = this.connection.Read(this.EnvelopeBuffer, this.EnvelopeOffset, this.buffer.Length, helper.RemainingTime());
                    this.HandleReadComplete(num, true);
                }
                else
                {
                    num = this.connection.Read(this.buffer, 0, this.buffer.Length, helper.RemainingTime());
                    this.HandleReadComplete(num, false);
                }
            }
            return null;
        }

        protected void SendFault(string faultString, TimeSpan timeout)
        {
            byte[] drainBuffer = new byte[0x80];
            InitialServerConnectionReader.SendFault(this.connection, faultString, drainBuffer, timeout, 0x10000);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            try
            {
                Message message = this.Receive(timeout);
                this.pendingMessage = message;
                return true;
            }
            catch (TimeoutException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                return false;
            }
        }

        protected byte[] EnvelopeBuffer
        {
            get
            {
                return this.envelopeBuffer;
            }
            set
            {
                this.envelopeBuffer = value;
            }
        }

        protected int EnvelopeOffset
        {
            get
            {
                return this.envelopeOffset;
            }
            set
            {
                this.envelopeOffset = value;
            }
        }

        protected int EnvelopeSize
        {
            get
            {
                return this.envelopeSize;
            }
            set
            {
                this.envelopeSize = value;
            }
        }
    }
}

