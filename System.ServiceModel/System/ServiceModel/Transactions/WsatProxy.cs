namespace System.ServiceModel.Transactions
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    internal class WsatProxy
    {
        private ActivationProxy activationProxy;
        private CoordinationService coordinationService;
        private static byte[] fixedPropagationToken;
        private ProtocolVersion protocolVersion;
        private object proxyLock = new object();
        private WsatConfiguration wsatConfig;

        public WsatProxy(WsatConfiguration wsatConfig, ProtocolVersion protocolVersion)
        {
            this.wsatConfig = wsatConfig;
            this.protocolVersion = protocolVersion;
        }

        private static ProxyIsolationLevel ConvertIsolationLevel(IsolationLevel IsolationLevel)
        {
            switch (IsolationLevel)
            {
                case IsolationLevel.Serializable:
                    return ProxyIsolationLevel.Serializable;

                case IsolationLevel.RepeatableRead:
                    return ProxyIsolationLevel.RepeatableRead;

                case IsolationLevel.ReadCommitted:
                    return ProxyIsolationLevel.CursorStability;

                case IsolationLevel.ReadUncommitted:
                    return ProxyIsolationLevel.ReadUncommitted;

                case IsolationLevel.Unspecified:
                    return ProxyIsolationLevel.Unspecified;
            }
            return ProxyIsolationLevel.Serializable;
        }

        private ActivationProxy CreateActivationProxy(EndpointAddress address)
        {
            ActivationProxy proxy;
            CoordinationService coordinationService = this.GetCoordinationService();
            try
            {
                proxy = coordinationService.CreateActivationProxy(address, false);
            }
            catch (CreateChannelFailureException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("WsatProxyCreationFailed"), exception));
            }
            return proxy;
        }

        private CoordinationContext CreateCoordinationContext(WsatTransactionInfo info)
        {
            CoordinationContext coordinationContext;
            Microsoft.Transactions.Wsat.Messaging.CreateCoordinationContext cccMessage = new Microsoft.Transactions.Wsat.Messaging.CreateCoordinationContext(this.protocolVersion) {
                CurrentContext = info.Context,
                IssuedToken = info.IssuedToken
            };
            try
            {
                using (new OperationContextScope(null))
                {
                    coordinationContext = this.Enlist(ref cccMessage).CoordinationContext;
                }
            }
            catch (WsatFaultException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("UnmarshalTransactionFaulted", new object[] { exception.Message }), exception));
            }
            catch (WsatSendFailureException exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionManagerCommunicationException(System.ServiceModel.SR.GetString("TMCommunicationError"), exception2));
            }
            return coordinationContext;
        }

        private static byte[] CreateFixedPropagationToken()
        {
            if (fixedPropagationToken == null)
            {
                CommittableTransaction transaction = new CommittableTransaction();
                byte[] transmitterPropagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
                try
                {
                    transaction.Commit();
                }
                catch (TransactionException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                Interlocked.CompareExchange<byte[]>(ref fixedPropagationToken, transmitterPropagationToken, null);
            }
            byte[] destinationArray = new byte[fixedPropagationToken.Length];
            Array.Copy(fixedPropagationToken, destinationArray, fixedPropagationToken.Length);
            return destinationArray;
        }

        private CreateCoordinationContextResponse Enlist(ref Microsoft.Transactions.Wsat.Messaging.CreateCoordinationContext cccMessage)
        {
            int num = 0;
            while (true)
            {
                ActivationProxy activationProxy = this.GetActivationProxy();
                EndpointAddress to = activationProxy.To;
                EndpointAddress objB = this.wsatConfig.LocalActivationService(this.protocolVersion);
                EndpointAddress address3 = this.wsatConfig.RemoteActivationService(this.protocolVersion);
                try
                {
                    return activationProxy.SendCreateCoordinationContext(ref cccMessage);
                }
                catch (WsatSendFailureException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    Exception innerException = exception.InnerException;
                    if (((innerException is TimeoutException) || (innerException is QuotaExceededException)) || (innerException is FaultException))
                    {
                        throw;
                    }
                    if (num > 10)
                    {
                        throw;
                    }
                    if (((num > 5) && (address3 != null)) && object.ReferenceEquals(to, objB))
                    {
                        to = address3;
                    }
                }
                finally
                {
                    activationProxy.Release();
                }
                this.TryStartMsdtcService();
                this.RefreshActivationProxy(to);
                Thread.Sleep(0);
                num++;
            }
        }

        private ActivationProxy GetActivationProxy()
        {
            if (this.activationProxy == null)
            {
                this.RefreshActivationProxy(null);
            }
            lock (this.proxyLock)
            {
                ActivationProxy activationProxy = this.activationProxy;
                activationProxy.AddRef();
                return activationProxy;
            }
        }

        private CoordinationService GetCoordinationService()
        {
            if (this.coordinationService == null)
            {
                lock (this.proxyLock)
                {
                    if (this.coordinationService == null)
                    {
                        try
                        {
                            CoordinationServiceConfiguration config = new CoordinationServiceConfiguration {
                                Mode = CoordinationServiceMode.Formatter,
                                RemoteClientsEnabled = this.wsatConfig.RemoteActivationService(this.protocolVersion) != null
                            };
                            this.coordinationService = new CoordinationService(config, this.protocolVersion);
                        }
                        catch (MessagingInitializationException exception)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("WsatMessagingInitializationFailed"), exception));
                        }
                    }
                }
            }
            return this.coordinationService;
        }

        private static byte[] MarshalPropagationToken(ref Guid transactionId, IsolationLevel isoLevel, IsolationFlags isoFlags, string description)
        {
            byte[] destinationArray = CreateFixedPropagationToken();
            byte[] sourceArray = transactionId.ToByteArray();
            Array.Copy(sourceArray, 0, destinationArray, 8, sourceArray.Length);
            byte[] bytes = BitConverter.GetBytes((int) ConvertIsolationLevel(isoLevel));
            Array.Copy(bytes, 0, destinationArray, 0x18, bytes.Length);
            byte[] buffer4 = BitConverter.GetBytes((int) isoFlags);
            Array.Copy(buffer4, 0, destinationArray, 0x1c, buffer4.Length);
            if (!string.IsNullOrEmpty(description))
            {
                byte[] buffer5 = Encoding.UTF8.GetBytes(description);
                int length = Math.Min(buffer5.Length, 0x27);
                Array.Copy(buffer5, 0, destinationArray, 0x24, length);
                destinationArray[0x24 + length] = 0;
            }
            return destinationArray;
        }

        private void RefreshActivationProxy(EndpointAddress suggestedAddress)
        {
            EndpointAddress address = suggestedAddress;
            if (address == null)
            {
                address = this.wsatConfig.LocalActivationService(this.protocolVersion);
                if (address == null)
                {
                    address = this.wsatConfig.RemoteActivationService(this.protocolVersion);
                }
            }
            if (address == null)
            {
                DiagnosticUtility.FailFast("Must have valid activation service address");
            }
            lock (this.proxyLock)
            {
                ActivationProxy proxy = this.CreateActivationProxy(address);
                if (this.activationProxy != null)
                {
                    this.activationProxy.Release();
                }
                this.activationProxy = proxy;
            }
        }

        private void TryStartMsdtcService()
        {
            try
            {
                TransactionInterop.GetWhereabouts();
            }
            catch (TransactionException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
            }
        }

        public Transaction UnmarshalTransaction(WsatTransactionInfo info)
        {
            if (info.Context.ProtocolVersion != this.protocolVersion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
            }
            if (this.wsatConfig.OleTxUpgradeEnabled)
            {
                byte[] propagationToken = info.Context.PropagationToken;
                if (propagationToken != null)
                {
                    try
                    {
                        return OleTxTransactionInfo.UnmarshalPropagationToken(propagationToken);
                    }
                    catch (TransactionException exception)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, 0xe000e, System.ServiceModel.SR.GetString("TraceCodeTxFailedToNegotiateOleTx", new object[] { info.Context.Identifier }));
                    }
                }
            }
            CoordinationContext context = info.Context;
            if (!this.wsatConfig.IsLocalRegistrationService(context.RegistrationService, this.protocolVersion))
            {
                if (!this.wsatConfig.IsProtocolServiceEnabled(this.protocolVersion))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("WsatProtocolServiceDisabled", new object[] { this.protocolVersion })));
                }
                if (!this.wsatConfig.InboundEnabled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("InboundTransactionsDisabled")));
                }
                if (this.wsatConfig.IsDisabledRegistrationService(context.RegistrationService))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("SourceTransactionsDisabled")));
                }
                context = this.CreateCoordinationContext(info);
            }
            Guid localTransactionId = context.LocalTransactionId;
            if (localTransactionId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("InvalidCoordinationContextTransactionId")));
            }
            return OleTxTransactionInfo.UnmarshalPropagationToken(MarshalPropagationToken(ref localTransactionId, context.IsolationLevel, context.IsolationFlags, context.Description));
        }

        private enum ProxyIsolationLevel
        {
            Browse = 0x100,
            Chaos = 0x10,
            CursorStability = 0x1000,
            Isolated = 0x100000,
            ReadCommitted = 0x1000,
            ReadUncommitted = 0x100,
            RepeatableRead = 0x10000,
            Serializable = 0x100000,
            Unspecified = -1
        }
    }
}

