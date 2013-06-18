namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    [Serializable]
    public class TransactionInDoubtException : TransactionException
    {
        public TransactionInDoubtException() : base(System.Transactions.SR.GetString("TransactionIndoubt"))
        {
        }

        public TransactionInDoubtException(string message) : base(message)
        {
        }

        protected TransactionInDoubtException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransactionInDoubtException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal static TransactionInDoubtException Create(string traceSource, Exception innerException)
        {
            return Create(traceSource, System.Transactions.SR.GetString("TransactionIndoubt"), innerException);
        }

        internal static TransactionInDoubtException Create(string traceSource, string message, Exception innerException)
        {
            if (DiagnosticTrace.Error)
            {
                TransactionExceptionTraceRecord.Trace(traceSource, message);
            }
            return new TransactionInDoubtException(message, innerException);
        }
    }
}

