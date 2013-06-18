namespace System.ServiceModel.Transactions
{
    using System;
    using System.Transactions;

    internal class TransactionManagerConfigurationException : TransactionException
    {
        public TransactionManagerConfigurationException(string error) : base(error)
        {
        }

        public TransactionManagerConfigurationException(string error, Exception e) : base(error, e)
        {
        }
    }
}

