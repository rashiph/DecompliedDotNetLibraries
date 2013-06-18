namespace System.Transactions
{
    using System;
    using System.Transactions.Diagnostics;

    public class TransactionInformation
    {
        private InternalTransaction internalTransaction;

        internal TransactionInformation(InternalTransaction internalTransaction)
        {
            this.internalTransaction = internalTransaction;
        }

        public DateTime CreationTime
        {
            get
            {
                return new DateTime(this.internalTransaction.CreationTime);
            }
        }

        public Guid DistributedIdentifier
        {
            get
            {
                Guid guid;
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "TransactionInformation.get_DistributedIdentifier");
                }
                try
                {
                    lock (this.internalTransaction)
                    {
                        return this.internalTransaction.State.get_Identifier(this.internalTransaction);
                    }
                }
                finally
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "TransactionInformation.get_DistributedIdentifier");
                    }
                }
                return guid;
            }
        }

        public string LocalIdentifier
        {
            get
            {
                string transactionIdentifier;
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "TransactionInformation.get_LocalIdentifier");
                }
                try
                {
                    transactionIdentifier = this.internalTransaction.TransactionTraceId.TransactionIdentifier;
                }
                finally
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "TransactionInformation.get_LocalIdentifier");
                    }
                }
                return transactionIdentifier;
            }
        }

        public TransactionStatus Status
        {
            get
            {
                TransactionStatus status;
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "TransactionInformation.get_Status");
                }
                try
                {
                    status = this.internalTransaction.State.get_Status(this.internalTransaction);
                }
                finally
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceLtm"), "TransactionInformation.get_Status");
                    }
                }
                return status;
            }
        }
    }
}

