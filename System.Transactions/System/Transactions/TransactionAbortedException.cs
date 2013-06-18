namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    [Serializable]
    public class TransactionAbortedException : TransactionException
    {
        public TransactionAbortedException() : base(System.Transactions.SR.GetString("TransactionAborted"))
        {
        }

        internal TransactionAbortedException(Exception innerException) : base(System.Transactions.SR.GetString("TransactionAborted"), innerException)
        {
        }

        public TransactionAbortedException(string message) : base(message)
        {
        }

        protected TransactionAbortedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransactionAbortedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal static TransactionAbortedException Create(string traceSource, Exception innerException)
        {
            return Create(traceSource, System.Transactions.SR.GetString("TransactionAborted"), innerException);
        }

        internal static TransactionAbortedException Create(string traceSource, string message, Exception innerException)
        {
            if (DiagnosticTrace.Error)
            {
                TransactionExceptionTraceRecord.Trace(traceSource, message);
            }
            return new TransactionAbortedException(message, innerException);
        }
    }
}

