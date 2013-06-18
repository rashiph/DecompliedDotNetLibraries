namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Threading;

    internal class StreamedFramingRequestChannel : RequestChannel
    {
        private ChannelBinding channelBindingToken;
        private IConnectionInitiator connectionInitiator;
        private ConnectionPool connectionPool;
        private MessageEncoder messageEncoder;
        private IConnectionOrientedTransportFactorySettings settings;
        private byte[] startBytes;
        private StreamUpgradeProvider upgrade;

        public StreamedFramingRequestChannel(ChannelManagerBase factory, IConnectionOrientedTransportChannelFactorySettings settings, EndpointAddress remoteAddresss, Uri via, IConnectionInitiator connectionInitiator, ConnectionPool connectionPool) : base(factory, remoteAddresss, via, settings.ManualAddressing)
        {
            this.settings = settings;
            this.connectionInitiator = connectionInitiator;
            this.connectionPool = connectionPool;
            this.messageEncoder = settings.MessageEncoderFactory.Encoder;
            this.upgrade = settings.Upgrade;
        }

        protected override IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state)
        {
            return new StreamedFramingAsyncRequest(this, callback, state);
        }

        protected override IRequest CreateRequest(Message message)
        {
            return new StreamedFramingRequest(this);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.BeginWaitForPendingRequests(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            base.WaitForPendingRequests(timeout);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.channelBindingToken != null)
            {
                this.channelBindingToken.Close();
                this.channelBindingToken = null;
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            base.EndWaitForPendingRequests(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void OnOpened()
        {
            EncodedVia via = new EncodedVia(base.Via.AbsoluteUri);
            EncodedContentType contentType = EncodedContentType.Create(this.settings.MessageEncoderFactory.Encoder.ContentType);
            int size = ClientSingletonEncoder.ModeBytes.Length + ClientSingletonEncoder.CalcStartSize(via, contentType);
            int dstOffset = 0;
            if (this.upgrade == null)
            {
                dstOffset = size;
                size += SessionEncoder.PreambleEndBytes.Length;
            }
            this.startBytes = DiagnosticUtility.Utility.AllocateByteArray(size);
            Buffer.BlockCopy(ClientSingletonEncoder.ModeBytes, 0, this.startBytes, 0, ClientSingletonEncoder.ModeBytes.Length);
            ClientSingletonEncoder.EncodeStart(this.startBytes, ClientSingletonEncoder.ModeBytes.Length, via, contentType);
            if (dstOffset > 0)
            {
                Buffer.BlockCopy(ClientSingletonEncoder.PreambleEndBytes, 0, this.startBytes, dstOffset, ClientSingletonEncoder.PreambleEndBytes.Length);
            }
            base.OnOpened();
        }

        private IConnection SendPreamble(IConnection connection, ref TimeoutHelper timeoutHelper, ClientFramingDecoder decoder, out SecurityMessageProperty remoteSecurity)
        {
            connection.Write(this.Preamble, 0, this.Preamble.Length, true, timeoutHelper.RemainingTime());
            if (this.upgrade != null)
            {
                IStreamUpgradeChannelBindingProvider property = this.upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                StreamUpgradeInitiator upgradeInitiator = this.upgrade.CreateUpgradeInitiator(base.RemoteAddress, base.Via);
                if (!ConnectionUpgradeHelper.InitiateUpgrade(upgradeInitiator, ref connection, decoder, this, ref timeoutHelper))
                {
                    ConnectionUpgradeHelper.DecodeFramingFault(decoder, connection, base.Via, this.messageEncoder.ContentType, ref timeoutHelper);
                }
                if ((property != null) && property.IsChannelBindingSupportEnabled)
                {
                    this.channelBindingToken = property.GetChannelBinding(upgradeInitiator, ChannelBindingKind.Endpoint);
                }
                remoteSecurity = StreamSecurityUpgradeInitiator.GetRemoteSecurity(upgradeInitiator);
                connection.Write(ClientSingletonEncoder.PreambleEndBytes, 0, ClientSingletonEncoder.PreambleEndBytes.Length, true, timeoutHelper.RemainingTime());
            }
            else
            {
                remoteSecurity = null;
            }
            byte[] buffer = new byte[1];
            int count = connection.Read(buffer, 0, buffer.Length, timeoutHelper.RemainingTime());
            if (!ConnectionUpgradeHelper.ValidatePreambleResponse(buffer, count, decoder, base.Via))
            {
                ConnectionUpgradeHelper.DecodeFramingFault(decoder, connection, base.Via, this.messageEncoder.ContentType, ref timeoutHelper);
            }
            return connection;
        }

        private byte[] Preamble
        {
            get
            {
                return this.startBytes;
            }
        }

        private class ClientSingletonConnectionReader : SingletonConnectionReader
        {
            private StreamedFramingRequestChannel.StreamedConnectionPoolHelper connectionPoolHelper;

            public ClientSingletonConnectionReader(IConnection connection, StreamedFramingRequestChannel.StreamedConnectionPoolHelper connectionPoolHelper, IConnectionOrientedTransportFactorySettings settings) : base(connection, 0, 0, connectionPoolHelper.RemoteSecurity, settings, null)
            {
                this.connectionPoolHelper = connectionPoolHelper;
            }

            protected override bool DecodeBytes(byte[] buffer, ref int offset, ref int size, ref bool isAtEof)
            {
                while (size > 0)
                {
                    int num = this.connectionPoolHelper.Decoder.Decode(buffer, offset, size);
                    if (num > 0)
                    {
                        offset += num;
                        size -= num;
                    }
                    ClientFramingDecoderState currentState = this.connectionPoolHelper.Decoder.CurrentState;
                    if (currentState != ClientFramingDecoderState.EnvelopeStart)
                    {
                        if (currentState == ClientFramingDecoderState.End)
                        {
                            goto Label_0047;
                        }
                        continue;
                    }
                    return true;
                Label_0047:
                    isAtEof = true;
                    return false;
                }
                return false;
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.connectionPoolHelper.Close(timeout);
            }

            protected override long StreamPosition
            {
                get
                {
                    return this.connectionPoolHelper.Decoder.StreamPosition;
                }
            }
        }

        internal class StreamedConnectionPoolHelper : ConnectionPoolHelper
        {
            private StreamedFramingRequestChannel channel;
            private ClientSingletonDecoder decoder;
            private SecurityMessageProperty remoteSecurity;

            public StreamedConnectionPoolHelper(StreamedFramingRequestChannel channel) : base(channel.connectionPool, channel.connectionInitiator, channel.Via)
            {
                this.channel = channel;
            }

            protected override IConnection AcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper)
            {
                this.decoder = new ClientSingletonDecoder(0L);
                return this.channel.SendPreamble(connection, ref timeoutHelper, this.decoder, out this.remoteSecurity);
            }

            protected override IAsyncResult BeginAcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                this.decoder = new ClientSingletonDecoder(0L);
                return new SendPreambleAsyncResult(this.channel, connection, ref timeoutHelper, this.decoder, callback, state);
            }

            protected override TimeoutException CreateNewConnectionTimeoutException(TimeSpan timeout, TimeoutException innerException)
            {
                return new TimeoutException(System.ServiceModel.SR.GetString("RequestTimedOutEstablishingTransportSession", new object[] { timeout, this.channel.Via.AbsoluteUri }), innerException);
            }

            protected override IConnection EndAcceptPooledConnection(IAsyncResult result)
            {
                return SendPreambleAsyncResult.End(result, out this.remoteSecurity);
            }

            public ClientSingletonDecoder Decoder
            {
                get
                {
                    return this.decoder;
                }
            }

            public SecurityMessageProperty RemoteSecurity
            {
                get
                {
                    return this.remoteSecurity;
                }
            }

            private class SendPreambleAsyncResult : AsyncResult
            {
                private StreamedFramingRequestChannel channel;
                private IStreamUpgradeChannelBindingProvider channelBindingProvider;
                private IConnection connection;
                private ClientFramingDecoder decoder;
                private static AsyncCallback onFailedUpgrade;
                private static WaitCallback onReadPreambleAck = new WaitCallback(StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult.OnReadPreambleAck);
                private static AsyncCallback onUpgrade;
                private static AsyncCallback onWritePreamble = Fx.ThunkCallback(new AsyncCallback(StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult.OnWritePreamble));
                private static AsyncCallback onWritePreambleEnd;
                private SecurityMessageProperty remoteSecurity;
                private TimeoutHelper timeoutHelper;
                private StreamUpgradeInitiator upgradeInitiator;

                public SendPreambleAsyncResult(StreamedFramingRequestChannel channel, IConnection connection, ref TimeoutHelper timeoutHelper, ClientFramingDecoder decoder, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.channel = channel;
                    this.connection = connection;
                    this.timeoutHelper = timeoutHelper;
                    this.decoder = decoder;
                    IAsyncResult result = connection.BeginWrite(channel.Preamble, 0, channel.Preamble.Length, true, timeoutHelper.RemainingTime(), onWritePreamble, this);
                    if (result.CompletedSynchronously && this.HandleWritePreamble(result))
                    {
                        base.Complete(true);
                    }
                }

                public static IConnection End(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
                {
                    StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult result2 = AsyncResult.End<StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult>(result);
                    remoteSecurity = result2.remoteSecurity;
                    return result2.connection;
                }

                private bool HandlePreambleAck()
                {
                    int count = this.connection.EndRead();
                    if (!ConnectionUpgradeHelper.ValidatePreambleResponse(this.connection.AsyncReadBuffer, count, this.decoder, this.channel.Via))
                    {
                        if (onFailedUpgrade == null)
                        {
                            onFailedUpgrade = Fx.ThunkCallback(new AsyncCallback(StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult.OnFailedUpgrade));
                        }
                        IAsyncResult result = ConnectionUpgradeHelper.BeginDecodeFramingFault(this.decoder, this.connection, this.channel.Via, this.channel.messageEncoder.ContentType, ref this.timeoutHelper, onFailedUpgrade, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                    }
                    return true;
                }

                private bool HandleUpgrade(IAsyncResult result)
                {
                    this.connection = ConnectionUpgradeHelper.EndInitiateUpgrade(result);
                    if ((this.channelBindingProvider != null) && this.channelBindingProvider.IsChannelBindingSupportEnabled)
                    {
                        this.channel.channelBindingToken = this.channelBindingProvider.GetChannelBinding(this.upgradeInitiator, ChannelBindingKind.Endpoint);
                    }
                    this.remoteSecurity = StreamSecurityUpgradeInitiator.GetRemoteSecurity(this.upgradeInitiator);
                    this.upgradeInitiator = null;
                    if (onWritePreambleEnd == null)
                    {
                        onWritePreambleEnd = Fx.ThunkCallback(new AsyncCallback(StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult.OnWritePreambleEnd));
                    }
                    IAsyncResult result2 = this.connection.BeginWrite(ClientSingletonEncoder.PreambleEndBytes, 0, ClientSingletonEncoder.PreambleEndBytes.Length, true, this.timeoutHelper.RemainingTime(), onWritePreambleEnd, this);
                    if (!result2.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.connection.EndWrite(result2);
                    return this.ReadPreambleAck();
                }

                private bool HandleWritePreamble(IAsyncResult result)
                {
                    this.connection.EndWrite(result);
                    if (this.channel.upgrade == null)
                    {
                        return this.ReadPreambleAck();
                    }
                    this.channelBindingProvider = this.channel.upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                    this.upgradeInitiator = this.channel.upgrade.CreateUpgradeInitiator(this.channel.RemoteAddress, this.channel.Via);
                    if (onUpgrade == null)
                    {
                        onUpgrade = Fx.ThunkCallback(new AsyncCallback(StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult.OnUpgrade));
                    }
                    IAsyncResult result2 = ConnectionUpgradeHelper.BeginInitiateUpgrade(this.channel.settings, this.channel.RemoteAddress, this.connection, this.decoder, this.upgradeInitiator, this.channel.messageEncoder.ContentType, null, this.timeoutHelper, onUpgrade, this);
                    if (!result2.CompletedSynchronously)
                    {
                        return false;
                    }
                    return this.HandleUpgrade(result2);
                }

                private static void OnFailedUpgrade(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult asyncState = (StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            ConnectionUpgradeHelper.EndDecodeFramingFault(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }

                private static void OnReadPreambleAck(object state)
                {
                    bool flag;
                    StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult result = (StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult) state;
                    Exception exception = null;
                    try
                    {
                        flag = result.HandlePreambleAck();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }

                private static void OnUpgrade(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult asyncState = (StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.HandleUpgrade(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void OnWritePreamble(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult asyncState = (StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.HandleWritePreamble(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void OnWritePreambleEnd(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        bool flag;
                        StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult asyncState = (StreamedFramingRequestChannel.StreamedConnectionPoolHelper.SendPreambleAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.connection.EndWrite(result);
                            flag = asyncState.ReadPreambleAck();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            flag = true;
                            exception = exception2;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private bool ReadPreambleAck()
                {
                    if (this.connection.BeginRead(0, 1, this.timeoutHelper.RemainingTime(), onReadPreambleAck, this) == AsyncReadResult.Queued)
                    {
                        return false;
                    }
                    return this.HandlePreambleAck();
                }
            }
        }

        private class StreamedFramingAsyncRequest : AsyncResult, IAsyncRequest, IAsyncResult, IRequestBase
        {
            private StreamedFramingRequestChannel channel;
            private IConnection connection;
            private StreamedFramingRequestChannel.StreamedConnectionPoolHelper connectionPoolHelper;
            private StreamedFramingRequestChannel.ClientSingletonConnectionReader connectionReader;
            private Message message;
            private static AsyncCallback onEstablishConnection = Fx.ThunkCallback(new AsyncCallback(StreamedFramingRequestChannel.StreamedFramingAsyncRequest.OnEstablishConnection));
            private static AsyncCallback onReceiveReply = Fx.ThunkCallback(new AsyncCallback(StreamedFramingRequestChannel.StreamedFramingAsyncRequest.OnReceiveReply));
            private static AsyncCallback onWriteMessage = Fx.ThunkCallback(new AsyncCallback(StreamedFramingRequestChannel.StreamedFramingAsyncRequest.OnWriteMessage));
            private Message replyMessage;
            private TimeoutHelper timeoutHelper;

            public StreamedFramingAsyncRequest(StreamedFramingRequestChannel channel, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                this.connectionPoolHelper = new StreamedFramingRequestChannel.StreamedConnectionPoolHelper(channel);
            }

            public void Abort(RequestChannel requestChannel)
            {
                this.Cleanup();
            }

            public void BeginSendRequest(Message message, TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.message = message;
                bool flag = false;
                bool flag2 = false;
                try
                {
                    try
                    {
                        IAsyncResult result = this.connectionPoolHelper.BeginEstablishConnection(this.timeoutHelper.RemainingTime(), onEstablishConnection, this);
                        if (result.CompletedSynchronously)
                        {
                            flag = this.HandleEstablishConnection(result);
                        }
                    }
                    catch (TimeoutException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnRequest", new object[] { timeout }), exception));
                    }
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        this.Cleanup();
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private void Cleanup()
            {
                this.connectionPoolHelper.Abort();
            }

            private bool CompleteReceiveReply(IAsyncResult result)
            {
                this.replyMessage = this.connectionReader.EndReceive(result);
                if (this.replyMessage != null)
                {
                    ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, this.replyMessage, false);
                }
                return true;
            }

            public Message End()
            {
                try
                {
                    AsyncResult.End<StreamedFramingRequestChannel.StreamedFramingAsyncRequest>(this);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnRequest", new object[] { this.timeoutHelper.OriginalTimeout }), exception));
                }
                return this.replyMessage;
            }

            public void Fault(RequestChannel requestChannel)
            {
                this.Cleanup();
            }

            private bool HandleEstablishConnection(IAsyncResult result)
            {
                this.connection = this.connectionPoolHelper.EndEstablishConnection(result);
                ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, this.message, false);
                IAsyncResult result2 = StreamingConnectionHelper.BeginWriteMessage(this.message, this.connection, true, this.channel.settings, ref this.timeoutHelper, onWriteMessage, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleWriteMessage(result2);
            }

            private bool HandleWriteMessage(IAsyncResult result)
            {
                StreamingConnectionHelper.EndWriteMessage(result);
                this.connectionReader = new StreamedFramingRequestChannel.ClientSingletonConnectionReader(this.connection, this.connectionPoolHelper, this.channel.settings);
                this.connectionReader.DoneSending(TimeSpan.Zero);
                IAsyncResult result2 = this.connectionReader.BeginReceive(this.timeoutHelper.RemainingTime(), onReceiveReply, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.CompleteReceiveReply(result2);
            }

            private static void OnEstablishConnection(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    StreamedFramingRequestChannel.StreamedFramingAsyncRequest asyncState = (StreamedFramingRequestChannel.StreamedFramingAsyncRequest) result.AsyncState;
                    Exception exception = null;
                    bool flag2 = true;
                    try
                    {
                        flag = asyncState.HandleEstablishConnection(result);
                        flag2 = false;
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    finally
                    {
                        if (flag2)
                        {
                            asyncState.Cleanup();
                        }
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnReceiveReply(IAsyncResult result)
            {
                bool flag;
                StreamedFramingRequestChannel.StreamedFramingAsyncRequest asyncState = (StreamedFramingRequestChannel.StreamedFramingAsyncRequest) result.AsyncState;
                Exception exception = null;
                bool flag2 = true;
                try
                {
                    flag = asyncState.CompleteReceiveReply(result);
                    flag2 = false;
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    flag = true;
                    exception = exception2;
                }
                finally
                {
                    if (flag2)
                    {
                        asyncState.Cleanup();
                    }
                }
                if (flag)
                {
                    asyncState.Complete(false, exception);
                }
            }

            private static void OnWriteMessage(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    StreamedFramingRequestChannel.StreamedFramingAsyncRequest asyncState = (StreamedFramingRequestChannel.StreamedFramingAsyncRequest) result.AsyncState;
                    Exception exception = null;
                    bool flag2 = true;
                    try
                    {
                        flag = asyncState.HandleWriteMessage(result);
                        flag2 = false;
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    finally
                    {
                        if (flag2)
                        {
                            asyncState.Cleanup();
                        }
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }

        private class StreamedFramingRequest : IRequest, IRequestBase
        {
            private StreamedFramingRequestChannel channel;
            private IConnection connection;
            private StreamedFramingRequestChannel.StreamedConnectionPoolHelper connectionPoolHelper;

            public StreamedFramingRequest(StreamedFramingRequestChannel channel)
            {
                this.channel = channel;
                this.connectionPoolHelper = new StreamedFramingRequestChannel.StreamedConnectionPoolHelper(channel);
            }

            public void Abort(RequestChannel requestChannel)
            {
                this.Cleanup();
            }

            private void Cleanup()
            {
                this.connectionPoolHelper.Abort();
            }

            public void Fault(RequestChannel requestChannel)
            {
                this.Cleanup();
            }

            public void SendRequest(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                try
                {
                    this.connection = this.connectionPoolHelper.EstablishConnection(timeoutHelper.RemainingTime());
                    ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, message, false);
                    bool flag = false;
                    try
                    {
                        StreamingConnectionHelper.WriteMessage(message, this.connection, true, this.channel.settings, ref timeoutHelper);
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.connectionPoolHelper.Abort();
                        }
                    }
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnRequest", new object[] { timeout }), exception));
                }
            }

            public Message WaitForReply(TimeSpan timeout)
            {
                StreamedFramingRequestChannel.ClientSingletonConnectionReader reader = new StreamedFramingRequestChannel.ClientSingletonConnectionReader(this.connection, this.connectionPoolHelper, this.channel.settings);
                reader.DoneSending(TimeSpan.Zero);
                Message message = reader.Receive(timeout);
                if (message != null)
                {
                    ChannelBindingUtility.TryAddToMessage(this.channel.channelBindingToken, message, false);
                }
                return message;
            }
        }
    }
}

