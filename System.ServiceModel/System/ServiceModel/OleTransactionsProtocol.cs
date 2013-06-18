namespace System.ServiceModel
{
    using System;

    internal class OleTransactionsProtocol : TransactionProtocol
    {
        private static TransactionProtocol instance = new OleTransactionsProtocol();

        internal static TransactionProtocol Instance
        {
            get
            {
                return instance;
            }
        }

        internal override string Name
        {
            get
            {
                return "OleTransactions";
            }
        }
    }
}

