namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    internal class ServerSessionPreambleConnectionReader : InitialServerConnectionReader
    {
        private ServerSessionPreambleCallback callback;
        private byte[] connectionBuffer;
        private ServerSessionDecoder decoder;
        private int offset;
        private IConnection rawConnection;
        private static WaitCallback readCallback;
        private TimeoutHelper receiveTimeoutHelper;
        private IConnectionOrientedTransportFactorySettings settings;
        private int size;
        private TransportSettingsCallback transportSettingsCallback;
        private Uri via;
        private Action<Uri> viaDelegate;

        public ServerSessionPreambleConnectionReader(IConnection connection, Action connectionDequeuedCallback, long streamPosition, int offset, int size, TransportSettingsCallback transportSettingsCallback, ConnectionClosedCallback closedCallback, ServerSessionPreambleCallback callback) : base(connection, closedCallback)
        {
            this.rawConnection = connection;
            this.decoder = new ServerSessionDecoder(streamPosition, base.MaxViaSize, base.MaxContentTypeSize);
            this.offset = offset;
            this.size = size;
            this.transportSettingsCallback = transportSettingsCallback;
            this.callback = callback;
            base.ConnectionDequeuedCallback = connectionDequeuedCallback;
        }

        private void ContinueReading()
        {
            bool flag = false;
            try
            {
                do
                {
                    if (this.size == 0)
                    {
                        if (readCallback == null)
                        {
                            readCallback = new WaitCallback(ServerSessionPreambleConnectionReader.ReadCallback);
                        }
                        if (base.Connection.BeginRead(0, this.connectionBuffer.Length, this.GetRemainingTimeout(), readCallback, this) == AsyncReadResult.Queued)
                        {
                            goto Label_0176;
                        }
                        this.GetReadResult();
                    }
                    int num = this.decoder.Decode(this.connectionBuffer, this.offset, this.size);
                    if (num > 0)
                    {
                        this.offset += num;
                        this.size -= num;
                    }
                }
                while (this.decoder.CurrentState != ServerSessionDecoder.State.PreUpgradeStart);
                this.via = this.decoder.Via;
                if (!base.Connection.Validate(this.via))
                {
                    return;
                }
                if (this.viaDelegate != null)
                {
                    try
                    {
                        this.viaDelegate(this.via);
                    }
                    catch (ServiceActivationException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                        this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/ServiceActivationFailed");
                        goto Label_0176;
                    }
                }
                this.settings = this.transportSettingsCallback(this.via);
                if (this.settings == null)
                {
                    EndpointNotFoundException exception2 = new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { this.decoder.Via }));
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                    this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/EndpointNotFound");
                    return;
                }
                this.callback(this);
            Label_0176:
                flag = true;
            }
            catch (CommunicationException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception4)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                }
            }
            catch (Exception exception5)
            {
                if (Fx.IsFatal(exception5))
                {
                    throw;
                }
                if (!System.ServiceModel.Dispatcher.ExceptionHandler.HandleTransportExceptionHelper(exception5))
                {
                    throw;
                }
            }
            finally
            {
                if (!flag)
                {
                    base.Abort();
                }
            }
        }

        public IDuplexSessionChannel CreateDuplexSessionChannel(ConnectionOrientedTransportChannelListener channelListener, EndpointAddress localAddress, bool exposeConnectionProperty, ConnectionDemuxer connectionDemuxer)
        {
            return new ServerFramingDuplexSessionChannel(channelListener, this, localAddress, exposeConnectionProperty, connectionDemuxer);
        }

        private void GetReadResult()
        {
            this.offset = 0;
            this.size = base.Connection.EndRead();
            if (this.size == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
            }
        }

        private TimeSpan GetRemainingTimeout()
        {
            return this.receiveTimeoutHelper.RemainingTime();
        }

        private static void ReadCallback(object state)
        {
            ServerSessionPreambleConnectionReader reader = (ServerSessionPreambleConnectionReader) state;
            bool flag = false;
            try
            {
                reader.GetReadResult();
                reader.ContinueReading();
                flag = true;
            }
            catch (CommunicationException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (!System.ServiceModel.Dispatcher.ExceptionHandler.HandleTransportExceptionHelper(exception3))
                {
                    throw;
                }
            }
            finally
            {
                if (!flag)
                {
                    reader.Abort();
                }
            }
        }

        public void SendFault(string faultString)
        {
            InitialServerConnectionReader.SendFault(base.Connection, faultString, this.connectionBuffer, this.GetRemainingTimeout(), 0x10000);
            base.Close(this.GetRemainingTimeout());
        }

        public void StartReading(Action<Uri> viaDelegate, TimeSpan receiveTimeout)
        {
            this.viaDelegate = viaDelegate;
            this.receiveTimeoutHelper = new TimeoutHelper(receiveTimeout);
            this.connectionBuffer = base.Connection.AsyncReadBuffer;
            this.ContinueReading();
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

        public ServerSessionDecoder Decoder
        {
            get
            {
                return this.decoder;
            }
        }

        public IConnection RawConnection
        {
            get
            {
                return this.rawConnection;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
        }

        private class ServerFramingDuplexSessionChannel : FramingDuplexSessionChannel
        {
            private IStreamUpgradeChannelBindingProvider channelBindingProvider;
            private ConnectionOrientedTransportChannelListener channelListener;
            private byte[] connectionBuffer;
            private ConnectionDemuxer connectionDemuxer;
            private ServerSessionDecoder decoder;
            private int offset;
            private IConnection rawConnection;
            private ServerSessionConnectionReader sessionReader;
            private int size;
            private StreamUpgradeAcceptor upgradeAcceptor;

            public ServerFramingDuplexSessionChannel(ConnectionOrientedTransportChannelListener channelListener, ServerSessionPreambleConnectionReader preambleReader, EndpointAddress localAddress, bool exposeConnectionProperty, ConnectionDemuxer connectionDemuxer) : base(channelListener, localAddress, preambleReader.Via, exposeConnectionProperty)
            {
                this.channelListener = channelListener;
                this.connectionDemuxer = connectionDemuxer;
                base.Connection = preambleReader.Connection;
                this.decoder = preambleReader.Decoder;
                this.connectionBuffer = preambleReader.connectionBuffer;
                this.offset = preambleReader.BufferOffset;
                this.size = preambleReader.BufferSize;
                this.rawConnection = preambleReader.RawConnection;
                StreamUpgradeProvider upgrade = channelListener.Upgrade;
                if (upgrade != null)
                {
                    this.channelBindingProvider = upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                    this.upgradeAcceptor = upgrade.CreateUpgradeAcceptor();
                }
            }

            private void AcceptUpgradedConnection(IConnection upgradedConnection)
            {
                base.Connection = upgradedConnection;
                if ((this.channelBindingProvider != null) && this.channelBindingProvider.IsChannelBindingSupportEnabled)
                {
                    base.SetChannelBinding(this.channelBindingProvider.GetChannelBinding(this.upgradeAcceptor, ChannelBindingKind.Endpoint));
                }
                this.connectionBuffer = base.Connection.AsyncReadBuffer;
            }

            private void DecodeBytes()
            {
                int num = this.decoder.Decode(this.connectionBuffer, this.offset, this.size);
                if (num > 0)
                {
                    this.offset += num;
                    this.size -= num;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static string GetIdentityNameFromContext(SecurityMessageProperty clientSecurity)
            {
                return System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(clientSecurity.ServiceSecurityContext.AuthorizationContext);
            }

            public override T GetProperty<T>() where T: class
            {
                if (typeof(T) == typeof(IChannelBindingProvider))
                {
                    return (T) this.channelBindingProvider;
                }
                return base.GetProperty<T>();
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
                bool flag = false;
                try
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    this.ValidateContentType(ref timeoutHelper);
                Label_0017:
                    if (this.size == 0)
                    {
                        this.offset = 0;
                        this.size = base.Connection.Read(this.connectionBuffer, 0, this.connectionBuffer.Length, timeoutHelper.RemainingTime());
                        if (this.size == 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
                        }
                    }
                Label_006B:
                    this.DecodeBytes();
                    switch (this.decoder.CurrentState)
                    {
                        case ServerSessionDecoder.State.UpgradeRequest:
                        {
                            this.ProcessUpgradeRequest(ref timeoutHelper);
                            base.Connection.Write(ServerSessionEncoder.UpgradeResponseBytes, 0, ServerSessionEncoder.UpgradeResponseBytes.Length, true, timeoutHelper.RemainingTime());
                            IConnection innerConnection = base.Connection;
                            if (this.size > 0)
                            {
                                innerConnection = new PreReadConnection(innerConnection, this.connectionBuffer, this.offset, this.size);
                            }
                            try
                            {
                                base.Connection = InitialServerConnectionReader.UpgradeConnection(innerConnection, this.upgradeAcceptor, this);
                                if ((this.channelBindingProvider != null) && this.channelBindingProvider.IsChannelBindingSupportEnabled)
                                {
                                    base.SetChannelBinding(this.channelBindingProvider.GetChannelBinding(this.upgradeAcceptor, ChannelBindingKind.Endpoint));
                                }
                                this.connectionBuffer = base.Connection.AsyncReadBuffer;
                                goto Label_018C;
                            }
                            catch (Exception exception)
                            {
                                if (Fx.IsFatal(exception))
                                {
                                    throw;
                                }
                                this.WriteAuditFailure(this.upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception);
                                throw;
                            }
                            break;
                        }
                        case ServerSessionDecoder.State.Start:
                            break;

                        default:
                            goto Label_018C;
                    }
                    this.SetupSecurityIfNecessary();
                    base.Connection.Write(ServerSessionEncoder.AckResponseBytes, 0, ServerSessionEncoder.AckResponseBytes.Length, true, timeoutHelper.RemainingTime());
                    this.SetupSessionReader();
                    flag = true;
                    return;
                Label_018C:
                    if (this.size != 0)
                    {
                        goto Label_006B;
                    }
                    goto Label_0017;
                }
                finally
                {
                    if (!flag)
                    {
                        base.Connection.Abort();
                    }
                }
            }

            protected override void PrepareMessage(Message message)
            {
                this.channelListener.RaiseMessageReceived();
                base.PrepareMessage(message);
            }

            private void ProcessUpgradeRequest(ref TimeoutHelper timeoutHelper)
            {
                if (this.upgradeAcceptor == null)
                {
                    this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/UpgradeInvalid", ref timeoutHelper);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("UpgradeRequestToNonupgradableService", new object[] { this.decoder.Upgrade })));
                }
                if (!this.upgradeAcceptor.CanUpgrade(this.decoder.Upgrade))
                {
                    this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/UpgradeInvalid", ref timeoutHelper);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("UpgradeProtocolNotSupported", new object[] { this.decoder.Upgrade })));
                }
            }

            protected override void ReturnConnectionIfNecessary(bool abort, TimeSpan timeout)
            {
                IConnection rawConnection = null;
                if (this.sessionReader != null)
                {
                    lock (base.ThisLock)
                    {
                        rawConnection = this.sessionReader.GetRawConnection();
                    }
                }
                if (rawConnection != null)
                {
                    if (abort)
                    {
                        rawConnection.Abort();
                    }
                    else
                    {
                        this.connectionDemuxer.ReuseConnection(rawConnection, timeout);
                    }
                    this.connectionDemuxer = null;
                }
            }

            private void SendFault(string faultString, ref TimeoutHelper timeoutHelper)
            {
                InitialServerConnectionReader.SendFault(base.Connection, faultString, this.connectionBuffer, timeoutHelper.RemainingTime(), 0x10000);
            }

            private void SetupSecurityIfNecessary()
            {
                StreamSecurityUpgradeAcceptor upgradeAcceptor = this.upgradeAcceptor as StreamSecurityUpgradeAcceptor;
                if (upgradeAcceptor != null)
                {
                    base.RemoteSecurity = upgradeAcceptor.GetRemoteSecurity();
                    if (base.RemoteSecurity == null)
                    {
                        Exception exception = new ProtocolException(System.ServiceModel.SR.GetString("RemoteSecurityNotNegotiatedOnStreamUpgrade", new object[] { this.Via }));
                        this.WriteAuditFailure(upgradeAcceptor, exception);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                    this.WriteAuditEvent(upgradeAcceptor, AuditLevel.Success, null);
                }
            }

            private void SetupSessionReader()
            {
                this.sessionReader = new ServerSessionConnectionReader(this);
                base.SetMessageSource(this.sessionReader);
            }

            private void ValidateContentType(ref TimeoutHelper timeoutHelper)
            {
                base.MessageEncoder = this.channelListener.MessageEncoderFactory.CreateSessionEncoder();
                if (!base.MessageEncoder.IsContentTypeSupported(this.decoder.ContentType))
                {
                    this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/ContentTypeInvalid", ref timeoutHelper);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("ContentTypeMismatch", new object[] { this.decoder.ContentType, base.MessageEncoder.ContentType })));
                }
            }

            private void WriteAuditEvent(StreamSecurityUpgradeAcceptor securityUpgradeAcceptor, AuditLevel auditLevel, Exception exception)
            {
                if (((this.channelListener.AuditBehavior.MessageAuthenticationAuditLevel & auditLevel) == auditLevel) && (securityUpgradeAcceptor != null))
                {
                    string clientIdentity = string.Empty;
                    SecurityMessageProperty remoteSecurity = securityUpgradeAcceptor.GetRemoteSecurity();
                    if (remoteSecurity != null)
                    {
                        clientIdentity = GetIdentityNameFromContext(remoteSecurity);
                    }
                    ServiceSecurityAuditBehavior auditBehavior = this.channelListener.AuditBehavior;
                    if (auditLevel == AuditLevel.Success)
                    {
                        SecurityAuditHelper.WriteTransportAuthenticationSuccessEvent(auditBehavior.AuditLogLocation, auditBehavior.SuppressAuditFailure, null, base.LocalVia, clientIdentity);
                    }
                    else
                    {
                        SecurityAuditHelper.WriteTransportAuthenticationFailureEvent(auditBehavior.AuditLogLocation, auditBehavior.SuppressAuditFailure, null, base.LocalVia, clientIdentity, exception);
                    }
                }
            }

            private void WriteAuditFailure(StreamSecurityUpgradeAcceptor securityUpgradeAcceptor, Exception exception)
            {
                try
                {
                    this.WriteAuditEvent(securityUpgradeAcceptor, AuditLevel.Failure, exception);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                }
            }

            private class OpenAsyncResult : AsyncResult
            {
                private ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel channel;
                private static AsyncCallback onUpgradeConnection;
                private static AsyncCallback onWriteAckResponse;
                private static AsyncCallback onWriteUpgradeResponse;
                private static WaitCallback readCallback;
                private TimeoutHelper timeoutHelper;

                public OpenAsyncResult(ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.channel = channel;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    bool flag = false;
                    bool flag2 = false;
                    try
                    {
                        channel.ValidateContentType(ref this.timeoutHelper);
                        flag = this.ContinueReading();
                        flag2 = true;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            this.CleanupOnError();
                        }
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }

                private void CleanupOnError()
                {
                    this.channel.Connection.Abort();
                }

                private bool ContinueReading()
                {
                    while (true)
                    {
                        if (this.channel.size == 0)
                        {
                            if (readCallback == null)
                            {
                                readCallback = new WaitCallback(ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult.ReadCallback);
                            }
                            if (this.channel.Connection.BeginRead(0, this.channel.connectionBuffer.Length, this.timeoutHelper.RemainingTime(), readCallback, this) == AsyncReadResult.Queued)
                            {
                                return false;
                            }
                            this.GetReadResult();
                        }
                        do
                        {
                            this.channel.DecodeBytes();
                            switch (this.channel.decoder.CurrentState)
                            {
                                case ServerSessionDecoder.State.UpgradeRequest:
                                {
                                    this.channel.ProcessUpgradeRequest(ref this.timeoutHelper);
                                    if (onWriteUpgradeResponse == null)
                                    {
                                        onWriteUpgradeResponse = Fx.ThunkCallback(new AsyncCallback(ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult.OnWriteUpgradeResponse));
                                    }
                                    IAsyncResult result = this.channel.Connection.BeginWrite(ServerSessionEncoder.UpgradeResponseBytes, 0, ServerSessionEncoder.UpgradeResponseBytes.Length, true, this.timeoutHelper.RemainingTime(), onWriteUpgradeResponse, this);
                                    if (result.CompletedSynchronously && this.HandleWriteUpgradeResponseComplete(result))
                                    {
                                        break;
                                    }
                                    return false;
                                }
                                case ServerSessionDecoder.State.Start:
                                {
                                    this.channel.SetupSecurityIfNecessary();
                                    if (onWriteAckResponse == null)
                                    {
                                        onWriteAckResponse = Fx.ThunkCallback(new AsyncCallback(ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult.OnWriteAckResponse));
                                    }
                                    IAsyncResult result2 = this.channel.Connection.BeginWrite(ServerSessionEncoder.AckResponseBytes, 0, ServerSessionEncoder.AckResponseBytes.Length, true, this.timeoutHelper.RemainingTime(), onWriteAckResponse, this);
                                    if (!result2.CompletedSynchronously)
                                    {
                                        return false;
                                    }
                                    return this.HandleWriteAckComplete(result2);
                                }
                            }
                        }
                        while (this.channel.size != 0);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult>(result);
                }

                private void GetReadResult()
                {
                    this.channel.offset = 0;
                    this.channel.size = this.channel.Connection.EndRead();
                    if (this.channel.size == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.channel.decoder.CreatePrematureEOFException());
                    }
                }

                private bool HandleUpgradeConnectionComplete(IAsyncResult result)
                {
                    this.channel.AcceptUpgradedConnection(InitialServerConnectionReader.EndUpgradeConnection(result));
                    return true;
                }

                private bool HandleWriteAckComplete(IAsyncResult result)
                {
                    this.channel.Connection.EndWrite(result);
                    this.channel.SetupSessionReader();
                    return true;
                }

                private bool HandleWriteUpgradeResponseComplete(IAsyncResult result)
                {
                    bool flag;
                    this.channel.Connection.EndWrite(result);
                    IConnection innerConnection = this.channel.Connection;
                    if (this.channel.size > 0)
                    {
                        innerConnection = new PreReadConnection(innerConnection, this.channel.connectionBuffer, this.channel.offset, this.channel.size);
                    }
                    if (onUpgradeConnection == null)
                    {
                        onUpgradeConnection = Fx.ThunkCallback(new AsyncCallback(ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult.OnUpgradeConnection));
                    }
                    try
                    {
                        IAsyncResult result2 = InitialServerConnectionReader.BeginUpgradeConnection(innerConnection, this.channel.upgradeAcceptor, this.channel, onUpgradeConnection, this);
                        if (!result2.CompletedSynchronously)
                        {
                            return false;
                        }
                        flag = this.HandleUpgradeConnectionComplete(result2);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.channel.WriteAuditFailure(this.channel.upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception);
                        throw;
                    }
                    return flag;
                }

                private static void OnUpgradeConnection(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult asyncState = (ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.HandleUpgradeConnectionComplete(result);
                            if (flag)
                            {
                                flag = asyncState.ContinueReading();
                            }
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                            flag = true;
                            asyncState.CleanupOnError();
                            asyncState.channel.WriteAuditFailure(asyncState.channel.upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception2);
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void OnWriteAckResponse(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult asyncState = (ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.HandleWriteAckComplete(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                            flag = true;
                            asyncState.CleanupOnError();
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void OnWriteUpgradeResponse(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult asyncState = (ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult) result.AsyncState;
                        bool flag = false;
                        Exception exception = null;
                        try
                        {
                            flag = asyncState.HandleWriteUpgradeResponseComplete(result);
                            if (flag)
                            {
                                flag = asyncState.ContinueReading();
                            }
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                            flag = true;
                            asyncState.CleanupOnError();
                            asyncState.channel.WriteAuditFailure(asyncState.channel.upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception2);
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private static void ReadCallback(object state)
                {
                    ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult result = (ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel.OpenAsyncResult) state;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        result.GetReadResult();
                        flag = result.ContinueReading();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                        result.CleanupOnError();
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            private class ServerSessionConnectionReader : SessionConnectionReader
            {
                private BufferManager bufferManager;
                private string contentType;
                private ServerSessionDecoder decoder;
                private int maxBufferSize;
                private MessageEncoder messageEncoder;
                private IConnection rawConnection;

                public ServerSessionConnectionReader(ServerSessionPreambleConnectionReader.ServerFramingDuplexSessionChannel channel) : base(channel.Connection, channel.rawConnection, channel.offset, channel.size, channel.RemoteSecurity)
                {
                    this.decoder = channel.decoder;
                    this.contentType = this.decoder.ContentType;
                    this.maxBufferSize = channel.channelListener.MaxBufferSize;
                    this.bufferManager = channel.channelListener.BufferManager;
                    this.messageEncoder = channel.MessageEncoder;
                    this.rawConnection = channel.rawConnection;
                }

                protected override Message DecodeMessage(byte[] buffer, ref int offset, ref int size, ref bool isAtEof, TimeSpan timeout)
                {
                    while (!isAtEof && (size > 0))
                    {
                        int envelopeSize;
                        int count = this.decoder.Decode(buffer, offset, size);
                        if (count > 0)
                        {
                            if (base.EnvelopeBuffer != null)
                            {
                                if (!object.ReferenceEquals(buffer, base.EnvelopeBuffer))
                                {
                                    Buffer.BlockCopy(buffer, offset, base.EnvelopeBuffer, base.EnvelopeOffset, count);
                                }
                                base.EnvelopeOffset += count;
                            }
                            offset += count;
                            size -= count;
                        }
                        switch (this.decoder.CurrentState)
                        {
                            case ServerSessionDecoder.State.EnvelopeStart:
                                envelopeSize = this.decoder.EnvelopeSize;
                                if (envelopeSize > this.maxBufferSize)
                                {
                                    base.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/MaxMessageSizeExceededFault", timeout);
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException((long) this.maxBufferSize));
                                }
                                break;

                            case ServerSessionDecoder.State.ReadingEnvelopeBytes:
                            case ServerSessionDecoder.State.ReadingEndRecord:
                            {
                                continue;
                            }
                            case ServerSessionDecoder.State.EnvelopeEnd:
                            {
                                if (base.EnvelopeBuffer == null)
                                {
                                    continue;
                                }
                                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity(true) : null)
                                {
                                    if (DiagnosticUtility.ShouldUseActivity)
                                    {
                                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityProcessingMessage", new object[] { TraceUtility.RetrieveMessageNumber() }), ActivityType.ProcessMessage);
                                    }
                                    Message message = null;
                                    try
                                    {
                                        message = this.messageEncoder.ReadMessage(new ArraySegment<byte>(base.EnvelopeBuffer, 0, base.EnvelopeSize), this.bufferManager, this.contentType);
                                    }
                                    catch (XmlException exception)
                                    {
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageXmlProtocolError"), exception));
                                    }
                                    if (DiagnosticUtility.ShouldUseActivity)
                                    {
                                        TraceUtility.TransferFromTransport(message);
                                    }
                                    base.EnvelopeBuffer = null;
                                    return message;
                                }
                            }
                            case ServerSessionDecoder.State.End:
                                goto Label_01A8;

                            default:
                            {
                                continue;
                            }
                        }
                        base.EnvelopeBuffer = this.bufferManager.TakeBuffer(envelopeSize);
                        base.EnvelopeOffset = 0;
                        base.EnvelopeSize = envelopeSize;
                        continue;
                    Label_01A8:
                        isAtEof = true;
                    }
                    return null;
                }

                protected override void EnsureDecoderAtEof()
                {
                    if ((this.decoder.CurrentState != ServerSessionDecoder.State.End) && (this.decoder.CurrentState != ServerSessionDecoder.State.EnvelopeEnd))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
                    }
                }

                protected override void PrepareMessage(Message message)
                {
                    base.PrepareMessage(message);
                    IPEndPoint remoteIPEndPoint = this.rawConnection.RemoteIPEndPoint;
                    if (remoteIPEndPoint != null)
                    {
                        RemoteEndpointMessageProperty property = new RemoteEndpointMessageProperty(remoteIPEndPoint);
                        message.Properties.Add(RemoteEndpointMessageProperty.Name, property);
                    }
                }
            }
        }
    }
}

