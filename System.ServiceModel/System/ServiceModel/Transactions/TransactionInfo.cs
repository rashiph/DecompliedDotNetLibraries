namespace System.ServiceModel.Transactions
{
    using System;
    using System.Transactions;

    internal abstract class TransactionInfo
    {
        protected TransactionInfo()
        {
        }

        public abstract Transaction UnmarshalTransaction();
    }
}

