namespace System.Transactions
{
    using System;

    public interface ITransactionPromoter
    {
        byte[] Promote();
    }
}

