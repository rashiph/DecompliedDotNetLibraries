namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.ServiceModel.Transactions;
    using System.Transactions;

    internal abstract class TransactionChannel<TChannel> : LayeredChannel<TChannel>, ITransactionChannel where TChannel: class, IChannel
    {
        private ITransactionChannelManager factory;
        private TransactionFormatter formatter;

        protected TransactionChannel(ChannelManagerBase channelManager, TChannel innerChannel) : base(channelManager, innerChannel)
        {
            this.factory = (ITransactionChannelManager) channelManager;
            if (this.factory.TransactionProtocol == TransactionProtocol.OleTransactions)
            {
                this.formatter = TransactionFormatter.OleTxFormatter;
            }
            else if (this.factory.TransactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004)
            {
                this.formatter = TransactionFormatter.WsatFormatter10;
            }
            else
            {
                if (this.factory.TransactionProtocol != TransactionProtocol.WSAtomicTransaction11)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxBadTransactionProtocols")));
                }
                this.formatter = TransactionFormatter.WsatFormatter11;
            }
        }

        private void FaultOnMessage(Message message, string reason, string codeString)
        {
            FaultCode code = FaultCode.CreateSenderFaultCode(codeString, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions");
            FaultException exception = new FaultException(reason, code, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault");
            throw TraceUtility.ThrowHelperError(exception, message);
        }

        private static bool Found(int index)
        {
            return (index != -1);
        }

        public T GetInnerProperty<T>() where T: class
        {
            return base.InnerChannel.GetProperty<T>();
        }

        private ICollection<RequestSecurityTokenResponse> GetIssuedTokens(Message message)
        {
            return IssuedTokensHeader.ExtractIssuances(message, this.factory.StandardsManager, message.Version.Envelope.UltimateDestinationActorValues, null);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(FaultConverter))
            {
                return (T) new TransactionChannelFaultConverter<TChannel>((TransactionChannel<TChannel>) this);
            }
            return base.GetProperty<T>();
        }

        public void ReadIssuedTokens(Message message, MessageDirection direction)
        {
            TransactionFlowOption flowIssuedTokens = this.factory.FlowIssuedTokens;
            ICollection<RequestSecurityTokenResponse> issuedTokens = this.GetIssuedTokens(message);
            if ((issuedTokens != null) && (issuedTokens.Count != 0))
            {
                if (flowIssuedTokens == TransactionFlowOption.NotAllowed)
                {
                    this.FaultOnMessage(message, System.ServiceModel.SR.GetString("IssuedTokenFlowNotAllowed"), "IssuedTokenFlowNotAllowed");
                }
                foreach (RequestSecurityTokenResponse response in issuedTokens)
                {
                    TransactionFlowProperty.Ensure(message).IssuedTokens.Add(response);
                }
            }
        }

        public virtual void ReadTransactionDataFromMessage(Message message, MessageDirection direction)
        {
            this.ReadIssuedTokens(message, direction);
            TransactionFlowOption transaction = this.factory.GetTransaction(direction, message.Headers.Action);
            if (TransactionFlowOptionHelper.AllowedOrRequired(transaction))
            {
                this.ReadTransactionFromMessage(message, transaction);
            }
        }

        private void ReadTransactionFromMessage(Message message, TransactionFlowOption txFlowOption)
        {
            TransactionInfo transactionInfo = null;
            try
            {
                transactionInfo = this.formatter.ReadTransaction(message);
            }
            catch (TransactionException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                this.FaultOnMessage(message, System.ServiceModel.SR.GetString("SFxTransactionDeserializationFailed", new object[] { exception.Message }), "TransactionHeaderMalformed");
            }
            if (transactionInfo != null)
            {
                TransactionMessageProperty.Set(transactionInfo, message);
            }
            else if (txFlowOption == TransactionFlowOption.Mandatory)
            {
                this.FaultOnMessage(message, System.ServiceModel.SR.GetString("SFxTransactionFlowRequired"), "TransactionHeaderMissing");
            }
        }

        public void WriteIssuedTokens(Message message, MessageDirection direction)
        {
            ICollection<RequestSecurityTokenResponse> tokenIssuances = TransactionFlowProperty.TryGetIssuedTokens(message);
            if (tokenIssuances != null)
            {
                IssuedTokensHeader header = new IssuedTokensHeader(tokenIssuances, this.factory.StandardsManager);
                message.Headers.Add(header);
            }
        }

        public void WriteTransactionDataToMessage(Message message, MessageDirection direction)
        {
            TransactionFlowOption transaction = this.factory.GetTransaction(direction, message.Headers.Action);
            if (TransactionFlowOptionHelper.AllowedOrRequired(transaction))
            {
                this.WriteTransactionToMessage(message, transaction);
            }
            if (TransactionFlowOptionHelper.AllowedOrRequired(this.factory.FlowIssuedTokens))
            {
                this.WriteIssuedTokens(message, direction);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteTransactionToMessage(Message message, TransactionFlowOption txFlowOption)
        {
            Transaction transaction = TransactionFlowProperty.TryGetTransaction(message);
            if (transaction != null)
            {
                try
                {
                    this.formatter.WriteTransaction(transaction, message);
                    return;
                }
                catch (TransactionException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(exception.Message, exception));
                }
            }
            if (txFlowOption == TransactionFlowOption.Mandatory)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("SFxTransactionFlowRequired")));
            }
        }

        internal TransactionFormatter Formatter
        {
            get
            {
                return this.formatter;
            }
        }

        internal TransactionProtocol Protocol
        {
            get
            {
                return this.factory.TransactionProtocol;
            }
        }
    }
}

