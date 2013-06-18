namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Transactions;
    using System.Transactions;

    public sealed class TransactionMessageProperty
    {
        private System.Transactions.Transaction flowedTransaction;
        private TransactionInfo flowedTransactionInfo;
        private const string PropertyName = "TransactionMessageProperty";

        private TransactionMessageProperty()
        {
        }

        private static TransactionMessageProperty GetPropertyAndThrowIfAlreadySet(Message message)
        {
            if (message.Properties.ContainsKey("TransactionMessageProperty"))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FaultException(System.ServiceModel.SR.GetString("SFxTryAddMultipleTransactionsOnMessage")));
            }
            return new TransactionMessageProperty();
        }

        internal static void Set(TransactionInfo transactionInfo, Message message)
        {
            TransactionMessageProperty propertyAndThrowIfAlreadySet = GetPropertyAndThrowIfAlreadySet(message);
            propertyAndThrowIfAlreadySet.flowedTransactionInfo = transactionInfo;
            message.Properties.Add("TransactionMessageProperty", propertyAndThrowIfAlreadySet);
        }

        public static void Set(System.Transactions.Transaction transaction, Message message)
        {
            TransactionMessageProperty propertyAndThrowIfAlreadySet = GetPropertyAndThrowIfAlreadySet(message);
            propertyAndThrowIfAlreadySet.flowedTransaction = transaction;
            message.Properties.Add("TransactionMessageProperty", propertyAndThrowIfAlreadySet);
        }

        internal static TransactionMessageProperty TryGet(Message message)
        {
            if (message.Properties.ContainsKey("TransactionMessageProperty"))
            {
                return (message.Properties["TransactionMessageProperty"] as TransactionMessageProperty);
            }
            return null;
        }

        internal static System.Transactions.Transaction TryGetTransaction(Message message)
        {
            if (!message.Properties.ContainsKey("TransactionMessageProperty"))
            {
                return null;
            }
            return ((TransactionMessageProperty) message.Properties["TransactionMessageProperty"]).Transaction;
        }

        public System.Transactions.Transaction Transaction
        {
            get
            {
                if ((this.flowedTransaction == null) && (this.flowedTransactionInfo != null))
                {
                    try
                    {
                        this.flowedTransaction = this.flowedTransactionInfo.UnmarshalTransaction();
                    }
                    catch (TransactionException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                }
                return this.flowedTransaction;
            }
        }
    }
}

