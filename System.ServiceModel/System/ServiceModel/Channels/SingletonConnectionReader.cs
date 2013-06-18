namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Xml;

    internal abstract class SingletonConnectionReader
    {
        private IConnection connection;
        private bool doneReceiving;
        private bool doneSending;
        private Stream inputStream;
        private bool isAtEof;
        private bool isClosed;
        private int offset;
        private SecurityMessageProperty security;
        private int size;
        private object thisLock = new object();
        private IConnectionOrientedTransportFactorySettings transportSettings;
        private Uri via;

        protected SingletonConnectionReader(IConnection connection, int offset, int size, SecurityMessageProperty security, IConnectionOrientedTransportFactorySettings transportSettings, Uri via)
        {
            this.connection = connection;
            this.offset = offset;
            this.size = size;
            this.security = security;
            this.transportSettings = transportSettings;
            this.via = via;
        }

        public void Abort()
        {
            this.connection.Abort();
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReceiveAsyncResult(this, timeout, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            lock (this.ThisLock)
            {
                if (this.isClosed)
                {
                    return;
                }
                this.isClosed = true;
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            bool flag = false;
            try
            {
                if (this.inputStream != null)
                {
                    byte[] buffer = DiagnosticUtility.Utility.AllocateByteArray(this.transportSettings.ConnectionBufferSize);
                    while (!this.isAtEof)
                    {
                        this.inputStream.ReadTimeout = TimeoutHelper.ToMilliseconds(helper.RemainingTime());
                        if (this.inputStream.Read(buffer, 0, buffer.Length) == 0)
                        {
                            this.isAtEof = true;
                        }
                    }
                }
                this.OnClose(helper.RemainingTime());
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    this.Abort();
                }
            }
        }

        protected abstract bool DecodeBytes(byte[] buffer, ref int offset, ref int size, ref bool isAtEof);
        public void DoneReceiving(bool atEof)
        {
            this.DoneReceiving(atEof, this.transportSettings.CloseTimeout);
        }

        private void DoneReceiving(bool atEof, TimeSpan timeout)
        {
            if (!this.doneReceiving)
            {
                this.isAtEof = atEof;
                this.doneReceiving = true;
                if (this.doneSending)
                {
                    this.Close(timeout);
                }
            }
        }

        public void DoneSending(TimeSpan timeout)
        {
            this.doneSending = true;
            if (this.doneReceiving)
            {
                this.Close(timeout);
            }
        }

        public virtual Message EndReceive(IAsyncResult result)
        {
            return ReceiveAsyncResult.End(result);
        }

        protected abstract void OnClose(TimeSpan timeout);
        protected virtual void PrepareMessage(Message message)
        {
            message.Properties.Via = this.via;
            message.Properties.Security = (this.security != null) ? ((SecurityMessageProperty) this.security.CreateCopy()) : null;
        }

        public Message Receive(TimeSpan timeout)
        {
            byte[] dst = DiagnosticUtility.Utility.AllocateByteArray(this.connection.AsyncReadBufferSize);
            if (this.size > 0)
            {
                Buffer.BlockCopy(this.connection.AsyncReadBuffer, this.offset, dst, this.offset, this.size);
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            while (!this.DecodeBytes(dst, ref this.offset, ref this.size, ref this.isAtEof))
            {
                if (this.isAtEof)
                {
                    this.DoneReceiving(true, helper.RemainingTime());
                    return null;
                }
                if (this.size == 0)
                {
                    this.offset = 0;
                    this.size = this.connection.Read(dst, 0, dst.Length, helper.RemainingTime());
                    if (this.size == 0)
                    {
                        this.DoneReceiving(true, helper.RemainingTime());
                        return null;
                    }
                }
            }
            IConnection innerConnection = this.connection;
            if (this.size > 0)
            {
                byte[] buffer2 = DiagnosticUtility.Utility.AllocateByteArray(this.size);
                Buffer.BlockCopy(dst, this.offset, buffer2, 0, this.size);
                innerConnection = new PreReadConnection(innerConnection, buffer2);
            }
            Stream stream = new SingletonInputConnectionStream(this, innerConnection, this.transportSettings);
            this.inputStream = new MaxMessageSizeStream(stream, this.transportSettings.MaxReceivedMessageSize);
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity(true) : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityProcessingMessage", new object[] { TraceUtility.RetrieveMessageNumber() }), ActivityType.ProcessMessage);
                }
                Message message = null;
                try
                {
                    message = this.transportSettings.MessageEncoderFactory.Encoder.ReadMessage(this.inputStream, this.transportSettings.MaxBufferSize, this.ContentType);
                }
                catch (XmlException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageXmlProtocolError"), exception));
                }
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    TraceUtility.TransferFromTransport(message);
                }
                this.PrepareMessage(message);
                return message;
            }
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            return new StreamedFramingRequestContext(this, this.Receive(timeout));
        }

        protected IConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        protected virtual string ContentType
        {
            get
            {
                return null;
            }
        }

        protected abstract long StreamPosition { get; }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private class ReceiveAsyncResult : AsyncResult
        {
            private Message message;
            private static Action<object> onReceiveScheduled = new Action<object>(SingletonConnectionReader.ReceiveAsyncResult.OnReceiveScheduled);
            private SingletonConnectionReader parent;
            private TimeSpan timeout;

            public ReceiveAsyncResult(SingletonConnectionReader parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.timeout = timeout;
                ActionItem.Schedule(onReceiveScheduled, this);
            }

            public static Message End(IAsyncResult result)
            {
                return AsyncResult.End<SingletonConnectionReader.ReceiveAsyncResult>(result).message;
            }

            private static void OnReceiveScheduled(object state)
            {
                SingletonConnectionReader.ReceiveAsyncResult result = (SingletonConnectionReader.ReceiveAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.message = result.parent.Receive(result.timeout);
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

        private class SingletonInputConnectionStream : ConnectionStream
        {
            private bool atEof;
            private byte[] chunkBuffer;
            private int chunkBufferOffset;
            private int chunkBufferSize;
            private int chunkBytesRemaining;
            private SingletonMessageDecoder decoder;
            private SingletonConnectionReader reader;

            public SingletonInputConnectionStream(SingletonConnectionReader reader, IConnection connection, IDefaultCommunicationTimeouts defaultTimeouts) : base(connection, defaultTimeouts)
            {
                this.reader = reader;
                this.decoder = new SingletonMessageDecoder(reader.StreamPosition);
                this.chunkBytesRemaining = 0;
                this.chunkBuffer = new byte[5];
            }

            private void AbortReader()
            {
                this.reader.Abort();
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return new ReadAsyncResult(this, buffer, offset, count, callback, state);
            }

            public override void Close()
            {
                this.reader.DoneReceiving(this.atEof);
            }

            private void DecodeData(byte[] buffer, int offset, int size)
            {
                while (size > 0)
                {
                    int num = this.decoder.Decode(buffer, offset, size);
                    offset += num;
                    size -= num;
                }
            }

            private void DecodeSize(byte[] buffer, ref int offset, ref int size)
            {
                while (size > 0)
                {
                    int num = this.decoder.Decode(buffer, offset, size);
                    if (num > 0)
                    {
                        offset += num;
                        size -= num;
                    }
                    SingletonMessageDecoder.State currentState = this.decoder.CurrentState;
                    if (currentState != SingletonMessageDecoder.State.ChunkStart)
                    {
                        if (currentState == SingletonMessageDecoder.State.End)
                        {
                            goto Label_0081;
                        }
                        continue;
                    }
                    this.chunkBytesRemaining = this.decoder.ChunkSize;
                    if ((size > 0) && !object.ReferenceEquals(buffer, this.chunkBuffer))
                    {
                        Buffer.BlockCopy(buffer, offset, this.chunkBuffer, 0, size);
                        this.chunkBufferOffset = 0;
                        this.chunkBufferSize = size;
                    }
                    return;
                Label_0081:
                    this.ProcessEof();
                    return;
                }
            }

            public override int EndRead(IAsyncResult result)
            {
                return ReadAsyncResult.End(result);
            }

            private void ProcessEof()
            {
                if (!this.atEof)
                {
                    this.atEof = true;
                    if (((this.chunkBufferSize > 0) || (this.chunkBytesRemaining > 0)) || (this.decoder.CurrentState != SingletonMessageDecoder.State.End))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
                    }
                    this.reader.DoneReceiving(true);
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int num = 0;
                while (count != 0)
                {
                    if (this.atEof)
                    {
                        return num;
                    }
                    if (this.chunkBufferSize > 0)
                    {
                        int num2 = Math.Min(this.chunkBytesRemaining, Math.Min(this.chunkBufferSize, count));
                        Buffer.BlockCopy(this.chunkBuffer, this.chunkBufferOffset, buffer, offset, num2);
                        this.DecodeData(this.chunkBuffer, this.chunkBufferOffset, num2);
                        this.chunkBufferOffset += num2;
                        this.chunkBufferSize -= num2;
                        this.chunkBytesRemaining -= num2;
                        if ((this.chunkBytesRemaining == 0) && (this.chunkBufferSize > 0))
                        {
                            this.DecodeSize(this.chunkBuffer, ref this.chunkBufferOffset, ref this.chunkBufferSize);
                        }
                        num += num2;
                        offset += num2;
                        count -= num2;
                    }
                    else
                    {
                        if (this.chunkBytesRemaining > 0)
                        {
                            int num3 = count;
                            if ((0x7fffffff - this.chunkBytesRemaining) >= 5)
                            {
                                num3 = Math.Min(count, this.chunkBytesRemaining + 5);
                            }
                            int num4 = this.ReadCore(buffer, offset, num3);
                            this.DecodeData(buffer, offset, Math.Min(num4, this.chunkBytesRemaining));
                            if (num4 > this.chunkBytesRemaining)
                            {
                                num += this.chunkBytesRemaining;
                                int size = num4 - this.chunkBytesRemaining;
                                int num6 = offset + this.chunkBytesRemaining;
                                this.chunkBytesRemaining = 0;
                                this.DecodeSize(buffer, ref num6, ref size);
                                return num;
                            }
                            num += num4;
                            this.chunkBytesRemaining -= num4;
                            return num;
                        }
                        if (count < 5)
                        {
                            this.chunkBufferOffset = 0;
                            this.chunkBufferSize = this.ReadCore(this.chunkBuffer, 0, this.chunkBuffer.Length);
                            this.DecodeSize(this.chunkBuffer, ref this.chunkBufferOffset, ref this.chunkBufferSize);
                        }
                        else
                        {
                            int num7 = this.ReadCore(buffer, offset, 5);
                            int num8 = offset;
                            this.DecodeSize(buffer, ref num8, ref num7);
                        }
                    }
                }
                return num;
            }

            private int ReadCore(byte[] buffer, int offset, int count)
            {
                int num = -1;
                try
                {
                    num = base.Read(buffer, offset, count);
                    if (num == 0)
                    {
                        this.ProcessEof();
                    }
                }
                finally
                {
                    if (num == -1)
                    {
                        this.AbortReader();
                    }
                }
                return num;
            }

            public class ReadAsyncResult : AsyncResult
            {
                private SingletonConnectionReader.SingletonInputConnectionStream parent;
                private int result;

                public ReadAsyncResult(SingletonConnectionReader.SingletonInputConnectionStream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.parent = parent;
                    this.result = this.parent.Read(buffer, offset, count);
                    base.Complete(true);
                }

                public static int End(IAsyncResult result)
                {
                    return AsyncResult.End<SingletonConnectionReader.SingletonInputConnectionStream.ReadAsyncResult>(result).result;
                }
            }
        }

        private class StreamedFramingRequestContext : RequestContextBase
        {
            private IConnection connection;
            private SingletonConnectionReader parent;
            private IConnectionOrientedTransportFactorySettings settings;
            private TimeoutHelper timeoutHelper;

            public StreamedFramingRequestContext(SingletonConnectionReader parent, Message requestMessage) : base(requestMessage, parent.transportSettings.CloseTimeout, parent.transportSettings.SendTimeout)
            {
                this.parent = parent;
                this.connection = parent.connection;
                this.settings = parent.transportSettings;
            }

            protected override void OnAbort()
            {
                this.parent.Abort();
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                return StreamingConnectionHelper.BeginWriteMessage(message, this.connection, false, this.settings, ref this.timeoutHelper, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.parent.Close(timeout);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                StreamingConnectionHelper.EndWriteMessage(result);
                this.parent.DoneSending(this.timeoutHelper.RemainingTime());
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                StreamingConnectionHelper.WriteMessage(message, this.connection, false, this.settings, ref this.timeoutHelper);
                this.parent.DoneSending(this.timeoutHelper.RemainingTime());
            }
        }
    }
}

