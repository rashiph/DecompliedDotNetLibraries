namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TransactionPromotionException : TransactionException
    {
        public TransactionPromotionException() : this(System.Transactions.SR.GetString("PromotionFailed"))
        {
        }

        public TransactionPromotionException(string message) : base(message)
        {
        }

        protected TransactionPromotionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransactionPromotionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

