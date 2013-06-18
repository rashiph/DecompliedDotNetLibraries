namespace System.ServiceModel.Transactions
{
    using System;
    using System.Transactions;

    internal class WsatExtendedInformationCache : TransactionCache<Transaction, WsatExtendedInformation>
    {
        public static void Cache(Transaction tx, WsatExtendedInformation info)
        {
            new WsatExtendedInformationCache().AddEntry(tx, tx, info);
        }
    }
}

