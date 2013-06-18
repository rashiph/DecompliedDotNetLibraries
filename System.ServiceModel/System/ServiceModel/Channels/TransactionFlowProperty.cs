namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Transactions;

    internal class TransactionFlowProperty
    {
        private System.Transactions.Transaction flowedTransaction;
        private List<RequestSecurityTokenResponse> issuedTokens;
        private const string PropertyName = "TransactionFlowProperty";

        private TransactionFlowProperty()
        {
        }

        internal static TransactionFlowProperty Ensure(Message message)
        {
            if (message.Properties.ContainsKey("TransactionFlowProperty"))
            {
                return (TransactionFlowProperty) message.Properties["TransactionFlowProperty"];
            }
            TransactionFlowProperty property = new TransactionFlowProperty();
            message.Properties.Add("TransactionFlowProperty", property);
            return property;
        }

        private static TransactionFlowProperty GetPropertyAndThrowIfAlreadySet(Message message)
        {
            TransactionFlowProperty property = TryGet(message);
            if (property != null)
            {
                if (property.flowedTransaction != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FaultException(System.ServiceModel.SR.GetString("SFxTryAddMultipleTransactionsOnMessage")));
                }
                return property;
            }
            return new TransactionFlowProperty();
        }

        internal static void Set(System.Transactions.Transaction transaction, Message message)
        {
            TransactionFlowProperty propertyAndThrowIfAlreadySet = GetPropertyAndThrowIfAlreadySet(message);
            propertyAndThrowIfAlreadySet.flowedTransaction = transaction;
            message.Properties.Add("TransactionFlowProperty", propertyAndThrowIfAlreadySet);
        }

        internal static TransactionFlowProperty TryGet(Message message)
        {
            if (message.Properties.ContainsKey("TransactionFlowProperty"))
            {
                return (message.Properties["TransactionFlowProperty"] as TransactionFlowProperty);
            }
            return null;
        }

        internal static ICollection<RequestSecurityTokenResponse> TryGetIssuedTokens(Message message)
        {
            TransactionFlowProperty property = TryGet(message);
            if ((property != null) && ((property.issuedTokens != null) && (property.issuedTokens.Count != 0)))
            {
                return property.issuedTokens;
            }
            return null;
        }

        internal static System.Transactions.Transaction TryGetTransaction(Message message)
        {
            if (!message.Properties.ContainsKey("TransactionFlowProperty"))
            {
                return null;
            }
            return ((TransactionFlowProperty) message.Properties["TransactionFlowProperty"]).Transaction;
        }

        internal ICollection<RequestSecurityTokenResponse> IssuedTokens
        {
            get
            {
                if (this.issuedTokens == null)
                {
                    this.issuedTokens = new List<RequestSecurityTokenResponse>();
                }
                return this.issuedTokens;
            }
        }

        internal System.Transactions.Transaction Transaction
        {
            get
            {
                return this.flowedTransaction;
            }
        }
    }
}

