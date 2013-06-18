namespace System.ServiceModel.Transactions
{
    using System;
    using System.Transactions;

    internal class WsatIncomingTransactionCache : TransactionCache<string, Transaction>
    {
        public static void Cache(string identifier, Transaction tx)
        {
            new WsatIncomingTransactionCache().AddEntry(tx, identifier, tx);
        }
    }
}

