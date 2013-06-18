namespace System.ServiceModel.Transactions
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.ServiceModel.Security;
    using System.Transactions;

    internal class WsatTransactionInfo : TransactionInfo
    {
        private CoordinationContext context;
        private RequestSecurityTokenResponse issuedToken;
        private WsatProxy wsatProxy;

        public WsatTransactionInfo(WsatProxy wsatProxy, CoordinationContext context, RequestSecurityTokenResponse issuedToken)
        {
            this.wsatProxy = wsatProxy;
            this.context = context;
            this.issuedToken = issuedToken;
        }

        public override Transaction UnmarshalTransaction()
        {
            Transaction transaction;
            if (!TransactionCache<string, Transaction>.Find(this.context.Identifier, out transaction))
            {
                transaction = this.wsatProxy.UnmarshalTransaction(this);
                new WsatExtendedInformation(this.context.Identifier, this.context.Expires).TryCache(transaction);
                WsatIncomingTransactionCache.Cache(this.context.Identifier, transaction);
            }
            return transaction;
        }

        public CoordinationContext Context
        {
            get
            {
                return this.context;
            }
        }

        public RequestSecurityTokenResponse IssuedToken
        {
            get
            {
                return this.issuedToken;
            }
        }
    }
}

