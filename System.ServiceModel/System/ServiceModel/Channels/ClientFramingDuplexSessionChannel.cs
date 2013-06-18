namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Threading;

    internal class ClientFramingDuplexSessionChannel : FramingDuplexSessionChannel
    {
        private ConnectionPoolHelper connectionPoolHelper;
        private ClientDuplexDecoder decoder;
        private bool flowIdentity;
        private IConnectionOrientedTransportChannelFactorySettings settings;
        private StreamUpgradeProvider upgrade;

        public ClientFramingDuplexSessionChannel(ChannelManagerBase factory, IConnectionOrientedTransportChannelFactorySettings settings, EndpointAddress remoteAddresss, Uri via, IConnectionInitiator connectionInitiator, ConnectionPool connectionPool, bool exposeConnectionProperty, bool flowIdentity) : base(factory, settings, remoteAddresss, via, exposeConnectionProperty)
        {
            this.settings = settings;
            base.MessageEncoder = settings.MessageEncoderFactory.CreateSessionEncoder();
            this.upgrade = settings.Upgrade;
            this.flowIdentity = flowIdentity;
            this.connectionPoolHelper = new DuplexConnectionPoolHelper(this, connectionPool, connectionInitiator);
        }

        private void AcceptConnection(IConnection connection)
        {
            base.SetMessageSource(new ClientDuplexConnectionReader(this, connection, this.decoder, this.settings, base.MessageEncoder));
            lock (base.ThisLock)
            {
                if (base.State != CommunicationState.Opening)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("DuplexChannelAbortedDuringOpen", new object[] { this.Via })));
                }
                base.Connection = connection;
            }
        }

        private IAsyncResult BeginSendPreamble(IConnection connection, ArraySegment<byte> preamble, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
        {
            return new SendPreambleAsyncResult(this, connection, preamble, this.flowIdentity, ref timeoutHelper, callback, state);
        }

        private ArraySegment<byte> CreatePreamble()
        {
            EncodedVia via = new EncodedVia(this.Via.AbsoluteUri);
            EncodedContentType contentType = EncodedContentType.Create(base.MessageEncoder.ContentType);
            int size = ClientDuplexEncoder.ModeBytes.Length + SessionEncoder.CalcStartSize(via, contentType);
            int dstOffset = 0;
            if (this.upgrade == null)
            {
                dstOffset = size;
                size += SessionEncoder.PreambleEndBytes.Length;
            }
            byte[] dst = DiagnosticUtility.Utility.AllocateByteArray(size);
            Buffer.BlockCopy(ClientDuplexEncoder.ModeBytes, 0, dst, 0, ClientDuplexEncoder.ModeBytes.Length);
            SessionEncoder.EncodeStart(dst, ClientDuplexEncoder.ModeBytes.Length, via, contentType);
            if (dstOffset > 0)
            {
                Buffer.BlockCopy(SessionEncoder.PreambleEndBytes, 0, dst, dstOffset, SessionEncoder.PreambleEndBytes.Length);
            }
            return new ArraySegment<byte>(dst, 0, size);
        }

        private IConnection EndSendPreamble(IAsyncResult result)
        {
            return SendPreambleAsyncResult.End(result);
        }

        public override T GetProperty<T>() where T: class
        {
            T property = base.GetProperty<T>();
            if ((property == null) && (this.upgrade != null))
            {
                property = this.upgrade.GetProperty<T>();
            }
            return property;
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            IConnection connection;
            try
            {
                connection = this.connectionPoolHelper.EstablishConnection(timeout);
            }
            catch (TimeoutException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnOpen", new object[] { timeout }), exception));
            }
            bool flag = false;
            try
            {
                this.AcceptConnection(connection);
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

        protected override void PrepareMessage(Message message)
        {
            base.PrepareMessage(message);
            if (base.RemoteSecurity != null)
            {
                message.Properties.Security = (SecurityMessageProperty) base.RemoteSecurity.CreateCopy();
            }
        }

        protected override void ReturnConnectionIfNecessary(bool abort, TimeSpan timeout)
        {
            lock (base.ThisLock)
            {
                if (abort)
                {
                    this.connectionPoolHelper.Abort();
                }
                else
                {
                    this.connectionPoolHelper.Close(timeout);
                }
            }
        }

        private IConnection SendPreamble(IConnection connection, ArraySegment<byte> preamble, ref TimeoutHelper timeoutHelper)
        {
            this.decoder = new ClientDuplexDecoder(0L);
            byte[] buffer = new byte[1];
            connection.Write(preamble.Array, preamble.Offset, preamble.Count, true, timeoutHelper.RemainingTime());
            if (this.upgrade != null)
            {
                IStreamUpgradeChannelBindingProvider property = this.upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                StreamUpgradeInitiator upgradeInitiator = this.upgrade.CreateUpgradeInitiator(this.RemoteAddress, this.Via);
                upgradeInitiator.Open(timeoutHelper.RemainingTime());
                if (!ConnectionUpgradeHelper.InitiateUpgrade(upgradeInitiator, ref connection, this.decoder, this, ref timeoutHelper))
                {
                    ConnectionUpgradeHelper.DecodeFramingFault(this.decoder, connection, this.Via, base.MessageEncoder.ContentType, ref timeoutHelper);
                }
                if ((property != null) && property.IsChannelBindingSupportEnabled)
                {
                    base.SetChannelBinding(property.GetChannelBinding(upgradeInitiator, ChannelBindingKind.Endpoint));
                }
                this.SetRemoteSecurity(upgradeInitiator);
                upgradeInitiator.Close(timeoutHelper.RemainingTime());
                connection.Write(SessionEncoder.PreambleEndBytes, 0, SessionEncoder.PreambleEndBytes.Length, true, timeoutHelper.RemainingTime());
            }
            int count = connection.Read(buffer, 0, buffer.Length, timeoutHelper.RemainingTime());
            if (!ConnectionUpgradeHelper.ValidatePreambleResponse(buffer, count, this.decoder, this.Via))
            {
                ConnectionUpgradeHelper.DecodeFramingFault(this.decoder, connection, this.Via, base.MessageEncoder.ContentType, ref timeoutHelper);
            }
            return connection;
        }

        private void SetRemoteSecurity(StreamUpgradeInitiator upgradeInitiator)
        {
            base.RemoteSecurity = StreamSecurityUpgradeInitiator.GetRemoteSecurity(upgradeInitiator);
        }

        private class DuplexConnectionPoolHelper : ConnectionPoolHelper
        {
            private ClientFramingDuplexSessionChannel channel;
            private ArraySegment<byte> preamble;

            public DuplexConnectionPoolHelper(ClientFramingDuplexSessionChannel channel, ConnectionPool connectionPool, IConnectionInitiator connectionInitiator) : base(connectionPool, connectionInitiator, channel.Via)
            {
                this.channel = channel;
                this.preamble = channel.CreatePreamble();
            }

            protected override IConnection AcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper)
            {
                return this.channel.SendPreamble(connection, this.preamble, ref timeoutHelper);
            }

            protected override IAsyncResult BeginAcceptPooledConnection(IConnection connection, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return this.channel.BeginSendPreamble(connection, this.preamble, ref timeoutHelper, callback, state);
            }

            protected override TimeoutException CreateNewConnectionTimeoutException(TimeSpan timeout, TimeoutException innerException)
            {
                return new TimeoutException(System.ServiceModel.SR.GetString("OpenTimedOutEstablishingTransportSession", new object[] { timeout, this.channel.Via.AbsoluteUri }), innerException);
            }

            protected override IConnection EndAcceptPooledConnection(IAsyncResult result)
            {
                return this.channel.EndSendPreamble(result);
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private ClientFramingDuplexSessionChannel duplexChannel;
            private static AsyncCallback onEstablishConnection = Fx.ThunkCallback(new AsyncCallback(ClientFramingDuplexSessionChannel.OpenAsyncResult.OnEstablishConnection));
            private TimeoutHelper timeoutHelper;

            public OpenAsyncResult(ClientFramingDuplexSessionChannel duplexChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                IAsyncResult result;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.duplexChannel = duplexChannel;
                try
                {
                    result = duplexChannel.connectionPoolHelper.BeginEstablishConnection(this.timeoutHelper.RemainingTime(), onEstablishConnection, this);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnOpen", new object[] { timeout }), exception));
                }
                if (result.CompletedSynchronously && this.HandleEstablishConnection(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ClientFramingDuplexSessionChannel.OpenAsyncResult>(result);
            }

            private bool HandleEstablishConnection(IAsyncResult result)
            {
                IConnection connection;
                try
                {
                    connection = this.duplexChannel.connectionPoolHelper.EndEstablishConnection(result);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("TimeoutOnOpen", new object[] { this.timeoutHelper.OriginalTimeout }), exception));
                }
                this.duplexChannel.AcceptConnection(connection);
                return true;
            }

            private static void OnEstablishConnection(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    ClientFramingDuplexSessionChannel.OpenAsyncResult asyncState = (ClientFramingDuplexSessionChannel.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleEstablishConnection(result);
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
        }

        private class SendPreambleAsyncResult : AsyncResult
        {
            private ClientFramingDuplexSessionChannel channel;
            private IStreamUpgradeChannelBindingProvider channelBindingProvider;
            private IConnection connection;
            private WindowsIdentity identityToImpersonate;
            private static WaitCallback onReadPreambleAck = new WaitCallback(ClientFramingDuplexSessionChannel.SendPreambleAsyncResult.OnReadPreambleAck);
            private static AsyncCallback onUpgrade;
            private static AsyncCallback onUpgradeInitiatorClose;
            private static AsyncCallback onUpgradeInitiatorOpen;
            private static AsyncCallback onWritePreamble = Fx.ThunkCallback(new AsyncCallback(ClientFramingDuplexSessionChannel.SendPreambleAsyncResult.OnWritePreamble));
            private static AsyncCallback onWritePreambleEnd;
            private TimeoutHelper timeoutHelper;
            private StreamUpgradeInitiator upgradeInitiator;

            public SendPreambleAsyncResult(ClientFramingDuplexSessionChannel channel, IConnection connection, ArraySegment<byte> preamble, bool flowIdentity, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                this.timeoutHelper = timeoutHelper;
                this.connection = connection;
                if (flowIdentity && !SecurityContext.IsWindowsIdentityFlowSuppressed())
                {
                    this.identityToImpersonate = WindowsIdentity.GetCurrent(true);
                }
                channel.decoder = new ClientDuplexDecoder(0L);
                IAsyncResult result = connection.BeginWrite(preamble.Array, preamble.Offset, preamble.Count, true, timeoutHelper.RemainingTime(), onWritePreamble, this);
                if (result.CompletedSynchronously && this.HandleWritePreamble(result))
                {
                    base.Complete(true);
                }
            }

            public static IConnection End(IAsyncResult result)
            {
                return AsyncResult.End<ClientFramingDuplexSessionChannel.SendPreambleAsyncResult>(result).connection;
            }

            private bool HandleInitiatorClose(IAsyncResult result)
            {
                this.upgradeInitiator.EndClose(result);
                this.upgradeInitiator = null;
                if (onWritePreambleEnd == null)
                {
                    onWritePreambleEnd = Fx.ThunkCallback(new AsyncCallback(ClientFramingDuplexSessionChannel.SendPreambleAsyncResult.OnWritePreambleEnd));
                }
                IAsyncResult result2 = this.connection.BeginWrite(SessionEncoder.PreambleEndBytes, 0, SessionEncoder.PreambleEndBytes.Length, true, this.timeoutHelper.RemainingTime(), onWritePreambleEnd, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                this.connection.EndWrite(result2);
                return this.ReadAck();
            }

            private bool HandleInitiatorOpen(IAsyncResult result)
            {
                this.upgradeInitiator.EndOpen(result);
                if (onUpgrade == null)
                {
                    onUpgrade = Fx.ThunkCallback(new AsyncCallback(ClientFramingDuplexSessionChannel.SendPreambleAsyncResult.OnUpgrade));
                }
                IAsyncResult result2 = ConnectionUpgradeHelper.BeginInitiateUpgrade(this.channel, this.channel.RemoteAddress, this.connection, this.channel.decoder, this.upgradeInitiator, this.channel.MessageEncoder.ContentType, this.identityToImpersonate, this.timeoutHelper, onUpgrade, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleUpgrade(result2);
            }

            private bool HandlePreambleAck()
            {
                int count = this.connection.EndRead();
                if (!ConnectionUpgradeHelper.ValidatePreambleResponse(this.connection.AsyncReadBuffer, count, this.channel.decoder, this.channel.Via))
                {
                    IAsyncResult result = ConnectionUpgradeHelper.BeginDecodeFramingFault(this.channel.decoder, this.connection, this.channel.Via, this.channel.MessageEncoder.ContentType, ref this.timeoutHelper, Fx.ThunkCallback(new AsyncCallback(this.OnFailedPreamble)), this);
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
                    this.channel.SetChannelBinding(this.channelBindingProvider.GetChannelBinding(this.upgradeInitiator, ChannelBindingKind.Endpoint));
                }
                this.channel.SetRemoteSecurity(this.upgradeInitiator);
                if (onUpgradeInitiatorClose == null)
                {
                    onUpgradeInitiatorClose = Fx.ThunkCallback(new AsyncCallback(ClientFramingDuplexSessionChannel.SendPreambleAsyncResult.OnUpgradeInitiatorClose));
                }
                IAsyncResult result2 = this.upgradeInitiator.BeginClose(this.timeoutHelper.RemainingTime(), onUpgradeInitiatorClose, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleInitiatorClose(result2);
            }

            private bool HandleWritePreamble(IAsyncResult result)
            {
                this.connection.EndWrite(result);
                if (this.channel.upgrade == null)
                {
                    return this.ReadAck();
                }
                this.channelBindingProvider = this.channel.upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                this.upgradeInitiator = this.channel.upgrade.CreateUpgradeInitiator(this.channel.RemoteAddress, this.channel.Via);
                if (onUpgradeInitiatorOpen == null)
                {
                    onUpgradeInitiatorOpen = Fx.ThunkCallback(new AsyncCallback(ClientFramingDuplexSessionChannel.SendPreambleAsyncResult.OnUpgradeInitiatorOpen));
                }
                IAsyncResult result2 = this.upgradeInitiator.BeginOpen(this.timeoutHelper.RemainingTime(), onUpgradeInitiatorOpen, this);
                if (!result2.CompletedSynchronously)
                {
                    return false;
                }
                return this.HandleInitiatorOpen(result2);
            }

            private void OnFailedPreamble(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
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
                    base.Complete(false, exception);
                }
            }

            private static void OnReadPreambleAck(object state)
            {
                bool flag;
                ClientFramingDuplexSessionChannel.SendPreambleAsyncResult result = (ClientFramingDuplexSessionChannel.SendPreambleAsyncResult) state;
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
                    ClientFramingDuplexSessionChannel.SendPreambleAsyncResult asyncState = (ClientFramingDuplexSessionChannel.SendPreambleAsyncResult) result.AsyncState;
                    bool flag = false;
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

            private static void OnUpgradeInitiatorClose(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ClientFramingDuplexSessionChannel.SendPreambleAsyncResult asyncState = (ClientFramingDuplexSessionChannel.SendPreambleAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleInitiatorClose(result);
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

            private static void OnUpgradeInitiatorOpen(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ClientFramingDuplexSessionChannel.SendPreambleAsyncResult asyncState = (ClientFramingDuplexSessionChannel.SendPreambleAsyncResult) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.HandleInitiatorOpen(result);
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
                    ClientFramingDuplexSessionChannel.SendPreambleAsyncResult asyncState = (ClientFramingDuplexSessionChannel.SendPreambleAsyncResult) result.AsyncState;
                    Exception exception = null;
                    bool flag = false;
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
                    ClientFramingDuplexSessionChannel.SendPreambleAsyncResult asyncState = (ClientFramingDuplexSessionChannel.SendPreambleAsyncResult) result.AsyncState;
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        asyncState.connection.EndWrite(result);
                        flag = asyncState.ReadAck();
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

            private bool ReadAck()
            {
                if (this.connection.BeginRead(0, 1, this.timeoutHelper.RemainingTime(), onReadPreambleAck, this) == AsyncReadResult.Queued)
                {
                    return false;
                }
                return this.HandlePreambleAck();
            }
        }
    }
}

