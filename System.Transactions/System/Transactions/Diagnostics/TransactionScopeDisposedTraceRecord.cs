namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class TransactionScopeDisposedTraceRecord : TraceRecord
    {
        private static TransactionScopeDisposedTraceRecord record = new TransactionScopeDisposedTraceRecord();
        private string traceSource;
        private TransactionTraceIdentifier txTraceId;

        internal static void Trace(string traceSource, TransactionTraceIdentifier txTraceId)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.txTraceId = txTraceId;
                DiagnosticTrace.TraceEvent(TraceEventType.Information, "http://msdn.microsoft.com/2004/06/System/Transactions/TransactionScopeDisposed", System.Transactions.SR.GetString("TraceTransactionScopeDisposed"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            TraceHelper.WriteTxId(xml, this.txTraceId);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/TransactionScopeDisposedTraceRecord";
            }
        }
    }
}

