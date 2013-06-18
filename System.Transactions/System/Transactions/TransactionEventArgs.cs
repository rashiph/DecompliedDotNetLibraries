namespace System.Transactions
{
    using System;

    public class TransactionEventArgs : EventArgs
    {
        internal System.Transactions.Transaction transaction;

        public System.Transactions.Transaction Transaction
        {
            get
            {
                return this.transaction;
            }
        }
    }
}

