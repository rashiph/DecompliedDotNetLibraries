namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;

    internal class ServerSingletonPreambleConnectionReader : InitialServerConnectionReader
    {
        private ServerSingletonPreambleCallback callback;
        private System.Security.Authentication.ExtendedProtection.ChannelBinding channelBindingToken;
        private byte[] connectionBuffer;
        private ServerSingletonDecoder decoder;
        private bool isReadPending;
        private int offset;
        private WaitCallback onAsyncReadComplete;
        private IConnection rawConnection;
        private TimeoutHelper receiveTimeoutHelper;
        private SecurityMessageProperty security;
        private int size;
        private IConnectionOrientedTransportFactorySettings transportSettings;
        private TransportSettingsCallback transportSettingsCallback;
        private Uri via;
        private Action<Uri> viaDelegate;

        public ServerSingletonPreambleConnectionReader(IConnection connection, Action connectionDequeuedCallback, long streamPosition, int offset, int size, TransportSettingsCallback transportSettingsCallback, ConnectionClosedCallback closedCallback, ServerSingletonPreambleCallback callback) : base(connection, closedCallback)
        {
            this.decoder = new ServerSingletonDecoder(streamPosition, base.MaxViaSize, base.MaxContentTypeSize);
            this.offset = offset;
            this.size = size;
            this.callback = callback;
            this.transportSettingsCallback = transportSettingsCallback;
            this.rawConnection = connection;
            base.ConnectionDequeuedCallback = connectionDequeuedCallback;
        }

        public IConnection CompletePreamble(TimeSpan timeout)
        {
            int num;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.transportSettings.MessageEncoderFactory.Encoder.IsContentTypeSupported(this.decoder.ContentType))
            {
                this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/ContentTypeInvalid", ref timeoutHelper);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("ContentTypeMismatch", new object[] { this.decoder.ContentType, this.transportSettings.MessageEncoderFactory.Encoder.ContentType })));
            }
            StreamUpgradeAcceptor upgradeAcceptor = null;
            StreamUpgradeProvider upgrade = this.transportSettings.Upgrade;
            IStreamUpgradeChannelBindingProvider property = null;
            if (upgrade != null)
            {
                property = upgrade.GetProperty<IStreamUpgradeChannelBindingProvider>();
                upgradeAcceptor = upgrade.CreateUpgradeAcceptor();
            }
            IConnection connection = base.Connection;
        Label_00B1:
            if (this.size == 0)
            {
                this.offset = 0;
                this.size = connection.Read(this.connectionBuffer, 0, this.connectionBuffer.Length, timeoutHelper.RemainingTime());
                if (this.size == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
                }
            }
        Label_0101:
            num = this.decoder.Decode(this.connectionBuffer, this.offset, this.size);
            if (num > 0)
            {
                this.offset += num;
                this.size -= num;
            }
            switch (this.decoder.CurrentState)
            {
                case ServerSingletonDecoder.State.UpgradeRequest:
                {
                    if (upgradeAcceptor == null)
                    {
                        this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/UpgradeInvalid", ref timeoutHelper);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("UpgradeRequestToNonupgradableService", new object[] { this.decoder.Upgrade })));
                    }
                    if (!upgradeAcceptor.CanUpgrade(this.decoder.Upgrade))
                    {
                        this.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/UpgradeInvalid", ref timeoutHelper);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("UpgradeProtocolNotSupported", new object[] { this.decoder.Upgrade })));
                    }
                    connection.Write(ServerSingletonEncoder.UpgradeResponseBytes, 0, ServerSingletonEncoder.UpgradeResponseBytes.Length, true, timeoutHelper.RemainingTime());
                    IConnection innerConnection = connection;
                    if (this.size > 0)
                    {
                        innerConnection = new PreReadConnection(innerConnection, this.connectionBuffer, this.offset, this.size);
                    }
                    try
                    {
                        connection = InitialServerConnectionReader.UpgradeConnection(innerConnection, upgradeAcceptor, this.transportSettings);
                        this.connectionBuffer = connection.AsyncReadBuffer;
                        if ((property != null) && property.IsChannelBindingSupportEnabled)
                        {
                            this.channelBindingToken = property.GetChannelBinding(upgradeAcceptor, ChannelBindingKind.Endpoint);
                        }
                        goto Label_02C0;
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.WriteAuditFailure(upgradeAcceptor as StreamSecurityUpgradeAcceptor, exception);
                        throw;
                    }
                    break;
                }
                case ServerSingletonDecoder.State.Start:
                    break;

                default:
                    goto Label_02C0;
            }
            this.SetupSecurityIfNecessary(upgradeAcceptor);
            connection.Write(ServerSessionEncoder.AckResponseBytes, 0, ServerSessionEncoder.AckResponseBytes.Length, true, timeoutHelper.RemainingTime());
            return connection;
        Label_02C0:
            if (this.size != 0)
            {
                goto Label_0101;
            }
            goto Label_00B1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetIdentityNameFromContext(SecurityMessageProperty clientSecurity)
        {
            return System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(clientSecurity.ServiceSecurityContext.AuthorizationContext);
        }

        private TimeSpan GetRemainingTimeout()
        {
            return this.receiveTimeoutHelper.RemainingTime();
        }

        private void HandleReadComplete()
        {
            this.offset = 0;
            this.size = base.Connection.EndRead();
            this.isReadPending = false;
            if (this.size == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.decoder.CreatePrematureEOFException());
            }
        }

        private void OnAsyncReadComplete(object state)
        {
            bool flag = false;
            try
            {
                this.HandleReadComplete();
                this.ReadAndDispatch();
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
                    base.Abort();
                }
            }
        }

        private void ReadAndDispatch()
        {
            bool flag = false;
            try
            {
                while (((this.size > 0) || !this.isReadPending) && !base.IsClosed)
                {
                    if (this.size == 0)
                    {
                        this.isReadPending = true;
                        if (this.onAsyncReadComplete == null)
                        {
                            this.onAsyncReadComplete = new WaitCallback(this.OnAsyncReadComplete);
                        }
                        if (base.Connection.BeginRead(0, this.connectionBuffer.Length, this.GetRemainingTimeout(), this.onAsyncReadComplete, null) == AsyncReadResult.Queued)
                        {
                            break;
                        }
                        this.HandleReadComplete();
                    }
                    int num = this.decoder.Decode(this.connectionBuffer, this.offset, this.size);
                    if (num > 0)
                    {
                        this.offset += num;
                        this.size -= num;
                    }
                    if (this.decoder.CurrentState == ServerSingletonDecoder.State.PreUpgradeStart)
                    {
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
                                break;
                            }
                        }
                        this.transportSettings = this.transportSettingsCallback(this.via);
                        if (this.transportSettings == null)
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
                        break;
                    }
                }
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

        public void SendFault(string faultString)
        {
            this.SendFault(faultString, ref this.receiveTimeoutHelper);
        }

        private void SendFault(string faultString, ref TimeoutHelper timeoutHelper)
        {
            InitialServerConnectionReader.SendFault(base.Connection, faultString, this.connectionBuffer, timeoutHelper.RemainingTime(), 0x10000);
        }

        private void SetupSecurityIfNecessary(StreamUpgradeAcceptor upgradeAcceptor)
        {
            StreamSecurityUpgradeAcceptor securityUpgradeAcceptor = upgradeAcceptor as StreamSecurityUpgradeAcceptor;
            if (securityUpgradeAcceptor != null)
            {
                this.security = securityUpgradeAcceptor.GetRemoteSecurity();
                if (this.security == null)
                {
                    Exception exception = new ProtocolException(System.ServiceModel.SR.GetString("RemoteSecurityNotNegotiatedOnStreamUpgrade", new object[] { this.Via }));
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
                this.WriteAuditEvent(securityUpgradeAcceptor, AuditLevel.Success, null);
            }
        }

        public void StartReading(Action<Uri> viaDelegate, TimeSpan timeout)
        {
            this.viaDelegate = viaDelegate;
            this.receiveTimeoutHelper = new TimeoutHelper(timeout);
            this.connectionBuffer = base.Connection.AsyncReadBuffer;
            this.ReadAndDispatch();
        }

        private void WriteAuditEvent(StreamSecurityUpgradeAcceptor securityUpgradeAcceptor, AuditLevel auditLevel, Exception exception)
        {
            if (((this.transportSettings.AuditBehavior.MessageAuthenticationAuditLevel & auditLevel) == auditLevel) && (securityUpgradeAcceptor != null))
            {
                string clientIdentity = string.Empty;
                SecurityMessageProperty remoteSecurity = securityUpgradeAcceptor.GetRemoteSecurity();
                if (remoteSecurity != null)
                {
                    clientIdentity = GetIdentityNameFromContext(remoteSecurity);
                }
                ServiceSecurityAuditBehavior auditBehavior = this.transportSettings.AuditBehavior;
                if (auditLevel == AuditLevel.Success)
                {
                    SecurityAuditHelper.WriteTransportAuthenticationSuccessEvent(auditBehavior.AuditLogLocation, auditBehavior.SuppressAuditFailure, null, this.Via, clientIdentity);
                }
                else
                {
                    SecurityAuditHelper.WriteTransportAuthenticationFailureEvent(auditBehavior.AuditLogLocation, auditBehavior.SuppressAuditFailure, null, this.Via, clientIdentity, exception);
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

        public System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return this.channelBindingToken;
            }
        }

        public ServerSingletonDecoder Decoder
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

        public SecurityMessageProperty Security
        {
            get
            {
                return this.security;
            }
        }

        public IConnectionOrientedTransportFactorySettings TransportSettings
        {
            get
            {
                return this.transportSettings;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
        }
    }
}

