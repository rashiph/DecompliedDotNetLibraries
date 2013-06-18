namespace System.Transactions
{
    using System;

    [Serializable]
    public sealed class SubordinateTransaction : Transaction
    {
        public SubordinateTransaction(IsolationLevel isoLevel, ISimpleTransactionSuperior superior) : base(isoLevel, superior)
        {
        }
    }
}

