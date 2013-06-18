namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Transactions;

    internal class WrappedTransaction
    {
        private System.Transactions.Transaction transaction;

        internal WrappedTransaction(System.Transactions.Transaction transaction)
        {
            this.transaction = transaction;
        }

        internal System.Transactions.Transaction Transaction
        {
            get
            {
                return this.transaction;
            }
        }
    }
}

