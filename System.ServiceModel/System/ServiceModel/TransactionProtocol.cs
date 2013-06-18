namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Configuration;

    [TypeConverter(typeof(TransactionProtocolConverter))]
    public abstract class TransactionProtocol
    {
        protected TransactionProtocol()
        {
        }

        internal static bool IsDefined(TransactionProtocol transactionProtocol)
        {
            if ((transactionProtocol != OleTransactions) && (transactionProtocol != WSAtomicTransactionOctober2004))
            {
                return (transactionProtocol == WSAtomicTransaction11);
            }
            return true;
        }

        public static TransactionProtocol Default
        {
            get
            {
                return OleTransactions;
            }
        }

        internal abstract string Name { get; }

        public static TransactionProtocol OleTransactions
        {
            get
            {
                return OleTransactionsProtocol.Instance;
            }
        }

        public static TransactionProtocol WSAtomicTransaction11
        {
            get
            {
                return WSAtomicTransaction11Protocol.Instance;
            }
        }

        public static TransactionProtocol WSAtomicTransactionOctober2004
        {
            get
            {
                return WSAtomicTransactionOctober2004Protocol.Instance;
            }
        }
    }
}

