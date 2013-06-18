namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    [Serializable]
    public class TransactionException : SystemException
    {
        public TransactionException()
        {
        }

        public TransactionException(string message) : base(message)
        {
        }

        protected TransactionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal static TransactionException Create(string traceSource, string message, Exception innerException)
        {
            if (DiagnosticTrace.Error)
            {
                TransactionExceptionTraceRecord.Trace(traceSource, message);
            }
            return new TransactionException(message, innerException);
        }

        internal static Exception CreateEnlistmentStateException(string traceSource, Exception innerException)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(traceSource, System.Transactions.SR.GetString("EnlistmentStateException"));
            }
            return new InvalidOperationException(System.Transactions.SR.GetString("EnlistmentStateException"), innerException);
        }

        internal static Exception CreateInvalidOperationException(string traceSource, string message, Exception innerException)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(traceSource, message);
            }
            return new InvalidOperationException(message, innerException);
        }

        internal static Exception CreateTransactionCompletedException(string traceSource)
        {
            if (DiagnosticTrace.Error)
            {
                InvalidOperationExceptionTraceRecord.Trace(traceSource, System.Transactions.SR.GetString("TransactionAlreadyCompleted"));
            }
            return new InvalidOperationException(System.Transactions.SR.GetString("TransactionAlreadyCompleted"));
        }

        internal static TransactionException CreateTransactionStateException(string traceSource, Exception innerException)
        {
            return Create(traceSource, System.Transactions.SR.GetString("TransactionStateException"), innerException);
        }
    }
}

