namespace System.Transactions
{
    using System;
    using System.Runtime.Serialization;
    using System.Transactions.Diagnostics;

    [Serializable]
    public class TransactionManagerCommunicationException : TransactionException
    {
        public TransactionManagerCommunicationException() : base(System.Transactions.SR.GetString("TransactionManagerCommunicationException"))
        {
        }

        public TransactionManagerCommunicationException(string message) : base(message)
        {
        }

        protected TransactionManagerCommunicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransactionManagerCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal static TransactionManagerCommunicationException Create(string traceSource, Exception innerException)
        {
            return Create(traceSource, System.Transactions.SR.GetString("TransactionManagerCommunicationException"), innerException);
        }

        internal static TransactionManagerCommunicationException Create(string traceSource, string message, Exception innerException)
        {
            if (DiagnosticTrace.Error)
            {
                TransactionExceptionTraceRecord.Trace(traceSource, message);
            }
            return new TransactionManagerCommunicationException(message, innerException);
        }
    }
}

