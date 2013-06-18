namespace System.ServiceModel.Transactions
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Transactions;
    using System.Xml;

    internal abstract class WsatTransactionFormatter : TransactionFormatter
    {
        private bool initialized;
        private ProtocolVersion protocolVersion;
        private WsatConfiguration wsatConfig;
        private WsatProxy wsatProxy;

        protected WsatTransactionFormatter(ProtocolVersion protocolVersion)
        {
            this.protocolVersion = protocolVersion;
        }

        public WsatTransactionInfo CreateTransactionInfo(CoordinationContext context, RequestSecurityTokenResponse issuedToken)
        {
            return new WsatTransactionInfo(this.wsatProxy, context, issuedToken);
        }

        private void EnsureInitialized()
        {
            if (!this.initialized)
            {
                lock (this)
                {
                    if (!this.initialized)
                    {
                        this.wsatConfig = new WsatConfiguration();
                        this.wsatProxy = new WsatProxy(this.wsatConfig, this.protocolVersion);
                        this.initialized = true;
                    }
                }
            }
        }

        private void ForcePromotion(Transaction transaction)
        {
            TransactionInterop.GetTransmitterPropagationToken(transaction);
        }

        public void MarshalAsCoordinationContext(Transaction transaction, out CoordinationContext context, out RequestSecurityTokenResponse issuedToken)
        {
            uint num;
            IsolationFlags flags;
            string str2;
            WsatExtendedInformation information;
            string str3;
            Guid distributedIdentifier = transaction.TransactionInformation.DistributedIdentifier;
            string contextId = null;
            context = new CoordinationContext(this.protocolVersion);
            OleTxTransactionFormatter.GetTransactionAttributes(transaction, out num, out flags, out str2);
            context.IsolationFlags = flags;
            context.Description = str2;
            if (TransactionCache<Transaction, WsatExtendedInformation>.Find(transaction, out information))
            {
                context.Expires = information.Timeout;
                if (!string.IsNullOrEmpty(information.Identifier))
                {
                    context.Identifier = information.Identifier;
                    contextId = information.Identifier;
                }
            }
            else
            {
                context.Expires = num;
                if (context.Expires == 0)
                {
                    context.Expires = (uint) TimeoutHelper.ToMilliseconds(this.wsatConfig.MaxTimeout);
                }
            }
            if (context.Identifier == null)
            {
                context.Identifier = CoordinationContext.CreateNativeIdentifier(distributedIdentifier);
                contextId = null;
            }
            if (!this.wsatConfig.IssuedTokensEnabled)
            {
                str3 = null;
                issuedToken = null;
            }
            else
            {
                CoordinationServiceSecurity.CreateIssuedToken(distributedIdentifier, context.Identifier, this.protocolVersion, out issuedToken, out str3);
            }
            AddressHeader refParam = new WsatRegistrationHeader(distributedIdentifier, contextId, str3);
            context.RegistrationService = this.wsatConfig.CreateRegistrationService(refParam, this.protocolVersion);
            context.IsolationLevel = transaction.IsolationLevel;
            context.LocalTransactionId = distributedIdentifier;
            if (this.wsatConfig.OleTxUpgradeEnabled)
            {
                context.PropagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
            }
        }

        public override TransactionInfo ReadTransaction(Message message)
        {
            RequestSecurityTokenResponse response;
            this.EnsureInitialized();
            CoordinationContext coordinationContext = WsatTransactionHeader.GetCoordinationContext(message, this.protocolVersion);
            if (coordinationContext == null)
            {
                return null;
            }
            try
            {
                response = CoordinationServiceSecurity.GetIssuedToken(message, coordinationContext.Identifier, this.protocolVersion);
            }
            catch (XmlException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException("FailedToDeserializeIssuedToken", exception));
            }
            return new WsatTransactionInfo(this.wsatProxy, coordinationContext, response);
        }

        public override void WriteTransaction(Transaction transaction, Message message)
        {
            CoordinationContext context;
            RequestSecurityTokenResponse response;
            this.EnsureInitialized();
            this.ForcePromotion(transaction);
            this.MarshalAsCoordinationContext(transaction, out context, out response);
            if (response != null)
            {
                CoordinationServiceSecurity.AddIssuedToken(message, response);
            }
            WsatTransactionHeader header = new WsatTransactionHeader(context, this.protocolVersion);
            message.Headers.Add(header);
        }
    }
}

