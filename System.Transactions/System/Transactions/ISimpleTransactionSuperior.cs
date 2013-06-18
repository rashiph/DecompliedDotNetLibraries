namespace System.Transactions
{
    using System;

    public interface ISimpleTransactionSuperior : ITransactionPromoter
    {
        void Rollback();
    }
}

