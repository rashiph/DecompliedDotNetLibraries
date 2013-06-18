namespace System.ServiceModel.Transactions
{
    using System;
    using System.ServiceModel.Channels;
    using System.Transactions;

    internal abstract class TransactionFormatter
    {
        private static TransactionFormatter oleTxFormatter = new OleTxTransactionFormatter();
        private static object syncRoot = new object();
        private static TransactionFormatter wsatFormatter10;
        private static TransactionFormatter wsatFormatter11;

        protected TransactionFormatter()
        {
        }

        public abstract TransactionInfo ReadTransaction(Message message);
        public abstract void WriteTransaction(Transaction transaction, Message message);

        public abstract MessageHeader EmptyTransactionHeader { get; }

        public static TransactionFormatter OleTxFormatter
        {
            get
            {
                return oleTxFormatter;
            }
        }

        public static TransactionFormatter WsatFormatter10
        {
            get
            {
                if (wsatFormatter10 == null)
                {
                    lock (syncRoot)
                    {
                        if (wsatFormatter10 == null)
                        {
                            wsatFormatter10 = new WsatTransactionFormatter10();
                        }
                    }
                }
                return wsatFormatter10;
            }
        }

        public static TransactionFormatter WsatFormatter11
        {
            get
            {
                if (wsatFormatter11 == null)
                {
                    lock (syncRoot)
                    {
                        if (wsatFormatter11 == null)
                        {
                            wsatFormatter11 = new WsatTransactionFormatter11();
                        }
                    }
                }
                return wsatFormatter11;
            }
        }
    }
}

